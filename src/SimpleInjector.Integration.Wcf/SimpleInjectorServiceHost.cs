// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.Wcf
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    /// <summary>
    /// This service host is used to set up the service behavior that replaces the instance provider to use 
    /// dependency injection.
    /// </summary>
    public class SimpleInjectorServiceHost : ServiceHost
    {
        private readonly Container container;
        private readonly Type serviceAbstraction;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorServiceHost"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="baseAddresses">The base addresses.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/> is a null
        /// reference or <paramref name="serviceType"/> is a null reference.</exception>
        public SimpleInjectorServiceHost(Container container, Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            Requires.IsNotNull(container, nameof(container));

            this.container = container;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorServiceHost"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="baseAddresses">The base addresses.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/> is a null
        /// reference or <paramref name="singletonInstance"/> is a null reference.</exception>
        public SimpleInjectorServiceHost(
            Container container, object singletonInstance, params Uri[] baseAddresses)
            : base(singletonInstance, baseAddresses)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(singletonInstance, nameof(singletonInstance));

            this.container = container;
        }

        internal SimpleInjectorServiceHost(
            Container container, Type serviceAbstraction, Type implementationType, params Uri[] baseAddresses)
            : base(implementationType, baseAddresses)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(serviceAbstraction, nameof(serviceAbstraction));

            this.container = container;
            this.serviceAbstraction = serviceAbstraction;
        }

        /// <summary>Gets the contracts implemented by the service hosted.</summary>
        /// <returns>The collection of <see cref="ContractDescription"/>s.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "The base class already has a protected ImplementedContracts property, " +
                "so that name was already taken.")]
        public IEnumerable<ContractDescription> GetImplementedContracts() => this.ImplementedContracts.Values;

        /// <summary>Opens the channel dispatchers.</summary>
        protected override void OnOpening()
        {
            this.Description.Behaviors.Add(new SimpleInjectorServiceBehavior(this.container)
            {
                ServiceType = this.serviceAbstraction
            });

            base.OnOpening();
        }
    }
}