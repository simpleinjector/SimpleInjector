namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using SimpleInjector.Advanced;

    // Mimics the constructor resolution behavior of Ninject, Castle Windsor and StructureMap.
    public class MostResolvableParametersConstructorResolutionBehavior : ConstructorResolutionBehavior
    {
        public override ConstructorInfo GetConstructor(Type type)
        {
            return (
                from constructor in type.GetConstructors()
                let parameters = constructor.GetParameters()
                orderby parameters.Length descending
                where this.IsRegistrationPhase ||
                    parameters.All(p => this.Container.GetRegistration(p.ParameterType) != null)
                select constructor)
                .FirstOrDefault();
        }

        protected override string BuildErrorMessageForTypeWithoutSuitableConstructor(Type type)
        {
            if (this.IsRegistrationPhase || !type.GetConstructors().Any())
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "For the container to be able to create {0}, it should contain at least one " +
                    "public constructor.", type);
            }

            return string.Format(CultureInfo.InvariantCulture,
                "For the container to be able to create {0}, it should contain a public constructor that " +
                "only contains parameters that can be resolved.", type);
        }
    }
}