// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.ProducerBuilders
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Builds InstanceProducers for concrete unregistered types.
    /// </summary>
    internal sealed class UnregisteredConcreteTypeInstanceProducerBuilder : IInstanceProducerBuilder
    {
        private readonly Dictionary<Type, InstanceProducer> unregisteredConcreteTypeInstanceProducers =
            new Dictionary<Type, InstanceProducer>();

        private readonly Container container;

        public UnregisteredConcreteTypeInstanceProducerBuilder(Container container)
        {
            this.container = container;
        }

        public InstanceProducer? TryBuild(Type serviceType)
        {
            if (!this.container.Options.ResolveUnregisteredConcreteTypes
                || serviceType.IsAbstract()
                || serviceType.IsValueType()
                || serviceType.ContainsGenericParameters()
                || !this.container.IsConcreteConstructableType(serviceType))
            {
                return null;
            }

            InstanceProducer BuildInstanceProducer()
            {
                var registration =
                    this.container.SelectionBasedLifestyle.CreateRegistration(serviceType, this.container);

                return BuildInstanceProducerForConcreteUnregisteredType(serviceType, registration);
            }

            return this.GetOrBuildInstanceProducerForConcreteUnregisteredType(serviceType, BuildInstanceProducer);
        }

        private static InstanceProducer BuildInstanceProducerForConcreteUnregisteredType(
            Type concreteType, Registration registration)
        {
            var producer = InstanceProducer.Create(concreteType, registration);

            producer.EnsureTypeWillBeExplicitlyVerified();

            // Flag that this producer is resolved by the container or using unregistered type resolution.
            producer.IsContainerAutoRegistered = true;

            return producer;
        }

        private InstanceProducer GetOrBuildInstanceProducerForConcreteUnregisteredType(
            Type concreteType, Func<InstanceProducer> instanceProducerBuilder)
        {
            // We need to take a lock here to make sure that we never create multiple InstanceProducer
            // instances for the same concrete type, which is a problem when the LifestyleSelectionBehavior
            // has been overridden. For instance in case the overridden behavior returns a Singleton lifestyle,
            // but the concrete type is requested concurrently over multiple threads, not taking the lock
            // could cause two InstanceProducers to be created, which might cause two instances from being
            // created.
            lock (this.unregisteredConcreteTypeInstanceProducers)
            {
                if (!this.unregisteredConcreteTypeInstanceProducers
                    .TryGetValue(concreteType, out InstanceProducer producer))
                {
                    producer = instanceProducerBuilder.Invoke();

                    this.unregisteredConcreteTypeInstanceProducers[concreteType] = producer;
                }

                return producer;
            }
        }
    }
}