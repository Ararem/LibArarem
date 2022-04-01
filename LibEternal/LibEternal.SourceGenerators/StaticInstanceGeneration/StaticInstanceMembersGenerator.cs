﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.MethodKind;
using static LibEternal.SourceGenerators.Helper.DocumentationHelper;
using static LibEternal.SourceGenerators.Helper.GenericHelper;

namespace LibEternal.SourceGenerators.StaticInstanceGeneration
{
	/// <inheritdoc/>
	[Generator]
	public sealed class StaticInstanceMembersGenerator : ISourceGenerator
	{
	#region Diagnostic Descriptions

		// ReSharper disable StringLiteralTypo
		// ReSharper disable CommentTypo
		// ReSharper disable InternalOrPrivateMemberNotDocumented

		//SIMG stands for "Static Instance Member Generator
		private static readonly DiagnosticDescriptor ClassIsStatic = new(
				"SIMG01",
				"Class cannot be static",
				"The target class must not be static",
				"Usage",
				DiagnosticSeverity.Error,
				true,
				"The class that is marked for generation is a static class, which is unsupported (as it has no instance members). Make the class an instance class (remove the `static` modifier) to fix this error."
		);

		//TODO: Unsupported stuff diagnostics
		// ReSharper restore StringLiteralTypo
		// ReSharper restore CommentTypo
		// ReSharper restore InternalOrPrivateMemberNotDocumented

	#endregion

		/// <inheritdoc/>
		public void Initialize(GeneratorInitializationContext ctx)
		{
			// Register a factory that can create our custom syntax receiver
			ctx.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
			//This is awesome by the way!!!
			// #if DEBUG
			// if (!Debugger.IsAttached) Debugger.Launch();
			// #endif
		}

		//From my understanding, the syntax receiver is the "scan" phase that finds stuff to work on,
		//and the "execute" is where we actually do the work
		/// <inheritdoc/>
		public void Execute(GeneratorExecutionContext context)
		{
			lock (_log)
			{
				_log.Clear();
			}

			SyntaxReceiver   receiver                  = (SyntaxReceiver)context.SyntaxContextReceiver!;
			INamedTypeSymbol genMembersAttributeSymbol = context.Compilation.GetTypeByMetadataName(typeof(GenerateStaticInstanceMembersAttribute).FullName!)!;
			Log($"GenMembers Attribute Symbol is {genMembersAttributeSymbol}");

			Log("Starting execution");
			Stopwatch totalStopWatch = Stopwatch.StartNew();
			foreach (var type in receiver.Types) ProcessType(type, context, genMembersAttributeSymbol);

			totalStopWatch.Stop();
			Log("");
			Log($"Execution complete in {totalStopWatch.Elapsed:mm':'ss'.'FF}");

			//Here we write to our log file
			lock (_log)
			{
				_log.Insert(0, $@"/*
===== {DateTime.Now} ====={Environment.NewLine}");
				_log.Append("*/");
				context.AddSource("SourceGenLog", SourceText.From(_log.ToString(), Encoding.UTF8));
				_log.Clear();
			}
		}

		/// <summary>
		///  Processes a given <see cref="ITypeSymbol"/>, generating static instance members for it
		/// </summary>
		/// <param name="type">The type that should be processed</param>
		/// <param name="context">The </param>
		/// <param name="genMembersAttributeSymbol">The attribute that marks members for generation</param>
		// ReSharper disable once SuggestBaseTypeForParameter
		//Lmao i love how i'm just ignoring the warnings about complexity
		//One day this is going to com back and bite me, I'm sure of it
		// ReSharper disable once CognitiveComplexity
		// ReSharper disable once CyclomaticComplexity
		private static void ProcessType(INamedTypeSymbol type, GeneratorExecutionContext context, INamedTypeSymbol genMembersAttributeSymbol)
		{
			StringBuilder sb = new();
			Stopwatch     sw = Stopwatch.StartNew();
			Log($"\nExamining type {type}");

			//Check if it is actually something we should generate
			//So if we don't have any attributes that match our 'generate members on this' attribute we return
			AttributeData? genAttribute = type.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, genMembersAttributeSymbol));
			if (genAttribute is null)
			{
				Log("Class not marked for generation, ignoring");
				return;
			}

			Log("Type marked for generation, processing");

			//Also check for static-ness
			if (type.IsStatic)
			{
				Log("\tType is static, ignoring");
				ReportDiagnostic(ClassIsStatic, type);
				return;
			}

			Log("\tPreparing to generate static class version:");
			//I don't like hard-coding but what else is there to do?
			string newTypeNamespace         = genAttribute.ConstructorArguments[0].Value!.ToString() ?? $"{type.ContainingNamespace}.Generated";
			string newTypeName              = genAttribute.ConstructorArguments[1].Value!.ToString() ?? $"Static_{type.Name}";
			string instanceName             = genAttribute.ConstructorArguments[2].Value!.ToString() ?? "__instance";
			Log($"\tNamespace:\t{newTypeNamespace}");
			Log($"\tType Name:\t{newTypeName}");
			Log($"\tInstance:\t{instanceName}");
			Log("\tGenerating class");
			//TODO: `using` statements for imports
			sb.Append($@"//Auto generated by a roslyn source generator

namespace {newTypeNamespace}
{{
	//Source type is {type}
	///{Inheritdoc(type)}
	[System.CodeDom.Compiler.GeneratedCode(""LibEternal.SourceGenerators"", ""{typeof(StaticInstanceMembersGenerator).Assembly.GetName().Version}"")]
	public partial class {newTypeName}{BuildGenericTypeArgs(type.TypeParameters)}{BuildGenericTypeConstraints(type.TypeParameters, "\t\t\t")}
	{{");
			//Now we generate a static version of each instance member
			//Only generate public instance members
			//TODO: Handle non-public members
			var members = type.GetMembers().Where(m => !m.IsStatic && (m.DeclaredAccessibility == Accessibility.Public)).ToArray();
			Log($"\t\tProcessing {members.Length} members");
			foreach (var member in members)
				switch (member)
				{
					//Normal (read/write fields)
					case IFieldSymbol { IsReadOnly: false } field:
						Log($"\t\tGenerating read/write field  {field}");
						sb.Append($@"

		///{Inheritdoc(field)}
		public static {field.Type} {field.Name}
		{{
			get => {instanceName}.{field.Name};
			set => {instanceName}.{field.Name} = value;
		}}");
						break;

					//Readonly fields
					case IFieldSymbol { IsReadOnly: true } field:
						Log($"\t\tGenerating readonly field    {field}");
						sb.Append($@"

		///{Inheritdoc(field)}
		public static {field.Type} {field.Name} => {instanceName}.{field.Name};");
						break;

					//Non-indexer properties
					case IPropertySymbol { IsIndexer: false } prop:
						if ((prop.GetMethod == null) && prop.SetMethod!.IsInitOnly) //It has no get method, and the set method is init only
						{
							Log($"\t\tIgnoring init-only property  {prop} ");
							break;
						}

						Log($"\t\tGenerating property          {prop}");
						sb.Append($@"

		///{Inheritdoc(prop)}
		public static {prop.Type} {prop.Name}
		{{");
						if (prop.GetMethod is not null)
							sb.Append($@"
			get => {instanceName}.{prop.Name};");
						if (prop.SetMethod is { IsInitOnly: false }) //BTW We can't really make static versions of init accessors so just skip them. Also this checks for null too!
							sb.Append($@"
			set => {instanceName}.{prop.Name} = value;");
						sb.Append(@"
		}");
						break;

					case IMethodSymbol { MethodKind: Ordinary } method:
						Log($"\t\tGenerating normal method     {method}");
						//Build the return type strings
						string returnType = "";
						if (method.IsAsync && !method.ReturnsVoid) returnType += "async "; //We don't add `async` to async void's because they're special (more like special needs)
						else if (method.ReturnsByRefReadonly) returnType      += "ref readonly ";
						else if (method.ReturnsByRef) returnType              += "ref ";
						returnType += method.ReturnType;

						//Now build the parameters
						//methodCallArgs is when we actually call the method: `foo(x,y,z)`
						//methodDecArgs is when we declare the method: `foo(int x, int z, bar z)`
						//TODO: Nullable stuff for parameters
						string genericArgs           = BuildGenericTypeArgs(method.TypeParameters);
						string genericArgConstraints = method.IsGenericMethod ? BuildGenericTypeConstraints(method.TypeParameters, "\t\t\t") : string.Empty;
						string methodCallArgs        = string.Join(", ", method.Parameters.Select(p => p.Name));
						string methodDecArgs         = string.Join(", ", method.Parameters.Select(p => $"{p.Type}{(p.NullableAnnotation == NullableAnnotation.Annotated ? "?" : string.Empty)} {p.Name}"));

						sb.Append($@"

		///{Inheritdoc(method)}
		public static {returnType} {method.Name}{genericArgs}({methodDecArgs}){genericArgConstraints}
			=> {(method.IsAsync && !method.ReturnsVoid ? "await " : "")}{instanceName}.{method.Name}{genericArgs}({methodCallArgs});");
						break;

					//Here we handle cases that we skip, so they don't hit the default block and throw warnings

					//Skip property get/set methods because (1) They're reserved, (2) We already generated the property itself above
					case IMethodSymbol { MethodKind: PropertyGet or PropertySet }:
						Log($"\t\tIgnoring property get/set    {member}");
						break;
					//Constructors don't work in a static class lol:
					case IMethodSymbol { MethodKind: Constructor or Destructor or StaticConstructor }:
						Log($"\t\tIgnoring con/destructor      {member}");
						break;
					case IMethodSymbol { MethodKind: MethodKind.Conversion or BuiltinOperator or UserDefinedOperator }:
						Log($"\t\tIgnoring conversion/operator {member}");
						break;
					case IMethodSymbol { MethodKind: EventAdd or EventRaise or EventRemove or DelegateInvoke }:
						Log($"\t\tIgnoring event method        {member}");
						break;
					//Indexers aren't really supportable, the best we can try is create a fake property
					//TODO: Maybe support this?
					case IPropertySymbol { IsIndexer: true } prop:
						Log($"\t\tIgnoring indexer property    {prop}");
						break;

					//Default unhandled cases, probably a bad thing
					default:
						Log($"\t\t*Couldn't generate {member.Kind.ToString().ToLowerInvariant().PadRight(9)} {member}");
						Debugger.Launch();
						break;
				}

			sb.Append(@"
	} //End class
} //End namespace
");
			context.AddSource($"{type.Name}__{newTypeName}", sb.ToString());
			Log($"\tClass generation complete ({sw.Elapsed:ss'.'FF})");

			void ReportDiagnostic(DiagnosticDescriptor desc, ISymbol target)
			{
				//Gotta loop through all the locations the class was declared (partial classes)
				foreach (var loc in target.Locations)
					context.ReportDiagnostic(Diagnostic.Create(desc, loc));
			}
		}

		/// <inheritdoc/>
		private sealed class SyntaxReceiver : ISyntaxContextReceiver
		{
			/// <summary>
			///  The list of types that was found by the receiver
			/// </summary>
			public readonly List<INamedTypeSymbol> Types = new();

			/// <summary>
			///  Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
			/// </summary>
			public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
			{
				if (context.Node is TypeDeclarationSyntax typeDec and not InterfaceDeclarationSyntax)
				{
					//Get the symbol being declared by the type
					INamedTypeSymbol type = context.SemanticModel.GetDeclaredSymbol(typeDec)!;
					Types.Add(type);
				}
			}
		}

	#region Logging, ignore this

		/// <summary>
		///  Stores messages we need to log later
		/// </summary>
		// ReSharper disable once InconsistentNaming
		private static readonly StringBuilder _log = new();

		private static void Log(string s)
		{
			lock (_log)
			{
				_log.AppendLine(s);
			}
		}

	#endregion
	}
}