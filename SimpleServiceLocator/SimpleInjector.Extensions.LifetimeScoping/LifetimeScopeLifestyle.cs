#region Copyright (c) 2012 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2012 S. van Deursen
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

    public sealed class LifetimeScopeLifestyle : Lifestyle
    {
        public static readonly Lifestyle Instance = WithDisposal;

        internal static readonly LifetimeScopeLifestyle WithDisposal = new LifetimeScopeLifestyle(true);
        internal static readonly LifetimeScopeLifestyle NoDisposal = new LifetimeScopeLifestyle(false);

        private readonly bool disposeInstanceWhenLifetimeScopeEnds;

        public LifetimeScopeLifestyle(bool disposeInstanceWhenLifetimeScopeEnds = true) 
            : base("Lifetime Scope")
        {
            this.disposeInstanceWhenLifetimeScopeEnds = disposeInstanceWhenLifetimeScopeEnds;
        }

        protected override int Length
        {
            get { return 100; }
        }

        public override Registration CreateRegistration<TService, TImplementation>(Container container)
        {
            this.EnableLifetimeScoping(container);

            return new LifetimeScopeRegistration<TService, TImplementation>(this, container)
            {
                Dispose = this.disposeInstanceWhenLifetimeScopeEnds
            };
        }

        public override Registration CreateRegistration<TService>(Func<TService> instanceCreator, 
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