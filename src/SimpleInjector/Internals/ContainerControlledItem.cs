// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Container controlled collections can be supplied with both Type objects or direct Registration
    /// instances.
    /// </summary>
    [DebuggerDisplay(nameof(ContainerControlledItem) + " ({" + nameof(ContainerControlledItem.DebuggerDisplay) + ", nq})")]
    internal sealed class ContainerControlledItem
    {
        public readonly Type ImplementationType;

        public readonly Registration? Registration;

        private ContainerControlledItem(Registration registration)
        {
            Requires.IsNotNull(registration, nameof(registration));

            this.Registration = registration;
            this.ImplementationType = registration.ImplementationType;
            this.RegisteredImplementationType = registration.ImplementationType;
        }

        private ContainerControlledItem(Type implementationType)
        {
            Requires.IsNotNull(implementationType, nameof(implementationType));

            this.ImplementationType = implementationType;
            this.RegisteredImplementationType = implementationType;
        }

        internal Type RegisteredImplementationType { get; private set; }
        
        internal string DebuggerDisplay =>
            $"ImplementationType: {this.ImplementationType.ToFriendlyName()}, " + (
            this.Registration != null
                ? $"Registration.ImplementationType: {this.Registration.ImplementationType.ToFriendlyName()}"
                : "Registration: <null>");

        public static ContainerControlledItem CreateFromRegistration(Registration registration) =>
            new ContainerControlledItem(registration);

        public static ContainerControlledItem CreateFromType(Type implementationType) =>
            new ContainerControlledItem(implementationType);

        public static ContainerControlledItem CreateFromType(
            Type registeredImplementationType, Type closedImplementationType) =>
            new ContainerControlledItem(closedImplementationType)
            {
                RegisteredImplementationType = registeredImplementationType
            };
    }
}