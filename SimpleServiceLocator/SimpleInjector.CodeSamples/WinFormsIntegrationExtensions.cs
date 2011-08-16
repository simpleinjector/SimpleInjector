namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Windows.Forms;

    using SimpleInjector;
    using SimpleInjector.Extensions;

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
                throw new InvalidOperationException("Don't forget to call " +
                    "WinFormsIntegrationExtensions.SetContainer first in " +
                    "the program's Main method.");
            }

            container.InjectProperties(instance);
        }
    }
}