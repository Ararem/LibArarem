using Serilog;
using Serilog.Context;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LibArarem.Core.Logging;

/// <summary>Class containing helper methods for logging</summary>
public static class LogUtils
{
	/// <summary>
	/// Returns a <see cref="ILogger"/> that contains a "Context" property for which object instance is making the log call
	/// </summary>
	public static ILogger WithInstanceContext(object obj)
	{
		return Log.ForContext("@Context", obj);
	}

	/// <summary>
	/// Pushes a property onto the <see cref="LogContext"/> that marks a section of code as extremely verbose (like even more verbose that <see cref="LogEventLevel.Verbose"/>) - e.g. frame update messages
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[StackTraceHidden]
	public static void TrackEvent(this ILogger logger, object? sender, EventArgs eventArgs)
	{
		logger.Debug("{CallbackName} from {@Sender}: {@EventArgs}", new StackFrame(1).GetMethod(), sender, eventArgs);
	}

	/// <summary>Logs an expression and it's value to the logger, with an optional message</summary>
	/// <param name="logger">Logger to log expression to</param>
	/// <param name="value">The value to log</param>
	/// <param name="message">Optional message to be logged after the expression</param>
	/// <param name="expressionString">(Please don't set this) string containing the compile time expression passed to <paramref name="value"/></param>
	/// <typeparam name="T">Type of the expression</typeparam>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[StackTraceHidden]
	public static void LogExpression<T>(this ILogger logger, LogEventLevel level, T value, string? message = null, [CallerArgumentExpression("value")] string expressionString = "Unknown Variable")
	{
		if (message is null)
			logger.Write(level, "{Expression}: {Value}", expressionString, value);
		else
			logger.Write(level, "{Expression}: {Value}, {Message}", expressionString, value, message);
	}
}