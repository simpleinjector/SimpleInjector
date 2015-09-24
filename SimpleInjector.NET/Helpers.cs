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
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using Decorators;
    using SimpleInjector.Internals;

    /// <summary>
    /// Helper methods for the container.
    /// </summary>
    internal static partial class Helpers
    {
        private static readonly Type[] AmbiguousTypes = new[] { typeof(Type), typeof(string) };

        internal static bool ContainsGenericParameter(this Type type)
        {
            return type.IsGenericParameter ||
                (type.IsGenericType && type.GetGenericArguments().Any(ContainsGenericParameter));
        }

        internal static bool IsGenericArgument(this Type type)
        {
            return type.IsGenericParameter || type.GetGenericArguments().Any(arg => arg.IsGenericArgument());
        }

        internal static bool IsGenericTypeDefinitionOf(this Type genericTypeDefinition, Type typeToCheck)
        {
            return typeToCheck.IsGenericType && typeToCheck.GetGenericTypeDefinition() == genericTypeDefinition;
        }

        internal static bool IsAmbiguousOrValueType(Type type)
        {
            return IsAmbiguousType(type) || type.IsValueType;
        }

        internal static bool IsAmbiguousType(Type type)
        {
            return AmbiguousTypes.Contains(type);
        }

        internal static Lazy<T> ToLazy<T>(T value)
        {
            return new Lazy<T>(() => value, LazyThreadSafetyMode.PublicationOnly);
        }

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

        internal static bool IsPartiallyClosed(this Type type)
        {
            return type.IsGenericType && type.ContainsGenericParameters && type.GetGenericTypeDefinition() != type;
        }

        // This method returns IQueryHandler<,> while ToFriendlyName returns IQueryHandler<TQuery, TResult>
        internal static string ToCSharpFriendlyName(Type genericTypeDefinition)
        {
            Requires.IsNotNull(genericTypeDefinition, nameof(genericTypeDefinition));

            return genericTypeDefinition.ToFriendlyName(arguments =>
                string.Join(",", arguments.Select(argument => string.Empty).ToArray()));
        }

        internal static string ToFriendlyName(this Type type)
        {
            Requires.IsNotNull(type, nameof(type));

            return type.ToFriendlyName(arguments =>
                string.Join(", ", arguments.Select(argument => argument.ToFriendlyName()).ToArray()));
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

        internal static bool IsConcreteConstructableType(Type serviceType)
        {
            // While array types are in fact concrete, we can not create them and creating them would be
            // pretty useless.
            return !serviceType.ContainsGenericParameters && IsConcreteType(serviceType);
        }

        internal static bool IsConcreteType(Type serviceType)
        {
            // While array types are in fact concrete, we can not create them and creating them would be
            // pretty useless.
            return !serviceType.IsAbstract && !serviceType.IsArray && serviceType != typeof(object) &&
                !typeof(Delegate).IsAssignableFrom(serviceType);
        }

        internal static bool IsDecorator(Type serviceType, ConstructorInfo implementationConstructor)
        {
            // TODO: Find out if the call to DecoratesBaseTypes is needed (all tests pass without it).
            return DecoratorHelpers.DecoratesServiceType(serviceType, implementationConstructor) &&
                DecoratorHelpers.DecoratesBaseTypes(serviceType, implementationConstructor);
        }

        internal static bool IsComposite(Type serviceType, ConstructorInfo implementationConstructor)
        {
            return CompositeHelpers.ComposesServiceType(serviceType, implementationConstructor);
        }

        internal static bool IsGenericCollectionType(Type serviceType)
        {
            if (!serviceType.IsGenericType)
            {
                return false;
            }

            Type serviceTypeDefinition = serviceType.GetGenericTypeDefinition();

            return
#if NET45
                serviceTypeDefinition == typeof(IReadOnlyList<>) ||
                serviceTypeDefinition == typeof(IReadOnlyCollection<>) ||
#endif
                serviceTypeDefinition == typeof(IEnumerable<>) ||
                serviceTypeDefinition == typeof(IList<>) ||
                serviceTypeDefinition == typeof(ICollection<>);
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

        /// <summary>
        /// Returns a list of base types and interfaces of implementationType that either
        /// equal to serviceType or are closed or partially closed version of serviceType (in case 
        /// serviceType itself is generic).
        /// So:
        /// -in case serviceType is non generic, only serviceType will be returned.
        /// -If implementationType is open generic, serviceType will be returned (or a partially closed 
        ///  version of serviceType is returned).
        /// -If serviceType is generic and implementationType is not, a closed version of serviceType will
        ///  be returned.
        /// -If implementationType implements multiple (partially) closed versions of serviceType, all those
        ///  (partially) closed versions will be returned.
        /// </summary>
        /// <param name="serviceType">The (open generic) service type to match.</param>
        /// <param name="implementationType">The implementationType to search.</param>
        /// <returns>A list of types.</returns>
        internal static IEnumerable<Type> GetBaseTypeCandidates(Type serviceType, Type implementationType)
        {
            return
                from baseType in implementationType.GetBaseTypesAndInterfaces()
                where baseType == serviceType || (
                    baseType.IsGenericType && serviceType.IsGenericType &&
                    baseType.GetGenericTypeDefinition() == serviceType.GetGenericTypeDefinition())
                select baseType;
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
            // CastEnumerator<[ServiceType]> to be able to supply it to the RegisterCollection<T> method.
            var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(resultType);

            return (IEnumerable)castMethod.Invoke(null, new[] { collection });
        }

        // PERF: This method is a hot path in the registration phase and can get called thousands of times
        // during application startup. For that reason it is heavily optimized to prevent unneeded memory 
        // allocations as much as possible. This method is called in a loop by Container.GetTypesToRegister 
        // and GetTypesToRegister is called by overloads of Register and RegisterCollection.
        internal static bool ServiceIsAssignableFromImplementation(Type service, Type implementation)
        {
            if (!service.IsGenericType)
            {
                return service.IsAssignableFrom(implementation);
            }

            if (implementation.IsGenericType && implementation.GetGenericTypeDefinition() == service)
            {
                return true;
            }

            // PERF: We don't use LINQ to prevent unneeded memory allocations.
            // Unfortunately we can't prevent memory allocations while calling GetInstances() :-(
            foreach (Type interfaceType in implementation.GetInterfaces())
            {
                if (IsGenericImplementationOf(interfaceType, service))
                {
                    return true;
                }
            }

            // PERF: We don't call GetBaseTypes(), to prevent memory allocations.
            Type baseType = implementation.BaseType ?? (implementation != typeof(object) ? typeof(object) : null);

            while (baseType != null)
            {
                if (IsGenericImplementationOf(baseType, service))
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }

        // Example: when implementation implements IComparable<int> and IComparable<double>, the method will
        // return typeof(IComparable<int>) and typeof(IComparable<double>) when serviceType is
        // typeof(IComparable<>).
        internal static IEnumerable<Type> GetBaseTypesAndInterfacesFor(this Type type, Type serviceType)
        {
            return GetGenericImplementationsOf(type.GetBaseTypesAndInterfaces(), serviceType);
        }

        internal static IEnumerable<Type> GetTypeBaseTypesAndInterfacesFor(this Type type, Type serviceType)
        {
            return GetGenericImplementationsOf(type.GetTypeBaseTypesAndInterfaces(), serviceType);
        }

        internal static IEnumerable<Type> GetBaseTypesAndInterfaces(this Type type)
        {
            return type.GetInterfaces().Concat(type.GetBaseTypes());
        }

        internal static IEnumerable<Type> GetTypeBaseTypesAndInterfaces(this Type type)
        {
            var thisType = new[] { type };
            return thisType.Concat(type.GetBaseTypesAndInterfaces());
        }

        internal static ContainerControlledItem[] GetClosedGenericImplementationsFor(
            Type closedGenericServiceType, IEnumerable<ContainerControlledItem> containerControlledItems,
            bool includeVariantTypes = true)
        {
            return (
                from item in containerControlledItems
                let openGenericImplementation = item.ImplementationType
                let builder = new GenericTypeBuilder(closedGenericServiceType, openGenericImplementation)
                let result = builder.BuildClosedGenericImplementation()
                where result.ClosedServiceTypeSatisfiesAllTypeConstraints || (
                    includeVariantTypes && closedGenericServiceType.IsAssignableFrom(openGenericImplementation))
                let closedImplementation = result.ClosedServiceTypeSatisfiesAllTypeConstraints
                    ? result.ClosedGenericImplementation
                    : openGenericImplementation
                select item.Registration != null ? item : ContainerControlledItem.CreateFromType(closedImplementation))
                .ToArray();
        }

        // Partitions a collection in two seperate collections, based on the predicate.
        internal static Tuple<T[], T[]> Partition<T>(this IEnumerable<T> collection, Predicate<T> predicate)
        {
            var trueList = new List<T>();
            var falseList = new List<T>();

            foreach (var item in collection)
            {
                var list = predicate(item) ? trueList : falseList;
                list.Add(item);
            }

            return Tuple.Create(trueList.ToArray(), falseList.ToArray());
        }

        private static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            Type baseType = type.BaseType ?? (type != typeof(object) ? typeof(object) : null);

            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }
        
        private static IEnumerable<Type> GetGenericImplementationsOf(IEnumerable<Type> types, Type serviceType)
        {
            return
                from type in types
                where IsGenericImplementationOf(type, serviceType)
                select type;
        }

        private static bool IsGenericImplementationOf(Type type, Type serviceType)
        {
            return type == serviceType || serviceType.IsVariantVersionOf(type) ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == serviceType);
        }

        private static bool IsVariantVersionOf(this Type type, Type otherType)
        {
            return
                type.IsGenericType &&
                otherType.IsGenericType &&
                type.GetGenericTypeDefinition() == otherType.GetGenericTypeDefinition() &&
                type.IsAssignableFrom(otherType);
        }

        private static string ToFriendlyName(this Type type, Func<Type[], string> argumentsFormatter)
        {
            if (type.IsArray)
            {
                return type.GetElementType().ToFriendlyName(argumentsFormatter) + "[]";
            }

            string name = type.Name;

            if (type.IsNested && !type.IsGenericParameter)
            {
                name = type.DeclaringType.ToFriendlyName(argumentsFormatter) + "." + type.Name;
            }

            var genericArguments = GetGenericArguments(type);

            if (!genericArguments.Any())
            {
                return name;
            }

            name = name.Substring(0, name.IndexOf('`'));

            return name + "<" + argumentsFormatter(genericArguments.ToArray()) + ">";
        }

        private static IEnumerable<T> CreateReadOnlyCollection<T>(IEnumerable<T> collection)
        {
            return RegisterCollectionEnumerable(collection);
        }

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

        // .NET 4.6 adds System.Array.Empty<T>, but we don't have that yet in .NET 4.0 and 4.5.
        internal static class Array<T> 
        {
            internal static readonly T[] Empty = new T[0];
        }
    }
}