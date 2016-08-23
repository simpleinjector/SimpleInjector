#region Copyright Simple Injector Contributors
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

namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Linq;
    using System.Reflection;

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
                throw new ArgumentException("nesting level bigger than 100 too high. Type: " +
                    type.ToFriendlyName(), nameof(nestingLevel));
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