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
                    .Any(parameter => IsDecorateeParameter(parameter, abstraction))
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
                where IsDecorateeParameter(parameter, decoratorType)
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
                where IsDecorateeDependencyType(baseType, decoratingBaseType)
                select baseType)
                .ToArray();
        }

        internal static bool IsDecorateeParameter(ParameterInfo parameter, Type decoratingType) =>
            IsDecorateeDependencyType(parameter.ParameterType, decoratingType)
            || IsDecorateeFactoryDependencyType(parameter.ParameterType, decoratingType);

        internal static bool IsDecorateeDependencyType(Type dependencyType, Type serviceType)
        {
            return dependencyType == serviceType;
        }

        internal static bool IsDecorateeFactoryDependencyType(Type dependencyType, Type decoratingType) =>
            IsScopelessDecorateeFactoryDependencyType(dependencyType, decoratingType)
            || IsScopeDecorateeFactoryDependencyParameter(dependencyType, decoratingType);

        internal static bool IsScopelessDecorateeFactoryDependencyType(Type dependencyType, Type decoratingType)
        {
            return typeof(Func<>).IsGenericTypeDefinitionOf(dependencyType)
                && dependencyType == typeof(Func<>).MakeGenericType(decoratingType);
        }

        internal static bool IsScopeDecorateeFactoryDependencyParameter(Type parameterType, Type decoratingType)
        {
            return typeof(Func<,>).IsGenericTypeDefinitionOf(parameterType)
                && parameterType == typeof(Func<,>).MakeGenericType(typeof(Scope), decoratingType);
        }
    }
}