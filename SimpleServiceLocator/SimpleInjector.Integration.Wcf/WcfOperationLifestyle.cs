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

namespace SimpleInjector.Integration.Wcf
{
    using System;
    using System.Linq.Expressions;

    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

    public class WcfOperationLifestyle : Lifestyle
    {
        public static readonly Lifestyle Instance = WithDisposal;

        internal static readonly WcfOperationLifestyle WithDisposal = new WcfOperationLifestyle(true);
        internal static readonly WcfOperationLifestyle NoDisposal = new WcfOperationLifestyle(false);

        private readonly bool disposeInstanceWhenOperationEnds;

        public WcfOperationLifestyle(bool disposeInstanceWhenOperationEnds = true) : base("WCF Operation")
        {
            this.disposeInstanceWhenOperationEnds = disposeInstanceWhenOperationEnds;
        }

        protected override int Length
        {
            get { return 250; }
        }

        public override Registration CreateRegistration<TService, TImplementation>(Container container)
        {
            this.EnablePerWcfOperationLifestyle(container);

            return new WcfOperationRegistration<TService, TImplementation>(this, container)
            {
                Dispose = this.disposeInstanceWhenOperationEnds
            };
        }

        public override Registration CreateRegistration<TService>(Func<TService> instanceCreator, 
            Container container)
        {
            this.EnablePerWcfOperationLifestyle(container);

            return new PerWcfOperationRegistration<TService>(this, container)
            {
                Dispose = this.disposeInstanceWhenOperationEnds,
                InstanceCreator = instanceCreator
            };
        }

        private void EnablePerWcfOperationLifestyle(Container container)
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
                    "'PerWcfOperation', but the instance is requested outside the context of a WCF " +
                    "operation.");
            }
        }
    }
}
