namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Threading;
    using SimpleInjector.Advanced;

    public static class AutomaticParameterizedFactoryExtensions
    {
        public static void EnableAutomaticParameterizedFactories(this ContainerOptions options)
        {
            if (GetBehavior(options.Container) != null)
            {
                throw new InvalidOperationException("Already called.");
            }

            var behavior = new AutomaticParameterizedFactoriesHelper(options);

            options.ConstructorInjectionBehavior = behavior;
            options.ConstructorVerificationBehavior = behavior;

            SetBehavior(options.Container, behavior);
        }

        public static void RegisterFactoryProduct<TConcrete>(this Container container, Lifestyle lifestyle = null)
            where TConcrete : class
        {
            RegisterFactoryProduct<TConcrete, TConcrete>(container, lifestyle);
        }

        public static void RegisterFactoryProduct<TService, TImplementation>(this Container container,
            Lifestyle lifestyle = null)
            where TImplementation : class, TService
            where TService : class
        {
            var behavior = GetBehavior(container);

            if (behavior == null)
            {
                throw new InvalidOperationException(
                    "Make sure you call container.Options.EnableAutomaticParameterizedFactories() first.");
            }

            behavior.RegisterFactoryProduct(typeof(TService), typeof(TImplementation));

            container.Register<TService, TImplementation>(lifestyle ?? Lifestyle.Transient);
        }

        public static void RegisterParameterizedFactory<TFactory>(this Container container)
        {
            if (!typeof(TFactory).IsInterface)
            {
                throw new ArgumentException(typeof(TFactory).Name + " is no interface");
            }

            var parameters = (
                from method in typeof(TFactory).GetMethods()
                from parameter in method.GetParameters()
                select new { method.ReturnType, parameter.ParameterType })
                .ToList();

            if (parameters.Any())
            {
                var behavior = GetBehavior(container);

                if (behavior == null)
                {
                    throw new InvalidOperationException("This factory contains parameterized methods. Make " +
                        "sure you call container.Options.EnableAutomaticParameterizedFactories() first.");
                }

                parameters.ForEach(p => behavior.Register(p.ReturnType, p.ParameterType));
            }

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(TFactory))
                {
                    var singletonFactory = Expression.Constant(
                        value: CreateFactory(typeof(TFactory), container),
                        type: typeof(TFactory));

                    e.Register(singletonFactory);
                }
            };
        }

        private static object CreateFactory(Type factoryType, Container container)
        {
            var proxy = new AutomaticFactoryProxy(factoryType, container);

            return proxy.GetTransparentProxy();
        }

        private static AutomaticParameterizedFactoriesHelper GetBehavior(Container container)
        {
            return (AutomaticParameterizedFactoriesHelper)
                container.GetItem(typeof(AutomaticParameterizedFactoriesHelper));
        }

        private static void SetBehavior(Container container, AutomaticParameterizedFactoriesHelper behavior)
        {
            container.SetItem(typeof(AutomaticParameterizedFactoriesHelper), behavior);
        }

        private sealed class AutomaticFactoryProxy : RealProxy
        {
            private readonly Type factoryType;
            private readonly Container container;
            private readonly AutomaticParameterizedFactoriesHelper helper;

            [DebuggerStepThrough]
            public AutomaticFactoryProxy(Type factoryType, Container container)
                : base(factoryType)
            {
                this.factoryType = factoryType;
                this.container = container;
                this.helper = AutomaticParameterizedFactoryExtensions.GetBehavior(container);
            }

            public override IMessage Invoke(IMessage msg)
            {
                IMethodCallMessage callMessage = msg as IMethodCallMessage;

                if (callMessage != null)
                {
                    return this.InvokeFactory(callMessage);
                }

                return msg;
            }

            private IMessage InvokeFactory(IMethodCallMessage message)
            {
                if (message.MethodName == "GetType")
                {
                    return new ReturnMessage(this.factoryType, null, 0, null, message);
                }

                if (message.MethodName == "ToString")
                {
                    return new ReturnMessage(this.factoryType.FullName, null, 0, null, message);
                }

                var method = (MethodInfo)message.MethodBase;

                var parameters = this.CreateParameterValues(message);

                try
                {
                    object instance = this.container.GetInstance(method.ReturnType);

                    return new ReturnMessage(instance, null, 0, null, message);
                }
                finally
                {
                    this.RestoreParameterValues(message, parameters);
                }
            }

            private ParameterValue[] CreateParameterValues(IMethodCallMessage message)
            {
                var method = (MethodInfo)message.MethodBase;

                var parameterValues = method.GetParameters()
                    .Zip(message.Args, (p, v) => new ParameterValue(p, v)).ToArray();

                foreach (var p in parameterValues)
                {
                    var local = this.helper.GetThreadLocal(method.ReturnType, p.Parameter.ParameterType);
                    p.OldValue = local.Value;
                    local.Value = p.FactoryValue;
                }

                return parameterValues;
            }

            private void RestoreParameterValues(IMethodCallMessage message, ParameterValue[] values)
            {
                var method = (MethodInfo)message.MethodBase;

                foreach (var p in values)
                {
                    var local = this.helper.GetThreadLocal(method.ReturnType, p.Parameter.ParameterType);
                    local.Value = p.OldValue;
                }
            }
        }

        private sealed class ParameterValue
        {
            internal readonly ParameterInfo Parameter;
            internal readonly object FactoryValue;

            internal ParameterValue(ParameterInfo parameter, object factoryValue)
            {
                this.Parameter = parameter;
                this.FactoryValue = factoryValue;
            }

            internal object OldValue { get; set; }
        }

        private sealed class AutomaticParameterizedFactoriesHelper
            : IConstructorVerificationBehavior, IConstructorInjectionBehavior
        {
            private readonly Container container;
            private readonly IConstructorVerificationBehavior originalVerificationBehavior;
            private readonly IConstructorInjectionBehavior originalInjectionBehavior;
            private readonly Dictionary<Type, Dictionary<Type, ThreadLocal<object>>> serviceLocals =
                new Dictionary<Type, Dictionary<Type, ThreadLocal<object>>>();
            
            private readonly Dictionary<Type, Dictionary<Type, ThreadLocal<object>>> implementationLocals =
                new Dictionary<Type, Dictionary<Type, ThreadLocal<object>>>();

            public AutomaticParameterizedFactoriesHelper(ContainerOptions options)
            {
                this.container = options.Container;
                this.originalVerificationBehavior = options.ConstructorVerificationBehavior;
                this.originalInjectionBehavior = options.ConstructorInjectionBehavior;
            }

            void IConstructorVerificationBehavior.Verify(ParameterInfo parameter)
            {
                if (this.FindThreadLocal(parameter) == null)
                {
                    this.originalVerificationBehavior.Verify(parameter);
                }
            }

            Expression IConstructorInjectionBehavior.BuildParameterExpression(ParameterInfo parameter)
            {
                var local = this.FindThreadLocal(parameter);

                if (local != null)
                {
                    if (parameter.ParameterType.IsValueType && this.container.IsVerifying())
                    {
                        throw new InvalidOperationException(
                            "You can't use Verify() is the factory product contains value types.");
                    }

                    return Expression.Convert(
                        Expression.Property(Expression.Constant(local), "Value"),
                        parameter.ParameterType);
                }

                return this.originalInjectionBehavior.BuildParameterExpression(parameter);
            }

            // Called by RegisterFactory<TFactory>
            internal void Register(Type serviceType, Type parameterType)
            {
                Dictionary<Type, ThreadLocal<object>> parameterLocals;

                if (!this.serviceLocals.TryGetValue(serviceType, out parameterLocals))
                {
                    this.serviceLocals[serviceType] =
                        parameterLocals = new Dictionary<Type, ThreadLocal<object>>();
                }

                parameterLocals[parameterType] = new ThreadLocal<object>();
            }

            // Called by RegisterFactoryProduct<T>
            internal void RegisterFactoryProduct(Type serviceType, Type implementationType)
            {
                // Create a mapping from implementationType to serviceType.
                if (this.serviceLocals.ContainsKey(serviceType))
                {
                    this.implementationLocals[implementationType] = this.serviceLocals[serviceType];
                }
            }

            internal ThreadLocal<object> GetThreadLocal(Type serviceType, Type parameterType)
            {
                return this.serviceLocals[serviceType][parameterType];
            }

            private ThreadLocal<object> FindThreadLocal(ParameterInfo parameter)
            {
                Dictionary<Type, ThreadLocal<object>> parameterLocals;

                if (this.implementationLocals.TryGetValue(parameter.Member.DeclaringType, out parameterLocals))
                {
                    ThreadLocal<object> local;

                    if (parameterLocals.TryGetValue(parameter.ParameterType, out local))
                    {
                        return local;
                    }
                }

                return null;
            }
        }
    }
}