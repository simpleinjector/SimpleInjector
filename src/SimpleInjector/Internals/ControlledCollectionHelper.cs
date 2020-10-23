// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using SimpleInjector.Advanced;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Lifestyles;

    // This class allows an ContainerControlledCollection<T> to notify about the creation of its wrapped items.
    // This is solely used for diagnostic verification.
    internal static class ControlledCollectionHelper
    {
        private static readonly object ServiceCreatedListenersLocker = new object();

        // The boolean flag is an optimization, to prevent slowing down resolving of items inside a collection
        // too much.
        private static List<Action<ServiceCreatedListenerArgs>>? serviceCreatedListeners;

        internal static bool ContainsServiceCreatedListeners { get; private set; }

        internal static void AddServiceCreatedListener(Action<ServiceCreatedListenerArgs> serviceCreated)
        {
            lock (ServiceCreatedListenersLocker)
            {
                var listeners = serviceCreatedListeners ??
                    (serviceCreatedListeners = new List<Action<ServiceCreatedListenerArgs>>());

                listeners.Add(serviceCreated);

                ContainsServiceCreatedListeners = true;
            }
        }

        internal static void RemoveServiceCreatedListener(Action<ServiceCreatedListenerArgs> serviceCreated)
        {
            lock (ServiceCreatedListenersLocker)
            {
                serviceCreatedListeners!.Remove(serviceCreated);

                if (serviceCreatedListeners.Count == 0)
                {
                    serviceCreatedListeners = null;

                    ContainsServiceCreatedListeners = false;
                }
            }
        }

        internal static void NotifyServiceCreatedListeners(InstanceProducer producer)
        {
            lock (ServiceCreatedListenersLocker)
            {
                if (serviceCreatedListeners != null)
                {
                    var args = new ServiceCreatedListenerArgs(producer);

                    // Iterate the list in reverse order, as the inner most listener should
                    // be able to act first.
                    foreach (var listener in Enumerable.Reverse(serviceCreatedListeners))
                    {
                        listener(args);
                    }
                }
            }
        }

        internal static IContainerControlledCollection? ExtractContainerControlledCollectionFromRegistration(
            Registration registration)
        {
            var controlledRegistration = registration as ContainerControlledCollectionRegistration;

            // We can only determine the value when registration is created using the
            // CreateRegistrationForContainerControlledCollection method. When the registration is null the
            // collection might be registered as container-uncontrolled collection.
            return controlledRegistration?.Collection;
        }

        internal static IContainerControlledCollection CreateContainerControlledCollection(
            Type serviceType, Container container)
        {
            var collection = Activator.CreateInstance(
                typeof(ContainerControlledCollection<>).MakeGenericType(serviceType),
                new object[] { container });

            return (IContainerControlledCollection)collection;
        }

        internal static InstanceProducer CreateInstanceProducer<TService>(
            this ContainerControlledCollection<TService> collection, Container container)
        {
            var collectionType = typeof(IEnumerable<TService>);

            return new InstanceProducer(
                serviceType: collectionType,
                registration: collection.CreateRegistration(collectionType, container));
        }

        internal static Registration CreateRegistration(
            this IContainerControlledCollection instance, Type collectionType, Container container)
        {
            // We need special handling for Collection<T> (and ReadOnlyCollection<T>), because the
            // ContainerControlledCollection does not (and can't) inherit it. So we have to wrap that
            // stream into a Collection<T> or ReadOnlyCollection<T>.
            return
                TryCreateRegistrationForFlowingCollection(instance, collectionType, container)
                ?? TryCreateRegistrationForCollectionOfT(collectionType, instance, container)
                ?? new ContainerControlledCollectionRegistration(
                    Lifestyle.Singleton, collectionType, instance, container);
        }

        private static ScopedRegistration? TryCreateRegistrationForFlowingCollection(
            IContainerControlledCollection instance, Type collectionType, Container container)
        {
            // Only create a scoped collection when we're in flowing mode and the graph contains scoped
            // components, because this makes it less likely to hit a Lifestyle Mismatch, as users typically
            // expect collections to be singletons. Downside of this approach is that all wrapped instance
            // producers need to be built here.
            if (ScopedLifestyle.Flowing != container.Options.DefaultScopedLifestyle
                || !ContainsScopedComponents(instance))
            {
                return null;
            }

            Type elementType = collectionType.GetGenericArguments()[0];

            var ctor = typeof(FlowingContainerControlledCollection<>).MakeGenericType(elementType)
                .GetConstructors().Single();

            Expression newCollectionExpression = Expression.New(
                ctor,
                container.GetRegistration(typeof(Scope), true)!.BuildExpression(),
                Expression.Constant(instance));

            // Wrap the collection in a Collection<T> or ReadOnlyCollection<T>
            if (collectionType.GetGenericTypeDefinition() == typeof(Collection<>)
                || collectionType.GetGenericTypeDefinition() == typeof(ReadOnlyCollection<>))
            {
                newCollectionExpression = Expression.New(
                    collectionType.GetConstructors().Where(c => c.GetParameters().Length == 1).First(),
                    newCollectionExpression);
            }

            return new ScopedRegistration(
                container.Options.DefaultScopedLifestyle!,
                container,
                collectionType,
                (Func<object>)container.Options.ExpressionCompilationBehavior.Compile(newCollectionExpression))
            {
                AdditionalInformationForLifestyleMismatchDiagnosticsProvider =
                    () => StringResources.FlowingCollectionIsScopedBecause(collectionType)
            };
        }

        private static bool ContainsScopedComponents(IContainerControlledCollection instance)
        {
            foreach (var producer in instance.GetProducers())
            {
                // This will trigger building the expression
                if (producer.ContainsScopedComponentsInGraph())
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsContainerControlledCollectionExpression(Expression enumerableExpression)
        {
            object? enumerable = (enumerableExpression as ConstantExpression)?.Value;

            return enumerable is IContainerControlledCollection;
        }

        internal static bool IsContainerControlledCollection(this InstanceProducer producer) =>
            IsContainerControlledCollection(producer.Registration);

        internal static bool IsContainerControlledCollection(this Registration registration) =>
            registration is ContainerControlledCollectionRegistration;

        internal static Type GetContainerControlledCollectionElementType(this InstanceProducer producer) =>
            ((ContainerControlledCollectionRegistration)producer.Registration).ElementType;

        private static Registration? TryCreateRegistrationForCollectionOfT(
            Type collectionType, IContainerControlledCollection controlledCollection, Container container)
        {
            if (collectionType.GetGenericTypeDefinition() == typeof(Collection<>)
                || collectionType.GetGenericTypeDefinition() == typeof(ReadOnlyCollection<>))
            {
                return new ContainerControlledCollectionRegistration(
                    Lifestyle.Singleton, collectionType, controlledCollection, container)
                {
                    Expression = Expression.Constant(
                        Activator.CreateInstance(collectionType, controlledCollection),
                        collectionType),
                };
            }

            return null;
        }

        private sealed class ContainerControlledCollectionRegistration : Registration
        {
            internal ContainerControlledCollectionRegistration(
                Lifestyle lifestyle,
                Type collectionType,
                IContainerControlledCollection collection,
                Container container)
                : base(lifestyle, container, collectionType)
            {
                this.Collection = collection;
                this.IsCollection = true;
            }

            internal override bool MustBeVerified => !this.Collection.AllProducersVerified;

            internal IContainerControlledCollection Collection { get; }

            internal Type ElementType => this.ImplementationType.GetGenericArguments()[0];

            internal ConstantExpression? Expression { get; set; }

            public override Expression BuildExpression() => this.Expression
                ?? System.Linq.Expressions.Expression.Constant(this.Collection, this.ImplementationType);

            internal override KnownRelationship[] GetRelationshipsCore() =>
                base.GetRelationshipsCore().Concat(this.GetCollectionRelationships()).ToArray();

            private IEnumerable<KnownRelationship> GetCollectionRelationships()
            {
                InjectionConsumerInfo? consumerInfo = null;

                InjectionConsumerInfo GetConsumer() => consumerInfo ??= this.Collection.InjectionConsumerInfo;

                return
                    from producer in this.Collection.GetProducers()
                    select new KnownRelationship(
                        this.ImplementationType, this.Lifestyle, GetConsumer(), producer)
                    {
                        // Since a controlled collection functions as a factory, their relationship should not
                        // be verified. That would lead to false positives, such as lifestyle mismatches and
                        // SRP violations.
                        UseForVerification = false
                    };
            }
        }
    }
}