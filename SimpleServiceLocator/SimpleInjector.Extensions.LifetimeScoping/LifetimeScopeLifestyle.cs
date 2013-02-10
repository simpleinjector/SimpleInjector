#region Copyright (c) 2013 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2013 S. van Deursen
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

namespace SimpleInjector.Extensions.LifetimeScoping
{
    using System;
    using System.Linq.Expressions;

    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Defines a lifestyle that caches instances during the lifetime of an explictly defined scope using the
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
    ///     container.GetInstance<IUnitOfWork>();
    /// }
    /// ]]></code>
    /// </example>
    public sealed class LifetimeScopeLifestyle : Lifestyle
    {
        internal static readonly Lifestyle WithDisposal = new LifetimeScopeLifestyle(true);

        internal static readonly LifetimeScopeLifestyle NoDisposal = new LifetimeScopeLifestyle(false);

        private readonly bool disposeInstanceWhenLifetimeScopeEnds;

        /// <summary>Initializes a new instance of the <see cref="LifetimeScopeLifestyle"/> class.</summary>
        /// <param name="disposeInstanceWhenLifetimeScopeEnds">
        /// Specifies whether the created and cached instance will be disposed when the created 
        /// <see cref="LifetimeScope"/> instance gets disposed and when the created object implements 
        /// <see cref="IDisposable"/>. 
        /// </param>
        public LifetimeScopeLifestyle(bool disposeInstanceWhenLifetimeScopeEnds = true) 
            : base("Lifetime Scope")
        {
            this.disposeInstanceWhenLifetimeScopeEnds = disposeInstanceWhenLifetimeScopeEnds;
        }
        
        /// <summary>Gets the length of the lifestyle.</summary>
        protected override int Length
        {
            get { return 100; }
        }

        internal static Lifestyle Get(bool disposeWhenLifetimeScopeEnds)
        {
            if (disposeWhenLifetimeScopeEnds)
            {
                return LifetimeScopeLifestyle.WithDisposal;
            }
            else
            {
                return LifetimeScopeLifestyle.NoDisposal;
            }
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
            this.EnableLifetimeScoping(container);

            return new LifetimeScopeRegistration<TService, TImplementation>(this, container)
            {
                Dispose = this.disposeInstanceWhenLifetimeScopeEnds
            };
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of the
        /// specified <typeparamref name="TService"/> using the supplied <paramref name="instanceCreator"/> 
        /// with the caching as specified by this lifestyle.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <param name="instanceCreator"></param>
        /// <param name="container">The <see cref="Container"/> instance for which a 
        /// <see cref="Registration"/> must be created.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator, 
            Container container)
        {
            this.EnableLifetimeScoping(container);

            return new LifetimeScopeRegistration<TService>(this, container)
            {
                Dispose = this.disposeInstanceWhenLifetimeScopeEnds,
                InstanceCreator = instanceCreator
            };
        }

        private void EnableLifetimeScoping(Container container)
        {
            try
            {
                SimpleInjectorLifetimeScopeExtensions.EnableLifetimeScoping(container);
            }
            catch (InvalidOperationException)
            {
                // Thrown when the container is locked.
            }
        }

        private sealed class LifetimeScopeRegistration<TService> : LifetimeScopeRegistration<TService, TService>
            where TService : class
        {
            internal LifetimeScopeRegistration(Lifestyle lifestyle, Container container) 
                : base(lifestyle, container)
            {
            }

            public Func<TService> InstanceCreator { get; set; }

            protected override Func<TService> BuildInstanceCreator()
            {
                return this.BuildTransientDelegate(this.InstanceCreator);
            }
        }

        private class LifetimeScopeRegistration<TService, TImplementation> : Registration
            where TService : class
            where TImplementation : class, TService
        {
            private Func<TService> instanceCreator;
            private LifetimeScopeManager manager;

            internal LifetimeScopeRegistration(Lifestyle lifestyle, Container container)
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
                    this.manager = this.Container.GetInstance<LifetimeScopeManager>();

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

                return scope.GetInstance(this.instanceCreator, this.Dispose);
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
                    "'LifetimeScope', but the instance is requested outside the context of a lifetime " +
                    "scope. Make sure you call container.BeginLifetimeScope() first.");
            }
        }
    } 
}