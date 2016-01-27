using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SimpleInjector.Extensions.ExecutionContextScoping")]
[assembly: AssemblyDescription("Execution Context Scoping extensions for the Simple Injector")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Simple Injector")]
[assembly: AssemblyProduct("SimpleInjector.Extensions.ExecutionContextScoping")]
[assembly: AssemblyCopyright("Copyright © 2013 - 2015 Simple Injector Contributors")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("af35c30c-2dc6-4a8e-9e63-ef6fba65b94e")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("3.0.1.0")]
[assembly: AssemblyFileVersion("3.0.1.0")]

[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguageAttribute("en-US")]

#if PUBLISH
#pragma warning disable 1699
[assembly: AssemblyKeyFileAttribute("..\\SimpleInjector.snk")]
#pragma warning restore 1699
#endif
