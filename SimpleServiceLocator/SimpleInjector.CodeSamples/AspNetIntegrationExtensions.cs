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
        private static Dictionary<Type, PropertyInfo[]> Types = 
            new Dictionary<Type, PropertyInfo[]>();

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

            var properties = GetInjectableProperties(instance.GetType());

            foreach (var property in properties)
            {
                InjectProperty(instance, property);
            }
        }

        private static IEnumerable<PropertyInfo> GetInjectableProperties(
            Type type)
        {
            var snapshot = Types;

            PropertyInfo[] properties;

            if (!snapshot.TryGetValue(type, out properties))
            {
                properties = (
                    from property in type.GetProperties()
                    where property.CanWrite
                    where property.DeclaringType.Namespace != "System.Web.UI"
                    let propertyType = property.PropertyType
                    where Container.GetRegistration(propertyType) != null
                    select property)
                    .ToArray();

                var copy = new Dictionary<Type, PropertyInfo[]>(snapshot);
                copy[type] = properties;
                Types = copy;
            }

            return properties;
        }

        private static void InjectProperty(object instance,
            PropertyInfo property)
        {
            var value = Container.GetInstance(property.PropertyType);

            if (value != null)
            {
                property.SetValue(instance, value, null);
            }
        }
    }
}