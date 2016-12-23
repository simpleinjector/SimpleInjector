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
    using System.Linq;

    // NOTE: Although 'TypeExtensions' would be a more obvious name, there is already such type in .NET.
    // Using that same name would easily cause conflicts.
    /// <summary>Useful extensions on <see cref="Type"/>.</summary>
    public static class TypesExtensions
    {
        /// <summary>
        /// Builds an easy to read type name. Namespaces will be omitted, and generic types will be displayed 
        /// in a C#-like syntax. Ideal for reporting type names in exception messages.
        /// </summary>
        /// <param name="type">The type to convert.</param>
        /// <returns>A human-readable string representation of that type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the supplied argument is a null reference.</exception>
        public static string ToFriendlyName(this Type type) => type.ToFriendlyName(fullyQualifiedName: false);

        /// <summary>
        /// Returns true is the <paramref name="type"/> is assignable from a closed version of the supplied
        /// <paramref name="closedGenericType"/>.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="closedGenericType">The generic type definition to match.</param>
        /// <returns>True when type is assignable; otherwise false.</returns>
        public static bool IsClosedTypeOf(this Type type, Type closedGenericType) =>
            GetClosedTypesOfInternal(type, closedGenericType).Any();

        /// <summary>
        /// Gets the list of closed versions of <paramref name="genericTypeDefinition"/> that are assignable
        /// from <paramref name="type"/>. Example: When <paramref name="type"/> is a type <b>X</b> that
        /// implements interfaces <b>IX&lt;int&gt;</b> and <b>IFoo&lt;string&gt;</b>, and 
        /// <paramref name="genericTypeDefinition"/> is type <b>IX&lt;T&gt;</b>, than this method will
        /// return type <b>IX&lt;int&gt;</b>.
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