namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System.Reflection;

    internal static class ContainerFactory
    {
        public static Container New()
        {
            var container = new Container();

            typeof(ContainerOptions).GetProperty("EnableDynamicAssemblyCompilation", 
                BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(container.Options, true);

            return container;
        }
    }
}