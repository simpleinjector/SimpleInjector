#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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

        private List<Lazy<InstanceProducer>> producers = new List<Lazy<InstanceProducer>>();

        // This constructor needs to be public. It is called using reflection.
        public ContainerControlledCollection(Container container)
        {
            this.container = container;
        }

        public bool AllProducersVerified => this.producers.All(lazy => lazy.IsValueCreated);

        public int Count => this.producers.Count;

        bool ICollection<TService>.IsReadOnly => true;

        public TService this[int index]
        {
            get
            {
                return (TService)this.producers[index].Value.GetInstance();
            }

            set
            {
                throw GetNotSupportedBecauseCollectionIsReadOnlyException();
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

        int IList<TService>.IndexOf(TService item)
        {
            throw GetNotSupportedException();
        }

        void IList<TService>.Insert(int index, TService item)
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        public void RemoveAt(int index)
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        void ICollection<TService>.Add(TService item)
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        void ICollection<TService>.Clear()
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        bool ICollection<TService>.Contains(TService item)
        {
            throw GetNotSupportedException();
        }

        void ICollection<TService>.CopyTo(TService[] array, int arrayIndex)
        {
            Requires.IsNotNull(array, nameof(array));

            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        bool ICollection<TService>.Remove(TService item)
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        void IContainerControlledCollection.Clear()
        {
            this.container.ThrowWhenContainerIsLockedOrDisposed();

            this.producers.Clear();
        }

        void IContainerControlledCollection.Append(ContainerControlledItem registration)
        {
            this.producers.Add(this.ToLazyInstanceProducer(registration));
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
                yield return (TService)producer.Value.GetInstance();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

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
                throw new InvalidOperationException(StringResources.ConfigurationInvalidCreatingInstanceFailed(
                    typeof(TService), ex), ex);
            }
        }

        private Lazy<InstanceProducer> ToLazyInstanceProducer(ContainerControlledItem registration) => 
            registration.Registration != null
                ? ToLazyInstanceProducer(registration.Registration)
                : this.ToLazyInstanceProducer(registration.ImplementationType);

        private static Lazy<InstanceProducer> ToLazyInstanceProducer(Registration registration) => 
            Helpers.ToLazy(new InstanceProducer(typeof(TService), registration));

        private Lazy<InstanceProducer> ToLazyInstanceProducer(Type implementationType) => 
            new Lazy<InstanceProducer>(() => this.GetOrCreateInstanceProducer(implementationType));

        // Note that the 'implementationType' could in fact be a service type as well and it is allowed
        // for the implementationType to equal TService. This will happen when someone does the following:
        // container.RegisterCollection<ILogger>(typeof(ILogger));
        private InstanceProducer GetOrCreateInstanceProducer(Type implementationType)
        {
            // If the implementationType is explicitly registered (using a Register call) we select this 
            // producer (but we skip any implicit registrations or anything that is assignable, since 
            // there could be more than one and it would be unclear which one to pick).
            var instanceProducer = this.GetExplicitRegisteredInstanceProducer(implementationType);

            // If that doesn't result in a producer, we request a registration using unregistered type
            // resolution, were we prevent concrete types from being created by the container, since
            // the creation of concrete type would 'pollute' the list of registrations, and might result
            // in two registrations (since below we need to create a new instance producer out of it),
            // and that might cause duplicate diagnostic warnings.
            if (instanceProducer == null)
            {
                instanceProducer =
                    this.GetInstanceProducerThroughUnregisteredTypeResolution(implementationType);
            }

            // If that still hasn't resulted in a producer, we create a new producer and return (or throw
            // an exception in case the implementation type is not a concrete type).
            if (instanceProducer == null)
            {
                return this.CreateNewExternalProducer(implementationType);
            }

            // If there is such a producer registered we return a new one with the service type.
            // This producer will be automatically registered as external producer.
            if (instanceProducer.ServiceType == typeof(TService))
            {
                return instanceProducer;
            }

            return new InstanceProducer(typeof(TService),
                new ExpressionRegistration(instanceProducer.BuildExpression(), this.container));
        }

        private InstanceProducer GetExplicitRegisteredInstanceProducer(Type implementationType)
        {
            var registrations = this.container.GetCurrentRegistrations(
                includeInvalidContainerRegisteredTypes: true,
                includeExternalProducers: false);

            return registrations.FirstOrDefault(p => p.ServiceType == implementationType);
        }

        private InstanceProducer GetInstanceProducerThroughUnregisteredTypeResolution(Type implementationType)
        {
            var producer = this.container.GetRegistrationEvenIfInvalid(implementationType,
                InjectionConsumerInfo.Root, autoCreateConcreteTypes: false);

            bool producerIsValid = producer != null && producer.IsValid;

            // Prevent returning invalid producers
            return producerIsValid ? producer : null;
        }

        private InstanceProducer CreateNewExternalProducer(Type implementationType)
        {
            if (!Types.IsConcreteConstructableType(implementationType))
            {
                // This method will throw an (expressive) exception since implementationType is not concrete.
                this.container.GetRegistration(implementationType, throwOnFailure: true);
            }

            Lifestyle lifestyle = this.container.SelectionBasedLifestyle;

            // This producer will be automatically registered as external producer.
            return lifestyle.CreateProducer(typeof(TService), implementationType, this.container);
        }

        private static NotSupportedException GetNotSupportedBecauseCollectionIsReadOnlyException() =>
            new NotSupportedException("Collection is read-only.");

        private static NotSupportedException GetNotSupportedException() => new NotSupportedException();
    }
}