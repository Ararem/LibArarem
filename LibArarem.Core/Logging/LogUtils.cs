using Serilog.Events;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Serilog.Log;

namespace LibArarem.Core.Logging;

/// <summary>Class containing helper methods for logging</summary>
public static class LogUtils
{
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
	public static void TrackEvent(object? sender, EventArgs eventArgs)
	{
		Debug("{CallbackName} from {Sender}: {@EventArgs}", new StackFrame(1).GetMethod(), sender, eventArgs);
	}

	/// <summary>Logs an expression and it's value to the logger, with an optional message</summary>
	/// <param name="value">The value to log</param>
	/// <param name="message">Optional message to be logged after the expression</param>
	/// <param name="expressionString">(Please don't set this) string containing the compile time expression passed to <paramref name="value"/></param>
	/// <typeparam name="T">Type of the expression</typeparam>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[StackTraceHidden]
	public static void LogExpression<T>(T value, string? message = null, [CallerArgumentExpression("value")] string expressionString = "Unknown Variable")
	{
		if (message is null)
			Verbose("{Expression}: {Value}", expressionString, value);
		else
			Verbose("{Expression}: {Value}, {Message}", expressionString, value, message);
	}

	/// <summary>Logs an expression and it's value to the logger, with an optional message</summary>
	/// <param name="level"><see cref="LogEventLevel"/> at which to log the event</param>
	/// <param name="value">The value to log</param>
	/// <param name="message">Optional message to be logged after the expression</param>
	/// <param name="expressionString">(Please don't set this) string containing the compile time expression passed to <paramref name="value"/></param>
	/// <typeparam name="T">Type of the expression</typeparam>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[StackTraceHidden]
	public static void LogExpression<T>(LogEventLevel level, T value, string? message = null, [CallerArgumentExpression("value")] string expressionString = "Unknown Variable")
	{
		if (message is null)
			Write(level, "{Expression}: {Value}", expressionString, value);
		else
			Write(level, "{Expression}: {Value}, {Message}", expressionString, value, message);
	}
}