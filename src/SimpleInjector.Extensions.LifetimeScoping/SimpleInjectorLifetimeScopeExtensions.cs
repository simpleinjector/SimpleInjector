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
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using SimpleInjector.Advanced;
    using SimpleInjector.Extensions.LifetimeScoping;
    
    /// <summary>
    /// Extension methods for enabling lifetime scoping for the Simple Injector.
    /// </summary>
    public static class SimpleInjectorLifetimeScopeExtensions
    {
        private static readonly object ManagerKey = new object();

        /// <summary>This method is obsolete.</summary>
        /// <param name="container">The container.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the container is locked.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "container",
            Justification = "We can't remove the 'container' parameter. That would break the API.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("LifetimeScoping is automatically enabled and there's no need to call this method anymore. " +
            "Remove the call to this method.",
            error: true)]
        [ExcludeFromCodeCoverage]
        public static void EnableLifetimeScoping(this Container container)
        {
            throw new InvalidOperationException(
                "LifetimeScoping is automatically enabled and there's no need to call this method anymore. " +
                "Remove the call to this method.");
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TConcrete"/> will be returned for
        /// each lifetime scope that has been started using 
        /// <see cref="BeginLifetimeScope">BeginLifetimeScope</see>. When the 
        /// lifetime scope is disposed and <typeparamref name="TConcrete"/> implements <see cref="IDisposable"/>,
        /// the cached instance will be disposed as well.
        /// Scopes can be nested, and each scope gets its own instance.
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
        [Obsolete("RegisterLifetimeScope has been deprecated and will be removed in a future release. " +
            "Please use Register<TConcrete>(Lifestyle.Scoped) instead. " +
            "See: https://simpleinjector.org/lifetimes#perlifetimescope",
            error: false)]
        public static void RegisterLifetimeScope<TConcrete>(this Container container)
            where TConcrete : class
        {
            Requires.IsNotNull(container, nameof(container));

            container.Register<TConcrete, TConcrete>(LifetimeScopeLifestyle.WithDisposal);            
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TImplementation"/> will be returned for
        /// each lifetime scope that has been started using 
        /// <see cref="BeginLifetimeScope">BeginLifetimeScope</see>. When the 
        /// lifetime scope is disposed and <typeparamref name="TImplementation"/> implements 
        /// <see cref="IDisposable"/>, the cached instance will be disposed as well.
        /// Scopes can be nested, and each scope gets its own instance.
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
        [Obsolete("RegisterLifetimeScope has been deprecated and will be removed in a future release. " +
            "Please use Register<TService, TImplementation>(Lifestyle.Scoped) instead. " +
            "See: https://simpleinjector.org/lifetimes#perlifetimescope",
            error: false)]
        public static void RegisterLifetimeScope<TService, TImplementation>(
            this Container container)
            where TImplementation : class, TService
            where TService : class
        {
            Requires.IsNotNull(container, nameof(container));

            container.Register<TService, TImplementation>(LifetimeScopeLifestyle.WithDisposal);            
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the lifetime of a given scope that has been started using
        /// <see cref="BeginLifetimeScope">BeginLifetimeScope</see>. When the lifetime scope is disposed, and 
        /// the cached instance implements <see cref="IDisposable"/>, that cached instance will be disposed as
        /// well. Scopes can be nested, and each scope gets its own instance.
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
        [Obsolete("RegisterLifetimeScope has been deprecated and will be removed in a future release. " +
            "Please use Register<TService>(Func<TService>, Lifestyle.Scoped) instead. " +
            "See: https://simpleinjector.org/lifetimes#perlifetimescope",
            error: false)]
        public static void RegisterLifetimeScope<TService>(this Container container,
            Func<TService> instanceCreator)
            where TService : class
        {
            RegisterLifetimeScope<TService>(container, instanceCreator, disposeWhenLifetimeScopeEnds: true);
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TConcrete"/> will be returned for
        /// each lifetime scope that has been started using 
        /// <see cref="BeginLifetimeScope">BeginLifetimeScope</see>. When the 
        /// lifetime scope is disposed and <typeparamref name="TConcrete"/> implements <see cref="IDisposable"/>,
        /// the cached instance will be disposed as well.
        /// Scopes can be nested, and each scope gets its own instance.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="disposeWhenLifetimeScopeEnds">If set to <c>true</c> the cached instance will be
        /// disposed at the end of its lifetime.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TConcrete"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TConcrete"/> is a type
        /// that can not be created by the container.</exception>
        [Obsolete("RegisterLifetimeScope has been deprecated and will be removed in a future release. " +
            "Please use Register<TService>(Func<TService>, new LifetimeScopeLifestyle(false)) instead " +
            "to suppress disposal.",
            error: false)]
        public static void RegisterLifetimeScope<TConcrete>(this Container container, 
            bool disposeWhenLifetimeScopeEnds)
            where TConcrete : class, IDisposable
        {
            Requires.IsNotNull(container, nameof(container));

            container.Register<TConcrete, TConcrete>(LifetimeScopeLifestyle.Get(disposeWhenLifetimeScopeEnds));
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TImplementation"/> will be returned for
        /// each lifetime scope that has been started using 
        /// <see cref="BeginLifetimeScope">BeginLifetimeScope</see>.  When the lifetime scope is disposed, 
        /// <paramref name="disposeWhenLifetimeScopeEnds"/> is set to <b>true</b>, and the cached instance
        /// implements <see cref="IDisposable"/>, that cached instance will be disposed as well.
        /// Scopes can be nested, and each scope gets its own instance.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="disposeWhenLifetimeScopeEnds">If set to <c>true</c> the cached instance will be
        /// disposed at the end of its lifetime.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <typeparamref name="TImplementation"/> 
        /// type is not a type that can be created by the container.
        /// </exception>
        [Obsolete("RegisterLifetimeScope has been deprecated and will be removed in a future release. " +
            "Please use Register<TService, TImplementation>(new LifetimeScopeLifestyle(false)) instead " +
            "to suppress disposal.",
            error: false)]
        public static void RegisterLifetimeScope<TService, TImplementation>(
            this Container container, bool disposeWhenLifetimeScopeEnds)
            where TImplementation : class, TService, IDisposable
            where TService : class
        {
            Requires.IsNotNull(container, nameof(container));

            container.Register<TService, TImplementation>(LifetimeScopeLifestyle.Get(disposeWhenLifetimeScopeEnds));            
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the lifetime of a given scope that has been started using
        /// <see cref="BeginLifetimeScope">BeginLifetimeScope</see>. When the lifetime scope is disposed, 
        /// <paramref name="disposeWhenLifetimeScopeEnds"/> is set to <b>true</b>, and the cached instance
        /// implements <see cref="IDisposable"/>, that cached instance will be disposed as well.
        /// Scopes can be nested, and each scope gets its own instance.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <param name="disposeWhenLifetimeScopeEnds">If set to <c>true</c> the cached instance will be
        /// disposed at the end of its lifetime.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the <paramref name="container"/>, or <paramref name="instanceCreator"/> are
        /// null references.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the
        /// <typeparamref name="TService"/> has already been registered.</exception>
        [Obsolete("RegisterLifetimeScope has been deprecated and will be removed in a future release. " +
            "Please use Register<TService>(Func<TService>, new LifetimeScopeLifestyle(false)) instead " +
            "to suppress disposal.",
            error: false)]
        public static void RegisterLifetimeScope<TService>(this Container container,
            Func<TService> instanceCreator, bool disposeWhenLifetimeScopeEnds)
            where TService : class
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            container.Register<TService>(instanceCreator, LifetimeScopeLifestyle.Get(disposeWhenLifetimeScopeEnds));            
        }
        
        /// <summary>
        /// Begins a new lifetime scope for the given <paramref name="container"/> on the current thread. 
        /// Services, registered with 
        /// <see cref="RegisterLifetimeScope{TService, TImplementation}(Container)">RegisterLifetimeScope</see> or
        /// using the <see cref="LifetimeScopeLifestyle"/> and are requested within the same thread as where the 
        /// lifetime scope is created, are cached during the lifetime of that scope.
        /// The scope should be disposed explicitly when the scope ends.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>A new <see cref="Scope"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="EnableLifetimeScoping"/> has
        /// not been called previously.</exception>
        /// <example>
        /// <code lang="cs"><![CDATA[
        /// using (container.BeginLifetimeScope())
        /// {
        ///     var handler container.GetInstance(rootType) as IRequestHandler;
        ///
        ///     handler.Handle(request);
        /// }
        /// ]]></code>
        /// </example>
        public static Scope BeginLifetimeScope(this Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            return container.GetLifetimeScopeManager().BeginLifetimeScope();
        }

        /// <summary>
        /// Gets the <see cref="Scope"/> that is currently in scope or <b>null</b> when no
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
        ///     container.GetCurrentLifetimeScope().RegisterForDisposal(instance);
        /// });
        /// ]]></code>
        /// </example>
        /// <param name="container">The container.</param>
        /// <returns>A new <see cref="Scope"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        [Obsolete("GetCurrentLifetimeScope has been deprecated and will be removed in a future release. " +
            "Please use Lifestyle.Scoped.GetCurrentScope(Container) instead.",
            error: false)]
        public static Scope GetCurrentLifetimeScope(this Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            return container.GetLifetimeScopeManager().CurrentScope;
        }

        // This method will never return null.
        internal static LifetimeScopeManager GetLifetimeScopeManager(this Container container) => 
            container.GetOrSetItem(ManagerKey, (c, key) => new LifetimeScopeManager(c));
    }
}