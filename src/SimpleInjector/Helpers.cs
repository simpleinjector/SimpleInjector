// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Internals;

    /// <summary>
    /// Helper methods for the container.
    /// </summary>
    internal static class Helpers
    {
        // Use System.Numerics.Hashing.HashHelpers.Combine instead, when all targets can take a dependency on
        // System.Numerics.Hashing. The used hashing algorithm here is identical to that in HashHelpers.
        internal static int CombineHashes(int a, int b)
        {
            uint num = (uint)((a << 5) | (int)((uint)a >> 27));
            return ((int)num + a) ^ b;
        }

        internal static int Hash(object? a) => a?.GetHashCode() ?? 0;

        internal static int Hash(object? a, object? b) => CombineHashes(a?.GetHashCode() ?? 0, b?.GetHashCode() ?? 0);

        internal static int Hash(object? a, object? b, object? c, object? d) =>
            Hash(Hash(Hash(a, b), c), d);

        internal static LazyEx<T> ToLazy<T>(T value) where T : class =>
            new LazyEx<T>(value);

        internal static T AddReturn<T>(this HashSet<T> set, T value)
        {
            set.Add(value);
            return value;
        }

        internal static string ToCommaSeparatedText(this IEnumerable<string> values)
        {
            var names = values.ToArray();

            return names.Length switch
            {
                0 => string.Empty,
                1 => names[0],
                2 => names[0] + " and " + names[1],

                // For three names or more, we use the Oxford comma.
                _ => string.Join(", ", names.Take(names.Length - 1)) + ", and " + names.Last(),
            };
        }

        // This makes the collection immutable for the consumer. The creator might still be able to change
        // the collection in the background.
        internal static IEnumerable<T> MakeReadOnly<T>(this IEnumerable<T> collection)
        {
            bool typeIsReadOnlyCollection = collection is ReadOnlyCollection<T>;

            bool typeIsMutable = collection is T[] || collection is IList || collection is ICollection<T>;

            if (typeIsReadOnlyCollection || !typeIsMutable)
            {
                return collection;
            }
            else
            {
                return CreateReadOnlyCollection(collection);
            }
        }

        internal static Dictionary<TKey, TValue> MakeCopy<TKey, TValue>(this Dictionary<TKey, TValue> source)
        {
            // We pick an initial capacity of count + 1, because we'll typically be adding 1 item to this copy.
            int initialCapacity = source.Count + 1;

            var copy = new Dictionary<TKey, TValue>(initialCapacity, source.Comparer);

            foreach (var pair in source)
            {
                copy.Add(pair.Key, pair.Value);
            }

            return copy;
        }

        [DebuggerStepThrough]
        internal static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key)
        {
            source.TryGetValue(key, out TValue value);

            return value;
        }

        internal static void VerifyCollection(IEnumerable collection, Type serviceType)
        {
            // This construct looks a bit weird, but prevents the collection from being iterated twice.
            bool collectionContainsNullElements = false;

            ThrowWhenCollectionCanNotBeIterated(
                collection,
                serviceType,
                item => collectionContainsNullElements |= item is null);

            ThrowWhenCollectionContainsNullElements(serviceType, collectionContainsNullElements);
        }

        internal static Action<T> CreateAction<T>(object action)
        {
            if (typeof(Action<T>).IsAssignableFrom(action.GetType()))
            {
                return (Action<T>)action;
            }

            // If we come here, the given T is most likely System.Object and this means that the caller needs
            // an Action<object>, the instance that needs to be casted, so we we need to build the following
            // delegate:
            // instance => action((actionArgumentType)instance);
            var parameter = Expression.Parameter(typeof(T), "instance");

            Type actionArgumentType = action.GetType().GetGenericArguments()[0];

            Expression argument = Expression.Convert(parameter, actionArgumentType);

            var instanceInitializer = Expression.Lambda<Action<T>>(
                Expression.Invoke(Expression.Constant(action), argument),
                parameter);

            return instanceInitializer.Compile();
        }

        internal static IEnumerable CastCollection(IEnumerable collection, Type resultType)
        {
            // The collection is not a IEnumerable<[ServiceType]>. We wrap it in a
            // CastEnumerator<[ServiceType]> to be able to supply it to the Collections.Register<T> method.
            var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(resultType);

            return (IEnumerable)castMethod.Invoke(null, new[] { collection });
        }

        // Partitions a collection in two separate collections, based on the predicate.
        internal static Tuple<List<T>, List<T>> Partition<T>(this T[] collection, Predicate<T> predicate)
        {
            // PERF: Assuming the predicate will return true most of the time.
            var trueList = new List<T>(capacity: collection.Length);
            var falseList = new List<T>();

            foreach (T item in collection)
            {
                if (predicate(item))
                {
                    trueList.Add(item);
                }
                else
                {
                    falseList.Add(item);
                }
            }

            return Tuple.Create(trueList, falseList);
        }

        internal static MethodInfo GetMethod(Expression<Action> methodCall) =>
            ((MethodCallExpression)methodCall.Body).Method;

        internal static MethodInfo GetGenericMethodDefinition(Expression<Action> methodCall) =>
            GetMethod(methodCall).GetGenericMethodDefinition();

        internal static ConstructorInfo GetConstructor<T>(Expression<Func<T>> constructorCall) =>
            ((NewExpression)constructorCall.Body).Constructor;

        private static IEnumerable<T> CreateReadOnlyCollection<T>(IEnumerable<T> collection) =>
            Collections_Register_Enumerable(collection);

        // This method name does not describe what it does, but since the C# compiler will create a iterator
        // type named after this method, it allows us to return a type that has a nice name that will show up
        // during debugging.
        private static IEnumerable<T> Collections_Register_Enumerable<T>(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                yield return item;
            }
        }

        private static void ThrowWhenCollectionCanNotBeIterated(
            IEnumerable collection, Type serviceType, Action<object> itemProcessor)
        {
            try
            {
                IEnumerator enumerator = collection.GetEnumerator();

                try
                {
                    // Just iterate the collection.
                    while (enumerator.MoveNext())
                    {
                        itemProcessor(enumerator.Current);
                    }
                }
                finally
                {
                    (enumerator as IDisposable)?.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    StringResources.ConfigurationInvalidIteratingCollectionFailed(serviceType, ex), ex);
            }
        }

        private static void ThrowWhenCollectionContainsNullElements(Type serviceType,
            bool collectionContainsNullItems)
        {
            if (collectionContainsNullItems)
            {
                throw new InvalidOperationException(
                    StringResources.ConfigurationInvalidCollectionContainsNullElements(serviceType));
            }
        }

        // .NET 4.6 adds System.Array.Empty<T>, but we don't have that yet in .NET 4.5.
        internal static class Array<T>
        {
            internal static readonly T[] Empty = new T[0];
        }
    }
}