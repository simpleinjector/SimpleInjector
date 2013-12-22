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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

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
        /// <summary>
        /// A default <see cref="WcfOperationLifestyle"/> instance that can be used for registering components
        /// per WCF Operation. This instance will ensure created instance get disposed after the WCF operation
        /// ends.
        /// </summary>
        internal static readonly Lifestyle WithDisposal = new WcfOperationLifestyle(true);

        internal static readonly WcfOperationLifestyle NoDisposal = new WcfOperationLifestyle(false);

        private readonly bool disposeInstanceWhenOperationEnds;
        
        /// <summary>Initializes a new instance of the <see cref="WcfOperationLifestyle"/> class. The instance
        /// will ensure that created and cached instance will be disposed after the execution of the web
        /// request ended and when the created object implements <see cref="IDisposable"/>.</summary>
        public WcfOperationLifestyle() : this(disposeInstanceWhenOperationEnds: true)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="WcfOperationLifestyle"/> class.</summary>
        /// <param name="disposeInstanceWhenOperationEnds">
        /// Specifies whether the created and cached instance will be disposed after the execution of the WCF
        /// operation ended and when the created object implements <see cref="IDisposable"/>. 
        /// </param>
        public WcfOperationLifestyle(bool disposeInstanceWhenOperationEnds) : base("WCF Operation")
        {
            this.disposeInstanceWhenOperationEnds = disposeInstanceWhenOperationEnds;
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
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var scope = container.GetCurrentWcfOperationScope();

            if (scope == null)
            {
                if (container.IsVerifying())
                {
                    // We're verifying the container, it's impossible to register the action somewhere, but
                    // verification should absolutely not fail because of this.
                    return;
                }

                throw new InvalidOperationException("This method can only be called within the context of " +
                    "an active WCF operation.");
            }

            scope.RegisterDelegateForScopeEnd(action);
        }

        /// <summary>
        /// Allows registering an <paramref name="action"/> delegate that will be called when the current WCF 
        /// operation ends, but before the scope disposes any instances.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> instance.</param>
        /// <param name="action">The delegate to run when the WCF operation ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="InvalidOperationException">Will be thrown when there is currently no active
        /// WCF operation in the supplied <paramref name="container"/> instance.</exception>
        public override void WhenScopeEnds(Container container, Action action)
        {
            WhenWcfOperationEnds(container, action);
        }

        /// <summary>
        /// Adds the <paramref name="disposable"/> to the list of items that will get disposed when the
        /// WCF operation ends.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> instance.</param>
        /// <param name="disposable">The instance that should be disposed when the WCF operation ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="InvalidOperationException">Will be thrown when the current thread isn't running
        /// in the context of a WCF operation.</exception>
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

            var scope = container.GetCurrentWcfOperationScope();

            if (scope == null)
            {
                if (container.IsVerifying())
                {
                    // We're verifying the container, it's impossible to register the action somewhere, but
                    // verification should absolutely not fail because of this.
                    return;
                }

                throw new InvalidOperationException("This method can only be called within the context of " +
                    "an active WCF operation.");
            }

            scope.RegisterForDisposal(disposable);
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
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Supplying the generic type arguments is needed, since internal types can not " +
                            "be created using the non-generic overloads in a sandbox.")]
        protected override Registration CreateRegistrationCore<TService, TImplementation>(Container container)
        {
            EnablePerWcfOperationLifestyle(container);

            return new WcfOperationRegistration<TService, TImplementation>(this, container)
            {
                Dispose = this.disposeInstanceWhenOperationEnds
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
            EnablePerWcfOperationLifestyle(container);

            return new PerWcfOperationRegistration<TService>(this, container)
            {
                Dispose = this.disposeInstanceWhenOperationEnds,
                InstanceCreator = instanceCreator
            };
        }

        private static void EnablePerWcfOperationLifestyle(Container container)
        {
            try
            {
                SimpleInjectorWcfExtensions.EnablePerWcfOperationLifestyle(container);
            }
            catch (InvalidOperationException)
            {
                // Thrown when the container is locked.
            }
        }

        private sealed class PerWcfOperationRegistration<TService>
            : WcfOperationRegistration<TService, TService>
            where TService : class
        {
            internal PerWcfOperationRegistration(Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
            }

            internal Func<TService> InstanceCreator { get; set; }

            protected override Func<TService> BuildInstanceCreator()
            {
                return this.BuildTransientDelegate(this.InstanceCreator);
            }
        }

        private class WcfOperationRegistration<TService, TImplementation> : Registration
            where TService : class
            where TImplementation : class, TService
        {
            private Func<TService> instanceCreator;
            private WcfOperationScopeManager manager;

            internal WcfOperationRegistration(Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
            }

            public override Type ImplementationType
            {
                get { return typeof(TImplementation); }
            }

            internal bool Dispose { get; set; }

            public override Expression BuildExpression()
            {
                if (this.instanceCreator == null)
                {
                    this.manager = this.Container.GetInstance<WcfOperationScopeManager>();

                    this.instanceCreator = this.BuildInstanceCreator();
                }

                return Expression.Call(Expression.Constant(this), this.GetType().GetMethod("GetInstance"));
            }

            // This method needs to be public, because the BuildExpression methods build a
            // MethodCallExpression using this method, and this would fail in partial trust when the 
            // method is not public.
            public TService GetInstance()
            {
                var scope = this.manager.CurrentScope;

                if (scope == null)
                {
                    return this.GetInstanceWithoutScope();
                }

                return scope.GetInstance(this, this.instanceCreator, this.Dispose);
            }

            protected virtual Func<TService> BuildInstanceCreator()
            {
                return this.BuildTransientDelegate<TService, TImplementation>();
            }

            private TService GetInstanceWithoutScope()
            {
                if (this.Container.IsVerifying())
                {
                    // Return a transient instance when this method is called during verification
                    return this.instanceCreator();
                }

                throw new ActivationException("The " + typeof(TService).Name + " is registered as " +
                    "'PerWcfOperation', but the instance is requested outside the context of a WCF " +
                    "operation.");
            }
        }
    }
}