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

namespace SimpleInjector.Integration.Web
{
    using System;
    using System.Linq.Expressions;
    using System.Web;

    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

    public sealed class PerWebRequestLifestyle : Lifestyle
    {
        internal static readonly PerWebRequestLifestyle Dispose = new PerWebRequestLifestyle(true);
        internal static readonly PerWebRequestLifestyle Disposeless = new PerWebRequestLifestyle(false);

        private readonly bool dispose;

        public PerWebRequestLifestyle(bool disposeInstanceWhenWebRequestEnds = true)
        {
            this.dispose = disposeInstanceWhenWebRequestEnds;
        }

        public override LifestyleRegistration CreateRegistration<TService, TImplementation>(
            Container container)
        {
            return new PerWebRequestLifestyleRegistration<TService, TImplementation>(container)
            {
                Dispose = this.dispose
            };
        }

        public override LifestyleRegistration CreateRegistration<TService>(
            Func<TService> instanceCreator, Container container)
        {
            return new PerWebRequestLifestyleRegistration<TService>(container)
            {
                InstanceCreator = instanceCreator,
                Dispose = this.dispose
            };
        }

        private class PerWebRequestLifestyleRegistration<TService>
            : PerWebRequestLifestyleRegistration<TService, TService>
            where TService : class
        {
            public PerWebRequestLifestyleRegistration(Container container) : base(container)
            {
            }

            public Func<TService> InstanceCreator { get; set; }

            public override Func<TService> BuildTransientInstanceCreator()
            {
                return this.BuildTransientDelegate(this.InstanceCreator);
            }
        }

        private class PerWebRequestLifestyleRegistration<TService, TImplementation> : LifestyleRegistration
            where TService : class
            where TImplementation : class, TService
        {
            private readonly object key = new object();

            private Func<TService> instanceCreator;

            public PerWebRequestLifestyleRegistration(Container container) : base(container)
            {
            }

            public bool Dispose { get; set; }

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