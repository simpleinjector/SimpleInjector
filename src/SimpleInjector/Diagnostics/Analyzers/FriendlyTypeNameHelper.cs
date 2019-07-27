// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics.Analyzers
{
    using System;

    internal static class FriendlyTypeNameHelper
    {
        internal static string FriendlyName(this Type type) =>
            type.ToFriendlyName(StringResources.UseFullyQualifiedTypeNames);
    }
}