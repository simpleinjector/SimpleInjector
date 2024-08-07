﻿// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    // NOTE: Although 'TypeExtensions' would be a more obvious name, there is already such type in .NET.
    // Using that same name would easily cause conflicts.
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>Useful extensions on <see cref="Type"/>.</summary>
    public static class TypesExtensions
    {
        internal const string FriendlyName =
            "SimpleInjector." + nameof(TypesExtensions) + "." + nameof(TypesExtensions.ToFriendlyName);

        /// <summary>
        /// Builds an easy to read type name. Namespaces will be omitted, and generic types will be displayed
        /// in a C#-like syntax. Ideal for reporting type names in exception messages.
        /// </summary>
        /// <param name="type">The type to convert.</param>
        /// <returns>A human-readable string representation of that type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the supplied argument is a null reference.</exception>
        public static string ToFriendlyName(this Type type) => type.ToFriendlyName(fullyQualifiedName: false);

        /// <summary>
        /// Returns true is there is a closed version of the supplied <paramref name="genericTypeDefinition"/>
        /// that is assignable from the current <paramref name="type"/>. This method returns true when either
        /// <paramref name="type"/> itself, one of its base classes or one of its implemented interfaces is a
        /// closed version of <paramref name="genericTypeDefinition"/>; otherwise false.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="genericTypeDefinition">The generic type definition to match.</param>
        /// <returns>True when type is assignable; otherwise false.</returns>
        public static bool IsClosedTypeOf(this Type type, Type genericTypeDefinition) =>
            GetClosedTypesOfInternal(type, genericTypeDefinition).Any();

        /// <summary>
        /// Gets the single closed version of <paramref name="genericTypeDefinition"/> that the current
        /// <paramref name="type"/> is assignable from. In case none or multiple matching closed types are
        /// found, and exception is thrown. Example: When <paramref name="type"/> is a type
        /// <c>class X : IX&lt;int&gt;, IFoo&lt;string&gt;</c> and <paramref name="genericTypeDefinition"/>
        /// is type <c>IX&lt;T&gt;</c>: this method will return type <c>IX&lt;int&gt;</c>.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="genericTypeDefinition">The generic type definition to match.</param>
        /// <returns>The matching closed type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="genericTypeDefinition"/> is not
        /// a generic type or when none of the base classes or implemented interfaces of <paramref name="type"/>
        /// are closed-versions of <paramref name="genericTypeDefinition"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when multiple matching closed generic types
        /// are found.</exception>
        public static Type GetClosedTypeOf(this Type type, Type genericTypeDefinition)
        {
            List<Type> types = GetClosedTypesOfInternal(type, genericTypeDefinition);

            if (types.Count == 0)
            {
                throw new ArgumentException(
                    StringResources.TypeIsNotAssignableFromOpenGenericType(type, genericTypeDefinition),
                    nameof(type));
            }

            if (types.Count > 1)
            {
                throw new InvalidOperationException(
                    StringResources.MultipleClosedTypesAreAssignableFromType(
                        type, genericTypeDefinition, types, nameof(TypesExtensions.GetClosedTypesOf)));
            }

            return types[0];
        }

        /// <summary>
        /// Gets the list of closed versions of <paramref name="genericTypeDefinition"/> that the current
        /// <paramref name="type"/> is assignable from. Example: When <paramref name="type"/> is a type
        /// <c>class X : IX&lt;int&gt;, IFoo&lt;string&gt;</c> and <paramref name="genericTypeDefinition"/>
        /// is type <c>IX&lt;T&gt;</c>: this method will return type <c>IX&lt;int&gt;</c>.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="genericTypeDefinition">The generic type definition to match.</param>
        /// <returns>A list of matching closed generic types.</returns>
        public static Type[] GetClosedTypesOf(this Type type, Type genericTypeDefinition)
            => GetClosedTypesOfInternal(type, genericTypeDefinition).ToArray();

        private static List<Type> GetClosedTypesOfInternal(Type type, Type genericTypeDefinition)
        {
            Requires.IsNotNull(type, nameof(type));
            Requires.IsNotNull(genericTypeDefinition, nameof(genericTypeDefinition));
            Requires.IsOpenGenericType(genericTypeDefinition, nameof(genericTypeDefinition));

            List<Type> assigableTypes = Types.GetTypeHierarchyFor(type);

            // PERF: To prevent memory allocations we don't use a LINQ query to filter out values, but simply remove
            // non-matching elements from the list.
            for (int index = assigableTypes.Count - 1; index >= 0; index--)
            {
                Type assigableType = assigableTypes[index];

                if (!genericTypeDefinition.IsGenericTypeDefinitionOf(assigableType)
                    || assigableType.ContainsGenericParameters())
                {
                    assigableTypes.RemoveAt(index);
                }
            }

            return assigableTypes;
        }
    }
}