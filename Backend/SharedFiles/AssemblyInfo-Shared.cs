﻿// documentation:
// * there are 2 assembly files for each .NET project:
// ** 1) private AssemblyInfo.cs             -> contains Title 
// ** 2) shared AssemblyInfo.Shared.cs       -> contains Product, Company, Copyright, Version, Culture, ComVisible & Configuration 

using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyProduct("Hey I'm in")]
[assembly: AssemblyCompany("Atos AG")]
[assembly: AssemblyCopyright("Copyright © Atos AG 2018. All rights reserved.")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]

[assembly: AssemblyVersion("0.0.1")]
[assembly: AssemblyFileVersion("0.0.1")]
[assembly: AssemblyInformationalVersion("0.0.1")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif