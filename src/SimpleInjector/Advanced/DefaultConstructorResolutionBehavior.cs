#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2016 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    [DebuggerDisplay(nameof(DefaultConstructorResolutionBehavior))]
    internal sealed class DefaultConstructorResolutionBehavior : IConstructorResolutionBehavior
    {
        // NOTE: The serviceType parameter is not used in the default implementation, but can be used by
        // alternative implementations to generate a proxy type based on the service type and return a
        // constructor of that proxy instead of returning a constructor of the implementationType.
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
                // About arrays: While array types are in fact concrete, we cannot create them and creating 
                // them would be pretty useless.
                // About object: System.Object is concrete and even contains a single public (default) 
                // constructor. Allowing it to be created however, would lead to confusion, since this allows
                // injecting System.Object into constructors, even though it is not registered explicitly.
                // This is bad, since creating an System.Object on the fly (transient) has no purpose and this
                // could lead to an accidentally valid container configuration, while there is in fact an
                // error in the configuration.
                throw new ActivationException(
                    StringResources.TypeShouldBeConcreteToBeUsedOnThisMethod(implementationType));
            }
        }

        private static ConstructorInfo GetSinglePublicConstructor(Type implementationType)
        {
            var constructors = implementationType.GetConstructors();

            if (!constructors.Any())
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