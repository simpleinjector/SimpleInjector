namespace SimpleInjector.Integration.Wcf
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    internal class SimpleInjectorInstanceProvider : IInstanceProvider
    {
        private readonly Container container;
        private readonly Type serviceType;

        public SimpleInjectorInstanceProvider(Container container, Type serviceType)
        {
            this.container = container;
            this.serviceType = serviceType;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return this.container.GetInstance(this.serviceType);
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.container.GetInstance(this.serviceType);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            // Simple Injector does not dispose instances.
        }
    }
}