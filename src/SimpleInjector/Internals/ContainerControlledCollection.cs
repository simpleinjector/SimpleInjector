#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2019 Simple Injector Contributors
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

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

    // A decoratable enumerable is a collection that holds a set of Expression objects. When a decorator is
    // applied to a collection, a new DecoratableEnumerable will be created
    internal class ContainerControlledCollection<TService> : IList<TService>,
#if NET40
        IContainerControlledCollection
#else
        IContainerControlledCollection, IReadOnlyList<TService>
#endif
    {
        private readonly Container container;

        private readonly List<Lazy<InstanceProducer>> producers = new List<Lazy<InstanceProducer>>();

        // This constructor needs to be public. It is called using reflection.
        public ContainerControlledCollection(Container container)
        {
            this.container = container;
        }

        public bool AllProducersVerified => this.producers.All(lazy => lazy.IsValueCreated);

        public int Count => this.producers.Count;

        bool ICollection<TService>.IsReadOnly => true;

        internal InstanceProducer? ParentProducer { get; set; }

        public TService this[int index]
        {
            get
            {
                var producer = this.producers[index].Value;

                return GetInstance(producer);
            }

            set
            {
                throw GetNotSupportedBecauseReadOnlyException();
            }
        }

        // Throws an InvalidOperationException on failure.
        public void VerifyCreatingProducers()
        {
            foreach (var lazy in this.producers)
            {
                VerifyCreatingProducer(lazy);
            }
        }

        public int IndexOf(TService item)
        {
            // InstanceProducers never return null, so we can short-circuit the operation and return -1.
            if (item == null)
            {
                return -1;
            }

            for (int index = 0; index < this.producers.Count; index++)
            {
                InstanceProducer producer = this.producers[index].Value;

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

        void ICollection<TService>.CopyTo(TService[] array, int arrayIndex)
        {
            Requires.IsNotNull(array, nameof(array));

            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        bool ICollection<TService>.Remove(TService item) => throw GetNotSupportedBecauseReadOnlyException();

        void IContainerControlledCollection.Clear()
        {
            this.container.ThrowWhenContainerIsLockedOrDisposed();

            this.producers.Clear();
        }

        void IContainerControlledCollection.Append(ContainerControlledItem item)
        {
            this.producers.Add(this.ToLazyInstanceProducer(item));
        }

        KnownRelationship[] IContainerControlledCollection.GetRelationships() => (
            from producer in this.producers.Select(p => p.Value)
            from relationship in producer.GetRelationships()
            select relationship)
            .Distinct()
            .ToArray();

        public IEnumerator<TService> GetEnumerator()
        {
            foreach (var producer in this.producers)
            {
                yield return GetInstance(producer.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        private static TService GetInstance(InstanceProducer producer)
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

        private static object VerifyCreatingProducer(Lazy<InstanceProducer> lazy)
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

        private Lazy<InstanceProducer> ToLazyInstanceProducer(ContainerControlledItem item) =>
            item.Registration != null
                ? ToLazyInstanceProducer(item.Registration)
                : new Lazy<InstanceProducer>(() => this.GetOrCreateInstanceProducer(item));

        private static Lazy<InstanceProducer> ToLazyInstanceProducer(Registration registration) =>
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
            if (producer == null)
            {
                producer = this.GetInstanceProducerThroughUnregisteredTypeResolution(implementationType);
            }

            // If that still hasn't resulted in a producer, we create a new producer and return (or throw
            // an exception in case the implementation type is not a concrete type).
            if (producer == null)
            {
                return this.CreateNewExternalProducer(item);
            }

            // If there is such a producer registered we return a new one with the service type.
            // This producer will be automatically registered as external producer.
            if (producer.ServiceType == typeof(TService))
            {
                return producer;
            }

            return new InstanceProducer(typeof(TService),
                new ExpressionRegistration(producer.BuildExpression(), this.container));
        }

        private InstanceProducer? GetExplicitRegisteredInstanceProducer(Type implementationType)
        {
            var registrations = this.container.GetCurrentRegistrations(
                includeInvalidContainerRegisteredTypes: true,
                includeExternalProducers: false);

            return registrations.FirstOrDefault(p => p.ServiceType == implementationType);
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
    }
}