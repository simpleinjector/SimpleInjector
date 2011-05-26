namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;

    public static class AspNetIntegrationExtensions
    {
        private static Container Container;

        // Don't forget to call this method during app_start.
        public static void SetContainer(Container container)
        {
            Container = container;
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
            if (Container == null)
            {
                throw new InvalidOperationException("Don't forget " +
                    "to call SetContainer first.");
            }

            var properties =
                from property in instance.GetType().GetProperties()
                where property.CanWrite
                let type = property.PropertyType
                where !type.IsValueType
                where type.Namespace != "System.Web.UI"
                let producer = Container.GetRegistration(type)
                where producer != null
                select new { property, producer };

            foreach (var p in properties)
            {
                object value = p.producer.GetInstance();
                p.property.SetValue(instance, value, null);
            }
        }
    }
}