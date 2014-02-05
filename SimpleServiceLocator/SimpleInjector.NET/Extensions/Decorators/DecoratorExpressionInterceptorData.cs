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

namespace SimpleInjector.Extensions.Decorators
{
    using System;

    internal sealed class DecoratorExpressionInterceptorData
    {
        public DecoratorExpressionInterceptorData(Container container, Type serviceType, Type decoratorType,
            Predicate<DecoratorPredicateContext> predicate, Lifestyle lifestyle,
            Func<DecoratorPredicateContext, Type> decoratorTypeFactory = null)
        {
            this.Container = container;
            this.ServiceType = serviceType;
            this.DecoratorType = decoratorType;
            this.DecoratorTypeFactory = this.WrapInNullProtector(decoratorTypeFactory);
            this.Predicate = predicate;
            this.Lifestyle = lifestyle;
        }

        internal Container Container { get; private set; }

        internal Type ServiceType { get; private set; }

        internal Type DecoratorType { get; private set; }

        internal Func<DecoratorPredicateContext, Type> DecoratorTypeFactory { get; private set; }

        internal Predicate<DecoratorPredicateContext> Predicate { get; private set; }

        internal Lifestyle Lifestyle { get; private set; }

        private Func<DecoratorPredicateContext, Type> WrapInNullProtector(
            Func<DecoratorPredicateContext, Type> decoratorTypeFactory)
        {
            if (decoratorTypeFactory == null)
            {
                return null;
            }

            return context =>
            {
                Type type = decoratorTypeFactory(context);

                if (type == null)
                {
                    throw new InvalidOperationException(
                        StringResources.DecoratorFactoryReturnedNull(this.ServiceType));
                }

                return type;
            };
        }
    }
}