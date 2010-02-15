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

namespace CuttingEdge.ServiceLocation
{
    /// <summary>Internal helper for string resources.</summary>
    internal static class StringResources
    {
        internal static string KeyCanNotBeAnEmptyString
        {
            get { return "key can not be an empty string."; }
        }

        internal static string NoRegistrationFoundForType(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "No registration for type {0} could be found.", serviceType.FullName);
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

        internal static string TypeAlreadyRegisteredForRegisterByKey(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Type {0} has already been registered by calling RegisterByKey<T>. Once the type is " +
                "registered by calling RegisterByKey<T>, registering it using RegisterSingleByKey<T> is " +
                "invalid.", serviceType.FullName);
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

        internal static string UnkeyedTypeAlreadyRegistered(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Type {0} has already been registered. Using Register<T> or RegisterOnce<T>, a type can " +
                "only be registered once.", serviceType);
        }

        internal static string CollectionTypeAlreadyRegistered(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Collection of items for type {0} has already been registered. A collection of items can " +
                "only be registered once per type.", serviceType);
        }

        internal static string TypeAlreadyRegisteredRegisterByKeyAlreadyCalled(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Type {0} has already been registered. " +
                "A type can only be registered once using RegisterByKey<T>.", serviceType);
        }

        internal static string TypeAlreadyRegisteredRegisterSingleByKeyAlreadyCalled(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Type {0} has already been registered using RegisterSingleByKey<T>. A type can be " +
                "registered multiple times with different keys using RegisterSingleByKey<T>, but it can " +
                "only be registered once using RegisterByKey<T>.", serviceType);
        }

        internal static string TypeAlreadyRegisteredWithKey(Type serviceType, string key)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Type {0} has already been registered with key '{1}'.", serviceType, key);
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

        private static string ImplicitRegistrationCouldNotBeMadeForType(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "No registration for type {0} could be found and an implicit registration could not be made. ",
                serviceType.FullName);
        }
    }
}