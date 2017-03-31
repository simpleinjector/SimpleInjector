#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Threading;
    using SimpleInjector.Internals;

#if NET40 || NET45
    internal sealed partial class PropertyInjectionHelper
    {
        private static long injectorClassCounter;

        partial void TryCompileLambdaInDynamicAssembly(LambdaExpression expression, ref Delegate compiledDelegate)
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
        private static Delegate CompileLambdaInDynamicAssemblyWithFallback(LambdaExpression expression)
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