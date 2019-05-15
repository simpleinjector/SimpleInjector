// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Extension methods for enable advanced scenarios.
    /// </summary>
    public static class AdvancedExtensions
    {
        /// <summary>
        /// Determines whether the specified container is locked making any new registrations. The container
        /// is automatically locked when <see cref="Container.GetInstance">GetInstance</see> is called for the
        /// first time.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>
        ///   <c>true</c> if the specified container is locked; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/> is null.</exception>
        public static bool IsLocked(this Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            return container.IsLocked;
        }

        /// <summary>Determines whether the specified container is currently verifying its configuration.</summary>
        /// <param name="container">The container.</param>
        /// <returns><c>true</c> if the specified container is verifying; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/> is null.</exception>
        [Obsolete("Please use Container." + nameof(Container.IsVerifying) + " instead. " +
            "Will be treated as an error from version 5.0. Will be removed in version 6.0.",
            error: false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static bool IsVerifying(this Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            return container.IsVerifying;
        }

        /// <summary>
        /// Retrieves an item from the container stored by the given <paramref name="key"/> or null when no
        /// item is stored by that key.
        /// </summary>
        /// <remarks>
        /// <b>Thread-safety:</b> Calls to this method are thread-safe, but users should take proper
        /// percussions when they call both <b>GetItem</b> and <see cref="SetItem"/>.
        /// </remarks>
        /// <param name="container">The container.</param>
        /// <param name="key">The key of the item to retrieve.</param>
        /// <returns>The stored item or null (Nothing in VB).</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        [Obsolete("Please use Container.ContainerScope.GetItem instead." +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static object GetItem(this Container container, object key)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(key, nameof(key));

            return container.ContainerScope.GetItem(key);
        }

        /// <summary>
        /// Stores an item by the given <paramref name="key"/> in the container. 
        /// </summary>
        /// <remarks>
        /// <b>Thread-safety:</b> Calls to this method are thread-safe, but users should take proper
        /// percussions when they call both <see cref="GetItem"/> and <b>SetItem</b>.
        /// </remarks>
        /// <param name="container">The container.</param>
        /// <param name="key">The key of the item to insert or override.</param>
        /// <param name="item">The actual item. May be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/> or
        /// <paramref name="key"/> is a null reference (Nothing in VB).</exception>
        [Obsolete("Please use Container.ContainerScope.SetItem instead. " +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void SetItem(this Container container, object key, object item)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(key, nameof(key));

            container.ContainerScope.SetItem(key, item);
        }

        /// <summary>
        /// Adds an item by the given <paramref name="key"/> in the container by using the specified function,
        /// if the key does not already exist. This operation is atomic.
        /// </summary>
        /// <typeparam name="T">The Type of the item to create.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="key">The key of the item to insert or override.</param>
        /// <param name="valueFactory">The function used to generate a value for the given key. The supplied
        /// value of <paramref name="key"/> will be supplied to the function when called.</param>
        /// <returns>The stored item or the item from the <paramref name="valueFactory"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="key"/> or <paramref name="valueFactory"/> is a null reference (Nothing in VB).</exception>
        [Obsolete("Please use Container.ContainerScope.GetOrSetItem instead. " +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static T GetOrSetItem<T>(
            this Container container, object key, Func<Container, object, T> valueFactory)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(key, nameof(key));
            Requires.IsNotNull(valueFactory, nameof(valueFactory));

            return container.ContainerScope.GetOrSetItem(key, valueFactory);
        }

        /// <summary>
        /// Allows appending new registrations to existing registrations made using one of the
        /// <b>Collections.Register</b> overloads.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="serviceType">The service type of the collection.</param>
        /// <param name="registration">The registration to append.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceType"/> is not a
        /// reference type, is open generic, or ambiguous.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the container is locked.</exception>
        /// <exception cref="NotSupportedException">Thrown when the method is called for a registration
        /// that is made with one of the <b>Collections.Register</b> overloads that accepts a dynamic collection
        /// (an <b>IEnumerable</b> or <b>IEnumerable&lt;TService&gt;</b>).</exception>
        [Obsolete("Please use Container.Collection.Append instead. " +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void AppendToCollection(
            this Container container, Type serviceType, Registration registration)
        {
            Requires.IsNotNull(container, nameof(container));

            container.Collection.Append(serviceType, registration);
        }

        /// <summary>
        /// Allows appending new registrations to existing registrations made using one of the
        /// <b>Collections.Register</b> overloads.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="serviceType">The service type of the collection.</param>
        /// <param name="implementationType">The implementation type to append.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceType"/> is not a
        /// reference type, or ambiguous.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the container is locked.</exception>
        /// <exception cref="NotSupportedException">Thrown when the method is called for a registration
        /// that is made with one of the <b>Collections.Register</b> overloads that accepts a dynamic collection
        /// (an <b>IEnumerable</b> or <b>IEnumerable&lt;TService&gt;</b>).</exception>
        [Obsolete("Please use Container.Collection.Append instead. " +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        public static void AppendToCollection(
            this Container container, Type serviceType, Type implementationType)
        {
            Requires.IsNotNull(container, nameof(container));

            container.Collection.Append(serviceType, implementationType);
        }

        internal static void Verify(this IDependencyInjectionBehavior behavior, ConstructorInfo constructor)
        {
            foreach (ParameterInfo parameter in constructor.GetParameters())
            {
                behavior.Verify(new InjectionConsumerInfo(parameter));
            }
        }
    }
}