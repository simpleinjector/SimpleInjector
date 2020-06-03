// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using SimpleInjector.DynamicAssemblyCompilation;

namespace SimpleInjector
{
    /// <summary>
    /// Extension methods for dynamic assembly compilation.
    /// </summary>
    public static class DynamicAssemblyCompilationExtensions
    {
        /// <summary>Enables dynamic assembly compilation.</summary>
        /// <param name="options">The container options.</param>
        public static void EnableDynamicAssemblyCompilation(this ContainerOptions options)
        {
            options.ExpressionCompilationBehavior = new DynamicAssemblyExpressionCompilationBehavior();
        }
    }
}