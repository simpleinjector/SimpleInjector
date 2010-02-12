using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.Practices.ServiceLocation;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Builds <see cref="Func{T}"/> delegates that can create a new instance of the supplied Type, where
    /// the supplied container will be used to locate the constructor arguments. The generated code of the
    /// built <see cref="Func{T}"/> might look like this.
    /// <![CDATA[
    ///     Func<object> func = () => return new Samurai(container.GetInstance<IWeapon>());
    /// ]]>
    /// </summary>
    internal sealed class DelegateBuilder
    {
        private static readonly MethodInfo GenericGetInstanceMethodDefinition =
            typeof(IServiceLocator).GetMethod("GetInstance", Type.EmptyTypes);

        private readonly Type serviceType;
        private readonly Dictionary<Type, Func<object>> registrations;
        private readonly IServiceLocator container;

        /// <summary>Initializes a new instance of the <see cref="DelegateBuilder"/> class.</summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="registrations">The set of registrations.</param>
        /// <param name="container">The service locator.</param>
        private DelegateBuilder(Type serviceType, Dictionary<Type, Func<object>> registrations,
            IServiceLocator container)
        {
            this.serviceType = serviceType;
            this.registrations = registrations;
            this.container = container;
        }

        /// <summary>Builds the specified service type.</summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="registrations">The registrations.</param>
        /// <param name="serviceLocator">The service locator.</param>
        /// <returns>A new <see cref="Func{T}"/>.</returns>
        public static Func<object> Build(Type serviceType, Dictionary<Type, Func<object>> registrations,
            IServiceLocator serviceLocator)
        {
            var builder = new DelegateBuilder(serviceType, registrations, serviceLocator);
            return builder.Build();
        }

        private Func<object> Build()
        {
            var constructor = this.GetOnlyPublicConstructor();

            Expression[] constructorArgumentCalls = this.BuildGetInstanceCallsForConstructor(constructor);

            var newServiceTypeMethod = Expression.Lambda<Func<object>>(
                Expression.New(constructor, constructorArgumentCalls), new ParameterExpression[0]);

            return newServiceTypeMethod.Compile();
        }

        private ConstructorInfo GetOnlyPublicConstructor()
        {
            var constructors = this.serviceType.GetConstructors();

            if (constructors.Length != 1)
            {
                throw new ActivationException(StringResources.TypeMustHaveASinglePublicConstructor(
                    this.serviceType, constructors.Length));
            }

            return constructors[0];
        }

        private Expression[] BuildGetInstanceCallsForConstructor(ConstructorInfo constructor)
        {
            List<Expression> getInstanceCalls = new List<Expression>();

            foreach (var parameter in constructor.GetParameters())
            {
                var getInstanceCall = this.BuildGetInstanceCallForType(parameter.ParameterType);

                getInstanceCalls.Add(getInstanceCall);
            }

            return getInstanceCalls.ToArray();
        }

        private Expression BuildGetInstanceCallForType(Type parameterType)
        {
            if (!this.registrations.ContainsKey(parameterType))
            {
                throw new ActivationException(
                    StringResources.ParameterTypeMustBeRegistered(this.serviceType, parameterType));
            }

            var getInstanceMethod = MakeGenericGetInstanceMethod(parameterType);

            // Build the call "serviceLocator.GetInstance<[ParameterType]>()"
            return Expression.Call(Expression.Constant(this.container), getInstanceMethod,
                new Expression[0]);
        }

        private static MethodInfo MakeGenericGetInstanceMethod(Type parameterType)
        {
            return GenericGetInstanceMethodDefinition.MakeGenericMethod(parameterType);
        }
    }
}