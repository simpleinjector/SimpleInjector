// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// An instance of this type will be supplied to the <see cref="System.Predicate{T}" />
    /// delegate that is that is supplied to the
    /// <see cref="ContainerOptions.RegisterResolveInterceptor(ResolveInterceptor, Predicate{InitializationContext})">RegisterResolveInterceptor</see>
    /// method that takes this delegate. This type contains contextual information about a resolved type and it
    /// allows the user to examine the given instance to decide whether the <see cref="ResolveInterceptor"/>
    /// should be applied or not.
    /// </summary>
    [DebuggerDisplay(nameof(InitializationContext) +
        " ({" + nameof(InitializationContext.DebuggerDisplay) + ", nq})")]
    public class InitializationContext
    {
        internal InitializationContext(InstanceProducer producer, Registration registration)
        {
            // producer will be null when a user calls Registration.BuildExpression() directly, instead of
            // calling InstanceProducer.BuildExpression() or InstanceProducer.GetInstance().
            Requires.IsNotNull(registration, nameof(registration));

            this.Producer = producer;
            this.Registration = registration;
        }

        /// <summary>
        /// Gets the <see cref="InstanceProducer"/> that is responsible for the initialization of the created
        /// instance.
        /// </summary>
        /// <value>The <see cref="InstanceProducer"/> or null (Nothing in VB) when the instance producer is
        /// unknown.</value>
        public InstanceProducer Producer { get; }

        /// <summary>
        /// Gets the <see cref="Registration"/> that is responsible for the initialization of the created
        /// instance.
        /// </summary>
        /// /// <value>The <see cref="Registration"/>.</value>
        public Registration Registration { get; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "This method is called by the debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebuggerDisplay =>
            string.Format(CultureInfo.InvariantCulture,
                "Producer.ServiceType: {0}, Registration.ImplementationType: {1}",
                this.Producer.ServiceType.ToFriendlyName(),
                this.Registration.ImplementationType.ToFriendlyName());
    }
}