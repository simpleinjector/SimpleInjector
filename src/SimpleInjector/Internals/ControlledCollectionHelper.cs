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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using SimpleInjector.Advanced;

    // This class allows an ContainerControlledCollection<T> to notify about the creation of its wrapped items.
    // This is solely used for diagnostic verification.
    internal static class ControlledCollectionHelper
    {
        private static readonly object ServiceCreatedListenersLocker = new object();

        // The boolean flag is an optimization, to prevent slowing down resolving of items inside a collection
        // too much.
        private static List<Action<ServiceCreatedListenerArgs>> serviceCreatedListeners;

        internal static bool ContainsServiceCreatedListeners { get; private set; }

        internal static void AddServiceCreatedListener(Action<ServiceCreatedListenerArgs> serviceCreated)
        {
            lock (ServiceCreatedListenersLocker)
            {
                var listeners =
                    serviceCreatedListeners ?? (serviceCreatedListeners = new List<Action<ServiceCreatedListenerArgs>>());

                listeners.Add(serviceCreated);

                ContainsServiceCreatedListeners = true;
            }
        }

        internal static void RemoveServiceCreatedListener(Action<ServiceCreatedListenerArgs> serviceCreated)
        {
            lock (ServiceCreatedListenersLocker)
            {
                serviceCreatedListeners.Remove(serviceCreated);

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

        internal static IContainerControlledCollection ExtractContainerControlledCollectionFromRegistration(
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
            // We need special handling for Collection<T>, because the ContainerControlledCollection does not
            // (and can't) inherit from Collection<T>. So we have to wrap that stream into a Collection<T>.
            return collectionType.GetGenericTypeDefinition() == typeof(Collection<>)
                ? CreateRegistrationForCollectionOfT(instance, collectionType, container)
                : new ContainerControlledCollectionRegistration(collectionType, instance, container);
        }

        internal static bool IsContainerControlledCollectionExpression(Expression enumerableExpression)
        {
            var constantExpression = enumerableExpression as ConstantExpression;

            object enumerable = constantExpression != null ? constantExpression.Value : null;

            return enumerable is IContainerControlledCollection;
        }

        internal static bool IsContainerControlledCollection(this InstanceProducer producer) =>
            IsContainerControlledCollection(producer.Registration);

        internal static bool IsContainerControlledCollection(this Registration registration) =>
            registration is ContainerControlledCollectionRegistration;

        internal static Type GetContainerControlledCollectionElementType(this InstanceProducer producer) =>
            ((ContainerControlledCollectionRegistration)producer.Registration).ElementType;

        private static Registration CreateRegistrationForCollectionOfT(
            IContainerControlledCollection controlledCollection, Type collectionType, Container container)
        {
            var collection = Activator.CreateInstance(collectionType, controlledCollection);

            return new ContainerControlledCollectionRegistration(
                collectionType, controlledCollection, container)
            {
                Expression = Expression.Constant(
                    Activator.CreateInstance(collectionType, controlledCollection),
                    collectionType),
            };
        }

        private sealed class ContainerControlledCollectionRegistration : Registration
        {
            internal ContainerControlledCollectionRegistration(
                Type collectionType,
                IContainerControlledCollection collection,
                Container container)
                : base(Lifestyle.Singleton, container)
            {
                this.Collection = collection;
                this.ImplementationType = collectionType;
                this.IsCollection = true;
            }

            public override Type ImplementationType { get; }

            internal override bool MustBeVerified => !this.Collection.AllProducersVerified;

            internal IContainerControlledCollection Collection { get; }

            internal Type ElementType => this.ImplementationType.GetGenericArguments()[0];

            internal ConstantExpression Expression { get; set; }

            public override Expression BuildExpression() => this.Expression
                ?? System.Linq.Expressions.Expression.Constant(this.Collection, this.ImplementationType);

            internal override KnownRelationship[] GetRelationshipsCore() =>
                base.GetRelationshipsCore().Concat(this.Collection.GetRelationships()).ToArray();
        }
    }
}