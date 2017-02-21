namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public sealed class ParameterConvention : IParameterConvention
    {
        private readonly List<ParameterDeclaringTypePair> parameters = new List<ParameterDeclaringTypePair>();

        bool IParameterConvention.CanResolve(InjectionTargetInfo target)
        {
            return target.Parameter != null && this.GetParameter(target.Parameter) != null;
        }

        Expression IParameterConvention.BuildExpression(InjectionConsumerInfo consumer)
        {
            return this.GetParameter(consumer.Target.Parameter).Expression;
        }

        public Parameter WithParameter<T>(T value)
        {
            return new Parameter(this, typeof(T))
            {
                Expression = Expression.Constant(value)
            };
        }

        public Parameter WithParameter<T>(string parameterName, T value)
        {
            return new Parameter(this, typeof(T))
            {
                Name = parameterName,
                Expression = Expression.Constant(value)
            };
        }

        public Parameter WithParameter<T>(string parameterName, Func<T> factory)
        {
            return new Parameter(this, typeof(T))
            {
                Name = parameterName,
                Expression = Expression.Invoke(Expression.Constant(factory))
            };
        }

        public Parameter WithParameter<T>(Func<T> factory)
        {
            return new Parameter(this, typeof(T))
            {
                Expression = Expression.Invoke(Expression.Constant(factory))
            };
        }

        internal void Add(Type concreteType, Parameter parameter)
        {
            this.parameters.Add(new ParameterDeclaringTypePair
            {
                DeclaringType = concreteType,
                Parameter = parameter
            });
        }

        private Parameter GetParameter(ParameterInfo parameter)
        {
            var parametersForType = (
                from param in this.parameters
                where param.DeclaringType == parameter.Member.DeclaringType
                select param)
                .ToArray();

            var constructorParameters = ((ConstructorInfo)parameter.Member).GetParameters();

            var invalidParameters = (
                from param in parametersForType
                where param.Parameter.Name != null
                where !constructorParameters.Any(p => p.Name == param.Parameter.Name)
                select param)
                .ToArray();

            if (invalidParameters.Any())
            {
                var invalidParameter = invalidParameters.First().Parameter;

                throw new ActivationException(string.Format(CultureInfo.CurrentCulture,
                    "Parameter with name '{0}' of type {1} is not a parameter of the constructor of type {2}",
                    invalidParameter.Name, invalidParameter.Type.Name, parameter.Member.DeclaringType.Name));
            }

            var suitableParameters = (
                from param in parametersForType
                where (param.Parameter.Name == null && param.Parameter.Type == parameter.ParameterType) ||
                    param.Parameter.Name == parameter.Name
                select param.Parameter)
                .ToArray();

            if (!suitableParameters.Any())
            {
                return null;
            }

            if (suitableParameters.Length == 1)
            {
                return suitableParameters[0];
            }

            throw new ActivationException(string.Format(CultureInfo.CurrentCulture, 
                "Multiple parameter registrations found for type " +
                "{0} that match to parameter with name '{1}' of type {2}.",
                parameter.Member.DeclaringType.Name, parameter.Name, parameter.ParameterType.Name));
        }

        public class Parameter
        {
            public Parameter(ParameterConvention convention, Type parameterType)
            {
                this.Convention = convention;
                this.Type = parameterType;
            }

            internal ParameterConvention Convention { get; }
            internal Type Type { get; }
            internal string Name { get; set; }
            internal Expression Expression { get; set; }
        }

        private class ParameterDeclaringTypePair
        {
            internal Parameter Parameter { get; set; }
            internal Type DeclaringType { get; set; }
        }
    }

    public static class WithParameterConventionExtensions
    {
        public static void Register<TConcrete>(this Container container,
            params ParameterConvention.Parameter[] parameters)
            where TConcrete : class
        {
            AddParameters(typeof(TConcrete), parameters);
            container.Register<TConcrete>();
        }

        public static void Register<TService, TImplementation>(this Container container,
            params ParameterConvention.Parameter[] parameters)
            where TImplementation : class, TService
            where TService : class
        {
            AddParameters(typeof(TImplementation), parameters);
            container.Register<TService, TImplementation>();
        }

        public static void RegisterSingleton<TConcrete>(this Container container,
            params ParameterConvention.Parameter[] parameters)
            where TConcrete : class
        {
            AddParameters(typeof(TConcrete), parameters);
            container.Register<TConcrete>(Lifestyle.Singleton);
        }

        public static void RegisterSingleton<TService, TImplementation>(this Container container,
            params ParameterConvention.Parameter[] parameters)
            where TImplementation : class, TService
            where TService : class
        {
            AddParameters(typeof(TImplementation), parameters);
            container.Register<TService, TImplementation>(Lifestyle.Singleton);
        }

        private static void AddParameters(Type concreteType, ParameterConvention.Parameter[] parameters)
        {
            foreach (var parameter in parameters ?? Enumerable.Empty<ParameterConvention.Parameter>())
            {
                parameter.Convention.Add(concreteType, parameter);
            }
        }
    }
}