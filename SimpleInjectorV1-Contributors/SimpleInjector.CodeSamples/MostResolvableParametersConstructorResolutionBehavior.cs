namespace SimpleInjector.CodeSamples
{
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

        private ConstructorInfo GetConstructorOrNull(Type type)
        {
            // We prevent calling GetRegistration during the registration phase, because at this point not
            // all dependencies might be registered, and calling GetRegistration would lock the container,
            // making it impossible to do other registrations.
            return (
                from ctor in type.GetConstructors()
                let parameters = ctor.GetParameters()
                orderby parameters.Length descending
                where this.IsCalledDuringRegistrationPhase ||
                    parameters.All(p => this.container.GetRegistration(p.ParameterType) != null)
                select ctor)
                .FirstOrDefault();
        }

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