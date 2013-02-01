#region Copyright (c) 2013 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2013 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal static class TypeGeneralizer
    {
        // This method takes generic type and returns a 'partially' generic type definition of that same type,
        // where all generic arguments up to the given nesting level. This allows us to group generic types
        // by their partial generic type definition, which allows a much nicer user experience.
        internal static Type MakeTypePartiallyGenericUpToLevel(Type type, int nestingLevel)
        {
            // example given type: IEnumerable<IQueryProcessor<MyQuery<Alpha>, int[]>>
            // level 0 returns: IEnumerable<T>
            // level 1 returns: IEnumerable<IQueryProcessor<TQuery, TResult>>
            // level 2 returns: IEnumerable<IQueryProcessor<MyQuery<T>, int[]>
            // level 3 returns: IEnumerable<IQueryProcessor<MyQuery<Alpha>, int[]>
            // level 4 returns: !type.ContainsGenericParameters
            if (!type.IsGenericType)
            {
                return type;
            }

            if (nestingLevel == 0)
            {
                return type.GetGenericTypeDefinition();
            }

            var arguments =
                from argument in type.GetGenericArguments()
                select MakeTypePartiallyGenericUpToLevel(argument, nestingLevel - 1);

            return type.GetGenericTypeDefinition().MakeGenericType(arguments.ToArray());
        }
    }
}
