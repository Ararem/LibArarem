using LibEternal.JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibEternal.Extensions
{
	/// <summary>
	/// Contains extension methods for changing a number to Metric representation (ToMetric)
	/// and from Metric representation back to the number (FromMetric)
	/// </summary>
	[PublicAPI]
	public static class MetricNumeralExtensions
	{
		/// <summary>
		/// Symbols is a list of every symbols for the Metric system.
		/// </summary>
		private static readonly char[][] Symbols =
		{
				new[] {'k', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y'},
				new[] {'m', 'μ', 'n', 'p', 'f', 'a', 'z', 'y'}
		};

		/// <summary>
		/// Names link a Metric symbol (as key) to its name (as value).
		/// </summary>
		/// <remarks>
		/// We dont support :
		/// {'h', "hecto"},
		/// {'da', "deca" }, // !string
		/// {'d', "deci" },
		/// {'c', "centi"},
		/// </remarks>
		private static readonly Dictionary<char, string> Names = new Dictionary<char, string>
		{
				{'Y', "yotta"}, {'Z', "zetta"}, {'E', "exa"}, {'P', "peta"}, {'T', "tera"}, {'G', "giga"}, {'M', "mega"}, {'k', "kilo"},
				{'m', "milli"}, {'μ', "micro"}, {'n', "nano"}, {'p', "pico"}, {'f', "femto"}, {'a', "atto"}, {'z', "zepto"}, {'y', "yocto"}
		};

		/// <summary>
		/// Converts a Metric representation into a number.
		/// </summary>
		/// <remarks>
		/// We don't support input in the format {number}{name} nor {number} {name}.
		/// We only provide a solution for {number}{symbol} and {number} {symbol}.
		/// </remarks>
		/// <param name="input">Metric representation to convert to a number</param>
		/// <example>
		/// "1k".FromMetric() => 1000d
		/// "123".FromMetric() => 123d
		/// "100m".FromMetric() => 1E-1
		/// </example>
		/// <returns>A number after a conversion from a Metric representation.</returns>
		public static double FromMetric(this string input)
		{
			if (input == null) throw new ArgumentNullException(nameof(input));
			input = input.Trim();
			input = ReplaceNameBySymbol(input);
			if (input.Length == 0 || input.IsInvalidMetricNumeral())
				throw new ArgumentException("Empty or invalid Metric string.", nameof(input));
			input = input.Replace(" ", string.Empty);
			char last = input[input.Length - 1];
			if (!char.IsLetter(last)) return double.Parse(input);
			double GetExponent(ICollection<char> symbols) => (symbols.IndexOf(last) + 1) * 3;
			double number = double.Parse(input.Remove(input.Length - 1));
			double exponent = Math.Pow(10, Symbols[0].Contains(last) ? GetExponent(Symbols[0]) : -GetExponent(Symbols[1]));
			return number * exponent;
		}

		/// <summary>
		/// Replace every symbol's name by its symbol representation.
		/// </summary>
		/// <param name="input">Metric representation with a name or a symbol</param>
		/// <returns>A metric representation with a symbol</returns>
		private static string ReplaceNameBySymbol(string input)
		{
			return Names.Aggregate(input, (current, name) => current.Replace(name.Value, name.Key.ToString()));
		}

		/// <summary>
		/// Converts a number into a valid and Human-readable Metric representation.
		/// </summary>
		/// <remarks>
		/// Inspired by a snippet from Thom Smith.
		/// <see cref="http://stackoverflow.com/questions/12181024/formatting-a-number-with-a-metric-prefix"/>
		/// </remarks>
		/// <param name="input">Number to convert to a Metric representation.</param>
		/// <param name="isSplitBySpace">True will split the number and the symbol with a whitespace.</param>
		/// <param name="useSymbol">True will use symbol instead of name</param>
		/// <example>
		/// 1000d.ToMetric() => "1k"
		/// 123d.ToMetric() => "123"
		/// 1E-1.ToMetric() => "100m"
		/// </example>
		/// <returns>A valid Metric representation</returns>
		public static string ToMetric(this double input, bool isSplitBySpace = false, bool useSymbol = true)
		{
			if (input.Equals(0)) return input.ToString();
			if (input.IsOutOfRange()) throw new ArgumentOutOfRangeException(nameof(input));
			int exponent = (int) Math.Floor(Math.Log10(Math.Abs(input)) / 3);
			if (exponent == 0) return input.ToString();
			double number = input * Math.Pow(1000, -exponent);
			char symbol = Math.Sign(exponent) == 1 ? Symbols[0][exponent - 1] : Symbols[1][-exponent - 1];
			return number
			       + (isSplitBySpace ? " " : string.Empty)
			       + GetUnit(symbol, useSymbol);
		}

		/// <summary>
		/// Converts a number into a valid and Human-readable Metric representation.
		/// </summary>
		/// <remarks>
		/// Inspired by a snippet from Thom Smith.
		/// <see cref="http://stackoverflow.com/questions/12181024/formatting-a-number-with-a-metric-prefix"/>
		/// </remarks>
		/// <param name="input">Number to convert to a Metric representation.</param>
		/// <param name="isSplitBySpace">True will split the number and the symbol with a whitespace.</param>
		/// <param name="useSymbol">True will use symbol instead of name</param>
		/// <example>
		/// 1000.ToMetric() => "1k"
		/// 123.ToMetric() => "123"
		/// 1E-1.ToMetric() => "100m"
		/// </example>
		/// <returns>A valid Metric representation</returns>
		public static string ToMetric(this int input, bool isSplitBySpace = false, bool useSymbol = true)
		{
			return Convert.ToDouble(input).ToMetric(isSplitBySpace, useSymbol);
		}

		/// <summary>
		/// Get the unit from a symbol of from the symbol's name.
		/// </summary>
		/// <param name="symbol">The symbol linked to the unit</param>
		/// <param name="useSymbol">True will use symbol instead of name</param>
		/// <returns>A symbol or a symbol's name</returns>
		private static string GetUnit(char symbol, bool useSymbol)
		{
			return useSymbol ? symbol.ToString() : Names[symbol];
		}

		/// <summary>
		/// Check if a Metric representation is out of the valid range.
		/// </summary>
		/// <param name="input">A Metric representation who might be out of the valid range.</param>
		/// <returns>True if input is out of the valid range.</returns>
		private static bool IsOutOfRange(this double input)
		{
			const int limit = 27;
			double bigLimit = Math.Pow(10, limit);
			double smallLimit = Math.Pow(10, -limit);
			bool Outside(double min, double max) => !(max > input && input > min);
			return (Math.Sign(input) == 1 && Outside(smallLimit, bigLimit))
			       || (Math.Sign(input) == -1 && Outside(-bigLimit, -smallLimit));
		}

		/// <summary>
		/// Check if a string is not a valid Metric representation.
		/// A valid representation is in the format "{0}{1}" or "{0} {1}"
		/// where {0} is a number and {1} is an allowed symbol.
		/// </summary>
		/// <param name="input">A string who might contain a invalid Metric representation.</param>
		/// <returns>True if input is not a valid Metric representation.</returns>
		private static bool IsInvalidMetricNumeral(this string input)
		{
			int index = input.Length - 1;
			char last = input[index];
			bool isSymbol = Symbols[0].Contains(last) || Symbols[1].Contains(last);
			return !double.TryParse(isSymbol ? input.Remove(index) : input, out double _);
		}

		/// <summary>
		/// Reports the zero-based index of the first occurrence of the specified Unicode
		/// character in this string.
		/// </summary>
		/// <param name="chars">The string containing the value.</param>
		/// <param name="value">A Unicode character to seek.</param>
		/// <returns>
		/// The zero-based index position of value if that character is found, or -1 if it is not.
		/// </returns>
		private static int IndexOf(this ICollection<char> chars, char value)
		{
			for (int i = 0; i < chars.Count; i++)
				if (chars.ElementAt(i).Equals(value))
					return i;
			return -1;
		}
	}
}