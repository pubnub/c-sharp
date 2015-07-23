// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("NUnitLite")]
[assembly: AssemblyDescription("NUnitLite unit-testing framework")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("NUnit Software")]
[assembly: AssemblyProduct("NUnitLite")]
[assembly: AssemblyCopyright("Copyright 2007, Charlie Poole")]
[assembly: AssemblyTrademark("NUnitLite")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("0be367fd-d825-4039-a70b-54a3557170ec")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("0.2.0.0")]
#if !PocketPC && !WindowsCE && !NETCF
[assembly: AssemblyFileVersion("0.1.0.0")]
#endif
