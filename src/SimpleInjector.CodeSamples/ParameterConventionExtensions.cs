namespace SimpleInjector.CodeSamples
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;
    using SimpleInjector.Advanced;

    public interface IParameterConvention
    {
        bool CanResolve(InjectionTargetInfo target);

        Expression BuildExpression(InjectionConsumerInfo consumer);
    }

    public static class ParameterConventionExtensions
    {
        public static void RegisterParameterConvention(this ContainerOptions options,
            IParameterConvention convention)
        {
            options.DependencyInjectionBehavior = new ConventionDependencyInjectionBehavior(
                options.DependencyInjectionBehavior, convention);
        }

        private class ConventionDependencyInjectionBehavior : IDependencyInjectionBehavior
        {
            private IDependencyInjectionBehavior decorated;
            private IParameterConvention convention;

            public ConventionDependencyInjectionBehavior(
                IDependencyInjectionBehavior decorated, IParameterConvention convention)
            {
                this.decorated = decorated;
                this.convention = convention;
            }

            [DebuggerStepThrough]
            public Expression BuildExpression(InjectionConsumerInfo consumer)
            {
                if (!this.convention.CanResolve(consumer.Target))
                {
                    return this.decorated.BuildExpression(consumer);
                }

                return this.convention.BuildExpression(consumer);
            }
            
            [DebuggerStepThrough]
            public void Verify(InjectionConsumerInfo consumer)
            {
                if (!this.convention.CanResolve(consumer.Target))
                {
                    this.decorated.Verify(consumer);
                }
            }
        }
    }

    // Example usage:
    // new ConnectionStringsConvention(name => ConfigurationManager.ConnectionStrings[name]?.ConnectionString)
    public class ConnectionStringsConvention : IParameterConvention
    {
        private const string ConnectionStringPostFix = "ConnectionString";

        private readonly Func<string, string> connectionStringRetriever;

        public ConnectionStringsConvention(Func<string, string> connectionStringRetriever)
        {
            this.connectionStringRetriever = connectionStringRetriever;
        }

        [DebuggerStepThrough]
        public bool CanResolve(InjectionTargetInfo target)
        {
            bool resolvable =
                target.TargetType == typeof(string) &&
                target.Name.EndsWith(ConnectionStringPostFix) &&
                target.Name.LastIndexOf(ConnectionStringPostFix) > 0;

            if (resolvable)
            {
                this.VerifyConfigurationFile(target);
            }

            return resolvable;
        }

        [DebuggerStepThrough]
        public Expression BuildExpression(InjectionConsumerInfo consumer)
        {
            string connectionString = this.GetConnectionString(consumer.Target);

            return Expression.Constant(connectionString, typeof(string));
        }

        [DebuggerStepThrough]
        private void VerifyConfigurationFile(InjectionTargetInfo target)
        {
            this.GetConnectionString(target);
        }

        [DebuggerStepThrough]
        private string GetConnectionString(InjectionTargetInfo target)
        {
            string name = target.Name.Substring(0,
                target.Name.LastIndexOf(ConnectionStringPostFix));

            var connectionString = this.connectionStringRetriever(name);

            if (connectionString == null)
            {
                throw new ActivationException(
                    "No connection string with name '" + name + "' could be found in the " + 
                    "application's configuration file.");
            }

            return connectionString;
        }
    }

    // Example usage:
    // new AppSettingsConvention(key => ConfigurationManager.AppSettings[key]);
    public class AppSettingsConvention : IParameterConvention
    {
        private const string AppSettingsPostFix = "AppSetting";

        private readonly Func<string, string> appSettingRetriever;

        public AppSettingsConvention(Func<string, string> appSettingRetriever)
        {
            this.appSettingRetriever = appSettingRetriever;
        }

        [DebuggerStepThrough]
        public bool CanResolve(InjectionTargetInfo target)
        {
            Type type = target.TargetType;

            bool resolvable =
                (type.IsValueType || type == typeof(string)) &&
                target.Name.EndsWith(AppSettingsPostFix) &&
                target.Name.LastIndexOf(AppSettingsPostFix) > 0;

            if (resolvable)
            {
                this.VerifyConfigurationFile(target);
            }

            return resolvable;
        }

        [DebuggerStepThrough]
        public Expression BuildExpression(InjectionConsumerInfo consumer)
        {
            object valueToInject = this.GetAppSettingValue(consumer.Target);

            return Expression.Constant(valueToInject, consumer.Target.TargetType);
        }

        [DebuggerStepThrough]
        private void VerifyConfigurationFile(InjectionTargetInfo target)
        {
            this.GetAppSettingValue(target);
        }

        [DebuggerStepThrough]
        private object GetAppSettingValue(InjectionTargetInfo target)
        {
            string key = target.Name.Substring(0,
                target.Name.LastIndexOf(AppSettingsPostFix));

            string configurationValue = this.appSettingRetriever(key); // ConfigurationManager.AppSettings[key];

            if (configurationValue != null)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(target.TargetType);

                return converter.ConvertFromString(null,
                    CultureInfo.InvariantCulture, configurationValue);
            }

            throw new ActivationException(
                "No application setting with key '" + key + "' could be found in the " +
                "application's configuration file.");
        }
    }
    
    // Using optional parameters in constructor arguments is highly discouraged. 
    // This code is merely an example.
    public class OptionalParameterConvention : IParameterConvention
    {
        private readonly IDependencyInjectionBehavior injectionBehavior;

        public OptionalParameterConvention(IDependencyInjectionBehavior injectionBehavior)
        {
            this.injectionBehavior = injectionBehavior;
        }

        [DebuggerStepThrough]
        public bool CanResolve(InjectionTargetInfo target)
        {
            return target.Parameter != null &&
                target.GetCustomAttributes(typeof(OptionalAttribute), true).Length > 0;
        }

        [DebuggerStepThrough]
        public Expression BuildExpression(InjectionConsumerInfo consumer)
        {
            try
            {
                return this.injectionBehavior.BuildExpression(consumer);
            }
            catch (ActivationException)
            {
                var parameter = consumer.Target.Parameter;
                return Expression.Constant(parameter.RawDefaultValue, parameter.ParameterType);
            }
        }
    }
}