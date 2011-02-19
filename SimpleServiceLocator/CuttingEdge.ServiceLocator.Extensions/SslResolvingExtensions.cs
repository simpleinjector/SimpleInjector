using System;
using CuttingEdge.ServiceLocation;

namespace CuttingEdge.ServiceLocator.Extensions
{
    /// <summary>
    /// Extension methods for resolving instances.
    /// </summary>
    public static class SslResolvingExtensions
    {
        // NOTE: This is an extension method on SimpleServiceLocator and not on IServiceProvider. While this
        // would work when using it on the Simple Service Locator, it will fail on all the adapters of the
        // Common Service Locator, because of an design flaw in the ServiceLocatorImplBase that most adapters
        // implement. For more details, see: http://commonservicelocator.codeplex.com/discussions/44293.
        public static bool TryGetInstance<T>(this SimpleServiceLocator container, out T instance)
        {
            IServiceProvider provider = container;

            instance = (T)provider.GetService(typeof(T));

            return instance != null;
        }
    }
}