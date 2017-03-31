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
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Web.Compilation;
    using System.Web.Mvc;
    using SimpleInjector.Advanced;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Integration.Web.Mvc;

    /// <summary>
    /// Extension methods for integrating Simple Injector with ASP.NET MVC applications.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mvc",
        Justification = "Mvc is the word")]
    public static class SimpleInjectorMvcExtensions
    {
        /// <summary>Registers a <see cref="IFilterProvider"/> that allows filter attributes to go through the
        /// Simple Injector pipeline (https://simpleinjector.org/pipel). This allows any registered property to be 
        /// injected if a custom <see cref="IPropertySelectionBehavior"/> in configured in the container, and 
        /// allows any<see cref="Container.RegisterInitializer">initializers</see> to be called on those 
        /// attributes.
        /// <b>Please note that attributes are cached by MVC, so only dependencies should be injected that
        /// have the singleton lifestyle.</b>
        /// </summary>
        /// <param name="container">The container that should be used for injecting properties into attributes
        /// that the MVC framework uses.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a MVC filter provider has already been registered for a different container.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mvc",
            Justification = "By postfixing 'Register' with 'Mvc', all MVC related methods are nicely " +
                            "grouped together.")]
        public static void RegisterMvcIntegratedFilterProvider(this Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            RequiresFilterProviderNotRegistered(container);

            var singletonFilterProvider = new SimpleInjectorFilterAttributeFilterProvider(container);

            container.RegisterSingleton<IFilterProvider>(singletonFilterProvider);

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
            foreach (Type controllerType in GetControllerTypesToRegister(container, assemblies))
            {
                Registration registration = Lifestyle.Transient.CreateRegistration(controllerType, container);

                registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent,
                    justification:
                        "MVC's DefaultControllerFactory disposes the controller when the web request ends.");

                container.AddRegistration(controllerType, registration);
            }
        }

        /// <summary>
        /// Returns all public, non abstract, non generic types that implement <see cref="IController"/> and
        /// which name end with "Controller" that are located in the supplied <paramref name="assemblies"/>.
        /// </summary>
        /// <remarks>
        /// Use this method to retrieve the list of <see cref="Controller"/> types for manual registration.
        /// In most cases, this method doesn't have to be called directly, but the 
        /// <see cref="RegisterMvcControllers"/> method can be used instead.
        /// </remarks>
        /// <param name="container">The container to use.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A list of types.</returns>
        public static Type[] GetControllerTypesToRegister(Container container, params Assembly[] assemblies)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (assemblies == null || assemblies.Length == 0)
            {
                assemblies = BuildManager.GetReferencedAssemblies().OfType<Assembly>().ToArray();
            }

            return (
                from assembly in assemblies
                where !assembly.IsDynamic
                from type in GetExportedTypes(assembly)
                where typeof(IController).IsAssignableFrom(type)
                where !type.IsAbstract
                where !type.IsGenericTypeDefinition
                where type.Name.EndsWith("Controller", StringComparison.Ordinal)
                select type)
                .ToArray();
        }

        private static void RequiresFilterProviderNotRegistered(Container container)
        {
            if (FilterProviderAlreadyRegisteredForDifferentContainer(container))
            {
                throw new InvalidOperationException(
                    "An MVC filter provider has already been registered for a different Container instance. " +
                    "Registering MVC filter providers for different containers is not supported by this method.");
            }
        }

        private static bool FilterProviderAlreadyRegisteredForDifferentContainer(Container container)
        {
            var integratedProviders =
                from provider in FilterProviders.Providers.OfType<SimpleInjectorFilterAttributeFilterProvider>()
                let differentContainer = !object.ReferenceEquals(container, provider.Container)
                where differentContainer
                select provider;

            return integratedProviders.Any();
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