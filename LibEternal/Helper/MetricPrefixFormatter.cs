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
		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			double value = Convert.ToDouble(arg);
        
			double exponent = Math.Log10(Math.Abs(value));
			double engExponent = Math.Floor(exponent / 3) * 3;

			string symbol = notationSymbols.ContainsKey(engExponent) ? notationSymbols[engExponent] : "e" + engExponent;

			return (value * Math.Pow(10, (int)-engExponent)) + symbol;
		}

		/// <inheritdoc />
		public object GetFormat(Type formatType)
		{
			return formatType == typeof(ICustomFormatter) ? this : null;
		}
	}
}