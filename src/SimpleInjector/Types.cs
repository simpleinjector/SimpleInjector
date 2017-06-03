#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2016 Simple Injector Contributors
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
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Decorators;
    using SimpleInjector.Internals;

    // Internal helper methods on System.Type.
    internal static class Types
    {
        private static readonly Type[] AmbiguousTypes = new[] { typeof(Type), typeof(string), typeof(Scope), typeof(Container) };

        private static readonly Func<Type[], string> FullyQualifiedNameArgumentsFormatter =
            args => string.Join(", ", args.Select(a => a.ToFriendlyName(fullyQualifiedName: true)).ToArray());

        private static readonly Func<Type[], string> SimpleNameArgumentsFormatter =
            args => string.Join(", ", args.Select(a => a.ToFriendlyName(fullyQualifiedName: false)).ToArray());

        private static readonly Func<Type[], string> CSharpFriendlyNameArgumentFormatter =
            args => string.Join(",", args.Select(argument => string.Empty).ToArray());

        internal static bool ContainsGenericParameter(this Type type) =>
            type.IsGenericParameter ||
                (type.IsGenericType() && type.GetGenericArguments().Any(ContainsGenericParameter));

        internal static bool IsGenericArgument(this Type type) =>
            type.IsGenericParameter || type.GetGenericArguments().Any(arg => arg.IsGenericArgument());

        internal static bool IsGenericTypeDefinitionOf(this Type genericTypeDefinition, Type typeToCheck) =>
            typeToCheck.IsGenericType() && typeToCheck.GetGenericTypeDefinition() == genericTypeDefinition;

        internal static bool IsAmbiguousOrValueType(Type type) =>
            IsAmbiguousType(type) || type.IsValueType();

        internal static bool IsAmbiguousType(Type type) => AmbiguousTypes.Contains(type);

        internal static bool IsPartiallyClosed(this Type type) =>
            type.IsGenericType()
            && type.ContainsGenericParameters()
            && type.GetGenericTypeDefinition() != type;

        // This method returns IQueryHandler<,> while ToFriendlyName returns IQueryHandler<TQuery, TResult>
        internal static string ToCSharpFriendlyName(Type genericTypeDefinition) =>
            ToCSharpFriendlyName(genericTypeDefinition, fullyQualifiedName: false);

        internal static string ToCSharpFriendlyName(Type genericTypeDefinition, bool fullyQualifiedName)
        {
            Requires.IsNotNull(genericTypeDefinition, nameof(genericTypeDefinition));

            return genericTypeDefinition.ToFriendlyName(fullyQualifiedName, CSharpFriendlyNameArgumentFormatter);
        }

        internal static string ToFriendlyName(this Type type, bool fullyQualifiedName)
        {
            Requires.IsNotNull(type, nameof(type));

            return type.ToFriendlyName(
                fullyQualifiedName,
                fullyQualifiedName ? FullyQualifiedNameArgumentsFormatter : SimpleNameArgumentsFormatter);
        }

        // While array types are in fact concrete, we can not create them and creating them would be
        // pretty useless.
        internal static bool IsConcreteConstructableType(Type serviceType) =>
            !serviceType.ContainsGenericParameters() && IsConcreteType(serviceType);

        // While array types are in fact concrete, we can not create them and creating them would be
        // pretty useless.
        internal static bool IsConcreteType(Type serviceType) =>
            !serviceType.IsAbstract() && !serviceType.IsArray && serviceType != typeof(object) &&
            !typeof(Delegate).IsAssignableFrom(serviceType);

        // TODO: Find out if the call to DecoratesBaseTypes is needed (all tests pass without it).
        internal static bool IsDecorator(Type serviceType, ConstructorInfo implementationConstructor) =>
            DecoratorHelpers.DecoratesServiceType(serviceType, implementationConstructor) &&
            DecoratorHelpers.DecoratesBaseTypes(serviceType, implementationConstructor);

        internal static bool IsComposite(Type serviceType, ConstructorInfo implementationConstructor) =>
            CompositeHelpers.ComposesServiceType(serviceType, implementationConstructor);

        internal static bool IsGenericCollectionType(Type serviceType)
        {
            if (!serviceType.IsGenericType())
            {
                return false;
            }

            Type serviceTypeDefinition = serviceType.GetGenericTypeDefinition();

            return
#if !NET40
                serviceTypeDefinition == typeof(IReadOnlyList<>) ||
                serviceTypeDefinition == typeof(IReadOnlyCollection<>) ||
#endif
                serviceTypeDefinition == typeof(IEnumerable<>) ||
                serviceTypeDefinition == typeof(IList<>) ||
                serviceTypeDefinition == typeof(ICollection<>);
        }

        // Return a list of all base types T inherits, all interfaces T implements and T itself.
        internal static ICollection<Type> GetTypeHierarchyFor(Type type)
        {
            var types = new List<Type>(4);

            types.Add(type);
            types.AddRange(GetBaseTypes(type));
            types.AddRange(type.GetInterfaces());

            return types;
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
        internal static IEnumerable<Type> GetBaseTypeCandidates(Type serviceType, Type implementationType) =>
            from baseType in implementationType.GetBaseTypesAndInterfaces()
            where baseType == serviceType || (
                baseType.IsGenericType() && serviceType.IsGenericType() &&
                baseType.GetGenericTypeDefinition() == serviceType.GetGenericTypeDefinition())
            select baseType;
        
        // PERF: This method is a hot path in the registration phase and can get called thousands of times
        // during application startup. For that reason it is heavily optimized to prevent unneeded memory 
        // allocations as much as possible. This method is called in a loop by Container.GetTypesToRegister 
        // and GetTypesToRegister is called by overloads of Register and RegisterCollection.
        internal static bool ServiceIsAssignableFromImplementation(Type service, Type implementation)
        {
            if (!service.IsGenericType())
            {
                return service.IsAssignableFrom(implementation);
            }

            if (implementation.IsGenericType() && implementation.GetGenericTypeDefinition() == service)
            {
                return true;
            }

            // PERF: We don't use LINQ to prevent unneeded memory allocations.
            // Unfortunately we can't prevent memory allocations while calling GetInterfaces() :-(
            foreach (Type interfaceType in implementation.GetInterfaces())
            {
                if (IsGenericImplementationOf(interfaceType, service))
                {
                    return true;
                }
            }

            // PERF: We don't call GetBaseTypes(), to prevent memory allocations.
            Type baseType = implementation.BaseType() ?? (implementation != typeof(object) ? typeof(object) : null);

            while (baseType != null)
            {
                if (IsGenericImplementationOf(baseType, service))
                {
                    return true;
                }

                baseType = baseType.BaseType();
            }

            return false;
        }

        // Example: when implementation implements IComparable<int> and IComparable<double>, the method will
        // return typeof(IComparable<int>) and typeof(IComparable<double>) when serviceType is
        // typeof(IComparable<>).
        internal static IEnumerable<Type> GetBaseTypesAndInterfacesFor(this Type type, Type serviceType) =>
            GetGenericImplementationsOf(type.GetBaseTypesAndInterfaces(), serviceType);

        internal static IEnumerable<Type> GetTypeBaseTypesAndInterfacesFor(this Type type, Type serviceType) =>
            GetGenericImplementationsOf(type.GetTypeBaseTypesAndInterfaces(), serviceType);

        internal static IEnumerable<Type> GetBaseTypesAndInterfaces(this Type type) =>
            type.GetInterfaces().Concat(type.GetBaseTypes());

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

        private static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            Type baseType = type.BaseType() ?? (type != typeof(object) ? typeof(object) : null);

            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType();
            }
        }

        private static IEnumerable<Type> GetGenericImplementationsOf(IEnumerable<Type> types, Type serviceType) =>
            from type in types
            where IsGenericImplementationOf(type, serviceType)
            select type;

        private static bool IsGenericImplementationOf(Type type, Type serviceType) =>
            type == serviceType
            || serviceType.IsVariantVersionOf(type)
            || (type.IsGenericType() && type.GetGenericTypeDefinition() == serviceType);

        private static bool IsVariantVersionOf(this Type type, Type otherType) =>
            type.IsGenericType()
            && otherType.IsGenericType()
            && type.GetGenericTypeDefinition() == otherType.GetGenericTypeDefinition()
            && type.IsAssignableFrom(otherType);

        private static string ToFriendlyName(this Type type, bool fullyQualifiedName,
            Func<Type[], string> argumentsFormatter)
        {
            if (type.IsArray)
            {
                return type.GetElementType().ToFriendlyName(fullyQualifiedName, argumentsFormatter) + "[]";
            }

            string name = fullyQualifiedName ? (type.FullName ?? type.Name) : type.Name;

            if (type.IsNested && !type.IsGenericParameter)
            {
                name = type.DeclaringType.ToFriendlyName(fullyQualifiedName, argumentsFormatter) + "." + name;
            }

            var genericArguments = GetGenericArguments(type);

            if (!genericArguments.Any())
            {
                return name;
            }

            name = name.Contains("`") ? name.Substring(0, name.LastIndexOf('`')) : name;

            return name + "<" + argumentsFormatter(genericArguments) + ">";
        }

        private static Type[] GetGenericArguments(Type type) =>
            type.IsNested
                ? type.GetGenericArguments().Skip(type.DeclaringType.GetGenericArguments().Length).ToArray()
                : type.GetGenericArguments();
    }
}
