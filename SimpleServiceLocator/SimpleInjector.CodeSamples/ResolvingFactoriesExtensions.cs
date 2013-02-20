namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public static class ResolvingFactoriesExtensions
    {
        // This extension method is equivalent to the following registration, for each and every T:
        // container.RegisterSingle<Func<T>>(() => container.GetInstance<T>());
        // This is useful for consumers that need to create multiple instances of a dependency.
        // This mimics the behavior of Autofac. In Autofac this behavior is default.
        public static void AllowResolvingFuncFactories(this Container container)
        {
            container.ResolveUnregisteredType += (sender, e) =>
            {
                if (e.UnregisteredServiceType.IsGenericType &&
                    e.UnregisteredServiceType.GetGenericTypeDefinition() == typeof(Func<>))
                {
                    Type serviceType = e.UnregisteredServiceType.GetGenericArguments()[0];

                    InstanceProducer registration = container.GetRegistration(serviceType);

                    if (registration != null)
                    {
                        Type funcType = typeof(Func<>).MakeGenericType(serviceType);

                        var factoryDelegate = 
                            Expression.Lambda(funcType, registration.BuildExpression()).Compile();

                        e.Register(Expression.Constant(factoryDelegate));
                    }
                }
            };
        }

        // This extension method is equivalent to the following registration, for each and every T:
        // container.Register<Lazy<T>>(() => new Lazy<T>(() => container.GetInstance<T>()));
        // This is useful for consumers that have a dependency on a service that is expensive to create, but
        // not always needed.
        // This mimics the behavior of Autofac and Ninject 3. In Autofac this behavior is default.
        public static void AllowResolvingLazyFactories(this Container container)
        {
            container.ResolveUnregisteredType += (sender, e) =>
            {
                if (e.UnregisteredServiceType.IsGenericType &&
                    e.UnregisteredServiceType.GetGenericTypeDefinition() == typeof(Lazy<>))
                {
                    Type serviceType = e.UnregisteredServiceType.GetGenericArguments()[0];

                    InstanceProducer registration = container.GetRegistration(serviceType);

                    if (registration != null)
                    {
                        Type funcType = typeof(Func<>).MakeGenericType(serviceType);
                        Type lazyType = typeof(Lazy<>).MakeGenericType(serviceType);

                        var factoryDelegate = 
                            Expression.Lambda(funcType, registration.BuildExpression()).Compile();
                        
                        var lazyConstructor = (
                            from ctor in lazyType.GetConstructors()
                            where ctor.GetParameters().Length == 1
                            where ctor.GetParameters()[0].ParameterType == funcType
                            select ctor)
                            .Single();

                        e.Register(Expression.New(lazyConstructor, Expression.Constant(factoryDelegate)));
                    }
                }
            };
        }
    }
}