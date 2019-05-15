// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

// This attribute will ensure that ASP.NET will call the SimpleInjectorWebInitializer.Initialize method on
// AppDomain startup.
[assembly: System.Web.PreApplicationStartMethod(
    typeof(SimpleInjector.Integration.Web.SimpleInjectorWebInitializer),
    nameof(SimpleInjector.Integration.Web.SimpleInjectorWebInitializer.Initialize))]

namespace SimpleInjector.Integration.Web
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    /// <summary>
    /// Pre application start code.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SimpleInjectorWebInitializer
    {
        private static bool hasStarted;

        /// <summary>Registers an HttpModule that allows disposing instances that are registered as
        /// Per Web Request.</summary>
        [ExcludeFromCodeCoverage]
        public static void Initialize()
        {
            if (!hasStarted)
            {
                hasStarted = true;

                DynamicModuleUtility.RegisterModule(typeof(SimpleInjectorHttpModule));
            }
        }
    }
}