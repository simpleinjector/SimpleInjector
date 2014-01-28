#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014 Simple Injector Contributors
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

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Linq.Expressions;

    internal sealed class ScopedRegistration<TService, TImplementation> : Registration
        where TImplementation : class, TService
        where TService : class
    {
        private readonly Func<TService> userSuppliedInstanceCreator;

        private Func<Scope> scopeFactory;
        private Func<TService> instanceCreator;

        internal ScopedRegistration(ScopedLifestyle lifestyle, Container container,
            bool registerForDisposal, Func<TService> instanceCreator)
            : this(lifestyle, container, registerForDisposal)
        {
            this.userSuppliedInstanceCreator = instanceCreator;
        }

        internal ScopedRegistration(ScopedLifestyle lifestyle, Container container,
            bool registerForDisposal)
            : base(lifestyle, container)
        {
            this.RegisterForDisposal = registerForDisposal;
        }

        public override Type ImplementationType
        {
            get { return typeof(TImplementation); }
        }

        public new ScopedLifestyle Lifestyle
        {
            get { return (ScopedLifestyle)base.Lifestyle; }
        }

        internal Func<TService> InstanceCreator
        {
            get { return this.instanceCreator; }
        }

        internal bool RegisterForDisposal { get; private set; }

        public override Expression BuildExpression()
        {
            if (this.instanceCreator == null)
            {
                this.scopeFactory = this.Lifestyle.CreateCurrentScopeProvider(this.Container);

                this.instanceCreator = this.BuildInstanceCreator();
            }

            return Expression.Call(Expression.Constant(this), this.GetType().GetMethod("GetInstance"));
        }

        // This method needs to be public, because the BuildExpression methods build a
        // MethodCallExpression using this method, and this would fail in partial trust when the 
        // method is not public.
        public TService GetInstance()
        {
            // Simple Injector does some aggressive optimizations for scoped lifestyles and this method will
            // is most cases not be called. It will however be called when the expression that is built by
            // this instance will get compiled by someone else than the core library. That's why this method
            // is still important.
            return Scope.GetInstance(this, this.scopeFactory());
        }

        private Func<TService> BuildInstanceCreator()
        {
            if (this.userSuppliedInstanceCreator != null)
            {
                return this.BuildTransientDelegate(this.userSuppliedInstanceCreator);
            }
            else
            {
                var instanceCreator = this.BuildTransientDelegate<TService, TImplementation>();

                // WTF! Somehow Func<T> is not contravariant in PCL :-(
#if PCL
                return () => instanceCreator();
#else
                return instanceCreator;
#endif
            }
        }
    }
}