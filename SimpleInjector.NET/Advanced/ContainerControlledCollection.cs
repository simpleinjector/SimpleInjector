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

namespace SimpleInjector.Advanced
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using SimpleInjector.Lifestyles;

    // A decoratable enumerable is a collection that holds a set of Expression objects. When a decorator is
    // applied to a collection, a new DecoratableEnumerable will be created
    internal class ContainerControlledCollection<TService> 
#if NET45
        : IList<TService>, IContainerControlledCollection, IReadOnlyList<TService>
#else
        : IList<TService>, IContainerControlledCollection
#endif
    {
        private readonly Container container;

        private List<Lazy<InstanceProducer>> producers;

        // This constructor needs to be public. It is called using reflection.
        public ContainerControlledCollection(Container container, Type[] serviceTypes)
        {
            this.container = container;
            this.producers = serviceTypes.Select(this.ToLazyInstanceProducer).ToList();
        }

        // This constructor needs to be public. It is called using reflection.
        public ContainerControlledCollection(Container container, IEnumerable<Registration> registrations)
        {
            this.container = container;
            this.producers = registrations.Select(ToLazyInstanceProducer).ToList();
        }

        // This constructor needs to be public. It is called using reflection.
        // Note: the parameter order is swapped to remove the ambiguity between the other ctors when using
        // Activator.CreateInstance.
        public ContainerControlledCollection(TService[] singletons, Container container)
            : this(container, ConvertSingletonsToInstanceProducers(container, singletons))
        {
            // TODO: Make sure this method isn't called directly anymore.
        }

        public int Count
        {
            get { return this.producers.Count; }
        }

        bool ICollection<TService>.IsReadOnly
        {
            get { return true; }
        }

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
            Requires.IsNotNull(array, "array");

            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        bool ICollection<TService>.Remove(TService item)
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        void IContainerControlledCollection.Append(Registration registration)
        {
            this.container.ThrowWhenContainerIsLocked();

            this.producers.Add(ToLazyInstanceProducer(registration));
        }

        KnownRelationship[] IContainerControlledCollection.GetRelationships()
        {
            return (
                from producer in this.producers.Select(p => p.Value)
                from relationship in producer.GetRelationships()
                select relationship)
                .Distinct()
                .ToArray();
        }

        public IEnumerator<TService> GetEnumerator()
        {
            foreach (var producer in this.producers)
            {
                yield return (TService)producer.Value.GetInstance();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private static object VerifyCreatingProducer(Lazy<InstanceProducer> lazy)
        {
            try
            {
                // We only check if the instance producer can be created. We don't verify building of the
                // expression. That will be done up the callstack.
                return lazy.Value;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(StringResources.ConfigurationInvalidCreatingInstanceFailed(
                    typeof(TService), ex), ex);
            }
        }

        private static IEnumerable<Registration> ConvertSingletonsToInstanceProducers(Container container,
            TService[] singletons)
        {
            return
                from instance in singletons
                select SingletonLifestyle.CreateSingleRegistration(typeof(TService), instance, container);
        }

        private static Lazy<InstanceProducer> ToLazyInstanceProducer(Registration registration)
        {
            return Helpers.ToLazy(new InstanceProducer(typeof(TService), registration));
        }

        private Lazy<InstanceProducer> ToLazyInstanceProducer(Type implementationType)
        {
            // Note that the 'implementationType' could in fact be a service type as well and it is allowed
            // for the implementationType to equal TService. This will happen when someone does the following:
            // container.RegisterAll<ILogger>(typeof(ILogger));
            return new Lazy<InstanceProducer>(() =>
            {
                // If the the elementType is explicitly registered (using Register) we select this registration
                // (but we skip any implicit registrations, sine there could be more than one and it would
                // be unclear which one to pick).
                var instanceProducer = 
                    this.container.GetCurrentRegistrations(includeInvalidContainerRegisteredTypes: true, 
                        includeExternalProducers: false)
                        .FirstOrDefault(p => p.ServiceType == implementationType);

                // If there is such a producer registered we return a new one with the collection's type.
                // This producer will be automatically registered as external producer.
                if (instanceProducer != null)
                {
                    if (instanceProducer.ServiceType == typeof(TService))
                    {
                        return instanceProducer;
                    }

                    return new InstanceProducer(typeof(TService),
                        new ExpressionRegistration(instanceProducer.BuildExpression(), container));
                }

                if (!Helpers.IsConcreteType(implementationType))
                {
                    // This method will throw an (expressive) exception since implementationType is not concrete.
                    this.container.GetRegistration(implementationType, throwOnFailure: true);
                }
                
                // In case there is no registration, we create a new one.
                // This producer will be automatically registered as external producer.
                return Lifestyle.Transient.CreateProducer(typeof(TService), implementationType, this.container);
            });
        }

        private static NotSupportedException GetNotSupportedBecauseCollectionIsReadOnlyException()
        {
            return new NotSupportedException("Collection is read-only.");
        }

        private static NotSupportedException GetNotSupportedException()
        {
            return new NotSupportedException();
        }
    }
}