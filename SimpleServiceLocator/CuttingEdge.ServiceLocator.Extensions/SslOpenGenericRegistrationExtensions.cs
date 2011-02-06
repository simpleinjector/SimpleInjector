namespace CuttingEdge.ServiceLocator.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using CuttingEdge.ServiceLocation;

    public static class SslOpenGenericRegistrationExtensions
    {
        public static void RegisterOpenGeneric(this SimpleServiceLocator container,
            Type openGenericServiceType, Type openGenericImplementation)
        {
            ThrowWhenTypeIsNotOpenGenericType(openGenericServiceType, "openGenericServiceType");
            ThrowWhenTypeIsNotOpenGenericType(openGenericImplementation, "openGenericImplementation");
            ThrowWhenImplementationDoesNotDeriveFromService(openGenericServiceType, openGenericImplementation);

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType.IsGenericType &&
                    e.UnregisteredServiceType.GetGenericTypeDefinition() == openGenericServiceType)
                {
                    var closedGenericImplementation = openGenericImplementation.MakeGenericType(
                        e.UnregisteredServiceType.GetGenericArguments());

                    e.Register(() => container.GetInstance(closedGenericImplementation));
                }
            };
        }

        private static void ThrowWhenImplementationDoesNotDeriveFromService(Type service, Type implementation)
        {
            var baseTypes = implementation.GetBaseTypesAndInterfaces();

            if (!baseTypes.Any(t => t.ContainsGenericParameters && t.GetGenericTypeDefinition() == service))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The supplied type '{0}' does not inherit from or implement '{1}'.",
                    implementation, service),
                    "openGenericImplementation");
            }
        }

        private static void ThrowWhenTypeIsNotOpenGenericType(Type openGenericType, string paramName)
        {
            if (!openGenericType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The supplied type '{0}' is not an open generic type.", openGenericType),
                    paramName);
            }
        }

        private static IEnumerable<Type> GetBaseTypesAndInterfaces(this Type type)
        {
            return type.GetInterfaces().Concat(type.GetBaseTypes());
        }

        private static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            Type baseType = type.BaseType;

            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }
    }
}