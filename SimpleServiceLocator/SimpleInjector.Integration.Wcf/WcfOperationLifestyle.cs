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

namespace SimpleInjector.Integration.Wcf
{
    using System;

    /// <summary>
    /// Defines a lifestyle that caches instances during the execution of a single WCF operation.
    /// </summary>
    /// <example>
    /// The following example shows the usage of the <b>WcfOperationLifestyle</b> class:
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    /// 
    /// container.Register<IUnitOfWork, EntityFrameworkUnitOfWork>(new WcfOperationLifestyle());
    /// ]]></code>
    /// </example>
    public class WcfOperationLifestyle : ScopedLifestyle
    {
        internal static readonly WcfOperationLifestyle WithDisposal = new WcfOperationLifestyle(true);

        internal static readonly WcfOperationLifestyle NoDisposal = new WcfOperationLifestyle(false);

        /// <summary>Initializes a new instance of the <see cref="WcfOperationLifestyle"/> class. The instance
        /// will ensure that created and cached instance will be disposed after the execution of the web
        /// request ended and when the created object implements <see cref="IDisposable"/>.</summary>
        public WcfOperationLifestyle() 
            : this(disposeInstanceWhenOperationEnds: true)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="WcfOperationLifestyle"/> class.</summary>
        /// <param name="disposeInstanceWhenOperationEnds">
        /// Specifies whether the created and cached instance will be disposed after the execution of the WCF
        /// operation ended and when the created object implements <see cref="IDisposable"/>. 
        /// </param>
        public WcfOperationLifestyle(bool disposeInstanceWhenOperationEnds)
            : base("WCF Operation", disposeInstanceWhenOperationEnds)
        {
        }

        /// <summary>Gets the length of the lifestyle.</summary>
        /// <value>The length of the lifestyle.</value>
        protected override int Length
        {
            get { return 250; }
        }

        /// <summary>
        /// Allows registering an <paramref name="action"/> delegate that will be called when the current
        /// WCF operation ends, but before the scope disposes any instances.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> instance.</param>
        /// <param name="action">The delegate to run when the WCF operation ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="InvalidOperationException">Will be thrown when there is currently no active
        /// WCF operation in the supplied <paramref name="container"/> instance.</exception>
        public static void WhenWcfOperationEnds(Container container, Action action)
        {
            WithDisposal.WhenScopeEnds(container, action);
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
            var manager = container.GetWcfOperationScopeManager();

            return () => manager.CurrentScope;
        }

        /// <summary>
        /// Returns the current <see cref="Scope"/> for this lifestyle and the given 
        /// <paramref name="container"/>, or null when this method is executed outside the context of a scope.
        /// </summary>
        /// <param name="container">The container instance that is related to the scope to return.</param>
        /// <returns>A <see cref="Scope"/> instance or null when there is no scope active in this context.</returns>
        protected override Scope GetCurrentScope(Container container)
        {
            return container.GetWcfOperationScopeManager().CurrentScope;
        }
    }
}