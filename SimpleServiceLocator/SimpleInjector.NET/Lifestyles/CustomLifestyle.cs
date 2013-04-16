namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Linq.Expressions;
    using System.Threading;

    internal sealed class CustomLifestyle : Lifestyle
    {
        private readonly CreateLifestyleApplier lifestyleApplierFactory;

        public CustomLifestyle(string name, CreateLifestyleApplier lifestyleApplierFactory)
            : base(name)
        {
            this.lifestyleApplierFactory = lifestyleApplierFactory;
        }

        // Ensure that this lifestyle can only be safely used with singleton dependencies.
        internal override int ComponentLength
        {
            get { return Lifestyle.Singleton.ComponentLength; }
        }

        // Ensure that this lifestyle can only be safely used with transient components/consumers.
        internal override int DependencyLength
        {
            get { return Lifestyle.Transient.DependencyLength; }
        }

        protected override int Length
        {
            get { throw new NotSupportedException("The length property is not supported for this lifestyle."); }
        }
        
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

            protected override Func<TService> BuildTransientDelegate()
            {
                return this.BuildTransientDelegate<TService>(this.InstanceCreator);
            }
        }

        private class CustomRegistration<TService, TImplementation> : Registration
            where TImplementation : class, TService
            where TService : class
        {
            private Lazy<Func<object>> lazyLifestyleApplier;

            public CustomRegistration(CreateLifestyleApplier lifestyleApplierFactory, 
                Lifestyle lifestyle, Container container) : base(lifestyle, container)
            {
                Func<Func<object>> initializer = () =>
                {
                    Func<TImplementation> transientInstanceCreator = this.BuildTransientDelegate();
                    return lifestyleApplierFactory(() => transientInstanceCreator());
                };

                this.lazyLifestyleApplier = new Lazy<Func<object>>(initializer);
            }

            public override Type ImplementationType
            {
                get { return typeof(TImplementation); }
            }

            public override Expression BuildExpression()
            {
                return Expression.Convert(
                    Expression.Invoke(
                        Expression.Constant(this.lazyLifestyleApplier.Value)),
                    typeof(TService));
            }

            protected virtual Func<TImplementation> BuildTransientDelegate()
            {
                return this.BuildTransientDelegate<TService, TImplementation>();
            }
        }
    }
}