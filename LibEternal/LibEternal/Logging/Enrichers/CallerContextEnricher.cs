using JetBrains.Annotations;
using LibEternal.ObjectPools;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace LibEternal.Logging.Enrichers
{
	/// <summary>
	///  An <see cref="ILogEventEnricher"/> that applies context-related enrichment. Currently implemented context:
	///  <list type="table">
	///   <listheader>
	///    <description>Description</description>
	///    <term>Term</term>
	///   </listheader>
	///   <item>
	///    <term>
	///     <see cref="CallingTypeProp"/>
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
	///     <see cref="CallingMethodProp"/>
	///    </term>
	///    <description xml:space="preserve">The method calling the log function</description>
	///   </item>
	///  </list>
	///  These values can be overwritten by using <see cref="Log.ForContext(string, object, bool)"/>
	/// </summary>
	[UsedImplicitly]
	public sealed class CallerContextEnricher : ILogEventEnricher
	{
		public const string CallingTypeProp = "CallingType";
		public const string StackTraceProp = "StackTrace";
		public const string CallingMethodProp = "CallingMethod";

		/// <inheritdoc/>
		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			EnhancedStackTrace? trace = GetStackTrace();
			string callingTypeStr, callingMethodStr, stackTraceStr;
			if (trace is null)
			{
				callingTypeStr = callingMethodStr = stackTraceStr = "<StackTrace Error>";
			}
			else
			{
				EnhancedStackFrame callerFrame = (EnhancedStackFrame)trace.GetFrame(0);
				ResolvedMethod callerMethod = callerFrame.MethodInfo;
				Type? callerType = callerMethod.DeclaringType;

				StringBuilder sb = StringBuilderPool.GetPooled();
				callerMethod.Append(sb, false);
				callingMethodStr = sb.ToString();

				/* If the type is null it belongs to a module not a class (I guess a 'global' function?)
				* From https://stackoverflow.com/a/35266094
				* If the MemberInfo object is a global member
				* (that is, if it was obtained from the Module.GetMethods method, which returns global methods on a module),
				* the returned DeclaringType will be null.
				*/
				callingTypeStr = callerType is null ? "<Module>" : sb.Clear().AppendTypeDisplayName(callerType, false, true).ToString();

				stackTraceStr = trace.ToString();
			}

			logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(CallingTypeProp, callingTypeStr));
			logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(CallingMethodProp, callingMethodStr));
			logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(StackTraceProp, stackTraceStr));
		}

		/// <summary>
		///  Returns an enhanced stack trace, skipping serilog methods
		/// </summary>
		internal static EnhancedStackTrace? GetStackTrace()
		{
			/*
			 * Example output:
			 * This >>>>		at void Core.Logging.StackTraceEnricher.GetStackTrace() in C:/Users/XXXXX/Documents/Projects/Unity/Team-Defense/Assets/Scripts/Core/Logging/StackTraceEnricher.cs:line 149
			 * 					at void Serilog.Enrichers.FunctionEnricher.Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
			 * 					at void Serilog.Core.Enrichers.SafeAggregateEnricher.Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
			 * 					at void Serilog.Core.Logger.Dispatch(LogEvent logEvent)
			 * 					at void Serilog.Core.Logger.Write(LogEventLevel level, Exception exception, string messageTemplate, params object[] propertyValues)
			 * 					at void Serilog.Core.Logger.Write(LogEventLevel level, string messageTemplate, params object[] propertyValues)
			 * 					at void Serilog.Core.Logger.Write(LogEventLevel level, string messageTemplate)
			 * 					at void Serilog.Log.Write(LogEventLevel level, string messageTemplate)
			 * 					at void Serilog.Log.Information(string messageTemplate)
			 * Logged >>>		at void Testing.TestBehaviour.Test() in C:/Users/XXXXX/Documents/Projects/Unity/Team-Defense/Assets/Scripts/Testing/TestBehaviour.cs:line 43
			 *
			 * Here we can see we need to skip 9 frames to avoid including the serilog stuff.
			 */

			//Find how far we need to go to skip all the serilog methods
			var frames = new StackTrace().GetFrames();

			bool gotToSerilogYet = false;
			int skip = 0;
			//Yes this is very complicated but I don't know how to make it easier to read
			//I essentially just used a truth table and then imported it as if/else statements
			//Of course, I forgot to save the table but hey, it works!
			//Also ignore the commented duplicate if/else statements, they're just how it was before resharper did it's "truth analysis" and got rid of the impossible branches
			for (int i = 0; i < frames.Length; i++)
			{
				MethodBase? method = frames[i].GetMethod();
				//If the method, type or name is null, the expression is false
				bool isSerilog = method?.DeclaringType?.FullName?.ToLower().Contains("serilog") ?? false;
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
				return new EnhancedStackTrace(new StackTrace(skip));
			}
			catch (Exception e)
			{
				SelfLog.WriteLine("Error creating EnhancedStackTrace: {0}", e);
				return null;
			}
		}
	}
}