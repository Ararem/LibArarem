using System;
using System.Collections.Generic;
using System.Linq;

namespace LibArarem.Core.Exceptions;

/// <summary>The exception that is thrown when the value of an argument is outside the allowable range of values as defined by the invoked method.</summary>
public sealed class ArgumentOutOfRangeException<T> : ArgumentException
{
	/// <inheritdoc cref="ArgumentException"/>
	/// <param name="actualValue">Actual value of the object that caused this exception to be thrown</param>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="paramName">The name of the parameter that caused the current exception.</param>
	/// <param name="innerException">
	///  The exception that is the cause of the current exception. If the <paramref name="innerException"/> parameter is not a null reference, the current
	///  exception is raised in a <see langword="catch"/> block that handles the inner exception.
	/// </param>
	/// <param name="bounds">Array of bounds between which the values are valid</param>
	public ArgumentOutOfRangeException(T actualValue, string? message=null, string? paramName = null, Exception? innerException = null, params (T Lower, T Upper)[] bounds) : base(message, paramName, innerException)
	{
		ActualValue = actualValue;
		Bounds      = bounds;
	}

	/// <inheritdoc cref="ArgumentException"/>
	/// <param name="actualValue">Actual value of the object that caused this exception to be thrown</param>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="paramName">The name of the parameter that caused the current exception.</param>
	/// <param name="bounds">Array of bounds between which the values are valid</param>
	public ArgumentOutOfRangeException(T actualValue, string? message=null, string? paramName = null, params (T Lower, T Upper)[] bounds) : this(actualValue, message,paramName,null,bounds)
	{
	}

	/// <inheritdoc cref="ArgumentException"/>
	/// <param name="actualValue">Actual value of the object that caused this exception to be thrown</param>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="paramName">The name of the parameter that caused the current exception.</param>
	/// <param name="bounds">Array of bounds containing valid values</param>
	public ArgumentOutOfRangeException(T actualValue, string? message=null, string? paramName = null, params T[] bounds) : this(actualValue, message,paramName,null, bounds.Select(b=>(b,b)).ToArray())
	{
	}

	/// <summary>Actual value of the object that was passed into the method</summary>
	public T ActualValue { get; }

	/// <summary>Enumerable containing the valid bounds for the value</summary>
	public (T Lower, T Upper)[] Bounds { get; }
}