#region Copyright (c) 2013 Simple Injector Contributors
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

namespace SimpleInjector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;

    /// <summary>
    /// Helper methods for the container.
    /// </summary>
    internal static partial class Helpers
    {
        private static readonly Type[] AmbiguousTypes = new[] { typeof(Type), typeof(string) };

        internal static bool IsAmbiguousType(Type type)
        {
            return AmbiguousTypes.Contains(type);
        }
        
        internal static Lazy<T> ToLazy<T>(T value)
        {
            return new Lazy<T>(() => value, LazyThreadSafetyMode.None);
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

        internal static string ToFriendlyName(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType().ToFriendlyName() + "[]";
            }

            string name = type.Name;

            if (type.IsNested && !type.IsGenericParameter)
            {
                name = type.DeclaringType.ToFriendlyName() + "+" + type.Name;
            }

            var genericArguments = GetGenericArguments(type);

            if (!genericArguments.Any())
            {
                return name;
            }

            name = name.Substring(0, name.IndexOf('`'));

            var argumentNames = genericArguments.Select(argument => argument.ToFriendlyName()).ToArray();

            return name + "<" + string.Join(", ", argumentNames) + ">";
        }

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

        internal static bool IsConcreteType(Type serviceType)
        {
            // While array types are in fact concrete, we can not create them and creating them would be
            // pretty useless.
            return !serviceType.IsAbstract && !serviceType.ContainsGenericParameters && !serviceType.IsArray;
        }

        // Return a list of all base types T inherits, all interfaces T implements and T itself.
        internal static Type[] GetTypeHierarchyFor(Type type)
        {
            var types = new List<Type>();

            types.Add(type);
            types.AddRange(GetBaseTypes(type));
            types.AddRange(type.GetInterfaces());

            return types.ToArray();
        }

        internal static Action<T> CreateAction<T>(object action)
        {
            Type actionArgumentType = action.GetType().GetGenericArguments()[0];

            if (actionArgumentType.IsAssignableFrom(typeof(T)))
            {
                // In most cases, the given T is a concrete type such as ServiceImpl, and supplied action
                // object can be everything from Action<ServiceImpl>, to Action<IService>, to Action<object>.
                // Since Action<T> is contravariant (we're running under .NET 4.0) we can simply cast it.
                return (Action<T>)action;
            }

            // If we come here, the given T is most likely System.Object and this means that the caller needs
            // a Action<object>, the instance that needs to be casted, so we we need to build the following
            // delegate:
            // instance => action((ActionType)instance);
            var parameter = Expression.Parameter(typeof(T), "instance");

            Expression argument = Expression.Convert(parameter, actionArgumentType);

            var instanceInitializer = Expression.Lambda<Action<T>>(
                Expression.Invoke(Expression.Constant(action), argument),
                parameter);

            return instanceInitializer.Compile();
        }

        internal static IEnumerable CastCollection(IEnumerable collection, Type resultType)
        {
            // The collection is not a IEnumerable<[ServiceType]>. We wrap it in a 
            // CastEnumerator<[ServiceType]> to be able to supply it to the RegisterAll<T> method.
            var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(resultType);

            return (IEnumerable)castMethod.Invoke(null, new[] { collection });
        }

        private static IEnumerable<Type> GetBaseTypes(Type type)
        {
            Type baseType = type.BaseType;

            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }

        private static IEnumerable<T> CreateReadOnlyCollection<T>(IEnumerable<T> collection)
        {
            return RegisterAllEnumerable(collection);
        }

        // This method name does not describe what it does, but since the C# compiler will create a iterator
        // type named after this method, it allows us to return a type that has a nice name that will show up
        // during debugging.
        private static IEnumerable<T> RegisterAllEnumerable<T>(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                yield return item;
            }
        }

        private static IEnumerable<Type> GetGenericArguments(Type type)
        {
            if (!type.Name.Contains("`"))
            {
                return Enumerable.Empty<Type>();
            }

            int numberOfGenericArguments = Convert.ToInt32(type.Name.Substring(type.Name.IndexOf('`') + 1),
                 CultureInfo.InvariantCulture);

            var argumentOfTypeAndOuterType = type.GetGenericArguments();

            return argumentOfTypeAndOuterType
                .Skip(argumentOfTypeAndOuterType.Length - numberOfGenericArguments)
                .ToArray();
        }

        private static void ThrowWhenCollectionCanNotBeIterated(IEnumerable collection, Type serviceType,
            Action<object> itemProcessor)
        {
            try
            {
                var enumerator = collection.GetEnumerator();
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
                    IDisposable disposable = enumerator as IDisposable;

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
    }
}