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

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    internal sealed class ExpressionRegistration : Registration
    {
        private readonly Expression expression;
        private Type implementationType;

        internal ExpressionRegistration(Expression expression, Container container)
            : this(expression, GetImplementationTypeFor(expression), GetLifestyleFor(expression), container)
        {
        }

        internal ExpressionRegistration(Expression expression, Type implementationType, Lifestyle lifestyle, 
            Container container)
            : base(lifestyle, container)
        {
            this.expression = expression;
            this.implementationType = implementationType;
        }

        public override Type ImplementationType
        {
            get { return this.implementationType; }
        }

        public override Expression BuildExpression()
        {
            return this.expression;
        }

        private static Lifestyle GetLifestyleFor(Expression expression)
        {
            if (expression is ConstantExpression)
            {
                return Lifestyle.Singleton;
            } 
            
            if (expression is NewExpression)
            {
                return Lifestyle.Transient;
            }

            return Lifestyle.Unknown;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
            Justification = "I don't care. This is not a performance critical path.")]
        private static Type GetImplementationTypeFor(Expression expression)
        {
            if (expression is ConstantExpression)
            {
                return ((ConstantExpression)expression).Type;
            }

            if (expression is NewExpression)
            {
                return ((NewExpression)expression).Constructor.DeclaringType;
            }

            return null;
        }
    }
}