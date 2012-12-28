namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=Web%20Forms%20Integration
    using System;
    using System.Web;
    using System.Web.UI;
    using SimpleInjector;

    public static class AspNetIntegrationExtensions
    {
        private static Container container;

        // Don't forget to call this method during app_start.
        public static void SetContainer(Container container)
        {
            AspNetIntegrationExtensions.container = container;
        }

        public static void BuildUp(this Page page)
        {
            InjectProperties(page);
        }

        public static void BuildUp(this IHttpHandler handler)
        {
            InjectProperties(handler);
        }

        public static void BuildUp(this UserControl control)
        {
            InjectProperties(control);
        }

        private static void InjectProperties(object instance)
        {
            if (container == null)
            {
                throw new InvalidOperationException("Don't forget to call " +
                    "AspNetIntegrationExtensions.SetContainer first in " +
                    "the Application_Start method of the Global.asax.");
            }

            container.InjectProperties(instance);
        }
    }
}