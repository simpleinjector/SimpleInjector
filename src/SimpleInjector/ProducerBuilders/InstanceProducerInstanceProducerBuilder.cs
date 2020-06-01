// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.ProducerBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector.Internals;

    /// <summary>
    /// Builds InstanceProducers that can inject and resolve <see cref="InstanceProducer{TService}"/>
    /// instances as singletons.
    /// </summary>
    internal sealed class InstanceProducerInstanceProducerBuilder : IInstanceProducerBuilder
    {
        private readonly Container container;

        public InstanceProducerInstanceProducerBuilder(Container container)
        {
            this.container = container;
        }

        public InstanceProducer? TryBuild(Type serviceType)
        {
            // Check for InstanceProducer<T>, but prevent sub types of InstanceProducer<T>.
            if (typeof(InstanceProducer<>).IsGenericTypeDefinitionOf(serviceType))
            {
                return this.BuildInstanceProducerForInstanceProducer(serviceType);
            }
            else if (typeof(IEnumerable<>).IsGenericTypeDefinitionOf(serviceType)
                && typeof(InstanceProducer<>).IsGenericTypeDefinitionOf(serviceType.GetGenericArguments()[0]))
            {
                return this.BuildInstanceProducerForEnumerableOfInstanceProducers(serviceType);
            }
            else
            {
                return null;
            }
        }

        private InstanceProducer BuildInstanceProducerForInstanceProducer(Type producerType)
        {
            Type serviceType = producerType.GetGenericArguments()[0];

            var instanceProducer =
                this.container.GetInstanceProducerForType(serviceType, InjectionConsumerInfo.Root);

            if (instanceProducer is null)
            {
                this.container.ThrowMissingInstanceProducerException(serviceType);
            }

            return InstanceProducer.Create(
                producerType,
                Lifestyle.Singleton.CreateRegistration(producerType, instanceProducer!, this.container));
        }

        private InstanceProducer BuildInstanceProducerForEnumerableOfInstanceProducers(
            Type enumerableOfProducersType)
        {
            Type producerType = enumerableOfProducersType.GetGenericArguments()[0];
            Type serviceType = producerType.GetGenericArguments()[0];

            var collection = this.container.GetAllInstances(serviceType) as IContainerControlledCollection;

            if (collection is null)
            {
                // This exception might not be expressive enough. If GetAllInstances succeeds, but the
                // returned type is not an IContainerControlledCollection, it likely means the collection is
                // container uncontrolled.
                this.container.ThrowMissingInstanceProducerException(serviceType);
            }

            IContainerControlledCollection producerCollection =
                ControlledCollectionHelper.CreateContainerControlledCollection(producerType, this.container);

            producerCollection.AppendAll(
                from producer in collection!.GetProducers()
                let reg = Lifestyle.Singleton.CreateRegistration(producerType, producer, this.container)
                select ContainerControlledItem.CreateFromRegistration(reg));

            return InstanceProducer.Create(
                serviceType: enumerableOfProducersType,
                registration: producerCollection.CreateRegistration(enumerableOfProducersType, this.container));
        }
    }
}