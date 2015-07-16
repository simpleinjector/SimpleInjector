#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2015 Simple Injector Contributors
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
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Extensions;
    using SimpleInjector.Internals;

    /// <summary>Internal helper for string resources.</summary>
    internal static class StringResources
    {
        internal static string ContainerCanNotBeChangedAfterUse(string stackTrace)
        {
            string message = "The container can't be changed after the first call to GetInstance, GetAllInstances " +
                "and Verify.";

            if (stackTrace == null)
            {
                return message;
            }

            return message +
                " The following stack trace describes the location where the container was locked:" +
                Environment.NewLine + Environment.NewLine + stackTrace;
        }

        internal static string DelegateForTypeReturnedNull(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The registered delegate for type {0} returned null.", serviceType.ToFriendlyName());
        }

        internal static string ResolveInterceptorDelegateReturnedNull()
        {
            return "The delegate that was registered using 'RegisterResolveInterceptor' returned null.";
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

        internal static string NoRegistrationForTypeFound(Type serviceType, bool containerHasRegistrations)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "No registration for type {0} could be found.{1}", 
                serviceType.ToFriendlyName(),
                ContainsHasNoRegistrationsAddition(containerHasRegistrations));
        }

        internal static string OpenGenericTypesCanNotBeResolved(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The request for type {0} is invalid because it is an open generic type: it is only " +
                "possible to instantiate instances of closed generic types. A generic type is closed if " +
                "all of its type parameters have been substituted with types that are recognized by the " +
                "compiler.",
                serviceType.ToFriendlyName());
        }

        internal static string LifestyleMismatchesReported(PotentialLifestyleMismatchDiagnosticResult error)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "A lifestyle mismatch is encountered. {0} Lifestyle mismatches can cause concurrency " +
                "bugs in your application. Please see https://simpleinjector.org/dialm to understand this " +
                "problem and how to solve it.",
                error.Description);
        }

        internal static string DiagnosticWarningsReported(IList<DiagnosticResult> errors)
        {
            var descriptions =
                from error in errors
                select "-" + error.Description;

            return string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. The following diagnostic warnings were reported:\n{0}\n" +
                "See the Error property for detailed information about the warnings. " +
                "Please see https://simpleinjector.org/diagnostics how to fix problems and how to suppress " +
                "individual warnings.",
                string.Join(Environment.NewLine, descriptions));
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
                "Type {0} has already been registered. If your intention is to resolve " +
                "a collection of {0} implementations, use the RegisterCollection overloads. More info: " +
                "https://simpleinjector.org/coll1" +
                ". If your intention is to replace the existing registration with this new registration, " +
                "you can allow overriding the current registration by setting Container.Options." +
                "AllowOverridingRegistrations to true. More info: https://simpleinjector.org/ovrrd.",
                serviceType.ToFriendlyName());
        }

        internal static string MakingConditionalRegistrationsInOverridingModeIsNotSupported()
        {
            return
                "The making of conditional registrations is not supported when AllowOverridingRegistrations " +
                "is set, because it is impossible for the container to detect whether the registration " +
                "should replace a different registration or not.";
        }

        internal static string NonGenericTypeAlreadyRegisteredAsConditionalRegistration(Type serviceType)
        {
            return NonGenericTypeAlreadyRegistered(serviceType, existingRegistrationIsConditional: true);
        }

        internal static string NonGenericTypeAlreadyRegisteredAsUnconditionalRegistration(Type serviceType)
        {
            return NonGenericTypeAlreadyRegistered(serviceType, existingRegistrationIsConditional: false);
        }

        internal static string CollectionTypeAlreadyRegistered(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Collection of items for type {0} has already been registered " +
                "and the container is currently not configured to allow overriding registrations. " +
                "To allow overriding the current registration, please create the container using the " +
                "constructor overload that takes a {1} instance and set the " +
                "AllowOverridingRegistrations property to true.",
                serviceType.ToFriendlyName(), typeof(ContainerOptions).Name);
        }

        internal static string ParameterTypeMustBeRegistered(InjectionTargetInfo target, int count)
        {
            if (target.Parameter != null)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "The constructor of type {0} contains the parameter with name '{1}' and type {2} that " +
                    "is not registered. Please ensure {2} is registered, or change the constructor of {0}.{3}",
                    target.Member.DeclaringType.ToFriendlyName(),
                    target.Name,
                    target.TargetType.ToFriendlyName(),
                    GetAdditionalInformationAboutExistingConditionalRegistrations(target, count));
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "Type {0} contains the property with name '{1}' and type {2} that is not registered. " +
                    "Please ensure {2} is registered, or change {0}.{3}",
                    target.Member.DeclaringType.ToFriendlyName(),
                    target.Name,
                    target.TargetType.ToFriendlyName(),
                    GetAdditionalInformationAboutExistingConditionalRegistrations(target, count));
            }
        }

        internal static string TypeMustHaveASinglePublicConstructor(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to create {0}, it should contain exactly one public " +
                "constructor, but it has {1}.",
                serviceType.ToFriendlyName(), serviceType.GetConstructors().Length);
        }

        internal static string TypeMustNotContainInvalidInjectionTarget(InjectionTargetInfo invalidTarget)
        {
            string reason = string.Empty;

            if (invalidTarget.TargetType.IsValueType)
            {
                reason = " because it is a value type";
            }

            if (invalidTarget.Parameter != null)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "The constructor of type {0} contains parameter '{1}' of type {2} which can not be used " +
                    "for constructor injection{3}.",
                    invalidTarget.Member.DeclaringType.ToFriendlyName(), 
                    invalidTarget.Name,
                    invalidTarget.TargetType.ToFriendlyName(), 
                    reason);
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "The type {0} contains property '{1}' of type {2} which can not be used for property " +
                    "injection{3}.",
                    invalidTarget.Member.DeclaringType.ToFriendlyName(),
                    invalidTarget.Name,
                    invalidTarget.TargetType.ToFriendlyName(), 
                    reason);
            }
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

        internal static string ImplicitRegistrationCouldNotBeMadeForType(Type serviceType, 
            bool containerHasRegistrations)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "No registration for type {0} could be found and an implicit registration could not be made.{1}",
                serviceType.ToFriendlyName(), 
                ContainsHasNoRegistrationsAddition(containerHasRegistrations));
        }

        internal static string TypeDependsOnItself(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. The type {0} is directly or indirectly depending on itself.",
                serviceType.ToFriendlyName());
        }

        internal static string CyclicDependencyGraphMessage(CyclicDependencyException exception)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0} The cyclic graph contains the following types: {1}.",
                exception.Message,
                string.Join(" -> ", exception.DependencyCycle.Select(Helpers.ToFriendlyName)));
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

        internal static string UnableToInjectPropertiesDueToSecurityConfiguration(Type serviceType,
            Exception innerException)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Unable to inject properties into type {0}. The security restrictions of your application's " +
                "sandbox do not permit the injection of one of its properties. Consider making it public. {1}",
                serviceType.ToFriendlyName(), innerException.Message);
        }

        internal static string UnableToInjectImplicitPropertiesDueToSecurityConfiguration(Type injectee,
            Exception innerException)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Unable to inject properties into type {0}. The security restrictions of your application's " +
                "sandbox do not permit the creation of one of its dependencies. Explicitly register that " +
                "dependency using one of the generic 'Register' overloads or consider making it public. {1}",
                injectee.ToFriendlyName(), innerException.Message);
        }

        internal static string PropertyCanNotBeChangedAfterTheFirstRegistration(string propertyName)
        {
            return
                "The " + propertyName + " property cannot be changed after the first registration has " +
                "been made to the container.";
        }

        internal static string RegisterCollectionCalledWithTypeAsTService(IEnumerable<Type> types)
        {
            return TypeIsAmbiguous(typeof(Type)) + " " + string.Format(CultureInfo.InvariantCulture,
                "The most likely cause of this happening is because the C# overload resolution picked " +
                "a different method for you than you expected to call. The method C# selected for you is: " +
                "RegisterCollection<Type>(new[] {{ {0} }}). Instead, you probably intended to call: " +
                "RegisterCollection(typeof({1}), new[] {{ {2} }}).",
                ToTypeOCfSharpFriendlyList(types),
                Helpers.ToCSharpFriendlyName(types.First()),
                ToTypeOCfSharpFriendlyList(types.Skip(1)));
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
                "The supplied type {0} is an open generic type. This type cannot be used for registration " +
                "using this method. Please use the RegisterCollection(Type, Type[]) method instead.",
                type.ToFriendlyName());
        }

        internal static string SuppliedTypeIsAnOpenGenericTypeWhileTheServiceTypeIsNot(Type type)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied type {0} is an open generic type. This type cannot be used for registration " +
                "of collections of non-generic types.",
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

        internal static string DependencyInjectionBehaviorReturnedNull(IDependencyInjectionBehavior behavior)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The {0} that was registered through the Container.Options.DependencyInjectionBehavior " +
                "property, returned a null reference after its BuildExpression() method. " +
                "{1}.BuildExpression implementations should never return null, but should throw " +
                "a {2} with an expressive message instead.",
                behavior.GetType().ToFriendlyName(),
                typeof(IDependencyInjectionBehavior).Name,
                typeof(ActivationException).FullName);
        }

        internal static string ConstructorResolutionBehaviorReturnedNull(
            IConstructorResolutionBehavior selectionBehavior, Type serviceType, Type implementationType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The {0} that was registered through Container.Options.ConstructorResolutionBehavior " +
                "returned a null reference after its GetConstructor(Type, Type) method was " +
                "supplied with values '{1}' for serviceType and '{2}' for implementationType. " +
                "{3}.GetConstructor implementations should never return null, but should throw " +
                "a {4} with an expressive message instead.",
                selectionBehavior.GetType().ToFriendlyName(),
                serviceType.ToFriendlyName(),
                implementationType.ToFriendlyName(),
                typeof(IConstructorResolutionBehavior).Name,
                typeof(ActivationException).FullName);
        }

        internal static string LifestyleSelectionBehaviorReturnedNull(
            ILifestyleSelectionBehavior selectionBehavior, Type serviceType, Type implementationType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The {0} that was registered through Container.Options.LifestyleSelectionBehavior " +
                "returned a null reference after its SelectLifestyle(Type, Type) method was " +
                "supplied with values '{1}' for serviceType and '{2}' for implementationType. " +
                "{3}.SelectLifestyle implementations should never return null.",
                selectionBehavior.GetType().ToFriendlyName(),
                serviceType.ToFriendlyName(),
                implementationType.ToFriendlyName(),
                typeof(ILifestyleSelectionBehavior).Name);
        }

        internal static string RegistrationReturnedNullFromBuildExpression(
            Registration lifestyleRegistration)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The {0} for the {1} returned a null reference from its BuildExpression method.",
                lifestyleRegistration.GetType().ToFriendlyName(),
                lifestyleRegistration.Lifestyle.GetType().ToFriendlyName());
        }

        internal static string MultipleTypesThatRepresentClosedGenericType(Type closedServiceType,
            Type[] implementations)
        {
            var friendlyNames = implementations.Select(type => type.ToFriendlyName());

            return string.Format(CultureInfo.InvariantCulture,
                    "There are {0} types in the supplied list of types or assemblies that represent the " +
                    "same closed generic type {1}. Conflicting types: {2}.",
                    implementations.Length,
                    closedServiceType.ToFriendlyName(),
                    friendlyNames.ToCommaSeparatedText());
        }

        internal static string CantGenerateFuncForDecorator(Type serviceType, Type decoratorType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "It's impossible for the container to generate a Func<{0}> for injection into the {1} " +
                "decorator, that will be wrapped around instances of the collection of {0} instances, " +
                "because the registration hasn't been made using one of the RegisterCollection overloads " +
                "that take a list of System.Type as serviceTypes. By passing in an IEnumerable<{0}> it is " +
                "impossible for the container to determine its lifestyle, which makes it impossible to " +
                "generate a Func<T>. Either switch to one of the other RegisterCollection overloads, or " +
                "don't use a decorator that depends on a Func<T> for injecting the decoratee.",
                serviceType.ToFriendlyName(), decoratorType.ToFriendlyName());
        }

        internal static string SuppliedTypeIsNotAGenericType(Type type)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied type {0} is not a generic type.", type.ToFriendlyName());
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
            Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to use {0} as a decorator, its constructor must include a " +
                "single parameter of type {1} (or Func<{1}>) - i.e. the type of the instance that is being " +
                "decorated. The parameter type {1} does not currently exist in the constructor of class {0}.",
                decoratorType.ToFriendlyName(), serviceType.ToFriendlyName());
        }

        internal static string TheConstructorOfTypeMustContainASingleInstanceOfTheServiceTypeAsArgument(
            Type decoratorType, Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to use {0} as a decorator, its constructor must include a " +
                "single parameter of type {1} (or Func<{1}>) - i.e. the type of the instance that is being " +
                "decorated. The parameter type {1} is defined multiple times in the constructor of class {0}.",
                decoratorType.ToFriendlyName(), serviceType.ToFriendlyName());
        }

        internal static string OpenGenericTypeContainsUnresolvableTypeArguments(Type openGenericImplementation)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied type {0} contains unresolvable type arguments. " +
                "The type would never be resolved and is therefore not suited to be used.",
                openGenericImplementation.ToFriendlyName());
        }

        internal static string DecoratorCanNotBeAGenericTypeDefinitionWhenServiceTypeIsNot(Type serviceType,
            Type decoratorType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied decorator {0} is an open generic type definition, while the supplied " +
                "service type {1} is not.", decoratorType.ToFriendlyName(), serviceType.ToFriendlyName());
        }

        internal static string TheSuppliedRegistrationBelongsToADifferentContainer()
        {
            return "The supplied Registration belongs to a different container.";
        }

        internal static string CanNotDecorateContainerUncontrolledCollectionWithThisLifestyle(
            Type decoratorType, Lifestyle lifestyle, Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "You are trying to apply the {0} decorator with the '{1}' lifestyle to a collection of " +
                "type {2}, but the registered collection is not controlled by the container. Since the " +
                "number of returned items might change on each call, the decorator with this lifestyle " +
                "cannot be applied to the collection. Instead, register the decorator with the Transient " +
                "lifestyle, or use one of the RegisterCollection overloads that takes a collection of " +
                "System.Type types.",
                decoratorType.ToFriendlyName(), lifestyle.Name, serviceType.ToFriendlyName());
        }

        internal static string PropertyHasNoSetter(PropertyInfo property)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The property named '{0}' with type {1} and declared on type {2} can't be injected, " +
                "because it has no set method.",
                property.Name, property.PropertyType.ToFriendlyName(), property.DeclaringType.ToFriendlyName());
        }

        internal static string PropertyIsStatic(PropertyInfo property)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Property of type {0} with name '{1}' can't be injected, because it is static.",
                property.PropertyType.ToFriendlyName(), property.Name);
        }

        internal static string ThisOverloadDoesNotAllowOpenGenerics(IEnumerable<Type> openGenericTypes)
        {
            var typeNames = openGenericTypes.Select(type => type.ToFriendlyName());

            return string.Format(CultureInfo.InvariantCulture,
                "The supplied list of types contains one or multiple open generic types, but this method is " +
                "unable to handle open generic types because it can only map closed generic service types " +
                "to a single implementation. Try using RegisterCollection instead. Invalid types: {0}.",
                typeNames.ToCommaSeparatedText());
        }

        internal static string AppendingRegistrationsToContainerUncontrolledCollectionsIsNotSupported(
            Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "You are trying to append a registration to the registered collection of {0} instances, " +
                "which is either registered using RegisterCollection<TService>(IEnumerable<TService>) or " +
                "RegisterCollection(Type, IEnumerable). Since the number of returned items might change on " +
                "each call, appending registrations to these collections is not supported. Please register " +
                "the collection with one of the other RegisterCollection overloads if appending is required.",
                serviceType.ToFriendlyName());
        }

        internal static string UnregisteredTypeEventArgsRegisterDelegateReturnedUncastableInstance(
            Type serviceType, InvalidCastException exception)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The delegate that was registered for service type {0} using the " +
                "UnregisteredTypeEventArgs.Register(Func<object>) method returned an object that " +
                "couldn't be casted to {0}. {1}",
                serviceType.ToFriendlyName(), exception.Message);
        }

        internal static string UnregisteredTypeEventArgsRegisterDelegateThrewAnException(Type serviceType,
            Exception exception)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The delegate that was registered for service type {0} using the " +
                "UnregisteredTypeEventArgs.Register(Func<object>) method threw an exception. {1}",
                serviceType.ToFriendlyName(), exception.Message);
        }

        internal static string TheServiceIsRequestedOutsideTheContextOfAScopedLifestyle(Type serviceType,
            ScopedLifestyle lifestyle)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The {0} is registered as '{1}' lifestyle, but the instance is requested outside the " +
                "context of a {1}.",
                serviceType.ToFriendlyName(),
                lifestyle.Name);
        }

        internal static string ThisMethodCanOnlyBeCalledWithinTheContextOfAnActiveScope(string lifestyleName)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "This method can only be called within the context of an active {0}.",
                lifestyleName);
        }

        internal static string DecoratorFactoryReturnedNull(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The decorator type factory delegate that was registered for service type {0} returned null.",
                serviceType.ToFriendlyName());
        }

        internal static string ImplementationTypeFactoryReturnedNull(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The implementation type factory delegate that was registered for service type {0} returned null.",
                serviceType.ToFriendlyName());
        }

        internal static string TheDecoratorReturnedFromTheFactoryShouldNotBeOpenGeneric(
            Type serviceType, Type decoratorType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The registered decorator type factory returned open generic type {0} while the registered " +
                "service type {1} is not generic, making it impossible for a closed generic decorator type " +
                "to be constructed.",
                decoratorType.ToFriendlyName(),
                serviceType.ToFriendlyName());
        }

        internal static string TheTypeReturnedFromTheFactoryShouldNotBeOpenGeneric(
            Type serviceType, Type implementationType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The registered implementation type factory returned open generic type {0} while the " +
                "registered service type {1} is not generic, making it impossible for a closed generic " +
                "decorator type to be constructed.",
                implementationType.ToFriendlyName(),
                serviceType.ToFriendlyName());
        }

        internal static string TypeFactoryReturnedIncompatibleType(Type serviceType, Type implementationType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The registered type factory returned type {0} which does not implement {1}.",
                implementationType.ToFriendlyName(), serviceType.ToFriendlyName());
        }

        internal static string RecursiveInstanceRegistrationDetected()
        {
            return
                "A recursive registration of Action or IDisposable instances was detected during disposal " +
                "of the scope. This is possibly caused by a component that is directly or indirectly " +
                "depending on itself.";
        }

        internal static string GetRootRegistrationsCanNotBeCalledBeforeVerify()
        {
            return
                "Root registrations can't be determined before Verify is called. Please call Verify first.";
        }

        internal static string VisualizeObjectGraphShouldBeCalledAfterTheExpressionIsCreated()
        {
            return "This method can only be called after GetInstance() or BuildExpression() have been called.";
        }

        internal static string MixingCallsToRegisterCollectionIsNotSupported(Type serviceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Mixing calls to RegisterCollection for the same open generic service type is not " +
                "supported. Consider making one single call to RegisterCollection(typeof({0}), types).",
                Helpers.ToCSharpFriendlyName(serviceType.GetGenericTypeDefinition()));
        }

        internal static string MixingRegistrationsWithControlledAndUncontrolledIsNotSupported(Type serviceType,
            bool controlled)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "You already made a registration for the {0} type using one of the RegisterCollection " +
                "overloads that registers container-{1} collections, while this method registers container-" +
                "{2} collections. Mixing calls is not supported. Consider merging those calls or make both " +
                "calls either as controlled or uncontrolled registration.",
                (serviceType.IsGenericType ? serviceType.GetGenericTypeDefinition() : serviceType).ToFriendlyName(),
                controlled ? "uncontrolled" : "controlled",
                controlled ? "controlled" : "uncontrolled");
        }

        internal static string ValueInvalidForEnumType(string paramName, object invalidValue, Type enumClass)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The value of argument '{0}' ({1}) is invalid for Enum type '{2}'.",
                paramName,
                invalidValue,
                enumClass.Name);
        }

        internal static string ServiceTypeCannotBeAPartiallyClosedType(Type openGenericServiceType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied type '{0}' is a partially-closed generic type, which is not supported by " +
                "this method. Please supply the open generic type '{1}' instead.",
                openGenericServiceType.ToFriendlyName(),
                Helpers.ToCSharpFriendlyName(openGenericServiceType.GetGenericTypeDefinition()));
        }

        internal static string ServiceTypeCannotBeAPartiallyClosedType(Type openGenericServiceType,
            string serviceTypeParamName, string implementationTypeParamName)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "The supplied type '{0}' is a partially-closed generic type, which is not supported as " +
                "value of the {1} parameter. Instead, please supply the open generic type '{2}' and make " +
                "the type supplied to the {3} parameter partially-closed instead.",
                openGenericServiceType.ToFriendlyName(),
                serviceTypeParamName,
                Helpers.ToCSharpFriendlyName(openGenericServiceType.GetGenericTypeDefinition()),
                implementationTypeParamName);
        }

        internal static string SuppliedTypeIsNotGenericExplainingAlternativesWithAssemblies(Type type)
        {
            return SuppliedTypeIsNotGenericExplainingAlternatives(type, typeof(Assembly).Name);
        }

        internal static string SuppliedTypeIsNotGenericExplainingAlternativesWithTypes(Type type)
        {
            return SuppliedTypeIsNotGenericExplainingAlternatives(type, typeof(Type).Name);
        }

        internal static string SuppliedTypeIsNotOpenGenericExplainingAlternativesWithAssemblies(Type type)
        {
            return SuppliedTypeIsNotOpenGenericExplainingAlternatives(type, typeof(Assembly).Name);
        }

        internal static string SuppliedTypeIsNotOpenGenericExplainingAlternativesWithTypes(Type type)
        {
            return SuppliedTypeIsNotOpenGenericExplainingAlternatives(type, typeof(Type).Name);
        }

        internal static string RegistrationForClosedServiceTypeOverlapsWithOpenGenericRegistration(
            Type closedServiceType, Type overlappingGenericImplementationType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "There is already an open generic registration for {0} (with implementation {1}) that " +
                "overlaps with the registration of {2} that you are trying to make. If your intention is " +
                "to use {1} as fallback registration, please instead call: " +
                "RegisterConditional(typeof({3}), typeof({4}), c => !c.Handled).",
                closedServiceType.GetGenericTypeDefinition().ToFriendlyName(),
                overlappingGenericImplementationType.ToFriendlyName(),
                closedServiceType.ToFriendlyName(),
                Helpers.ToCSharpFriendlyName(closedServiceType.GetGenericTypeDefinition()),
                Helpers.ToCSharpFriendlyName(overlappingGenericImplementationType));
        }

        internal static string AnOverlappingGenericRegistrationExists(Type openGenericServiceType,
            Type overlappingImplementationType, bool isExistingRegistrationConditional,
            Type implementationTypeOfNewRegistration, bool isNewRegistrationConditional)
        {
            string solution = "Either remove one of the registrations or make them both conditional.";

            if (isExistingRegistrationConditional && isNewRegistrationConditional &&
                overlappingImplementationType == implementationTypeOfNewRegistration)
            {
                solution =
                    "You can merge both registrations into a single conditional registration and combine " +
                    "both predicates into one single predicate.";
            }

            return string.Format(CultureInfo.InvariantCulture,
                "There is already a {0}registration for {1} (with implementation {2}) that " +
                "overlaps with the registration for {3} that you are trying to make. This new " +
                "registration would cause ambiguity, because both registrations would be used for the " +
                "same closed service types. {4}",
                isExistingRegistrationConditional ? "conditional " : string.Empty,
                openGenericServiceType.ToFriendlyName(),
                overlappingImplementationType.ToFriendlyName(),
                implementationTypeOfNewRegistration.ToFriendlyName(),
                solution);
        }

        internal static string MultipleApplicableRegistrationsFound(Type serviceType, 
            Tuple<Type, Type, InstanceProducer>[] overlappingRegistrations)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Multiple applicable registrations found for {0}. The applicable registrations are {1}. " +
                "If your goal is to make one registration a fallback in case another registration is not " +
                "applicable, make the fallback registration last and check the Handled property in the " +
                "predicate.",
                serviceType.ToFriendlyName(),
                overlappingRegistrations.Select(BuildRegistrationName).ToCommaSeparatedText());
        }

        internal static string UnableToLoadTypesFromAssembly(Assembly assembly, Exception innerException)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Unable to load types from assembly {0}. {1}", assembly.FullName, innerException.Message);
        }

        private static string BuildRegistrationName(Tuple<Type, Type, InstanceProducer> registration, int index)
        {
            Type serviceType = registration.Item1;
            Type implementationType = registration.Item2;
            InstanceProducer producer = registration.Item3;

            return string.Format(CultureInfo.InvariantCulture,
                "({0}) the {1} {2}registration for {3} using {4}",
                index + 1,
                producer.IsConditional ? "conditional" : "unconditional",
                serviceType.IsGenericTypeDefinition 
                    ? "open generic " 
                    : serviceType.IsGenericType ? "closed generic " : string.Empty,
                serviceType.ToFriendlyName(),
                implementationType.ToFriendlyName());
        }

        private static string NonGenericTypeAlreadyRegistered(Type serviceType,
            bool existingRegistrationIsConditional)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Type {0} has already been registered as {1} registration. For non-generic types, " +
                "conditional and unconditional registrations can't be mixed.",
                serviceType.ToFriendlyName(),
                existingRegistrationIsConditional ? "conditional" : "unconditional");
        }

        private static string GetAdditionalInformationAboutExistingConditionalRegistrations(
            InjectionTargetInfo target, int count)
        {
            string serviceTypeName = target.TargetType.ToFriendlyName();

            bool isGenericType = target.TargetType.IsGenericType;

            string openServiceTypeName = isGenericType
                ? target.TargetType.GetGenericTypeDefinition().ToFriendlyName()
                : target.TargetType.ToFriendlyName();

            if (count > 1)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    " {0} conditional registrations for {1} exist{2}, but none of the supplied predicates " +
                    "returned true when provided with the contextual information for {3}.",
                    count,
                    openServiceTypeName,
                    isGenericType ? (" that are applicable to " + serviceTypeName) : string.Empty,
                    target.Member.DeclaringType.ToFriendlyName());
            }
            else if (count == 1)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    " 1 conditional registration for {0} exists{1}, but its supplied predicate didn't " +
                    "return true when provided with the contextual information for {2}.",
                    openServiceTypeName,
                    isGenericType ? (" that is applicable to " + serviceTypeName) : string.Empty,
                    target.Member.DeclaringType.ToFriendlyName());
            }
            else
            {
                return string.Empty;
            }
        }

        private static string SuppliedTypeIsNotOpenGenericExplainingAlternatives(Type type, string registeringElement)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Supply this method with the open generic type {0} to register all available " +
                "implementations of this type, or call RegisterCollection(Type, IEnumerable<{1}>) either " +
                "with the open or closed version of that type to register a collection of instances based " +
                "on that type.",
                Helpers.ToCSharpFriendlyName(type.GetGenericTypeDefinition()),
                registeringElement);
        }

        private static string ToTypeOCfSharpFriendlyList(IEnumerable<Type> types)
        {
            return string.Join(", ",
                from type in types
                select string.Format(CultureInfo.InvariantCulture, "typeof({0})", Helpers.ToCSharpFriendlyName(type)));
        }

        private static string SuppliedTypeIsNotGenericExplainingAlternatives(Type type, string registeringElement)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "This method only supports open generic types. " +
                "If you meant to register all available implementations of {0}, call " +
                "RegisterCollection(typeof({0}), IEnumerable<{1}>) instead.",
                type.ToFriendlyName(),
                registeringElement);
        }

        private static object ContainsHasNoRegistrationsAddition(bool containerHasRegistrations)
        {
            return containerHasRegistrations
                ? string.Empty
                : " Please note that the container instance you are resolving from contains no " +
                  "registrations. Could it be that you accidentally created a new -and empty- container?";
        }
    }
}