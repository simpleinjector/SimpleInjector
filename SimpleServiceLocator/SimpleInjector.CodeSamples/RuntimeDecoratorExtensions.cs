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
            Predicate<DecoratorPredicateContext> runtimePredicate)
        {
            container.RegisterRuntimeDecorator(
                serviceType, decoratorType,
                Lifestyle.Transient,
                runtimePredicate);
        }

        public static void RegisterRuntimeDecorator(
            this Container container, Type serviceType, Type decoratorType,
            Lifestyle lifestyle,
            Predicate<DecoratorPredicateContext> runtimePredicate,
            Predicate<DecoratorPredicateContext> compileTimePredicate = null)
        {
            var localContext = new ThreadLocal<DecoratorPredicateContext>();

            compileTimePredicate = compileTimePredicate ?? (context => true);

            container.RegisterDecorator(serviceType, decoratorType, lifestyle, c =>
            {
                bool mustDecorate = compileTimePredicate(c);
                localContext.Value = mustDecorate ? c : null;
                return mustDecorate;
            });

            container.ExpressionBuilt += (s, e) =>
            {
                bool isDecorated = localContext.Value != null;

                if (isDecorated)
                {
                    Expression decorator = e.Expression;
                    Expression original = localContext.Value.Expression;

                    Expression shouldDecorate = Expression.Invoke(
                        Expression.Constant(runtimePredicate),
                        Expression.Constant(localContext.Value));

                    e.Expression = Expression.Condition(shouldDecorate,
                        Expression.Convert(decorator, e.RegisteredServiceType),
                        Expression.Convert(original, e.RegisteredServiceType));

                    localContext.Value = null;
                }
            };
        }
    }
}