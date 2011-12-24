namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=DecoratorExtensions
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class DecoratorContext
    {
        public Type ServiceType { get; set; }

        public Type ImplementationType { get; set; }

        public Type[] AppliedDecorators { get; set; }

        public Expression Expression { get; set; }
    }

    public static class DecoratorExtensions
    {
        public static void RegisterGenericDecorator(this Container container,
            Type openGenericType, Type openGenericDecorator)
        {
            Func<DecoratorContext, bool> always = c => true;

            DecoratorExtensions.RegisterGenericDecorator(container, openGenericType,
                openGenericDecorator, always);
        }
        
        public static void RegisterGenericDecorator(this Container container,
            Type openGenericType, Type openGenericDecorator, 
            Func<DecoratorContext, bool> predicate)
        {
            var interceptor = new DecoratorExpressionInterceptor
            {
                Container = container,
                OpenGenericType = openGenericType,
                OpenGenericDecorator = openGenericDecorator,
                Predicate = predicate
            };

            container.ExpressionBuilt += interceptor.Decorate;
        }

        private sealed class DecoratorExpressionInterceptor
        {
            [ThreadStatic]
            private static Dictionary<Container, 
                Dictionary<Type, ServiceTypeDecoratorInfo>> serviceTypeInfos;

            public Container Container { get; set; }

            public Type OpenGenericType { get; set; }

            public Type OpenGenericDecorator { get; set; }

            public Func<DecoratorContext, bool> Predicate { get; set; }

            public void Decorate(object sender, ExpressionBuiltEventArgs e)
            {
                var serviceType = e.RegisteredServiceType;

                if (serviceType.IsGenericType && 
                    serviceType.GetGenericTypeDefinition() == this.OpenGenericType &&
                    this.Predicate(this.CreatePredicateContext(e)))
                {
                    var closedGenericDecorator = this.OpenGenericDecorator
                        .MakeGenericType(serviceType.GetGenericArguments());

                    var ctor = closedGenericDecorator.GetConstructors().Single();

                    var parameters =
                        from parameter in ctor.GetParameters()
                        let type = parameter.ParameterType
                        select type == serviceType ? e.Expression :
                            this.Container.GetRegistration(type, true)
                                .BuildExpression();

                    var expression = Expression.New(ctor, parameters);

                    var info = this.GetServiceTypeInfo(e);
                    
                    info.AppliedDecorators.Add(closedGenericDecorator);

                    e.Expression = expression;
                }
            }
            
            private DecoratorContext CreatePredicateContext(ExpressionBuiltEventArgs e)
            {
                var info = this.GetServiceTypeInfo(e);

                return new DecoratorContext
                {
                    ServiceType = e.RegisteredServiceType,
                    Expression = e.Expression,
                    ImplementationType = info.ImplementationType,
                    AppliedDecorators = info.AppliedDecorators.ToArray()
                };
            }

            private ServiceTypeDecoratorInfo GetServiceTypeInfo(
                ExpressionBuiltEventArgs e)
            {
                var containerCache = serviceTypeInfos;

                if (containerCache == null)
                {
                    containerCache = new Dictionary<Container, Dictionary<Type, ServiceTypeDecoratorInfo>>();
                    serviceTypeInfos = containerCache;
                }

                if (!containerCache.ContainsKey(this.Container))
                {
                    containerCache[this.Container] = 
                        new Dictionary<Type, ServiceTypeDecoratorInfo>();
                }

                var cache = containerCache[this.Container];

                if (!cache.ContainsKey(e.RegisteredServiceType))
                {
                    cache[e.RegisteredServiceType] =
                        new ServiceTypeDecoratorInfo(DetermineImplementationType(e));
                }

                return cache[e.RegisteredServiceType];
            }

            private static Type DetermineImplementationType(ExpressionBuiltEventArgs e)
            {
                if (e.Expression is ConstantExpression)
                {
                    // Singleton
                    return ((ConstantExpression)e.Expression).Value.GetType();
                }

                if (e.Expression is NewExpression)
                {
                    // Transient without initializers.
                    return ((NewExpression)e.Expression).Constructor.DeclaringType;
                }

                var invocation = e.Expression as InvocationExpression;

                if (invocation != null && 
                    invocation.Expression is ConstantExpression &&
                    invocation.Arguments.Count == 1 && 
                    invocation.Arguments[0] is NewExpression)
                {
                    // Transient with initializers.
                    return ((NewExpression)invocation.Arguments[0])
                        .Constructor.DeclaringType;
                }

                // Implementation type can not be determined.
                return e.RegisteredServiceType;
            }

            private sealed class ServiceTypeDecoratorInfo
            {
                public ServiceTypeDecoratorInfo(Type implementationType)
                {
                    this.ImplementationType = implementationType;
                    this.AppliedDecorators = new List<Type>();
                }

                public Type ImplementationType { get; private set; }

                public List<Type> AppliedDecorators { get; private set; }
            }
        }
    }
}