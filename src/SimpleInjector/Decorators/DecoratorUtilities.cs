// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Decorators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class DecoratorUtilities
    {
        private static readonly MethodInfo EnumerableSelectMethod =
            Helpers.GetGenericMethodDefinition(() => Enumerable.Select(null, (Func<int, int>?)null));

        private static readonly MethodInfo DecoratorHelpersReadOnlyCollectionMethod =
            Helpers.GetGenericMethodDefinition(() => ReadOnlyCollection<int>(null!));

        // This method name does not describe what it does, but since the C# compiler will create an iterator
        // type named after this method, it allows us to return a type that has a nice name that will show up
        // during debugging.
        // WARNING: This method is public in an internal class. Don't make it internal, and don't move it to
        // a public class. It should not become part of Simple Injector's API.
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

        internal static Type DetermineImplementationType(Expression expression,
            InstanceProducer registeredProducer)
        {
            // A ConstantExpression with null is supplied in case of a uncontrolled collection.
            if (expression is ConstantExpression constant && constant.Value is null)
            {
                return constant.Type;
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

        internal static MethodCallExpression Select(
            Expression collectionExpression, Type type, Delegate selector)
        {
            // We make use of .NET's built in Enumerable.Select to wrap the collection with the decorators.
            var selectMethod = EnumerableSelectMethod.MakeGenericMethod(type, type);

            return Expression.Call(selectMethod, collectionExpression, Expression.Constant(selector));
        }
    }
}