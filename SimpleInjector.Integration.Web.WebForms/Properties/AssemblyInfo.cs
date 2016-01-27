using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SimpleInjector.Integration.Web.Forms")]
[assembly: AssemblyDescription("Integration library for ASP.NET Web Forms for the Simple Injector Inversion of Control library.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Simple Injector")]
[assembly: AssemblyProduct("SimpleInjector.Integration.Web.Forms")]
[assembly: AssemblyCopyright("Copyright © 2013 Simple Injector Contributors")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a3f31ca2-f315-4057-a06f-f088bf4704e5")]

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
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguageAttribute("en-US")]

[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", 
    Scope = "namespace", 
    Target = "SimpleInjector.Integration.Web.Forms",
    Justification = "We can't merge namespaces, because this basically is the only namespace in the assembly.")]

#if PUBLISH
#pragma warning disable 1699
[assembly: AssemblyKeyFileAttribute("..\\SimpleInjector.snk")]
#pragma warning restore 1699
#endif
