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
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel;
    using SimpleInjector.Advanced;
    using SimpleInjector.Integration.Wcf;

    /// <summary>
    /// Extension methods for integrating Simple Injector with WCF services.
    /// </summary>
    public static class SimpleInjectorWcfExtensions
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

        /// <summary>This method is obsolete. Do not call this method.</summary>
        /// <param name="container">The container.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "container",
            Justification = "We can't remove the 'container' parameter. That would break the API.")]
        [Obsolete("The WcfOperationLifestyle is enabled implicitly by Simple Injector. " +
            "This method has therefore become a no-op and the call to this method can be removed safely.",
            error: true)]
        [ExcludeFromCodeCoverage]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void EnablePerWcfOperationLifestyle(this Container container)
        {
            throw new InvalidOperationException(
                "The WcfOperationLifestyle is enabled implicitly by Simple Injector. " +
                "This method has therefore become a no-op and the call to this method can be removed safely.");
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TConcrete"/> will be returned during
        /// the WCF configured lifetime of the WCF service class. When the WCF service class is released by
        /// WCF and <typeparamref name="TConcrete"/> implements <see cref="IDisposable"/>,
        /// the cached instance will be disposed as well.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TConcrete"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TConcrete"/> is a type
        /// that can not be created by the container.</exception>
        public static void RegisterPerWcfOperation<TConcrete>(this Container container)
            where TConcrete : class
        {
            Requires.IsNotNull(container, nameof(container));

            container.Register<TConcrete, TConcrete>(WcfOperationLifestyle.WithDisposal);
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TImplementation"/> will be returned during
        /// the WCF configured lifetime of the WCF service class. When the WCF service class is released by
        /// WCF and <typeparamref name="TImplementation"/> implements <see cref="IDisposable"/>, the cached 
        /// instance will be disposed as well.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <typeparamref name="TImplementation"/> 
        /// type is not a type that can be created by the container.
        /// </exception>
        public static void RegisterPerWcfOperation<TService, TImplementation>(
            this Container container)
            where TImplementation : class, TService
            where TService : class
        {
            Requires.IsNotNull(container, nameof(container));

            container.Register<TService, TImplementation>(WcfOperationLifestyle.WithDisposal);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the WCF configured lifetime of the WCF service class.
        /// When the WCF service class is released by WCF and the cached instance implements 
        /// <see cref="IDisposable"/>, that cached instance will be disposed as well.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the <paramref name="container"/>, or <paramref name="instanceCreator"/> are
        /// null references.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the
        /// <typeparamref name="TService"/> has already been registered.</exception>
        public static void RegisterPerWcfOperation<TService>(this Container container,
            Func<TService> instanceCreator)
            where TService : class
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            container.Register<TService>(instanceCreator, WcfOperationLifestyle.WithDisposal);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the WCF configured lifetime of the WCF service class.
        /// When the WCF service class is released by WCF, <paramref name="disposeWhenRequestEnds"/> is set to
        /// <b>true</b>, and the cached instance implements <see cref="IDisposable"/>, that cached instance 
        /// will be disposed as well.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <param name="disposeWhenRequestEnds">If set to <c>true</c> the cached instance will be
        /// disposed at the end of its lifetime.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the <paramref name="container"/>, or <paramref name="instanceCreator"/> are
        /// null references.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the
        /// <typeparamref name="TService"/> has already been registered.</exception>
        public static void RegisterPerWcfOperation<TService>(this Container container,
            Func<TService> instanceCreator, bool disposeWhenRequestEnds)
            where TService : class
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            var lifestyle =
                disposeWhenRequestEnds ? WcfOperationLifestyle.WithDisposal : WcfOperationLifestyle.NoDisposal;

            container.Register<TService>(instanceCreator, lifestyle);
        }

        /// <summary>
        /// Gets the <see cref="Scope"/> for the current WCF request or <b>null</b> when no
        /// <see cref="Scope"/> is currently in scope.
        /// </summary>
        /// <example>
        /// The following example registers a <b>ServiceImpl</b> type as transient (a new instance will be
        /// returned every time) and registers an initializer for that type that will register that instance
        /// for disposal in the <see cref="Scope"/> in which context it is created:
        /// <code lang="cs"><![CDATA[
        /// container.Register<IService, ServiceImpl>();
        /// container.RegisterInitializer<ServiceImpl>(instance =>
        /// {
        ///     container.GetCurrentWcfOperationScope().RegisterForDisposal(instance);
        /// });
        /// ]]></code>
        /// </example>
        /// <param name="container">The container.</param>
        /// <returns>A new <see cref="Scope"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the current <paramref name="container"/>
        /// has both no <b>LifetimeScope</b> registrations.
        /// </exception>
        public static Scope GetCurrentWcfOperationScope(this Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            return WcfOperationLifestyle.GetCurrentScopeCore();
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

            return singleton ? Lifestyle.Singleton : behavior.SelectLifestyle(wcfServiceType, wcfServiceType);
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