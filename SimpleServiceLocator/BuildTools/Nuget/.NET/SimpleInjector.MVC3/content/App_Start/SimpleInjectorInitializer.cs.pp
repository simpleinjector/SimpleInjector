[assembly: WebActivator.PreApplicationStartMethod(typeof($rootnamespace$.App_Start.SimpleInjectorInitializer), "Initialize")]

namespace $rootnamespace$.App_Start
{
    using System.Reflection;
    using System.Web.Mvc;

    using SimpleInjector;
    using SimpleInjector.Integration.Web.Mvc;
    
    public static class SimpleInjectorInitializer
    {
        /// <summary>Initialize the container and register it as MVC3 Dependency Resolver.</summary>
        public static void Initialize()
        {
            var container = new Container();
            
            InitializeContainer(container);

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            
            container.RegisterMvcAttributeFilterProvider();
       
            // Using Entity Framework? Please read this: http://simpleinjector.codeplex.com/discussions/363935
            container.Verify();
            
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }
     
        private static void InitializeContainer(Container container)
        {
            // Please note that if you updated the SimpleInjector.MVC3 package from a previous version, this
            // SimpleInjectorInitializer class replaces the previous SimpleInjectorMVC3 class. You should
            // move the registrations from the old SimpleInjectorMVC3.InitializeContainer to this method,
            // and remove the SimpleInjectorMVC3 and SimpleInjectorMVC3Extensions class from the App_Start
            // folder.
#error Register your services here (remove this line).

            // For instance:
            // container.Register<IUserRepository, SqlUserRepository>();
        }
    }
}