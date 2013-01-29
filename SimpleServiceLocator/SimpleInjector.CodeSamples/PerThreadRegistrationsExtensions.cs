namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=PerThreadExtensionMethod
    using System;
    using System.Linq.Expressions;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;
    
    /// <summary>
    /// Extension methods for registering types on a thread-static basis.
    /// </summary>
    public static partial class PerThreadRegistrationsExtensions
    {
        public static void RegisterPerThread<TService, TImplementation>(
            this Container container)
            where TService : class
            where TImplementation : class, TService
        {
            container.Register<TService, TImplementation>(ThreadLifestyle.Instance);
        }

        public static void RegisterPerThread<TService>(this Container container,
            Func<TService> instanceCreator) where TService : class
        {
            container.Register<TService>(instanceCreator, ThreadLifestyle.Instance);
        }
    }

    // There is no support for a Thread lifestyle in the core library, because this lifestyle is considered
    // harmful. It should not be used in web applications, because ASP.NET can finish a request on a different
    // thread. This can cause a Per Thread instance to be used from another thread, which can cause all sorts 
    // of race conditions. Instead of using Per Thread lifestyle, use one of the 'scoped' lifestyles instead 
    // (that is Per Web Request, Per WCF Operation, and Lifetime Scope) or register a singleton and make sure 
    // that it is thread-safe.
    public sealed class ThreadLifestyle : Lifestyle
    {
        public static readonly Lifestyle Instance = new ThreadLifestyle();

        private ThreadLifestyle() : base("Thread")
        {
        }

        protected override int Length
        {
            // Greater than Lifetime Scope, and WCF operation, but smaller than Singleton.
            get { return 500; }
        }

        public override Registration CreateRegistration<TService, TImplementation>(Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            return new PerThreadRegistration<TService, TImplementation>(this, container);
        }

        public override Registration CreateRegistration<TService>(Func<TService> instanceCreator, 
            Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

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
            private Func<TService> instanceCreator;

            [ThreadStatic]
            private static TService instance;

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
                var value = instance;

                if (value == null)
                {
                    instance = value = this.instanceCreator();
                }

                return value;
            }
        }
    }
}