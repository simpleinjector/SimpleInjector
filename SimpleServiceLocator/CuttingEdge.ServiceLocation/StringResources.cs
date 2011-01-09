#region Copyright (c) 2010 S. van Deursen
/* The SimpleServiceLocator library is a simple but complete implementation of the CommonServiceLocator 
 * interface.
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

namespace CuttingEdge.ServiceLocation
{
    /// <summary>Internal helper for string resources.</summary>
    internal static class StringResources
    {
        internal static string KeyCanNotBeAnEmptyString
        {
            get { return "key can not be an empty string."; }
        }

        internal static string NoRegistrationFoundForKeyedType(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "No keyed registration for type {0} could be found.", serviceType.FullName);
        }

        internal static string SimpleServiceLocatorCanNotBeChangedAfterUse(Type simpleServiceLocatorType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The {0} can't be changed after the first call to GetInstance and GetAllInstances.",
                simpleServiceLocatorType.Name);
        }

        internal static string DelegateForTypeReturnedNull(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The registered delegate for type {0} returned null.", serviceType.FullName);
        }

        internal static string NoRegistrationForTypeFound(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "No registration for type {0} could be found.", serviceType.FullName);
        }

        internal static string ConfigurationInvalidCreatingInstanceFailed(Type type, Exception exception)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. Creating the instance for type {0} failed. {1}",
                type, exception.Message);
        }

        internal static string ConfigurationInvalidCreatingKeyedInstanceFailed(Type type, string key,
            Exception exception)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. Creating the instance for type {0} with key '{1}' failed. {2}",
                type, key, exception.Message);
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

        internal static string UnkeyedTypeAlreadyRegistered(Type serviceType)
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

        internal static string KeyForTypeNotFound(Type serviceType, string key)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The key '{0}' for type {1} could not be found.", key, serviceType);
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
                    parameterType.FullName, parameterType.Name, serviceType.Name);
        }

        internal static string TypeMustHaveASinglePublicConstructor(Type serviceType, int constructorCount)
        {
            return string.Format(CultureInfo.InvariantCulture,
                ImplicitRegistrationCouldNotBeMadeForType(serviceType) +
                    "The type should contain exactly one public constructor, but it currently has {0}.",
                    constructorCount);
        }

        internal static string TypeShouldBeConcreteToBeUsedOnRegisterSingle(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The given type {0} is not a concrete type. Please use one of the other " +
                    "RegisterSingle<T> overloads to register this type.",
                serviceType.FullName);
        }

        internal static string TypeAlreadyRegisteredUsingByKeyString(Type serviceType,
            string methodUsedForRegistration)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Type {0} has already been registered using {1}. A type can be " +
                "registered multiple times with different keys using RegisterSingleByKey<T>, but it can't " +
                "be mixed with methods that take an Func<string, T> delegate.", serviceType, 
                methodUsedForRegistration);
        }

        internal static string TypeAlreadyRegisteredWithKey(Type serviceType, string key)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Type {0} has already been registered with key '{1}'.", serviceType, key);
        }

        internal static string TypeAlreadyRegisteredUsingRegisterByKeyFuncStringT(Type serviceType)
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Type {0} has already been registered using RegisterByKey<T>(Func<string, T>). " +
                "A Func<string, T> can only be registered once for each type.", serviceType);
        }

        internal static string ForKeyTypeAlreadyRegisteredUsingRegisterByKeyFuncStringT(Type serviceType)
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Type {0} has already been registered using RegisterByKey<T>(Func<string, T>). " +
                "A registration with this method for a type can't be mixed with methods that register the " +
                "type using a string key.", serviceType);
        }

        internal static string TypeAlreadyRegisteredUsingRegisterSingleByKeyFuncStringT(
            Type serviceType)
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Type {0} has already been registered using RegisterSingleByKey<T>(Func<string, T>). " +
                "A Func<string, T> can only be registered once for each type.", serviceType);
        }

        internal static string ForKeyTypeAlreadyRegisteredUsingRegisterSingleByKeyFuncStringT(
            Type serviceType)
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Type {0} has already been registered using RegisterSingleByKey<T>(Func<string, T>). " +
                "A registration with this method for a type can't be mixed with methods that register the " +
                "type using a string key.", serviceType);
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
                "The delegate that that was hooked to the ResolveUnregisteredType event and responded " +
                "to the {0} service type, registered a delegate that returned a null reference.", serviceType);
        }

        internal static string HandlerReturnedDelegateThatReturnedAnUnassignableFrom(Type serviceType,
            Type actualType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The delegate that that was hooked to the ResolveUnregisteredType event and responded " +
                "to the {0} service type, registered a delegate that created an instance of type {1} that " +
                "can not be casted to the specified service type.", serviceType, actualType);
        }

        private static string ImplicitRegistrationCouldNotBeMadeForType(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "No registration for type {0} could be found and an implicit registration could not be made. ",
                serviceType.FullName);
        }
    }
}