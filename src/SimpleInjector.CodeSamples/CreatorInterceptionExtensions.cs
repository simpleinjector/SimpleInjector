namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq.Expressions;

    public static class CreatorInterceptionExtensions
    {
        public static void ApplyInterceptor(
            this ContainerOptions options,
            Func<Func<object>, object> interceptor,
            Predicate<Type> predicate = null)
        {
            var container = options.Container;

            container.ExpressionBuilding += (s, e) =>
            {
                if (predicate != null && !predicate(e.KnownImplementationType)) return;

                Delegate factory = Expression.Lambda(typeof(Func<object>), e.Expression).Compile();

                e.Expression = Expression.Convert(
                    Expression.Invoke(
                        Expression.Constant(interceptor),
                        Expression.Constant(factory)),
                    e.Expression.Type);

            };
        }
    }
}