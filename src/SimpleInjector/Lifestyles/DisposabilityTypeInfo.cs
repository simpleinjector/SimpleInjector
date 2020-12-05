// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;
    using SimpleInjector.Internals;

    internal enum Disposability : byte { Always, Never, Maybe };

    internal struct DisposabilityTypeInfo
    {
        public readonly Disposability Sync;
        public readonly Disposability Async;

        public DisposabilityTypeInfo(Type type, ScopedRegistration registration)
        {
            // The use of an interceptor or instance creator might produce sub types. Those sub types
            // could implement IDisposable, even if the ImplementationType itself does not. Most
            // common in the case of an instance creator as the ImplementationType will often be the
            // abstraction, and abstractions usually don't implement IDisposable.
            bool expressionIntercepted = registration.ExpressionIntercepted
                ?? throw new InvalidOperationException(
                    "IsDisposable should be called after BuildExpression.");

            Disposability subTypable =
                !registration.ImplementationType.IsSealed()
                && (expressionIntercepted || registration.instanceCreator != null)
                ? Disposability.Maybe
                : Disposability.Never;

            this.Sync = typeof(IDisposable).IsAssignableFrom(type)
                ? Disposability.Always
                : subTypable;

            this.Async = DisposableHelpers.IsAsyncDisposableType(type)
                ? Disposability.Always
                : subTypable;
        }
    }
}