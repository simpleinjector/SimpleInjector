#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2018 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

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