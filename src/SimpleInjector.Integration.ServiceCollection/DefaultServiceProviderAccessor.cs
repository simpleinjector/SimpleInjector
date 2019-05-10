#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2019 Simple Injector Contributors
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

namespace SimpleInjector.Integration.ServiceCollection
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    internal class DefaultServiceProviderAccessor : IServiceProviderAccessor
    {
        private readonly Container container;
        private InstanceProducer serviceScopeProducer;

        internal DefaultServiceProviderAccessor(Container container)
        {
            this.container = container;
        }

        public IServiceProvider Current => this.CurentServiceScope.ServiceProvider;

        private IServiceScope CurentServiceScope
        {
            get
            {
                if (this.serviceScopeProducer == null)
                {
                    this.serviceScopeProducer = this.container.GetRegistration(typeof(IServiceScope), true);
                }

                try
                {
                    // This resolved IServiceScope will be cached inside a Simple Injector Scope and will be
                    // disposed of when the scope is disposed of.
                    return (IServiceScope)this.serviceScopeProducer.GetInstance();
                }
                catch (Exception)
                {
                    var lifestyle = this.container.Options.DefaultScopedLifestyle;

                    // PERF: Since GetIstance() checks the availability of the scope itself internally, we
                    // would be duplicating the check (and duplicate the local storage access call). Doing
                    // this inside the catch, therefore, prevents having to do the check on every resolve.
                    if (Lifestyle.Scoped.GetCurrentScope(this.container) is null)
                    {
                        throw new ActivationException(
                             "You are trying to resolve a cross-wired service, but are doing so outside " +
                             $"the context of an active ({lifestyle.Name}) scope. To be able to resolve " +
                             "this service the operation must run in the context of such scope. " +
                             "Please see https://simpleinjector.org/scoped for more information about how " +
                             "to manage scopes.");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}