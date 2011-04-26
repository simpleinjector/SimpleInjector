namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SimpleInjector;

    public static class CollectionRegistrationExtensions
    {
        private interface IArrayResolver
        {
            Container Container { get; set; }

            object GetInstance();
        }

        public static void AllowToResolveArrays(this Container container)
        {
            container.ResolveUnregisteredType += (sender, e) =>
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
            Container container, Type elementType)
        {
            IArrayResolver resolver = CreateArrayResolver(elementType);
            resolver.Container = container;
            e.Register(() => resolver.GetInstance());
        }

        private static IArrayResolver CreateArrayResolver(Type elementType)
        {
            return Activator.CreateInstance(typeof(ArrayResolver<>)
                .MakeGenericType(elementType)) as IArrayResolver;
        }

        private sealed class ArrayResolver<T> : IArrayResolver
        {
            public Container Container { get; set; }

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