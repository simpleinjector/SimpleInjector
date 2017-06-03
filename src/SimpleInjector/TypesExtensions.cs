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
        /// a generic type or when none of the base classes or implemented interfaces of </exception>
        /// <exception cref="InvalidOperationException">Thrown when multiple matching closed generic types
        /// are found.</exception>
        public static Type GetClosedTypeOf(this Type type, Type genericTypeDefinition)
        {
            Type[] types = GetClosedTypesOfInternal(type, genericTypeDefinition).ToArray();

            if (types.Length == 0)
            {
                throw new ArgumentException(
                    StringResources.TypeIsNotAssignableFromOpenGenericType(type, genericTypeDefinition),
                    nameof(type));
            }
            
            if (types.Length > 1)
            {
                throw new InvalidOperationException(
                    StringResources.MultipleClosedTypesAreAssignableFromType(type, genericTypeDefinition,
                        types, nameof(TypesExtensions.GetClosedTypesOf)));
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

        private static IEnumerable<Type> GetClosedTypesOfInternal(Type type, Type genericTypeDefinition)
        {
            Requires.IsNotNull(type, nameof(type));
            Requires.IsNotNull(genericTypeDefinition, nameof(genericTypeDefinition));
            Requires.IsOpenGenericType(genericTypeDefinition, nameof(genericTypeDefinition));

            return
                from assigableType in Types.GetTypeHierarchyFor(type)
                where genericTypeDefinition.IsGenericTypeDefinitionOf(assigableType)
                select assigableType;
        }
    }
}