namespace SimpleInjector.CodeSamples.Tests.Unit
{
    internal static class ContainerFactory
    {
        public static Container New()
        {
            var container = new Container();

            container.Options.EnableDynamicAssemblyCompilation();
            container.Options.EnableAutoVerification = false;

            return container;
        }
    }
}