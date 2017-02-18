namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
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
                options.DependencyInjectionBehavior, convention, options.Container);
        }

        private class ConventionDependencyInjectionBehavior : IDependencyInjectionBehavior
        {
            private IDependencyInjectionBehavior decorated;
            private IParameterConvention convention;
            private Container container;

            public ConventionDependencyInjectionBehavior(
                IDependencyInjectionBehavior decorated, IParameterConvention convention,
                Container container)
            {
                this.decorated = decorated;
                this.convention = convention;
                this.container = container;
            }

            [DebuggerStepThrough]
            public void Verify(InjectionConsumerInfo consumer)
            {
                if (!this.convention.CanResolve(consumer.Target))
                {
                    this.decorated.Verify(consumer);
                }
            }

            [DebuggerStepThrough]
            public InstanceProducer GetInstanceProducer(InjectionConsumerInfo consumer, bool throwOnFailure)
            {
                if (!this.convention.CanResolve(consumer.Target))
                {
                    return this.decorated.GetInstanceProducer(consumer, throwOnFailure);
                }

                return InstanceProducer.FromExpression(
                    serviceType: consumer.Target.TargetType,
                    expression: this.convention.BuildExpression(consumer),
                    container: this.container);
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
                System.ComponentModel.TypeConverter converter = 
                    System.ComponentModel.TypeDescriptor.GetConverter(target.TargetType);

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
        public bool CanResolve(InjectionTargetInfo target) =>
            target.Parameter != null && target.GetCustomAttributes(typeof(OptionalAttribute), true).Any();

        [DebuggerStepThrough]
        public Expression BuildExpression(InjectionConsumerInfo consumer) =>
            this.GetProducer(consumer)?.BuildExpression() ?? GetDefault(consumer.Target.Parameter);

        private InstanceProducer GetProducer(InjectionConsumerInfo consumer) =>
            this.injectionBehavior.GetInstanceProducer(consumer, throwOnFailure: false);

        private static ConstantExpression GetDefault(ParameterInfo parameter) =>
            Expression.Constant(parameter.RawDefaultValue, parameter.ParameterType);
    }
}