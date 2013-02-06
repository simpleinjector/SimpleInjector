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

        internal DependencyContext(Type serviceType, Type implementationType, DependencyContext parent)
        {
            this.ServiceType = serviceType;
            this.ImplementationType = implementationType;
            this.Parent = parent;
        }

        private DependencyContext()
        {
        }

        public Type ServiceType { get; private set; }

        public Type ImplementationType { get; private set; }

        public DependencyContext Parent { get; private set; }

        internal DependencyContext AddParent(Type serviceType, Type implementationType)
        {
            if (this == Root)
            {
                return new DependencyContext(serviceType, implementationType, null);
            }

            return new DependencyContext(
                this.ServiceType, 
                this.ImplementationType,
                this.CreateNewParent(serviceType, implementationType));
        }

        private DependencyContext CreateNewParent(Type serviceType, Type implementationType)
        {
            if (this.Parent == null)
            {
                return new DependencyContext(serviceType, implementationType, null);
            }

            return this.Parent.AddParent(serviceType, implementationType);
        }
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
                    OriginalExpression = e.Expression
                };

                e.Expression = rewriter.Visit(e.Expression);
            };
        }

        private sealed class DependencyContextRewriter : ExpressionVisitor
        {
            public object ContextBasedFactory { get; set; }

            public Type ServiceType { get; set; }

            public Expression OriginalExpression { get; set; }

            private Type ImplementationType
            {
                get 
                {
                    var newExpression = this.OriginalExpression as NewExpression;

                    return newExpression != null ? newExpression.Constructor.DeclaringType : this.ServiceType;
                }
            }

            protected override Expression VisitInvocation(
                InvocationExpression node)
            {
                var expression = node.Expression as ConstantExpression;

                if (expression == null || !object.ReferenceEquals(expression.Value, this.ContextBasedFactory))
                {
                    return node;
                }

                var contextExpression = (ConstantExpression)node.Arguments[0];
                var context = (DependencyContext)contextExpression.Value;

                return Expression.Invoke(
                    Expression.Constant(this.ContextBasedFactory),
                    Expression.Constant(context.AddParent(this.ServiceType, this.ImplementationType)));
            }
        }
    }
}