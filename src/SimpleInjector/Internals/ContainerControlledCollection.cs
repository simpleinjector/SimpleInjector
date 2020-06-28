// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector.Lifestyles;

    // A decoratable enumerable is a collection that holds a set of Expression objects. When a decorator is
    // applied to a collection, a new DecoratableEnumerable will be created
    internal class ContainerControlledCollection<TService>
        : IList<TService>,
        IContainerControlledCollection, IReadOnlyList<TService>
    {
        private static readonly InjectionConsumerInfo ConsumerInfo =
            new InjectionConsumerInfo(
                // Finds the TService parameter of the ctor(TService) constructor.
                typeof(ContainerControlledCollection<TService>).GetConstructors(includeNonPublic: true)
                    .Select(ctor => ctor.GetParameters())
                    .Where(p => p.Length == 1 && p[0].ParameterType == typeof(TService))
                    .Select(p => p[0])
                    .Single());

        private readonly Container container;
        private readonly List<LazyEx<InstanceProducer>> lazyProducers;

        // PERF: This is an optimization.
        // warning: this changes the behavior and makes the loading of producers less lazy, but according to
        // our specs (tests), this seems to be allowed.
        private readonly LazyEx<InstanceProducer[]> producers;

        // This constructor needs to be public. It is called using reflection.
        public ContainerControlledCollection(Container container)
        {
            this.container = container;

            this.lazyProducers = new List<LazyEx<InstanceProducer>>();
            this.producers =
                new LazyEx<InstanceProducer[]>(() => this.lazyProducers.Select(p => p.Value).ToArray());
        }

        // This constructor is called when we're in 'flowing' mode, because in that case we need to set the
        // scope before forwarding the call to InstanceProducers.
        protected ContainerControlledCollection(
            Container container, ContainerControlledCollection<TService> definition)
        {
            this.container = container;

            // Reference the producers from the definition. This doesn't cause any new objects.
            this.lazyProducers = definition.lazyProducers;
            this.producers = definition.producers;
        }

        // This constructor is not called; its metadata is used to build valid KnownRelationship types.
        internal ContainerControlledCollection(TService services)
        {
            throw new NotSupportedException("This constructor is not intended to be called.");
        }

        public InjectionConsumerInfo InjectionConsumerInfo => ConsumerInfo;

        public bool AllProducersVerified => this.lazyProducers.All(lazy => lazy.IsValueCreated);

        public int Count => this.lazyProducers.Count;

        bool ICollection<TService>.IsReadOnly => true;

        internal InstanceProducer? ParentProducer { get; set; }

        public virtual TService this[int index]
        {
            get => GetInstance(this.producers.Value[index]);
            set => throw GetNotSupportedBecauseReadOnlyException();
        }

        // Throws an InvalidOperationException on failure.
        public void VerifyCreatingProducers()
        {
            // We must iterate the list of lazies, because we want to wrap creation of producers in a try-catch.
            foreach (LazyEx<InstanceProducer> lazy in this.lazyProducers)
            {
                VerifyCreatingProducer(lazy);
            }
        }

        public virtual int IndexOf(TService item)
        {
            // InstanceProducers never return null, so we can short-circuit the operation and return -1.
            if (item is null)
            {
                return -1;
            }

            var producers = this.producers.Value;

            for (int index = 0; index < producers.Length; index++)
            {
                InstanceProducer producer = producers[index];

                // NOTE: We call GetInstance directly as we don't want to notify about the creation to the
                // ContainsServiceCreatedListeners here; created instances will not leak out of this method
                // and can, therefore, never cause Captive Dependencies.
                var instance = producer.GetInstance();

                if (instance.Equals(item))
                {
                    return index;
                }
            }

            return -1;
        }

        void IList<TService>.Insert(int index, TService item)
        {
            throw GetNotSupportedBecauseReadOnlyException();
        }

        public void RemoveAt(int index) => throw GetNotSupportedBecauseReadOnlyException();

        void ICollection<TService>.Add(TService item) => throw GetNotSupportedBecauseReadOnlyException();

        void ICollection<TService>.Clear() => throw GetNotSupportedBecauseReadOnlyException();

        bool ICollection<TService>.Contains(TService item) => this.IndexOf(item) > -1;

        public virtual void CopyTo(TService[] array, int arrayIndex)
        {
            Requires.IsNotNull(array, nameof(array));

            foreach (var producer in this.producers.Value)
            {
                array[arrayIndex++] = GetInstance(producer);
            }
        }

        bool ICollection<TService>.Remove(TService item) => throw GetNotSupportedBecauseReadOnlyException();

        void IContainerControlledCollection.Clear()
        {
            this.container.ThrowWhenContainerIsLockedOrDisposed();

            this.ThrowWhenCollectionAlreadyHasBeenIterated();

            this.lazyProducers.Clear();
        }

        void IContainerControlledCollection.Append(ContainerControlledItem item)
        {
            this.ThrowWhenCollectionAlreadyHasBeenIterated();

            this.lazyProducers.Add(this.ToLazyInstanceProducer(item));
        }

        public InstanceProducer[] GetProducers() => this.producers.Value;

        public virtual IEnumerator<TService> GetEnumerator() => new Enumerator(this.producers.Value);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        protected static TService GetInstance(InstanceProducer producer)
        {
            var service = (TService)producer.GetInstance();

            // This check is an optimization that prevents always calling the helper method, while in the
            // happy path it is not needed.
            // This code is in the happy path, so we want the performance penalty to be minimal.
            // That's why we don't have a lock around this field access. This might cause the value to become
            // stale (when read by other threads), but that's not an issue here; other threads might still see
            // an old value (for some time), but we are actually only interested in getting notifications from
            // the same thread anyway.
            if (ControlledCollectionHelper.ContainsServiceCreatedListeners)
            {
                ControlledCollectionHelper.NotifyServiceCreatedListeners(producer);
            }

            return service;
        }

        private static object VerifyCreatingProducer(LazyEx<InstanceProducer> lazy)
        {
            try
            {
                // We only check if the instance producer can be created. We don't verify building of the
                // expression. That will be done up the call stack.
                return lazy.Value;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    StringResources.ConfigurationInvalidCreatingInstanceFailed(typeof(TService), ex),
                    ex);
            }
        }

        private LazyEx<InstanceProducer> ToLazyInstanceProducer(ContainerControlledItem item) =>
            item.Registration != null
                ? ToLazyInstanceProducer(item.Registration)
                : new LazyEx<InstanceProducer>(() => this.GetOrCreateInstanceProducer(item));

        private static LazyEx<InstanceProducer> ToLazyInstanceProducer(Registration registration) =>
            Helpers.ToLazy(new InstanceProducer(typeof(TService), registration));

        // Note that the 'implementationType' could in fact be a service type as well and it is allowed
        // for the implementationType to equal TService. This will happen when someone does the following:
        // container.Collections.Register<ILogger>(typeof(ILogger));
        private InstanceProducer GetOrCreateInstanceProducer(ContainerControlledItem item)
        {
            Type implementationType = item.ImplementationType;

            // If the implementationType is explicitly registered (using a Register call) we select this
            // producer (but we skip any implicit registrations or anything that is assignable, since
            // there could be more than one and it would be unclear which one to pick).
            InstanceProducer? producer = this.GetExplicitRegisteredInstanceProducer(implementationType);

            // If that doesn't result in a producer, we request a registration using unregistered type
            // resolution, were we prevent concrete types from being created by the container, since
            // the creation of concrete type would 'pollute' the list of registrations, and might result
            // in two registrations (since below we need to create a new instance producer out of it),
            // and that might cause duplicate diagnostic warnings.
            if (producer is null)
            {
                producer = this.GetInstanceProducerThroughUnregisteredTypeResolution(implementationType);
            }

            // If that still hasn't resulted in a producer, we create a new producer and return (or throw
            // an exception in case the implementation type is not a concrete type).
            if (producer is null)
            {
                return this.CreateNewExternalProducer(item);
            }

            // If there is such a producer registered we return a new one with the service type.
            // This producer will be automatically registered as external producer.
            if (producer.ServiceType == typeof(TService))
            {
                return producer;
            }

            return new InstanceProducer(
                typeof(TService),
                new ExpressionRegistration(producer.BuildExpression(), this.container));
        }

        private InstanceProducer? GetExplicitRegisteredInstanceProducer(Type implementationType)
        {
            var registrations = this.container.GetCurrentRegistrations(
                includeInvalidContainerRegisteredTypes: true,
                includeExternalProducers: false);

            return Array.Find(registrations, p => p.ServiceType == implementationType);
        }

        private InstanceProducer? GetInstanceProducerThroughUnregisteredTypeResolution(Type implementationType)
        {
            var producer = this.container.GetRegistrationEvenIfInvalid(
                implementationType,
                InjectionConsumerInfo.Root,
                autoCreateConcreteTypes: false);

            bool producerIsValid = producer?.IsValid == true;

            // Prevent returning invalid producers
            return producerIsValid ? producer : null;
        }

        private InstanceProducer CreateNewExternalProducer(ContainerControlledItem item)
        {
            if (!Types.IsConcreteConstructableType(item.ImplementationType))
            {
                throw new ActivationException(
                    StringResources.UnregisteredAbstractionFoundInCollection(
                        serviceType: typeof(TService),
                        registeredType: item.RegisteredImplementationType,
                        foundAbstractType: item.ImplementationType));
            }

            Lifestyle lifestyle = this.container.SelectionBasedLifestyle;

            // This producer will be automatically registered as external producer.
            return lifestyle.CreateProducer(typeof(TService), item.ImplementationType, this.container);
        }

        private static NotSupportedException GetNotSupportedBecauseReadOnlyException() =>
            new NotSupportedException("Collection is read-only.");

        private void ThrowWhenCollectionAlreadyHasBeenIterated()
        {
            if (this.producers.IsValueCreated)
            {
                throw new InvalidOperationException("Can't append. The collection has already been iterated.");
            }
        }

        // PERF: This custom enumerator is slightly faster compared to the one generated by the C# compiler.
        private sealed class Enumerator : IEnumerator<TService>
        {
            private readonly InstanceProducer[] producers;
            private readonly int length;
            private int index;
            private TService current;

            public Enumerator(InstanceProducer[] producers)
            {
                this.producers = producers;
                this.length = producers.Length;
                this.current = default!;
            }

            public TService Current => this.current!;

            object IEnumerator.Current => this.current!;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this.index < this.length)
                {
                    this.current = GetInstance(this.producers[this.index]);
                    this.index++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                this.current = default!;
                this.index = 0;
            }
        }
    }
}