using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SimpleInjector.Diagnostics")]
[assembly: AssemblyDescription("Diagnostics for Simple Injector")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Simple Injector")]
[assembly: AssemblyProduct("SimpleInjector.Diagnostics")]
[assembly: AssemblyCopyright("Copyright © 2013 Simple Injector Contributors")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

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
[assembly: AssemblyVersion("2.6.0.0")]
[assembly: AssemblyFileVersion("2.6.0.0")]

[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguageAttribute("en-US")]

// NOTE: This attribute is not available in a Portable Class Library
// [assembly: AllowPartiallyTrustedCallers]
#if !PUBLISH
[assembly: InternalsVisibleTo("SimpleInjector.Diagnostics.Tests.Unit")]
#endif

[assembly: SecurityTransparent]

// During a publish build (using the build.bat) we need to compile this assembly with a strong name key, since 
// we otherwise will not be able to access SimpleInjector.dll's internals. Although we could have done this
// using delayed signing, it is considerably easier to do it this way.
// Please note that the SimpleInjector.snk is private and is not in source control.
#if PUBLISH
#pragma warning disable 1699
[assembly: AssemblyKeyFileAttribute("..\\SimpleInjector.snk")]
#pragma warning restore 1699
#endif
