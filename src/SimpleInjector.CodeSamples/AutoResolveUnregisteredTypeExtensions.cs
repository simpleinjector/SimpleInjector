namespace SimpleInjector.CodeSamples
{
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
        public static void AutoResolveUnregisteredTypes(this Container container, params Assembly[] assemblies)
        {
            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType.IsAbstract)
                {
                    var types = container.GetTypesToRegister(e.UnregisteredServiceType, assemblies).ToArray();

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