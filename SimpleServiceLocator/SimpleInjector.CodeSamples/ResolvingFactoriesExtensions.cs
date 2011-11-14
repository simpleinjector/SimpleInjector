namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq.Expressions;

    public static class ResolvingFactoriesExtensions
    {
        public interface ILazyBuilder
        {
            object NewLazy();
        }
        
        // This extension method is equivalent to the following registration, for each and every T:
        // container.Register<Lazy<T>>(() => new Lazy<T>(() => container.GetInstance<T>()));
        // This is useful for consumers that have a dependency on a service that is expensive to create, but
        // not always needed.
        // This mimics the behavior of Autofac. In Autofac this behavior is default.
        public static void AllowResolvingLazyFactories(this Container container)
        {
            container.ResolveUnregisteredType += (sender, e) =>
            {
                if (e.UnregisteredServiceType.IsGenericType &&
                    e.UnregisteredServiceType.GetGenericTypeDefinition() == typeof(Lazy<>))
                {
                    Type serviceType = e.UnregisteredServiceType.GetGenericArguments()[0];

                    var producer = container.GetRegistration(serviceType);

                    if (producer != null)
                    {
                        var lazyBuilder = Activator.CreateInstance(
                            typeof(LazyBuilder<>).MakeGenericType(serviceType), producer) as ILazyBuilder;

                        e.Register(() => lazyBuilder.NewLazy());
                    }
                }
            };
        }

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

                    var producer = container.GetRegistration(serviceType);

                    if (producer != null)
                    {
                        var func = typeof(ResolvingFactoriesExtensions).GetMethod("BuildFactory")
                            .MakeGenericMethod(serviceType)
                            .Invoke(null, new[] { producer });

                        e.Register(() => func);
                    }
                }
            };
        }

        public static Lazy<T> BuildLazy<T>(InstanceProducer producer)
        {
            return new Lazy<T>(BuildFactory<T>(producer));
        }

        public static Func<T> BuildFactory<T>(InstanceProducer producer)
        {
            var factoryExpression = Expression.Lambda<Func<T>>(producer.BuildExpression(),
                new ParameterExpression[0]);

            return factoryExpression.Compile();
        }

        public class LazyBuilder<T> : ILazyBuilder
        {
            private readonly Func<T> producer;

            public LazyBuilder(InstanceProducer producer)
            {
                this.producer = BuildFactory<T>(producer);
            }

            public object NewLazy()
            {
                return new Lazy<T>(this.producer);
            }
        }
    }
}