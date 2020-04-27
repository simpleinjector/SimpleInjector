namespace SimpleInjector.Tests.Unit
{
    internal static class ContainerFactory
    {
        public static Container New()
        {
            var container = new Container();

            container.Options.EnableDynamicAssemblyCompilation = true;
            container.Options.EnableAutoVerification = false;

            // Makes writing tests easier.
            container.Options.ResolveUnregisteredConcreteTypes = true;

            return container;
        }
    }
}