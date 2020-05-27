// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace System.Threading
{
#if NET45
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Threading;

    internal sealed class AsyncLocal<T>
    {
        private readonly string key = Guid.NewGuid().ToString("N").Substring(0, 12);

        public T Value
        {
           [SecuritySafeCritical]
            get
            {
                var wrapper = (AsyncScopeWrapper?)CallContext.LogicalGetData(this.key);

                return wrapper != null ? wrapper.Value : default(T)!;
            }

            [SecuritySafeCritical]
            set
            {
                var wrapper = value is null ? null : new AsyncScopeWrapper(value);

                CallContext.LogicalSetData(this.key, wrapper);
            }
        }

        [Serializable]
        internal sealed class AsyncScopeWrapper : MarshalByRefObject
        {
            [NonSerialized]
            internal readonly T Value;

            internal AsyncScopeWrapper(T value)
            {
                this.Value = value;
            }
        }
    }
#endif
}