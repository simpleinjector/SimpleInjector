namespace SimpleInjector.Interception
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    using SimpleInjector.Advanced;

    internal static class Helpers
    {
        internal static void ThrowArgumentExceptionWhenTypeIsNotConstructable(Container container, 
            Type implementationType, string parameterName)
        {
            string message;

            bool constructable = IsConstructableType(container, implementationType, out message);

            if (!constructable)
            {
                throw new ArgumentException(message, parameterName);
            }
        }
        
        internal static bool IsConstructableType(Container container, Type implementationType, 
            out string errorMessage)
        {
            errorMessage = null;

            try
            {
                var constructor = container.Options.ConstructorResolutionBehavior
                    .GetConstructor(implementationType, implementationType);

                container.Options.ConstructorVerificationBehavior.Verify(constructor);
            }
            catch (ActivationException ex)
            {
                errorMessage = ex.Message;
            }

            return errorMessage == null;
        }

        internal static void Verify(this IConstructorVerificationBehavior behavior, ConstructorInfo constructor)
        {
            foreach (var parameter in constructor.GetParameters())
            {
                behavior.Verify(parameter);
            }
        }

        internal static void AddRange<T>(this Collection<T> collection, IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                collection.Add(item);
            }
        }
    }
}