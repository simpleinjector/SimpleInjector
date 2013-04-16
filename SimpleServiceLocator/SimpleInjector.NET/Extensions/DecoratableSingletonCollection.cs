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
        IDecoratableSingletonCollection
    {
        private readonly Lazy<InstanceProducer[]> instanceProducers;

        internal DecoratableSingletonCollection(Container container, TService[] instances)
        {
            // Ensure that for every instance only one InstanceProducer is created (to prevent double
            // initialization and creation of multiple singleton decorators).
            this.instanceProducers = new Lazy<InstanceProducer[]>(
                () => CreateSingletonInstanceProducers(container, instances));
        }

        Expression[] IDecoratableSingletonCollection.BuildExpressions()
        {
            return (
                from producer in this.instanceProducers.Value
                select producer.BuildExpression())
                .ToArray();
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
    }
}