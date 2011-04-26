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
                    "The supplied type '{0}' is an open generic type.", type),
                    paramName);
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

        internal static void NoDuplicateRegistrations(Type serviceType, IEnumerable<Type> typesToRegister)
        {
            var invalidTypes = (
                from type in typesToRegister
                from service in type.GetBaseTypesAndInterfaces(serviceType)
                group type by service into g
                where g.Count() > 1
                select new { ClosedType = g.Key, Duplicates = g.ToArray() }).FirstOrDefault();

            if (invalidTypes != null)
            {
                var typeDescription = string.Join(", ", (
                    from type in invalidTypes.Duplicates
                    select string.Format(CultureInfo.InvariantCulture, "'{0}'", type)).ToArray());

                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "There are {0} types that represent the closed generic type '{1}'. Types: {2}.",
                    invalidTypes.Duplicates.Length, invalidTypes.ClosedType, typeDescription));
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

        internal static void IsConcreteType(Type concreteType, string paramName)
        {
            if (!Helpers.IsConcreteType(concreteType))
            {
                throw new ArgumentException(string.Format(CultureInfo.InstalledUICulture,
                    "{0} is not a concrete type.", concreteType), paramName);
            }
        }
    }
}