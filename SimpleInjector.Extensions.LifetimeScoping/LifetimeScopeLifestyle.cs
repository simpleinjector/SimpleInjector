#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2014 Simple Injector Contributors
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

namespace SimpleInjector.Extensions.LifetimeScoping
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Defines a lifestyle that caches instances during the lifetime of an explicitly defined scope using the
    /// <see cref="SimpleInjectorLifetimeScopeExtensions.BeginLifetimeScope(Container)">BeginLifetimeScope</see>
    /// method. A scope is thread-specific, each thread should define its own scope. Scopes can be nested and
    /// nested scopes will get their own instance. Instances created by this lifestyle can be disposed when 
    /// the created scope gets <see cref="LifetimeScope.Dispose">disposed</see>. 
    /// </summary>
    /// <example>
    /// The following example shows the usage of the <b>LifetimeScopeLifestyle</b> class:
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    /// 
    /// container.Register<IUnitOfWork, EntityFrameworkUnitOfWork>(new LifetimeScopeLifestyle());
    /// 
    /// using (container.BeginLifetimeScope())
    /// {
    ///     var instance1 = container.GetInstance<IUnitOfWork>();
    ///     
    ///     // This call will return the same instance.
    ///     var instance2 = container.GetInstance<IUnitOfWork>();
    ///     
    ///     Assert.IsTrue(object.ReferenceEquals(instance1, instance2));
    ///     
    ///     // Create a nested scope.
    ///     using (container.BeginLifetimeScope())
    ///     {
    ///         // A nested scope gets its own instance.
    ///         var instance3 = container.GetInstance<IUnitOfWork>();
    /// 
    ///         Assert.IsFalse(object.ReferenceEquals(instance1, instance3));
    ///     
    ///         // This call will return the same instance.
    ///         var instance4 = container.GetInstance<IUnitOfWork>();
    ///         
    ///         Assert.IsTrue(object.ReferenceEquals(instance3, instance4));
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    public sealed class LifetimeScopeLifestyle : ScopedLifestyle
    {
        internal static readonly LifetimeScopeLifestyle WithDisposal = new LifetimeScopeLifestyle(true);

        internal static readonly LifetimeScopeLifestyle NoDisposal = new LifetimeScopeLifestyle(false);

        /// <summary>Initializes a new instance of the <see cref="LifetimeScopeLifestyle"/> class. The instance
        /// will ensure that created and cached instance will be disposed after the execution of the web
        /// request ended and when the created object implements <see cref="IDisposable"/>.</summary>
        public LifetimeScopeLifestyle()
            : this(disposeInstanceWhenLifetimeScopeEnds: true)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="LifetimeScopeLifestyle"/> class.</summary>
        /// <param name="disposeInstanceWhenLifetimeScopeEnds">
        /// Specifies whether the created and cached instance will be disposed when the created 
        /// <see cref="LifetimeScope"/> instance gets disposed and when the created object implements 
        /// <see cref="IDisposable"/>. 
        /// </param>
        public LifetimeScopeLifestyle(bool disposeInstanceWhenLifetimeScopeEnds)
            : base("Lifetime Scope", disposeInstanceWhenLifetimeScopeEnds)
        {
        }

        /// <summary>Gets the length of the lifestyle.</summary>
        /// <value>The <see cref="Int32"/> representing the length of this lifestyle.</value>
        protected override int Length
        {
            get { return 100; }
        }

        /// <summary>
        /// Allows registering an <paramref name="action"/> delegate that will be called when the current
        /// lifetime scope ends, but before the scope disposes any instances.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> instance.</param>
        /// <param name="action">The delegate to run when the scope ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="InvalidOperationException">Will be thrown when there is currently no active
        /// lifetime scope in the supplied <paramref name="container"/> instance.</exception>
        public static void WhenCurrentScopeEnds(Container container, Action action)
        {
            WithDisposal.WhenScopeEnds(container, action);
        }

        internal static LifetimeScopeLifestyle Get(bool withDisposal)
        {
            return withDisposal ? WithDisposal : NoDisposal;
        }

        /// <summary>
        /// Returns the current <see cref="Scope"/> for this lifestyle and the given 
        /// <paramref name="container"/>, or null when this method is executed outside the context of a scope.
        /// </summary>
        /// <param name="container">The container instance that is related to the scope to return.</param>
        /// <returns>A <see cref="Scope"/> instance or null when there is no scope active in this context.</returns>
        protected override Scope GetCurrentScopeCore(Container container)
        {
            return container.GetLifetimeScopeManager().CurrentScope;
        }

        /// <summary>
        /// Creates a delegate that that upon invocation return the current <see cref="Scope"/> for this
        /// lifestyle and the given <paramref name="container"/>, or null when the delegate is executed outside
        /// the context of such scope.
        /// </summary>
        /// <param name="container">The container for which the delegate gets created.</param>
        /// <returns>A <see cref="Func{T}"/> delegate. This method never returns null.</returns>
        protected override Func<Scope> CreateCurrentScopeProvider(Container container)
        {
            var manager = container.GetLifetimeScopeManager();

            return () => manager.CurrentScope;
        }
    }
}