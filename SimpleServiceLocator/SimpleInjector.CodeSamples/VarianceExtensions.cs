using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleInjector.CodeSamples
{
    public static class VarianceExtensions
    {
        /// <summary>
        /// When this method is called on a container, it allows the container to map an unregistered 
        /// requested (interface or delegate) type to an assignable and (interface or delegate) type that has
        /// been registered in the container. When there are multiple compatible types, an 
        /// <see cref="ActivationException"/> will be thrown.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        public static void AllowToResolveVariantTypes(this Container container)
        {
            container.ResolveUnregisteredType += (sender, e) =>
            {
                Type serviceType = e.UnregisteredServiceType;

                if (!serviceType.IsGenericType)
                {
                    return;
                }

                var registrations = FindAssignableRegistrations(container, serviceType);

                if (registrations.Length == 0)
                {
                    // No registration found. We're done.
                }
                else if (registrations.Length == 1)
                {
                    // Exactly one registration. Let's map the registration to the unregistered service type.
                    var registration = registrations[0];
                    e.Register(() => registration.GetInstance());
                }
                else if (registrations.Length > 1)
                {
                    var names =
                        string.Join(", ", registrations.Select(r => string.Format("{0}", r.ServiceType)));

                    throw new ActivationException(string.Format("There is an error in the container's " +
                        "contiguration. It is impossible to resolve type {0}, because there are " +
                        "{1} registrations that are applicable. Ambiguous registrations: {2}.",
                        serviceType, registrations.Length, names));
                }
            };
        }

        /// <summary>
        /// When this method is called on a container, it allows the container to map an unregistered 
        /// requested collection of a given (interface or delegate) type to all assignable (interface or 
        /// delegate) types registered in the container.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        public static void AllowToResolveVariantCollections(this Container container)
        {
            container.ResolveUnregisteredType += (sender, e) =>
            {
                // Only handle IEnumerable<T>.
                if (!IsGenericEnumerable(e.UnregisteredServiceType))
                {
                    return;
                }

                Type serviceType = e.UnregisteredServiceType.GetGenericArguments()[0];

                if (!serviceType.IsGenericType)
                {
                    return;
                }

                var registrations = FindAssignableRegistrations(container, serviceType);

                if (registrations.Length == 0)
                {
                    // No registration found. We're done.
                }
                else
                {
                    var instances = registrations.Select(r => r.GetInstance());

                    var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(serviceType);

                    var castedInstances = castMethod.Invoke(null, new[] { instances });

                    e.Register(() => castedInstances);
                }
            };
        }

        private static IInstanceProducer[] FindAssignableRegistrations(Container container, Type serviceType)
        {
            Type serviceTypeDefinition = serviceType.GetGenericTypeDefinition();

            return (
                from registration in container.GetCurrentRegistrations()
                where registration.ServiceType.IsGenericType
                where registration.ServiceType.GetGenericTypeDefinition() == serviceTypeDefinition
                where serviceType.IsAssignableFrom(registration.ServiceType)
                select registration)
                .ToArray();
        }

        private static bool IsGenericEnumerable(Type serviceType)
        {
            return serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }
    }
} 