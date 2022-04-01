using System;

namespace LibEternal.SourceGenerators.AutoVersioning
{
	/// <summary>
	///  Marks a class to automatically generate incrementing version numbers based on the current <see cref="DateTime"/> of compilation
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class AutoVersionAttribute : Attribute
	{
		/// <summary>
		///  Constructs a new <see cref="AutoVersionAttribute"/> with the specified build version numbers
		/// </summary>
		/// <remarks>
		///  Any values left <see langword="null"/> will be automatically generated
		/// </remarks>
		public AutoVersionAttribute(string? major = null, string? minor = null, string? build = null, string? revision = null)
		{
			Major    = major;
			Minor    = minor;
			Build    = build;
			Revision = revision;
		}

		/// <summary>
		///  An optional major version number
		/// </summary>
		public string? Major { get; }

		/// <summary>
		///  An optional minor version number
		/// </summary>
		public string? Minor { get; }

		/// <summary>
		///  An optional build version number
		/// </summary>
		public string? Build { get; }

		/// <summary>
		///  An optional revision version number
		/// </summary>
		public string? Revision { get; }
	}
}