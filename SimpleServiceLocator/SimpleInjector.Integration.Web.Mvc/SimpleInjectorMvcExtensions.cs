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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Web.Compilation;
    using System.Web.Mvc;

    using SimpleInjector.Integration.Web.Mvc;

    /// <summary>
    /// Extension methods for integrating Simple Injector with ASP.NET MVC applications.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mvc",
        Justification = "Mvc is the word")]
    public static class SimpleInjectorMvcExtensions
    {
        /// <summary>Registers a <see cref="IFilterProvider"/> that allows implicit property injection into
        /// the filter attributes that are returned from the provider.</summary>
        /// <param name="container">The container that should be used for injecting properties into attributes
        /// that the MVC framework uses.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mvc",
            Justification = "By postfixing 'Register' with 'Mvc', all MVC related methods are nicely " +
                            "grouped together.")]
        public static void RegisterMvcAttributeFilterProvider(this Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            var singletonFilterProvider = new SimpleInjectorFilterAttributeFilterProvider(container);

            container.RegisterSingle<IFilterProvider>(singletonFilterProvider);

            var providers = FilterProviders.Providers.OfType<FilterAttributeFilterProvider>().ToList();

            providers.ForEach(provider => FilterProviders.Providers.Remove(provider));

            FilterProviders.Providers.Add(singletonFilterProvider);
        }

        /// <summary>
        /// Registers the MVC <see cref="IController"/> instances (which name end with 'Controller') that are 
        /// declared as public non-abstract in the supplied set of <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="container">The container the controllers should be registered in.</param>
        /// <param name="assemblies">The assemblies to search.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> is a null 
        /// reference (Nothing in VB).</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mvc",
            Justification = "By postfixing 'Register' with 'Mvc', all MVC related methods are nicely " +
                            "grouped together.")]
        public static void RegisterMvcControllers(this Container container, params Assembly[] assemblies)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (assemblies == null || assemblies.Length == 0)
            {
                assemblies = BuildManager.GetReferencedAssemblies().OfType<Assembly>().ToArray();
            }

            var controllerTypes =
                from assembly in assemblies
                where !assembly.IsDynamic
                from type in assembly.GetExportedTypes()
                where typeof(IController).IsAssignableFrom(type)
                where !type.IsAbstract
                where !type.IsGenericTypeDefinition
                where type.Name.EndsWith("Controller", StringComparison.Ordinal)
                select type;

            foreach (var controllerType in controllerTypes)
            {
                container.Register(controllerType, controllerType, Lifestyle.Transient);
            }
        }
    }
}