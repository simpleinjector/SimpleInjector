// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Threading;

    // The idea of this implementation is loosely based on code from ProtoActor. See:
    // https://github.com/AsynkronIT/protoactor-dotnet/blob/dev/src/Proto.Actor/Utils/TypedDictionary.cs
    /// <summary>
    /// Provides O(1) lookup based on compile-time available Types as key. In case of creation of multiple
    /// <see cref="DictionaryOfType{TValue}"/> instances, the internal array can grow considerably in size
    /// with lots of empty 'holes'.
    /// </summary>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    internal sealed class DictionaryOfType<TValue>
    {
        private static int TypeCounter = -1;

        private readonly object locker = new();

        // If this class is used in multiple Container instance that each have a different set types, there
        // will be many empty values in this array. The array could grow quite large.
        private Bucket[] buckets;

        public DictionaryOfType()
        {
            this.buckets = new Bucket[16];
        }

        public bool TryGetValue<TKey>(out TValue? value)
        {
            var index = TypeIndex<TKey>.Index;

            // No need to lock, because Array.Resize is an atomic operation, although cache misses could
            // theoretically occur. Cache misses are not a problem for Simple Injector, because in that case
            // the instance is simply added again.
            if (index < this.buckets.Length)
            {
                var bucket = this.buckets[index];
                value = bucket.Value;
                return bucket.Exists;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public void Add<TKey>(TValue value)
        {
            lock (this.locker)
            {
                var index = TypeIndex<TKey>.Index;

                // Because there could exist multiple instances of this class, each with their own set of
                // types, it could be that doubling the array once is not enough. We, therefore, keep doubling
                // the array in size until the index would fit in the array.
                if (index >= this.buckets.Length)
                {
                    int newSize = this.buckets.Length;

                    // Double the size until it fits.
                    while (index >= newSize)
                    {
                        newSize *= 2;
                    }

                    // Resize is atomic. It creates a new array, copies the values in, and does the reference
                    // swap after the copying is done.
                    Array.Resize(ref this.buckets, newSize);
                }

                this.buckets[index] = new(value);
            }
        }

        private static class TypeIndex<TKey>
        {
            internal static readonly int Index = Interlocked.Increment(ref TypeCounter);
        }

        private struct Bucket
        {
            public readonly TValue? Value;
            public readonly bool Exists;

            public Bucket(TValue? value)
            {
                this.Value = value;
                this.Exists = true;
            }
        }
    }
}