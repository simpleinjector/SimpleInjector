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
    using SimpleInjector.Advanced;

    // A decoratable enumerable is a collection that holds a set of Expression objects. When a decorator is
    // applied to a collection, a new DecoratableEnumerable will be created
    internal sealed class ContainerControlledEnumerable<TService> : IndexableEnumerable<TService>, 
        IDecoratedEnumerable
    {
        private readonly Container container;
        private readonly Type[] serviceTypes;

        private DecoratorPredicateContext[] contexts;
        private Func<TService>[] instanceCreators;

        // This constructor needs to be public. It is called using reflection.
        public ContainerControlledEnumerable(Container container, Type[] serviceTypes)
        {
            this.container = container;
            this.serviceTypes = serviceTypes;
        }

        // This constructor needs to be public. It is called using reflection.
        public ContainerControlledEnumerable(Container container, IEnumerable<Expression> expressions)
            : this(
                container,
                DecoratorPredicateContext.CreateFromExpressions(container, typeof(TService), expressions))
        {
        }

        // This constructor needs to be public. It is called using reflection.
        public ContainerControlledEnumerable(Container container, DecoratorPredicateContext[] contexts)
        {
            this.container = container;
            this.contexts = contexts;
        }

        public override int Count
        {
            get { return this.serviceTypes != null ? this.serviceTypes.Length : this.contexts.Length; }
        }

        public override TService this[int index]
        {
            get
            {
                this.InitializeInstanceCreators();

                return this.instanceCreators[index]();
            }

            set
            {
                throw IndexableEnumerable<TService>.GetNotSupportedBecauseCollectionIsReadOnlyException();
            }
        }

        public DecoratorPredicateContext[] GetDecoratorPredicateContexts()
        {
            this.BuildContexts();

            return this.contexts.ToArray();
        }

        public override IEnumerator<TService> GetEnumerator()
        {
            this.InitializeInstanceCreators();

            return this.GetEnumeratorForCreators();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void InitializeInstanceCreators()
        {
            if (this.instanceCreators == null)
            {
                this.instanceCreators = this.BuildInstanceCreators();
            }
        }

        private Func<TService>[] BuildInstanceCreators()
        {
            this.BuildContexts();

            return (
                from context in this.contexts
                select this.BuildInstanceCreator(context.Expression))
                .ToArray();
        }

        private Func<TService> BuildInstanceCreator(Expression expression)
        {
            Func<object> instanceCreator = null;

            return () =>
            {
                if (instanceCreator == null)
                {
                    object instance;

                    instanceCreator = Helpers.CompileAndRun(this.container, expression, out instance);

                    if (instance != null)
                    {
                        return (TService)instance;
                    }
                }

                return (TService)instanceCreator();
            };
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
            foreach (var instanceCreator in this.instanceCreators)
            {
                yield return instanceCreator();
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