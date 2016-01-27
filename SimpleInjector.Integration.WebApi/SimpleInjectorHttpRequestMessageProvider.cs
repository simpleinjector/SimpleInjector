#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014 Simple Injector Contributors
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

namespace SimpleInjector.Integration.WebApi
{
    using System;
    using System.Net.Http;
    using System.Runtime.Remoting.Messaging;

    internal static class SimpleInjectorHttpRequestMessageProvider
    {
        private static readonly string Key = Guid.NewGuid().ToString("N").Substring(0, 12);

        internal static HttpRequestMessage CurrentMessage
        {
            get
            {
                var wrapper = (HttpRequestMessageWrapper)CallContext.LogicalGetData(Key);

                return wrapper != null ? wrapper.Message : null;
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
            [NonSerializedAttribute]
            internal readonly HttpRequestMessage Message;

            internal HttpRequestMessageWrapper(HttpRequestMessage message)
            {
                this.Message = message;
            }
        }
    }
}