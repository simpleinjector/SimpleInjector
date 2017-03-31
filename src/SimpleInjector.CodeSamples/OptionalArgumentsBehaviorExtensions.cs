namespace SimpleInjector.CodeSamples
{
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Advanced;

    public static class OptionalArgumentsBehaviorExtensions
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

        public void Verify(InjectionConsumerInfo consumer)
        {
            if (consumer.Target.Parameter != null && !IsOptional(consumer.Target.Parameter))
            {
                this.original.Verify(consumer);
            }
        }

        public InstanceProducer GetInstanceProducer(InjectionConsumerInfo c, bool t) =>
            this.TryCreateInstanceProducer(c.Target.Parameter) ?? this.original.GetInstanceProducer(c, t);

        private InstanceProducer TryCreateInstanceProducer(ParameterInfo parameter) =>
            parameter != null && IsOptional(parameter) && !this.CanBeResolved(parameter)
                ? this.CreateConstantValueProducer(parameter)
                : null;

        private InstanceProducer CreateConstantValueProducer(ParameterInfo parameter) =>
            InstanceProducer.FromExpression(
                parameter.ParameterType, 
                Expression.Constant(parameter.DefaultValue, parameter.ParameterType), 
                this.container);

        private static bool IsOptional(ParameterInfo parameter) =>
            (parameter.Attributes & ParameterAttributes.Optional) != 0;

        private bool CanBeResolved(ParameterInfo parameter) =>
            this.container.GetRegistration(parameter.ParameterType, false) != null;
    }
}