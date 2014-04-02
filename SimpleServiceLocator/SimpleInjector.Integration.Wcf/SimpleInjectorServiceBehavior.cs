#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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

    internal class SimpleInjectorServiceBehavior : IServiceBehavior
    {
        private readonly Container container;

        public SimpleInjectorServiceBehavior(Container container)
        {
            this.container = container;
        }

        public void AddBindingParameters(ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
        {
            Requires.IsNotNull(serviceDescription, "serviceDescription");
            Requires.IsNotNull(serviceHostBase, "serviceHostBase");

            var instanceProvider =
                new SimpleInjectorInstanceProvider(this.container, serviceDescription.ServiceType);

            var endpointDispatchers = 
                GetEndpointDispatchersForImplementedContracts(serviceDescription, serviceHostBase);

            foreach (var endpointDispatcher in endpointDispatchers)
            {
                endpointDispatcher.DispatchRuntime.InstanceProvider = instanceProvider;
            }
        }

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