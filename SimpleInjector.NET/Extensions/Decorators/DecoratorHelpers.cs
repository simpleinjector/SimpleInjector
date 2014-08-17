#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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

namespace SimpleInjector.Extensions.Decorators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Advanced;

    internal static partial class DecoratorHelpers
    {
        private static readonly MethodInfo EnumerableSelectMethod =
            ExtensionHelpers.GetGenericMethod(() => Enumerable.Select<int, int>(null, (Func<int, int>)null));

        internal static Registration CreateRegistrationForContainerControlledCollection(Type serviceType,
            IContainerControlledCollection instance, Container container)
        {
            Type enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);

            return new ContainerControlledCollectionRegistration(enumerableServiceType, instance, container)
            {
                IsCollection = true
            };
        }

        internal static IContainerControlledCollection ExtractContainerControlledCollectionFromRegistration(
            Registration registration)
        {
            var controlledRegistration = registration as ContainerControlledCollectionRegistration;

            // We can only determine the value when registration is created using the 
            // CreateRegistrationForContainerControlledCollection method. When the registration is null the
            // collection might be registered as container-uncontrolled collection.
            if (controlledRegistration == null)
            {
                return null;
            }

            return controlledRegistration.Collection;
        }

        internal static IContainerControlledCollection CreateContainerControlledCollection(Type serviceType,
            Container container, Type[] serviceTypes)
        {
            return CreateContainerControlledCollectionInternal(serviceType, container, serviceTypes);
        }

        internal static IContainerControlledCollection CreateContainerControlledCollection(Type serviceType,
            Container container, IEnumerable<Registration> registrations)
        {
            return CreateContainerControlledCollectionInternal(serviceType, container, registrations);
        }

        internal static IContainerControlledCollection CreateContainerControlledCollection(Type serviceType,
            Container container, object arrayOfSingletons)
        {
            return CreateContainerControlledCollectionInternal(serviceType, arrayOfSingletons, container);
        }

        internal static bool IsContainerControlledCollectionExpression(Expression enumerableExpression)
        {
            var constantExpression = enumerableExpression as ConstantExpression;

            object enumerable = constantExpression != null ? constantExpression.Value : null;

            return enumerable is IContainerControlledCollection;
        }

        internal static IEnumerable Select(this IEnumerable source, Type type, Delegate selector)
        {
            var selectMethod = EnumerableSelectMethod.MakeGenericMethod(type, type);

            return (IEnumerable)selectMethod.Invoke(null, new object[] { source, selector });
        }

        internal static MethodCallExpression Select(Expression collectionExpression, Type type,
            Delegate selector)
        {
            // We make use of .NET's built in Enumerable.Select to wrap the collection with the decorators.
            var selectMethod = EnumerableSelectMethod.MakeGenericMethod(type, type);

            return Expression.Call(selectMethod, collectionExpression, Expression.Constant(selector));
        }

        internal static bool IsDecorator(Container container, Type serviceType, Type typeToCheck)
        {
            ConstructorInfo constructorToCheck;

            try
            {
                constructorToCheck = container.Options.SelectConstructor(serviceType, typeToCheck);
            }
            catch (ActivationException)
            {
                // If the constructor resolution behavior fails, we can't determine whether this type is a
                // decorator. Since this method is used by batch registration, by returning false the type
                // will be included in batch registration and at that point GetConstructor is called again
                // -and will fail again- giving the user the required information.
                return false;
            }

            return DecoratesServiceType(serviceType, constructorToCheck) &&
                DecoratesBaseTypes(serviceType, constructorToCheck);
        }
        
        internal static bool DecoratesServiceType(Type serviceType, ConstructorInfo decoratorConstructor)
        {
            int numberOfServiceTypeDependencies =
                GetNumberOfServiceTypeDependencies(serviceType, decoratorConstructor);

            return numberOfServiceTypeDependencies == 1;
        }

        internal static int GetNumberOfServiceTypeDependencies(Type serviceType,
            ConstructorInfo decoratorConstructor)
        {
            var validServiceTypeArguments =
                from parameter in decoratorConstructor.GetParameters()
                where
                    IsDecorateeDependencyParameter(parameter.ParameterType, serviceType) ||
                    IsDecorateeFactoryDependencyParameter(parameter.ParameterType, serviceType)
                select parameter;

            return validServiceTypeArguments.Count();
        }

        internal static bool DecoratesBaseTypes(Type serviceType, ConstructorInfo decoratorConstructor)
        {
            var baseTypes = GetValidDecoratorConstructorArgumentTypes(serviceType,
                decoratorConstructor.DeclaringType);

            var constructorParameters = decoratorConstructor.GetParameters();

            // For a type to be a decorator, one of its constructor parameter types must exactly match with
            // one of the interfaces it implements or base types it inherits from.
            var decoratorParameters =
                from baseType in baseTypes
                from parameter in constructorParameters
                where parameter.ParameterType == baseType ||
                    parameter.ParameterType == typeof(Func<>).MakeGenericType(baseType)
                select parameter;

            return decoratorParameters.Any();
        }

        internal static Type[] GetValidDecoratorConstructorArgumentTypes(Type serviceType, Type decoratorType)
        {
            return (
                from baseType in decoratorType.GetBaseTypesAndInterfaces()
                where IsDecorateeDependencyParameter(baseType, serviceType)
                select baseType)
                .ToArray();
        }

        // Checks if the given parameterType can function as the decorated instance of the given service type.
        private static bool IsDecorateeFactoryDependencyParameter(Type parameterType, Type serviceType)
        {
            if (!parameterType.IsGenericType || parameterType.GetGenericTypeDefinition() != typeof(Func<>))
            {
                return false;
            }

            Type funcArgumentType = parameterType.GetGenericArguments()[0];

            return IsDecorateeDependencyParameter(funcArgumentType, serviceType);
        }

        // Checks if the given parameterType can function as the decorated instance of the given service type.
        private static bool IsDecorateeDependencyParameter(Type parameterType, Type serviceType)
        {
            if (parameterType == serviceType)
            {
                return true;
            }

            if (!parameterType.IsGenericType || !serviceType.IsGenericType)
            {
                return false;
            }

            return
                (serviceType.ContainsGenericParameters || parameterType.IsGenericType) &&
                serviceType.GetGenericTypeDefinition() == parameterType.GetGenericTypeDefinition();
        }

        private static IContainerControlledCollection CreateContainerControlledCollectionInternal(
            Type serviceType, params object[] arguments)
        {
            Type allInstancesEnumerableType =
                typeof(ContainerControlledCollection<>).MakeGenericType(serviceType);

            var collection = Activator.CreateInstance(allInstancesEnumerableType, arguments);

            return (IContainerControlledCollection)collection;
        }
        
        private sealed class ContainerControlledCollectionRegistration : Registration
        {
            private readonly Type serviceType;

            internal ContainerControlledCollectionRegistration(Type serviceType,
                IContainerControlledCollection collection, Container container)
                : base(Lifestyle.Singleton, container)
            {
                this.Collection = collection;
                this.serviceType = serviceType;
            }

            public override Type ImplementationType
            {
                get { return this.serviceType; }
            }

            internal IContainerControlledCollection Collection { get; private set; }

            public override Expression BuildExpression()
            {
                return Expression.Constant(this.Collection, this.serviceType);
            }

            internal override KnownRelationship[] GetRelationshipsCore()
            {
                return base.GetRelationshipsCore().Concat(this.Collection.GetRelationships()).ToArray();
            }
        }
    }
}