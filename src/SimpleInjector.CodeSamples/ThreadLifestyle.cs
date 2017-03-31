namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using SimpleInjector;

    // There is no support for a Thread lifestyle in the core library, because this lifestyle is considered
    // harmful. It should not be used in web applications, because ASP.NET can finish a request on a different
    // thread. This can cause a Per Thread instance to be used from another thread, which can cause all sorts 
    // of race conditions. Even letting transient component depend on a per-thread component can cause trouble.
    // Instead of using Per Thread lifestyle, use ThreadScopedLifestyle instead.
    public sealed class ThreadLifestyle : Lifestyle
    {
        public static readonly Lifestyle Instance = new ThreadLifestyle();

        public ThreadLifestyle() : base("Thread")
        {
        }

        // Greater than Scope, but smaller than Singleton.
        public override int Length => Lifestyle.Singleton.Length - 1;

        protected override Registration CreateRegistrationCore<TConcrete>(Container container)
        {
            return new PerThreadRegistration<TConcrete>(this, container);
        }

        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container)
        {
            if (instanceCreator == null)
            {
                throw new ArgumentNullException(nameof(instanceCreator));
            }

            return new PerThreadRegistration<TService>(this, container, instanceCreator);
        }

        private sealed class PerThreadRegistration<TImplementation> : Registration
            where TImplementation : class
        {
            private readonly ThreadLocal<TImplementation> threadSpecificCache = new ThreadLocal<TImplementation>();
            private readonly Func<TImplementation> instanceCreator;

            private Func<TImplementation> instanceProducer;

            public PerThreadRegistration(Lifestyle lifestyle, Container container, 
                Func<TImplementation> instanceCreator = null)
                : base(lifestyle, container)
            {
                this.instanceCreator = instanceCreator;
            }

            public override Type ImplementationType => typeof(TImplementation);
            
            public override Expression BuildExpression()
            {
                if (this.instanceProducer == null)
                {
                    this.instanceProducer = this.BuildTransientInstanceCreator();
                }

                return Expression.Call(Expression.Constant(this), this.GetType().GetMethod("GetInstance"));
            }

            public TImplementation GetInstance()
            {
                TImplementation value = this.threadSpecificCache.Value;

                if (value == null)
                {
                    this.threadSpecificCache.Value = value = this.instanceProducer();
                }

                return value;
            }

            private Func<TImplementation> BuildTransientInstanceCreator() =>
                this.instanceCreator == null
                    ? (Func<TImplementation>)this.BuildTransientDelegate()
                    : this.BuildTransientDelegate(this.instanceCreator);
        }
    }
}