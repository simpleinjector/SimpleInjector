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
            Func<DecoratorPredicateContext, bool> runtimePredicate)
        {
            var localContext = new ThreadLocal<DecoratorPredicateContext>();

            container.RegisterDecorator(serviceType, decoratorType, c =>
            {
                localContext.Value = c;
                return true;
            });

            container.ExpressionBuilt += (s, e) =>
            {
                if (localContext.Value != null)
                {
                    DecoratorPredicateContext context = localContext.Value;
                    Expression undecorated = context.Expression;
                    Expression decorated = e.Expression;

                    localContext.Value = null;

                    Expression shouldDecorate = Expression.Invoke(
                        Expression.Constant(runtimePredicate),
                        Expression.Constant(context));

                    Type type = e.RegisteredServiceType;

                    e.Expression = Expression.Condition(shouldDecorate,
                        Expression.Convert(decorated, type),
                        Expression.Convert(undecorated, type));
                }
            };
        }

        public static void RegisterRuntimeDecorator<TContextInfo>(
            this Container container, Type serviceType, Type decoratorType,
            Lifestyle lifestyle,
            Func<DecoratorPredicateContext, bool> compileTimePredicate,
            Func<DecoratorPredicateContext, TContextInfo, bool> runtimePredicate)
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
                    DecoratorPredicateContext context = localContext.Value;
                    Expression undecorated = context.Expression;
                    Expression decorated = e.Expression;

                    localContext.Value = null;

                    var contextInfoRegistration =
                        container.GetRegistration(typeof(TContextInfo), true);

                    Expression shouldDecorate = Expression.Invoke(
                        Expression.Constant(runtimePredicate),
                        Expression.Constant(context),
                        contextInfoRegistration.BuildExpression());

                    Type type = e.RegisteredServiceType;

                    e.Expression = Expression.Condition(shouldDecorate,
                        Expression.Convert(decorated, type),
                        Expression.Convert(undecorated, type));
                }
            };
        }
    }
}