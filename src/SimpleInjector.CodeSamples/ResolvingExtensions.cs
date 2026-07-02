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

        public static bool TryGetInstance<T>(this Container container, out T instance) where T : class
        {
            if (container.TryGetInstance(typeof(T), out var result))
            {
                instance = (T)result;
                return true;
            }
            else
            {
                instance = null;
                return false;
            }
        }
    }
}