using JetBrains.Annotations;
using LibArarem.Core.ObjectPools;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LibArarem.Core.Logging.Enrichers;

/// <summary>
///  An <see cref="ILogEventEnricher"/> that applies context-related enrichment. Currently implemented context:
///  <list type="table">
///   <listheader>
///    <description>Description</description> <term>Term</term>
///   </listheader>
///   <item>
///    <term>
///     <see cref="CallingTypeNameProp"/>
///    </term>
///    <description xml:space="preserve">The type that the method calling the log function was declared in</description>
///   </item>
///   <item>
///    <term>
///     <see cref="StackTraceProp"/>
///    </term>
///    <description xml:space="preserve">The <see cref="System.Diagnostics.StackTrace"/> of the method where a <see cref="Log"/> call was made. Does not include <see
///     cref="Serilog"/> methods</description>
///   </item>
///   <item>
///    <term>
///     <see cref="CallingMethodNameProp"/>
///    </term>
///    <description xml:space="preserve">The method calling the log function</description>
///   </item>
///  </list>
///  These values can be overwritten by using <see cref="Log.ForContext(string, object, bool)"/>
/// </summary>
[UsedImplicitly]
public sealed class CallerContextEnricher : ILogEventEnricher
{
	/// <summary>An enum specifying what performance mode to use (use to control if you want lots of information, or performance)</summary>
	public enum PerfMode
	{
		/// <summary>Enriches with a proper <see cref="StackTrace"/>, but is VERY slow</summary>
		FullTraceSlow,

		/// <summary>
		///  Enriches much faster, without including a full <see cref="StackTrace"/>. Almost 20x faster than <see cref="FullTraceSlow"/> and allocates ~9% of the
		///  memory. Methods and classes are demystified properly
		/// </summary>
		SingleFrameFast
	}

	/// <summary>The name of the property for the calling type</summary>
	public const string CallingTypeNameProp = "CallingTypeName";

	/// <summary>The name of the property for the line at which the log function was called</summary>
	public const string CallingMethodLineProp = "CallingMethodLine";

	/// <summary>The name of the property for the column at which the log function was called</summary>
	public const string CallingMethodColumnProp = "CallingMethodColumn";

	/// <summary>The name of the property for the file in which the log function was called</summary>
	public const string CallingMethodFileProp = "CallingMethodFile";

	/// <summary>The name of the property for the stack trace</summary>
	public const string StackTraceProp = "StackTrace";

	/// <summary>The name of the property for the calling method</summary>
	public const string CallingMethodNameProp = "CallingMethodName";

	private static readonly ConcurrentDictionary<MethodBase, bool> StackTraceHiddenMethods = new();
	private static readonly ConcurrentDictionary<Type, bool>       StackTraceHiddenTypes   = new();

	private readonly PerfMode perfMode;

	/// <summary>Constructs a new instance of this type</summary>
	/// <param name="perfMode">
	///  An enum argument that when set to <see cref="PerfMode.SingleFrameFast"/> greatly increasing performance by disabling stack trace demystifying. This
	///  should only be used if the stack trace is not going to be displayed, only the other properties.
	/// </param>
	public CallerContextEnricher(PerfMode perfMode)
	{
		this.perfMode = perfMode;
	}


	/// <inheritdoc/>
	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		ArgumentNullException.ThrowIfNull(logEvent);
		ArgumentNullException.ThrowIfNull(propertyFactory);

		int offset = FindOffsetToCaller();

		ResolvedMethod callerMethod;
		string         fileName;
		int            lineNumber;
		int            columnNumber;
		Type?          callerType;
		string         callingMethodStr;
		string         callingTypeStr;

		if (perfMode == PerfMode.FullTraceSlow)
		{
			EnhancedStackTrace trace       = new(new StackTrace(offset));
			EnhancedStackFrame callerFrame = (EnhancedStackFrame)trace.GetFrame(0);
			logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(StackTraceProp, trace));

			callerMethod = callerFrame.MethodInfo;
			fileName     = callerFrame.GetFileName() ?? "<Unknown File>";
			lineNumber   = callerFrame.GetFileLineNumber();
			columnNumber = callerFrame.GetFileColumnNumber();
			callerType   = callerMethod.DeclaringType;
			callingMethodStr = StringBuilderPool.BorrowInline(
					static (sb, callerMethod) =>
					{
						sb.Append(callerMethod.Name);
						if (callerMethod.SubMethod is {} subMethod)
						{
							sb.Append('+').Append(subMethod);
						}
					}, callerMethod
			);
			/* If the type is null it belongs to a module not a class (I guess a 'global' function?)
			* From https://stackoverflow.com/a/35266094
			* If the MemberInfo object is a global member
			* (that is, if it was obtained from the Module.GetMethods method, which returns global methods on a module),
			* the returned DeclaringType will be null.
			*/
			callingTypeStr = callerType is null ? "<Module>" : StringBuilderPool.BorrowInline(static (sb, callerType) => sb.AppendTypeDisplayName(callerType, false), callerType);
		}
		else
		{
			StackFrame frame = new(offset, true);
			callerMethod = EnhancedStackTrace.GetMethodDisplayString(frame.GetMethod()!); //Method shouldn't be null because FindOffset() skips null methods/types
			fileName     = frame.GetFileName() ?? "<Unknown File>";
			lineNumber   = frame.GetFileLineNumber();
			columnNumber = frame.GetFileColumnNumber();
			callerType   = callerMethod.DeclaringType;
			callingMethodStr = StringBuilderPool.BorrowInline(
					static (sb, callerMethod) =>
					{
						sb.Append(callerMethod.Name);
						if (callerMethod.SubMethod is {} subMethod)
						{
							sb.Append('+').Append(subMethod);
						}
					}, callerMethod
			);
			/* If the type is null it belongs to a module not a class (I guess a 'global' function?)
			* From https://stackoverflow.com/a/35266094
			* If the MemberInfo object is a global member
			* (that is, if it was obtained from the Module.GetMethods method, which returns global methods on a module),
			* the returned DeclaringType will be null.
			*/
			callingTypeStr = callerType is null ? "<Module>" : StringBuilderPool.BorrowInline(static (sb, callerType) => sb.AppendTypeDisplayName(callerType, false), callerType);
		}

		logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(CallingTypeNameProp,     callingTypeStr)!);
		logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(CallingMethodNameProp,   callingMethodStr)!);
		logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(CallingMethodFileProp,   fileName)!);
		logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(CallingMethodLineProp,   lineNumber)!);
		logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(CallingMethodColumnProp, columnNumber)!);
	}

	/// <summary>Finds the number of stack frames that need to be skipped to get to the caller</summary>
	[MethodImpl(MethodImplOptions.NoInlining)] //Can't inline since we're working with StackTraces here
	private static int FindOffsetToCaller()
	{
		StackFrame[] frames          = new StackTrace(false).GetFrames();
		bool         gotToSerilogYet = false;
		for (int i = 0; i < frames.Length; i++)
		{
			StackFrame  frame        = frames[i];
			MethodBase? callerMethod = frame.GetMethod();

			if (callerMethod is null) continue; //Invalid (nonexistent) method
			Type? callerType = callerMethod.DeclaringType;
			if (callerType is null) continue; //Invalid (nonexistent) type

			//Skip if method or type is hidden in stacktraces
			bool hidden = StackTraceHiddenMethods.GetOrAdd(callerMethod, static method => method.GetCustomAttribute<StackTraceHiddenAttribute>() is not null) ||
						  StackTraceHiddenTypes.GetOrAdd(callerType, static type => type.GetCustomAttribute<StackTraceHiddenAttribute>() is not null);
			if (hidden) continue;

			bool isSerilog = callerType.Namespace?.StartsWith("Serilog") ?? false;
			switch (gotToSerilogYet)
			{
				case false when !isSerilog:
					continue;
				case false:
					gotToSerilogYet = true;
					continue;
			}

			if (isSerilog) continue;

			//Finally found the right number
			//https://youtu.be/Vy7RaQUmOzE?t=203
			return i -1; //Subtract one to account for the fact that we're calculating the offset from this method
		}

		return 0;
		// throw new InvalidOperationException("Should not have reached here");
	}
}