// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Contains contextual information for creating an implementation type.
    /// </summary>
    /// <remarks>
    /// Please see the
    /// <see cref="Container.RegisterConditional(System.Type, System.Type, Lifestyle, Predicate{PredicateContext})">Register</see>
    /// method for more information.
    /// </remarks>
    [DebuggerDisplay(nameof(TypeFactoryContext) + " ({" + nameof(TypeFactoryContext.DebuggerDisplay) + ", nq})")]
    public sealed class TypeFactoryContext : ApiObject
    {
        private readonly InjectionConsumerInfo consumer;

        internal TypeFactoryContext(Type serviceType, InjectionConsumerInfo consumer)
        {
            this.ServiceType = serviceType;
            this.consumer = consumer;
        }

        /// <summary>Gets the closed generic service type that is to be created.</summary>
        /// <value>The closed generic service type.</value>
        public Type ServiceType { get; }

        /// <summary>
        /// Gets the contextual information of the consuming component that directly depends on the resolved
        /// service. This property will return null in case the service is resolved directly from the container.
        /// </summary>
        /// <value>The <see cref="InjectionConsumerInfo"/> or null.</value>
        public InjectionConsumerInfo? Consumer =>
            this.consumer != InjectionConsumerInfo.Root ? this.consumer : null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebuggerDisplay => string.Format(
            CultureInfo.InvariantCulture,
            "{0}: {1}, {2}: {3}",
            nameof(this.ServiceType),
            this.ServiceType.ToFriendlyName(),
            nameof(this.Consumer),
            this.Consumer);
    }
}