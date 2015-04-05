[assembly: WebActivator.PreApplicationStartMethod(typeof($rootnamespace$.App_Start.SimpleInjectorInitializer), "PreInitialize")]
[assembly: WebActivator.PostApplicationStartMethod(typeof($rootnamespace$.App_Start.SimpleInjectorInitializer), "Initialize")]

namespace $rootnamespace$.App_Start
{
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using SimpleInjector;
    using SimpleInjector.Integration.Web.Forms;
    
    public static class SimpleInjectorInitializer
    {
        /// <summary>Registers a Module for enabling initialization of Web Form types.</summary>
        public static void PreInitialize()
        {
            // Enable initialization of Web Form Pages, User Controls and HTTP handlers according to 
            // the container's configuration. When used in combination with the 
            // WebFormsPropertySelectionBehavior class, properties will be injected into those type.
            DynamicModuleUtility.RegisterModule(typeof(SimpleInjectorWebFormsHttpModule));
		}

        /// <summary>Creates and initializes the container.</summary>
        public static void Initialize()
        {
            // Did you know the container can diagnose your configuration?
            // Go to: https://simpleinjector.org/diagnostics
            var container = new Container();

            // Override the default behavior and allows properties to be injected into pages, handlers
            // and user controls.
            container.Options.PropertySelectionBehavior =
                new WebFormsPropertySelectionBehavior(container.Options.PropertySelectionBehavior);
                        
            InitializeContainer(container);

            container.RegisterHttpHandlers();
            container.RegisterPages();
            container.RegisterUserControls();
       
            container.Verify();

            SimpleInjectorWebFormsHttpModule.SetContainer(container);
        }
     
        private static void InitializeContainer(Container container)
        {
#error Register your services here (remove this line).

            // For instance:
            // container.Register<IUserRepository, SqlUserRepository>();
        }
    }
}