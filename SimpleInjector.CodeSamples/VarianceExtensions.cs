﻿namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Globalization;
    using System.Linq;
    using SimpleInjector;

    // https://simpleinjector.readthedocs.org/en/latest/varianceextensions.html
    public static class VarianceExtensions
    {
        /// <summary>
        /// When this method is called on a container, it allows the container to map an 
        /// unregistered requested (interface or delegate) type to an assignable and
        /// (interface or delegate) type that has been registered in the container. When 
        /// there are multiple compatible types, an <see cref="ActivationException"/> will
        /// be thrown.
        /// </summary>
        /// <param name="ContainerOptions">The options to make the registrations in.</param>
        public static void AllowToResolveVariantTypes(this ContainerOptions options)
        {
            var container = options.Container;
            container.ResolveUnregisteredType += (sender, e) =>
            {
                Type serviceType = e.UnregisteredServiceType;

                if (!serviceType.IsGenericType || e.Handled)
                {
                    return;
                }

                var registrations = FindAssignableRegistrations(container, serviceType);

                if (!registrations.Any())
                {
                    // No registration found. We're done.
                }
                else if (registrations.Length == 1)
                {
                    // Exactly one registration. Let's map the registration to the 
                    // unregistered service type.
                    e.Register(registrations[0].Registration);
                }
                else
                {
                    var names = string.Join(", ", 
                        registrations.Select(r => string.Format("{0}", r.ServiceType)));

                    throw new ActivationException(string.Format(CultureInfo.CurrentCulture,
                        "There is an error in the container's configuration. It is impos" + 
                        "sible to resolve type {0}, because there are {1} registrations " + 
                        "that are applicable. Ambiguous registrations: {2}.",
                        serviceType, registrations.Length, names));
                }
            };
        }

        private static InstanceProducer[] FindAssignableRegistrations(Container container, 
            Type serviceType)
        {
            Type serviceTypeDefinition = serviceType.GetGenericTypeDefinition();

            return (
                from reg in container.GetCurrentRegistrations()
                where reg.ServiceType.IsGenericType
                where reg.ServiceType.GetGenericTypeDefinition() == serviceTypeDefinition
                where serviceType.IsAssignableFrom(reg.ServiceType)
                select reg)
                .ToArray();
        }
    }
} 