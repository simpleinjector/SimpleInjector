namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using SimpleInjector.Extensions;

    public static class RuntimeDecoratorExtensions
    {
        public static void RegisterRuntimeDecorator(
            this Container container, Type serviceType, Type decoratorType,
            Func<bool> runtimePredicate)
        {
            container.RegisterRuntimeDecorator<Container>(
                serviceType, decoratorType,
                Lifestyle.Transient,
                context => true,
                context => runtimePredicate());
        }

        public static void RegisterRuntimeDecorator<TContextInfo>(
            this Container container, Type serviceType, Type decoratorType,
            Lifestyle lifestyle,
            Predicate<DecoratorPredicateContext> compileTimePredicate,
            Predicate<TContextInfo> runtimePredicate)
            where TContextInfo : class
        {
            var localContext = new ThreadLocal<DecoratorPredicateContext>();

            container.RegisterDecorator(serviceType, decoratorType, lifestyle, c =>
            {
                bool mustDecorate = compileTimePredicate(c);
                localContext.Value = mustDecorate ? c : null;
                return mustDecorate;
            });

            container.ExpressionBuilt += (s, e) =>
            {
                if (localContext.Value != null)
                {
                    Expression decoratee = localContext.Value.Expression;
                    Expression decorated = e.Expression;

                    localContext.Value = null;

                    var contextInfoRegistration =
                        container.GetRegistration(typeof(TContextInfo), true);

                    Expression shouldDecorate = Expression.Invoke(
                        Expression.Constant(runtimePredicate),
                        contextInfoRegistration.BuildExpression());

                    e.Expression = Expression.Condition(shouldDecorate,
                        Expression.Convert(decorated, e.RegisteredServiceType),
                        Expression.Convert(decoratee, e.RegisteredServiceType));
                }
            };
        }
    }
}