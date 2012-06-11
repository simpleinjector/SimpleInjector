namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using SimpleInjector.Advanced;

    // Mimics the constructor resolution behavior of Autofac and Unity.
    public class MostParametersConstructorResolutionBehavior : ConstructorResolutionBehavior
    {
        public override ConstructorInfo GetConstructor(Type type)
        {
            var constructors = GetConstructorsWithMostParameters(type);

            return constructors.Length == 1 ? constructors[0] : null;
        }

        protected override string BuildErrorMessageForTypeWithoutSuitableConstructor(Type type)
        {
            var constructors = GetConstructorsWithMostParameters(type);

            if (constructors.Length == 0)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "For the container to be able to create {0}, it should contain at least one " +
                    "public constructor.", type);
            }

            return string.Format(CultureInfo.InvariantCulture,
                "{0} contains multiple public constructors that contain {1} parameters, and because of " +
                "the container is unable to create it.", type, constructors[0].GetParameters().Length);
        }

        private static ConstructorInfo[] GetConstructorsWithMostParameters(Type type)
        {
            if (type.GetConstructors().Length == 0)
            {
                return new ConstructorInfo[0];
            }

            var maximumNumberOfParameters = (
                from constructor in type.GetConstructors()
                select constructor.GetParameters().Length)
                .Max();

            return (
                from constructor in type.GetConstructors()
                where constructor.GetParameters().Length == maximumNumberOfParameters
                select constructor)
                .ToArray();
        }
    }
}