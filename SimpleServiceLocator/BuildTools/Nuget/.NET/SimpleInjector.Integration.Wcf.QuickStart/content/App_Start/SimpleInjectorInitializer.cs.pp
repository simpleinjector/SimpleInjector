[assembly: WebActivator.PostApplicationStartMethod(typeof($rootnamespace$.App_Start.SimpleInjectorInitializer), "Initialize")]

namespace $rootnamespace$.App_Start
{
    using System.Reflection;

    using SimpleInjector;
    using SimpleInjector.Integration.Wcf;

    public static class SimpleInjectorInitializer
    {
        /// <summary>Initialize the container and register it for the WCF ServiceHostFactory.</summary>
        public static void Initialize()
        {
            // Did you know the container can diagnose your configuration? Go to: http://bit.ly/YINd6R.
            var container = new Container();

            InitializeContainer(container);

            container.RegisterWcfServices(Assembly.GetExecutingAssembly());

            container.Verify();
			
            SimpleInjectorServiceHostFactory.SetContainer(container);

            // TODO: Add the following attribute to all .svc files:
            // Factory="SimpleInjector.Integration.Wcf.SimpleInjectorServiceHostFactory, SimpleInjector.Integration.Wcf"
        }

        private static void InitializeContainer(Container container)
        {
#error Register your services here (remove this line).

            // For instance:
            // container.Register<IUserRepository, SqlUserRepository>();
        }
    }
}