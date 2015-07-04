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

// This class is placed in the root namespace to allow users to start using these extension methods after
// adding the assembly reference, without find and add the correct namespace.
namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.UI;
    using SimpleInjector.Integration.Web.Forms;

    /// <summary>
    /// Extension methods for integrating Simple Injector with ASP.NET Web Forms applications.
    /// </summary>
    public static class SimpleInjectorWebFormExtensions
    {
        /// <summary>
        /// Registers the <see cref="Page"/> instances that are declared as public concrete types in
        /// the set of assemblies that can be found in the application's bin folder. The types will be
        /// registered as concrete transient.
        /// </summary>
        /// <param name="container">The container.</param>
        public static void RegisterPages(this Container container)
        {
            RegisterPages(container, GetAssemblies());
        }

        /// <summary>
        /// Registers the <see cref="Page"/> instances that are declared as public concrete types in the 
        /// supplied set of <paramref name="assemblies"/>. The types will be registered as concrete transient.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assemblies">The assemblies.</param>
        public static void RegisterPages(this Container container, params Assembly[] assemblies)
        {
            RegisterPages(container, (IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Registers the <see cref="Page"/> instances that are declared as public concrete types in the 
        /// supplied set of <paramref name="assemblies"/>. The types will be registered as concrete transient.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assemblies">The assemblies.</param>
        public static void RegisterPages(this Container container, IEnumerable<Assembly> assemblies)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(assemblies, "assemblies");

            var pageTypes = GetConcreteTypesThatDeriveFrom<Page>(assemblies);

            container.RegisterBatchAsConcrete(pageTypes);
        }

        /// <summary>
        /// Registers the <see cref="UserControl"/> instances that are declared as public concrete 
        /// types in the set of assemblies that can be found in the application's bin folder.
        /// The types will be registered as concrete transient.
        /// </summary>
        /// <param name="container">The container.</param>
        public static void RegisterUserControls(this Container container)
        {
            RegisterUserControls(container, GetAssemblies());
        }

        /// <summary>
        /// Registers the <see cref="UserControl"/> instances that are declared as public concrete 
        /// types in the supplied set of <paramref name="assemblies"/>.
        /// The types will be registered as concrete transient.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assemblies">The assemblies.</param>
        public static void RegisterUserControls(this Container container, params Assembly[] assemblies)
        {
            RegisterUserControls(container, (IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Registers the <see cref="UserControl"/> instances that are declared as public concrete transient 
        /// types in the supplied set of <paramref name="assemblies"/>.
        /// The types will be registered as concrete transient.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assemblies">The assemblies.</param>
        public static void RegisterUserControls(this Container container, IEnumerable<Assembly> assemblies)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(assemblies, "assemblies");

            var userControlTypes = GetConcreteTypesThatDeriveFrom<UserControl>(assemblies);

            container.RegisterBatchAsConcrete(userControlTypes);
        }

        /// <summary>
        /// Registers all concrete <see cref="IHttpHandler"/> implementations (except <see cref="Page"/> and
        /// <see cref="HttpApplication"/> implementations) that are declared as public concrete  
        /// types in the set of assemblies that can be found in the application's bin folder.
        /// The types will be registered as concrete transient.
        /// </summary>
        /// <param name="container">The container.</param>
        public static void RegisterHttpHandlers(this Container container)
        {
            RegisterHttpHandlers(container, GetAssemblies());
        }

        /// <summary>
        /// Registers all concrete <see cref="IHttpHandler"/> implementations (except <see cref="Page"/> and
        /// <see cref="HttpApplication"/> implementations) that are declared as public concrete  
        /// types in the supplied set of <paramref name="assemblies"/>.
        /// The types will be registered as concrete transient.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assemblies">The assemblies.</param>
        public static void RegisterHttpHandlers(this Container container, params Assembly[] assemblies)
        {
            RegisterHttpHandlers(container, (IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Registers all concrete <see cref="IHttpHandler"/> implementations (except <see cref="Page"/> and
        /// <see cref="HttpApplication"/> implementations) that are declared as public concrete  
        /// types in the supplied set of <paramref name="assemblies"/>.
        /// The types will be registered as concrete transient.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assemblies">The assemblies.</param>
        public static void RegisterHttpHandlers(this Container container, IEnumerable<Assembly> assemblies)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(assemblies, "assemblies");

            var handlerTypes =
                from type in GetConcreteTypesThatDeriveFrom<IHttpHandler>(assemblies)
                where !typeof(Page).IsAssignableFrom(type)
                where !typeof(HttpApplication).IsAssignableFrom(type)
                select type;

            container.RegisterBatchAsConcrete(handlerTypes);
        }
    
        private static IEnumerable<Assembly> GetAssemblies()
        {
            return
                from assembly in BuildManager.GetReferencedAssemblies().Cast<Assembly>()
                where !assembly.IsDynamic
                where !assembly.GlobalAssemblyCache
                select assembly;
        }

        private static IEnumerable<Type> GetConcreteTypesThatDeriveFrom<T>(IEnumerable<Assembly> assemblies)
        {
            return
                from assembly in assemblies
                where !assembly.IsDynamic
                from type in GetExportedTypes(assembly)
                where typeof(T).IsAssignableFrom(type) && typeof(T) != type
                where !type.IsAbstract
                where !type.IsGenericTypeDefinition
                select type;
        }

        private static void RegisterBatchAsConcrete(this Container container, IEnumerable<Type> types)
        {
            foreach (Type concreteType in types)
            {
                container.Register(concreteType);
            }
        }

        private static IEnumerable<Type> GetExportedTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (NotSupportedException)
            {
                // A type load exception would typically happen on an Anonymously Hosted DynamicMethods 
                // Assembly and it would be safe to skip this exception.
                return Type.EmptyTypes;
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Return the types that could be loaded. Types can contain null values.
                return ex.Types.Where(type => type != null);
            }
            catch (Exception ex)
            {
                // Throw a more descriptive message containing the name of the assembly.
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "Unable to load types from assembly {0}. {1}", assembly.FullName, ex.Message), ex);
            }
        }
    }
}