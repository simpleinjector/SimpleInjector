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
    using System.Linq.Expressions;
    using SimpleInjector.Extensions.LifetimeScoping;

    /// <summary>
    /// Extension methods for enabling lifetime scoping for the Simple Injector.
    /// </summary>
    public static class SimpleInjectorLifetimeScopeExtensions
    {
        private const string LifetimeScopingIsNotEnabledExceptionMessage =
            "To enable lifetime scoping, please make sure the EnableLifetimeScoping extension method is " +
            "called during the configuration of the container.";

        /// <summary>
        /// Begins a new lifetime scope for the given <paramref name="container"/>. 
        /// Services, registered with <b>RegisterLifetimeScope</b>, that are requested within the same thread
        /// as where the lifetime scope is created, are cached during the lifetime of that scope.
        /// The scope should be disposed explicitly when the scope ends.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>A new <see cref="LifetimeScope"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="RegisterLifetimeScopeManager"/> has
        /// not been called previously.</exception>
        public static LifetimeScope BeginLifetimeScope(this Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            IServiceProvider provider = container;

            var manager = provider.GetService(typeof(LifetimeScopeManager)) as LifetimeScopeManager;

            if (manager != null)
            {
                return manager.BeginLifetimeScope();
            }

            // When no LifetimeScopeManager is registered, this means that there are no lifetime scope
            // registrations (since the first call to RegisterLifetimeScope also registers the singleton
            // manager) and we can return a dummy scope.
            return new LifetimeScope(null);
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TConcrete"/> will be returned for
        /// each lifetime scope that has been started using <see cref="BeginLifetimeScope"/>. When the 
        /// lifetime scope is disposed and <typeparamref name="TConcrete"/> implements <see cref="IDisposable"/>,
        /// the cached instance will be disposed as well.
        /// Scopes can be nested, and each scope gets its own instance. Instances that are requested outside 
        /// the context of a scope will have the lifetime of the container (singleton).
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
        public static void RegisterLifetimeScope<TConcrete>(this Container container)
            where TConcrete : class
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            // Dummy registration. This registration will be replaced later. By explicitly registering the
            // instance we use allow the container to verify whether TConcrete can be created and if
            // TConcrete has already been registered or not. Also, the ExpressionBuilt event will only get
            // raised for types that are registered, and because the type is registered, we can cleverly make
            // use of the Expression that has already been created by the container for this registration,
            // saving us from having to build such registration ourselves.
            container.Register<TConcrete>();

            container.RegisterLifetimeScopeManager();

            ReplaceRegistrationAsLifetimeScope<TConcrete>(container);
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TImplementation"/> will be returned for
        /// each lifetime scope that has been started using <see cref="BeginLifetimeScope"/>. When the 
        /// lifetime scope is disposed and <typeparamref name="TImplementation"/> implements 
        /// <see cref="IDisposable"/>, the cached instance will be disposed as well.
        /// Scopes can be nested, and each scope gets its own instance. Instances that are requested outside 
        /// the context of a scope will have the lifetime of the container (singleton).
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
        public static void RegisterLifetimeScope<TService, TImplementation>(
            this Container container)
            where TImplementation : class, TService
            where TService : class
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            container.Register<TService, TImplementation>();

            container.RegisterLifetimeScopeManager();

            ReplaceRegistrationAsLifetimeScope<TService>(container);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the lifetime of a given scope that has been started using
        /// <see cref="BeginLifetimeScope"/>. When the lifetime scope is disposed, and the cached instance
        /// implements <see cref="IDisposable"/>, that cached instance will be disposed as well.
        /// Scopes can be nested, and each scope gets its own instance. Instances that are requested outside
        /// the context of a scope will have the lifetime of the container (singleton).
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
        public static void RegisterLifetimeScope<TService>(this Container container,
            Func<TService> instanceCreator)
            where TService : class
        {
            RegisterLifetimeScope<TService>(container, instanceCreator, disposeWhenLifetimeScopeEnds: true);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the lifetime of a given scope that has been started using
        /// <see cref="BeginLifetimeScope"/>. When the lifetime scope is disposed, 
        /// <paramref name="disposeWhenLifetimeScopeEnds"/> is set to <b>true</b>, and the cached instance
        /// implements <see cref="IDisposable"/>, that cached instance will be disposed as well.
        /// Scopes can be nested, and each scope gets its own instance. Instances that are requested outside
        /// the context of a scope will have the lifetime of the container (singleton).
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
        public static void RegisterLifetimeScope<TService>(this Container container,
            Func<TService> instanceCreator, bool disposeWhenLifetimeScopeEnds)
            where TService : class
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            container.Register<TService>(instanceCreator);

            container.RegisterLifetimeScopeManager();

            ReplaceRegistrationAsLifetimeScope<TService>(container, disposeWhenLifetimeScopeEnds);
        }

        private static void ReplaceRegistrationAsLifetimeScope<TService>(Container container,
            bool disposeWhenLifetimeScopeEnds = true)
            where TService : class
        {
            var helper = new ReplaceRegistrationAsLifetimeScopeHelper<TService>(container);

            helper.DisposeWhenLifetimeScopeEnds = disposeWhenLifetimeScopeEnds;

            container.ExpressionBuilt += helper.ExpressionBuilt;
        }

        private static void RegisterLifetimeScopeManager(this Container container)
        {
            try
            {
                container.RegisterSingle<LifetimeScopeManager>(new LifetimeScopeManager());
            }
            catch (InvalidOperationException)
            {
                // Suppress the failure when LifetimeScopeManager has already been registered. This is a bit
                // nasty, but probably the only way to do this.
            }
        }

        // This class is thread-safe within the context of a single Container.
        private sealed class ReplaceRegistrationAsLifetimeScopeHelper<TService> where TService : class
        {
            private readonly Container container;
            private TService containerScopedServiceInstance;
            private LifetimeScopeManager manager;
            private Func<TService> instanceCreator;

            internal ReplaceRegistrationAsLifetimeScopeHelper(Container container)
            {
                this.container = container;
            }

            internal bool DisposeWhenLifetimeScopeEnds { get; set; }

            internal void ExpressionBuilt(object sender, ExpressionBuiltEventArgs e)
            {
                if (e.RegisteredServiceType == typeof(TService))
                {
                    this.manager = this.container.GetInstance<LifetimeScopeManager>();
                    this.instanceCreator = Expression.Lambda<Func<TService>>(e.Expression).Compile();

                    Func<TService> scopedInstanceCreator = this.CreateScopedInstance;

                    // Replace the original expression with an invocation of the Func<TService>
                    e.Expression = Expression.Invoke(Expression.Constant(scopedInstanceCreator));
                }
            }

            private TService CreateScopedInstance()
            {
                var scope = this.manager.CurrentScope;

                if (scope != null)
                {
                    var instance = scope.GetInstance(this.instanceCreator);

                    if (this.DisposeWhenLifetimeScopeEnds)
                    {
                        var disposable = instance as IDisposable;

                        if (disposable != null)
                        {
                            scope.RegisterForDisposal(disposable);
                        }
                    }

                    return instance;
                }

                // Return a singleton when there is no scope.
                return this.GetSingleton();
            }

            private TService GetSingleton()
            {
                if (this.containerScopedServiceInstance == null)
                {
                    lock (this)
                    {
                        if (this.containerScopedServiceInstance == null)
                        {
                            this.containerScopedServiceInstance = this.instanceCreator();
                        }
                    }
                }

                return this.containerScopedServiceInstance;
            }
        }
    }
}