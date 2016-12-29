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

namespace SimpleInjector.Decorators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Advanced;
    using SimpleInjector.Internals;

    internal static partial class DecoratorHelpers
    {
        private static readonly MethodInfo EnumerableSelectMethod =
            Helpers.GetGenericMethodDefinition(() => Enumerable.Select(null, (Func<int, int>)null));

        private static readonly MethodInfo DecoratorHelpersReadOnlyCollectionMethod =
            Helpers.GetGenericMethodDefinition(() => ReadOnlyCollection<int>(null));

        // This method name does not describe what it does, but since the C# compiler will create an iterator
        // type named after this method, it allows us to return a type that has a nice name that will show up
        // during debugging.
        public static IEnumerable<T> ReadOnlyCollection<T>(T[] collection)
        {
            for (int index = 0; index < collection.Length; index++)
            {
                yield return collection[index];
            }
        }

        internal static IEnumerable MakeReadOnly(Type elementType, Array collection)
        {
            var readOnlyCollection =
                DecoratorHelpersReadOnlyCollectionMethod
                .MakeGenericMethod(elementType)
                .Invoke(null, new object[] { collection });

            return (IEnumerable)readOnlyCollection;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
            Justification = "I don't care about the extra casts. This is not a performance critical part.")]
        internal static Type DetermineImplementationType(Expression expression,
            InstanceProducer registeredProducer)
        {
            var constantExpression = expression as ConstantExpression;

            // A ConstantExpression with null is supplied in case of a uncontrolled collection.
            if (constantExpression != null && object.ReferenceEquals(null, constantExpression.Value))
            {
                return constantExpression.Type;
            }

            return registeredProducer.Registration.ImplementationType;
        }

        internal static void AddRange<T>(this Collection<T> collection, IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                collection.Add(item);
            }
        }

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

        internal static IContainerControlledCollection CreateContainerControlledCollection(
            Type serviceType, Container container)
        {
            Type allInstancesEnumerableType =
                typeof(ContainerControlledCollection<>).MakeGenericType(serviceType);

            var collection = Activator.CreateInstance(allInstancesEnumerableType, new object[] { container });

            return (IContainerControlledCollection)collection;
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

        internal static bool DecoratesServiceType(Type serviceType, ConstructorInfo decoratorConstructor)
        {
            int numberOfServiceTypeDependencies =
                GetNumberOfServiceTypeDependencies(serviceType, decoratorConstructor);

            return numberOfServiceTypeDependencies == 1;
        }

        // Returns the base type of the decorator that can be used for decoration (because serviceType might
        // be open generic, while the base type might not be).
        internal static Type GetDecoratingBaseType(Type serviceType, ConstructorInfo decoratorConstructor)
        {
            var decoratorInterfaces =
                from abstraction in Types.GetBaseTypeCandidates(serviceType, decoratorConstructor.DeclaringType)
                where decoratorConstructor.GetParameters()
                    .Any(parameter => IsDecorateeParameter(parameter.ParameterType, abstraction))
                select abstraction;

            return decoratorInterfaces.FirstOrDefault();
        }

        internal static int GetNumberOfServiceTypeDependencies(Type serviceType,
            ConstructorInfo decoratorConstructor)
        {
            Type decoratorType = GetDecoratingBaseType(serviceType, decoratorConstructor);

            if (decoratorType == null)
            {
                return 0;
            }

            var validServiceTypeArguments =
                from parameter in decoratorConstructor.GetParameters()
                where IsDecorateeParameter(parameter.ParameterType, decoratorType)
                select parameter;

            return validServiceTypeArguments.Count();
        }

        internal static bool DecoratesBaseTypes(Type serviceType, ConstructorInfo decoratorConstructor)
        {
            var baseTypes = GetValidDecoratorConstructorArgumentTypes(serviceType,
                decoratorConstructor);

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

        internal static Type[] GetValidDecoratorConstructorArgumentTypes(Type serviceType, 
            ConstructorInfo decoratorConstructor)
        {
            Type decoratingBaseType = GetDecoratingBaseType(serviceType, decoratorConstructor);

            return (
                from baseType in decoratorConstructor.DeclaringType.GetBaseTypesAndInterfaces()
                where IsDecorateeDependencyParameter(baseType, decoratingBaseType)
                select baseType)
                .ToArray();
        }

        internal static bool IsDecorateeParameter(Type parameterType, Type decoratingType) => 
            IsDecorateeDependencyParameter(parameterType, decoratingType) ||
            IsDecorateeFactoryDependencyParameter(parameterType, decoratingType);

        // Checks if the given parameterType can function as the decorated instance of the given service type.
        internal static bool IsDecorateeFactoryDependencyParameter(Type parameterType, Type serviceType)
        {
            if (!parameterType.IsGenericType() || parameterType.GetGenericTypeDefinition() != typeof(Func<>))
            {
                return false;
            }

            Type funcArgumentType = parameterType.GetGenericArguments()[0];

            return IsDecorateeDependencyParameter(funcArgumentType, serviceType);
        }

        // Checks if the given parameterType can function as the decorated instance of the given service type.
        private static bool IsDecorateeDependencyParameter(Type parameterType, Type serviceType) => 
            parameterType == serviceType;

        private sealed class ContainerControlledCollectionRegistration : Registration
        {
            internal ContainerControlledCollectionRegistration(Type serviceType,
                IContainerControlledCollection collection, Container container)
                : base(Lifestyle.Singleton, container)
            {
                this.Collection = collection;
                this.ImplementationType = serviceType;
            }

            public override Type ImplementationType { get; }

            internal override bool MustBeVerified => !this.Collection.AllProducersVerified;

            internal IContainerControlledCollection Collection { get; }

            public override Expression BuildExpression() => 
                Expression.Constant(this.Collection, this.ImplementationType);

            internal override KnownRelationship[] GetRelationshipsCore() => 
                base.GetRelationshipsCore().Concat(this.Collection.GetRelationships()).ToArray();
        }
    }
}