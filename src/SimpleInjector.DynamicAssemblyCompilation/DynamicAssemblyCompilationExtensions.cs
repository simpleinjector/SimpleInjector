// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using SimpleInjector.Advanced;
using SimpleInjector.DynamicAssemblyCompilation;

namespace SimpleInjector
{
    /// <summary>
    /// Extension methods for dynamic assembly compilation.
    /// </summary>
    public static class DynamicAssemblyCompilationExtensions
    {
        private static readonly IExpressionCompilationBehavior Behavior =
            new DynamicAssemblyExpressionCompilationBehavior();

        /// <summary>Enables dynamic assembly compilation.</summary>
        /// <param name="options">The container options.</param>
        public static void EnableDynamicAssemblyCompilation(this ContainerOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.ExpressionCompilationBehavior = Behavior;
        }
    }
}