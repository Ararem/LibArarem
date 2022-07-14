using LibArarem.Core.ObjectPools;
using System;
using System.Linq;

namespace LibArarem.Core.Exceptions;

/// <summary>The exception that is thrown when the value of an argument is outside the allowable range of values as defined by the invoked method.</summary>
public sealed class ArgumentOutOfRangeException<T> : ArgumentOutOfRangeException
{
	/// <inheritdoc cref="ArgumentException"/>
	/// <param name="actualValue">Actual value of the object that caused this exception to be thrown</param>
	/// <param name="paramName">The name of the parameter that caused the current exception.</param>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="bounds">Array of bounds between which the values are valid</param>
	public ArgumentOutOfRangeException(T actualValue, string? paramName = null, string? message = null, params (T Lower, T Upper)[] bounds) : base(paramName, actualValue, message)
	{
		Bounds = bounds;
	}

	/// <inheritdoc cref="ArgumentException"/>
	/// <param name="actualValue">Actual value of the object that caused this exception to be thrown</param>
	/// <param name="paramName">The name of the parameter that caused the current exception.</param>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="bounds">Array of bounds containing valid values</param>
	public ArgumentOutOfRangeException(T actualValue, string? paramName = null, string? message = null, params T[] bounds) : this(actualValue, paramName, message, bounds.Select(b => (b, b)).ToArray())
	{
	}

	/// <summary>Enumerable containing the valid bounds for the value</summary>
	public (T Lower, T Upper)[] Bounds { get; }

	/// <inheritdoc/>
	public override string Message
	{
		get
		{
			string baseMessage = base.Message;
			return $@"{baseMessage}
Valid bounds are:
{StringBuilderPool.BorrowInline(static (sb, bounds) => {
	for (int i = 0; i < bounds.Length; i++)
	{
		(T lower, T upper) = bounds[i];
		//If both bounds are the same, don't add the ellipses
		sb.Append(Equals(lower, upper) ? $"{lower}\n" : $"{lower} ... {upper}\n");
	}
}, Bounds)}";
		}
	}
}