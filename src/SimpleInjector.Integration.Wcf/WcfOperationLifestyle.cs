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

namespace SimpleInjector.Integration.Wcf
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// Defines a lifestyle that caches instances for the lifetime of a WCF service class. WCF allows service
    /// classes to be (both implicitly and explicitly) configured to have a lifetime of <b>PerCall</b>, 
    /// <b>PerSession</b> or <b>Single</b> using the <see cref="InstanceContextMode"/> enumeration. The
    /// lifetime of WCF service classes is controlled by WCF and this lifestyle allows registrations to be
    /// scoped according to the containing WCF service class.
    /// </summary>
    /// <example>
    /// The following example shows the usage of the <b>WcfOperationLifestyle</b> class:
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    /// container.Options.DefaultScopedLifestyle = new WcfOperationLifestyle();
    /// container.Register<IUnitOfWork, EntityFrameworkUnitOfWork>(Lifestyle.Scoped);
    /// ]]></code>
    /// </example>
    public class WcfOperationLifestyle : ScopedLifestyle
    {
        /// <summary>Initializes a new instance of the <see cref="WcfOperationLifestyle"/> class. The instance
        /// will ensure that created and cached instance will be disposed after the execution of the web
        /// request ended and when the created object implements <see cref="IDisposable"/>.</summary>
        public WcfOperationLifestyle() : base("WCF Operation")
        {
        }

        /// <summary>Initializes a new instance of the <see cref="WcfOperationLifestyle"/> class.</summary>
        /// <param name="disposeInstanceWhenOperationEnds">
        /// Specifies whether the created and cached instance will be disposed after the execution of the WCF
        /// operation ended and when the created object implements <see cref="IDisposable"/>. 
        /// </param>
        [Obsolete("This constructor has been deprecated. Please use WcfOperationLifestyle() instead.",
            error: true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public WcfOperationLifestyle(bool disposeInstanceWhenOperationEnds) : this()
        {
            throw new NotSupportedException(
                "This constructor has been deprecated. Please use WcfOperationLifestyle() instead.");
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
        [Obsolete("WhenWcfOperationEnds has been deprecated. " +
            "Please use Lifestyle.Scoped.WhenScopeEnds(Container, Action) instead.",
            error: true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void WhenWcfOperationEnds(Container container, Action action)
        {
            throw new NotSupportedException(
                "WhenWcfOperationEnds has been deprecated. " +
                "Please use Lifestyle.Scoped.WhenScopeEnds(Container, Action) instead.");
        }
        
        internal static Scope GetCurrentScopeCore()
        {
            var operationContext = OperationContext.Current;

            var instanceContext = operationContext != null ? operationContext.InstanceContext : null;

            return instanceContext != null ? instanceContext.GetCurrentScope() : null;
        }

        /// <summary>
        /// Returns the current <see cref="Scope"/> for this lifestyle and the given 
        /// <paramref name="container"/>, or null when this method is executed outside the context of a scope.
        /// </summary>
        /// <param name="container">The container instance that is related to the scope to return.</param>
        /// <returns>A <see cref="Scope"/> instance or null when there is no scope active in this context.</returns>
        protected override Scope GetCurrentScopeCore(Container container) => GetCurrentScopeCore();

        /// <summary>
        /// Creates a delegate that upon invocation return the current <see cref="Scope"/> for this
        /// lifestyle and the given <paramref name="container"/>, or null when the delegate is executed outside
        /// the context of such scope.
        /// </summary>
        /// <param name="container">The container for which the delegate gets created.</param>
        /// <returns>A <see cref="Func{T}"/> delegate. This method never returns null.</returns>
        protected override Func<Scope> CreateCurrentScopeProvider(Container container)
        {
            return GetCurrentScopeCore;
        }
    }
}