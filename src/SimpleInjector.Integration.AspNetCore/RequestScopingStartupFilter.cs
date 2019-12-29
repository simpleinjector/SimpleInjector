// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.AspNetCore
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

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
                this.ConfigureRequestScoping(builder);

                next(builder);
            };
        }

        private void ConfigureRequestScoping(IApplicationBuilder builder)
        {
            builder.Use(async (_, next) =>
            {
                using (var scope = AsyncScopedLifestyle.BeginScope(this.container))
                {
                    await next();

                    await DisposeAsync(scope);
                }
            });
        }

        private static async ValueTask DisposeAsync(Scope scope)
        {
            IDisposable[] disposables = scope.GetDisposables();

            // Dispose in reverse order
            for (int index = disposables.Length - 1; index >= 0; index--)
            {
                if (disposables[index] is IAsyncDisposable asyncDisposable)
                {
                    // We don't need to catch-and-continue; Scope will guarantee everything is disposed.
                    await asyncDisposable.DisposeAsync();
                }
            }
        }
    }
}