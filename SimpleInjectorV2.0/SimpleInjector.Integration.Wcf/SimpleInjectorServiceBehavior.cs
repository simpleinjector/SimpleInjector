namespace SimpleInjector.Integration.Wcf
{
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
            var endpoints =
                from dispatcher in serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>()
                from endpoint in dispatcher.Endpoints
                select endpoint;

            foreach (var endpoint in endpoints)
            {
                endpoint.DispatchRuntime.InstanceProvider =
                    new SimpleInjectorInstanceProvider(this.container, serviceDescription.ServiceType);
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }
}