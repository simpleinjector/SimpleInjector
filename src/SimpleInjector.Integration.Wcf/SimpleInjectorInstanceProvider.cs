// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.Wcf
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using SimpleInjector.Lifestyles;

    internal class SimpleInjectorInstanceProvider : IInstanceProvider
    {
        private readonly Container container;
        private readonly Type serviceType;

        public SimpleInjectorInstanceProvider(Container container, Type serviceType)
        {
            this.container = container;
            this.serviceType = serviceType;
        }

        public object GetInstance(InstanceContext instanceContext, Message message) => this.GetInstance(instanceContext);

        public object GetInstance(InstanceContext instanceContext)
        {
            Requires.IsNotNull(instanceContext, nameof(instanceContext));

            Scope scope = AsyncScopedLifestyle.BeginScope(this.container);

            // During the time that WCF calls ReleaseInstance, the ambient context provided by AsyncLocal<T> will be reset and 
            // AsyncScopedLifestyle.GetCurrentScope will return null. That's why we have to attach the scope to the current
            // InstanceContext. This way we can still dispose the Scope during ReleaseInstance.
            Attach(instanceContext, scope);

            try
            {
                return this.container.GetInstance(this.serviceType);
            }
            catch
            {
                // We need to dispose the scope here, because WCF will never call ReleaseInstance if
                // this method throws an exception (since it has no instance to pass to ReleaseInstance).
                scope.Dispose();

                throw;
            }
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            Requires.IsNotNull(instanceContext, nameof(instanceContext));

            var extension = instanceContext.Extensions.Find<InstanceContextScopeWrapper>();

            extension?.Scope?.Dispose();
        }

        private static void Attach(InstanceContext instanceContext, Scope scope)
        {
            var extension = instanceContext.Extensions.Find<InstanceContextScopeWrapper>();

            if (extension == null)
            {
                instanceContext.Extensions.Add(extension = new InstanceContextScopeWrapper());
            }

            if (extension.Scope == null)
            {
                extension.Scope = scope;
            }
        }

        private sealed class InstanceContextScopeWrapper : IExtension<InstanceContext>
        {
            internal Scope Scope { get; set; }

            public void Attach(InstanceContext owner)
            {
            }

            public void Detach(InstanceContext owner)
            {
            }
        }
    }
}