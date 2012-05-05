#region Copyright (c) 2012 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2012 S. van Deursen
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

// This class is placed in the root namespace to allow users to start using these extension methods after
// adding the assembly reference, without find and add the correct namespace.
namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.UI;

    using SimpleInjector.Extensions;

    /// <summary>
    /// Extension methods for integrating Simple Injector with ASP.NET Web Forms applications.
    /// </summary>
    public static class SimpleInjectorWebFormExtensions
    {
        /// <summary>
        /// Registers the <see cref="Page"/> instances that are declared as public concrete types in the 
        /// supplied set of <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assemblies">The assemblies.</param>
        public static void RegisterWebFormPages(this Container container, params Assembly[] assemblies)
        {
            RegisterTypes<Page>(container, assemblies);
        }

        /// <summary>
        /// Registers the <see cref="UserControl"/> instances that are declared as public concrete types in the 
        /// supplied set of <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assemblies">The assemblies.</param>
        public static void RegisterWebFormUserControls(this Container container, params Assembly[] assemblies)
        {
            RegisterTypes<UserControl>(container, assemblies);
        }

        private static void RegisterTypes<TBase>(Container container, params Assembly[] assemblies)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (assemblies == null || assemblies.Length == 0)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }            

            var concreteTypes =
                from assembly in assemblies
                from type in GetExportedTypesFrom(assembly)
                where typeof(TBase).IsAssignableFrom(type)
                where typeof(TBase) != type
                where !type.IsAbstract
                where !type.IsGenericTypeDefinition
                select type;

            foreach (var concreteType in concreteTypes)
            {
                container.Register(concreteType);
            }
        }

        private static IEnumerable<Type> GetExportedTypesFrom(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (NotSupportedException)
            {
                // A type load exception would typically happen on an Anonymously Hosted DynamicMethods 
                // Assembly and it would be safe to skip this exception.
                return Enumerable.Empty<Type>();
            }
        }
    }
}