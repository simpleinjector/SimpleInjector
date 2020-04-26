namespace SimpleInjector.CodeSamples
{
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Advanced;

    public static class OptionalArgumentsBehaviorExtensions
    {
        public static void EnableSkippingOptionalConstructorArguments(this ContainerOptions options)
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

        public bool VerifyDependency(InjectionConsumerInfo dependency, out string errorMessage)
        {
            if (IsOptional(dependency.Target.Parameter))
            {
                errorMessage = null;
                return true;
            }
            else
            {
                return this.original.VerifyDependency(dependency, out errorMessage);
            }
        }

        public InstanceProducer GetInstanceProducer(InjectionConsumerInfo d, bool t) =>
            this.TryCreateInstanceProducer(d.Target.Parameter) ?? this.original.GetInstanceProducer(d, t);

        private InstanceProducer TryCreateInstanceProducer(ParameterInfo parameter) =>
            IsOptional(parameter) && !this.CanBeResolved(parameter)
                ? this.CreateConstantValueProducer(parameter)
                : null;

        private InstanceProducer CreateConstantValueProducer(ParameterInfo parameter) =>
            InstanceProducer.FromExpression(
                parameter.ParameterType,
                Expression.Constant(parameter.DefaultValue, parameter.ParameterType),
                this.container);

        private static bool IsOptional(ParameterInfo parameter) =>
            parameter != null && (parameter.Attributes & ParameterAttributes.Optional) != 0;

        private bool CanBeResolved(ParameterInfo parameter) =>
            this.container.GetRegistration(parameter.ParameterType, false) != null;
    }
}