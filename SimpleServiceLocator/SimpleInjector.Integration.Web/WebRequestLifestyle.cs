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

namespace SimpleInjector.Integration.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Web;
    using SimpleInjector.Advanced;

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
        /// <summary>
        /// A default <see cref="WebRequestLifestyle"/> instance that can be used for registering components
        /// per web request. This instance will ensure created instance get disposed after the web request
        /// ends.
        /// </summary>
        internal static readonly Lifestyle WithDisposal = new WebRequestLifestyle();

        internal static readonly WebRequestLifestyle Disposeless = new WebRequestLifestyle(false);

        private readonly bool dispose;

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
        public WebRequestLifestyle(bool disposeInstanceWhenWebRequestEnds) : base("Web Request")
        {
            this.dispose = disposeInstanceWhenWebRequestEnds;
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
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var context = HttpContext.Current;

            if (context == null)
            {
                if (container.IsVerifying())
                {
                    // We're verifying the container, it's impossible to register the action somewhere, but
                    // verification should absolutely not fail because of this.
                    return;
                }

                throw new InvalidOperationException(
                    "This method can only be called in the context of a web request.");
            }

            SimpleInjectorWebExtensions.RegisterDelegateForWebRequestEnd(context, action);
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
        public override void WhenScopeEnds(Container container, Action action)
        {
            WhenCurrentRequestEnds(container, action);
        }

        /// <summary>
        /// Adds the <paramref name="disposable"/> to the list of items that will get disposed when the
        /// web request ends.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> instance.</param>
        /// <param name="disposable">The instance that should be disposed when the web request ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="InvalidOperationException">Will be thrown when the current thread isn't running
        /// in the context of a web request.</exception>
        public override void RegisterForDisposal(Container container, IDisposable disposable)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (disposable == null)
            {
                throw new ArgumentNullException("disposable");
            }

            var context = HttpContext.Current;

            if (context == null)
            {
                if (container.IsVerifying())
                {
                    // We're verifying the container, it's impossible to register the action somewhere, but
                    // verification should absolutely not fail because of this.
                    return;
                }

                throw new InvalidOperationException(
                    "This method can only be called in the context of a web request.");
            }

            SimpleInjectorWebExtensions.RegisterDisposableForEndWebRequest(context, disposable);
        }

        internal static new void DisposeInstances(IList<IDisposable> disposables)
        {
            ScopedLifestyle.DisposeInstances(disposables);
        }

        internal static Lifestyle Get(bool disposeInstanceWhenWebRequestEnds)
        {
            return disposeInstanceWhenWebRequestEnds ? WithDisposal : Disposeless;
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of the
        /// specified <typeparamref name="TImplementation"/> with the caching as specified by this lifestyle.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="container">The <see cref="Container"/> instance for which a 
        /// <see cref="Registration"/> must be created.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        protected override Registration CreateRegistrationCore<TService, TImplementation>(Container container)
        {
            return new WebRequestRegistration<TService, TImplementation>(this, container)
            {
                Dispose = this.dispose
            };
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of the
        /// specified <typeparamref name="TService"/> using the supplied <paramref name="instanceCreator"/> 
        /// with the caching as specified by this lifestyle.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <param name="instanceCreator">A delegate that will create a new instance of 
        /// <typeparamref name="TService"/> every time it is called.</param>
        /// <param name="container">The <see cref="Container"/> instance for which a 
        /// <see cref="Registration"/> must be created.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator, 
            Container container)
        {
            return new WebRequestRegistration<TService>(this, container)
            {
                InstanceCreator = instanceCreator,
                Dispose = this.dispose
            };
        }

        private class WebRequestRegistration<TService> : WebRequestRegistration<TService, TService>
            where TService : class
        {
            public WebRequestRegistration(Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
            }

            public Func<TService> InstanceCreator { get; set; }

            public override Func<TService> BuildTransientInstanceCreator()
            {
                return this.BuildTransientDelegate<TService>(this.InstanceCreator);
            }
        }

        private class WebRequestRegistration<TService, TImplementation> : Registration
            where TService : class
            where TImplementation : class, TService
        {
            private readonly object key = new object();

            private Func<TService> instanceCreator;

            public WebRequestRegistration(Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
            }

            public bool Dispose { get; set; }

            public override Type ImplementationType
            {
                get { return typeof(TImplementation); }
            }

            public override Expression BuildExpression()
            {
                if (this.instanceCreator == null)
                {
                    this.instanceCreator = this.BuildTransientInstanceCreator();
                }

                return Expression.Call(Expression.Constant(this), this.GetType().GetMethod("GetInstance"));
            }

            public virtual Func<TService> BuildTransientInstanceCreator()
            {
                return this.BuildTransientDelegate<TService, TImplementation>();
            }

            public TService GetInstance()
            {
                // This method needs to be public, because the BuildExpression extension methods build a
                // MethodCallExpression using this method, and this would fail in partial trust when the 
                // method is not public.
                var context = HttpContext.Current;

                if (context == null)
                {
                    return this.GetInstanceWithoutContext();
                }

                TService instance = (TService)context.Items[this.key];

                if (instance == null)
                {
                    context.Items[this.key] = instance = this.instanceCreator();

                    this.RegisterForDisposal(instance);
                }

                return instance;
            }

            private TService GetInstanceWithoutContext()
            {
                if (this.Container.IsVerifying())
                {
                    // Return a transient instance when this method is called during verification
                    return this.instanceCreator();
                }

                throw new ActivationException("The " + typeof(TService).FullName + " is registered as " +
                    "'PerWebRequest', but the instance is requested outside the context of a " +
                    "HttpContext (HttpContext.Current is null). Make sure instances using this " +
                    "lifestyle are not resolved during the application initialization phase and when " +
                    "running on a background thread. For resolving instances on background threads, " +
                    "try registering this instance as 'Per Lifetime Scope': http://bit.ly/N1s8hN.");
            }

            private void RegisterForDisposal(TService instance)
            {
                if (this.Dispose)
                {
                    var disposableInstance = instance as IDisposable;

                    if (disposableInstance != null)
                    {
                        SimpleInjectorWebExtensions.RegisterForDisposal(disposableInstance);
                    }
                }
            }
        }
    }
}