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
    internal sealed class DependencyMetadataInstanceProducerBuilder : IInstanceProducerBuilder
    {
        private readonly Container container;

        public DependencyMetadataInstanceProducerBuilder(Container container)
        {
            this.container = container;
        }

        public InstanceProducer? TryBuild(Type serviceType)
        {
            // Check for InstanceProducer<T>, but prevent sub types of InstanceProducer<T>.
            if (typeof(DependencyMetadata<>).IsGenericTypeDefinitionOf(serviceType))
            {
                return this.BuildInstanceProducerForDependencyMetadata(serviceType);
            }
            else if (typeof(IEnumerable<>).IsGenericTypeDefinitionOf(serviceType)
                && typeof(DependencyMetadata<>).IsGenericTypeDefinitionOf(serviceType.GetGenericArguments()[0]))
            {
                return this.BuildInstanceProducerForMetadataList(serviceType);
            }
            else
            {
                return null;
            }
        }

        private InstanceProducer BuildInstanceProducerForDependencyMetadata(Type metadataType)
        {
            Type serviceType = metadataType.GetGenericArguments()[0];

            InstanceProducer? instanceProducer =
                this.container.GetInstanceProducerForType(serviceType, InjectionConsumerInfo.Root);

            if (instanceProducer is null)
            {
                this.container.ThrowMissingInstanceProducerException(serviceType);
            }

            object metadata = CreateMetadata(metadataType, instanceProducer!);

            return new InstanceProducer(
                metadataType,
                Lifestyle.Singleton.CreateRegistration(metadataType, metadata, this.container));
        }

        private InstanceProducer BuildInstanceProducerForMetadataList(Type enumerableOfProducersType)
        {
            Type metadataType = enumerableOfProducersType.GetGenericArguments()[0];
            Type serviceType = metadataType.GetGenericArguments()[0];

            var collection = this.container.GetAllInstances(serviceType) as IContainerControlledCollection;

            if (collection is null)
            {
                // This exception might not be expressive enough. If GetAllInstances succeeds, but the
                // returned type is not an IContainerControlledCollection, it likely means the collection is
                // container uncontrolled.
                this.container.ThrowMissingInstanceProducerException(serviceType);
            }

            IContainerControlledCollection metadataCollection =
                ControlledCollectionHelper.CreateContainerControlledCollection(metadataType, this.container);

            metadataCollection.AppendAll(
                from producer in collection!.GetProducers()
                let metadata = CreateMetadata(metadataType, producer)
                let reg = Lifestyle.Singleton.CreateRegistration(metadataType, metadata, this.container)
                select ContainerControlledItem.CreateFromRegistration(reg));

            return new InstanceProducer(
                serviceType: enumerableOfProducersType,
                registration: metadataCollection.CreateRegistration(enumerableOfProducersType, this.container));
        }

        private static object CreateMetadata(Type metadataType, InstanceProducer instanceProducer)
        {
            var ctor = metadataType.GetConstructors(includeNonPublic: true).Single();
            return ctor.Invoke(new object[] { instanceProducer });
        }
    }
}