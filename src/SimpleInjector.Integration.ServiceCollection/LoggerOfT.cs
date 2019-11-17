// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.ServiceCollection
{
    using Microsoft.Extensions.Logging;

    // This class will only be used when a future version of Microsoft.Extensions.Logging.Logger<T> gets a
    // second constructor. In that case Simple Injector can't resolve Microsoft.Extensions.Logging.Logger<T>
    // and it falls back to this self-defined class.
    internal sealed class Logger<T> : Microsoft.Extensions.Logging.Logger<T>
    {
        // This constructor needs to be public for Simple Injector to create this type.
        public Logger(ILoggerFactory factory) : base(factory)
        {
        }
    }
}