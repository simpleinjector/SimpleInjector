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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel;

    using SimpleInjector.Integration.Wcf;

    /// <summary>
    /// Extension methods for integrating Simple Injector with WCF services.
    /// </summary>
    public static class SimpleInjectorWcfExtensions
    {
        private const string WcfScopingIsNotEnabledExceptionMessage =
            "To enable WCF request scoping, please make sure the SimpleInjectorWcfExtensions." +
            "EnablePerWcfOperationScoping(Container) extension method is " +
            "called during the configuration of the container.";

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
            Requires.IsNotNull(container, "container");

            if (assemblies == null || assemblies.Length == 0)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
            
            var serviceTypes =
                from assembly in assemblies
                where !assembly.IsDynamic
                from type in assembly.GetExportedTypes()
                where !type.IsAbstract
                where !type.IsGenericTypeDefinition
                where IsWcfServiceType(type)
                select type;

            foreach (Type serviceType in serviceTypes)
            {
                container.Register(serviceType, serviceType, Lifestyle.Transient);
            }
        }

        /// <summary>
        /// Allows the container to resolve instances with a Per Wcf Operation lifestyle for the given 
        /// <paramref name="container"/>. This is 
        /// enabled automatically when services get registered using one of the <b>RegisterPerWcfRequest</b>
        /// overloads or when the container passed onto the 
        /// <see cref="SimpleInjectorServiceHostFactory.SetContainer"/> method. 
        /// </summary>
        /// <param name="container">The container.</param>
        public static void EnablePerWcfOperationLifestyle(this Container container)
        {
            Requires.IsNotNull(container, "container");

            bool oldBehavior = container.Options.AllowOverridingRegistrations;

            try
            {
                // Ensure a registered manager doesn't get overrided by disallowing overrides.
                container.Options.AllowOverridingRegistrations = false;

                container.RegisterSingle<WcfOperationScopeManager>(new WcfOperationScopeManager(null));
            }
            catch (InvalidOperationException ex)
            {
                // Suppress the failure when WcfOperationScopeManager has already been registered. This is a bit
                // nasty, but probably the only way to do this.
                if (!ex.Message.Contains("already been registered"))
                {
                    throw;
                }
            }
            finally
            {
                container.Options.AllowOverridingRegistrations = oldBehavior;
            }
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TConcrete"/> will be returned during
        /// the execution of a single Operation Contract. When the 
        /// operation ends and <typeparamref name="TConcrete"/> implements <see cref="IDisposable"/>,
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
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A design without a generic T would be unpractical, because the other " +
            "overloads also take a generic T.")]
        public static void RegisterPerWcfOperation<TConcrete>(this Container container)
            where TConcrete : class
        {
            Requires.IsNotNull(container, "container");

            container.Register<TConcrete, TConcrete>(WcfOperationLifestyle.WithDisposal);
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TImplementation"/> will be returned during
        /// the execution of a single Operation Contract. When the 
        /// operation ends and <typeparamref name="TImplementation"/> implements 
        /// <see cref="IDisposable"/>, the cached instance will be disposed as well.
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
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A design without a generic T would be unpractical, because we will lose " +
            "compile-time support.")]
        public static void RegisterPerWcfOperation<TService, TImplementation>(
            this Container container)
            where TImplementation : class, TService
            where TService : class
        {
            Requires.IsNotNull(container, "container");

            container.Register<TService, TImplementation>(WcfOperationLifestyle.WithDisposal);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the execution of a single Operation Contract.
        /// When the operation ends, and the cached instance
        /// implements <see cref="IDisposable"/>, that cached instance will be disposed as well.
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
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(instanceCreator, "instanceCreator");

            container.Register<TService>(instanceCreator, WcfOperationLifestyle.WithDisposal);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the execution of a Operation Contract.
        /// When the operation ends, 
        /// <paramref name="disposeWhenRequestEnds"/> is set to <b>true</b>, and the cached instance
        /// implements <see cref="IDisposable"/>, that cached instance will be disposed as well.
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
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(instanceCreator, "instanceCreator");

            var lifestyle = 
                disposeWhenRequestEnds ? WcfOperationLifestyle.WithDisposal : WcfOperationLifestyle.NoDisposal;

            container.Register<TService>(instanceCreator, lifestyle);
        }

        /// <summary>
        /// Gets the <see cref="WcfOperationScope"/> for the current WCF request or <b>null</b> when no
        /// <see cref="WcfOperationScope"/> is currently in scope.
        /// </summary>
        /// <example>
        /// The following example registers a <b>ServiceImpl</b> type as transient (a new instance will be
        /// returned every time) and registers an initializer for that type that will register that instance
        /// for disposal in the <see cref="WcfOperationScope"/> in which context it is created:
        /// <code lang="cs"><![CDATA[
        /// container.Register<IService, ServiceImpl>();
        /// container.RegisterInitializer<ServiceImpl>(instance =>
        /// {
        ///     container.GetCurrentWcfOperationScope().RegisterForDisposal(instance);
        /// });
        /// ]]></code>
        /// </example>
        /// <param name="container">The container.</param>
        /// <returns>A new <see cref="WcfOperationScope"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the current <paramref name="container"/>
        /// has both no <b>LifetimeScope</b> registrations <i>and</i> <see cref="EnablePerWcfOperationLifestyle"/> is
        /// not called. Lifetime scoping must be enabled by calling <see cref="EnablePerWcfOperationLifestyle"/> or
        /// by registering a service using one of the 
        /// <see cref="RegisterPerWcfOperation{TService, TImplementation}(Container)">RegisterPerWcfRequest</see>
        /// overloads.
        /// </exception>
        public static WcfOperationScope GetCurrentWcfOperationScope(this Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            IServiceProvider provider = container;

            var manager = provider.GetService(typeof(WcfOperationScopeManager)) as WcfOperationScopeManager;

            if (manager != null)
            {
                // CurrentScope can be null, when there is currently no scope.
                return manager.CurrentScope;
            }

            // When no WcfRequestScopeManager is registered, we explicitly throw an exception. See the comments
            // in the BeginWcfRequestScope method for more information.
            throw new InvalidOperationException(WcfScopingIsNotEnabledExceptionMessage);
        }

        internal static WcfOperationScope BeginWcfOperationScope(this Container container)
        {
            Requires.IsNotNull(container, "container");

            IServiceProvider provider = container;

            var manager = provider.GetService(typeof(WcfOperationScopeManager)) as WcfOperationScopeManager;

            if (manager != null)
            {
                return manager.BeginScope();
            }

            // When no LifetimeScopeManager is registered, this means that there are no lifetime scope
            // registrations (since the first call to RegisterLifetimeScope also registers the singleton
            // manager). However, since the user has called BeginLifetimeScope, he/she expects to be able to
            // use it, for instance to allow disposing instances with a different/shorter lifetime than 
            // Lifetime Scope (using the LifetimeScope.RegisterForDisposal method). For this to work however,
            // we need a LifetimeScopeManager, but at this point it is impossible to register it, since
            // BeginLifetimeScope will be called after the initialization phase. We have no other option than
            // to inform the user about enabling lifetime scoping explicitly by throwing an exception. You
            // might see this as a design flaw, but since this feature is implemented on top of the core 
            // library (instead of being written inside of the core library), there is no other option.
            throw new InvalidOperationException(WcfScopingIsNotEnabledExceptionMessage);
        }

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
    }
}