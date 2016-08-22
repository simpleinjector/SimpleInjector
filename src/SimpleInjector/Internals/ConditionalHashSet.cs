#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2016 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ConditionalHashSet<T> where T : class
    {
        private const int ShrinkStepCount = 100;

        private static readonly Predicate<WeakReference> IsDead = reference => !reference.IsAlive;

        private readonly Dictionary<int, List<WeakReference>> dictionary = 
            new Dictionary<int, List<WeakReference>>();

        private int shrinkCount = 0;

        internal void Add(T item)
        {
            Requires.IsNotNull(item, nameof(item));

            lock (this.dictionary)
            {
                if (this.GetWeakReferenceOrNull(item) == null)
                {
                    var weakReference = new WeakReference(item);

                    int key = weakReference.Target.GetHashCode();

                    List<WeakReference> bucket;

                    if (!this.dictionary.TryGetValue(key, out bucket))
                    {
                        this.dictionary[key] = bucket = new List<WeakReference>(capacity: 1);
                    }

                    bucket.Add(weakReference);
                }
            }
        }

        internal void Remove(T item)
        {
            Requires.IsNotNull(item, nameof(item));

            lock (this.dictionary)
            {
                WeakReference reference = this.GetWeakReferenceOrNull(item);

                if (reference != null)
                {
                    reference.Target = null;
                }

                if ((++this.shrinkCount % ShrinkStepCount) == 0)
                {
                    this.RemoveDeadItems();
                }
            }
        }

        internal T[] GetLivingItems()
        {
            lock (this.dictionary)
            {
                var producers =
                    from pair in this.dictionary
                    from reference in pair.Value
                    let target = reference.Target
                    where !object.ReferenceEquals(target, null)
                    select (T)target;

                return producers.ToArray();
            }
        }

        private WeakReference GetWeakReferenceOrNull(T item)
        {
            List<WeakReference> bucket;

            if (this.dictionary.TryGetValue(item.GetHashCode(), out bucket))
            {
                foreach (var reference in bucket)
                {
                    if (object.ReferenceEquals(item, reference.Target))
                    {
                        return reference;
                    }
                }
            }

            return null;
        }

        private void RemoveDeadItems()
        {
            foreach (int key in this.dictionary.Keys.ToArray())
            {
                var bucket = this.dictionary[key];

                bucket.RemoveAll(IsDead);

                // Remove empty buckets.
                if (bucket.Count == 0)
                {
                    this.dictionary.Remove(key);
                }
            }
        }
    }
}