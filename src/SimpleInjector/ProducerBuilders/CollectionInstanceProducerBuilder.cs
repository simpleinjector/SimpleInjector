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
    /// Builds InstanceProducers for all collections types except <see cref="IEnumerable{T}"/>.
    /// </summary>
    internal sealed class CollectionInstanceProducerBuilder : IInstanceProducerBuilder
    {
        private static readonly MethodInfo EnumerableToArrayMethod =
            typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray));

        private static readonly MethodInfo EnumerableToListMethod =
            typeof(Enumerable).GetMethod(nameof(Enumerable.ToList));

        private readonly Dictionary<Type, InstanceProducer?> emptyAndRedirectedCollectionRegistrationCache =
            new Dictionary<Type, InstanceProducer?>();

        private readonly Container container;

        public CollectionInstanceProducerBuilder(Container container)
        {
            this.container = container;
        }

        public InstanceProducer? TryBuild(Type serviceType) =>
            this.TryBuildInstanceProducerForMutableCollection(serviceType) ??
            this.TryBuildInstanceProducerForStream(serviceType);

        private InstanceProducer? TryBuildInstanceProducerForMutableCollection(Type serviceType)
        {
            if (serviceType.IsArray)
            {
                return this.BuildInstanceProducerForMutableCollectionType(
                    serviceType,
                    serviceType.GetElementType());
            }
            else if (typeof(List<>).IsGenericTypeDefinitionOf(serviceType))
            {
                return this.BuildInstanceProducerForMutableCollectionType(
                    serviceType,
                    serviceType.GetGenericArguments().FirstOrDefault());
            }
            else
            {
                return null;
            }
        }

        private InstanceProducer? BuildInstanceProducerForMutableCollectionType(
            Type serviceType, Type elementType)
        {
            // We don't auto-register collections for ambiguous types.
            if (Types.IsAmbiguousOrValueType(elementType))
            {
                return null;
            }

            // GetAllInstances locks the container
            if (this.container.GetAllInstances(elementType) is IContainerControlledCollection)
            {
                return this.BuildMutableCollectionProducerFromControlledCollection(serviceType, elementType);
            }
            else
            {
                return this.BuildMutableCollectionProducerFromUncontrolledCollection(serviceType, elementType);
            }
        }

        private InstanceProducer BuildMutableCollectionProducerFromControlledCollection(
            Type serviceType, Type elementType)
        {
            Expression expression =
                this.BuildMutableCollectionExpressionFromControlledCollection(serviceType, elementType);

            // Technically, we could determine the longest lifestyle out of the elements of the collection,
            // instead of using Transient here. This would make it less likely for the user to get false
            // positive Lifestyle Mismatch warnings. Problem with that is that trying to retrieve the
            // longest lifestyle might cause the array to be cached in a way that is incorrect, because
            // who knows what kind of lifestyles the used created.
            Registration registration =
                new ExpressionRegistration(expression, serviceType, Lifestyle.Transient, this.container);

            var producer = new InstanceProducer(serviceType, registration);
            producer.IsContainerAutoRegistered = !this.container.GetAllInstances(elementType).Any();
            return producer;
        }

        private Expression BuildMutableCollectionExpressionFromControlledCollection(
            Type serviceType, Type elementType)
        {
            var streamExpression = Expression.Constant(
                value: this.container.GetAllInstances(elementType),
                type: typeof(IEnumerable<>).MakeGenericType(elementType));

            if (serviceType.IsArray)
            {
                // builds: Enumerable.ToArray(collection)
                return Expression.Call(
                    EnumerableToArrayMethod.MakeGenericMethod(elementType),
                    streamExpression);
            }
            else
            {
                // builds: new List<T>(collection)
                var listConstructor = typeof(List<>).MakeGenericType(elementType)
                    .GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(elementType) });

                return Expression.New(listConstructor, streamExpression);
            }
        }

        private InstanceProducer BuildMutableCollectionProducerFromUncontrolledCollection(
            Type serviceType, Type elementType)
        {
            InstanceProducer? enumerableProducer = this.container.GetRegistration(
                typeof(IEnumerable<>).MakeGenericType(elementType), throwOnFailure: true);
            Expression enumerableExpression = enumerableProducer!.BuildExpression();

            var expression = Expression.Call(
                method: serviceType.IsArray
                    ? EnumerableToArrayMethod.MakeGenericMethod(elementType)
                    : EnumerableToListMethod.MakeGenericMethod(elementType),
                arg0: enumerableExpression);

            Registration registration =
                new ExpressionRegistration(expression, serviceType, Lifestyle.Transient, this.container);

            var producer = new InstanceProducer(serviceType, registration);
            producer.IsContainerAutoRegistered = true;
            return producer;
        }

        private InstanceProducer? TryBuildInstanceProducerForStream(Type serviceType)
        {
            if (!Types.IsGenericCollectionType(serviceType))
            {
                return null;
            }

            // We don't auto-register collections for ambiguous types.
            if (Types.IsAmbiguousOrValueType(serviceType.GetGenericArguments()[0]))
            {
                return null;
            }

            lock (this.emptyAndRedirectedCollectionRegistrationCache)
            {
                // We need to cache these generated producers, to prevent getting duplicate producers; which
                // will cause (incorrect) diagnostic warnings.
                if (!this.emptyAndRedirectedCollectionRegistrationCache.TryGetValue(
                    serviceType, out InstanceProducer? producer))
                {
                    // This call might lock the container
                    producer = this.TryBuildStreamInstanceProducer(serviceType);

                    this.emptyAndRedirectedCollectionRegistrationCache[serviceType] = producer;
                }

                return producer;
            }
        }

        private InstanceProducer? TryBuildStreamInstanceProducer(Type collectionType)
        {
            if (typeof(IEnumerable<>).IsGenericTypeDefinitionOf(collectionType))
            {
                // Building up enumerables is done inside TryGetInstanceProducerForRegisteredCollection.
                return null;
            }

            Type elementType = collectionType.GetGenericArguments()[0];

            if (!(this.container.GetAllInstances(elementType) is IContainerControlledCollection stream))
            {
                return null;
            }

            Registration registration = stream.CreateRegistration(collectionType, this.container);

            var producer = new InstanceProducer(collectionType, registration);
            producer.IsContainerAutoRegistered = stream.Count == 0;
            return producer;
        }
    }
}