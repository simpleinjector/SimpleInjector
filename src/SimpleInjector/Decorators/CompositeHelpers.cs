// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Decorators
{
    using System;
    using System.Linq;
    using System.Reflection;

    internal static class CompositeHelpers
    {
        internal static bool ComposesServiceType(Type serviceType, ConstructorInfo constructor) =>
            GetNumberOfCompositeServiceTypeDependencies(serviceType, constructor) > 0;

        private static int GetNumberOfCompositeServiceTypeDependencies(
            Type serviceType, ConstructorInfo compositeConstructor)
        {
            Type compositeServiceType = GetCompositeBaseType(serviceType, compositeConstructor);

            if (compositeServiceType == null)
            {
                return 0;
            }

            var validServiceTypeParameters =
                from parameter in compositeConstructor.GetParameters()
                where IsCompositeParameter(parameter, compositeServiceType)
                select parameter;

            return validServiceTypeParameters.Count();
        }

        // Returns the base type of the composite that can be used for decoration (because serviceType might
        // be open generic, while the base type might not be).
        private static Type GetCompositeBaseType(Type serviceType, ConstructorInfo compositeConstructor)
        {
            // This list can only contain serviceType and closed and partially closed versions of serviceType.
            var baseTypeCandidates =
                Types.GetBaseTypeCandidates(serviceType, compositeConstructor.DeclaringType);

            var compositeInterfaces =
                from baseTypeCandidate in baseTypeCandidates
                where ContainsCompositeParameters(compositeConstructor, baseTypeCandidate)
                select baseTypeCandidate;

            return compositeInterfaces.FirstOrDefault();
        }

        private static bool ContainsCompositeParameters(
            ConstructorInfo compositeConstructor, Type serviceType) =>
            compositeConstructor.GetParameters()
                .Any(parameter => IsCompositeParameter(parameter, serviceType));

        private static bool IsCompositeParameter(ParameterInfo parameter, Type serviceType) =>
            Types.IsGenericCollectionType(parameter.ParameterType)
                && parameter.ParameterType.GetGenericArguments()[0] == serviceType;
    }
}