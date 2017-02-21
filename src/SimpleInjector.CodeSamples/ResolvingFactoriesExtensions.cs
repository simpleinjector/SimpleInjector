namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class ResolvingFactoriesExtensions
    {
        // This extension method is equivalent to the following registration, for each and every T:
        // container.RegisterSingleton<Func<T>>(() => container.GetInstance<T>());
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

                    InstanceProducer registration = container.GetRegistration(serviceType, true);

                    Type funcType = typeof(Func<>).MakeGenericType(serviceType);

                    var factoryDelegate = 
                        Expression.Lambda(funcType, registration.BuildExpression()).Compile();

                    e.Register(Expression.Constant(factoryDelegate));
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

                    InstanceProducer registration = container.GetRegistration(serviceType, true);

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
            };
        }

        public static void AllowResolvingParameterizedFuncFactories(this Container container)
        {
            container.ResolveUnregisteredType += (sender, e) =>
            {
                if (!IsParameterizedFuncDelegate(e.UnregisteredServiceType))
                {
                    return;
                }

                Type[] genericArguments = e.UnregisteredServiceType.GetGenericArguments();

                Type componentType = genericArguments.Last();

                if (componentType.IsAbstract)
                {
                    return;
                }

                var funcType = e.UnregisteredServiceType;

                var factoryArguments = genericArguments.Take(genericArguments.Length - 1).ToArray();

                var constructor = container.Options.ConstructorResolutionBehavior
                    .GetConstructor(componentType);

                var parameters = (
                    from factoryArgumentType in factoryArguments
                    select Expression.Parameter(factoryArgumentType))
                    .ToArray();

                var factoryDelegate = Expression.Lambda(funcType,
                    BuildNewExpression(container, constructor, parameters),
                    parameters)
                    .Compile();

                e.Register(Expression.Constant(factoryDelegate));
            };
        }

        private static bool IsParameterizedFuncDelegate(Type type)
        {
            if (!type.IsGenericType || !type.FullName.StartsWith("System.Func`"))
            {
                return false;
            }

            return type.GetGenericTypeDefinition().GetGenericArguments().Length > 1;
        }

        private static NewExpression BuildNewExpression(Container container, 
            ConstructorInfo constructor, 
            ParameterExpression[] funcParameterExpression)
        {
            var ctorParameters = constructor.GetParameters();
            var ctorParameterTypes = ctorParameters.Select(p => p.ParameterType).ToArray();
            var funcParameterTypes = funcParameterExpression.Select(p => p.Type).ToArray();

            int funcParametersIndex = IndexOfSubCollection(ctorParameterTypes, funcParameterTypes);

            if (funcParametersIndex == -1)
            {
                throw new ActivationException(string.Format(CultureInfo.CurrentCulture,
                    "The constructor of type {0} did not contain the sequence of the following " +
                    "constructor parameters: {1}.",
                    constructor.DeclaringType.ToFriendlyName(),
                    string.Join(", ", funcParameterTypes.Select(t => t.ToFriendlyName()))));
            }

            var firstCtorParameterExpressions = ctorParameterTypes
                .Take(funcParametersIndex)
                .Select(type => container.GetRegistration(type, true).BuildExpression());

            var lastCtorParameterExpressions = ctorParameterTypes
                .Skip(funcParametersIndex + funcParameterTypes.Length)
                .Select(type => container.GetRegistration(type, true).BuildExpression());

            var expressions = firstCtorParameterExpressions
                .Concat(funcParameterExpression)
                .Concat(lastCtorParameterExpressions)
                .ToArray();

            return Expression.New(constructor, expressions);
        }

        private static int IndexOfSubCollection(Type[] collection, Type[] subCollection)
        {
            return (
                from index in Enumerable.Range(0, collection.Length - subCollection.Length + 1)
                let collectionPart = collection.Skip(index).Take(subCollection.Length)
                where collectionPart.SequenceEqual(subCollection)
                select (int?)index)
                .FirstOrDefault() ?? -1;
        }
    }
}