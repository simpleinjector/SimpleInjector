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
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// Helper methods for the container.
    /// </summary>
    internal static class Helpers
    {
        internal static Lazy<T> ToLazy<T>(T value) =>
            new Lazy<T>(() => value, LazyThreadSafetyMode.PublicationOnly);

        internal static T AddReturn<T>(this HashSet<T> set, T value)
        {
            set.Add(value);
            return value;
        }

        internal static string ToCommaSeparatedText(this IEnumerable<string> values)
        {
            var names = values.ToArray();

            if (names.Length <= 1)
            {
                return names.FirstOrDefault() ?? string.Empty;
            }

            return string.Join(", ", names.Take(names.Length - 1)) + " and " + names.Last();
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
            TValue value;

            source.TryGetValue(key, out value);

            return value;
        }

        internal static void VerifyCollection(IEnumerable collection, Type serviceType)
        {
            // This construct looks a bit weird, but prevents the collection from being iterated twice.
            bool collectionContainsNullElements = false;

            ThrowWhenCollectionCanNotBeIterated(collection, serviceType, item =>
            {
                collectionContainsNullElements |= item == null;
            });

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
            // CastEnumerator<[ServiceType]> to be able to supply it to the RegisterCollection<T> method.
            var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(resultType);

            return (IEnumerable)castMethod.Invoke(null, new[] { collection });
        }

        // Partitions a collection in two separate collections, based on the predicate.
        internal static Tuple<T[], T[]> Partition<T>(this IEnumerable<T> collection, Predicate<T> predicate)
        {
            var trueList = new List<T>();
            var falseList = new List<T>();

            foreach (T item in collection)
            {
                List<T> list = predicate(item) ? trueList : falseList;
                list.Add(item);
            }

            return Tuple.Create(trueList.ToArray(), falseList.ToArray());
        }

        internal static MethodInfo GetMethod(Expression<Action> methodCall) =>
            ((MethodCallExpression)methodCall.Body).Method;

        internal static MethodInfo GetGenericMethodDefinition(Expression<Action> methodCall) =>
            GetMethod(methodCall).GetGenericMethodDefinition();

        internal static ConstructorInfo GetConstructor<T>(Expression<Func<T>> constructorCall) =>
            ((NewExpression)constructorCall.Body).Constructor;

        private static IEnumerable<T> CreateReadOnlyCollection<T>(IEnumerable<T> collection) =>
            RegisterCollectionEnumerable(collection);

        // This method name does not describe what it does, but since the C# compiler will create a iterator
        // type named after this method, it allows us to return a type that has a nice name that will show up
        // during debugging.
        private static IEnumerable<T> RegisterCollectionEnumerable<T>(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                yield return item;
            }
        }

        private static void ThrowWhenCollectionCanNotBeIterated(IEnumerable collection, Type serviceType,
            Action<object> itemProcessor)
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
                    var disposable = enumerator as IDisposable;

                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
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

        // .NET 4.6 adds System.Array.Empty<T>, but we don't have that yet in .NET 4.0 and 4.5.
        internal static class Array<T>
        {
            internal static readonly T[] Empty = new T[0];
        }
    }
}