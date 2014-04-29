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

        Expression BuildExpression(ParameterInfo parameter);
    }

    public static class ParameterConventionExtensions
    {
        public static void RegisterParameterConvention(
            this ContainerOptions options,
            IParameterConvention convention)
        {
            options.ConstructorVerificationBehavior =
                new ConventionConstructorVerificationBehavior(
                    options.ConstructorVerificationBehavior,
                    convention);

            options.ConstructorInjectionBehavior =
                new ConventionConstructorInjectionBehavior(
                    options.ConstructorInjectionBehavior,
                    convention);
        }

        private class ConventionConstructorVerificationBehavior
            : IConstructorVerificationBehavior
        {
            private IConstructorVerificationBehavior decorated;
            private IParameterConvention convention;

            public ConventionConstructorVerificationBehavior(
                IConstructorVerificationBehavior decorated,
                IParameterConvention convention)
            {
                this.decorated = decorated;
                this.convention = convention;
            }

            [DebuggerStepThrough]
            public void Verify(ParameterInfo parameter)
            {
                if (!this.convention.CanResolve(parameter))
                {
                    this.decorated.Verify(parameter);
                }
            }
        }

        private class ConventionConstructorInjectionBehavior
            : IConstructorInjectionBehavior
        {
            private IConstructorInjectionBehavior decorated;
            private IParameterConvention convention;

            public ConventionConstructorInjectionBehavior(
                IConstructorInjectionBehavior decorated,
                IParameterConvention convention)
            {
                this.decorated = decorated;
                this.convention = convention;
            }

            [DebuggerStepThrough]
            public Expression BuildParameterExpression(
                ParameterInfo parameter)
            {
                if (!this.convention.CanResolve(parameter))
                {
                    return this.decorated
                        .BuildParameterExpression(parameter);
                }

                return this.convention.BuildExpression(parameter);
            }
        }
    }

    public class ConnectionStringsConvention : IParameterConvention
    {
        private const string ConnectionStringPostFix =
            "ConnectionString";

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
        public Expression BuildExpression(ParameterInfo parameter)
        {
            var constr = this.GetConnectionString(parameter);

            return Expression.Constant(constr, typeof(string));
        }

        [DebuggerStepThrough]
        private void VerifyConfigurationFile(ParameterInfo parameter)
        {
            this.GetConnectionString(parameter);
        }

        [DebuggerStepThrough]
        private string GetConnectionString(ParameterInfo parameter)
        {
            string name = parameter.Name.Substring(0,
                parameter.Name.LastIndexOf(ConnectionStringPostFix));

            ConnectionStringSettings settings =
                ConfigurationManager.ConnectionStrings[name];

            if (settings == null)
            {
                throw new ActivationException(
                    "No connection string with name '" + name +
                    "' could be found in the application's " +
                    "configuration file.");
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
        public Expression BuildExpression(ParameterInfo parameter)
        {
            object valueToInject = this.GetAppSettingValue(parameter);

            return Expression.Constant(valueToInject,
                parameter.ParameterType);
        }

        [DebuggerStepThrough]
        private void VerifyConfigurationFile(ParameterInfo parameter)
        {
            this.GetAppSettingValue(parameter);
        }

        [DebuggerStepThrough]
        private object GetAppSettingValue(ParameterInfo parameter)
        {
            string key = parameter.Name.Substring(0,
                parameter.Name.LastIndexOf(AppSettingsPostFix));

            string configurationValue =
                ConfigurationManager.AppSettings[key];

            if (configurationValue == null)
            {
                throw new ActivationException(
                    "No app setting with key '" + key + "' " +
                    "could be found in the application's " +
                    "configuration file.");
            }

            TypeConverter converter = TypeDescriptor.GetConverter(
                parameter.ParameterType);

            return converter.ConvertFromString(null,
                CultureInfo.InvariantCulture, configurationValue);
        }
    }

    // Using optional parameters in constructor arguments is higly discouraged. This code is merely an example.
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
        public Expression BuildExpression(ParameterInfo parameter)
        {
            try
            {
                return this.injectionBehavior.BuildParameterExpression(parameter);
            }
            catch (ActivationException)
            {
                return Expression.Constant(parameter.RawDefaultValue, parameter.ParameterType);
            }
        }
    }
}