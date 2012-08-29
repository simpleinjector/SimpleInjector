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

        internal static string TheConstructorOfTypeMustContainTheServiceTypeAsArgument(Type decoratorType,
            Type serviceType, int numberOfServiceTypeDependencies)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to use {0} as a decorator, its constructor should have a " +
                "single argument of type {1} or Func<{1}>, but it currently has {2}.",
                decoratorType.ToFriendlyName(), serviceType.ToFriendlyName(), numberOfServiceTypeDependencies);
        }

        internal static string SuppliedTypeDoesNotInheritFromOrImplement(Type service, Type implementation)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied type '{0}' does not inherit from or implement '{1}'.",
                implementation.ToFriendlyName(), service.ToFriendlyName());
        }

        internal static string SuppliedTypeCanNotBeOpenWhenDecoratorIsClosed()
        {
            return
                "Registering a closed generic service type with an open generic decorator is not " +
                "supported. Instead, register the service type as open generic, and the decorator as " +
                "closed generic type.";
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

        internal static string DecoratorCanNotBeAGenericTypeDefinitionWhenServiceTypeIsNot(Type serviceType,
            Type decoratorType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied decorator '{0}' is an open generic type definition, while the supplied " +
                "service type '{1}' is not.", decoratorType.ToFriendlyName(), serviceType.ToFriendlyName());
        }

        internal static string MultipleTypesThatRepresentClosedGenericType(Type closedServiceType,
            Type[] implementations)
        {
            var typeDescription =
                string.Join(", ", implementations.Select(type => type.ToFriendlyName()).ToArray());

            return string.Format(CultureInfo.InvariantCulture,
                    "There are {0} types that represent the closed generic type '{1}'. Types: {2}. " +
                    "Either remove one of the types or use an overload that takes an {3} delegate, " +
                    "which allows you to define the way these types should be registered.",
                    implementations.Length, closedServiceType.ToFriendlyName(), typeDescription,
                    typeof(BatchRegistrationCallback).Name);
        }

        internal static string CantGenerateFuncForDecorator(Type serviceType, Type decoratorType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "It's impossible for the container to generate a Func<{0}> for injection into the {1} " +
                "decorator, that will be wrapped around instances of the collection of {0} instances, " +
                "because the registration hasn't been made using one of the RegisterAll overloads that " +
                "take a list of System.Type as serviceTypes. By passing in an IEnumerable<{0}> it is " +
                "impossible for the container to determine its lifestyle, which makes it impossible to " +
                "generate a Func<T>. Either switch to one of the other RegisterAll overloads, or don't " +
                "use a decorator that depends on a Func<T> for injecting the decoratee.",
                serviceType.ToFriendlyName(), decoratorType.ToFriendlyName());
        }

        internal static string ErrorInRegisterOpenGenericRegistration(Type openGenericServiceType,
            Type closedGenericImplementation, string message)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "There was an error in the registration of open generic type {0}. " +
                "Failed to build a registration for type {1}. {2}",
                openGenericServiceType.ToFriendlyName(), closedGenericImplementation.ToFriendlyName(),
                message);
        }
    }
}