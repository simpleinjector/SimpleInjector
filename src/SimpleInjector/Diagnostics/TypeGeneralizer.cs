// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Linq;

    internal static class TypeGeneralizer
    {
        // This method takes generic type and returns a 'partially' generic type definition of that same type,
        // where all generic arguments up to the given nesting level. This allows us to group generic types
        // by their partial generic type definition, which allows a much nicer user experience.
        internal static Type MakeTypePartiallyGenericUpToLevel(Type type, int nestingLevel)
        {
            if (nestingLevel > 100)
            {
                // Stack overflow prevention
                throw new ArgumentException(
                    "nesting level bigger than 100 too high. Type: " + type.ToFriendlyName(),
                    nameof(nestingLevel));
            }

            // example given type: IEnumerable<IQueryProcessor<MyQuery<Alpha>, int[]>>
            // nestingLevel 4 returns: IEnumerable<IQueryHandler<MyQuery<Alpha>, int[]>
            // nestingLevel 3 returns: IEnumerable<IQueryHandler<MyQuery<Alpha>, int[]>
            // nestingLevel 2 returns: IEnumerable<IQueryHandler<MyQuery<T>, int[]>
            // nestingLevel 1 returns: IEnumerable<IQueryHandler<TQuery, TResult>>
            // nestingLevel 0 returns: IEnumerable<T>
            if (!type.IsGenericType())
            {
                return type;
            }

            if (nestingLevel == 0)
            {
                return type.GetGenericTypeDefinition();
            }

            return MakeTypePartiallyGeneric(type, nestingLevel);
        }

        private static Type MakeTypePartiallyGeneric(Type type, int nestingLevel)
        {
            var arguments = (
                from argument in type.GetGenericArguments()
                select MakeTypePartiallyGenericUpToLevel(argument, nestingLevel - 1))
                .ToArray();

            try
            {
                return type.GetGenericTypeDefinition().MakeGenericType(arguments.ToArray());
            }
            catch (ArgumentException)
            {
                // If we come here, MakeGenericType failed because of generic type constraints.
                // In that case we skip this nesting level and go one level deeper.
                return MakeTypePartiallyGenericUpToLevel(type, nestingLevel + 1);
            }
        }
    }
}