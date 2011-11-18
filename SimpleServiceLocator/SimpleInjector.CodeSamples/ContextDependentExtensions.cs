namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;

    using SimpleInjector;

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
        public static void RegisterWithContext<TService>(this Container container,
            Func<DependencyContext, TService> contextBasedInstanceCreator)
            where TService : class
        {
            AllowServiceToBeResolvedAsRootType<TService>(container, contextBasedInstanceCreator);
            AllowServiceToBeResolvedAsDependency<TService>(container, contextBasedInstanceCreator);
        }

        private static void AllowServiceToBeResolvedAsRootType<TService>(Container container, 
            Func<DependencyContext, TService> contextBasedInstanceCreator) where TService : class
        {
            // Allow TService to be resolved when calling Container.GetInstance<TService>());
            container.Register<TService>(() => contextBasedInstanceCreator(DependencyContext.Root));
        }

        private static void AllowServiceToBeResolvedAsDependency<TService>(Container container, 
            Func<DependencyContext, TService> contextBasedInstanceCreator) where TService : class
        {
            // Allow the Func<DependencyContext, TService> to be injected into transient parent types.
            container.ExpressionBuilt += (sender, e) =>
            {
                bool serviceIsDependency = ServiceIsADirectDependencyOfType(e.Expression, typeof(TService));

                if (serviceIsDependency)
                {
                    var expression = (NewExpression)e.Expression;

                    var arguments = BuildNewListOfConstructorArguments(e, contextBasedInstanceCreator);

                    e.Expression = Expression.New(expression.Constructor, arguments.ToArray());
                }
            };
        }

        private static bool ServiceIsADirectDependencyOfType(Expression expression, Type serviceType)
        {
            var newExpression = expression as NewExpression;

            // Only constructor calls can be intercepted.
            if (newExpression == null)
            {
                return false;
            }

            var parameters = newExpression.Constructor.GetParameters();

            bool constructorContainsService = parameters.Any(p => p.ParameterType == serviceType);

            return constructorContainsService;
        }

        private static IEnumerable<Expression> BuildNewListOfConstructorArguments<TService>(
            ExpressionBuiltEventArgs e, Func<DependencyContext, TService> instanceCreator)
        {
            var expression = (NewExpression)e.Expression;

            var context = new DependencyContext(e.RegisteredServiceType, expression.Type);

            var parameters = expression.Constructor.GetParameters();

            for (int index = 0; index < expression.Arguments.Count; index++)
            {
                if (parameters[index].ParameterType == typeof(TService))
                {
                    // Change the argument to an invocation of the instanceCreator with a context.
                    yield return Expression.Invoke(Expression.Constant(instanceCreator), 
                        Expression.Constant(context));
                }
                else
                {
                    // The original argument
                    yield return expression.Arguments[index];
                }
            }
        }
    }
}