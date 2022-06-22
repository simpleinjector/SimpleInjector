// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.ProducerBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Internals;
    using SimpleInjector.Lifestyles;

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

            return new InstanceProducer(
                metadataType,
                this.CreateMetadataRegistration(metadataType, instanceProducer!));
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
                select ContainerControlledItem.CreateFromRegistration(
                    this.CreateMetadataRegistration(metadataType, producer)));

            return new InstanceProducer(
                serviceType: enumerableOfProducersType,
                registration: metadataCollection.CreateRegistration(enumerableOfProducersType, this.container));
        }

        private Registration CreateMetadataRegistration(Type metadataType, InstanceProducer producer)
        {
            if (this.ShouldFlow(producer))
            {
                var registration = (ScopedRegistration)Lifestyle.Scoped.CreateRegistration(
                    serviceType: metadataType,
                    instanceCreator: this.BuildMetadataInstanceCreator(metadataType, producer, flowing: true),
                    container: this.container);

                registration.AdditionalInformationForLifestyleMismatchDiagnosticsProvider =
                    () => StringResources.FlowingMetadataIsScopedBecause(metadataType);

                return registration;
            }
            else
            {
                return Lifestyle.Singleton.CreateRegistration(
                    serviceType: metadataType,
                    instanceCreator: this.BuildMetadataInstanceCreator(metadataType, producer, flowing: false),
                    container: this.container);
            }
        }

        // Only create a scoped registration when we're in flowing mode and the graph contains scoped
        // components, because this makes it less likely to hit a Lifestyle Mismatch, as users typically
        // expect metadata to be singletons. Downside of this approach is that the instance producer needs
        // to be built here.
        private bool ShouldFlow(InstanceProducer producer) =>
            ScopedLifestyle.Flowing == this.container.Options.DefaultScopedLifestyle
                && producer.ContainsScopedComponentsInGraph();

        private Func<object> BuildMetadataInstanceCreator(
            Type metadataType, InstanceProducer producer, bool flowing)
        {
            ConstructorInfo ctor = metadataType.GetConstructors(includeNonPublic: true)[0];

            Expression scopeExpression = flowing
                ? this.container.GetRegistration(typeof(Scope), true)!.BuildExpression()
                : Expression.Constant(null, typeof(Scope));

            Expression newMetadataExpression =
                Expression.New(ctor, scopeExpression, Expression.Constant(producer));

            return (Func<object>)CompilationHelpers.CompileExpression(this.container, newMetadataExpression);
        }
    }
}