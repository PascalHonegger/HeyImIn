// documentation:
// * there are 2 assembly files for each .NET project:
// ** 1) private AssemblyInfo.cs             -> contains Title 
// ** 2) shared AssemblyInfo.Shared.cs       -> contains Product, Company, Copyright, Version, Culture, ComVisible & Configuration 

using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyProduct("Hey, I'm in")]
[assembly: AssemblyCompany("Atos AG")]
[assembly: AssemblyCopyright("Copyright © Atos AG 2018. All rights reserved.")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]

[assembly: AssemblyVersion("1.1.0")]
[assembly: AssemblyFileVersion("1.1.0")]
[assembly: AssemblyInformationalVersion("1.1.0")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif
