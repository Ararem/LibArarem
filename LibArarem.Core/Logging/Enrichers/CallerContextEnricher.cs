using JetBrains.Annotations;
using LibArarem.Core.ObjectPools;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Reflection;

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

	/// <summary>The name of the property for the stack trace</summary>
	public const string StackTraceProp = "StackTrace";

	/// <summary>The name of the property for the calling method</summary>
	public const string CallingMethodNameProp = "CallingMethodName";

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
		// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
		switch (perfMode)
		{
			case PerfMode.SingleFrameFast:
				EnrichFastDemystify(logEvent, propertyFactory);
				break;
			case PerfMode.FullTraceSlow:
				EnrichSlowFullTrace(logEvent, propertyFactory);
				break;
		}
	}

	private static void EnrichFastDemystify(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		ResolvedMethod callerMethod = null!;
		Type?          callerType   = null;
		{
			StackFrame[] frames          = new StackTrace().GetFrames();
			bool         gotToSerilogYet = false;
			for (int i = 0; i < frames.Length; i++)
			{
				MethodBase? tempCallerMethod = frames[i].GetMethod();

				if (tempCallerMethod is null) continue;
				callerMethod = EnhancedStackTrace.GetMethodDisplayString(tempCallerMethod);
				callerType   = callerMethod.DeclaringType;
				if (callerType is null) continue;

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
				break;
			}
		}

		//Now do the actual enriching, with nicer names
		string callingMethodStr = callerMethod.Name!;
		string callingTypeStr   = callerType is null ? "<Module>" : StringBuilderPool.BorrowInline(static (sb, callerType) => sb.AppendTypeDisplayName(callerType, false), callerType);

		logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(CallingTypeNameProp,   callingTypeStr)!);
		logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(CallingMethodNameProp, callingMethodStr)!);
		logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(StackTraceProp,        "<<<ERROR: STACKTRACE DISABLED FOR PERFORMANCE>>>")!);
	}

#region Slow Enriching

	private static void EnrichSlowFullTrace(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		ArgumentNullException.ThrowIfNull(logEvent);
		ArgumentNullException.ThrowIfNull(propertyFactory);

		EnhancedStackTrace? trace = GetStackTrace();
		string              callingTypeStr, callingMethodStr, stackTraceStr;
		if (trace is null)
		{
			callingTypeStr = callingMethodStr = stackTraceStr = "<StackTrace Error>";
		}
		else
		{
			EnhancedStackFrame callerFrame  = (EnhancedStackFrame)trace.GetFrame(0);
			ResolvedMethod     callerMethod = callerFrame.MethodInfo;
			Type?              callerType   = callerMethod.DeclaringType;

			callingMethodStr = callerMethod.Name!;

			/* If the type is null it belongs to a module not a class (I guess a 'global' function?)
			* From https://stackoverflow.com/a/35266094
			* If the MemberInfo object is a global member
			* (that is, if it was obtained from the Module.GetMethods method, which returns global methods on a module),
			* the returned DeclaringType will be null.
			*/
			callingTypeStr = callerType is null ? "<Module>" : StringBuilderPool.BorrowInline(static (sb, callerType) => sb.AppendTypeDisplayName(callerType, false), callerType);

			stackTraceStr = trace.ToString();
		}

		logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(CallingTypeNameProp,   callingTypeStr)!);
		logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(CallingMethodNameProp, callingMethodStr)!);
		logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(StackTraceProp,        stackTraceStr)!);
	}

	/// <summary>Returns an enhanced stack trace, skipping serilog methods</summary>
	private static EnhancedStackTrace? GetStackTrace()
	{
		/*
	* Example output:
	* This >>>>		at void LibArarem.Core.Logging.Enrichers.CallerContextEnricher.Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	* 					at void Serilog.Core.Enrichers.SafeAggregateEnricher.Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	* 					at void Serilog.Core.Logger.Dispatch(LogEvent logEvent)
	* 					at void Serilog.Core.Logger.Write(LogEventLevel level, Exception exception, string messageTemplate, params object[] propertyValues)
	* 					at void Serilog.Core.Logger.Write(LogEventLevel level, string messageTemplate, params object[] propertyValues)
	* 					at void Serilog.Core.Logger.Write(LogEventLevel level, string messageTemplate)
	* 					at void Serilog.Log.Write(LogEventLevel level, string messageTemplate)
	* 					at void Serilog.Log.Information(string messageTemplate)
	* Logged >>>		at void Testing.TestBehaviour.Test()
	*
	* Here we can see we need to skip 9 frames to avoid including the serilog stuff.
	*/

		//Find how far we need to go to skip all the serilog methods
		StackFrame[] frames = new StackTrace().GetFrames();

		bool gotToSerilogYet = false;
		int  skip            = 0;
		//Yes this is very complicated but I don't know how to make it easier to read
		//I essentially just used a truth table and then imported it as if/else statements
		//Of course, I forgot to save the table but hey, it works!
		//Also ignore the commented duplicate if/else statements, they're just how it was before resharper did it's "truth analysis" and got rid of the impossible branches
		for (int i = 0; i < frames.Length; i++)
		{
			MethodBase? method = frames[i].GetMethod();
			//If the method, type or name is null, the expression is false
			bool isSerilog = method?.DeclaringType?.Namespace?.StartsWith("Serilog") ?? false;
			//E.g. "at void Core.Logger.ReinitialiseLogger()+GetStackTrace()"
			// ReSharper disable once ConvertIfStatementToSwitchStatement
			if (!gotToSerilogYet && !isSerilog)
				continue;
			//E.g. "at void Serilog.Enrichers.FunctionEnricher.Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)"
			// if (!gotToSerilogYet && isSerilog)
			if (!gotToSerilogYet)
			{
				gotToSerilogYet = true;
				continue;
			}

			//E.g. "at void Serilog.Core.Logger.Dispatch(LogEvent logEvent)"
			// if (gotToSerilogYet && isSerilog) continue;
			if (isSerilog) continue;

			//E.g. "at void Testing.TestBehaviour.Test()"
			// if (gotToSerilogYet && !isSerilog)
			//Finally found the right number
			skip = i;
			break;
		}

		try
		{
			//Tis a shame that one cannot pass in the frames on their own, only create a new trace
			return new EnhancedStackTrace(new StackTrace(skip));
		}
		catch (Exception e)
		{
			SelfLog.WriteLine("Error creating EnhancedStackTrace: {0}", e);
			return null;
		}
	}

#endregion
}