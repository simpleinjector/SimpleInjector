namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=ContextDependentExtensions
    // NOTE: You need .NET 4.0 to be able to use this code.
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using SimpleInjector;

    [DebuggerDisplay("DependencyContext (ServiceType: {ServiceType}, ImplementationType: {ImplementationType})")]
    public class DependencyContext
    {
        internal static readonly DependencyContext Root =
            new DependencyContext(null, null);

        internal DependencyContext(Type serviceType,
            Type implementationType)
        {
            this.ServiceType = serviceType;
            this.ImplementationType = implementationType;
        }

        public Type ServiceType { get; private set; }

        public Type ImplementationType { get; private set; }
    }

    public static class ContextDependentExtensions
    {
        public static void RegisterWithContext<TService>(
            this Container container,
            Func<DependencyContext, TService> contextBasedFactory)
            where TService : class
        {
            if (contextBasedFactory == null)
            {
                throw new ArgumentNullException("contextBasedFactory");
            }

            // By using the ResolveUnregisteredType event we can
            // exactly control which which expression is built.
            container.ResolveUnregisteredType += (sender, e) =>
            {
                if (e.UnregisteredServiceType == typeof(TService))
                {
                    // () => contextBasedFactory(DependencyContext.Root)
                    var expression = Expression.Invoke(
                        Expression.Constant(contextBasedFactory),
                        Expression.Constant(DependencyContext.Root));

                    e.Register(expression);
                }
            };

            // Allow the Func<DependencyContext, TService> to be 
            // injected into transient parent types.
            container.ExpressionBuilding += (sender, e) =>
            {
                var rewriter = new DependencyContextRewriter
                {
                    ContextBasedFactory = contextBasedFactory,
                    ServiceType = e.RegisteredServiceType,
                    OriginalExpression = e.Expression
                };

                e.Expression = rewriter.Visit(e.Expression);
            };
        }

        private sealed class DependencyContextRewriter
            : ExpressionVisitor
        {
            private readonly List<Expression> parents =
                new List<Expression>();

            public object ContextBasedFactory { get; set; }

            public Type ServiceType { get; set; }

            public Expression OriginalExpression { get; set; }

            protected override Expression VisitInvocation(
                InvocationExpression node)
            {
                if (this.IsRootTypeContextRegistration(node))
                {
                    var parent = this.OriginalExpression as NewExpression;

                    var context = new DependencyContext(
                        this.ServiceType, 
                        parent != null ? parent.Type : this.ServiceType);

                    return Expression.Invoke(
                        Expression.Constant(this.ContextBasedFactory),
                        Expression.Constant(context));
                }

                return base.VisitInvocation(node);
            }

            private bool IsRootTypeContextRegistration(
                InvocationExpression node)
            {
                if (!(node.Expression is ConstantExpression) ||
                    node.Arguments.Count != 1 ||
                    !(node.Arguments[0] is ConstantExpression))
                {
                    return false;
                }

                var target =
                    ((ConstantExpression)node.Expression).Value;

                var value =
                    ((ConstantExpression)node.Arguments[0]).Value;

                return target == this.ContextBasedFactory &&
                    value == DependencyContext.Root;
            }
        }
    }
}