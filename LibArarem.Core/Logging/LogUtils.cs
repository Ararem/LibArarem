using LibArarem.Core.Logging.Enrichers;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibArarem.Core.Logging;

/// <summary>Class containing helper methods for logging</summary>
public static class LogUtils
{
	/// <summary>
	/// Returns a <see cref="ILogger"/> that contains a "Context" property for which object instance is making the log call
	/// </summary>
	public static ILogger WithInstanceContext(object obj, bool destructure = true)
	{
		return Log.ForContext(new InstanceContextEnricher(obj, destructure));
	}

	/// <summary>
	/// Pushes a property onto the <see cref="LogContext"/> that marks a section of code as extremely verbose (like even more verbose that <see cref="LogEventLevel.Verbose"/>) - e.g. frame update messages or other logs that get called regularly and aren't important normally
	/// </summary>
	/// <seealso cref="LogContext"/>
	/// <seealso cref="LogContext.PushProperty"/>
	public static IDisposable MarkContextAsExtremelyVerbose()
	{
		return LogContext.PushProperty(nameof(MarkContextAsExtremelyVerbose), true);
	}

	/// <summary>Logs a message that an event was called, allowing for callback tracing</summary>
	/// <example>
	///  <code>
	///  void ButtonClickedCallback(object? sender, EventArgs eventArgs){
	/// 		TrackEvent(sender, eventArgs); //Outputs "ButtonClickedCallback() from MyButton: {ButtonState: DoubleClick}"
	/// 		//...
	///   }
	///   </code>
	/// </example>
	[StackTraceHidden]
	public static void TrackEvent(this ILogger logger, object? sender, EventArgs eventArgs, [CallerMemberName] string? caller = null)
	{
		TrackEvent(logger, LogEventLevel.Debug, sender, eventArgs, caller);
	}

	///<inheritdoc cref="TrackEvent(Serilog.ILogger,object?,System.EventArgs,string?)"/>
	[StackTraceHidden]
	public static void TrackEvent(this ILogger logger, LogEventLevel level, object? sender, EventArgs eventArgs, [CallerMemberName] string? caller = null)
	{
		logger.Write(level,"{CallbackName} from {@Sender}: {@EventArgs}", caller+"()", sender, eventArgs);
	}

	/// <summary>Logs an expression and it's value to the logger, with an optional message</summary>
	/// <param name="logger">Logger to log expression to</param>
	/// <param name="value">The value to log</param>
	/// <param name="message">Optional message to be logged after the expression</param>
	/// <param name="expressionString">(Please don't set this) string containing the compile time expression passed to <paramref name="value"/></param>
	/// <typeparam name="T">Type of the expression</typeparam>
	[StackTraceHidden]
	public static void LogExpression<T>(this ILogger logger, T value, string? message = null, [CallerArgumentExpression("value")] string expressionString = "Unknown Variable")
	{
		if (message is null)
			logger.Verbose("{Expression}: {Value}", expressionString, value);
		else
			logger.Verbose("{Expression}: {Value}, {Message}", expressionString, value, message);
	}

	/// <summary>Logs an expression and it's value to the logger, with an optional message</summary>
	/// <param name="logger">Logger to log expression to</param>
	/// <param name="value">The value to log</param>
	/// <param name="message">Optional message to be logged after the expression</param>
	/// <param name="expressionString">(Please don't set this) string containing the compile time expression passed to <paramref name="value"/></param>
	/// <typeparam name="T">Type of the expression</typeparam>
	[StackTraceHidden]
	public static void LogExpressionDestructured<T>(this ILogger logger, T value, string? message = null, [CallerArgumentExpression("value")] string expressionString = "Unknown Variable")
	{
		if (message is null)
			logger.Verbose("{Expression}: {@Value}", expressionString, value);
		else
			logger.Verbose("{Expression}: {@Value}, {Message}", expressionString, value, message);
	}

	/// <summary>Logs an expression and it's value to the logger, with an optional message</summary>
	/// <param name="logger">Logger to log expression to</param>
	/// <param name="level"><see cref="LogEventLevel"/> at which to log the event</param>
	/// <param name="value">The value to log</param>
	/// <param name="message">Optional message to be logged after the expression</param>
	/// <param name="expressionString">(Please don't set this) string containing the compile time expression passed to <paramref name="value"/></param>
	/// <typeparam name="T">Type of the expression</typeparam>
	[StackTraceHidden]
	public static void LogExpression<T>(this ILogger logger, LogEventLevel level, T value, string? message = null, [CallerArgumentExpression("value")] string expressionString = "Unknown Variable")
	{
		if (message is null)
			logger.Write(level, "{Expression}: {Value}", expressionString, value);
		else
			logger.Write(level, "{Expression}: {Value}, {Message}", expressionString, value, message);
	}
}