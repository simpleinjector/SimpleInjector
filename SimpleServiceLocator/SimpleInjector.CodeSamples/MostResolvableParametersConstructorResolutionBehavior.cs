namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/discussions/353520
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using SimpleInjector.Advanced;

    // Mimics the constructor resolution behavior of Ninject, Castle Windsor and StructureMap.
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
        public ConstructorInfo GetConstructor(Type serviceType, Type implementationType)
        {
            var constructor = this.GetConstructorOrNull(implementationType);

            if (constructor != null)
            {
                return constructor;
            }

            throw new ActivationException(this.BuildExceptionMessage(implementationType));
        }

        [DebuggerStepThrough]
        private ConstructorInfo GetConstructorOrNull(Type type)
        {
            // We prevent calling GetRegistration during the registration phase, because at this point not
            // all dependencies might be registered, and calling GetRegistration would lock the container,
            // making it impossible to do other registrations.
            return (
                from ctor in type.GetConstructors()
                let parameters = ctor.GetParameters()
                orderby parameters.Length descending
                where this.IsCalledDuringRegistrationPhase || parameters.All(this.CanBeResolved)
                select ctor)
                .FirstOrDefault();
        }

        [DebuggerStepThrough]
        private bool CanBeResolved(ParameterInfo parameter)
        {
            return this.container.GetRegistration(parameter.ParameterType) != null ||
                this.CanBuildParameterExpression(parameter);
        }

        [DebuggerStepThrough]
        private bool CanBuildParameterExpression(ParameterInfo parameter)
        {
            try
            {
                this.container.Options.ConstructorInjectionBehavior.BuildParameterExpression(parameter);
                return true;
            }
            catch (ActivationException)
            {
                return false;
            }
        }

        [DebuggerStepThrough]
        private string BuildExceptionMessage(Type type)
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