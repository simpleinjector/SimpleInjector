// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// An instance of this type will be supplied to the <see cref="System.Predicate{T}" />
    /// delegate that is that is supplied to the
    /// <see cref="SimpleInjector.Container.RegisterInitializer(Action{InstanceInitializationData}, Predicate{InitializerContext})">RegisterInitializer</see>
    /// overload that takes this delegate. This type contains contextual information about the creation and it
    /// allows the user to examine the given instance to decide whether the instance should be initialized or
    /// not.
    /// </summary>
    [DebuggerDisplay(nameof(InitializerContext) + " ({" + nameof(InitializerContext.DebuggerDisplay) + ", nq})")]
    public class InitializerContext
    {
        internal InitializerContext(Registration registration)
        {
            Requires.IsNotNull(registration, nameof(registration));

            this.Registration = registration;
        }

        /// <summary>
        /// Gets a null reference. This property has been deprecated.
        /// </summary>
        /// <value>The null (Nothing in VB).</value>
        [Obsolete("The Producer property has been deprecated. Please use Registration instead. " +
            "Will be removed in version 5.0.",
            error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public InstanceProducer? Producer { get; }

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
                "Registration.ImplementationType: {0}",
                this.Registration.ImplementationType.ToFriendlyName());
    }
}