namespace SimpleInjector.Tests.Unit
{
    using System.Reflection;

    internal static class ContainerFactory
    {
        public static Container New()
        {
            var container = new Container();

#if DEBUG
            container.Options.EnableDynamicAssemblyCompilation = true;
#endif

            return container;
        }
    }
}