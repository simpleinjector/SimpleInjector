// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// An instance of this type will be supplied to the <see cref="Predicate{T}" />
    /// delegate that is that is supplied to the
    /// <see cref="Container.RegisterInitializer(Action{InstanceInitializationData}, Predicate{InitializerContext})">RegisterInitializer</see>
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
        /// Gets the <see cref="Registration"/> that is responsible for the initialization of the created
        /// instance.
        /// </summary>
        /// /// <value>The <see cref="Registration"/>.</value>
        public Registration Registration { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebuggerDisplay =>
            string.Format(CultureInfo.InvariantCulture,
                "Registration.ImplementationType: {0}",
                this.Registration.ImplementationType.ToFriendlyName());
    }
}