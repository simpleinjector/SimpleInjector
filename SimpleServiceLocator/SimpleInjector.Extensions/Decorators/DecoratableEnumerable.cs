#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2010 S. van Deursen
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

namespace SimpleInjector.Extensions.Decorators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    // A decoratable enumerable is a collection that holds a set of Expression objects. When a decorator is
    // applied to a collection, a new DecoratableEnumerable will be created
    internal sealed class DecoratableEnumerable<TService> : IEnumerable<TService>, IDecoratableEnumerable
    {
        private readonly Container container;
        private readonly Type[] serviceTypes;

        private IEnumerable<DecoratorPredicateContext> contexts;
        private Func<TService>[] instanceCreators;

        // This constructor needs to be public. It is called using reflection.
        public DecoratableEnumerable(Container container, Type[] serviceTypes)
        {
            this.container = container;
            this.serviceTypes = serviceTypes;
        }

        // This constructor needs to be public. It is called using reflection.
        public DecoratableEnumerable(DecoratorPredicateContext[] contexts)
        {
            this.contexts = contexts;
        }

        // This constructor needs to be public. It is called using reflection.
        public DecoratableEnumerable(Container container, IEnumerable<Expression> expressions)
        {
            this.container = container;
            this.contexts = 
                DecoratorPredicateContext.CreateFromExpressions(container, typeof(TService), expressions);
        }

        public DecoratorPredicateContext[] GetDecoratorPredicateContexts()
        {
            this.BuildContexts();

            return this.contexts.ToArray();
        }

        public IEnumerator<TService> GetEnumerator()
        {
            if (this.instanceCreators == null)
            {
                this.BuildInstanceCreators();
            }

            return this.GetEnumeratorForCreators();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void BuildInstanceCreators()
        {
            this.BuildContexts();

            this.instanceCreators = (
                from context in this.contexts
                let lambda = Expression.Lambda<Func<TService>>(context.Expression)
                let instanceCreator = lambda.Compile()
                select instanceCreator)
                .ToArray();
        }

        private void BuildContexts()
        {
            if (this.contexts == null)
            {
                this.contexts = (
                    from implementationType in this.serviceTypes
                    let producer = this.GetRegistration(implementationType)
                    select DecoratorPredicateContext.CreateFromExpression(this.container, producer.ServiceType, 
                        implementationType, producer.BuildExpression()))
                    .ToArray();
            }
        }

        private IEnumerator<TService> GetEnumeratorForCreators()
        {
            for (int i = 0; i < this.instanceCreators.Length; i++)
            {
                yield return this.instanceCreators[i]();
            }
        }

        private InstanceProducer GetRegistration(Type serviceType)
        {
            var producer = this.container.GetRegistration(serviceType);

            if (producer == null)
            {
                // This will throw an exception, because there is no registration for the service type.
                // By calling GetInstance we reuse the descriptive exception messages of the container.
                this.container.GetInstance(serviceType);
            }

            return producer;
        }
    }
}