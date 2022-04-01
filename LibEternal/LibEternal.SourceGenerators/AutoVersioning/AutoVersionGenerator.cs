using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace LibEternal.SourceGenerators.AutoVersioning
{
	/// <summary>
	/// Automatically generates incrementing version numbers based on the current <see cref="DateTime"/> of compilation
	/// </summary>
	[Generator]
	public class AutoVersionGenerator : ISourceGenerator
	{
		/// <inheritdoc />
		public void Initialize(GeneratorInitializationContext context)
		{
		}

		/// <inheritdoc />
		public void Execute(GeneratorExecutionContext context)
		{
			var asmAttributes = context.Compilation.Assembly.GetAttributes();
			INamedTypeSymbol autoVersionAttribute = context.Compilation.GetTypeByMetadataName(typeof(AutoVersionAttribute).FullName!)!;
			AttributeData? attribute = asmAttributes.FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, autoVersionAttribute));
			// ReSharper disable once UseNullPropagationWhenPossible
			if (attribute is null) return; //No attribute on the assembly

			//Hard-coding be like
			string major = attribute.ConstructorArguments[0].Value?.ToString() ?? GetMajor();
			string minor = attribute.ConstructorArguments[1].Value?.ToString() ?? GetMinor();
			string build = attribute.ConstructorArguments[2].Value?.ToString() ?? GetBuild();
			string revision = attribute.ConstructorArguments[3].Value?.ToString() ?? GetRevision();

			string versionString = $"{major}.{minor}.{build}.{revision}";
			context.AddSource("AutoVersion",
			$@"
using System.Reflection;

[assembly: AssemblyFileVersion			(""{versionString}"")]
[assembly: AssemblyInformationalVersion	(""{versionString}"")]
[assembly: AssemblyVersion				(""{versionString}"")]
"
			);
		}

	#region Version Generating Methods

		private static string GetMajor() => DateTime.Now.Year.ToString();
		private static string GetMinor() => DateTime.Now.Month.ToString("00");
		private static string GetBuild() => DateTime.Now.Day.ToString("00");
		private static string GetRevision() => DateTime.Now.TimeOfDay.ToString("hhmm");

	#endregion
	}
}