using LibEternal.SourceGenerators.AutoVersioning;
using System.Reflection;

[assembly: AutoVersion]
[assembly: AssemblyCompany("LibEternal")]
[assembly: AssemblyProductAttribute("LibEternal")]
[assembly: AssemblyTitleAttribute("LibEternal")]

#if DEBUG
[assembly: AssemblyConfigurationAttribute("Debug")]
#else
[assembly: AssemblyConfigurationAttribute("Release")]
#endif
