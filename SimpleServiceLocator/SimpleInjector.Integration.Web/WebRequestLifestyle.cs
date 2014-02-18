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

namespace SimpleInjector.Integration.Web
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    /// <summary>
    /// Defines a lifestyle that caches instances during the execution of a single HTTP Web Request.
    /// Unless explicitly stated otherwise, instances created by this lifestyle will be disposed at the end
    /// of the web request.
    /// </summary>
    /// <example>
    /// The following example shows the usage of the <b>WebRequestLifestyle</b> class:
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    /// 
    /// container.Register<IUnitOfWork, EntityFrameworkUnitOfWork>(new WebRequestLifestyle());
    /// ]]></code>
    /// </example>
    public sealed class WebRequestLifestyle : ScopedLifestyle
    {
        internal static readonly WebRequestLifestyle WithDisposal = new WebRequestLifestyle();

        internal static readonly WebRequestLifestyle Disposeless = new WebRequestLifestyle(false);

        private static readonly object ScopeKey = new object();

        /// <summary>Initializes a new instance of the <see cref="WebRequestLifestyle"/> class. The instance
        /// will ensure that created and cached instance will be disposed after the execution of the web
        /// request ended and when the created object implements <see cref="IDisposable"/>.</summary>
        public WebRequestLifestyle() : this(disposeInstanceWhenWebRequestEnds: true)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="WebRequestLifestyle"/> class.</summary>
        /// <param name="disposeInstanceWhenWebRequestEnds">
        /// Specifies whether the created and cached instance will be disposed after the execution of the web
        /// request ended and when the created object implements <see cref="IDisposable"/>. 
        /// </param>
        public WebRequestLifestyle(bool disposeInstanceWhenWebRequestEnds)
            : base("Web Request", disposeInstanceWhenWebRequestEnds)
        {
        }

        /// <summary>Gets the length of the lifestyle.</summary>
        /// <value>The length of the lifestyle.</value>
        protected override int Length
        {
            get { return 300; }
        }

        /// <summary>
        /// Allows registering an <paramref name="action"/> delegate that will be called when the scope ends,
        /// but before the scope disposes any instances.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> instance.</param>
        /// <param name="action">The delegate to run when the scope ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="InvalidOperationException">Will be thrown when the current thread isn't running
        /// in the context of a web request.</exception>
        public static void WhenCurrentRequestEnds(Container container, Action action)
        {
            WithDisposal.WhenScopeEnds(container, action);
        }

        internal static Lifestyle Get(bool disposeInstanceWhenWebRequestEnds)
        {
            return disposeInstanceWhenWebRequestEnds ? WithDisposal : Disposeless;
        }

        internal static void RegisterForDisposal(IDisposable disposable, HttpContext context)
        {
            GetCurrentScope(context).RegisterForDisposal(disposable);
        }

        internal static void CleanUpWebRequest()
        {
            var context = HttpContext.Current;

            if (context == null)
            {
                throw new InvalidOperationException("HttpContext.Current == null.");
            }

            var items = context.Items;

            if (items == null)
            {
                throw new InvalidOperationException("HttpContext.Current.Items == null.");
            }

            Scope scope = (Scope)items[ScopeKey];

            if (scope != null)
            {
                scope.Dispose();
            }
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
            Requires.IsNotNull(container, "container");

            return () => GetCurrentScope(HttpContext.Current);
        }

        /// <summary>
        /// Returns the current <see cref="Scope"/> for this lifestyle and the given 
        /// <paramref name="container"/>, or null when this method is executed outside the context of a scope.
        /// </summary>
        /// <param name="container">The container instance that is related to the scope to return.</param>
        /// <returns>A <see cref="Scope"/> instance or null when there is no scope active in this context.</returns>
        public override Scope GetCurrentScope(Container container)
        {
            Requires.IsNotNull(container, "container");

            return GetCurrentScope(HttpContext.Current);
        }

        private static Scope GetCurrentScope(HttpContext context)
        {
            if (context == null)
            {
                return null;
            }

            Scope scope = (Scope)context.Items[ScopeKey];

            if (scope == null)
            {
                // If there are multiple container instances that run on the same request (which is a
                // strange but valid scenario), all containers will get the same Scope instance for that
                // request. This behavior is correct and even allows all instances that are registered for
                // disposal to be disposed in reversed order of creation, independant of the container that
                // created them.
                context.Items[ScopeKey] = scope = new Scope();
            }

            return scope;
        }
    }
}