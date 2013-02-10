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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

    /// <summary>Internal helper for string resources.</summary>
    internal static class StringResources
    {
        internal static string ContainerCanNotBeChangedAfterUse()
        {
            return "The container can't be changed after the first call to GetInstance, GetAllInstances " +
                "and Verify.";
        }

        internal static string DelegateForTypeReturnedNull(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The registered delegate for type {0} returned null.", serviceType.ToFriendlyName());
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
                "Type {0} has already been registered and the container is currently not configured to " +
                "allow overriding registrations. To allow overriding the current registration, please set " +
                "the Container.Options.AllowOverridingRegistrations to true.",
                serviceType.ToFriendlyName());
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
                "The supplied type {0} is not a reference type. Only reference types are supported.",
                type.ToFriendlyName());
        }

        internal static string SuppliedTypeIsAnOpenGenericType(Type type)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied type {0} is an open generic type. Use the RegisterOpenGeneric or " +
                "RegisterManyForOpenGeneric extension method for registering open generic types.",
                type.ToFriendlyName());
        }

        internal static string SuppliedTypeDoesNotInheritFromOrImplement(Type service, Type implementation)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied type {0} does not {1} {2}.",
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

        internal static string ConstructorInjectionBehaviorReturnedNull(
            IConstructorInjectionBehavior injectionBehavior, ParameterInfo parameter)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The {0} that was registered through Container.Options.ConstructorInjectionBehavior " +
                "returned a null reference after its BuildParameterExpression(ParameterInfo) method was " +
                "supplied with the argument of type {1} with name '{2}' from the constructor of type {3}. " +
                "{4}.BuildParameterExpression implementations should never return null, but should throw " +
                "an {5} with an expressive message instead.",
                injectionBehavior.GetType().Namespace + "." + injectionBehavior.GetType().ToFriendlyName(),
                parameter.ParameterType.ToFriendlyName(), parameter.Name,
                parameter.Member.DeclaringType.ToFriendlyName(),
                typeof(IConstructorInjectionBehavior).Name,
                typeof(ActivationException).FullName);
        }

        internal static string RegistrationReturnedNullFromBuildExpression(
            Registration lifestyleRegistration)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The {0} for the {1} returned a null reference from its BuildExpression method.",
                lifestyleRegistration.GetType().ToFriendlyName(),
                lifestyleRegistration.Lifestyle.GetType().ToFriendlyName());
        }

        internal static string CanNotCallBuildParameterExpressionContainerOptionsNotPartOfContainer()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The ContainerOptions instance for this ConstructorInjectionBehavior is not part of a " +
                "Container instance. Please make sure the ContainerOptions instance is supplied as " +
                "argument to the constructor of a Container.");
        }
        
        internal static string MultipleTypesThatRepresentClosedGenericType(Type closedServiceType,
            Type[] implementations)
        {
            var typeDescription =
                string.Join(", ", implementations.Select(type => type.ToFriendlyName()).ToArray());

            return string.Format(CultureInfo.InvariantCulture,
                    "There are {0} types that represent the closed generic type {1}. Types: {2}. " +
                    "Either remove one of the types or use an overload that takes an {3} delegate, " +
                    "which allows you to define the way these types should be registered.",
                    implementations.Length, closedServiceType.ToFriendlyName(), typeDescription,
                    typeof(SimpleInjector.Extensions.BatchRegistrationCallback).Name);
        }

        internal static string ErrorWhileTryingToGetInstanceOfType(Type serviceType, string message)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Error occurred while trying to get an instance of type {0}. {1}",
                serviceType.ToFriendlyName(), message);
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
        
        internal static string SuppliedTypeIsNotAnOpenGenericType(Type type)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied type {0} is not an open generic type.", type.ToFriendlyName());
        }

        internal static string SuppliedTypeCanNotBeOpenWhenDecoratorIsClosed()
        {
            return
                "Registering a closed generic service type with an open generic decorator is not " +
                "supported. Instead, register the service type as open generic, and the decorator as " +
                "closed generic type.";
        }

        internal static string TheConstructorOfTypeMustContainTheServiceTypeAsArgument(Type decoratorType,
            Type serviceType, int numberOfServiceTypeDependencies)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to use {0} as a decorator, its constructor should have a " +
                "single argument of type {1} or Func<{1}>, but it currently has {2}.",
                decoratorType.ToFriendlyName(), serviceType.ToFriendlyName(), numberOfServiceTypeDependencies);
        }

        internal static string TheConstructorOfTypeMustContainTheServiceTypeAsArgument(Type decoratorType,
            IEnumerable<Type> validConstructorArgumentTypes)
        {
            var validConstructorArgumentFuncTypes =
                from type in validConstructorArgumentTypes
                select typeof(Func<>).MakeGenericType(type);

            var friendlyValidTypes =
                from type in validConstructorArgumentTypes.Concat(validConstructorArgumentFuncTypes)
                select type.ToFriendlyName();

            return string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to use {0} as a decorator, its constructor should have an " +
                "argument of one of the following types: {1}.",
                decoratorType.ToFriendlyName(), string.Join(", ", friendlyValidTypes.ToArray()));
        }

        internal static string DecoratorContainsUnresolvableTypeArguments(Type decoratorType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied decorator {0} contains unresolvable type arguments. " +
                "The type would never be resolved and is therefore not suited to be used as decorator.",
                decoratorType.ToFriendlyName());
        }
        
        internal static string DecoratorCanNotBeAGenericTypeDefinitionWhenServiceTypeIsNot(Type serviceType,
            Type decoratorType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied decorator {0} is an open generic type definition, while the supplied " +
                "service type {1} is not.", decoratorType.ToFriendlyName(), serviceType.ToFriendlyName());
        }
        
        internal static string ValueIsInvalidForEnumType(int value, Type enumType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The value {0} is invalid for Enum-type {1}.",
                value, enumType.ToFriendlyName());
        }

        internal static string TheSuppliedRegistrationBelongsToADifferentContainer()
        {
            return "The supplied Registration belongs to a different container.";
        }
    }
}