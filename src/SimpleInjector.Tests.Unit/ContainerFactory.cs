namespace SimpleInjector.Tests.Unit
{
    using System.Reflection;

    internal static class ContainerFactory
    {
        public static Container New()
        {
            var container = new Container();

            container.Options.EnableDynamicAssemblyCompilation = true;

            return container;
        }
    }
}