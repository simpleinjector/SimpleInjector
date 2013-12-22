#region Copyright (c) 2013 Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
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

namespace SimpleInjector
{
    using System;
    using System.Linq.Expressions;

    internal static partial class CompilationHelpers
    {
        // Compile the expression. If the expression is compiled in a dynamic assembly, the compiled delegate
        // is called (to ensure that it will run, because it tends to fail now and then) and the created
        // instance is returned through the out parameter. Note that NO created instance will be returned when
        // the expression is compiled using Expression.Compile)(.
        internal static Func<object> CompileAndRun(Container container, Expression expression,
            out object createdInstance)
        {
            createdInstance = null;

            var constantExpression = expression as ConstantExpression;

            // Skip compiling if all we need to do is return a singleton.
            if (constantExpression != null)
            {
                return CreateConstantOptimizedExpression(constantExpression);
            }

            Func<object> compiledLambda = null;

            TryCompileAndExecuteInDynamicAssembly(container, expression, ref compiledLambda,
                ref createdInstance);

            return compiledLambda ?? CompileLambda(expression);
        }

        static partial void TryCompileAndExecuteInDynamicAssembly(Container container, Expression expression, 
            ref Func<object> compiledLambda, ref object createdInstance);
        
        private static Func<object> CreateConstantOptimizedExpression(ConstantExpression expression)
        {
            object singleton = expression.Value;

            // This lambda will be a tiny little bit faster than a compiled delegate.
            return () => singleton;
        }

        private static Func<object> CompileLambda(Expression expression)
        {
            return Expression.Lambda<Func<object>>(expression).Compile();
        }
    }
}