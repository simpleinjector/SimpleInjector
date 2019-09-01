// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.ServiceCollection
{
    using Microsoft.Extensions.Localization;

    // This class wouldn't strictly be required, but since Microsoft could decide to add an extra ctor
    // to the Microsoft.Extensions.Localization.StringLocalizer<T> class, this sub type prevents this integration
    // package to break when this happens.
    internal sealed class StringLocalizer<T> : Microsoft.Extensions.Localization.StringLocalizer<T>
    {
        public StringLocalizer(IStringLocalizerFactory factory) : base(factory)
        {
        }
    }
}