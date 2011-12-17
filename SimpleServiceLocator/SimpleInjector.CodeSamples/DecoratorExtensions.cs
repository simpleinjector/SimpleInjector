namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=DecoratorExtensions
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class DecoratorPredicateContext
    {
        public Type ServiceType { get; set; }

        public Type ImplementationType { get; set; }

        public Type[] AppliedDecorators { get; set; }

        public Expression Expression { get; set; }
    }

    public static class DecoratorExtensions
    {
        [ThreadStatic]
        private static Dictionary<Type, ServiceTypeDecoratorInfo> serviceTypeInfos;

        public static void RegisterOpenGenericDecorator(this Container container,
            Type openGenericType, Type openGenericDecorator)
        {
            DecoratorExtensions.RegisterOpenGenericDecorator(container, openGenericType, openGenericDecorator, 
                c => true);
        }

        public static void RegisterOpenGenericDecorator(this Container container,
            Type openGenericType, Type openGenericDecorator, Func<DecoratorPredicateContext, bool> predicate)
        {
            var helper = new DecorationHelper
            {
                Container = container,
                OpenGenericType = openGenericType,
                OpenGenericDecorator = openGenericDecorator,
                Predicate = predicate
            };

            container.ExpressionBuilt += helper.OnExpressionBuild;
        }

        private static DecoratorPredicateContext CreatePredicateContext(ExpressionBuiltEventArgs e)
        {
            var info = GetServiceTypeInfo(e);

            return new DecoratorPredicateContext
            {
                ServiceType = e.RegisteredServiceType,
                Expression = e.Expression,
                ImplementationType = info.ImplementationType,
                AppliedDecorators = info.AppliedDecorators.ToArray()
            };
        }

        private static ServiceTypeDecoratorInfo GetServiceTypeInfo(ExpressionBuiltEventArgs e)
        {
            var cache =
                serviceTypeInfos ?? (serviceTypeInfos = new Dictionary<Type, ServiceTypeDecoratorInfo>());

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

            if (invocation != null && invocation.Expression is ConstantExpression &&
                invocation.Arguments.Count == 1 && invocation.Arguments[0] is NewExpression)
            {
                // Transient with initializers.
                return ((NewExpression)invocation.Arguments[0]).Constructor.DeclaringType;
            }

            // Implementation type can not be determined.
            return e.RegisteredServiceType;
        }

        private sealed class DecorationHelper
        {
            public Container Container { get; set; }

            public Type OpenGenericType { get; set; }

            public Type OpenGenericDecorator { get; set; }

            public Func<DecoratorPredicateContext, bool> Predicate { get; set; }

            public void OnExpressionBuild(object sender, ExpressionBuiltEventArgs e)
            {
                var serviceType = e.RegisteredServiceType;

                if (serviceType.IsGenericType && 
                    serviceType.GetGenericTypeDefinition() == this.OpenGenericType &&
                    this.Predicate(CreatePredicateContext(e)))
                {
                    var closedGenericDecorator =
                        this.OpenGenericDecorator.MakeGenericType(serviceType.GetGenericArguments());

                    var ctor = closedGenericDecorator.GetConstructors().Single();

                    var expression = Expression.New(ctor,
                        from parameter in ctor.GetParameters()
                        let type = parameter.ParameterType
                        select type == serviceType ? e.Expression :
                            this.Container.GetRegistration(type, true).BuildExpression());

                    GetServiceTypeInfo(e).AppliedDecorators.Add(closedGenericDecorator);

                    e.Expression = expression;
                }
            }
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