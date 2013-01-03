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

namespace SimpleInjector
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

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
                "The registered delegate for type {0} returned null.", serviceType.ToFriendlyName());
        }

        internal static string ErrorWhileTryingToGetInstanceOfType(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Error occurred while trying to get an instance of type {0}.",
                serviceType.ToFriendlyName());
        }

        internal static string ErrorWhileBuildingDelegateFromExpression(Type serviceType,
            Expression expression, Exception exception)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Error occurred while trying to build a delegate for type {0} using the expression \"{1}\". " +
                "{2}", serviceType.ToFriendlyName(), expression, exception.Message);
        }

        internal static string DelegateForTypeThrewAnException(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The registered delegate for type {0} threw an exception.", serviceType.ToFriendlyName());
        }

        internal static string NoRegistrationForTypeFound(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "No registration for type {0} could be found.", serviceType.ToFriendlyName());
        }

        internal static string ConfigurationInvalidCreatingInstanceFailed(Type serviceType,
            Exception exception)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. Creating the instance for type {0} failed. {1}",
                serviceType.ToFriendlyName(), exception.Message);
        }

        internal static string ConfigurationInvalidIteratingCollectionFailed(Type serviceType,
            Exception exception)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. Iterating the collection for type {0} failed. {1}",
                serviceType.ToFriendlyName(), exception.Message);
        }

        internal static string ConfigurationInvalidCollectionContainsNullElements(Type firstInvalidServiceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. One of the items in the collection for type {0} is " +
                "a null reference.", firstInvalidServiceType.ToFriendlyName());
        }

        internal static string TypeAlreadyRegistered(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Type {0} has already been registered " +
                "and the container is currently not configured to allow overriding registrations. " +
                "To allow overriding the current registration, please create the container using the " +
                "constructor overload that takes a {1} instance and set the " +
                "AllowOverridingRegistrations property to true.",
                serviceType.ToFriendlyName(), typeof(ContainerOptions).Name);
        }

        internal static string CollectionTypeAlreadyRegistered(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Collection of items for type {0} has already been registeredand " +
                "and the container is currently not configured to allow overriding registrations. " +
                "To allow overriding the current registration, please create the container using the " +
                "constructor overload that takes a {1} instance and set the " +
                "AllowOverridingRegistrations property to true.",
                serviceType.ToFriendlyName(), typeof(ContainerOptions).Name);
        }

        internal static string ParameterTypeMustBeRegistered(Type implementationType, ParameterInfo parameter)
        {
            return string.Format(CultureInfo.InvariantCulture,
                ImplicitRegistrationCouldNotBeMadeForType(implementationType) +
                "The constructor of the type {3} contains the parameter of type {0} with name '{1}' that is " +
                "not registered. Please ensure {0} is registered in the container, or change the " +
                "constructor of {2}.",
                parameter.ParameterType.ToFriendlyName(), parameter.Name, implementationType.ToFriendlyName(),
                parameter.Member.DeclaringType.ToFriendlyName());
        }

        internal static string TypeMustHaveASinglePublicConstructor(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to create {0}, it should contain exactly one public " +
                "constructor, but it has {1}.",
                serviceType.ToFriendlyName(), serviceType.GetConstructors().Length);
        }

        internal static string ConstructorMustNotContainInvalidParameter(ConstructorInfo constructor,
            ParameterInfo invalidParameter)
        {
            string reason = string.Empty;

            if (invalidParameter.ParameterType.IsValueType)
            {
                reason = " because it is a value type";
            }

            return string.Format(CultureInfo.InvariantCulture,
                "The constructor of type {0} contains parameter '{1}' of type {2} which can not be used " +
                "for constructor injection{3}.",
                constructor.DeclaringType.ToFriendlyName(), invalidParameter.Name,
                invalidParameter.ParameterType.ToFriendlyName(), reason);
        }

        internal static string TypeShouldBeConcreteToBeUsedOnThisMethod(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The given type {0} is not a concrete type. Please use one of the other overloads to " +
                "register this type.", serviceType.ToFriendlyName());
        }

        internal static string MultipleObserversRegisteredTheSameTypeToResolveUnregisteredType(
            Type unregisteredServiceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Multiple observers of the ResolveUnregisteredType event are registering a delegate for " +
                "the same service type: {0}. Make sure only one of the registered handlers calls the " +
                "ResolveUnregisteredType.Register method for a given service type.",
                unregisteredServiceType.ToFriendlyName());
        }

        internal static string DelegateRegisteredUsingResolveUnregisteredTypeThatThrewAnException(
            Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The delegate that was hooked to the ResolveUnregisteredType event and responded " +
                "to the {0} service type, registered a delegate that threw an exception.",
                serviceType.ToFriendlyName());
        }

        internal static string DelegateRegisteredUsingResolveUnregisteredTypeThatReturnedNull(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The delegate that was hooked to the ResolveUnregisteredType event and responded " +
                "to the {0} service type, registered a delegate that returned a null reference.",
                serviceType.ToFriendlyName());
        }

        internal static string DelegateRegisteredUsingResolveUnregisteredTypeReturnedAnUnassignableFrom(
            Type serviceType, Type actualType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The delegate that was hooked to the ResolveUnregisteredType event and responded " +
                "to the {0} service type, registered a delegate that created an instance of type {1} that " +
                "can not be cast to the specified service type.",
                serviceType.ToFriendlyName(), actualType.ToFriendlyName());
        }

        internal static string ImplicitRegistrationCouldNotBeMadeForType(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "No registration for type {0} could be found and an implicit registration could not be made. ",
                serviceType.ToFriendlyName());
        }

        internal static string TypeDependsOnItself(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. The type {0} is directly or indirectly depending on itself.",
                serviceType.ToFriendlyName());
        }

        internal static string UnableToResolveTypeDueToSecurityConfiguration(Type serviceType,
            Exception innerException)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Unable to resolve type {0}. The security restrictions of your application's sandbox do " +
                "not permit the creation of this type. Explicitly register the type using one of the " +
                "generic 'Register' overloads or consider making it public. {1}",
                serviceType.ToFriendlyName(), innerException.Message);
        }

        internal static string UnableToInjectPropertiesDueToSecurityConfiguration(Type injectee,
            Exception innerException)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Unable to inject properties into type {0}. The security restrictions of your application's " +
                "sandbox do not permit the creation of one of its dependencies. Explicitly register that " +
                "dependency using one of the generic 'Register' overloads or consider making it public. " +
                "Please see the inner exception for more details about which type caused this failure. {1}",
                injectee.ToFriendlyName(), innerException.Message);
        }

        internal static string ContainerOptionsBelongsToAnotherContainer()
        {
            return
                "The supplied ContainerOptions instance belongs to another Container instance. Create a " +
                "new ContainerOptions per Container instance.";
        }

        internal static string PropertyCanNotBeChangedAfterTheFirstRegistration(string propertyName)
        {
            return
                "The " + propertyName + " property cannot be changed after the first registration has " +
                "been made to the container.";
        }

        internal static string TypeIsAmbiguous(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "You are trying to register {0} as a service type, but registering this type is not " +
                "allowed to be registered because the type is ambiguous. The registration of such a type " + 
                "almost always indicates a flaw in the design of the application and is therefore not " +
                "allowed. Please change any component that depends on a dependency of this type. Ensure " +
                "that the container does not have to inject any dependencies of this type by injecting a " +
                "different type.",
                serviceType.ToFriendlyName());
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

        internal static string SuppliedTypeDoesNotInheritFromOrImplement(Type service, Type implementation)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied type '{0}' does not {1} '{2}'.",
                implementation.ToFriendlyName(), 
                service.IsInterface ? "implement" : "inherit from",
                service.ToFriendlyName());
        }

        internal static string TheInitializersCouldNotBeApplied(Type type, Exception innerException)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The initializer(s) for type {0} could not be applied. {1}",
                type.ToFriendlyName(), innerException.Message);
        }
    }
}