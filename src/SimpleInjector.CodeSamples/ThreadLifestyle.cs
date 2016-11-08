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
    // Instead of using Per Thread lifestyle, use one of the 'scoped' lifestyles instead (that is Per Web 
    // Request, Per WCF Operation, and Lifetime Scope) or register a singleton and make sure that it is 
    // thread-safe.
    public sealed class ThreadLifestyle : Lifestyle
    {
        public static readonly Lifestyle Instance = new ThreadLifestyle();

        public ThreadLifestyle() : base("Thread")
        {
        }

        // Greater than Lifetime Scope, and WCF operation, but smaller than Singleton.
        public override int Length => Lifestyle.Singleton.Length - 1;

        protected override Registration CreateRegistrationCore<TService, TImplementation>(Container container)
        {
            return new PerThreadRegistration<TService, TImplementation>(this, container);
        }

        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container)
        {
            return new PerThreadRegistration<TService>(this, container)
            {
                InstanceCreator = instanceCreator
            };
        }

        private sealed class PerThreadRegistration<TService> : PerThreadRegistration<TService, TService>
            where TService : class
        {
            public PerThreadRegistration(Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
            }

            public Func<TService> InstanceCreator { get; set; }

            public override Func<TService> BuildTransientInstanceCreator()
            {
                return this.BuildTransientDelegate(this.InstanceCreator);
            }
        }

        private class PerThreadRegistration<TService, TImplementation> : Registration
            where TService : class
            where TImplementation : class, TService
        {
            private readonly ThreadLocal<TService> threadSpecificCache = new ThreadLocal<TService>();

            private Func<TService> instanceCreator;

            public PerThreadRegistration(Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
            }

            public override Type ImplementationType
            {
                get { return typeof(TImplementation); }
            }

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
                TService value = this.threadSpecificCache.Value;

                if (value == null)
                {
                    this.threadSpecificCache.Value = value = this.instanceCreator();
                }

                return value;
            }
        }
    }
}