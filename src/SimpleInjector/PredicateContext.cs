// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using SimpleInjector.Advanced;

    /// <summary>
    /// An instance of this type will be supplied to the <see cref="Predicate{T}"/>
    /// delegate that is that is supplied to the 
    /// <see cref="Container.RegisterConditional(System.Type, System.Type, Lifestyle, Predicate{PredicateContext})">RegisterConditional</see>
    /// overload that takes this delegate. This type contains information about the open generic service that 
    /// is about to be created and it allows the user to examine the given instance to decide whether this 
    /// implementation should be created or not.
    /// </summary>
    /// <remarks>
    /// Please see the 
    /// <see cref="Container.RegisterConditional(System.Type, System.Type, Lifestyle, Predicate{PredicateContext})">Register</see>
    /// method for more information.
    /// </remarks>
    [DebuggerDisplay(nameof(PredicateContext) + " ({" + nameof(PredicateContext.DebuggerDisplay) + ", nq})")]
    public sealed class PredicateContext : ApiObject
    {
        private readonly InjectionConsumerInfo consumer;
        private readonly Func<Type> implementationTypeProvider;
        private Type implementationType;

        internal PredicateContext(InstanceProducer producer, InjectionConsumerInfo consumer, bool handled)
            : this(producer.ServiceType, producer.Registration.ImplementationType, consumer, handled)
        {
        }

        internal PredicateContext(
            Type serviceType, Type implementationType, InjectionConsumerInfo consumer, bool handled)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(implementationType, nameof(implementationType));
            Requires.IsNotNull(consumer, nameof(consumer));

            this.ServiceType = serviceType;
            this.implementationType = implementationType;
            this.consumer = consumer;
            this.Handled = handled;
        }

        internal PredicateContext(
            Type serviceType,
            Func<Type> implementationTypeProvider,
            InjectionConsumerInfo consumer,
            bool handled)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(implementationTypeProvider, nameof(implementationTypeProvider));
            Requires.IsNotNull(consumer, nameof(consumer));

            this.ServiceType = serviceType;
            this.implementationTypeProvider = implementationTypeProvider;
            this.consumer = consumer;
            this.Handled = handled;
        }

        /// <summary>Gets the closed generic service type that is to be created.</summary>
        /// <value>The closed generic service type.</value>
        public Type ServiceType { get; }

        /// <summary>
        /// Gets the closed generic implementation type that will be created by the container.
        /// </summary>
        /// <value>The implementation type.</value>
        public Type ImplementationType =>
            this.implementationType ?? (this.implementationType = this.implementationTypeProvider());

        /// <summary>Gets a value indicating whether a previous <b>Register</b> registration has already
        /// been applied for the given <see cref="ServiceType"/>.</summary>
        /// <value>The indication whether the event has been handled.</value>
        public bool Handled { get; }

        /// <summary>
        /// Gets the contextual information of the consuming component that directly depends on the resolved
        /// service. This property will return null in case the service is resolved directly from the container.
        /// </summary>
        /// <value>The <see cref="InjectionConsumerInfo"/> or null.</value>
        public InjectionConsumerInfo Consumer =>
            this.consumer != InjectionConsumerInfo.Root ? this.consumer : null;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "This method is called by the debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebuggerDisplay => string.Format(
            CultureInfo.InvariantCulture,
            "{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}",
            nameof(this.ServiceType),
            this.ServiceType.ToFriendlyName(),
            nameof(this.ImplementationType),
            this.ImplementationType.ToFriendlyName(),
            nameof(this.Handled),
            this.Handled,
            nameof(this.Consumer),
            this.Consumer);
    }
}