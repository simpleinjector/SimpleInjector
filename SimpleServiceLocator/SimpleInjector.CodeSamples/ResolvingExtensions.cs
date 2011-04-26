namespace SimpleInjector.CodeSamples
{
    // Do not include this extension method in the release. It will be put on the project's wiki page. This
    // way we can still unit test this code example.
    using System;

    using SimpleInjector;

    /// <summary>
    /// Extension methods for resolving instances.
    /// </summary>
    public static class ResolvingExtensions
    {
        public static bool TryGetInstance<T>(this Container container, out T instance)
        {
            IServiceProvider provider = container;

            instance = (T)provider.GetService(typeof(T));

            return instance != null;
        }

        public static bool TryGetInstance(this Container container, Type type, out object instance)
        {
            IServiceProvider provider = container;

            instance = provider.GetService(type);

            return instance != null;
        }
    }
}