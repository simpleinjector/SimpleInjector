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
            options.ConstructorVerificationBehavior =
                new OptionalArgumentsVerificationBehavior(options.ConstructorVerificationBehavior);

            options.ConstructorInjectionBehavior =
                new OptionalArgumentsInjectionBehavior(options.Container, options.ConstructorInjectionBehavior);
        }
    }

    public class OptionalArgumentsVerificationBehavior : IConstructorVerificationBehavior
    {
        private readonly IConstructorVerificationBehavior original;

        public OptionalArgumentsVerificationBehavior(IConstructorVerificationBehavior original)
        {
            this.original = original;
        }

        [DebuggerStepThrough]
        public void Verify(ParameterInfo parameter)
        {
            if ((parameter.Attributes & ParameterAttributes.Optional) == 0)
            {
                this.original.Verify(parameter);
            }
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
        public Expression BuildParameterExpression(ParameterInfo parameter)
        {
            bool isOptional = (parameter.Attributes & ParameterAttributes.Optional) != 0;

            if (isOptional && !this.CanBeResolved(parameter))
            {
                return Expression.Constant(parameter.DefaultValue, parameter.ParameterType);
            }

            return this.original.BuildParameterExpression(parameter);
        }

        private bool CanBeResolved(ParameterInfo parameter)
        {
            return this.container.GetRegistration(parameter.ParameterType, false) != null;
        }
    }
}