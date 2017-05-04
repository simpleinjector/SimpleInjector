using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;

[assembly: AssemblyTitle("SimpleInjector.Packaging")]
[assembly: AssemblyDescription("Allow packaging a set of services together for registration.")]
[assembly: AssemblyCopyright("Copyright © Simple Injector Contributors")]
[assembly: AssemblyCompany("Simple Injector")]
[assembly: AssemblyProduct("SimpleInjector.Packaging")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguage("en-US")]

// These attributes needed to be removed because they were removed from the core library. They can be added again,
// after they are added again to the core library.
// [assembly: AllowPartiallyTrustedCallers]
// [assembly: SecurityTransparent]
