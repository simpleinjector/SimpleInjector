// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Decorators
{
    using System;
    using System.Linq;
    using System.Reflection;

    using SimpleInjector.Internals;

    /// <summary>Helper methods for working with decorators.</summary>
    public static class DecoratorHelpers
    {
        /// <summary>
        /// Returns true when the supplied <paramref name="constructor"/> contains parameters that makes the
        /// constructor's type a decorator for the <paramref name="serviceType"/> and can be used by Simple
        /// Injector as a decorator.
        /// </summary>
        /// <param name="serviceType">The service type the decorator should be wrapped around.</param>
        /// <param name="constructor">The constructor used by Simple Injector.</param>
        /// <returns>True when decorator; false otherwise.</returns>
        public static bool IsDecorator(Type serviceType, ConstructorInfo constructor)
        {
            int numberOfServiceTypeDependencies =
                GetNumberOfServiceTypeDependencies(serviceType, constructor);

            return numberOfServiceTypeDependencies == 1;
        }

        internal static int GetNumberOfServiceTypeDependencies(
            Type serviceType, ConstructorInfo decoratorConstructor)
        {
            Type decoratorType = GetDecoratingBaseType(serviceType, decoratorConstructor);

            if (decoratorType is null)
            {
                return 0;
            }

            var validServiceTypeArguments =
                from parameter in decoratorConstructor.GetParameters()
                where IsDecorateeParameter(parameter, decoratorType)
                select parameter;

            return validServiceTypeArguments.Count();
        }

        internal static bool IsDecorateeParameter(ParameterInfo parameter, Type decoratingType) =>
            IsDecorateeDependencyType(parameter.ParameterType, decoratingType)
            || IsDecorateeFactoryDependencyType(parameter.ParameterType, decoratingType);

        internal static bool IsDecorateeFactoryDependencyType(Type dependencyType, Type decoratingType) =>
            IsScopelessDecorateeFactoryDependencyType(dependencyType, decoratingType)
            || IsScopeDecorateeFactoryDependencyParameter(dependencyType, decoratingType);

        internal static bool IsScopelessDecorateeFactoryDependencyType(
            Type dependencyType, Type decoratingType) =>
            typeof(Func<>).IsGenericTypeDefinitionOf(dependencyType)
                && dependencyType == typeof(Func<>).MakeGenericType(decoratingType);

        internal static bool IsScopeDecorateeFactoryDependencyParameter(
            Type parameterType, Type decoratingType) =>
            typeof(Func<,>).IsGenericTypeDefinitionOf(parameterType)
                && parameterType == typeof(Func<,>).MakeGenericType(typeof(Scope), decoratingType);

        // Returns the base type of the decorator that can be used for decoration (because serviceType might
        // be open generic, while the base type might not be).
        private static Type GetDecoratingBaseType(Type serviceType, ConstructorInfo decoratorConstructor)
        {
            var abstractions = Types.GetBaseTypeCandidates(serviceType, decoratorConstructor.DeclaringType);

            ParameterInfo[] constructorParameters = decoratorConstructor.GetParameters();

            var decoratorInterfaces =
                from abstraction in abstractions
                where constructorParameters.Any(parameter => IsDecorateeParameter(parameter, abstraction))
                select abstraction;

            return decoratorInterfaces.FirstOrDefault();
        }

        private static bool IsDecorateeDependencyType(Type dependencyType, Type serviceType) =>
            dependencyType == serviceType;
    }
}