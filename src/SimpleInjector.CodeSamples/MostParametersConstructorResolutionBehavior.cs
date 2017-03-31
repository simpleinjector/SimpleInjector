namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using SimpleInjector.Advanced;

    // Mimics the constructor resolution behavior of Unity and StructureMap.
    // Register this as follows:
    // container.Options.ConstructorResolutionBehavior = new MostParametersConstructorResolutionBehavior();
    public class MostParametersConstructorResolutionBehavior : IConstructorResolutionBehavior
    {
        [DebuggerStepThrough]
        public ConstructorInfo GetConstructor(Type implementationType)
        {
            ConstructorInfo[] constructors = GetConstructorsWithMostParameters(implementationType);

            if (constructors.Length == 1)
            {
                return constructors[0];
            }

            string exceptionMessage = BuildExceptionMessage(implementationType, constructors);

            throw new ActivationException(exceptionMessage);
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

        [DebuggerStepThrough]
        private static string BuildExceptionMessage(Type type, ConstructorInfo[] constructors)
        {
            if (constructors.Length == 0)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "For the container to be able to create {0}, it should contain at least one " +
                    "public constructor.", type.ToFriendlyName());
            }

            return string.Format(CultureInfo.InvariantCulture,
                "{0} contains multiple public constructors that contain {1} parameters. " +
                "There can only be one public constructor with the highest number of parameters.",
                type, constructors[0].GetParameters().Length);
        }
    }
}