using JetBrains.Annotations;
using LibEternal.ObjectPools;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace LibEternal.SourceGenerators.Helper
{
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
			return $"<{string.Join(", ", typeParameters.Select(t => t.Name))}>";
		}

		/// <summary>
		/// Builds a string that holds the generic type constraints that will satisfy those of the <paramref name="typeParameterSymbols"/>
		/// </summary>
		/// <param name="typeParameterSymbols">A list of constraints to generate the string with</param>
		/// <returns>A string that can be used when generating source code to constrain a set of types for a generic type or method</returns>
		[MustUseReturnValue]
		public static string BuildGenericTypeConstraints(ImmutableArray<ITypeParameterSymbol> typeParameterSymbols)
		{
			StringBuilder sb = StringBuilderPool.GetPooled();
			foreach (ITypeParameterSymbol param in typeParameterSymbols)
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
				sb.AppendLine($"\t\t\twhere {param} : {string.Join(", ", constraints)}");
			}

			return StringBuilderPool.ToStringAndReturn(sb);
		}
	}
}