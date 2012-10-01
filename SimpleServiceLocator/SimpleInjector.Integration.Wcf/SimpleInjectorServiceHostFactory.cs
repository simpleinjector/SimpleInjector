namespace SimpleInjector.Integration.Wcf
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activation;

    /// <summary>
    /// Factory that provides instances of <see cref="SimpleInjectorServiceHost"/> in managed hosting 
    /// environments where the host instance is created dynamically in response to incoming messages.
    /// </summary>
    public class SimpleInjectorServiceHostFactory : ServiceHostFactory
    {
        private static Container container;

        /// <summary>Sets the container.</summary>
        /// <param name="container">The container.</param>
        public static void SetContainer(Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (SimpleInjectorServiceHostFactory.container != null)
            {
                throw new InvalidOperationException(
                    "This method can only be called once. The container has already been set.");
            }

            SimpleInjectorWcfExtensions.EnablePerWcfOperationLifestyle(container);

            SimpleInjectorServiceHostFactory.container = container;
        }

        /// <summary>
        /// Creates a <see cref="SimpleInjectorServiceHost"/> for a specified type of service with a specific 
        /// base address. 
        /// </summary>
        /// <returns>
        /// A <see cref="SimpleInjectorServiceHost"/> for the type of service specified with a specific base 
        /// address.
        /// </returns>
        /// <param name="serviceType">
        /// Specifies the type of service to host. 
        /// </param>
        /// <param name="baseAddresses">
        /// The <see cref="T:System.Array"/> of type <see cref="T:System.Uri"/> that contains the base 
        /// addresses for the service hosted.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceType"/> is a null 
        /// reference.</exception>
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (container == null)
            {
                throw new InvalidOperationException("The operation failed. Please call the " +
                    typeof(SimpleInjectorServiceHostFactory).FullName + ".SetContainer(Container) method " +
                    "supplying the application's " + typeof(Container).FullName + " instance during " +
                    "application startup (for instance inside the Application_Start event of the Global.asax).");
            }

            return new SimpleInjectorServiceHost(container, serviceType, baseAddresses);
        }
    }
}