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

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Linq.Expressions;

    internal sealed class CustomLifestyle : Lifestyle
    {
        private readonly CreateLifestyleApplier lifestyleApplierFactory;

        public CustomLifestyle(string name, CreateLifestyleApplier lifestyleApplierFactory)
            : base(name)
        {
            this.lifestyleApplierFactory = lifestyleApplierFactory;
        }

        public override int Length
        {
            get { throw new NotSupportedException("The length property is not supported for this lifestyle."); }
        }

        // Ensure that this lifestyle can only be safely used with singleton dependencies.
        internal override int ComponentLength(Container container) => Singleton.ComponentLength(container);

        // Ensure that this lifestyle can only be safely used with transient components/consumers.
        internal override int DependencyLength(Container container) => Transient.DependencyLength(container);

        protected internal override Registration CreateRegistrationCore<TConcrete>(Container container)
        {
            return new CustomRegistration<TConcrete>(this.lifestyleApplierFactory, this, container);
        }

        protected internal override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container)
        {
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            return new CustomRegistration<TService>(this.lifestyleApplierFactory, this, container, instanceCreator);
        }

        private sealed class CustomRegistration<TImplementation> : Registration where TImplementation : class
        {
            private readonly CreateLifestyleApplier lifestyleApplierFactory;
            private readonly Func<TImplementation> instanceCreator;

            public CustomRegistration(CreateLifestyleApplier lifestyleApplierFactory, Lifestyle lifestyle,
                Container container, Func<TImplementation> instanceCreator = null)
                : base(lifestyle, container)
            {
                this.lifestyleApplierFactory = lifestyleApplierFactory;
                this.instanceCreator = instanceCreator;
            }

            public override Type ImplementationType => typeof(TImplementation);

            public override Expression BuildExpression() =>
                Expression.Convert(
                    Expression.Invoke(
                        Expression.Constant(this.CreateInstanceCreator())),
                    typeof(TImplementation));

            private Func<object> CreateInstanceCreator()
            {
                Func<TImplementation> transientInstanceCreator =
                    this.instanceCreator == null
                        ? (Func<TImplementation>)this.BuildTransientDelegate()
                        : this.BuildTransientDelegate(this.instanceCreator);

                return this.lifestyleApplierFactory(() => transientInstanceCreator());
            }
        }
    }
}