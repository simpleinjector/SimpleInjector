namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;
    
    /// <summary>
    /// Extension methods for registering types on a thread-static basis.
    /// </summary>
    public static partial class PerThreadRegistrationsExtensions
    {
        // There are two ways of creating a Per-Thread lifestyle. The easy way is through
        // the Lifestyle.CreateCustom method; the hard way is to implement a custom Lifestyle.
        // This is the easy way.
        private static readonly Lifestyle PerThreadLifestyle = Lifestyle.CreateCustom("Thread", 
            transientInstanceCreator =>
            {
                var local = new ThreadLocal<object>();
                return () => local.Value ?? (local.Value = transientInstanceCreator());
            });

        public static void RegisterPerThread<TService, TImplementation>(
            this Container container)
            where TService : class
            where TImplementation : class, TService
        {
            container.Register<TService, TImplementation>(PerThreadLifestyle);
        }

        public static void RegisterPerThread<TService>(this Container container,
            Func<TService> instanceCreator) where TService : class
        {
            container.Register<TService>(instanceCreator, PerThreadLifestyle);           
        }
    }
}