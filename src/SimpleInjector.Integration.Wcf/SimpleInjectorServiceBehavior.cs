// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.Wcf
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    /// <summary>
    /// <see cref="IServiceBehavior"/> implementation for Simple Injector.
    /// </summary>
    public class SimpleInjectorServiceBehavior : IServiceBehavior
    {
        private readonly Container container;

        /// <summary>Initializes a new instance of the <see cref="SimpleInjectorServiceBehavior"/> class.</summary>
        /// <param name="container">The container instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/> is a null reference.
        /// </exception>
        public SimpleInjectorServiceBehavior(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            this.container = container;
        }

        internal Type ServiceType { get; set; }

        /// <summary>
        /// Provides the ability to pass custom data to binding elements to support the contract implementation.
        /// </summary>
        /// <param name="serviceDescription">The service description of the service.</param>
        /// <param name="serviceHostBase">The host of the service.</param>
        /// <param name="endpoints">The service endpoints.</param>
        /// <param name="bindingParameters">Custom objects to which binding elements have access.</param>
        public void AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Provides the ability to change run-time property values or insert custom extension objects such as 
        /// error handlers, message or parameter interceptors, security extensions, and other custom extension 
        /// objects.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The host that is currently being built.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
        {
            Requires.IsNotNull(serviceDescription, nameof(serviceDescription));
            Requires.IsNotNull(serviceHostBase, nameof(serviceHostBase));

            var instanceProvider = new SimpleInjectorInstanceProvider(this.container,
                this.ServiceType ?? serviceDescription.ServiceType);

            var endpointDispatchers =
                GetEndpointDispatchersForImplementedContracts(serviceDescription, serviceHostBase);

            foreach (var endpointDispatcher in endpointDispatchers)
            {
                endpointDispatcher.DispatchRuntime.InstanceProvider = instanceProvider;
            }
        }

        /// <summary>
        /// Provides the ability to inspect the service host and the service description to confirm that the 
        /// service can run successfully.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The service host that is currently being constructed.</param>
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        private static IEnumerable<EndpointDispatcher> GetEndpointDispatchersForImplementedContracts(
            ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            var implementedContracts = (
                from serviceEndpoint in serviceDescription.Endpoints
                where serviceEndpoint.Contract.ContractType.IsAssignableFrom(serviceDescription.ServiceType)
                select serviceEndpoint.Contract.Name)
                .ToList();

            return
                from channelDispatcher in serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>()
                from endpointDispatcher in channelDispatcher.Endpoints
                where implementedContracts.Contains(endpointDispatcher.ContractName)
                select endpointDispatcher;
        }
    }
}