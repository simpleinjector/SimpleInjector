using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace SimpleInjector.Extensions
{
    /// <summary>
    /// Internal helper class for precondition validation.
    /// </summary>
    internal static class Requires
    {
        internal static void IsNotNull(object instance, string paramName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        internal static void DoesNotContainNullValues<T>(IEnumerable<T> collection, string paramName)
            where T : class
        {
            if (collection != null && collection.Contains(null))
            {
                throw new ArgumentException("The collection contains null elements.", paramName);
            }
        }

        internal static void IsValidValue(AccessibilityOption accessibility, string paramName)
        {
            if (accessibility != AccessibilityOption.AllTypes &&
                accessibility != AccessibilityOption.PublicTypesOnly)
            {
                throw new InvalidEnumArgumentException(paramName, (int)accessibility,
                    typeof(AccessibilityOption));
            }
        }

        internal static void TypeIsOpenGeneric(Type type, string paramName)
        {
            if (!type.IsGenericTypeDefinition)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The supplied type '{0}' is not an open generic type.", type),
                    paramName);
            }
        }

        internal static void TypeIsNotOpenGeneric(Type type, string paramName)
        {
            if (type.IsGenericTypeDefinition)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The supplied type '{0}' is an open generic type. Use the RegisterOpenGeneric or " +
                    "RegisterManyForOpenGeneric extension method for registering open generic types.", type),
                    paramName);
            }
        }

        internal static void DoesNotContainOpenGenericTypes(IEnumerable<Type> serviceTypes, string paramName)
        {
            foreach (var type in serviceTypes)
            {
                TypeIsNotOpenGeneric(type, paramName);
            }
        }

        internal static void ServiceIsAssignableFromImplementation(Type service, Type implementation,
            string paramName)
        {
            if (service != implementation && 
                !Helpers.ServiceIsAssignableFromImplementation(service, implementation))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The supplied type '{0}' does not inherit from or implement '{1}'.",
                    implementation, service),
                    paramName);
            }
        }

        internal static void ServiceIsAssignableFromImplementations(Type serviceType, 
            IEnumerable<Type> typesToRegister, string paramName)
        {
            var invalidType = (
                from type in typesToRegister
                where !Helpers.ServiceIsAssignableFromImplementation(serviceType, type)
                select type).FirstOrDefault();

            if (invalidType != null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The supplied type '{0}' does not implement '{1}'.", invalidType, serviceType),
                    paramName);
            }
        }

        internal static void ServiceTypeDiffersFromImplementationType(Type serviceType, Type implementation, 
            string paramName, string implementationParamName)
        {
            if (serviceType == implementation)
            {
                throw new ArgumentException(paramName + " and " + implementationParamName + 
                    " must be different types.", paramName);
            }
        }
    }
}