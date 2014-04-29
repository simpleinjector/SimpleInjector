using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SimpleInjector.Extensions.ExecutionContextScoping")]
[assembly: AssemblyDescription("Execution Context Scoping extensions for the Simple Injector")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Simple Injector")]
[assembly: AssemblyProduct("SimpleInjector.Extensions.ExecutionContextScoping")]
[assembly: AssemblyCopyright("Copyright © 2014 Simple Injector Contributors")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("0b9a2e17-f343-4216-bd8c-e2137e73b4f3")]

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
[assembly: AssemblyVersion("2.4.0.0")]
[assembly: AssemblyFileVersion("2.4.0.0")]

[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguageAttribute("en-US")]

[assembly: AllowPartiallyTrustedCallers]

[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes",
    Scope = "namespace",
    Target = "SimpleInjector.Extensions.ExecutionContextScoping",
    Justification = "Not much we can do about this.")]

#if PUBLISH
#pragma warning disable 1699
[assembly: AssemblyKeyFileAttribute("..\\SimpleInjector.snk")]
#pragma warning restore 1699
#endif
