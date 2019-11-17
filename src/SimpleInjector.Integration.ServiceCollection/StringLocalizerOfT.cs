// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.ServiceCollection
{
    using Microsoft.Extensions.Localization;

    // This class will only be used when a future version of Microsoft.Extensions.Localization
    // .StringLocalizer<T> gets a second constructor. In that case Simple Injector can't resolve
    // Microsoft.Extensions.Localization.StringLocalizer<T> and it falls back to this self-defined class.
    internal sealed class StringLocalizer<T> : Microsoft.Extensions.Localization.StringLocalizer<T>
    {
        public StringLocalizer(IStringLocalizerFactory factory) : base(factory)
        {
        }
    }
}