namespace SimpleInjector.CodeSamples
{
    using System;
    using SimpleInjector;

    /// <summary>
    /// Extension methods for resolving instances.
    /// </summary>
    public static class ResolvingExtensions
    {
        public static bool CanGetInstance<T>(this Container container) => 
            container.GetRegistration(typeof(T)) != null;

        public static bool CanGetInstance(this Container container, Type serviceType) => 
            container.GetRegistration(serviceType) != null;

        public static bool TryGetInstance<T>(this Container container, out T instance)
        {
            IServiceProvider provider = container;
            instance = (T)provider.GetService(typeof(T));
            return instance != null;
        }

        public static bool TryGetInstance(this Container container, Type serviceType, out object instance)
        {
            IServiceProvider provider = container;
            instance = provider.GetService(serviceType);
            return instance != null;
        }
    }
}