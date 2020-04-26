// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Threading;
    using SimpleInjector.Internals;

#if NET45
    internal sealed partial class PropertyInjectionHelper
    {
        private static long injectorClassCounter;

        partial void TryCompileLambdaInDynamicAssembly(LambdaExpression expression, ref Delegate? compiledDelegate)
        {
            compiledDelegate = null;

            if (this.container.Options.EnableDynamicAssemblyCompilation &&
                !CompilationHelpers.ExpressionNeedsAccessToInternals(expression))
            {
                compiledDelegate = CompileLambdaInDynamicAssemblyWithFallback(expression);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Not all delegates can be JITted. We fall back to the slower expression.Compile " +
                            "in that case.")]
        private static Delegate? CompileLambdaInDynamicAssemblyWithFallback(LambdaExpression expression)
        {
            try
            {
                var @delegate = CompilationHelpers.CompileLambdaInDynamicAssembly(expression,
                    "DynamicPropertyInjector" + GetNextInjectorClassId(),
                    "InjectProperties");

                // Test the creation. Since we're using a dynamically created assembly, we can't create every
                // delegate we can create using expression.Compile(), so we need to test this.
                CompilationHelpers.JitCompileDelegate(@delegate);

                return @delegate;
            }
            catch
            {
                return null;
            }
        }

        private static long GetNextInjectorClassId() => Interlocked.Increment(ref injectorClassCounter);
    }
#endif
}