namespace SimpleInjector.CodeSamples
{
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Advanced;

    public class OptionalArgumentsBehaviorExtensions
    {
        public static void EnableSkippingOptionalConstructorArguments(ContainerOptions options)
        {
            options.DependencyInjectionBehavior =
                new OptionalArgumentsInjectionBehavior(options.Container, options.DependencyInjectionBehavior);
        }
    }

    public class OptionalArgumentsInjectionBehavior : IDependencyInjectionBehavior
    {
        private readonly Container container;
        private readonly IDependencyInjectionBehavior original;

        public OptionalArgumentsInjectionBehavior(Container container, IDependencyInjectionBehavior original)
        {
            this.container = container;
            this.original = original;
        }

        [DebuggerStepThrough]
        public void Verify(InjectionConsumerInfo consumer)
        {
            var parameter = consumer.Target.Parameter;

            if (parameter != null && !IsOptional(parameter))
            {
                this.original.Verify(consumer);
            }
        }

        [DebuggerStepThrough]
        public Expression BuildParameterExpression(InjectionConsumerInfo consumer)
        {
            var parameter = consumer.Target.Parameter;

            if (parameter != null && IsOptional(parameter) && !this.CanBeResolved(parameter))
            {
                return Expression.Constant(parameter.DefaultValue, parameter.ParameterType);
            }

            return this.original.BuildParameterExpression(consumer);
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