namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=Windows%20Forms%20Integration
    using System;
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
                throw new InvalidOperationException("Don't forget to call " +
                    "WinFormsIntegrationExtensions.SetContainer first in " +
                    "the program's Main method.");
            }

            container.InjectProperties(instance);
        }
    }
}