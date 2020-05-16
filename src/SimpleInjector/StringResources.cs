// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

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

    /// <summary>Internal helper for string resources.</summary>
    internal static class StringResources
    {
        private const string CollectionsRegisterMethodName =
            nameof(Container) + "." + nameof(Container.Collection) + "." + nameof(ContainerCollectionRegistrator.Register);

        private const string EnableAutoVerificationPropertyName =
            nameof(Container) + "." + nameof(Container.Options) + "." + nameof(ContainerOptions.EnableAutoVerification);

        private const string CollectionsAppendMethodName =
            nameof(Container) + "." + nameof(Container.Collection) + "." + nameof(ContainerCollectionRegistrator.Append);

        // Assembly.Location only exists in .NETStandard1.5 and up, .NET4.0 and PCL, but we only compile
        // against .NETStandard1.0 and .NETStandard1.3. We don't want to add an extra build directive solely
        // for the Location property.
        private static readonly PropertyInfo AssemblyLocationProperty =
            typeof(Assembly).GetProperties().SingleOrDefault(p => p.Name == "Location");

        internal static bool UseFullyQualifiedTypeNames { get; set; }

        internal static string ContainerCanNotBeChangedAfterUse(string? stackTrace)
        {
            string message = Format(
                "The container can't be changed after the first call to {0}, {1}, {2}, and some calls of {3}. " +
                "Please see https://simpleinjector.org/locked to understand why the container is locked.",
                nameof(Container.GetInstance),
                nameof(Container.GetAllInstances),
                nameof(Container.Verify),
                nameof(Container.GetRegistration));

            if (stackTrace == null)
            {
                return message;
            }

            return message +
                " The following stack trace describes the location where the container was locked:" +
                Environment.NewLine + Environment.NewLine + stackTrace;
        }

        internal static string ContainerCanNotBeUsedAfterDisposal(Type type, string? stackTrace)
        {
            string message = Format(
                "Cannot access a disposed object.{0}Object name: '{1}'.",
                Environment.NewLine,
                type.FullName);

            if (stackTrace == null)
            {
                return message;
            }

            return message + Environment.NewLine +
                "The following stack trace describes the location where the container was disposed:" +
                Environment.NewLine + Environment.NewLine + stackTrace;
        }

        internal static string DelegateForTypeReturnedNull(Type serviceType) =>
            Format(
                "The registered delegate for type {0} returned null.",
                serviceType.TypeName());

        internal static string ResolveInterceptorDelegateReturnedNull() =>
            Format(
                "The delegate that was registered using '{0}' returned null.",
                nameof(ContainerOptions.RegisterResolveInterceptor));

        internal static string ErrorWhileBuildingDelegateFromExpression(
            Type serviceType, Expression expression, Exception exception) =>
            Format(
                "Error occurred while trying to build a delegate for type {0} using the expression \"{1}\". " +
                "{2}",
                serviceType.TypeName(),
                expression,
                exception.Message);

        internal static string DelegateForTypeThrewAnException(Type serviceType) =>
            Format(
                "The registered delegate for type {0} threw an exception.",
                serviceType.TypeName());

        internal static string NoRegistrationForTypeFound(
            Type serviceType,
            bool containerHasRegistrations,
            bool collectionRegistrationDoesNotExists,
            bool containerHasRelatedOneToOneMapping,
            bool containerHasRelatedCollectionMapping,
            Type[] skippedDecorators,
            Type[] lookalikes) =>
            Format(
                "No registration for type {0} could be found.{1}{2}{3}{4}{5}{6}",
                serviceType.TypeName(),
                ContainerHasNoRegistrationsAddition(containerHasRegistrations),
                DidYouMeanToCallGetInstanceInstead(containerHasRelatedOneToOneMapping, serviceType),
                NoCollectionRegistrationExists(collectionRegistrationDoesNotExists, serviceType),
                DidYouMeanToCallGetAllInstancesInstead(containerHasRelatedCollectionMapping, serviceType),
                NoteThatSkippedDecoratorsWereFound(serviceType, skippedDecorators),
                NoteThatTypeLookalikesAreFound(serviceType, lookalikes),
                NoCollectionRegistrationExists(false, serviceType));

        internal static string KnownImplementationTypeShouldBeAssignableFromExpressionType(
            Type knownImplementationType, Type currentExpressionType) =>
            Format(
                "You are trying to set the {0}.{1} property with an Expression instance that has a type " +
                "of {2}. The expression type however should be a {3} (or a sub type). You can't change " +
                "the type of the expression using the {4} event. If you need to change the " +
                "implementation, please use the {5} event instead.",
                nameof(ExpressionBuildingEventArgs),
                nameof(ExpressionBuildingEventArgs.Expression),
                currentExpressionType.TypeName(),
                knownImplementationType.TypeName(),
                nameof(Container.ExpressionBuilding),
                nameof(Container.ExpressionBuilt));

        internal static string MultipleClosedTypesAreAssignableFromType(
            Type type, Type genericTypeDefinition, Type[] types, string otherMethod) =>
            Format(
                "Your request is ambiguous. " +
                "There are multiple closed version of {0} that are assignable from {1}, namely: {2}. " +
                "Use {3} instead to get this list of closed types to select the proper type.",
                genericTypeDefinition.TypeName(),
                type.TypeName(),
                types.Select(TypeName).ToCommaSeparatedText(),
                otherMethod);

        internal static string TypeIsNotAssignableFromOpenGenericType(Type type, Type genericTypeDefinition) =>
            Format(
                "None of the base classes or implemented interfaces of {0}, nor {0} itself are a closed " +
                "type of {1}.",
                type.TypeName(),
                genericTypeDefinition.TypeName());

        internal static string OpenGenericTypesCanNotBeResolved(Type serviceType) =>
            Format(
                "The request for type {0} is invalid because it is an open-generic type: it is only " +
                "possible to instantiate instances of closed-generic types. A generic type is closed if " +
                "all of its type parameters have been substituted with types that are recognized by the " +
                "compiler.",
                serviceType.TypeName());

        internal static string LifestyleMismatchesReported(LifestyleMismatchDiagnosticResult error) =>
            Format(
                "A lifestyle mismatch has been detected. {0} {1} " +
                "Please see https://simpleinjector.org/dialm to understand this problem and how to solve it.",
                error.Description,
                LifestyleMismatchesCanCauseConcurrencyBugs());

        internal static string LifestyleMismatchesCanCauseConcurrencyBugs() =>
            "Lifestyle mismatches can cause concurrency bugs in your application.";

        internal static string DiagnosticWarningsReported(IList<DiagnosticResult> errors)
        {
            var descriptions =
                from error in errors
                select Format("-[{0}] {1}", error.Name, error.Description);

            return Format(
                "The configuration is invalid. The following diagnostic warnings were reported:{1}{0}{1}" +
                "See the Error property for detailed information about the warnings. " +
                "Please see https://simpleinjector.org/diagnostics how to fix problems and how to suppress " +
                "individual warnings.",
                string.Join(Environment.NewLine, descriptions.Distinct()),
                Environment.NewLine);
        }

        internal static string EnableAutoVerificationIsEnabled(string innerMessage) =>
            innerMessage + $" Verification was triggered because {EnableAutoVerificationPropertyName} was " +
            "enabled. To prevent the container from being verified on first resolve, set " +
            $"{EnableAutoVerificationPropertyName} to false.";

        internal static string ConfigurationInvalidCreatingInstanceFailed(
            Type serviceType, Exception exception) =>
            Format(
                "The configuration is invalid. Creating the instance for type {0} failed. {1}",
                serviceType.TypeName(),
                exception.Message);

        internal static string ConfigurationInvalidIteratingCollectionFailed(
            Type serviceType, Exception exception) =>
            Format(
                "The configuration is invalid. Iterating the collection for type {0} failed. {1}",
                serviceType.TypeName(),
                exception.Message);

        internal static string ConfigurationInvalidCollectionContainsNullElements(
            Type firstInvalidServiceType) =>
            Format(
                "The configuration is invalid. One of the items in the collection for type {0} is " +
                "a null reference.",
                firstInvalidServiceType.TypeName());

        internal static string TypeAlreadyRegistered(Type serviceType) =>
            Format(
                "Type {0} has already been registered. If your intention is to resolve a collection of " +
                "{0} implementations, use the {1} overloads. " +
                "For more information, see https://simpleinjector.org/coll1. " +
                "If your intention is to replace the existing registration with this new registration, " +
                "you can allow overriding the current registration by setting {2}.{3} to true. " +
                "For more information, see https://simpleinjector.org/ovrrd.",
                serviceType.TypeName(),
                CollectionsRegisterMethodName,
                nameof(Container) + "." + nameof(Container.Options),
                nameof(ContainerOptions.AllowOverridingRegistrations));

        internal static string ScopePropertyCanOnlyBeUsedWhenDefaultScopedLifestyleIsConfigured() =>
            "To be able to use the Lifestyle.Scoped property, please ensure that the container is " +
            "configured with a default scoped lifestyle by setting the Container.Options." +
            "DefaultScopedLifestyle property with the required scoped lifestyle for your type of " +
            "application. For more information, see https://simpleinjector.org/scoped.";

        internal static string MakingConditionalRegistrationsInOverridingModeIsNotSupported() =>
            "The making of conditional registrations is not supported when " +
            $"{nameof(ContainerOptions.AllowOverridingRegistrations)} is set, because it is impossible " +
            "for the container to detect whether the registration should replace a different registration " +
            "or not.";

        internal static string MakingRegistrationsWithTypeConstraintsInOverridingModeIsNotSupported() =>
            MakingConditionalRegistrationsInOverridingModeIsNotSupported() +
            " Your registration is considered conditional, because of its generic type constraints. " +
            "This makes Simple Injector apply it conditionally, based on its type constraints.";

        internal static string NonGenericTypeAlreadyRegisteredAsConditionalRegistration(Type serviceType) =>
            NonGenericTypeAlreadyRegistered(serviceType, existingRegistrationIsConditional: true);

        internal static string CollectionUsedDuringConstruction(
            Type consumer, InstanceProducer producer, KnownRelationship? relationship = null) =>
            relationship != null && IsListOrArrayRelationship(relationship)
                ? CollectionUsedDuringConstructionByInjectingMutableCollection(consumer, producer, relationship!)
                : CollectionUsedDuringConstructionByIteratingAStream(consumer, producer, relationship);

        internal static string UnregisteredAbstractionFoundInCollection(
            Type serviceType, Type registeredType, Type foundAbstractType) =>
            Format(
                "The registration for the collection of {0} (i.e. IEnumerable<{0}>) is supplied with the " +
                "abstract type {1}, which hasn't been registered explicitly, and wasn't resolved using " +
                "unregistered type resolution. For Simple Injector to be able to resolve this collection, " +
                "an explicit one-to-one registration is required, e.g. " +
                "Container.Register<{2}, MyImpl>(). Otherwise, in case {1} was supplied by accident, make " +
                "sure it is removed. Please see https://simpleinjector.org/collections for more " +
                "information about registering and resolving collections.",
                serviceType.TypeName(),
                registeredType.TypeName(),
                foundAbstractType.TypeName());

        internal static string NonGenericTypeAlreadyRegisteredAsUnconditionalRegistration(Type serviceType) =>
            NonGenericTypeAlreadyRegistered(serviceType, existingRegistrationIsConditional: false);

        internal static string CollectionTypeAlreadyRegistered(Type serviceType) =>
            Format(
                "Collection of items for type {0} has already been registered " +
                "and the container is currently not configured to allow overriding registrations. " +
                "If your intention is to replace the existing collection with this new one, " +
                "you can allow overriding the current registration by setting Container.{1}.{2} to true. " +
                "In case it is your goal to append items to an already registered collection, please use " +
                "the Container.{3}.{4} method overloads. " +
                "More info on overriding registration: https://simpleinjector.org/ovrrd.",
                serviceType.TypeName(),
                nameof(ContainerOptions),
                nameof(ContainerOptions.AllowOverridingRegistrations),
                nameof(Container.Collection),
                nameof(ContainerCollectionRegistrator.Append));

        internal static string ParameterTypeMustBeRegistered(
            Container container,
            InjectionTargetInfo target,
            int numberOfConditionals,
            bool hasRelatedOneToOneMapping,
            bool collectionRegistrationDoesNotExists,
            bool hasRelatedCollectionMapping,
            Type[] skippedDecorators,
            Type[] lookalikes)
        {
            var formatString = target.Parameter != null
                ? "The constructor of type {0} contains the parameter "
                : "Type {0} contains the property ";

            formatString +=
                "with name '{1}' and type {2}, but {2} is not registered. " +
                "For {2} to be resolved, it must be registered in the container.{3}";

            string extraInfo = string.Concat(
                GetAdditionalInformationAboutExistingConditionalRegistrations(target, numberOfConditionals),
                DidYouMeanToDependOnNonCollectionInstead(hasRelatedOneToOneMapping, target.TargetType),
                NoCollectionRegistrationExists(collectionRegistrationDoesNotExists, target.TargetType),
                DidYouMeanToDependOnCollectionInstead(hasRelatedCollectionMapping, target.TargetType),
                NoteThatSkippedDecoratorsWereFound(target.TargetType, skippedDecorators),
                NoteThatConcreteTypeCanNotBeResolvedDueToConfiguration(container, target),
                NoteThatTypeLookalikesAreFound(target.TargetType, lookalikes, numberOfConditionals));

            return Format(
                formatString,
                target.Member.DeclaringType.TypeName(),
                target.Name,
                target.TargetType.TypeName(),
                extraInfo);
        }

        internal static string TypeMustHaveASinglePublicConstructorButItHasNone(Type serviceType) =>
            Format(
                "For the container to be able to create {0} it should have only one public constructor: " +
                "it has none.",
                serviceType.TypeName());

        internal static string TypeMustHaveASinglePublicConstructorButItHas(Type serviceType, int count) =>
            Format(
                "For the container to be able to create {0} it should have only one public constructor: " +
                "it has {1}. See https://simpleinjector.org/one-constructor for more " +
                "information.",
                serviceType.TypeName(),
                count);

        internal static string TypeMustNotContainInvalidInjectionTarget(InjectionTargetInfo invalidTarget)
        {
            string reason = string.Empty;

            if (invalidTarget.TargetType.IsValueType())
            {
                reason = " because it is a value type";
            }

            if (invalidTarget.Parameter != null)
            {
                return Format(
                    "The constructor of type {0} contains parameter '{1}' of type {2}, which can not be " +
                    "used for constructor injection{3}.",
                    invalidTarget.Member.DeclaringType.TypeName(),
                    invalidTarget.Name,
                    invalidTarget.TargetType.TypeName(),
                    reason);
            }
            else
            {
                return Format(
                    "The type {0} contains property '{1}' of type {2}, which can not be used for property " +
                    "injection{3}.",
                    invalidTarget.Member.DeclaringType.TypeName(),
                    invalidTarget.Name,
                    invalidTarget.TargetType.TypeName(),
                    reason);
            }
        }

        internal static string TypeShouldBeConcreteToBeUsedOnThisMethod(Type serviceType) =>
            Format(
                "The given type {0} is not a concrete type. Please use one of the other overloads to " +
                "register this type.",
                serviceType.TypeName());

        internal static string MultipleObserversRegisteredTheSameTypeToResolveUnregisteredType(
            Type unregisteredServiceType) =>
            Format(
                "Multiple observers of the {0} event are registering a delegate for the same service " +
                "type: {1}. Make sure only one of the registered handlers calls the {2}.{3} method for a " +
                "given service type.",
                nameof(Container.ResolveUnregisteredType),
                unregisteredServiceType.TypeName(),
                nameof(UnregisteredTypeEventArgs),
                nameof(UnregisteredTypeEventArgs.Register));

        internal static string ImplicitRegistrationCouldNotBeMadeForType(
            Type serviceType, bool containerHasRegistrations) =>
            Format(
                "No registration for type {0} could be found and an implicit registration could not be " +
                "made.{1}",
                serviceType.TypeName(),
                ContainerHasNoRegistrationsAddition(containerHasRegistrations));

        internal static string ImplicitRegistrationCouldNotBeMadeForType(
            Container container, Type serviceType, bool containerHasRegistrations) =>
            Format(
                "No registration for type {0} could be found. Make sure {0} is registered, for instance by " +
                "calling '{1}' during the registration phase.{2}{3}",
                serviceType.TypeName(),
                nameof(Container) + "." + nameof(Container.Register) +
                    "<" + serviceType.ToFriendlyName(fullyQualifiedName: false) + ">();",
                NoteThatConcreteTypeCanNotBeResolvedDueToConfiguration(container, serviceType),
                ContainerHasNoRegistrationsAddition(containerHasRegistrations));

        internal static string DefaultScopedLifestyleCanNotBeSetWithLifetimeScoped() =>
            $"{nameof(ContainerOptions.DefaultScopedLifestyle)} can't be set with the value of " +
            $"{nameof(Lifestyle)}.{nameof(Lifestyle.Scoped)}.";

        internal static string TypeDependsOnItself(Type serviceType) =>
            Format(
                "The configuration is invalid. The type {0} is directly or indirectly depending on itself.",
                serviceType.TypeName());

        internal static string CyclicDependencyGraphMessage(IEnumerable<Type> dependencyCycle) =>
            Format(
                "The cyclic graph contains the following types: {0}.",
                string.Join(" -> ", dependencyCycle.Select(TypeName)));

        internal static string UnableToResolveTypeDueToSecurityConfiguration(
            Type serviceType, Exception innerException) =>
            Format(
                "Unable to resolve type {0}. The security restrictions of your application's sandbox do " +
                "not permit the creation of this type. Explicitly register the type using one of the " +
                "generic '{2}' overloads or consider making it public. {1}",
                serviceType.TypeName(),
                innerException.Message,
                nameof(Container.Register));

        internal static string UnableToInjectPropertiesDueToSecurityConfiguration(Type serviceType,
            Exception innerException) =>
            Format(
                "Unable to inject properties into type {0}. The security restrictions of your application's " +
                "sandbox do not permit the injection of one of its properties. Consider making it public. {1}",
                serviceType.TypeName(),
                innerException.Message);

        internal static string UnableToInjectImplicitPropertiesDueToSecurityConfiguration(Type injectee,
            Exception innerException) =>
            Format(
                "Unable to inject properties into type {0}. The security restrictions of your application's " +
                "sandbox do not permit the creation of one of its dependencies. Explicitly register that " +
                "dependency using one of the generic '{2}' overloads or consider making it public. {1}",
                injectee.TypeName(),
                innerException.Message,
                nameof(Container.Register));

        internal static string PropertyCanNotBeChangedAfterTheFirstRegistration(string propertyName) =>
            $"The {propertyName} property cannot be changed after the first registration has " +
            "been made to the container.";

        internal static string CollectionsRegisterCalledWithTypeAsTService(IEnumerable<Type> types) =>
            TypeIsAmbiguous(typeof(Type)) + " " + Format(
                "The most likely cause of this happening is because the C# overload resolution picked " +
                "a different method for you than you expected to call. The method C# selected for you is: " +
                "{3}<Type>(new[] {{ {0} }}). Instead, you probably intended to call: " +
                "{3}(typeof({1}), new[] {{ {2} }}).",
                ToTypeOCfSharpFriendlyList(types),
                CSharpFriendlyName(types.First()),
                ToTypeOCfSharpFriendlyList(types.Skip(1)),
                CollectionsRegisterMethodName);

        internal static string TypeIsAmbiguous(Type serviceType) =>
            Format(
                "You are trying to register {0} as a service type, but registering this type is not " +
                "allowed to be registered because the type is ambiguous. The registration of such a type " +
                "almost always indicates a flaw in the design of the application and is therefore not " +
                "allowed. Please change any component that depends on a dependency of this type. Ensure " +
                "that the container does not have to inject any dependencies of this type by injecting a " +
                "different type.",
                serviceType.TypeName());

        internal static string SuppliedTypeIsNotAReferenceType(Type type) =>
            Format(
                "The supplied type {0} is not a reference type. Only reference types are supported.",
                type.TypeName());

        internal static string SuppliedTypeIsAnOpenGenericType(Type type) =>
            Format(
                "The supplied type {0} is an open-generic type. This type cannot be used for registration " +
                "using this method.",
                type.TypeName());

        internal static string SuppliedTypeIsAnOpenGenericTypeWhileTheServiceTypeIsNot(Type type) =>
            Format(
                "The supplied type {0} is an open-generic type. This type cannot be used for registration " +
                "of collections of non-generic types.",
                type.TypeName());

        internal static string SuppliedElementDoesNotInheritFromOrImplement(
            Type serviceType, Type elementType, string elementDescription) =>
            Format(
                "The supplied {0} of type {1} does not {2} {3}.",
                elementDescription,
                elementType.TypeName(),
                serviceType.IsInterface() ? "implement" : "inherit from",
                serviceType.TypeName());

        internal static string SuppliedTypeDoesNotInheritFromOrImplement(Type service, Type implementation) =>
            Format(
                "The supplied type {0} does not {1} {2}.",
                implementation.TypeName(),
                service.IsInterface() ? "implement" : "inherit from",
                service.TypeName());

        internal static string DependencyInjectionBehaviorReturnedNull(IDependencyInjectionBehavior behavior) =>
            Format(
                "The {0} that was registered through the Container.{1}.{2} property, returned a null " +
                "reference from its {3} method. {4}.{3} implementations should not return null when " +
                "supplied with throwOnFailure = true, but should throw an {5} with an expressive message " +
                "instead.",
                behavior.GetType().TypeName(),
                nameof(Container.Options),
                nameof(ContainerOptions.DependencyInjectionBehavior),
                nameof(IDependencyInjectionBehavior.GetInstanceProducer),
                nameof(IDependencyInjectionBehavior),
                typeof(ActivationException).FullName);

        internal static string LifestyleSelectionBehaviorReturnedNull(
            ILifestyleSelectionBehavior selectionBehavior, Type implementationType) =>
            Format(
                "The {0} that was registered through Container.{3}.{4} returned a null reference after " +
                "its {5} method was supplied with implementationType '{1}'. {2}.{5} implementations " +
                "should never return null.",
                selectionBehavior.GetType().TypeName(),
                implementationType.TypeName(),
                nameof(ILifestyleSelectionBehavior),
                nameof(Container.Options),
                nameof(ContainerOptions.LifestyleSelectionBehavior),
                nameof(ILifestyleSelectionBehavior.SelectLifestyle));

        internal static string TypeHasNoInjectableConstructorAccordingToCustomResolutionBehavior(
            IConstructorResolutionBehavior behavior, Type implementationType) =>
            Format(
                "For the container to be able to create {0} it should have a constructor that can be " +
                "called, but according to the customly configured {1} of type {2}, there is no " +
                "selectable constructor. The {2}, however, didn't supply a reason why.",
                implementationType.TypeName(),
                nameof(IConstructorResolutionBehavior),
                behavior.GetType().TypeName());

        internal static string DependencyNotValidForInjectionAccordingToCustomInjectionBehavior(
            IDependencyInjectionBehavior behavior, InjectionConsumerInfo dependency) =>
            Format(
                "The conumer-dependency pair {0} is not valid for injection according to the custom {1} of" +
                "type {2}. The {2}, however, didn't supply a reason why.",
                dependency,
                nameof(IDependencyInjectionBehavior),
                behavior.GetType().TypeName());

        internal static string RegistrationReturnedNullFromBuildExpression(
            Registration lifestyleRegistration) =>
            Format(
                "The {0} for the {1} returned a null reference from its {2} method.",
                lifestyleRegistration.GetType().TypeName(),
                lifestyleRegistration.Lifestyle.GetType().TypeName(),
                nameof(Registration.BuildExpression));

        internal static string MultipleTypesThatRepresentClosedGenericType(
            Type closedServiceType, Type[] implementations) =>
            Format(
                "In the supplied list of types or assemblies, there are {0} types that represent the " +
                "same closed-generic type {1}. Did you mean to register the types as a collection " +
                "using the {2} method instead? Conflicting types: {3}.",
                implementations.Length,
                closedServiceType.TypeName(),
                CollectionsRegisterMethodName,
                implementations.Select(TypeName).ToCommaSeparatedText());

        internal static string CantGenerateFuncForDecorator(
            Type serviceType, Type decorateeFactoryType, Type decoratorType) =>
            Format(
                "It's impossible for the container to generate a {3} for injection into the {1} " +
                "decorator that will be wrapped around instances of the collection of {0} instances, " +
                "because the registered collection is not controlled by the container. The collection is " +
                "considered to be container-uncontrolled collection, because the registration was made " +
                "using either the {2}<{0}>(IEnumerable<{0}>) or {2}(Type, IEnumerable) overloads. " +
                "It is impossible for the container to determine its lifestyle of an element in a " +
                "container-uncontrolled collections, which makes it impossible to generate a {3} for {1}. " +
                "Either switch to one of the other {2} overloads, or use a decorator that depends on {0} " +
                "instead of {3}.",
                serviceType.TypeName(),
                decoratorType.TypeName(),
                CollectionsRegisterMethodName,
                decorateeFactoryType.TypeName());

        internal static string ScopeSuppliedToScopedDecorateeFactoryMustHaveAContainer<TService>() =>
            Format(
                "For scoped decoratee factories to function, they have to be supplied with a Scope " +
                "instance that references the Container for which the object graph has been built. But the " +
                "Scope instance, provided to this {0} delegate does not belong to any container. Please " +
                "ensure the supplied Scope instance is created using the constructor overload that accepts " +
                "a Container instance.",
                typeof(Func<Scope, TService>).TypeName());

        internal static string ScopeSuppliedToScopedDecorateeFactoryMustBeForSameContainer<TService>() =>
            Format(
                "For scoped decoratee factories to function, they have to be supplied with a Scope " +
                "instance that references the Container for which the object graph has been built. But the " +
                "Scope instance, provided to this {0} delegate, references a different Container instance.",
                typeof(Func<Scope, TService>).TypeName());

        internal static string SuppliedTypeIsNotAGenericType(Type type) =>
            Format("The supplied type {0} is not a generic type.", type.TypeName());

        internal static string SuppliedTypeIsNotAnOpenGenericType(Type type) =>
            Format("The supplied type {0} is not an open-generic type.", type.TypeName());

        internal static string SuppliedTypeCanNotBeOpenWhenDecoratorIsClosed() =>
            "Registering a closed-generic service type with an open-generic decorator is not " +
            "supported. Instead, register the service type as open generic, and the decorator type as " +
            "closed generic.";

        internal static string TheConstructorOfTypeMustContainTheServiceTypeAsArgument(
            Type decoratorType, Type serviceType) =>
            Format(
                "For the container to be able to use {0} as a decorator, its constructor must include a " +
                "single parameter of type {1} (or Func<{1}>) - i.e. the type of the instance that is " +
                "being decorated. The parameter type {1} does not currently exist in the constructor of " +
                "class {0}.",
                decoratorType.TypeName(),
                serviceType.TypeName());

        internal static string TheConstructorOfTypeMustContainASingleInstanceOfTheServiceTypeAsArgument(
            Type decoratorType, Type serviceType) =>
            Format(
                "For the container to be able to use {0} as a decorator, its constructor must include a " +
                "single parameter of type {1} (or Func<{1}>) - i.e. the type of the instance that is " +
                "being decorated. The parameter type {1} is defined multiple times in the constructor of " +
                "class {0}.",
                decoratorType.TypeName(),
                serviceType.TypeName());

        internal static string OpenGenericTypeContainsUnresolvableTypeArguments(
            Type openGenericImplementation) =>
            Format(
                "The supplied type {0} contains unresolvable type arguments. " +
                "The type would never be resolved and is therefore not suited to be used.",
                openGenericImplementation.TypeName());

        internal static string DecoratorCanNotBeAGenericTypeDefinitionWhenServiceTypeIsNot(
            Type serviceType, Type decoratorType) =>
            Format(
                "The supplied decorator {0} is an open-generic type definition, while the supplied " +
                "service type {1} is not.",
                decoratorType.TypeName(),
                serviceType.TypeName());

        internal static string TheSuppliedRegistrationBelongsToADifferentContainer() =>
            "The supplied Registration belongs to a different Container instance.";

        internal static string CanNotDecorateContainerUncontrolledCollectionWithThisLifestyle(
            Type decoratorType, Lifestyle lifestyle, Type serviceType) =>
            Format(
                "You are trying to apply the {0} decorator with the '{1}' lifestyle to a collection of " +
                "type {2}, but the registered collection is not controlled by the container. Because the " +
                "number of returned items might change on each call, the decorator with this lifestyle " +
                "cannot be applied to the collection. Instead, register the decorator with the Transient " +
                "lifestyle, or use one of the {3} overloads that takes a collection of System.Type types.",
                decoratorType.TypeName(),
                lifestyle.Name,
                serviceType.TypeName(),
                CollectionsRegisterMethodName);

        internal static string PropertyHasNoSetter(PropertyInfo property) =>
            Format(
                "The property named '{0}' with type {1} and declared on type {2} can't be used for " +
                "injection, because it has no set method.",
                property.Name,
                property.PropertyType.TypeName(),
                property.DeclaringType.TypeName());

        internal static string PropertyIsStatic(PropertyInfo property) =>
            Format(
                "Property of type {0} with name '{1}' can't be used for injection, because it is static.",
                property.PropertyType.TypeName(),
                property.Name);

        internal static string ThisOverloadDoesNotAllowOpenGenerics(
            Type openGenericServiceType, Type[] openGenericTypes, Type[] closedAndNonGenericTypes) =>
            Format(
                "The supplied list of types contains {0} open-generic {1}, but this method is unable to " +
                "handle open-generic implementations—it can only map a single implementation to " +
                "closed-generic service types. {2}You must register {3} open-generic {1} separately using " +
                "the Register(Type, Type) overload. Alternatively, try using {4} " +
                "instead, if you expect to have multiple implementations per closed-generic service type " +
                "and want to inject a collection of them into consumers. Invalid {1}: {5}.",
                openGenericTypes.Length == 1 ? "an" : "multiple",
                openGenericTypes.Length == 1 ? "type" : "types",
                ThisOverloadDoesNotAllowOpenGenericsExample(
                    openGenericServiceType: openGenericServiceType,
                    openGenericTypes: openGenericTypes,
                    firstClosedAndNonGenericType: closedAndNonGenericTypes.FirstOrDefault()),
                openGenericTypes.Length == 1 ? "this" : "these",
                CollectionsRegisterMethodName,
                openGenericTypes.Count() > 1 ? "types" : "type",
                openGenericTypes.Select(TypeName).ToCommaSeparatedText());

        internal static string ThisOverloadDoesNotAllowOpenGenericsExample(
            Type openGenericServiceType, Type[] openGenericTypes, Type firstClosedAndNonGenericType) =>
            firstClosedAndNonGenericType != null
                ? Format(
                    "As an example, the supplied type {0} can be used as implementation, because it " +
                    "implements the closed-generic service type {1}. The supplied open-generic {2}, " +
                    "however, can't be mapped to a closed-generic service because of its generic type {3}. ",
                    firstClosedAndNonGenericType.TypeName(),
                    firstClosedAndNonGenericType.GetBaseTypesAndInterfacesFor(openGenericServiceType)
                        .First().TypeName(),
                    openGenericTypes.First().TypeName(),
                    openGenericTypes.First().GetGenericArguments().Length == 1 ? "argument" : "arguments")
                : string.Empty;

        internal static string AppendingRegistrationsToContainerUncontrolledCollectionsIsNotSupported(
            Type serviceType) =>
            Format(
                "You are trying to append a registration to the registered collection of {0} instances, " +
                "which is either registered using {1}<TService>(IEnumerable<TService>) or " +
                "{1}(Type, IEnumerable). Because the number of returned items might change on each call, " +
                "appending registrations to these collections is not supported. Please register the " +
                "collection with one of the other {1} overloads if appending is required.",
                serviceType.TypeName(),
                CollectionsRegisterMethodName);

        internal static string UnregisteredTypeEventArgsRegisterDelegateReturnedUncastableInstance(
            Type serviceType, InvalidCastException exception) =>
            Format(
                "The delegate that was registered for service type {0} using the {2}.{3}(Func<object>) " +
                "method returned an object that couldn't be casted to {0}. {1}",
                serviceType.TypeName(),
                exception.Message,
                nameof(UnregisteredTypeEventArgs),
                nameof(UnregisteredTypeEventArgs.Register));

        internal static string UnregisteredTypeEventArgsRegisterDelegateThrewAnException(
            Type serviceType, Exception exception) =>
            Format(
                "The delegate that was registered for service type {0} using the {2}.{3}(Func<object>) " +
                "method threw an exception. {1}",
                serviceType.TypeName(),
                exception.Message,
                nameof(UnregisteredTypeEventArgs),
                nameof(UnregisteredTypeEventArgs.Register));

        internal static string TheServiceIsRequestedOutsideTheContextOfAScopedLifestyle(
            Type serviceType, ScopedLifestyle lifestyle) =>
            Format(
                "{0} is registered using the '{1}' lifestyle, but the instance is requested outside the " +
                "context of an active ({1}) scope. Please see https://simpleinjector.org/scoped " +
                "for more information about how apply lifestyles and manage scopes.",
                serviceType.TypeName(),
                lifestyle.Name);

        internal static string ThisMethodCanOnlyBeCalledWithinTheContextOfAnActiveScope(
            ScopedLifestyle lifestyle) =>
            Format(
                "This method can only be called within the context of an active ({0}) scope.",
                lifestyle.Name);

        internal static string DecoratorFactoryReturnedNull(Type serviceType) =>
            Format(
                "The decorator type factory delegate that was registered for service type {0} returned null.",
                serviceType.TypeName());

        internal static string FactoryReturnedNull(Type serviceType) =>
            Format(
                "The type factory delegate that was registered for service type {0} returned null.",
                serviceType.TypeName());

        internal static string ImplementationTypeFactoryReturnedNull(Type serviceType) =>
            Format(
                "The implementation type factory delegate that was registered for service type {0} " +
                "returned null.",
                serviceType.TypeName());

        internal static string TheDecoratorReturnedFromTheFactoryShouldNotBeOpenGeneric(
            Type serviceType, Type decoratorType) =>
            Format(
                "The registered decorator type factory returned open-generic type {0} while the registered " +
                "service type {1} is not generic, making it impossible for a closed-generic decorator type " +
                "to be constructed.",
                decoratorType.TypeName(),
                serviceType.TypeName());

        internal static string TheTypeReturnedFromTheFactoryShouldNotBeOpenGeneric(
            Type serviceType, Type implementationType) =>
            Format(
                "The registered type factory returned open-generic type {0} while the registered service " +
                "type {1} is not generic, making it impossible for a closed-generic type to be constructed.",
                implementationType.TypeName(),
                serviceType.TypeName());

        internal static string TypeFactoryReturnedIncompatibleType(
            Type serviceType, Type implementationType) =>
            Format(
                "The registered type factory returned type {0} which does not implement {1}.",
                implementationType.TypeName(),
                serviceType.TypeName());

        internal static string RecursiveInstanceRegistrationDetected() =>
            "A recursive registration of Action or IDisposable instances was detected during disposal " +
            "of the scope. This is possibly caused by a component that is directly or indirectly " +
            "depending on itself.";

        internal static string GetRootRegistrationsCanNotBeCalledBeforeVerify() =>
            "Root registrations can't be determined before Verify is called. Please call Verify first.";

        internal static string VisualizeObjectGraphShouldBeCalledAfterTheExpressionIsCreated() =>
            Format(
                "This method can only be called after {0}() or {1}() have been called.",
                nameof(Container.GetInstance),
                nameof(Registration.BuildExpression));

        internal static string MixingCallsToCollectionsRegisterIsNotSupported(Type serviceType) =>
            Format(
                "Mixing calls to {1} for the same open-generic service type is not supported. Consider " +
                "making one single call to {1}(typeof({0}), types).",
                CSharpFriendlyName(serviceType.GetGenericTypeDefinition()),
                CollectionsRegisterMethodName);

        internal static string MixingRegistrationsWithControlledAndUncontrolledIsNotSupported(
            Type serviceType, bool controlled) =>
            Format(
                "You already made a registration for {0} using one of the {3} overloads that registers " +
                "container-{1} collections, while this method registers container-{2} collections. " +
                "Mixing calls is not supported. Consider merging those calls or make both calls either as " +
                "controlled or uncontrolled registration.",
                (serviceType.IsGenericType() ? serviceType.GetGenericTypeDefinition() : serviceType).TypeName(),
                controlled ? "uncontrolled" : "controlled",
                controlled ? "controlled" : "uncontrolled",
                CollectionsRegisterMethodName);

        internal static string ValueInvalidForEnumType(
            string paramName, object invalidValue, Type enumClass) =>
            Format(
                "The value of argument '{0}' ({1}) is invalid for Enum type '{2}'.",
                paramName,
                invalidValue,
                enumClass.Name);

        internal static string ServiceTypeCannotBeAPartiallyClosedType(Type openGenericServiceType) =>
            Format(
                "The supplied type '{0}' is a partially closed generic type, which is not supported by " +
                "this method. Please supply the open-generic type '{1}' instead.",
                openGenericServiceType.TypeName(),
                CSharpFriendlyName(openGenericServiceType.GetGenericTypeDefinition()));

        internal static string ServiceTypeCannotBeAPartiallyClosedType(
            Type openGenericServiceType, string serviceTypeParamName, string implementationTypeParamName) =>
            Format(
                "The supplied type '{0}' is a partially closed generic type, which is not supported as " +
                "value of the {1} parameter. Instead, please supply the open-generic type '{2}' and make " +
                "the type supplied to the {3} parameter partially closed instead.",
                openGenericServiceType.TypeName(),
                serviceTypeParamName,
                CSharpFriendlyName(openGenericServiceType.GetGenericTypeDefinition()),
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
            Format(
                "There is already an open-generic registration for {0} (with implementation {1}) that " +
                "overlaps with the registration of {2} that you are trying to make. If your intention is " +
                "to use {1} as fallback registration, please instead call: " +
                "{5}(typeof({3}), typeof({4}), c => !c.Handled).",
                closedServiceType.GetGenericTypeDefinition().TypeName(),
                overlappingGenericImplementationType.TypeName(),
                closedServiceType.TypeName(),
                CSharpFriendlyName(closedServiceType.GetGenericTypeDefinition()),
                CSharpFriendlyName(overlappingGenericImplementationType),
                nameof(Container.RegisterConditional));

        internal static string AnOverlappingRegistrationExists(
            Type openGenericServiceType,
            Type overlappingImplementationType,
            bool isExistingRegistrationConditional,
            Type implementationTypeOfNewRegistration,
            bool isNewRegistrationConditional)
        {
            string solution = "Either remove one of the registrations or make them both conditional.";

            if (isExistingRegistrationConditional && isNewRegistrationConditional
                && overlappingImplementationType == implementationTypeOfNewRegistration)
            {
                solution =
                    "You can merge both registrations into a single conditional registration and combine " +
                    "both predicates into one single predicate.";
            }

            return Format(
                "There is already a {0}registration for {1} (with implementation {2}) that " +
                "overlaps with the {3}registration for {4} that you are trying to make. This new " +
                "registration causes ambiguity, because both registrations would be used for the " +
                "same closed service types. {5}",
                isExistingRegistrationConditional ? "conditional " : string.Empty,
                openGenericServiceType.TypeName(),
                overlappingImplementationType.TypeName(),
                isNewRegistrationConditional ? "conditional " : string.Empty,
                implementationTypeOfNewRegistration.TypeName(),
                solution);
        }

        internal static string MultipleApplicableRegistrationsFound(
            Type serviceType, Tuple<Type, Type, InstanceProducer>[] overlappingRegistrations) =>
            Format(
                "Multiple applicable registrations found for {0}. The applicable registrations are {1}. " +
                "If your goal is to make one registration a fallback in case another registration is not " +
                "applicable, make the fallback registration last using RegisterConditional and make sure " +
                "the supplied predicate returns false in case the Handled property is true.",
                serviceType.TypeName(),
                overlappingRegistrations.Select(BuildRegistrationName).ToCommaSeparatedText());

        internal static string UnableToLoadTypesFromAssembly(Assembly assembly, Exception innerException) =>
            Format(
                "Unable to load types from assembly {0}. {1}",
                assembly.FullName,
                innerException.Message);

        private static bool IsListOrArrayRelationship(KnownRelationship relationship) =>
            typeof(List<>).IsGenericTypeDefinitionOf(relationship.Consumer.Target.TargetType)
            || relationship.Consumer.Target.TargetType.IsArray;

        private static string CollectionUsedDuringConstructionByInjectingMutableCollection(
            Type consumer, InstanceProducer producer, KnownRelationship relationship) =>
            Format(
                "{0} is part of the {3} that is injected into {2}. The problem in {2} is that instead " +
                "of depending on one of the collection types that stream services (e.g. IEnumerable<{1}>, " +
                "ICollection<{1}>, etc), it depends on the mutable collection type {4}. This causes " +
                "{0} to be resolved during object construction, which is not advised.",
                producer.FinalImplementationType.ToFriendlyName(),
                producer.ServiceType.ToFriendlyName(),
                consumer.ToFriendlyName(),
                relationship.Dependency.ServiceType.ToFriendlyName(),
                relationship.Consumer.Target.TargetType.ToFriendlyName());

        private static string CollectionUsedDuringConstructionByIteratingAStream(
            Type consumer, InstanceProducer producer, KnownRelationship? relationship = null) =>
            Format(
                "{0} is part of the {3} that is injected into {2}. The problem in {2} is that instead " +
                "of storing the injected {3} in a private field and iterating over it at the point " +
                "its instances are required, {0} is being resolved (from the collection) during " +
                "object construction. Resolving services from an injected collection during object " +
                "construction (e.g. by calling {4}.ToList() in the constructor) is not advised.",
                producer.FinalImplementationType.ToFriendlyName(),
                producer.ServiceType.ToFriendlyName(),
                consumer.ToFriendlyName(),
                relationship != null
                    ? relationship.Dependency.ServiceType.ToFriendlyName()
                    : Format(
                        "collection of {0} services",
                        producer.ServiceType.ToFriendlyName()),
                relationship?.Consumer.IsRoot == false
                    ? relationship.Consumer.Target.Name
                    : "collection");

        private static string BuildRegistrationName(
            Tuple<Type, Type, InstanceProducer> registration, int index)
        {
            Type serviceType = registration.Item1;
            Type implementationType = registration.Item2;
            InstanceProducer producer = registration.Item3;

            return Format(
                "({0}) the {1} {2}registration for {3} using {4}",
                index + 1,
                producer.IsConditional ? "conditional" : "unconditional",
                serviceType.IsGenericTypeDefinition()
                    ? "open-generic "
                    : serviceType.IsGenericType() ? "closed-generic " : string.Empty,
                serviceType.TypeName(),
                implementationType.TypeName());
        }

        private static string NonGenericTypeAlreadyRegistered(
            Type serviceType, bool existingRegistrationIsConditional)
        {
            return Format(
                "Type {0} has already been registered as {1} registration. For non-generic types, " +
                "conditional and unconditional registrations can't be mixed.",
                serviceType.TypeName(),
                existingRegistrationIsConditional ? "conditional" : "unconditional");
        }

        private static string GetAdditionalInformationAboutExistingConditionalRegistrations(
            InjectionTargetInfo target, int numberOfConditionalRegistrations)
        {
            string serviceTypeName = target.TargetType.TypeName();

            bool isGenericType = target.TargetType.IsGenericType();

            string openServiceTypeName = isGenericType
                ? target.TargetType.GetGenericTypeDefinition().TypeName()
                : target.TargetType.TypeName();

            if (numberOfConditionalRegistrations > 1)
            {
                return Format(
                    " {0} conditional registrations for {1} exist{2}, but none of the supplied predicates " +
                    "returned true when provided with the contextual information for {3}.",
                    numberOfConditionalRegistrations,
                    openServiceTypeName,
                    isGenericType ? (" that are applicable to " + serviceTypeName) : string.Empty,
                    target.Member.DeclaringType.TypeName());
            }
            else if (numberOfConditionalRegistrations == 1)
            {
                return Format(
                    " 1 conditional registration for {0} exists{1}, but its supplied predicate didn't " +
                    "return true when provided with the contextual information for {2}.",
                    openServiceTypeName,
                    isGenericType ? (" that is applicable to " + serviceTypeName) : string.Empty,
                    target.Member.DeclaringType.TypeName());
            }
            else
            {
                return string.Empty;
            }
        }

        private static string SuppliedTypeIsNotOpenGenericExplainingAlternatives(
            Type type, string registeringElement) =>
            Format(
                "Supply this method with the open-generic type {0} to register all available " +
                "implementations of this type, or call {2}(Type, IEnumerable<{1}>) either with the open " +
                "or closed version of that type to register a collection of instances based on that type.",
                CSharpFriendlyName(type.GetGenericTypeDefinition()),
                registeringElement,
                CollectionsRegisterMethodName);

        private static string ToTypeOCfSharpFriendlyList(IEnumerable<Type> types) =>
            string.Join(", ",
                from type in types
                select Format("typeof({0})", CSharpFriendlyName(type)));

        private static string SuppliedTypeIsNotGenericExplainingAlternatives(
            Type type, string registeringElement) =>
            Format(
                "This method only supports open-generic types. " +
                "If you meant to register all available implementations of {0}, call " +
                "{2}(typeof({0}), IEnumerable<{1}>) instead.",
                type.TypeName(),
                registeringElement,
                CollectionsRegisterMethodName);

        private static object ContainerHasNoRegistrationsAddition(bool containerHasRegistrations) =>
            containerHasRegistrations
                ? string.Empty
                : " Please note that the container instance you are resolving from contains no " +
                  "registrations. Could it be that you accidentally created a new -and empty- container?";

        private static object DidYouMeanToCallGetInstanceInstead(
            bool hasRelatedOneToOneMapping, Type collectionServiceType) =>
            hasRelatedOneToOneMapping
                ? Format(
                    " There is, however, a registration for {0}; Did you mean to call GetInstance<{0}>() " +
                    "or depend on {0}? Or did you mean to register a collection of types using " +
                    "{1}? Please see https://simpleinjector.org/collections for more information " +
                    "about registering and resolving collections.",
                    collectionServiceType.GetGenericArguments()[0].TypeName(),
                    CollectionsRegisterMethodName)
                : string.Empty;

        private static object NoCollectionRegistrationExists(
            bool shouldDisplayMessage, Type collectionServiceType) =>
            shouldDisplayMessage
                ? Format(
                    " You can use one of the {0} overloads to register a collection of {1} types, or one " +
                    "of the {2} overloads to append a single registration to a collection. In case you " +
                    "intend to resolve an empty collection of {1} elements, make sure you register an " +
                    "empty collection; Simple Injector requires a call to {0} to be made, even in the " +
                    "absence of any instances. Please see https://simpleinjector.org/collections for more " +
                    "information about registering and resolving collections.",
                    CollectionsRegisterMethodName,
                    collectionServiceType.GetGenericArguments()[0].TypeName(),
                    CollectionsAppendMethodName)
                : string.Empty;

        private static string DidYouMeanToDependOnNonCollectionInstead(
            bool hasRelatedOneToOneMapping, Type collectionServiceType) =>
            hasRelatedOneToOneMapping
                ? Format(
                    " There is, however, a registration for {0}; Did you mean to depend on {0}?",
                    collectionServiceType.GetGenericArguments()[0].TypeName())
                : string.Empty;

        private static string DidYouMeanToCallGetAllInstancesInstead(bool hasCollection, Type serviceType) =>
            hasCollection
                ? Format(
                    " There is, however, a registration for {0}; Did you mean to call " +
                    "GetAllInstances<{1}>() or depend on {0}? " +
                    "Please see https://simpleinjector.org/collections for more information " +
                    "about registering and resolving collections.",
                    typeof(IEnumerable<>).MakeGenericType(serviceType).TypeName(),
                    serviceType.TypeName())
                : string.Empty;

        private static string DidYouMeanToDependOnCollectionInstead(bool hasCollection, Type serviceType) =>
            hasCollection
                ? Format(
                    " There is, however, a registration for a collection of {0} instances; Did you mean to " +
                    "depend on {1} instead? If you meant to depend on {0}, you should use one of the {3} " +
                    "overloads instead of using {2}. " +
                    "Please see https://simpleinjector.org/collections for more information about " +
                    "registering and resolving collections.",
                    serviceType.TypeName(),
                    typeof(IEnumerable<>).MakeGenericType(serviceType).TypeName(),
                    CollectionsRegisterMethodName,
                    nameof(Container) + "." + nameof(Container.Register))
                : string.Empty;

        private static string NoteThatSkippedDecoratorsWereFound(Type serviceType, Type[] decorators) =>
            decorators.Any()
                ? Format(
                    " Note that {0} {1} found as implementation of {2}, but {1} skipped during auto-" +
                    "registration by the container because {3} considered to be a decorator (because {4} " +
                    "{2}).",
                    decorators.Select(TypeName).ToCommaSeparatedText(),
                    decorators.Length == 1 ? "was" : "were",
                    serviceType.GetGenericTypeDefinition().TypeName(),
                    decorators.Length == 1 ? "it is" : "they are",
                    decorators.Length == 1 ? "it references" : "they reference",
                    decorators.Length == 1 ? "itself" : "themselves")
                : string.Empty;

        private static string NoteThatTypeLookalikesAreFound(
            Type serviceType, Type[] lookalikes, int numberOfConditionals = 0)
        {
            if (!lookalikes.Any() || numberOfConditionals > 0)
            {
                return string.Empty;
            }

            Type duplicateAssemblyLookalike =
                GetDuplicateLoadedAssemblyLookalikeTypeOrNull(serviceType, lookalikes);

            if (duplicateAssemblyLookalike != null)
            {
                return Format(
                    " Type {0} is a member of the assembly {1} which seems to have been loaded more than " +
                    "once. The CLR believes the second instance of the assembly is a different assembly " +
                    "to the first. It is this multiple loading of assemblies that is causing this issue. " +
                    "The most likely cause is that the same assembly has been loaded from different " +
                    "locations within different contexts. " +
                    "{2}" +
                    "Please see https://simpleinjector.org/asmld for more information about this " +
                    "problem and how to solve it.",
                    Types.ToCSharpFriendlyName(duplicateAssemblyLookalike, fullyQualifiedName: true),
                    serviceType.GetAssembly().FullName,
                    BuildAssemblyLocationMessage(serviceType, duplicateAssemblyLookalike));
            }
            else
            {
                return Format(
                    " Note that there exists a registration for a different type {0} while " +
                    "the requested type is {1}.",
                    lookalikes.First().ToFriendlyName(fullyQualifiedName: true),
                    serviceType.ToFriendlyName(fullyQualifiedName: true));
            }
        }

        private static string NoteThatConcreteTypeCanNotBeResolvedDueToConfiguration(
            Container container, InjectionTargetInfo target) =>
            NoteThatConcreteTypeCanNotBeResolvedDueToConfiguration(
                container, target.TargetType, target.Member.DeclaringType);

        private static string NoteThatConcreteTypeCanNotBeResolvedDueToConfiguration(
            Container container, Type resolvedType, Type? consumingType = null) =>
            container.IsConcreteConstructableType(resolvedType)
                && !container.Options.ResolveUnregisteredConcreteTypes
                ? ThereIsAMappingToImplementationType(container, resolvedType, consumingType) +
                    " An implicit registration could not be made because " +
                    "Container.Options.ResolveUnregisteredConcreteTypes is set to 'false', which is now " +
                    "the default setting in v5. This disallows the container to construct this " +
                    "unregistered concrete type. For more information on why resolving unregistered " +
                    "concrete types is now disallowed by default, and what possible fixes you can apply, " +
                    "see https://simpleinjector.org/ructd."
                : string.Empty;

        private static string ThereIsAMappingToImplementationType(
            Container container, Type resolvedType, Type? consumingType)
        {
            var serviceTypes = GetServiceTypesForMappedImplementation(container, resolvedType).ToArray();

            return ThereIsAMappingToImplementationType(resolvedType, serviceTypes)
                + DidYouIntendForXToDependOnYInstead(consumingType, serviceTypes);
        }

        private static string ThereIsAMappingToImplementationType(Type resolvedType, Type[] serviceTypes)
        {
            switch (serviceTypes.Length)
            {
                case 0:
                    return string.Empty;

                case 1:
                    return Format(
                        " There is a registration for {0} though, which maps to {1}.",
                        serviceTypes[0].TypeName(),
                        resolvedType.TypeName());

                default:
                    return Format(
                       " There are registrations for {0} though, which {1} map to {2}.",
                       serviceTypes.Select(TypeName).ToCommaSeparatedText(),
                       serviceTypes.Length > 2 ? "all" : "both",
                       resolvedType.TypeName());
            }
        }


        private static string DidYouIntendForXToDependOnYInstead(Type? consumingType, Type[] serviceTypes)
        {
            if (serviceTypes.Length == 0)
            {
                return string.Empty;
            }
            else
            {
                var name = serviceTypes[0].ToFriendlyName(fullyQualifiedName: false);

                return consumingType is null
                    ? serviceTypes.Length == 1
                        ? Format(" Did you intend to request {0} instead?", name)
                        : " Did you intend to request one of those types instead?"
                    : serviceTypes.Length == 1
                        ? Format(
                            " Did you intend for {0} to depend on {1} instead?",
                            consumingType.TypeName(),
                            name)
                        : Format(
                            " Did you intend for {0} to depend on one of those registrations instead?",
                            consumingType.TypeName());
            }
        }

        private static IEnumerable<Type> GetServiceTypesForMappedImplementation(Container container, Type type) =>
            from producer in container.GetCurrentRegistrations()
            where producer.FinalImplementationType == type && producer.ServiceType != type
            select producer.ServiceType;

        private static string BuildAssemblyLocationMessage(Type serviceType, Type duplicateAssemblyLookalike)
        {
            string? serviceTypeLocation = GetAssemblyLocationOrNull(serviceType);
            string? lookalikeLocation = GetAssemblyLocationOrNull(duplicateAssemblyLookalike);

            if (serviceTypeLocation != lookalikeLocation
                && (lookalikeLocation != null || serviceTypeLocation != null))
            {
                return Format(
                    "The assembly of the requested type is located at {0}, while the " +
                    "assembly of the registered type is located at {1}. ",
                    serviceTypeLocation,
                    lookalikeLocation);
            }

            return string.Empty;
        }

        private static string? GetAssemblyLocationOrNull(Type type) =>
            AssemblyLocationProperty != null && !type.GetAssembly().IsDynamic
                ? (string)AssemblyLocationProperty.GetValue(type.GetAssembly(), null)
                : null;

        private static Type GetDuplicateLoadedAssemblyLookalikeTypeOrNull(
            Type serviceType, Type[] lookalikes) => (
            from lookalike in lookalikes
            where !object.ReferenceEquals(serviceType.GetAssembly(), lookalike.GetAssembly())
            where serviceType.GetAssembly().FullName == lookalike.GetAssembly().FullName
            let lookalikeName = lookalike.ToFriendlyName(fullyQualifiedName: true)
            let serviceTypeFullName = serviceType.ToFriendlyName(fullyQualifiedName: true)
            where lookalikeName == serviceTypeFullName || (
                serviceType.IsGenericType()
                && serviceType.GetGenericTypeDefinition().ToFriendlyName(fullyQualifiedName: true) == lookalikeName)
            select lookalike)
            .FirstOrDefault();

        private static string TypeName(this Type type) => type.ToFriendlyName(UseFullyQualifiedTypeNames);

        private static string CSharpFriendlyName(Type type) =>
            Types.ToCSharpFriendlyName(type, UseFullyQualifiedTypeNames);

        private static string Format(string format, params object?[] args) =>
            string.Format(CultureInfo.InvariantCulture, format, args);
    }
}