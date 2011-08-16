namespace SimpleInjector.CodeSamples
{
    using SimpleInjector;
    using SimpleInjector.Extensions;

    public static class ImplicitPropertyInjectionExtensions
    {
        public static void AllowImplicitPropertyInjection(this Container container)
        {
            container.AllowImplicitPropertyInjectionOn<object>();
        }

        public static void AllowImplicitPropertyInjectionOn<TService>(
            this Container container) where TService : class
        {
            container.RegisterInitializer<TService>(service => container.InjectProperties(service));
        }
    }
}