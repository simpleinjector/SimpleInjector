// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.ServiceCollection
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    internal class DefaultServiceProviderAccessor : IServiceProviderAccessor
    {
        private readonly Container container;
        private InstanceProducer? serviceScopeProducer;

        internal DefaultServiceProviderAccessor(Container container)
        {
            this.container = container;
        }

        public IServiceProvider Current => this.CurentServiceScope.ServiceProvider;

        private IServiceScope CurentServiceScope
        {
            get
            {
                if (this.serviceScopeProducer is null)
                {
                    this.serviceScopeProducer = this.container.GetRegistration(typeof(IServiceScope), true)!;
                }

                try
                {
                    // This resolved IServiceScope will be cached inside a Simple Injector Scope and will be
                    // disposed of when the scope is disposed of.
                    return (IServiceScope)this.serviceScopeProducer.GetInstance();
                }
                catch (Exception ex)
                {
                    var lifestyle = this.container.Options.DefaultScopedLifestyle;

                    // PERF: Since GetIstance() checks the availability of the scope itself internally, we
                    // would be duplicating the check (and duplicate the local storage access call). Doing
                    // this inside the catch, therefore, prevents having to do the check on every resolve.
                    if (Lifestyle.Scoped.GetCurrentScope(this.container) is null)
                    {
                        throw new ActivationException(
                             "You are trying to resolve a cross-wired service, but are doing so outside " +
                             $"the context of an active ({lifestyle?.Name}) scope. To be able to resolve " +
                             "this service the operation must run in the context of such scope. " +
                             "Please see https://simpleinjector.org/scoped for more information about how " +
                             "to manage scopes.", ex);
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