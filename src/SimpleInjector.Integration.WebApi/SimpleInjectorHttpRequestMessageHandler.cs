// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.WebApi
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class SimpleInjectorHttpRequestMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SimpleInjectorHttpRequestMessageProvider.CurrentMessage = request;

            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            finally
            {
                // Fixes #628. Not clearing the current message caused a memory leak under .NET < v4.7.
                SimpleInjectorHttpRequestMessageProvider.CurrentMessage = null;
            }
        }
    }
}