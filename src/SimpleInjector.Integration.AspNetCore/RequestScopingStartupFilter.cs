// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.AspNetCore
{
    using System;
    using Lifestyles;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;

    internal sealed class RequestScopingStartupFilter : IStartupFilter
    {
        private readonly Container container;

        public RequestScopingStartupFilter(Container container)
        {
            this.container = container;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                ConfigureRequestScoping(builder);

                next(builder);
            };
        }

        private void ConfigureRequestScoping(IApplicationBuilder builder)
        {
            builder.Use(async (context, next) =>
            {
                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    await next();
                }
            });
        }
    }
}