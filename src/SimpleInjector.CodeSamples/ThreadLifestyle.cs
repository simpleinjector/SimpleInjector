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

        protected override Registration CreateRegistrationCore(Type concreteType, Container container)
        {
            return new PerThreadRegistration(this, container, concreteType);
        }

        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container)
        {
            if (instanceCreator is null)
            {
                throw new ArgumentNullException(nameof(instanceCreator));
            }

            return new PerThreadRegistration<TService>(this, container, instanceCreator);
        }

        private class PerThreadRegistration : Registration
        {
            private readonly ThreadLocal<object> threadSpecificCache = new ThreadLocal<object>();

            private Func<object> instanceProducer;

            public PerThreadRegistration(Lifestyle lifestyle, Container container, Type implementationType)
                : base(lifestyle, container)
            {
                this.ImplementationType = implementationType;
            }

            public override Type ImplementationType { get; }
            
            public override Expression BuildExpression()
            {
                if (this.instanceProducer is null)
                {
                    this.instanceProducer = this.BuildTransientInstanceCreator();
                }

                return Expression.Call(
                    Expression.Constant(this),
                    this.GetType().GetMethod(nameof(GetInstance)).MakeGenericMethod(this.ImplementationType));
            }

            public TImplementation GetInstance<TImplementation>()
            {
                object value = this.threadSpecificCache.Value;

                if (value is null)
                {
                    this.threadSpecificCache.Value = value = this.instanceProducer();
                }

                return (TImplementation)value;
            }

            protected virtual Func<object> BuildTransientInstanceCreator() => this.BuildTransientDelegate();
        }

        private sealed class PerThreadRegistration<TImplementation> : PerThreadRegistration
            where TImplementation : class
        {
            private readonly Func<TImplementation> instanceCreator;

            public PerThreadRegistration(
                Lifestyle lifestyle, Container container, Func<TImplementation> instanceCreator)
                : base(lifestyle, container, typeof(TImplementation))
            {
                this.instanceCreator = instanceCreator;
            }

            protected override Func<object> BuildTransientInstanceCreator() =>
                this.BuildTransientDelegate(this.instanceCreator);
        }
    }
}