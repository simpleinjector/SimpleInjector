namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Threading;
    using SimpleInjector;

    // There is no support for a Thread lifestyle in the core library, because this lifestyle is considered
    // harmful. It should not be used in web applications, because ASP.NET can finish a request on a different
    // thread. This can cause a Per Thread instance to be used from another thread, which can cause all sorts 
    // of race conditions. Even letting transient component depend on a per-thread component can cause trouble.
    // Instead of using Per Thread lifestyle, use ThreadScopedLifestyle instead.
    public static class PerThreadRegistrationsExtensions
    {
        // There are two ways of creating a Per-Thread lifestyle. The easy way is through
        // the Lifestyle.CreateCustom method; the hard way is to implement a custom Lifestyle.
        // This is the easy way.
        private static readonly Lifestyle PerThreadLifestyle = Lifestyle.CreateCustom("Thread", creator =>
        {
            var local = new ThreadLocal<object>();
            return () => local.Value ?? (local.Value = creator());
        });

        public static void RegisterPerThread<TService, TImplementation>(this Container container)
            where TService : class
            where TImplementation : class, TService
        {
            container.Register<TService, TImplementation>(PerThreadLifestyle);
        }

        public static void RegisterPerThread<TService>(this Container container,
            Func<TService> instanceCreator) where TService : class
        {
            container.Register(instanceCreator, PerThreadLifestyle);           
        }
    }
}