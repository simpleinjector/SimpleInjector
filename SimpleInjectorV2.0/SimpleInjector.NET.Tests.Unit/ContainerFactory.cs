namespace SimpleInjector.Tests.Unit
{
    using System.Reflection;

    internal static class ContainerFactory
    {
        public static Container New()
        {
            var container = new Container();

#if !SILVERLIGHT
            // This will flag the container as 'first', which does another compilation step which is important
            // to test.
            var id = typeof(Container).GetField("containerId", BindingFlags.Instance | BindingFlags.NonPublic);
            id.SetValue(container, 1);
#endif

            return container;
        }
    }
}