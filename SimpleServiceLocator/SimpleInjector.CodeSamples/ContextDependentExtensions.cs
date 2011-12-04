namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=ContextDependentExtensions
    // NOTE: You need .NET 4.0 to be able to use this code.
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using SimpleInjector;

    public class DependencyContext
    {
        internal static readonly DependencyContext Root = new DependencyContext(null, null);

        internal DependencyContext(Type serviceType, Type implementationType)
        {
            this.ServiceType = serviceType;
            this.ImplementationType = implementationType;
        }

        public Type ServiceType { get; private set; }

        public Type ImplementationType { get; private set; }
    }

    public static class ContextDependentExtensions
    {
        public static void RegisterWithContext<TService>(this Container container,
            Func<DependencyContext, TService> contextBasedInstanceCreator)
            where TService : class
        {
            Func<TService> rootTypeInstanceCreator = () => contextBasedInstanceCreator(DependencyContext.Root);

            // Allow TService to be resolved when calling Container.GetInstance<TService>());
            container.Register<TService>(rootTypeInstanceCreator);

            // Allow the Func<DependencyContext, TService> to be injected into transient parent types.
            container.ExpressionBuilt += (sender, e) =>
            {
                var rewriter = new DependencyContextRewriter
                {
                    DelegateToReplace = rootTypeInstanceCreator,
                    ContextBasedInstanceCreator = contextBasedInstanceCreator,
                    ServiceType = e.RegisteredServiceType
                };

                e.Expression = rewriter.Visit(e.Expression);
            };
        }

        private sealed class DependencyContextRewriter : ExpressionVisitor
        {
            private readonly List<Expression> parents = new List<Expression>();
            
            public object DelegateToReplace { get; set; }

            public object ContextBasedInstanceCreator { get; set; }

            public Type ServiceType { get; set; }

            private Expression Parent
            {
                get { return this.parents.Count < 2 ? null : this.parents[this.parents.Count - 2]; }
            }

            public override Expression Visit(Expression node)
            {
                this.parents.Add(node);

                node = base.Visit(node);

                this.parents.RemoveAt(this.parents.Count - 1);

                return node;
            }

            protected override Expression VisitInvocation(InvocationExpression node)
            {
                bool isRootTypeContextRegistration = 
                    node.Expression.NodeType == ExpressionType.Constant &&
                    ((ConstantExpression)node.Expression).Value == this.DelegateToReplace;

                var parent = this.Parent as NewExpression;

                if (isRootTypeContextRegistration && parent != null)
                {
                    var context = new DependencyContext(this.ServiceType, parent.Type);

                    return Expression.Invoke(Expression.Constant(this.ContextBasedInstanceCreator),
                        Expression.Constant(context));
                }

                return base.VisitInvocation(node);
            }
        }
    }
}