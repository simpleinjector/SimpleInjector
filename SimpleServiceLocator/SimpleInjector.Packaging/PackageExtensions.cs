#region Copyright (c) 2013 Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using SimpleInjector.Packaging;

    /// <summary>
    /// Extension methods for working with packages.
    /// </summary>
    public static class PackageExtensions
    {
        /// <summary>
        /// Loads all <see cref="IPackage"/> implementations from assemblies that are currently loaded in the 
        /// current AppDomain, and calls their <see cref="IPackage.RegisterServices">Register</see> method. 
        /// Note that only publicly exposed classes that contain a public default constructor will be loaded.
        /// Note that this method will only pick up assemblies that are loaded at that moment in time. A
        /// more reliable way of registering packages is by explicitly supplying the list of assemblies using
        /// the <see cref="RegisterPackages(Container, IEnumerable{Assembly})"/> overload.
        /// </summary>
        /// <param name="container">The container to which the packages will be applied to.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> is a null
        /// reference.</exception>
        public static void RegisterPackages(this Container container)
        {
            var assemblies = 
                AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic);

            RegisterPackages(container, assemblies);
        }

        /// <summary>
        /// Loads all <see cref="IPackage"/> implementations from the given set of 
        /// <paramref name="assemblies"/> and calls their <see cref="IPackage.RegisterServices">Register</see> method. 
        /// Note that only publicly exposed classes that contain a public default constructor will be loaded. 
        /// </summary>
        /// <param name="container">The container to which the packages will be applied to.</param>
        /// <param name="assemblies">The assemblies that will be searched for packages.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> is a null
        /// reference.</exception>
        public static void RegisterPackages(this Container container, IEnumerable<Assembly> assemblies)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (assemblies == null)
            {
                throw new ArgumentNullException("assemblies");
            }

            var packageTypes = (
                from assembly in assemblies
                from type in GetExportedTypesFrom(assembly)
                where typeof(IPackage).IsAssignableFrom(type)
                where !type.IsAbstract
                where !type.IsGenericTypeDefinition
                select type)
                .ToArray();

            RequiresPackageTypesHaveDefaultConstructor(packageTypes);
            
            var packages = (
                from type in packageTypes
                select CreatePackage(type))
                .ToArray();

            foreach (var package in packages)
            {
                package.RegisterServices(container);
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

        private static void RequiresPackageTypesHaveDefaultConstructor(Type[] packageTypes)
        {
            var invalidPackageType =
                packageTypes.FirstOrDefault(type => type.GetConstructor(Type.EmptyTypes) == null);

            if (invalidPackageType != null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "The type {0} does not contain a default (public parameterless) constructor. Packages " +
                    "must have a default constructor.", invalidPackageType.FullName));
            }
        }

        private static IPackage CreatePackage(Type packageType)
        {
            try
            {
                return (IPackage)Activator.CreateInstance(packageType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "The creation of package type {0} failed. " + ex.Message), ex);
            }
        }
    }
}