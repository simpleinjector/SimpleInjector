namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Advanced;

    public class OptionalArgumentsBehaviorExtensions
    {
        public static void EnableSkippingOptionalConstructorArguments(ContainerOptions options)
        {
            options.ConstructorInjectionBehavior =
                new OptionalArgumentsInjectionBehavior(options.Container, options.ConstructorInjectionBehavior);
        }
    }

    public class OptionalArgumentsInjectionBehavior : IConstructorInjectionBehavior
    {
        private readonly Container container;
        private readonly IConstructorInjectionBehavior original;

        public OptionalArgumentsInjectionBehavior(Container container, IConstructorInjectionBehavior original)
        {
            this.container = container;
            this.original = original;
        }

        [DebuggerStepThrough]
        public Expression BuildParameterExpression(Type serviceType, Type implementationType, 
            ParameterInfo parameter)
        {
            if (IsOptional(parameter) && !this.CanBeResolved(parameter))
            {
                return Expression.Constant(parameter.DefaultValue, parameter.ParameterType);
            }

            return this.original.BuildParameterExpression(serviceType, implementationType, parameter);
        }

        [DebuggerStepThrough]
        public void Verify(ParameterInfo parameter)
        {
            if (!IsOptional(parameter))
            {
                this.original.Verify(parameter);
            }
        }

        private static bool IsOptional(ParameterInfo parameter)
        {
            return (parameter.Attributes & ParameterAttributes.Optional) != 0;
        }

        private bool CanBeResolved(ParameterInfo parameter)
        {
            return this.container.GetRegistration(parameter.ParameterType, false) != null;
        }
    }
}