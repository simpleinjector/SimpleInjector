namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    
    public interface IInterceptor
    {
        void Intercept(IInvocation invocation);
    }

    public interface IInvocation
    {
        object InvocationTarget { get; }

        object ReturnValue { get; set; }

        void Proceed();

        MethodBase GetConcreteMethod();
    }

    // Extension methods for interceptor registration    
    public static class InterceptorExtensions
    {
        public static void RegisterWithInterceptor<TService, TInterceptor, TImplementation>(
            this Container container)
            where TService : class
            where TInterceptor : class, IInterceptor
            where TImplementation : class, TService
        {
            container.Register<TService>(() =>
            {
                var interceptor = container.GetInstance<TInterceptor>();
                var realInstance = container.GetInstance<TImplementation>();
                return Interceptor.CreateProxy<TService>(interceptor, realInstance);
            });          
        }

        public static void RegisterSingleWithInterceptor<TService, TInterceptor, TImplementation>(
            this Container container)
            where TService : class
            where TInterceptor : class, IInterceptor
            where TImplementation : class, TService
        {
            container.RegisterSingle<TService>(() =>
            {
                var interceptor = container.GetInstance<TInterceptor>();
                var realInstance = container.GetInstance<TImplementation>();
                return Interceptor.CreateProxy<TService>(interceptor, realInstance);
            });  
        }

        public static void RegisterWithInterceptor<TService, TInterceptor1, TInterceptor2, TImplementation>(
            this Container container)
            where TService : class
            where TInterceptor1 : class, IInterceptor
            where TInterceptor2 : class, IInterceptor
            where TImplementation : class, TService
        {
            container.Register<TService>(() =>
            {
                var interceptor1 = container.GetInstance<TInterceptor1>();
                var interceptor2 = container.GetInstance<TInterceptor2>();
                var realInstance = container.GetInstance<TImplementation>();

                return Interceptor.CreateProxy<TService>(
                    interceptor1,
                    Interceptor.CreateProxy<TService>(
                        interceptor2,
                        realInstance));
            });  
        }

        public static void RegisterSingleWithInterceptor<TService, TInterceptor1, TInterceptor2, TImplementation>(
            this Container container)
            where TService : class
            where TInterceptor1 : class, IInterceptor
            where TInterceptor2 : class, IInterceptor
            where TImplementation : class, TService
        {
            container.RegisterSingle<TService>(() =>
            {
                var interceptor1 = container.GetInstance<TInterceptor1>();
                var interceptor2 = container.GetInstance<TInterceptor2>();
                var realInstance = container.GetInstance<TImplementation>();

                return Interceptor.CreateProxy<TService>(
                    interceptor1,
                    Interceptor.CreateProxy<TService>(
                        interceptor2,
                        realInstance));
            });  
        }
    }

    public static class Interceptor
    {
        public static T CreateProxy<T>(IInterceptor interceptor, T realInstance)
        {
            var proxy = new InterceptorProxy(typeof(T), realInstance, interceptor);

            return (T)proxy.GetTransparentProxy();
        }

        private sealed class InterceptorProxy : RealProxy
        {
            private object realInstance;
            private IInterceptor interceptor;

            public InterceptorProxy(Type classToProxy, object realInstance, IInterceptor interceptor)
                : base(classToProxy)
            {
                this.realInstance = realInstance;
                this.interceptor = interceptor;
            }

            public override IMessage Invoke(IMessage msg)
            {
                if (msg is IMethodCallMessage)
                {
                    return this.InvokeMethodCall((IMethodCallMessage)msg);
                }

                return msg;
            }

            private IMessage InvokeMethodCall(IMethodCallMessage message)
            {
                var invocation = new Invocation { Proxy = this, Message = message };

                invocation.Proceeding += (s, e) =>
                {
                    invocation.ReturnValue = message.MethodBase.Invoke(this.realInstance, message.Args);
                };

                this.interceptor.Intercept(invocation);

                return new ReturnMessage(invocation.ReturnValue, null, 0, null, message);
            }

            private class Invocation : IInvocation
            {
                public event EventHandler Proceeding;

                public InterceptorProxy Proxy { get; set; }

                public IMethodCallMessage Message { get; set; }

                public object ReturnValue { get; set; }

                public object InvocationTarget
                {
                    get { return this.Proxy.realInstance; }
                }

                public void Proceed()
                {
                    if (this.Proceeding != null)
                    {
                        this.Proceeding(this, EventArgs.Empty);
                    }
                }

                public MethodBase GetConcreteMethod()
                {
                    return this.Message.MethodBase;
                }
            }
        }
    }
}