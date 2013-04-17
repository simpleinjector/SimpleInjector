namespace SimpleInjector.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using SimpleInjector.Extensions.Decorators;
    using SimpleInjector.Lifestyles;

    internal sealed class DecoratableSingletonCollection<TService> : IEnumerable<TService>,
        IDecoratableEnumerable
    {
        private readonly Container container;
        private readonly Lazy<InstanceProducer[]> instanceProducers;
        private readonly Lazy<DecoratorPredicateContext[]> decoratorPredicateContexts;

        internal DecoratableSingletonCollection(Container container, TService[] instances)
        {
            this.container = container;

            // Ensure that for every instance only one InstanceProducer is created (to prevent double
            // initialization and creation of multiple singleton decorators).
            this.instanceProducers = new Lazy<InstanceProducer[]>(
                () => CreateSingletonInstanceProducers(container, instances));

            this.decoratorPredicateContexts = new Lazy<DecoratorPredicateContext[]>(
                this.CreateDecoratorPredicateContexts);
        }

        public DecoratorPredicateContext[] GetDecoratorPredicateContexts()
        {
            return this.decoratorPredicateContexts.Value;
        }

        public IEnumerator<TService> GetEnumerator()
        {
            foreach (var item in this.instanceProducers.Value)
            {
                yield return (TService)item.GetInstance();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private static InstanceProducer[] CreateSingletonInstanceProducers(Container container,
            TService[] instances)
        {
            return (
                from instance in instances
                let type = instance.GetType()
                let registration = SingletonLifestyle.CreateSingleRegistration(type, instance, container)
                let producer = new InstanceProducer(type, registration)
                select producer)
                .ToArray();
        }

        private DecoratorPredicateContext[] CreateDecoratorPredicateContexts()
        {
            return (
                from instanceProducer in this.instanceProducers.Value
                select DecoratorPredicateContext.CreateFromProducer(this.container, typeof(TService),
                    instanceProducer))
                .ToArray();
        }
    }
}