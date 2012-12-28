namespace SimpleInjector.Integration.Wcf
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// This service host is used to set up the service behavior that replaces the instance provider to use 
    /// dependency injection.
    /// </summary>
    public class SimpleInjectorServiceHost : ServiceHost
    {
        private readonly Container container;

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
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            this.container = container;
        }

        /// <summary>Opens the channel dispatchers.</summary>
        protected override void OnOpening()
        {
            this.Description.Behaviors.Add(new SimpleInjectorServiceBehavior(this.container));

            base.OnOpening();
        }
    }
}