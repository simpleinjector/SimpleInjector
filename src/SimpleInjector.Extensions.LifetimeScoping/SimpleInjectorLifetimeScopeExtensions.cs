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
    using Lifestyles;
    using SimpleInjector.Extensions.LifetimeScoping;

    /// <summary>
    /// Extension methods for enabling lifetime scoping for the Simple Injector.
    /// </summary>
    public static partial class SimpleInjectorLifetimeScopeExtensions
    {
        private static readonly ScopedLifestyle Lifestyle = new ThreadScopedLifestyle();

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
        [Obsolete("This lifestyle is obsolete. Please use SimpleInjector.Lifestyles." + 
            "ThreadScopedLifestyle.BeginScope(Container) instead.", error: false)]
        public static Scope BeginLifetimeScope(this Container container)
        {
            return ThreadScopedLifestyle.BeginScope(container);
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
        [Obsolete("GetCurrentLifetimeScope has been deprecated. Please use " +
            "SimpleInjector.Lifestyles.ThreadScopedLifestyle.GetCurrentScope(Container) instead.",
            error: true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static Scope GetCurrentLifetimeScope(this Container container)
        {
            return Lifestyle.GetCurrentScope(container);
        }
    }
}