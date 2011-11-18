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

        /// <summary>Gets the service type in which the current type is created or null when there is 
        /// no parent (will happen when the type is directly created using container.GetInstance).</summary>
        /// <value>A <see cref="Type"/>.</value>
        public Type ServiceType { get; private set; }

        /// <summary>Gets the actual implementation in which the current type is created or null when there is
        /// no parent (will happen when the type is directly created using container.GetInstance).</summary>
        /// <value>A <see cref="Type"/>.</value>
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
            // Allow TService to be resolved as root type (i.e. calls to container.GetInstance<TService>());
            container.ResolveUnregisteredType += (sender, e) =>
            {
                if (e.UnregisteredServiceType == typeof(TService))
                {
                    Func<object> rootInstanceCreator = 
                        () => contextBasedInstanceCreator(DependencyContext.Root);

                    e.Register(rootInstanceCreator);
                }
            };
        }

        private static void AllowServiceToBeResolvedAsDependency<TService>(Container container, 
            Func<DependencyContext, TService> contextBasedInstanceCreator) where TService : class
        {
            // Allow the Func<DependencyContext, TService> to be injected into transient parent types.
            container.ExpressionBuilt += (sender, e) =>
            {
                bool serviceIsADirectDependency =
                    ServiceIsADirectDependencyOfType(e.Expression, typeof(TService));

                if (serviceIsADirectDependency)
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
            ExpressionBuiltEventArgs e, Func<DependencyContext, TService> contextBasedInstanceCreator)
        {
            var expression = (NewExpression)e.Expression;

            var context = new DependencyContext(e.RegisteredServiceType, expression.Type);
            var contextExpression = Expression.Constant(context);

            var parameters = expression.Constructor.GetParameters();

            for (int index = 0; index < expression.Arguments.Count; index++)
            {
                if (parameters[index].ParameterType == typeof(TService))
                {
                    // Change the argument to an invocation of the instanceCreator with a context.
                    yield return
                        Expression.Invoke(Expression.Constant(contextBasedInstanceCreator), contextExpression);
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