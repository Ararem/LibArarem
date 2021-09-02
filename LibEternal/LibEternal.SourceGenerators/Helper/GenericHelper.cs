using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace LibEternal.SourceGenerators.Helper
{
	/// <summary>
	/// A helper class for dealing with source generation for generics
	/// </summary>
	public static class GenericHelper
	{
		/// <summary>
		/// Builds the string representation of a set of type parameters (E.g. <c>"&lt;TBase, TEnumerable, TFoo&gt;"</c>)
		/// </summary>
		/// <param name="typeParameters">A list of type parameters to use</param>
		/// <remarks>Will return <see cref="string.Empty"/> if a zero-length or uninitialized array is passed in</remarks>
		[MustUseReturnValue]
		public static string BuildGenericTypeArgs(ImmutableArray<ITypeParameterSymbol> typeParameters)
		{
			if (typeParameters.IsDefaultOrEmpty) return string.Empty;
			StringBuilder sb = new(128);
			sb.Append('<');
			for (int i = 0; i < typeParameters.Length; i++)
			{
				sb.Append(typeParameters[i].Name);
				if (i != typeParameters.Length - 1) sb.Append(", ");
			}

			sb.Append('>');
			return sb.ToString();
		}

		/// <summary>
		/// Builds a string that holds the generic type constraints that will satisfy those of the <paramref name="typeParameterSymbols"/>
		/// </summary>
		/// <param name="typeParameterSymbols">A list of constraints to generate the string with</param>
		/// <param name="indent">An optional indent to apply to lines after the first (i.e. will only apply to lines 2,3,4 if 4 lines are generated)</param>
		/// <returns>A string that can be used when generating source code to constrain a set of types for a gen/eric type or method</returns>
		[MustUseReturnValue]
		// ReSharper disable once CognitiveComplexity
		public static string BuildGenericTypeConstraints(ImmutableArray<ITypeParameterSymbol> typeParameterSymbols, string indent = "")
		{
			if (typeParameterSymbols.IsDefaultOrEmpty) return string.Empty;
			StringBuilder sb = new(256);
			foreach (var param in typeParameterSymbols)
			{
				//If it has no constraints we skip this
				if (!param.HasConstructorConstraint && !param.HasNotNullConstraint && !param.HasReferenceTypeConstraint && !param.HasUnmanagedTypeConstraint && !param.HasValueTypeConstraint && (param.ConstraintTypes.Length == 0)) break;

				List<string> constraints = new();
				//Special constraints, such as new, notnull, etc come before type constraints
				if (param.HasReferenceTypeConstraint) constraints.Add("class");
				else if (param.HasUnmanagedTypeConstraint) constraints.Add("unmanaged");
				else if (param.HasValueTypeConstraint) constraints.Add("struct");
				else if (param.HasNotNullConstraint) constraints.Add("notnull");
				//Add all the `where T inherits from this class` constraints
				constraints.AddRange(param.ConstraintTypes.Select(t => $"{t}"));
				//The new constraint has to come last
				if (param.HasConstructorConstraint) constraints.Add("new()");
				sb.Append($"\n{indent}where {param} : {string.Join(", ", constraints)}");
			}

			return sb.ToString();
		}
	}
}