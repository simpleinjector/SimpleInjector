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

namespace SimpleInjector.Extensions
{
    using System;
    using System.Globalization;
    using System.Linq;

    /// <summary>Internal helper for string resources.</summary>
    internal static class StringResources
    {
        // Gets called when the user tries to resolve an internal type inside a (Silverlight) sandbox.
        internal static string UnableToResolveTypeDueToSecurityConfiguration(Type serviceType,
            Exception innerException)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Unable to register type {0}. The security restrictions of your application's sandbox do " +
                "not permit the creation of this type. Explicitly register the type using one of the " +
                "generic Register overloads or consider making it public. {1}", serviceType,
                innerException.Message);
        }

        internal static string ErrorWhileTryingToGetInstanceOfType(Type serviceType, string message)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Error occurred while trying to get an instance of type {0}. {1}",
                serviceType.ToFriendlyName(), message);
        }

        internal static string TypeMustHaveASinglePublicConstructor(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to create {0}, it should contain exactly one public " +
                "constructor, but it has {1}.",
                serviceType.ToFriendlyName(), serviceType.GetConstructors().Length);
        }

        internal static string TheConstructorOfTypeMustContainTheServiceTypeAsArgument(Type decoratorType,
            Type serviceType, int numberOfServiceTypeDependencies)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to use {0} as a decorator, its constructor should have a " +
                "single argument of type {1}, but it currently has {2}.", decoratorType.ToFriendlyName(),
                serviceType.ToFriendlyName(), numberOfServiceTypeDependencies);
        }

        internal static string SuppliedTypeDoesNotInheritFromOrImplement(Type implementation, Type service)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied type '{0}' does not inherit from or implement '{1}'.",
                implementation.ToFriendlyName(), service.ToFriendlyName());
        }

        internal static string SuppliedTypeIsNotAReferenceType(Type type)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied type '{0}' is not a reference type. Only reference types are supported.",
                type.ToFriendlyName());
        }

        internal static string SuppliedTypeIsAnOpenGenericType(Type type)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied type '{0}' is an open generic type. Use the RegisterOpenGeneric or " +
                "RegisterManyForOpenGeneric extension method for registering open generic types.",
                type.ToFriendlyName());
        }

        internal static string SuppliedTypeIsNotAnOpenGenericType(Type type)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied type '{0}' is not an open generic type.", type.ToFriendlyName());
        }

        internal static string ValueIsInvalidForEnumType(int value, Type enumType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The value {0} is invalid for Enum-type {1}.",
                value, enumType.ToFriendlyName());
        }

        private static string ToFriendlyName(this Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }

            string name = type.Name.Substring(0, type.Name.IndexOf('`'));

            var genericArguments = type.GetGenericArguments().Select(argument => argument.ToFriendlyName());

            return name + "<" + string.Join(", ", genericArguments.ToArray()) + ">";
        }
    }
}