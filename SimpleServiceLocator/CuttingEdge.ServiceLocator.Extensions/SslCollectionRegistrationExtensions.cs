namespace CuttingEdge.ServiceLocator.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CuttingEdge.ServiceLocation;

    public static class SslCollectionRegistrationExtensions
    {
        private interface IResolve
        {
            SimpleServiceLocator Container { get; set; }

            object GetInstance();
        }

        public static void AllowToResolveArrays(this SimpleServiceLocator container)
        {
            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType.IsArray)
                {
                    // Allow an IEnumerable<T> to be resolved as T[].
                    Type elementType = e.UnregisteredServiceType.GetElementType();
                    RegisterArrayResolver(e, container, elementType);
                }
                else if (e.UnregisteredServiceType.IsGenericType &&
                    e.UnregisteredServiceType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    // Allow an IEnumerable<T> to be resolved as IList<T>.
                    Type elementType = e.UnregisteredServiceType.GetGenericArguments()[0];
                    RegisterArrayResolver(e, container, elementType);
                }
            };
        }

        private static void RegisterArrayResolver(UnregisteredTypeEventArgs e, 
            SimpleServiceLocator container, Type elementType)
        {
            IResolve resolver = CreateArrayResolver(elementType);
            resolver.Container = container;
            e.Register(() => resolver.GetInstance());
        }

        private static IResolve CreateArrayResolver(Type elementType)
        {
            return Activator.CreateInstance(typeof(ArrayResolver<>)
                .MakeGenericType(elementType)) as IResolve;
        }

        private sealed class ArrayResolver<T> : IResolve
        {
            public SimpleServiceLocator Container { get; set; }

            public object GetInstance()
            {
                // We must call .ToArray() on each request. We can not cache it
                // because: 1. The collection might change, and; 2. Changing the
                // contents of this returned array would have a global effect.
                return this.Container.GetAllInstances<T>().ToArray();
            }
        }
    }
}