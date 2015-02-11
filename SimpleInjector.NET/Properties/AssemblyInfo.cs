using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("28b6edc8-9b68-4053-85b4-dbaa25b07432")]

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
[assembly: AssemblyVersion("2.7.2.0")]
[assembly: AssemblyFileVersion("2.7.2.0")]

[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguageAttribute("en-US")]

[assembly: AllowPartiallyTrustedCallers]

#if !PUBLISH
[assembly: InternalsVisibleTo("SimpleInjector.Diagnostics")]
[assembly: InternalsVisibleTo("SimpleInjector.Diagnostics.Tests.Unit")]
[assembly: InternalsVisibleTo("SimpleInjector.Extensions.LifetimeScoping.Tests.Unit")]
[assembly: InternalsVisibleTo("SimpleInjector.Tests.Unit")]
#else
[assembly: InternalsVisibleTo("SimpleInjector.Diagnostics, PublicKey=" + 
    "0024000004800000940000000602000000240000525341310004000001000100d9fe51dff71d3f" +
    "dcf9bfcccf7b4f589f530449a7414aec14b3d08abdde4229eea5a42f5636c738272c44e07dad2a" +
    "92a1186525779360997c6e0e6153d6d8b1d25c7fe9359b2d230530e7ccccd02a32269ce22e4f1a" +
    "a313cd995f6ee682c88b24acf8e6c9f6ddc95094eaeafe39e626b3765fd9b4f2e7789c3a6ed1c4" +
    "a66dedb9")]
#endif

#if PUBLISH
#pragma warning disable 1699
[assembly: AssemblyKeyFileAttribute("..\\SimpleInjector.snk")]
#pragma warning restore 1699
#endif

[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace",
    Target = "SimpleInjector",
    Justification = "Can't make up new types just to satisfy this rule.")]

[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace",
    Target = "SimpleInjector.Advanced",
    Justification = "Can't make up new types just to satisfy this rule.")]
