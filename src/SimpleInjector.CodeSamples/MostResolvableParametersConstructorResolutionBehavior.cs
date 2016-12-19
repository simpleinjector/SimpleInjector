namespace SimpleInjector.CodeSamples
{
    // https://simpleinjector.readthedocs.io/en/latest/extensibility.html#overriding-constructor-resolution-behavior
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using SimpleInjector.Advanced;

    // Mimics the constructor resolution behavior of Autofac, Ninject and Castle Windsor.
    // Register this as follows:
    // container.Options.ConstructorResolutionBehavior =
    //     new MostResolvableParametersConstructorResolutionBehavior(container);
    public class MostResolvableParametersConstructorResolutionBehavior : IConstructorResolutionBehavior
    {
        private readonly Container container;

        public MostResolvableParametersConstructorResolutionBehavior(Container container)
        {
            this.container = container;
        }

        private bool IsCalledDuringRegistrationPhase
        {
            [DebuggerStepThrough]
            get { return !this.container.IsLocked(); }
        }

        [DebuggerStepThrough]
        public ConstructorInfo GetConstructor(Type implementationType)
        {
            var constructor = this.GetConstructors(implementationType).FirstOrDefault();

            if (constructor != null)
            {
                return constructor;
            }

            throw new ActivationException(BuildExceptionMessage(implementationType));
        }

        [DebuggerStepThrough]
        private IEnumerable<ConstructorInfo> GetConstructors(Type implementation)
        {
            var constructors = implementation.GetConstructors();

            // We prevent calling GetRegistration during the registration phase, because at this point not
            // all dependencies might be registered, and calling GetRegistration would lock the container,
            // making it impossible to do other registrations.
            return
                from ctor in constructors
                let parameters = ctor.GetParameters()
                where this.IsCalledDuringRegistrationPhase
                    || constructors.Length == 1
                    || ctor.GetParameters().All(p => this.CanBeResolved(p, implementation))
                orderby parameters.Length descending
                select ctor;
        }

        [DebuggerStepThrough]
        private bool CanBeResolved(ParameterInfo parameter, Type implementationType)
        {
            return this.container.GetRegistration(parameter.ParameterType) != null ||
                this.CanBuildExpression(implementationType, parameter);
        }

        [DebuggerStepThrough]
        private bool CanBuildExpression(Type implementationType, ParameterInfo parameter)
        {
            try
            {
                this.container.Options.DependencyInjectionBehavior.BuildExpression(
                    new InjectionConsumerInfo(implementationType, implementationType, parameter));

                return true;
            }
            catch (ActivationException)
            {
                return false;
            }
        }

        [DebuggerStepThrough]
        private static string BuildExceptionMessage(Type type)
        {
            if (!type.GetConstructors().Any())
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "For the container to be able to create {0}, it should contain at least one public " +
                    "constructor.", type);
            }

            return string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to create {0}, it should contain a public constructor that " +
                "only contains parameters that can be resolved.", type);
        }
    }
}