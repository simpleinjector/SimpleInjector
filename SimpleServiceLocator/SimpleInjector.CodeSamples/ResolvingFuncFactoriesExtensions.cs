namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq.Expressions;

    public static class ResolvingFuncFactoriesExtensions
    {
        // This extension method is equivalent to the following registration, for each and every T:
        // container.RegisterSingle<Func<T>>(() => container.GetInstance<T>());
        // This is useful for consumers tht need to create multiple instances of a dependency.
        // This mimics the behavior of Autofac. In Autofac this behavior is the default.
        public static void AllowResolvingFuncFactories(this Container container)
        {
            container.ResolveUnregisteredType += (sender, e) =>
            {
                if (e.UnregisteredServiceType.IsGenericType &&
                    e.UnregisteredServiceType.GetGenericTypeDefinition() == typeof(Func<>))
                {
                    Type serviceType = e.UnregisteredServiceType.GetGenericArguments()[0];

                    var producer = container.GetRegistration(serviceType);

                    if (producer != null)
                    {
                        var func = typeof(ResolvingFuncFactoriesExtensions).GetMethod("BuildFactory")
                            .MakeGenericMethod(serviceType)
                            .Invoke(null, new[] { producer });

                        e.Register(() => func);
                    }
                }
            };
        }

        public static Func<T> BuildFactory<T>(IInstanceProducer producer)
        {
            var factoryExpression = Expression.Lambda<Func<T>>(producer.BuildExpression(),
                new ParameterExpression[0]);

            return factoryExpression.Compile();
        }
    }
}