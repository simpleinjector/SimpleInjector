using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Simple Injector")]
[assembly: AssemblyDescription("Simple Injector is an easy-to-use Inversion of Control library for .NET.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Simple Injector")]
[assembly: AssemblyProduct("Simple Injector")]
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
[assembly: AssemblyVersion("3.2.0.0")]
[assembly: AssemblyFileVersion("3.2.0.0")]

[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguageAttribute("en-US")]

#if !PUBLISH
[assembly: InternalsVisibleTo("SimpleInjector.Diagnostics")]
[assembly: InternalsVisibleTo("SimpleInjector.Diagnostics.Tests.Unit")]
[assembly: InternalsVisibleTo("SimpleInjector.Extensions.LifetimeScoping.Tests.Unit")]
[assembly: InternalsVisibleTo("SimpleInjector.Tests.Unit")]
[assembly: InternalsVisibleTo("SimpleInjector.Silverlight.Tests.Unit")]
#else
[assembly: InternalsVisibleTo("SimpleInjector.Diagnostics, PublicKey=" + 
    "0024000004800000940000000602000000240000525341310004000001000100d9fe51dff71d3f" +
    "dcf9bfcccf7b4f589f530449a7414aec14b3d08abdde4229eea5a42f5636c738272c44e07dad2a" +
    "92a1186525779360997c6e0e6153d6d8b1d25c7fe9359b2d230530e7ccccd02a32269ce22e4f1a" +
    "a313cd995f6ee682c88b24acf8e6c9f6ddc95094eaeafe39e626b3765fd9b4f2e7789c3a6ed1c4" +
    "a66dedb9")]
#endif

[assembly: SecurityTransparent]

#if PUBLISH
#pragma warning disable 1699
[assembly: AssemblyKeyFileAttribute("..\\SimpleInjector.snk")]
#pragma warning restore 1699
#endif
