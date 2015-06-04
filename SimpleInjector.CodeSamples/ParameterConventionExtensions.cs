namespace SimpleInjector.CodeSamples
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using SimpleInjector.Advanced;

    public interface IParameterConvention
    {
        bool CanResolve(ParameterInfo parameter);

        Expression BuildExpression(Type serviceType, Type implementationType, ParameterInfo parameter);
    }

    public static class ParameterConventionExtensions
    {
        public static void RegisterParameterConvention(this ContainerOptions options,
            IParameterConvention convention)
        {
            options.ConstructorInjectionBehavior = new ConventionConstructorInjectionBehavior(
                options.ConstructorInjectionBehavior, convention);
        }

        private class ConventionConstructorInjectionBehavior : IConstructorInjectionBehavior
        {
            private IConstructorInjectionBehavior decorated;
            private IParameterConvention convention;

            public ConventionConstructorInjectionBehavior(
                IConstructorInjectionBehavior decorated, IParameterConvention convention)
            {
                this.decorated = decorated;
                this.convention = convention;
            }

            [DebuggerStepThrough]
            public Expression BuildParameterExpression(Type serviceType, Type implementationType, 
                ParameterInfo parameter)
            {
                if (!this.convention.CanResolve(parameter))
                {
                    return this.decorated.BuildParameterExpression(serviceType, implementationType, parameter);
                }

                return this.convention.BuildExpression(serviceType, implementationType, parameter);
            }
            
            [DebuggerStepThrough]
            public void Verify(Type serviceType, Type implementationType, ParameterInfo parameter)
            {
                if (!this.convention.CanResolve(parameter))
                {
                    this.decorated.Verify(serviceType, implementationType, parameter);
                }
            }
        }
    }

    public class ConnectionStringsConvention : IParameterConvention
    {
        private const string ConnectionStringPostFix = "ConnectionString";

        [DebuggerStepThrough]
        public bool CanResolve(ParameterInfo parameter)
        {
            bool resolvable =
                parameter.ParameterType == typeof(string) &&
                parameter.Name.EndsWith(ConnectionStringPostFix) &&
                parameter.Name.LastIndexOf(ConnectionStringPostFix) > 0;

            if (resolvable)
            {
                this.VerifyConfigurationFile(parameter);
            }

            return resolvable;
        }

        [DebuggerStepThrough]
        public Expression BuildExpression(Type serviceType, Type implementationType, ParameterInfo parameter)
        {
            string connectionString = GetConnectionString(parameter);

            return Expression.Constant(connectionString, typeof(string));
        }

        [DebuggerStepThrough]
        private void VerifyConfigurationFile(ParameterInfo parameter)
        {
            GetConnectionString(parameter);
        }

        [DebuggerStepThrough]
        private static string GetConnectionString(ParameterInfo parameter)
        {
            string name = parameter.Name.Substring(0,
                parameter.Name.LastIndexOf(ConnectionStringPostFix));

            var settings = ConfigurationManager.ConnectionStrings[name];

            if (settings == null)
            {
                throw new ActivationException(
                    "No connection string with name '" + name + "' could be found in the " + 
                    "application's configuration file.");
            }

            return settings.ConnectionString;
        }
    }

    public class AppSettingsConvention : IParameterConvention
    {
        private const string AppSettingsPostFix = "AppSetting";

        [DebuggerStepThrough]
        public bool CanResolve(ParameterInfo parameter)
        {
            Type type = parameter.ParameterType;

            bool resolvable =
                (type.IsValueType || type == typeof(string)) &&
                parameter.Name.EndsWith(AppSettingsPostFix) &&
                parameter.Name.LastIndexOf(AppSettingsPostFix) > 0;

            if (resolvable)
            {
                this.VerifyConfigurationFile(parameter);
            }

            return resolvable;
        }

        [DebuggerStepThrough]
        public Expression BuildExpression(Type serviceType, Type implementationType, ParameterInfo parameter)
        {
            object valueToInject = GetAppSettingValue(parameter);

            return Expression.Constant(valueToInject, parameter.ParameterType);
        }

        [DebuggerStepThrough]
        private void VerifyConfigurationFile(ParameterInfo parameter)
        {
            GetAppSettingValue(parameter);
        }

        [DebuggerStepThrough]
        private static object GetAppSettingValue(ParameterInfo parameter)
        {
            string key = parameter.Name.Substring(0,
                parameter.Name.LastIndexOf(AppSettingsPostFix));

            string configurationValue = ConfigurationManager.AppSettings[key];

            if (configurationValue == null)
            {
                throw new ActivationException(
                    "No application setting with key '" + key + "' could be found in the " +
                    "application's configuration file.");
            }

            TypeConverter converter = TypeDescriptor.GetConverter(parameter.ParameterType);

            return converter.ConvertFromString(null,
                CultureInfo.InvariantCulture, configurationValue);
        }
    }

    // Using optional parameters in constructor arguments is highly discouraged. 
    // This code is merely an example.
    public class OptionalParameterConvention : IParameterConvention
    {
        private readonly IConstructorInjectionBehavior injectionBehavior;

        public OptionalParameterConvention(IConstructorInjectionBehavior injectionBehavior)
        {
            this.injectionBehavior = injectionBehavior;
        }

        [DebuggerStepThrough]
        public bool CanResolve(ParameterInfo parameter)
        {
            return parameter.GetCustomAttributes(typeof(OptionalAttribute), true).Length > 0;
        }

        [DebuggerStepThrough]
        public Expression BuildExpression(Type serviceType, Type implementationType, ParameterInfo parameter)
        {
            try
            {
                return this.injectionBehavior.BuildParameterExpression(serviceType, implementationType,
                    parameter);
            }
            catch (ActivationException)
            {
                return Expression.Constant(parameter.RawDefaultValue, parameter.ParameterType);
            }
        }
    }
}