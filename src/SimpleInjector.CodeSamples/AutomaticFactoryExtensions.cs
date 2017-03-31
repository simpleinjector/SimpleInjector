namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;

    public static class AutomaticFactoryExtensions
    {
        public static void RegisterFactory<TFactory>(this Container container)
        {
            if (!typeof(TFactory).IsInterface)
            {
                throw new ArgumentException(typeof(TFactory).Name + " is no interface");
            }

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(TFactory))
                {
                    e.Register(Expression.Constant(
                        value: CreateFactory(typeof(TFactory), container),
                        type: typeof(TFactory)));
                }
            };
        }

        [DebuggerStepThrough]
        public static object CreateFactory(Type factoryType, Container container) =>
            new AutomaticFactoryProxy(factoryType, container).GetTransparentProxy();

        private sealed class AutomaticFactoryProxy : RealProxy
        {
            private readonly Type factoryType;
            private readonly Container container;

            [DebuggerStepThrough]
            public AutomaticFactoryProxy(Type factoryType, Container container)
                : base(factoryType)
            {
                this.factoryType = factoryType;
                this.container = container;
            }

            public override IMessage Invoke(IMessage msg)
            {
                var callMessage = msg as IMethodCallMessage;

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

                object instance = this.container.GetInstance(method.ReturnType);

                return new ReturnMessage(instance, null, 0, null, message);
            }
        }
    }
}