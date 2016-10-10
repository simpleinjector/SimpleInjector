namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq.Expressions;
    using System.Threading;

    // https://simpleinjector.readthedocs.io/en/2.8/RuntimeDecorators.html
    public static class RuntimeDecoratorExtensions
    {
        public static void RegisterRuntimeDecorator(
            this Container container, Type serviceType, Type decoratorType,
            Predicate<DecoratorPredicateContext> runtimePredicate)
        {
            container.RegisterRuntimeDecorator(serviceType, decoratorType, null, runtimePredicate);
        }

        public static void RegisterRuntimeDecorator(
            this Container container, Type serviceType, Type decoratorType,
            Lifestyle lifestyle,
            Predicate<DecoratorPredicateContext> runtimePredicate,
            Predicate<DecoratorPredicateContext> compileTimePredicate = null)
        {
            var localContext = new ThreadLocal<DecoratorPredicateContext>();

            compileTimePredicate = compileTimePredicate ?? (context => true);

            Predicate<DecoratorPredicateContext> predicate = c =>
            {
                bool mustDecorate = compileTimePredicate(c);
                localContext.Value = mustDecorate ? c : null;
                return mustDecorate;
            };

            if (lifestyle == null)
            {
                container.RegisterDecorator(serviceType, decoratorType, predicate);
            }
            else
            {
                container.RegisterDecorator(serviceType, decoratorType, lifestyle, predicate);
            }

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