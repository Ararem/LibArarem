using LibEternal.JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace LibEternal.Helper
{
	/// <summary>
	/// An <see cref="IFormatProvider"/> that formats numbers using metric prefixes.
	/// </summary>
	/// <example>
	///<code>
	/// (0.00005678).ToString(new MetricPrefixFormatter()); //56.78μ
	/// (0.1234).ToString(new MetricPrefixFormatter());     //123.4m
	/// (0).ToString(new MetricPrefixFormatter());          //0
	/// (1300).ToString(new MetricPrefixFormatter());       //1.3k
	/// (19000).ToString(new MetricPrefixFormatter());      //19k
	/// </code>
	/// </example>
	[PublicAPI]
	public class MetricPrefixFormatter : IFormatProvider, ICustomFormatter
	{
		private readonly Dictionary<double, string> notationSymbols = new Dictionary<double, string>
		{
				{double.NegativeInfinity, ""}, //Handles when value is 0
				{-24, "y"},
				{-21, "z"},
				{-18, "a"},
				{-15, "f"},
				{-12, "p"},
				{-9, "n"},
				{-6, "μ"},
				{-3, "m"},
				{0, ""},
				{3, "k"},
				{6, "M"},
				{9, "G"},
				{12, "T"},
				{15, "P"},
				{18, "E"},
				{21, "Z"},
				{24, "Y"},
		};

		/// <inheritdoc />
		public string Format(string format, object arg, IFormatProvider formatProvider = null)
		{
			double value = Convert.ToDouble(arg);

			return Format(value, format, formatProvider);
		}
		
		/// <inheritdoc cref="Format(string,object,System.IFormatProvider)"/>
		public string Format(double value, string format = "", IFormatProvider formatProvider = null)
		{
			//Prevent recursive loops
			if (formatProvider is MetricPrefixFormatter) formatProvider = null;
			
			//Gets the rounded third log (cubed log??) idk how to explain it better
			double key = Math.Floor(Math.Log10(Math.Abs(value)) / 3) * 3;

			//Try get the value from the dictionary
			if (!notationSymbols.TryGetValue(key, out string postfix))
				//If not just use scientific notation
				postfix = "e" + key;

			//Shrink it down to the range < ±1000;
			//Max is ±999.999999999... etc
			value *= Math.Pow(10, (int) -key);
			return $"{value.ToString(format, formatProvider)} {postfix}";
		}

		/// <inheritdoc />
		public object GetFormat(Type formatType)
		{
			return formatType == typeof(ICustomFormatter) ? this : null;
		}
	}
}