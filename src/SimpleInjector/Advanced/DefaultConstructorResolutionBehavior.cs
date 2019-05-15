// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    [DebuggerDisplay(nameof(DefaultConstructorResolutionBehavior))]
    internal sealed class DefaultConstructorResolutionBehavior : IConstructorResolutionBehavior
    {
        public ConstructorInfo GetConstructor(Type implementationType)
        {
            Requires.IsNotNull(implementationType, nameof(implementationType));

            VerifyTypeIsConcrete(implementationType);

            return GetSinglePublicConstructor(implementationType);
        }

        private static void VerifyTypeIsConcrete(Type implementationType)
        {
            if (!Types.IsConcreteType(implementationType))
            {
                throw new ActivationException(
                    StringResources.TypeShouldBeConcreteToBeUsedOnThisMethod(implementationType));
            }
        }

        private static ConstructorInfo GetSinglePublicConstructor(Type implementationType)
        {
            var constructors = implementationType.GetConstructors();

            if (constructors.Length == 0)
            {
                throw new ActivationException(
                    StringResources.TypeMustHaveASinglePublicConstructorButItHasNone(implementationType));
            }

            if (constructors.Length > 1)
            {
                throw new ActivationException(
                    StringResources.TypeMustHaveASinglePublicConstructorButItHas(implementationType,
                        constructors.Length));
            }

            return constructors[0];
        }
    }
}