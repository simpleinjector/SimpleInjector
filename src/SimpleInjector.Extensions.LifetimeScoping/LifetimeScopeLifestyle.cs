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

namespace SimpleInjector.Extensions.LifetimeScoping
{
    using System;
    using Lifestyles;

    /// <summary>
    /// Defines a lifestyle that caches instances during the lifetime of an explicitly defined scope using the
    /// <see cref="SimpleInjectorLifetimeScopeExtensions.BeginLifetimeScope(Container)">BeginLifetimeScope</see>
    /// method. A scope is thread-specific, each thread should define its own scope. Scopes can be nested and
    /// nested scopes will get their own instance. Instances created by this lifestyle can be disposed when 
    /// the created scope gets disposed. 
    /// </summary>
    /// <example>
    /// The following example shows the usage of the <b>LifetimeScopeLifestyle</b> class:
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    /// container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();
    /// 
    /// container.Register<IUnitOfWork, EntityFrameworkUnitOfWork>(Lifestyle.Scoped);
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
    [Obsolete("This lifestyle is obsolete. Please use SimpleInjector.Lifestyles.ThreadScopedLifestyle instead.", error: false)]
    public sealed class LifetimeScopeLifestyle : ThreadScopedLifestyle
    {
        /// <summary>Initializes a new instance of the <see cref="LifetimeScopeLifestyle"/> class. The instance
        /// will ensure that created and cached instance will be disposed after the execution of the web
        /// request ended and when the created object implements <see cref="IDisposable"/>.</summary>
        public LifetimeScopeLifestyle()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="LifetimeScopeLifestyle"/> class.</summary>
        /// <param name="disposeInstanceWhenLifetimeScopeEnds">
        /// Specifies whether the created and cached instance will be disposed when the created 
        /// <see cref="Scope"/> instance gets disposed and when the created object implements 
        /// <see cref="IDisposable"/>. 
        /// </param>
        [Obsolete("This constructor has been deprecated. Please use LifetimeScopeLifestyle() instead.",
            error: true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public LifetimeScopeLifestyle(bool disposeInstanceWhenLifetimeScopeEnds)
        {
            throw new NotSupportedException(
                "This constructor overload has been deprecated. Please use LifetimeScopeLifestyle() instead.");
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
        [Obsolete("WhenCurrentScopeEnds has been deprecated. " +
            "Please use Lifestyle.Scoped.WhenScopeEnds(Container, Action) instead.",
            error: true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void WhenCurrentScopeEnds(Container container, Action action)
        {
            throw new NotSupportedException("WhenCurrentScopeEnds has been deprecated. " +
                "Please use Lifestyle.Scoped.WhenScopeEnds(Container, Action) instead.");
        }
    }
}