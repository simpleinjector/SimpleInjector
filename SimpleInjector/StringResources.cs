﻿#region Copyright Simple Injector Contributors
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
    using SimpleInjector.Internals;

    /// <summary>Internal helper for string resources.</summary>
    internal static class StringResources
    {
        internal static string ContainerCanNotBeChangedAfterUse(string stackTrace)
        {
            string message = string.Format(CultureInfo.InvariantCulture, 
                "The container can't be changed after the first call to {0}, {1} and {2}.",
                nameof(Container.GetInstance),
                nameof(Container.GetAllInstances),
                nameof(Container.Verify));

            if (stackTrace == null)
            {
                return message;
            }

            return message +
                " The following stack trace describes the location where the container was locked:" +
                Environment.NewLine + Environment.NewLine + stackTrace;
        }

        internal static string DelegateForTypeReturnedNull(Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "The registered delegate for type {0} returned null.", serviceType.ToFriendlyName());

        internal static string ResolveInterceptorDelegateReturnedNull() =>
            string.Format(CultureInfo.InvariantCulture,
                "The delegate that was registered using '{0}' returned null.",
                nameof(ContainerOptions.RegisterResolveInterceptor));

        internal static string ErrorWhileBuildingDelegateFromExpression(Type serviceType,
            Expression expression, Exception exception) => 
            string.Format(CultureInfo.InvariantCulture,
                "Error occurred while trying to build a delegate for type {0} using the expression \"{1}\". " +
                "{2}", serviceType.ToFriendlyName(), expression, exception.Message);

        internal static string DelegateForTypeThrewAnException(Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "The registered delegate for type {0} threw an exception.", serviceType.ToFriendlyName());

        internal static string NoRegistrationForTypeFound(Type serviceType, bool containerHasRegistrations,
            bool containerHasRelatedOneToOneMapping, bool containerHasRelatedCollectionMapping,
            Type[] skippedDecorators) => 
            string.Format(CultureInfo.InvariantCulture,
                "No registration for type {0} could be found.{1}{2}{3}{4}",
                serviceType.ToFriendlyName(),
                ContainerHasNoRegistrationsAddition(containerHasRegistrations),
                DidYouMeanToCallGetInstanceInstead(containerHasRelatedOneToOneMapping, serviceType),
                DidYouMeanToCallGetAllInstancesInstead(containerHasRelatedCollectionMapping, serviceType),
                NoteThatSkippedDecoratorsWereFound(serviceType, skippedDecorators));

        internal static string OpenGenericTypesCanNotBeResolved(Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "The request for type {0} is invalid because it is an open generic type: it is only " +
                "possible to instantiate instances of closed generic types. A generic type is closed if " +
                "all of its type parameters have been substituted with types that are recognized by the " +
                "compiler.",
                serviceType.ToFriendlyName());

        internal static string LifestyleMismatchesReported(LifestyleMismatchDiagnosticResult error) => 
            string.Format(CultureInfo.InvariantCulture,
                "A lifestyle mismatch is encountered. {0} Lifestyle mismatches can cause concurrency " +
                "bugs in your application. Please see https://simpleinjector.org/dialm to understand this " +
                "problem and how to solve it.",
                error.Description);

        internal static string DiagnosticWarningsReported(IList<DiagnosticResult> errors)
        {
            var descriptions =
                from error in errors
                select string.Format(CultureInfo.InvariantCulture, "-[{0}] {1}", error.Name, error.Description);

            return string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. The following diagnostic warnings were reported:\n{0}\n" +
                "See the Error property for detailed information about the warnings. " +
                "Please see https://simpleinjector.org/diagnostics how to fix problems and how to suppress " +
                "individual warnings.",
                string.Join(Environment.NewLine, descriptions.Distinct()));
        }

        internal static string ConfigurationInvalidCreatingInstanceFailed(Type serviceType, Exception exception) => 
            string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. Creating the instance for type {0} failed. {1}",
                serviceType.ToFriendlyName(), exception.Message);

        internal static string ConfigurationInvalidIteratingCollectionFailed(Type serviceType, Exception exception) => 
            string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. Iterating the collection for type {0} failed. {1}",
                serviceType.ToFriendlyName(), exception.Message);

        internal static string ConfigurationInvalidCollectionContainsNullElements(Type firstInvalidServiceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. One of the items in the collection for type {0} is " +
                "a null reference.", firstInvalidServiceType.ToFriendlyName());

        internal static string TypeAlreadyRegistered(Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "Type {0} has already been registered. If your intention is to resolve a collection of " + 
                "{0} implementations, use the {1} overloads. More info: https://simpleinjector.org/coll1" +
                ". If your intention is to replace the existing registration with this new registration, " +
                "you can allow overriding the current registration by setting {2}.{3} to true. " +
                "More info: https://simpleinjector.org/ovrrd.",
                serviceType.ToFriendlyName(),
                nameof(Container.RegisterCollection),
                nameof(Container) + "." + nameof(Container.Options),
                nameof(ContainerOptions.AllowOverridingRegistrations));

        internal static string MakingConditionalRegistrationsInOverridingModeIsNotSupported() =>
            string.Format(CultureInfo.InvariantCulture,
                "The making of conditional registrations is not supported when {0} is set, because it is " +
                "impossible for the container to detect whether the registration should replace a " +
                "different registration or not.",
                nameof(ContainerOptions.AllowOverridingRegistrations));

        internal static string NonGenericTypeAlreadyRegisteredAsConditionalRegistration(Type serviceType) => 
            NonGenericTypeAlreadyRegistered(serviceType, existingRegistrationIsConditional: true);

        internal static string NonGenericTypeAlreadyRegisteredAsUnconditionalRegistration(Type serviceType) => 
            NonGenericTypeAlreadyRegistered(serviceType, existingRegistrationIsConditional: false);

        internal static string CollectionTypeAlreadyRegistered(Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "Collection of items for type {0} has already been registered " +
                "and the container is currently not configured to allow overriding registrations. " +
                "To allow overriding the current registration, please create the container using the " +
                "constructor overload that takes a {1} instance and set the {2} property to true.",
                serviceType.ToFriendlyName(),
                nameof(ContainerOptions),
                nameof(ContainerOptions.AllowOverridingRegistrations));

        internal static string ParameterTypeMustBeRegistered(InjectionTargetInfo target, int numberOfConditionals,
            bool hasRelatedOneToOneMapping, bool hasRelatedCollectionMapping, Type[] skippedDecorators) =>
            target.Parameter != null
                ? string.Format(CultureInfo.InvariantCulture,
                    "The constructor of type {0} contains the parameter with name '{1}' and type {2} that " +
                    "is not registered. Please ensure {2} is registered, or change the constructor of {0}.{3}{4}{5}{6}",
                    target.Member.DeclaringType.ToFriendlyName(),
                    target.Name,
                    target.TargetType.ToFriendlyName(),
                    GetAdditionalInformationAboutExistingConditionalRegistrations(target, numberOfConditionals),
                    DidYouMeanToDependOnNonCollectionInstead(hasRelatedOneToOneMapping, target.TargetType),
                    DidYouMeanToDependOnCollectionInstead(hasRelatedCollectionMapping, target.TargetType),
                    NoteThatSkippedDecoratorsWereFound(target.TargetType, skippedDecorators))
                : string.Format(CultureInfo.InvariantCulture,
                    "Type {0} contains the property with name '{1}' and type {2} that is not registered. " +
                    "Please ensure {2} is registered, or change {0}.{3}{4}{5}{6}",
                    target.Member.DeclaringType.ToFriendlyName(),
                    target.Name,
                    target.TargetType.ToFriendlyName(),
                    GetAdditionalInformationAboutExistingConditionalRegistrations(target, numberOfConditionals),
                    DidYouMeanToDependOnNonCollectionInstead(hasRelatedOneToOneMapping, target.TargetType),
                    DidYouMeanToDependOnCollectionInstead(hasRelatedCollectionMapping, target.TargetType),
                    NoteThatSkippedDecoratorsWereFound(target.TargetType, skippedDecorators));

        internal static string TypeMustHaveASinglePublicConstructorButItHasNone(Type serviceType) =>
            string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to create {0} it should have only one public constructor: " +
                "it has none.",
                serviceType.ToFriendlyName());

        internal static string TypeMustHaveASinglePublicConstructorButItHas(Type serviceType, int count) => 
            string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to create {0} it should have only one public constructor: " +
                "it has {1}. See https://simpleinjector.org/one-constructor for more " +
                "information.",
                serviceType.ToFriendlyName(), count);

        internal static string TypeMustNotContainInvalidInjectionTarget(InjectionTargetInfo invalidTarget)
        {
            string reason = string.Empty;

            if (invalidTarget.TargetType.Info().IsValueType)
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

        internal static string TypeShouldBeConcreteToBeUsedOnThisMethod(Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "The given type {0} is not a concrete type. Please use one of the other overloads to " +
                "register this type.", serviceType.ToFriendlyName());

        internal static string MultipleObserversRegisteredTheSameTypeToResolveUnregisteredType(
            Type unregisteredServiceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "Multiple observers of the {0} event are registering a delegate for the same service " + 
                "type: {1}. Make sure only one of the registered handlers calls the {2}.{3} method for a " +
                "given service type.",
                nameof(Container.ResolveUnregisteredType),
                unregisteredServiceType.ToFriendlyName(),
                nameof(UnregisteredTypeEventArgs),
                nameof(UnregisteredTypeEventArgs.Register));

        internal static string ImplicitRegistrationCouldNotBeMadeForType(Type serviceType,
            bool containerHasRegistrations) => 
            string.Format(CultureInfo.InvariantCulture,
                "No registration for type {0} could be found and an implicit registration could not be made.{1}",
                serviceType.ToFriendlyName(),
                ContainerHasNoRegistrationsAddition(containerHasRegistrations));

        internal static string DefaultScopedLifestyleCanNotBeSetWithLifetimeScoped() =>
            string.Format(CultureInfo.InvariantCulture,
                "{0} can't be set with the value of {1}.{2}.",
                nameof(ContainerOptions.DefaultScopedLifestyle), nameof(Lifestyle), nameof(Lifestyle.Scoped));

        internal static string TypeDependsOnItself(Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "The configuration is invalid. The type {0} is directly or indirectly depending on itself.",
                serviceType.ToFriendlyName());

        internal static string CyclicDependencyGraphMessage(CyclicDependencyException exception) => 
            string.Format(CultureInfo.InvariantCulture,
                "{0} The cyclic graph contains the following types: {1}.",
                exception.Message,
                string.Join(" -> ", exception.DependencyCycle.Select(Helpers.ToFriendlyName)));

        internal static string UnableToResolveTypeDueToSecurityConfiguration(Type serviceType,
            Exception innerException) => 
            string.Format(CultureInfo.InvariantCulture,
                "Unable to resolve type {0}. The security restrictions of your application's sandbox do " +
                "not permit the creation of this type. Explicitly register the type using one of the " +
                "generic '{2}' overloads or consider making it public. {1}",
                serviceType.ToFriendlyName(), innerException.Message, nameof(Container.Register));

        internal static string UnableToInjectPropertiesDueToSecurityConfiguration(Type serviceType,
            Exception innerException) => 
            string.Format(CultureInfo.InvariantCulture,
                "Unable to inject properties into type {0}. The security restrictions of your application's " +
                "sandbox do not permit the injection of one of its properties. Consider making it public. {1}",
                serviceType.ToFriendlyName(), innerException.Message);

        internal static string UnableToInjectImplicitPropertiesDueToSecurityConfiguration(Type injectee,
            Exception innerException) => 
            string.Format(CultureInfo.InvariantCulture,
                "Unable to inject properties into type {0}. The security restrictions of your application's " +
                "sandbox do not permit the creation of one of its dependencies. Explicitly register that " +
                "dependency using one of the generic '{2}' overloads or consider making it public. {1}",
                injectee.ToFriendlyName(), innerException.Message, nameof(Container.Register));

        internal static string PropertyCanNotBeChangedAfterTheFirstRegistration(string propertyName) => 
            "The " + propertyName + " property cannot be changed after the first registration has " +
            "been made to the container.";

        internal static string RegisterCollectionCalledWithTypeAsTService(IEnumerable<Type> types) => 
            TypeIsAmbiguous(typeof(Type)) + " " + string.Format(CultureInfo.InvariantCulture,
                "The most likely cause of this happening is because the C# overload resolution picked " +
                "a different method for you than you expected to call. The method C# selected for you is: " +
                "{3}<Type>(new[] {{ {0} }}). Instead, you probably intended to call: " +
                "{3}(typeof({1}), new[] {{ {2} }}).",
                ToTypeOCfSharpFriendlyList(types),
                Helpers.ToCSharpFriendlyName(types.First()),
                ToTypeOCfSharpFriendlyList(types.Skip(1)),
                nameof(Container.RegisterCollection));

        internal static string TypeIsAmbiguous(Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "You are trying to register {0} as a service type, but registering this type is not " +
                "allowed to be registered because the type is ambiguous. The registration of such a type " +
                "almost always indicates a flaw in the design of the application and is therefore not " +
                "allowed. Please change any component that depends on a dependency of this type. Ensure " +
                "that the container does not have to inject any dependencies of this type by injecting a " +
                "different type.",
                serviceType.ToFriendlyName());

        internal static string SuppliedTypeIsNotAReferenceType(Type type) => 
            string.Format(CultureInfo.InvariantCulture,
                "The supplied type {0} is not a reference type. Only reference types are supported.",
                type.ToFriendlyName());

        internal static string SuppliedTypeIsAnOpenGenericType(Type type) => 
            string.Format(CultureInfo.InvariantCulture,
                "The supplied type {0} is an open generic type. This type cannot be used for registration " +
                "using this method. Please use the {1}(Type, Type[]) method instead.",
                type.ToFriendlyName(), nameof(Container.RegisterCollection));

        internal static string SuppliedTypeIsAnOpenGenericTypeWhileTheServiceTypeIsNot(Type type) => 
            string.Format(CultureInfo.InvariantCulture,
                "The supplied type {0} is an open generic type. This type cannot be used for registration " +
                "of collections of non-generic types.",
                type.ToFriendlyName());

        internal static string SuppliedElementDoesNotInheritFromOrImplement(Type serviceType, Type elementType,
            string elementDescription) =>
            string.Format(CultureInfo.InvariantCulture,
                "The supplied {0} of type {1} does not {2} {3}.",
                elementDescription,
                elementType.ToFriendlyName(),
                serviceType.Info().IsInterface ? "implement" : "inherit from",
                serviceType.ToFriendlyName());

        internal static string SuppliedTypeDoesNotInheritFromOrImplement(Type service, Type implementation) => 
            string.Format(CultureInfo.InvariantCulture,
                "The supplied type {0} does not {1} {2}.",
                implementation.ToFriendlyName(),
                service.Info().IsInterface ? "implement" : "inherit from",
                service.ToFriendlyName());

        internal static string TheInitializersCouldNotBeApplied(Type type, Exception innerException) => 
            string.Format(CultureInfo.InvariantCulture,
                "The initializer(s) for type {0} could not be applied. {1}",
                type.ToFriendlyName(), innerException.Message);

        internal static string DependencyInjectionBehaviorReturnedNull(IDependencyInjectionBehavior behavior) => 
            string.Format(CultureInfo.InvariantCulture,
                "The {0} that was registered through the Container.{3}.{4} property, returned a null " +
                "reference after its BuildExpression() method. {1}.BuildExpression implementations should " +
                "never return null, but should throw a {2} with an expressive message instead.",
                behavior.GetType().ToFriendlyName(),
                nameof(IDependencyInjectionBehavior),
                typeof(ActivationException).FullName,
                nameof(Container.Options),
                nameof(ContainerOptions.DependencyInjectionBehavior));

        internal static string ConstructorResolutionBehaviorReturnedNull(
            IConstructorResolutionBehavior selectionBehavior, Type serviceType, Type implementationType) => 
            string.Format(CultureInfo.InvariantCulture,
                "The {0} that was registered through Container.{5}.{6} returned a null reference after " +
                "its {7}(Type, Type) method was supplied with values '{1}' for serviceType and '{2}' for " +
                "implementationType. {3}.{7} implementations should never return null, but should throw " +
                "a {4} with an expressive message instead.",
                selectionBehavior.GetType().ToFriendlyName(),
                serviceType.ToFriendlyName(),
                implementationType.ToFriendlyName(),
                nameof(IConstructorResolutionBehavior),
                typeof(ActivationException).FullName,
                nameof(Container.Options),
                nameof(ContainerOptions.ConstructorResolutionBehavior),
                nameof(IConstructorResolutionBehavior.GetConstructor));

        internal static string LifestyleSelectionBehaviorReturnedNull(
            ILifestyleSelectionBehavior selectionBehavior, Type serviceType, Type implementationType) => 
            string.Format(CultureInfo.InvariantCulture,
                "The {0} that was registered through Container.{4}.{5} returned a null reference after " +
                "its {6}(Type, Type) method was supplied with values '{1}' for serviceType and '{2}' for " +
                "implementationType. {3}.{6} implementations should never return null.",
                selectionBehavior.GetType().ToFriendlyName(),
                serviceType.ToFriendlyName(),
                implementationType.ToFriendlyName(),
                nameof(ILifestyleSelectionBehavior),
                nameof(Container.Options),
                nameof(ContainerOptions.LifestyleSelectionBehavior),
                nameof(ILifestyleSelectionBehavior.SelectLifestyle));

        internal static string RegistrationReturnedNullFromBuildExpression(Registration lifestyleRegistration) => 
            string.Format(CultureInfo.InvariantCulture,
                "The {0} for the {1} returned a null reference from its {2} method.",
                lifestyleRegistration.GetType().ToFriendlyName(),
                lifestyleRegistration.Lifestyle.GetType().ToFriendlyName(),
                nameof(Registration.BuildExpression));

        internal static string MultipleTypesThatRepresentClosedGenericType(Type closedServiceType,
            Type[] implementations) => 
            string.Format(CultureInfo.InvariantCulture,
                "There are {0} types in the supplied list of types or assemblies that represent the " +
                "same closed generic type {1}. Conflicting types: {2}.",
                implementations.Length,
                closedServiceType.ToFriendlyName(),
                implementations.Select(type => type.ToFriendlyName()).ToCommaSeparatedText());

        internal static string CantGenerateFuncForDecorator(Type serviceType, Type decoratorType) =>
            string.Format(CultureInfo.InvariantCulture,
                "It's impossible for the container to generate a Func<{0}> for injection into the {1} " +
                "decorator, that will be wrapped around instances of the collection of {0} instances, " +
                "because the registration hasn't been made using one of the {2} overloads that take a " +
                "list of System.Type as serviceTypes. By passing in an IEnumerable<{0}> it is impossible " +
                "for the container to determine its lifestyle, which makes it impossible to generate a" +
                "Func<T>. Either switch to one of the other {2} overloads, or don't use a decorator that " +
                "depends on a Func<T> for injecting the decoratee.",
                serviceType.ToFriendlyName(), 
                decoratorType.ToFriendlyName(),
                nameof(Container.RegisterCollection));

        internal static string SuppliedTypeIsNotAGenericType(Type type) => 
            string.Format(CultureInfo.InvariantCulture,
                "The supplied type {0} is not a generic type.", type.ToFriendlyName());

        internal static string SuppliedTypeIsNotAnOpenGenericType(Type type) => 
            string.Format(CultureInfo.InvariantCulture,
                "The supplied type {0} is not an open generic type.", type.ToFriendlyName());

        internal static string SuppliedTypeCanNotBeOpenWhenDecoratorIsClosed() => 
            "Registering a closed generic service type with an open generic decorator is not " +
            "supported. Instead, register the service type as open generic, and the decorator as " +
            "closed generic type.";

        internal static string TheConstructorOfTypeMustContainTheServiceTypeAsArgument(Type decoratorType,
            Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to use {0} as a decorator, its constructor must include a " +
                "single parameter of type {1} (or Func<{1}>) - i.e. the type of the instance that is being " +
                "decorated. The parameter type {1} does not currently exist in the constructor of class {0}.",
                decoratorType.ToFriendlyName(), serviceType.ToFriendlyName());

        internal static string TheConstructorOfTypeMustContainASingleInstanceOfTheServiceTypeAsArgument(
            Type decoratorType, Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to use {0} as a decorator, its constructor must include a " +
                "single parameter of type {1} (or Func<{1}>) - i.e. the type of the instance that is being " +
                "decorated. The parameter type {1} is defined multiple times in the constructor of class {0}.",
                decoratorType.ToFriendlyName(), serviceType.ToFriendlyName());

        internal static string OpenGenericTypeContainsUnresolvableTypeArguments(Type openGenericImplementation) => 
            string.Format(CultureInfo.InvariantCulture,
                "The supplied type {0} contains unresolvable type arguments. " +
                "The type would never be resolved and is therefore not suited to be used.",
                openGenericImplementation.ToFriendlyName());

        internal static string DecoratorCanNotBeAGenericTypeDefinitionWhenServiceTypeIsNot(Type serviceType,
            Type decoratorType) => 
            string.Format(CultureInfo.InvariantCulture,
                "The supplied decorator {0} is an open generic type definition, while the supplied " +
                "service type {1} is not.", decoratorType.ToFriendlyName(), serviceType.ToFriendlyName());

        internal static string TheSuppliedRegistrationBelongsToADifferentContainer() => 
            "The supplied Registration belongs to a different container.";

        internal static string CanNotDecorateContainerUncontrolledCollectionWithThisLifestyle(
            Type decoratorType, Lifestyle lifestyle, Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "You are trying to apply the {0} decorator with the '{1}' lifestyle to a collection of " +
                "type {2}, but the registered collection is not controlled by the container. Since the " +
                "number of returned items might change on each call, the decorator with this lifestyle " +
                "cannot be applied to the collection. Instead, register the decorator with the Transient " +
                "lifestyle, or use one of the {3} overloads that takes a collection of System.Type types.",
                decoratorType.ToFriendlyName(), 
                lifestyle.Name, 
                serviceType.ToFriendlyName(),
                nameof(Container.RegisterCollection));

        internal static string PropertyHasNoSetter(PropertyInfo property) => 
            string.Format(CultureInfo.InvariantCulture,
                "The property named '{0}' with type {1} and declared on type {2} can't be used for injection, " +
                "because it has no set method.",
                property.Name, property.PropertyType.ToFriendlyName(), property.DeclaringType.ToFriendlyName());

        internal static string PropertyIsStatic(PropertyInfo property) => 
            string.Format(CultureInfo.InvariantCulture,
                "Property of type {0} with name '{1}' can't be used for injection, because it is static.",
                property.PropertyType.ToFriendlyName(), property.Name);

        internal static string ThisOverloadDoesNotAllowOpenGenerics(IEnumerable<Type> openGenericTypes) => 
            string.Format(CultureInfo.InvariantCulture,
                "The supplied list of types contains one or multiple open generic types, but this method is " +
                "unable to handle open generic types because it can only map closed generic service types " +
                "to a single implementation. Try using {1} instead. Invalid types: {0}.",
                openGenericTypes.Select(type => type.ToFriendlyName()).ToCommaSeparatedText(),
                nameof(Container.RegisterCollection));

        internal static string AppendingRegistrationsToContainerUncontrolledCollectionsIsNotSupported(
            Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "You are trying to append a registration to the registered collection of {0} instances, " +
                "which is either registered using {1}<TService>(IEnumerable<TService>) or " +
                "{1}(Type, IEnumerable). Since the number of returned items might change on each call, " +
                "appending registrations to these collections is not supported. Please register the " +
                "collection with one of the other {1} overloads if appending is required.",
                serviceType.ToFriendlyName(),
                nameof(Container.RegisterCollection));

        internal static string UnregisteredTypeEventArgsRegisterDelegateReturnedUncastableInstance(
            Type serviceType, InvalidCastException exception) => 
            string.Format(CultureInfo.InvariantCulture,
                "The delegate that was registered for service type {0} using the {2}.{3}(Func<object>) " +
                "method returned an object that couldn't be casted to {0}. {1}",
                serviceType.ToFriendlyName(), 
                exception.Message,
                nameof(UnregisteredTypeEventArgs),
                nameof(UnregisteredTypeEventArgs.Register));

        internal static string UnregisteredTypeEventArgsRegisterDelegateThrewAnException(Type serviceType,
            Exception exception) => 
            string.Format(CultureInfo.InvariantCulture,
                "The delegate that was registered for service type {0} using the {2}.{3}(Func<object>) " + 
                "method threw an exception. {1}",
                serviceType.ToFriendlyName(), 
                exception.Message,
                nameof(UnregisteredTypeEventArgs),
                nameof(UnregisteredTypeEventArgs.Register));

        internal static string TheServiceIsRequestedOutsideTheContextOfAScopedLifestyle(Type serviceType,
            ScopedLifestyle lifestyle) => 
            string.Format(CultureInfo.InvariantCulture,
                "The {0} is registered as '{1}' lifestyle, but the instance is requested outside the " +
                "context of a {1}.",
                serviceType.ToFriendlyName(),
                lifestyle.Name);

        internal static string ThisMethodCanOnlyBeCalledWithinTheContextOfAnActiveScope(string lifestyleName) => 
            string.Format(CultureInfo.InvariantCulture,
                "This method can only be called within the context of an active {0}.",
                lifestyleName);

        internal static string DecoratorFactoryReturnedNull(Type serviceType) =>
            string.Format(CultureInfo.InvariantCulture,
                "The decorator type factory delegate that was registered for service type {0} returned null.",
                serviceType.ToFriendlyName());

        internal static string FactoryReturnedNull(Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "The type factory delegate that was registered for service type {0} returned null.",
                serviceType.ToFriendlyName());

        internal static string ImplementationTypeFactoryReturnedNull(Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "The implementation type factory delegate that was registered for service type {0} returned null.",
                serviceType.ToFriendlyName());

        internal static string TheDecoratorReturnedFromTheFactoryShouldNotBeOpenGeneric(
            Type serviceType, Type decoratorType) =>
            string.Format(CultureInfo.InvariantCulture,
                "The registered decorator type factory returned open generic type {0} while the registered " +
                "service type {1} is not generic, making it impossible for a closed generic decorator type " +
                "to be constructed.",
                decoratorType.ToFriendlyName(),
                serviceType.ToFriendlyName());

        internal static string TheTypeReturnedFromTheFactoryShouldNotBeOpenGeneric(
            Type serviceType, Type implementationType) =>
            string.Format(CultureInfo.InvariantCulture,
                "The registered type factory returned open generic type {0} while the registered service " +
                "type {1} is not generic, making it impossible for a closed generic type to be constructed.",
                implementationType.ToFriendlyName(),
                serviceType.ToFriendlyName());

        internal static string TypeFactoryReturnedIncompatibleType(Type serviceType, Type implementationType) =>
            string.Format(CultureInfo.InvariantCulture,
                "The registered type factory returned type {0} which does not implement {1}.",
                implementationType.ToFriendlyName(), serviceType.ToFriendlyName());

        internal static string RecursiveInstanceRegistrationDetected() => 
            "A recursive registration of Action or IDisposable instances was detected during disposal " +
            "of the scope. This is possibly caused by a component that is directly or indirectly " +
            "depending on itself.";

        internal static string GetRootRegistrationsCanNotBeCalledBeforeVerify() => 
            "Root registrations can't be determined before Verify is called. Please call Verify first.";

        internal static string VisualizeObjectGraphShouldBeCalledAfterTheExpressionIsCreated() =>
            string.Format(CultureInfo.InvariantCulture,
                "This method can only be called after {0}() or {1}() have been called.",
                nameof(Container.GetInstance),
                nameof(Registration.BuildExpression));

        internal static string MixingCallsToRegisterCollectionIsNotSupported(Type serviceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "Mixing calls to {1} for the same open generic service type is not supported. Consider " +
                "making one single call to {1}(typeof({0}), types).",
                Helpers.ToCSharpFriendlyName(serviceType.GetGenericTypeDefinition()),
                nameof(Container.RegisterCollection));

        internal static string MixingRegistrationsWithControlledAndUncontrolledIsNotSupported(Type serviceType,
            bool controlled) => 
            string.Format(CultureInfo.InvariantCulture,
                "You already made a registration for the {0} type using one of the {3} " +
                "overloads that registers container-{1} collections, while this method registers container-" +
                "{2} collections. Mixing calls is not supported. Consider merging those calls or make both " +
                "calls either as controlled or uncontrolled registration.",
                (serviceType.Info().IsGenericType ? serviceType.GetGenericTypeDefinition() : serviceType).ToFriendlyName(),
                controlled ? "uncontrolled" : "controlled",
                controlled ? "controlled" : "uncontrolled",
                nameof(Container.RegisterCollection));

        internal static string ValueInvalidForEnumType(string paramName, object invalidValue, Type enumClass) =>
            string.Format(CultureInfo.InvariantCulture,
                "The value of argument '{0}' ({1}) is invalid for Enum type '{2}'.",
                paramName,
                invalidValue,
                enumClass.Name);

        internal static string ServiceTypeCannotBeAPartiallyClosedType(Type openGenericServiceType) => 
            string.Format(CultureInfo.InvariantCulture,
                "The supplied type '{0}' is a partially-closed generic type, which is not supported by " +
                "this method. Please supply the open generic type '{1}' instead.",
                openGenericServiceType.ToFriendlyName(),
                Helpers.ToCSharpFriendlyName(openGenericServiceType.GetGenericTypeDefinition()));

        internal static string ServiceTypeCannotBeAPartiallyClosedType(Type openGenericServiceType,
            string serviceTypeParamName, string implementationTypeParamName) => 
            string.Format(CultureInfo.InvariantCulture,
                "The supplied type '{0}' is a partially-closed generic type, which is not supported as " +
                "value of the {1} parameter. Instead, please supply the open generic type '{2}' and make " +
                "the type supplied to the {3} parameter partially-closed instead.",
                openGenericServiceType.ToFriendlyName(),
                serviceTypeParamName,
                Helpers.ToCSharpFriendlyName(openGenericServiceType.GetGenericTypeDefinition()),
                implementationTypeParamName);

        internal static string SuppliedTypeIsNotGenericExplainingAlternativesWithAssemblies(Type type) =>
            SuppliedTypeIsNotGenericExplainingAlternatives(type, typeof(Assembly).Name);

        internal static string SuppliedTypeIsNotGenericExplainingAlternativesWithTypes(Type type) =>
            SuppliedTypeIsNotGenericExplainingAlternatives(type, typeof(Type).Name);

        internal static string SuppliedTypeIsNotOpenGenericExplainingAlternativesWithAssemblies(Type type) =>
            SuppliedTypeIsNotOpenGenericExplainingAlternatives(type, typeof(Assembly).Name);

        internal static string SuppliedTypeIsNotOpenGenericExplainingAlternativesWithTypes(Type type) =>
            SuppliedTypeIsNotOpenGenericExplainingAlternatives(type, typeof(Type).Name);

        internal static string RegistrationForClosedServiceTypeOverlapsWithOpenGenericRegistration(
            Type closedServiceType, Type overlappingGenericImplementationType) =>
            string.Format(CultureInfo.InvariantCulture,
                "There is already an open generic registration for {0} (with implementation {1}) that " +
                "overlaps with the registration of {2} that you are trying to make. If your intention is " +
                "to use {1} as fallback registration, please instead call: " +
                "{5}(typeof({3}), typeof({4}), c => !c.Handled).",
                closedServiceType.GetGenericTypeDefinition().ToFriendlyName(),
                overlappingGenericImplementationType.ToFriendlyName(),
                closedServiceType.ToFriendlyName(),
                Helpers.ToCSharpFriendlyName(closedServiceType.GetGenericTypeDefinition()),
                Helpers.ToCSharpFriendlyName(overlappingGenericImplementationType),
                nameof(Container.RegisterConditional));

        internal static string AnOverlappingRegistrationExists(Type openGenericServiceType,
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
            Tuple<Type, Type, InstanceProducer>[] overlappingRegistrations) => 
            string.Format(CultureInfo.InvariantCulture,
                "Multiple applicable registrations found for {0}. The applicable registrations are {1}. " +
                "If your goal is to make one registration a fallback in case another registration is not " +
                "applicable, make the fallback registration last using RegisterConditional and make sure " +
                "the supplied predicate returns false in case the Handled property is true.",
                serviceType.ToFriendlyName(),
                overlappingRegistrations.Select(BuildRegistrationName).ToCommaSeparatedText());

        internal static string UnableToLoadTypesFromAssembly(Assembly assembly, Exception innerException) =>
            string.Format(CultureInfo.InvariantCulture,
                "Unable to load types from assembly {0}. {1}", assembly.FullName, innerException.Message);

        private static string BuildRegistrationName(Tuple<Type, Type, InstanceProducer> registration, int index)
        {
            Type serviceType = registration.Item1;
            Type implementationType = registration.Item2;
            InstanceProducer producer = registration.Item3;

            return string.Format(CultureInfo.InvariantCulture,
                "({0}) the {1} {2}registration for {3} using {4}",
                index + 1,
                producer.IsConditional ? "conditional" : "unconditional",
                serviceType.Info().IsGenericTypeDefinition 
                    ? "open generic " 
                    : serviceType.Info().IsGenericType ? "closed generic " : string.Empty,
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
            InjectionTargetInfo target, int numberOfConditionalRegistrations)
        {
            string serviceTypeName = target.TargetType.ToFriendlyName();

            bool isGenericType = target.TargetType.Info().IsGenericType;

            string openServiceTypeName = isGenericType
                ? target.TargetType.GetGenericTypeDefinition().ToFriendlyName()
                : target.TargetType.ToFriendlyName();

            if (numberOfConditionalRegistrations > 1)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    " {0} conditional registrations for {1} exist{2}, but none of the supplied predicates " +
                    "returned true when provided with the contextual information for {3}.",
                    numberOfConditionalRegistrations,
                    openServiceTypeName,
                    isGenericType ? (" that are applicable to " + serviceTypeName) : string.Empty,
                    target.Member.DeclaringType.ToFriendlyName());
            }
            else if (numberOfConditionalRegistrations == 1)
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

        private static string SuppliedTypeIsNotOpenGenericExplainingAlternatives(Type type, string registeringElement) =>
            string.Format(CultureInfo.InvariantCulture,
                "Supply this method with the open generic type {0} to register all available " +
                "implementations of this type, or call {2}(Type, IEnumerable<{1}>) either with the open " +
                "or closed version of that type to register a collection of instances based on that type.",
                Helpers.ToCSharpFriendlyName(type.GetGenericTypeDefinition()),
                registeringElement,
                nameof(Container.RegisterCollection));

        private static string ToTypeOCfSharpFriendlyList(IEnumerable<Type> types) => 
            string.Join(", ",
                from type in types
                select string.Format(CultureInfo.InvariantCulture, "typeof({0})", Helpers.ToCSharpFriendlyName(type)));

        private static string SuppliedTypeIsNotGenericExplainingAlternatives(Type type, string registeringElement) => 
            string.Format(CultureInfo.InvariantCulture,
                "This method only supports open generic types. " +
                "If you meant to register all available implementations of {0}, call " +
                "{2}(typeof({0}), IEnumerable<{1}>) instead.",
                type.ToFriendlyName(),
                registeringElement,
                nameof(Container.RegisterCollection));

        private static object ContainerHasNoRegistrationsAddition(bool containerHasRegistrations) =>
            containerHasRegistrations
                ? string.Empty
                : " Please note that the container instance you are resolving from contains no " +
                  "registrations. Could it be that you accidentally created a new -and empty- container?";

        private static object DidYouMeanToCallGetInstanceInstead(bool hasRelatedOneToOneMapping, 
            Type collectionServiceType) =>
            hasRelatedOneToOneMapping
                ? string.Format(CultureInfo.InvariantCulture,
                    " There is, however, a registration for {0}; Did you mean to call GetInstance<{0}>() " +
                    "or depend on {0}?",
                    collectionServiceType.GetGenericArguments()[0].ToFriendlyName())
                : string.Empty;

        private static string DidYouMeanToDependOnNonCollectionInstead(bool hasRelatedOneToOneMapping,
            Type collectionServiceType) =>
            hasRelatedOneToOneMapping
                ? string.Format(CultureInfo.InvariantCulture,
                    " There is, however, a registration for {0}; Did you mean to depend on {0}?",
                    collectionServiceType.GetGenericArguments()[0].ToFriendlyName())
                : string.Empty;

        private static string DidYouMeanToCallGetAllInstancesInstead(bool hasCollection, Type serviceType) =>
            hasCollection
                ? string.Format(CultureInfo.InvariantCulture,
                    " There is, however, a registration for {0}; Did you mean to call " +
                    "GetAllInstances<{1}>() or depend on {0}?",
                    typeof(IEnumerable<>).MakeGenericType(serviceType).ToFriendlyName(),
                    serviceType.ToFriendlyName())
                : string.Empty;

        private static string DidYouMeanToDependOnCollectionInstead(bool hasCollection, Type serviceType) =>
            hasCollection
                ? string.Format(CultureInfo.InvariantCulture,
                    " There is, however, a registration for {0}; Did you mean to depend on {0}?",
                    typeof(IEnumerable<>).MakeGenericType(serviceType).ToFriendlyName())
                : string.Empty;

        private static string NoteThatSkippedDecoratorsWereFound(Type serviceType, Type[] decorators) =>
            decorators.Any()
                ? string.Format(CultureInfo.InvariantCulture,
                    " Note that {0} {1} found as implementation of {2}, but {1} skipped during batch-" +
                    "registration by the container because {3} considered to be a decorator (because {4} " +
                    "a cyclic reference to {5}).",
                    Helpers.ToCommaSeparatedText(decorators.Select(Helpers.ToFriendlyName)),
                    decorators.Length == 1 ? "was" : "were",
                    serviceType.GetGenericTypeDefinition().ToFriendlyName(),
                    decorators.Length == 1 ? "it is" : "there are",
                    decorators.Length == 1 ? "it contains" : "they contain",
                    decorators.Length == 1 ? "itself" : "themselves")
                : string.Empty;
    }
}