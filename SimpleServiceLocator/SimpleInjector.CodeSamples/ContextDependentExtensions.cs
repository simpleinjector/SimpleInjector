namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=ContextDependentExtensions
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using SimpleInjector;

    [DebuggerDisplay("DependencyContext (ServiceType: {ServiceType}, ImplementationType: {ImplementationType})")]
    public class DependencyContext
    {
        internal static readonly DependencyContext Root = new DependencyContext();

        internal DependencyContext(Type serviceType, Type implementationType)
        {
            this.ServiceType = serviceType;
            this.ImplementationType = implementationType;
        }

        private DependencyContext()
        {
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
            // exactly control which expression is built.
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
                    Expression = e.Expression
                };

                e.Expression = rewriter.Visit(e.Expression);
            };
        }

        private sealed class DependencyContextRewriter : ExpressionVisitor
        {
            internal object ContextBasedFactory { get; set; }

            internal Type ServiceType { get; set; }

            internal Expression Expression { get; set; }

            internal Type ImplementationType
            {
                get 
                {
                    var expression = this.Expression as NewExpression;

                    if (expression != null)
                    {
                        return expression.Constructor.DeclaringType;
                    }

                    return this.ServiceType;
                }
            }

            protected override Expression VisitInvocation(
                InvocationExpression node)
            {
                if (!this.IsRootedContextBasedFactory(node))
                {
                    return node;
                }

                return Expression.Invoke(
                    Expression.Constant(this.ContextBasedFactory),
                    Expression.Constant(
                        new DependencyContext(
                            this.ServiceType, 
                            this.ImplementationType)));
            }

            private bool IsRootedContextBasedFactory(InvocationExpression node)
            {
                var expression = node.Expression as ConstantExpression;

                if (expression == null)
                {
                    return false;
                }

                if (!object.ReferenceEquals(expression.Value, this.ContextBasedFactory))
                {
                    return false;
                }

                var contextExpression = (ConstantExpression)node.Arguments[0];
                var context = (DependencyContext)contextExpression.Value;

                return context == DependencyContext.Root;
            }
        }
    }
}