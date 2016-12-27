#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2014 Simple Injector Contributors
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
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Decorators;
    using SimpleInjector.Internals;

    internal static class Requires
    {
#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        internal static void IsNotNull(object instance, string paramName)
        {
            if (instance == null)
            {
                ThrowArgumentNullException(paramName);
            }
        }

        [DebuggerStepThrough]
        internal static void IsNotNullOrEmpty(string instance, string paramName)
        {
            IsNotNull(instance, paramName);

            if (instance.Length == 0)
            {
                throw new ArgumentException("Value can not be empty.", paramName);
            }
        }

        [DebuggerStepThrough]
        internal static void IsReferenceType(Type type, string paramName)
        {
            if (!type.IsClass() && !type.IsInterface())
            {
                throw new ArgumentException(StringResources.SuppliedTypeIsNotAReferenceType(type), paramName);
            }
        }

        [DebuggerStepThrough]
        internal static void IsNotOpenGenericType(Type type, string paramName)
        {
            // We check for ContainsGenericParameters to see whether there is a Generic Parameter 
            // to find out if this type can be created.
            if (type.ContainsGenericParameters())
            {
                throw new ArgumentException(StringResources.SuppliedTypeIsAnOpenGenericType(type), paramName);
            }
        }

        [DebuggerStepThrough]
        internal static void ServiceIsAssignableFromExpression(Type service, Expression expression,
            string paramName)
        {
            if (!service.IsAssignableFrom(expression.Type))
            {
                ThrowSuppliedElementDoesNotInheritFromOrImplement(service, expression.Type, "expression", 
                    paramName);
            }
        }

        [DebuggerStepThrough]
        internal static void ServiceIsAssignableFromRegistration(Type service, Registration registration,
            string paramName)
        {
            if (!service.IsAssignableFrom(registration.ImplementationType))
            {
                ThrowSuppliedElementDoesNotInheritFromOrImplement(service, registration.ImplementationType, 
                    "registration", paramName);
            }
        }

        [DebuggerStepThrough]
        internal static void ServiceIsAssignableFromImplementation(Type service, Type implementation,
            string paramName)
        {
            if (!service.IsAssignableFrom(implementation))
            {
                ThrowSuppliedTypeDoesNotInheritFromOrImplement(service, implementation, paramName);
            }
        }

        [DebuggerStepThrough]
        internal static void IsNotAnAmbiguousType(Type type, string paramName)
        {
            if (Types.IsAmbiguousType(type))
            {
                throw new ArgumentException(StringResources.TypeIsAmbiguous(type), paramName);
            }
        }

        [DebuggerStepThrough]
        internal static void IsGenericType(Type type, string paramName, Func<Type, string> guidance = null)
        {
            if (!type.IsGenericType())
            {
                string message = StringResources.SuppliedTypeIsNotAGenericType(type) +
                    (guidance == null ? string.Empty : (" " + guidance(type)));

                throw new ArgumentException(message, paramName);
            }
        }

        [DebuggerStepThrough]
        internal static void IsOpenGenericType(Type type, string paramName, Func<Type, string> guidance = null)
        {
            // We don't check for ContainsGenericParameters, because we can't handle types that don't have
            // a direct parameter (such as Lazy<Func<TResult>>). This is a limitation in the current
            // implementation of the GenericArgumentFinder. That's not an easy thing to fix :-(
            if (!type.ContainsGenericParameters())
            {
                string message = StringResources.SuppliedTypeIsNotAnOpenGenericType(type) +
                    (guidance == null ? string.Empty : (" " + guidance(type)));

                throw new ArgumentException(message, paramName);
            }
        }

        internal static void DoesNotContainNullValues<T>(IEnumerable<T> collection, string paramName)
            where T : class
        {
            if (collection != null && collection.Contains(null))
            {
                throw new ArgumentException("The collection contains null elements.", paramName);
            }
        }

        internal static void DoesNotContainOpenGenericTypesWhenServiceTypeIsNotGeneric(Type serviceType,
            IEnumerable<Type> serviceTypes, string paramName)
        {
            Type openGenericType = serviceTypes.FirstOrDefault(t => t.ContainsGenericParameters());

            if (!serviceType.IsGenericType() && openGenericType != null)
            {
                throw new ArgumentException(
                    StringResources.SuppliedTypeIsAnOpenGenericTypeWhileTheServiceTypeIsNot(openGenericType),
                    paramName);
            }
        }

        internal static void ServiceTypeIsNotClosedWhenImplementationIsOpen(Type service, Type implementation)
        {
            bool implementationIsOpen = 
                implementation.IsGenericType() && implementation.ContainsGenericParameters();

            bool serviceTypeIsClosed = 
                service.IsGenericType() && !service.ContainsGenericParameters();

            if (implementationIsOpen && serviceTypeIsClosed)
            {
                throw new NotSupportedException(StringResources.SuppliedTypeCanNotBeOpenWhenDecoratorIsClosed());
            }
        }

        internal static void ServiceOrItsGenericTypeDefinitionIsAssignableFromImplementation(Type service,
            Type implementation, string paramName)
        {
            if (service != implementation &&
                !Types.ServiceIsAssignableFromImplementation(service, implementation))
            {
                throw new ArgumentException(
                    StringResources.SuppliedTypeDoesNotInheritFromOrImplement(service, implementation),
                    paramName);
            }
        }

        internal static void ServiceIsAssignableFromImplementations(Type serviceType,
            IEnumerable<Type> typesToRegister, string paramName, bool typeCanBeServiceType = false)
        {
            var invalidType = (
                from type in typesToRegister
                where !Types.ServiceIsAssignableFromImplementation(serviceType, type)
                where !typeCanBeServiceType || type != serviceType
                select type)
                .FirstOrDefault();

            if (invalidType != null)
            {
                throw new ArgumentException(
                    StringResources.SuppliedTypeDoesNotInheritFromOrImplement(serviceType, invalidType),
                    paramName);
            }
        }

        internal static void ServiceIsAssignableFromImplementations(Type serviceType,
            IEnumerable<Registration> registrations, string paramName, bool typeCanBeServiceType = false)
        {
            var typesToRegister = registrations.Select(registration => registration.ImplementationType);

            ServiceIsAssignableFromImplementations(serviceType, typesToRegister, paramName, typeCanBeServiceType);
        }

        internal static void ImplementationHasSelectableConstructor(Container container,
            Type implementationType, string paramName)
        {
            string message;

            if (!container.Options.IsConstructableType(implementationType, out message))
            {
                throw new ArgumentException(message, paramName);
            }
        }

        internal static void TypeFactoryReturnedTypeThatDoesNotContainUnresolvableTypeArguments(
            Type serviceType, Type implementationType)
        {
            try
            {
                OpenGenericTypeDoesNotContainUnresolvableTypeArguments(
                    serviceType.IsGenericType() ? serviceType.GetGenericTypeDefinition() : serviceType,
                    implementationType,
                    null);
            }
            catch (ArgumentException ex)
            {
                throw new ActivationException(ex.Message);
            }
        }

        internal static void OpenGenericTypesDoNotContainUnresolvableTypeArguments(Type serviceType,
            IEnumerable<Registration> registrations, string parameterName)
        {
            OpenGenericTypesDoNotContainUnresolvableTypeArguments(serviceType,
                registrations.Select(registration => registration.ImplementationType), parameterName);
        }

        internal static void OpenGenericTypesDoNotContainUnresolvableTypeArguments(Type serviceType,
            IEnumerable<Type> implementationTypes, string parameterName)
        {
            foreach (Type implementationType in implementationTypes)
            {
                OpenGenericTypeDoesNotContainUnresolvableTypeArguments(serviceType, implementationType,
                    parameterName);
            }
        }

        internal static void OpenGenericTypeDoesNotContainUnresolvableTypeArguments(Type serviceType,
            Type implementationType, string parameterName)
        {
            if (serviceType.ContainsGenericParameters() && implementationType.ContainsGenericParameters())
            {
                var builder = new GenericTypeBuilder(serviceType, implementationType);

                if (!builder.OpenGenericImplementationCanBeAppliedToServiceType())
                {
                    string error =
                        StringResources.OpenGenericTypeContainsUnresolvableTypeArguments(implementationType);

                    throw new ArgumentException(error, parameterName);
                }
            }
        }

        internal static void DecoratorIsNotAnOpenGenericTypeDefinitionWhenTheServiceTypeIsNot(Type serviceType,
            Type decoratorType, string parameterName)
        {
            if (!serviceType.ContainsGenericParameters() && decoratorType.ContainsGenericParameters())
            {
                throw new ArgumentException(
                    StringResources.DecoratorCanNotBeAGenericTypeDefinitionWhenServiceTypeIsNot(
                        serviceType, decoratorType), parameterName);
            }
        }

        internal static void HasFactoryCreatedDecorator(Container container, Type serviceType, Type decoratorType)
        {
            try
            {
                IsDecorator(container, serviceType, decoratorType, null);
            }
            catch (ArgumentException ex)
            {
                throw new ActivationException(ex.Message);
            }
        }

        internal static void FactoryReturnsATypeThatIsAssignableFromServiceType(Type serviceType,
            Type implementationType)
        {
            if (!serviceType.IsAssignableFrom(implementationType))
            {
                throw new ActivationException(StringResources.TypeFactoryReturnedIncompatibleType(
                    serviceType, implementationType));
            }
        }

        internal static void IsDecorator(Container container, Type serviceType, Type decoratorType,
            string paramName)
        {
            ConstructorInfo decoratorConstructor = container.Options.SelectConstructor(decoratorType);

            Requires.DecoratesServiceType(serviceType, decoratorConstructor, paramName);
        }

        internal static void AreRegistrationsForThisContainer(Container container,
            IEnumerable<Registration> registrations, string paramName)
        {
            foreach (Registration registration in registrations)
            {
                IsRegistrationForThisContainer(container, registration, paramName);
            }
        }

        internal static void IsRegistrationForThisContainer(Container container, Registration registration,
            string paramName)
        {
            if (!object.ReferenceEquals(container, registration.Container))
            {
                string message = StringResources.TheSuppliedRegistrationBelongsToADifferentContainer();

                throw new ArgumentException(message, paramName);
            }
        }

        internal static void CollectionDoesNotContainOpenGenericTypes(IEnumerable<Type> typesToRegister,
            string paramName)
        {
            var openGenericTypes =
                from type in typesToRegister
                where type.ContainsGenericParameters()
                select type;

            if (openGenericTypes.Any())
            {
                string message = StringResources.ThisOverloadDoesNotAllowOpenGenerics(openGenericTypes);

                throw new ArgumentException(message, paramName);
            }
        }

        [DebuggerStepThrough]
        internal static void IsValidEnum<TEnum>(TEnum value, string paramName) where TEnum : struct
        {
            if (!Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Contains(value))
            {
                throw new ArgumentException(
                    StringResources.ValueInvalidForEnumType(paramName, value, typeof(TEnum)));
            }
        }

        internal static void IsNotPartiallyClosed(Type openGenericServiceType, string paramName)
        {
            if (openGenericServiceType.IsPartiallyClosed())
            {
                throw new ArgumentException(
                    StringResources.ServiceTypeCannotBeAPartiallyClosedType(openGenericServiceType),
                    paramName);
            }
        }

        internal static void IsNotPartiallyClosed(Type openGenericServiceType, string paramName,
            string implementationTypeParamName)
        {
            if (openGenericServiceType.IsPartiallyClosed())
            {
                throw new ArgumentException(
                    StringResources.ServiceTypeCannotBeAPartiallyClosedType(openGenericServiceType, paramName,
                        implementationTypeParamName),
                    paramName);
            }
        }

        private static void DecoratesServiceType(Type serviceType, ConstructorInfo decoratorConstructor,
            string paramName)
        {
            bool decoratesServiceType = DecoratorHelpers.DecoratesServiceType(serviceType, decoratorConstructor);

            if (!decoratesServiceType)
            {
                ThrowMustDecorateServiceType(serviceType, decoratorConstructor, paramName);
            }
        }

        private static void ThrowMustDecorateServiceType(Type serviceType,
            ConstructorInfo constructor, string paramName)
        {
            int numberOfServiceTypeDependencies =
                DecoratorHelpers.GetNumberOfServiceTypeDependencies(serviceType, constructor);

            if (numberOfServiceTypeDependencies == 0)
            {
                // We must get the real type to be decorated to prevent the exception message from being
                // confusing to the user.
                // At this point we know that the decorator type implements an service type in some way
                // (either open or closed), so we this call will return at least one record.
                serviceType = Types.GetBaseTypeCandidates(serviceType, constructor.DeclaringType).First();

                ThrowMustContainTheServiceTypeAsArgument(serviceType, constructor, paramName);
            }
            else
            {
                ThrowMustContainASingleInstanceOfTheServiceTypeAsArgument(serviceType, constructor, paramName);
            }
        }

        private static void ThrowMustContainTheServiceTypeAsArgument(Type serviceType,
            ConstructorInfo decoratorConstructor, string paramName)
        {
            string message = StringResources.TheConstructorOfTypeMustContainTheServiceTypeAsArgument(
                decoratorConstructor.DeclaringType, serviceType);

            throw new ArgumentException(message, paramName);
        }

        private static void ThrowMustContainASingleInstanceOfTheServiceTypeAsArgument(Type serviceType,
            ConstructorInfo decoratorConstructor, string paramName)
        {
            string message =
                StringResources.TheConstructorOfTypeMustContainASingleInstanceOfTheServiceTypeAsArgument(
                    decoratorConstructor.DeclaringType, serviceType);

            throw new ArgumentException(message, paramName);
        }

        private static void ThrowArgumentNullException(string paramName)
        {
            throw new ArgumentNullException(paramName);
        }

        private static void ThrowSuppliedElementDoesNotInheritFromOrImplement(Type service, 
            Type implementation, string elementDescription, string paramName)
        {
            throw new ArgumentException(
                StringResources.SuppliedElementDoesNotInheritFromOrImplement(service, implementation, 
                    elementDescription),
                paramName);
        }

        private static void ThrowSuppliedTypeDoesNotInheritFromOrImplement(Type service, Type implementation,
            string paramName)
        {
            throw new ArgumentException(
                StringResources.SuppliedTypeDoesNotInheritFromOrImplement(service, implementation),
                paramName);
        }
    }
}