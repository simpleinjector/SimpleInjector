[assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]

namespace SimpleInjector.Interception
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;

    internal sealed class InterceptorProxy : RealProxy
    {
        private IInterceptor interceptor;
        private object interceptee;

        public InterceptorProxy(Type interfaceToProxy, object interceptee, IInterceptor interceptor)
            : base(interfaceToProxy)
        {
            if (interceptor == null)
            {
                throw new ArgumentNullException("interceptor");
            }

            if (interceptee == null)
            {
                throw new ArgumentNullException("interceptee");
            }

            this.interceptee = interceptee;
            this.interceptor = interceptor;
        }

        public override IMessage Invoke(IMessage msg)
        {
            var message = msg as IMethodCallMessage;

            if (message != null)
            {
                return this.InvokeMethodCall(message);
            }

            return msg;
        }

        private IMessage InvokeMethodCall(IMethodCallMessage message)
        {
            var invocation = new Invocation { Proxy = this, Message = message };

            invocation.Proceeding = () =>
            {
                invocation.ReturnValue = message.MethodBase.Invoke(this.interceptee, message.Args);
            };

            if (message.MethodBase.DeclaringType != typeof(object))
            {
                this.interceptor.Intercept(invocation);
            }
            else
            {
                invocation.Proceeding();
            }

            return new ReturnMessage(invocation.ReturnValue, null, 0, null, message);
        }

        private sealed class Invocation : IInvocation
        {
            public IEnumerable<object> Arguments
            {
                get { return this.Message.Args; }
            }

            public object ReturnValue { get; set; }

            public object Target
            {
                get { return this.Proxy.interceptee; }
            }

            public MethodBase Method
            {
                get { return this.Message.MethodBase; }
            }

            internal Action Proceeding { get; set; }

            internal InterceptorProxy Proxy { get; set; }

            internal IMethodCallMessage Message { get; set; }
           
            public void Proceed()
            {
                if (this.Proceeding != null)
                {
                    this.Proceeding();
                }
            }
        }
    }
}