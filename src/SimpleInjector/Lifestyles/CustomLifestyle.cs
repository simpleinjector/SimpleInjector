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

        protected override Registration CreateRegistrationCore<TService, TImplementation>(Container container)
        {
            return new CustomRegistration<TService, TImplementation>(this.lifestyleApplierFactory, this, container);
        }

        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator, 
            Container container)
        {
            return new CustomRegistration<TService>(this.lifestyleApplierFactory, this, container)
            {
                InstanceCreator = instanceCreator,
            };
        }

        private sealed class CustomRegistration<TService> : CustomRegistration<TService, TService>
            where TService : class
        {
            public CustomRegistration(CreateLifestyleApplier lifestyleCreator, Lifestyle lifestyle,
                Container container)
                : base(lifestyleCreator, lifestyle, container)
            {
            }

            public Func<TService> InstanceCreator { get; set; }

            protected override Func<TService> BuildTransientDelegate(InstanceProducer producer) => 
                this.BuildTransientDelegate(producer, this.InstanceCreator);
        }

        private class CustomRegistration<TService, TImplementation> : Registration
            where TImplementation : class, TService
            where TService : class
        {
            private readonly CreateLifestyleApplier lifestyleApplierFactory;

            public CustomRegistration(CreateLifestyleApplier lifestyleApplierFactory, 
                Lifestyle lifestyle, Container container) : base(lifestyle, container)
            {
                this.lifestyleApplierFactory = lifestyleApplierFactory;
            }

            public override Type ImplementationType => typeof(TImplementation);

            public override Expression BuildExpression(InstanceProducer producer) => 
                Expression.Convert(
                    Expression.Invoke(
                        Expression.Constant(this.CreateInstanceCreator(producer))),
                    typeof(TService));

            protected virtual Func<TImplementation> BuildTransientDelegate(InstanceProducer producer) => 
                this.BuildTransientDelegate<TService, TImplementation>(producer);

            private Func<object> CreateInstanceCreator(InstanceProducer producer)
            {
                Func<TImplementation> transientInstanceCreator = this.BuildTransientDelegate(producer);
                return this.lifestyleApplierFactory(() => transientInstanceCreator());
            }
        }
    }
}