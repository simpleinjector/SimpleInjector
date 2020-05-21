// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace System.Diagnostics.CodeAnalysis
{
#if NETSTANDARD1_0 || NETSTANDARD1_3
    [Conditional("EXCLUDED")]
    [AttributeUsage(
        AttributeTargets.Assembly
        | AttributeTargets.Class
        | AttributeTargets.Constructor
        | AttributeTargets.Event
        | AttributeTargets.Method
        | AttributeTargets.Property
        | AttributeTargets.Struct,
        AllowMultiple = false,
        Inherited = false)]
    internal sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
    }
#endif
}