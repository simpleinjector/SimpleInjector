// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Reflection;

    internal static class Requires
    {
        internal static void IsNotNull(object instance, string paramName)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        internal static void IsNotOpenGenericType(Type type, string paramName)
        {
            // We check for ContainsGenericParameters to see whether there is a Generic Parameter
            // to find out if this type can be created.
            if (type.GetTypeInfo().ContainsGenericParameters)
            {
                throw new ArgumentException(
                    $"The supplied type {type.FullName} is an open-generic type. " +
                    "This type cannot be used for registration using this method.",
                    paramName);
            }
        }

        internal static void ServiceIsAssignableFromImplementation(
            Type service, Type implementation, string paramName)
        {
            if (!service.IsAssignableFrom(implementation))
            {
                ThrowSuppliedTypeDoesNotInheritFromOrImplement(service, implementation, paramName);
            }
        }

        private static void ThrowSuppliedTypeDoesNotInheritFromOrImplement(
            Type service, Type implementation, string paramName)
        {
            var implementOrInherit = service.GetTypeInfo().IsInterface ? "implement" : "inherit from";

            throw new ArgumentException(
                $"The supplied type {implementation.FullName} does not {implementOrInherit} " +
                $"from {service.FullName}.",
                paramName);
        }
    }
}