// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Diagnostics;

    // This class replaces the .NET's default Lazy<T> implementation. The behavior of the
    // ExecutionAndPublication mode of the default implementation is unsuited for use inside Simple Injector
    // because it will ensure that the factory is called just once, which means it caches any thrown exception.
    // That behavior is problematic because it can cause corruption of Simple Injector; for instance when a
    // ThreadAbortException is thrown during the execution of the factory. See: #731.
    // This implementation behaves different to Lazy<T> in the following respects:
    // * It only supports reference types
    // * It does not allow the Value to be null.
    // * It only supports a single mode, which is a mix between ExecutionAndPublication and PublicationOnly.
    //   This mode has the following behavior:
    //   * It ensures the call to the factory is synchronized (like ExecutionAndPublication)
    //   * It ensures only one successful call to factory is made (like ExecutionAndPublication)
    //   * It will not cache any thrown exception and allow the factory to be called again (like PublicationOnly)
    // Simple Injector internally used Lazy<T>, especially with the ExecutionAndPublication mode, to ensure
    // that Singletons are guaranteed to be created just once. Replacing Lazy<T> will not per se cause this
    // guarantee to be broken, but now the underlying construct needs to make care that the guarantee isn't
    // broken. In the majority of cases, however, this loosening of constraints is perfectly fine.
    [DebuggerDisplay("IsValueCreated={IsValueCreated}, Value={value}")]
    internal sealed class LazyEx<T>
        where T : class
    {
        private Func<T>? factory;
        private T? value;

        public LazyEx(Func<T> valueFactory)
        {
            Requires.IsNotNull(valueFactory, nameof(valueFactory));

            this.factory = valueFactory;
        }

        public LazyEx(T value)
        {
            Requires.IsNotNull(value, nameof(value));

            this.value = value;
        }

        public bool IsValueCreated => !(this.value is null);

        public T Value
        {
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get => this.value ?? this.InitializeAndReturn();
        }

        public override string ToString() =>
            !this.IsValueCreated ? "Value is not created." : this.Value.ToString();

        private T InitializeAndReturn()
        {
            // NOTE: Locking on 'this' is typically not adviced, but this type is internal, which means the
            // risk is minimal. Locking on 'this' allows us to safe some bytes for the extra lock object.
            // OPTIMIZATION: Because this is a very common code path, and very regularly part of a
            // user's stack trace, this code is inlined here to make the call stack shorter and more
            // readable (for user's and maintainers).
            lock (this)
            {
                if (this.value is null)
                {
                    this.value = this.factory!();

                    if (this.value is null)
                    {
                        throw new InvalidOperationException("The valueFactory produced null.");
                    }

                    // We don't need the factory any longer. It might now be eligible for garbage collection.
                    this.factory = null;
                }

                return this.value;
            }
        }
    }
}