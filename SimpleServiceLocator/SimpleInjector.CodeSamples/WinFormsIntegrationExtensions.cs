namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq;
    using System.Windows.Forms;
    using SimpleInjector;

    public static class WinFormsIntegrationExtensions
    {
        private static Container container;

        // Don't forget to call this method during app_start.
        public static void SetContainer(Container container)
        {
            WinFormsIntegrationExtensions.container = container;
        }

        public static void BuildUp(this Form page)
        {
            InjectProperties(page);
        }

        public static void BuildUp(this UserControl control)
        {
            InjectProperties(control);
        }

        private static void InjectProperties(object instance)
        {
            if (container == null)
            {
                throw new InvalidOperationException("Don't forget " +
                    "to call SetContainer first.");
            }

            var properties =
                from property in instance.GetType().GetProperties()
                where property.CanWrite
                where !property.PropertyType.IsValueType
                where property.DeclaringType.Namespace != "System.Windows.Forms"
                let type = property.PropertyType
                let producer = container.GetRegistration(type)
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