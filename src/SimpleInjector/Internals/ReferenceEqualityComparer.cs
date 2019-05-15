// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        internal static readonly ReferenceEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();

        [DebuggerStepThrough]
        public bool Equals(T x, T y) => object.ReferenceEquals(x, y);

        [DebuggerStepThrough]
        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}