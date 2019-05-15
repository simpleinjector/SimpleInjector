// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.AspNetCore
{
    using System;
    using Microsoft.AspNetCore.Http;
    using SimpleInjector.Integration.ServiceCollection;

    internal class AspNetCoreServiceProviderAccessor : IServiceProviderAccessor
    {
        private readonly IHttpContextAccessor accessor;
        private readonly IServiceProviderAccessor decoratee;

        internal AspNetCoreServiceProviderAccessor(
            IHttpContextAccessor accessor,
            IServiceProviderAccessor decoratee)
        {
            this.decoratee = decoratee;
            this.accessor = accessor;
        }

        public IServiceProvider Current =>
            this.accessor.HttpContext?.RequestServices ?? this.decoratee.Current;
    }
}