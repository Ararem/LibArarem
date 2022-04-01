using JetBrains.Annotations;
using System;

namespace LibEternal.SourceGenerators.StaticInstanceGeneration
{
	/// <summary>
	///  Marks a class or struct so that any instance members (properties, fields or methods) will also be duplicated into static members, which will call
	///  their counterparts on the targeted instance
	/// </summary>
	/// <remarks>The generated class will be partial, so you can add custom code yourself if needed. An implementation for the <see cref="GeneratedInstanceName">Generated instance</see> must be provided within the partial copy of the generated class
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	[PublicAPI]
	public class GenerateStaticInstanceMembersAttribute : Attribute
	{
		/// <summary>
		///  The name of the variable that will be used as the instance. This must be a member of 
		/// </summary>
		public readonly string GeneratedInstanceName;

		/// <summary>
		///  The name of the type that should be generated
		/// </summary>
		public readonly string GeneratedTypeName;

		/// <summary>
		///  The namespace the generated type will be placed in.
		/// </summary>
		public readonly string GeneratedTypeNamespace;

		/// <summary>The constructor for a <see cref="GenerateStaticInstanceMembersAttribute"/></summary>
		/// <param name="generatedTypeNamespace">The namespace the generated type will be placed in</param>
		/// <param name="generatedTypeName">The name of the type that should be generated</param>
		/// <param name="generatedInstanceName">The name of the variable that will be used as the instance</param>
		public GenerateStaticInstanceMembersAttribute(string generatedTypeNamespace,
													string   generatedTypeName,
													string   generatedInstanceName    = "instance")
		{
			GeneratedTypeName      = generatedTypeName;
			GeneratedInstanceName  = generatedInstanceName;
			GeneratedTypeNamespace = generatedTypeNamespace;
		}
	}
}