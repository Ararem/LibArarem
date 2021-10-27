using System;

namespace LibEternal.SourceGenerators.AutoVersioning
{
	/// <summary>
	/// Marks a class to automatically generate incrementing version numbers based on the current <see cref="DateTime"/> of compilation
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class AutoVersionAttribute : Attribute
	{
		/// <summary>
		/// Constructs a new <see cref="AutoVersionAttribute"/> with the specified build version numbers
		/// </summary>
		/// <remarks>
		///	Any values left <see langword="null"/> will be automatically generated
		/// </remarks>
		public AutoVersionAttribute(string? major = null, string? minor = null, string? build = null, string? revision = null)
		{
			Major = major;
			Minor = minor;
			Build = build;
			Revision = revision;
		}

		public string? Major { get; }
		public string? Minor { get; }
		public string? Build { get; }
		public string? Revision { get; }
	}
}