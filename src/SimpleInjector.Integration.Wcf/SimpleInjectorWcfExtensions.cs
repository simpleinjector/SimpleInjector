#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2016 Simple Injector Contributors
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
    using System.ServiceModel;
    using SimpleInjector.Advanced;
    using SimpleInjector.Integration.Wcf;

    /// <summary>
    /// Extension methods for integrating Simple Injector with WCF services.
    /// </summary>
    public static partial class SimpleInjectorWcfExtensions
    {
        /// <summary>
        /// Registers the WCF services instances (public classes that implement an interface that
        /// is decorated with a <see cref="ServiceContractAttribute"/>) that are 
        /// declared as public non-abstract in the supplied set of <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="container">The container the services should be registered in.</param>
        /// <param name="assemblies">The assemblies to search.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> is 
        /// a null reference (Nothing in VB).</exception>
        public static void RegisterWcfServices(this Container container, params Assembly[] assemblies)
        {
            Requires.IsNotNull(container, nameof(container));

            if (assemblies == null || assemblies.Length == 0)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            var serviceTypes = (
                from assembly in assemblies
                where !assembly.IsDynamic
                from type in GetExportedTypes(assembly)
                where !type.IsAbstract
                where !type.IsGenericTypeDefinition
                where IsWcfServiceType(type)
                select type)
                .ToArray();

            VerifyConcurrencyMode(serviceTypes);

            foreach (Type serviceType in serviceTypes)
            {
                Lifestyle lifestyle = 
                    GetAppropriateLifestyle(serviceType, container.Options.LifestyleSelectionBehavior);

                container.Register(serviceType, serviceType, lifestyle);
            }
        }

        internal static ServiceBehaviorAttribute GetServiceBehaviorAttribute(this Type type) => 
            type.GetCustomAttributes(typeof(ServiceBehaviorAttribute), true)
                .OfType<ServiceBehaviorAttribute>()
                .FirstOrDefault();

        private static bool IsWcfServiceType(Type type)
        {
            bool typeIsDecorated = type.GetCustomAttributes(typeof(ServiceContractAttribute), true).Any();

            bool typesInterfacesAreDecorated = (
                from @interface in type.GetInterfaces()
                where @interface.IsPublic
                where @interface.GetCustomAttributes(typeof(ServiceContractAttribute), true).Any()
                select @interface)
                .Any();

            return typeIsDecorated || typesInterfacesAreDecorated;
        }

        private static void VerifyConcurrencyMode(Type[] serviceTypes)
        {
            foreach (Type serviceType in serviceTypes)
            {
                VerifyConcurrencyMode(serviceType);
            }
        }

        private static void VerifyConcurrencyMode(Type wcfServiceType)
        {
            if (HasInvalidConcurrencyMode(wcfServiceType))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "The WCF service class {0} is configured with ConcurrencyMode Multiple, but this is not " +
                    "supported by Simple Injector. Please change the ConcurrencyMode to Single.",
                    wcfServiceType.FullName));
            }
        }

        private static bool HasInvalidConcurrencyMode(Type wcfServiceType)
        {
            var attribute = GetServiceBehaviorAttribute(wcfServiceType);

            return attribute != null && attribute.ConcurrencyMode == ConcurrencyMode.Multiple;
        }

        private static Lifestyle GetAppropriateLifestyle(Type wcfServiceType, 
            ILifestyleSelectionBehavior behavior)
        {
            var attribute = GetServiceBehaviorAttribute(wcfServiceType);

            bool singleton = attribute?.InstanceContextMode == InstanceContextMode.Single;

            return singleton ? Lifestyle.Singleton : behavior.SelectLifestyle(wcfServiceType);
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