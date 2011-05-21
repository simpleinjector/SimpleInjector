#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2010 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
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

using System;
using System.Globalization;
using System.Reflection;

namespace SimpleInjector
{
    /// <summary>Internal helper for string resources.</summary>
    internal static class StringResources
    {
        internal static string ContainerCanNotBeChangedAfterUse(Type containerType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The {0} can't be changed after the first call to GetInstance and GetAllInstances.",
                containerType.Name);
        }

        internal static string DelegateForTypeReturnedNull(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The registered delegate for type {0} returned null.", serviceType);
        }

        internal static string ErrorWhileTryingToGetInstanceOfType(Type serviceType, Exception exception)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Error occurred while trying to get instance of type {0}. {1}",
                serviceType, exception.Message);
        }
           
        internal static string DelegateForTypeThrewAnException(Type serviceType, Exception exception)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The registered delegate for type {0} threw an exception. {1}",
                serviceType, exception.Message);
        }

        internal static string NoRegistrationForTypeFound(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "No registration for type {0} could be found.", serviceType);
        }

        internal static string ConfigurationInvalidCreatingInstanceFailed(Type type, Exception exception)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. Creating the instance for type {0} failed. {1}",
                type, exception.Message);
        }

        internal static string ConfigurationInvalidIteratingCollectionFailed(Type type, Exception exception)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. Iterating the collection for type {0} failed. {1}",
                type, exception.Message);
        }

        internal static string ConfigurationInvalidCollectionContainsNullElements(Type firstInvalidType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. One of the items in the collection for type {0} is " +
                "a null reference.", firstInvalidType);
        }

        internal static string TypeAlreadyRegistered(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Type {0} has already been registered.", serviceType);
        }

        internal static string CollectionTypeAlreadyRegistered(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Collection of items for type {0} has already been registered. A collection of items can " +
                "only be registered once per type.", serviceType);
        }

        internal static string RegisteredDelegateForTypeReturnedNull(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The registered delegate for type {0} returned null.", serviceType);
        }

        internal static string ParameterTypeMustBeRegistered(Type serviceType, Type parameterType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                ImplicitRegistrationCouldNotBeMadeForType(serviceType) +
                    "The constructor of the type contains the parameter of type {0} that is not registered. " +
                    "Please ensure {1} is registered in the container, change the constructor of the " +
                    "type or register the type {2} directly.",
                    parameterType, parameterType.Name, serviceType.Name);
        }

        internal static string TypeMustHaveASinglePublicConstructor(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to create {0}, it should contain exactly one public " + 
                "constructor, but it has {1}.",
                serviceType, serviceType.GetConstructors().Length);
        }

        internal static string ConstructorMustNotContainInvalidParameter(Type serviceType, 
            ParameterInfo invalidParameter)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The constructor of type {0} contains parameter '{1}' of type {2} which can not be used " +
                "for constructor injection.", serviceType, invalidParameter.Name, 
                invalidParameter.ParameterType);
        }

        internal static string TypeShouldBeConcreteToBeUsedOnThisMethod(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The given type {0} is not a concrete type. Please use one of the other overloads to " +
                "register this type.", serviceType);
        }

        internal static string MultipleObserversRegisteredTheSameType(Type unregisteredServiceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Multiple observers of the ResolveUnregisteredType event are registering a delegate for " +
                "the same service type: {0}. Make sure only one of the registered handlers calls the " +
                "ResolveUnregisteredType.Register(Func<object>) method for a given service type.",
                unregisteredServiceType);
        }

        internal static string HandlerReturnedADelegateThatThrewAnException(Type serviceType, 
            string innerExceptionMessage)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The delegate that was hooked to the ResolveUnregisteredType event and responded " +
                "to the {0} service type, registered a delegate that threw an exception. {1}",
                serviceType, innerExceptionMessage);
        }
        
        internal static string HandlerReturnedADelegateThatReturnedNull(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The delegate that was hooked to the ResolveUnregisteredType event and responded " +
                "to the {0} service type, registered a delegate that returned a null reference.", serviceType);
        }

        internal static string HandlerReturnedDelegateThatReturnedAnUnassignableFrom(Type serviceType,
            Type actualType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The delegate that was hooked to the ResolveUnregisteredType event and responded " +
                "to the {0} service type, registered a delegate that created an instance of type {1} that " +
                "can not be cast to the specified service type.", serviceType, actualType);
        }

        internal static string ImplicitRegistrationCouldNotBeMadeForType(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "No registration for type {0} could be found and an implicit registration could not be made. ",
                serviceType);
        }

        internal static string TypeDependsOnItself(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. The type {0} is directly or indirectly depending on itself.",
                serviceType);
        }

        internal static string UnableToResolveTypeDueToSecurityConfiguration(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Unable to resolve type {0}. The security restrictions of your application's sandbox do " +
                "not permit the creation of this type. Explicitly register the type using " + 
                "'container.Register<{1}>()' or consider making it public.", serviceType, serviceType.Name);
        }
    }
}