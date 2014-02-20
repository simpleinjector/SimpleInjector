#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014 Simple Injector Contributors
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
    using SimpleInjector.Advanced;
    using SimpleInjector.Extensions.ExecutionContextScoping;

    /// <summary>
    /// Extension methods for enabling execution context scoping for the Simple Injector.
    /// </summary>
    public static class SimpleInjectorExecutionContextScopeExtensions
    {
        private static readonly object ManagerKey = new object();
        
        /// <summary>
        /// Registers that a single instance of <typeparamref name="TConcrete"/> will be returned for
        /// each execution context scope that has been started using 
        /// <see cref="BeginExecutionContextScope">BeginExecutionContextScope</see>. When the 
        /// execution context scope is disposed and <typeparamref name="TConcrete"/> implements <see cref="IDisposable"/>,
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
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A design without a generic T would be unpractical, because the other " +
            "overloads also take a generic T.")]
        public static void RegisterExecutionContextScope<TConcrete>(this Container container)
            where TConcrete : class
        {
            Requires.IsNotNull(container, "container");

            container.Register<TConcrete, TConcrete>(ExecutionContextScopeLifestyle.WithDisposal);
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TImplementation"/> will be returned for
        /// each execution context scope that has been started using 
        /// <see cref="BeginExecutionContextScope">BeginExecutionContextScope</see>. When the 
        /// execution context scope is disposed and <typeparamref name="TImplementation"/> implements 
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
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A design without a generic T would be unpractical, because we will lose " +
            "compile-time support.")]
        public static void RegisterExecutionContextScope<TService, TImplementation>(
            this Container container)
            where TImplementation : class, TService
            where TService : class
        {
            Requires.IsNotNull(container, "container");

            container.Register<TService, TImplementation>(ExecutionContextScopeLifestyle.WithDisposal);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the lifetime of a given scope that has been started using
        /// <see cref="BeginExecutionContextScope">BeginExecutionContextScope</see>. When the execution context scope is disposed, and 
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
        public static void RegisterExecutionContextScope<TService>(this Container container,
            Func<TService> instanceCreator)
            where TService : class
        {
            RegisterExecutionContextScope<TService>(container, instanceCreator, disposeWhenScopeEnds: true);
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TConcrete"/> will be returned for
        /// each execution context scope that has been started using 
        /// <see cref="BeginExecutionContextScope">BeginExecutionContextScope</see>. When the 
        /// execution context scope is disposed and <typeparamref name="TConcrete"/> implements <see cref="IDisposable"/>,
        /// the cached instance will be disposed as well.
        /// Scopes can be nested, and each scope gets its own instance.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="disposeWhenExecutionContextScopeEnds">If set to <c>true</c> the cached instance will be
        /// disposed at the end of its lifetime.</param>
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
        public static void RegisterExecutionContextScope<TConcrete>(this Container container,
            bool disposeWhenExecutionContextScopeEnds)
            where TConcrete : class, IDisposable
        {
            Requires.IsNotNull(container, "container");

            container.Register<TConcrete, TConcrete>(ExecutionContextScopeLifestyle.Get(disposeWhenExecutionContextScopeEnds));
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TImplementation"/> will be returned for
        /// each execution context scope that has been started using 
        /// <see cref="BeginExecutionContextScope">BeginExecutionContextScope</see>.  When the execution context scope is disposed, 
        /// <paramref name="disposeWhenScopeEnds"/> is set to <b>true</b>, and the cached instance
        /// implements <see cref="IDisposable"/>, that cached instance will be disposed as well.
        /// Scopes can be nested, and each scope gets its own instance.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="disposeWhenScopeEnds">If set to <c>true</c> the cached instance will be
        /// disposed at the end of its lifetime.</param>
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
        public static void RegisterExecutionContextScope<TService, TImplementation>(
            this Container container, bool disposeWhenScopeEnds)
            where TImplementation : class, TService, IDisposable
            where TService : class
        {
            Requires.IsNotNull(container, "container");

            container.Register<TService, TImplementation>(ExecutionContextScopeLifestyle.Get(disposeWhenScopeEnds));
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the lifetime of a given scope that has been started using
        /// <see cref="BeginExecutionContextScope">BeginExecutionContextScope</see>. When the execution context scope is disposed, 
        /// <paramref name="disposeWhenScopeEnds"/> is set to <b>true</b>, and the cached instance
        /// implements <see cref="IDisposable"/>, that cached instance will be disposed as well.
        /// Scopes can be nested, and each scope gets its own instance.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <param name="disposeWhenScopeEnds">If set to <c>true</c> the cached instance will be
        /// disposed at the end of its lifetime.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the <paramref name="container"/>, or <paramref name="instanceCreator"/> are
        /// null references.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the
        /// <typeparamref name="TService"/> has already been registered.</exception>
        public static void RegisterExecutionContextScope<TService>(this Container container,
            Func<TService> instanceCreator, bool disposeWhenScopeEnds)
            where TService : class
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(instanceCreator, "instanceCreator");

            container.Register<TService>(instanceCreator, ExecutionContextScopeLifestyle.Get(disposeWhenScopeEnds));
        }

        /// <summary>
        /// Begins a new execution context scope for the given <paramref name="container"/>. 
        /// Services, registered with 
        /// <see cref="RegisterExecutionContextScope{TService, TImplementation}(Container)">RegisterExecutionContextScope</see> or using
        /// the <see cref="ExecutionContextScopeLifestyle"/> are cached during the lifetime of that scope.
        /// The scope should be disposed explicitly when the scope ends.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>A new <see cref="Scope"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <example>
        /// <code lang="cs"><![CDATA[
        /// using (container.BeginExecutionContextScope())
        /// {
        ///     var handler container.GetInstance(rootType) as IRequestHandler;
        ///
        ///     handler.Handle(request);
        /// }
        /// ]]></code>
        /// </example>
        public static Scope BeginExecutionContextScope(this Container container)
        {
            Requires.IsNotNull(container, "container");

            return container.GetExecutionContextScopeManager().BeginExecutionContextScope();
        }

        /// <summary>
        /// Gets the Execution Context <see cref="Scope"/> that is currently in scope or <b>null</b> when no
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
        ///     container.GetCurrentExecutionContextScope().RegisterForDisposal(instance);
        /// });
        /// ]]></code>
        /// </example>
        /// <param name="container">The container.</param>
        /// <returns>A new <see cref="Scope"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        public static Scope GetCurrentExecutionContextScope(this Container container)
        {
            Requires.IsNotNull(container, "container");

            return container.GetExecutionContextScopeManager().CurrentScope;
        }

        // This method will never return null.
        internal static ExecutionContextScopeManager GetExecutionContextScopeManager(this Container container)
        {
            var manager = (ExecutionContextScopeManager)container.GetItem(ManagerKey);

            if (manager == null)
            {
                lock (ManagerKey)
                {
                    manager = (ExecutionContextScopeManager)container.GetItem(ManagerKey);

                    if (manager == null)
                    {
                        manager = new ExecutionContextScopeManager();
                        container.SetItem(ManagerKey, manager);
                    }
                }
            }

            return manager;
        }
    }
}