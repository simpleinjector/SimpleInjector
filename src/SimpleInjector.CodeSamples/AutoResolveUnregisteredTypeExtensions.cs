namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class AutoResolveUnregisteredTypeExtensions
    {
        /// <summary>
        /// Automatically resolves an implementation of an unregistered abstraction, by searching the supplied
        /// list of assemblies. The implementation will be resolved only if the list of assemblies contains
        /// exactly one implementation of the missing abstraction.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assemblies">The list of assemblies to search for implementations.</param>
        public static void AutoResolveMatchingImplementation(this Container container, params Assembly[] assemblies)
        {
            var options = new TypesToRegisterOptions
            {
                IncludeDecorators = false,
                IncludeGenericTypeDefinitions = false,
                IncludeComposites = false
            };

            container.ResolveUnregisteredType += (s, e) =>
            {
                Type serviceType = e.UnregisteredServiceType;

                if (serviceType.IsAbstract)
                {
                    var types = container.GetTypesToRegister(serviceType, assemblies, options).ToArray();

                    // Only map when there is no ambiguity, meaning: exactly one implementation.
                    if (types.Length == 1)
                    {
                        e.Register(container.Options.DefaultLifestyle.CreateRegistration(types[0], container));
                    }
                }
            };
        }

        /// <summary>
        /// Automatically resolves an implementation of an unregistered abstraction, by searching the supplied
        /// list of assemblies for a type with the identical name as the interface, but without the 'I'. 
        /// In other words, if an IProductService is being resolved, the concrete type with name "ProductService"
        /// will be registered.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assemblies">The list of assemblies to search for implementations.</param>
        public static void AutoResolveDefaultImplementation(this Container container, params Assembly[] assemblies)
        {
            container.ResolveUnregisteredType += (s, e) =>
            {
                Type serviceType = e.UnregisteredServiceType;

                if (serviceType.IsAbstract && !serviceType.IsGenericType && serviceType.Name.StartsWith("I"))
                {
                    string implementationName = serviceType.Name.Substring(1);

                    var types = container.GetTypesToRegister(serviceType, assemblies)
                        .Where(t => t.Name == implementationName)
                        .ToArray();

                    // Only map when there is no ambiguity, meaning: exactly one implementation.
                    if (types.Length == 1)
                    {
                        e.Register(container.Options.DefaultLifestyle.CreateRegistration(types[0], container));
                    }
                }
            };
        }
    }
}