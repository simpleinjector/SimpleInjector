// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    [DebuggerDisplay(nameof(DefaultConstructorResolutionBehavior))]
    internal sealed class DefaultConstructorResolutionBehavior : IConstructorResolutionBehavior
    {
        public ConstructorInfo? TryGetConstructor(Type implementationType, out string? errorMessage)
        {
            Requires.IsNotNull(implementationType, nameof(implementationType));

            VerifyTypeIsConcrete(implementationType);

            var constructors = implementationType.GetConstructors();

            if (constructors.Length == 0)
            {
                errorMessage =
                    StringResources.TypeMustHaveASinglePublicConstructorButItHasNone(implementationType);

                return null;
            }
            else if (constructors.Length > 1)
            {
                errorMessage =
                    StringResources.TypeMustHaveASinglePublicConstructorButItHas(
                        implementationType, constructors.Length);

                return null;
            }
            else
            {
                errorMessage = null;
                return constructors[0];
            }
        }

        private static void VerifyTypeIsConcrete(Type implementationType)
        {
            if (!Types.IsConcreteType(implementationType))
            {
                throw new ActivationException(
                    StringResources.TypeShouldBeConcreteToBeUsedOnThisMethod(implementationType));
            }
        }
    }
}