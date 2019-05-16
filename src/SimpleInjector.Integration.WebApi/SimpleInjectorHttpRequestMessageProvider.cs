// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.WebApi
{
    using System;
    using System.Net.Http;
    using System.Runtime.Remoting.Messaging;

    internal static class SimpleInjectorHttpRequestMessageProvider
    {
        private static readonly string Key = Guid.NewGuid().ToString("N").Substring(0, 12);

        internal static HttpRequestMessage? CurrentMessage
        {
            get
            {
                var wrapper = (HttpRequestMessageWrapper?)CallContext.LogicalGetData(Key);

                return wrapper?.Message;
            }

            set
            {
                var wrapper = value == null ? null : new HttpRequestMessageWrapper(value);

                CallContext.LogicalSetData(Key, wrapper);
            }
        }

        [Serializable]
        internal sealed class HttpRequestMessageWrapper : MarshalByRefObject
        {
            [NonSerialized]
            internal readonly HttpRequestMessage Message;

            internal HttpRequestMessageWrapper(HttpRequestMessage message)
            {
                this.Message = message;
            }
        }
    }
}