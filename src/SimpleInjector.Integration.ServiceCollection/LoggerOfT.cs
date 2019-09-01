// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.ServiceCollection
{
    using Microsoft.Extensions.Logging;

    // This class wouldn't strictly be required, but since Microsoft could decide to add an extra ctor
    // to the Microsoft.Extensions.Logging.Logger<T> class, this sub type prevents this integration
    // package to break when this happens.
    internal sealed class Logger<T> : Microsoft.Extensions.Logging.Logger<T>
    {
        // This constructor needs to be public for Simple Injector to create this type.
        public Logger(ILoggerFactory factory) : base(factory)
        {
        }
    }
}