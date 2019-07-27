// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;
    using SimpleInjector.Internals;

#if !PUBLISH && (NET40 || NET45)
    /// <summary>Common Container methods specific for the full .NET version of Simple Injector.</summary>
#endif
#if NET40 || NET45
    public partial class Container
    {
        private static readonly LazyEx<ModuleBuilder> LazyBuilder = new LazyEx<ModuleBuilder>(() =>
            AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("SimpleInjector.Compiled"),
                AssemblyBuilderAccess.Run)
                .DefineDynamicModule("SimpleInjector.CompiledModule"));

        internal static ModuleBuilder ModuleBuilder => LazyBuilder.Value;
    }
#endif
}