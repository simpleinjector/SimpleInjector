#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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

namespace SimpleInjector.Advanced
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class ConditionalHashSet<T> where T : class
    {
        private readonly object locker = new object();
        private readonly List<WeakReference> weakList = new List<WeakReference>();
        private readonly ConditionalWeakTable<T, WeakReference> weakDictionary =
            new ConditionalWeakTable<T, WeakReference>();

        public T[] Keys
        {
            get
            {
                lock (this.locker)
                {
                    return (
                        from weakReference in this.weakList
                        let item = (T)weakReference.Target
                        where item != null
                        select item)
                        .ToArray();
                }
            }
        }

        public void Add(T item)
        {
            Requires.IsNotNull(item, "item");

            lock (this.locker)
            {
                var reference = new WeakReference(item);

                this.weakDictionary.Add(item, reference);

                this.weakList.Add(reference);

                this.Shrink();
            }
        }

        public void Remove(T item)
        {
            Requires.IsNotNull(item, "item");

            lock (this.locker)
            {
                WeakReference reference;

                if (this.weakDictionary.TryGetValue(item, out reference))
                {
                    reference.Target = null;

                    this.weakDictionary.Remove(item);
                }
            }
        }

        private void Shrink()
        {
            if (this.weakList.Capacity == this.weakList.Count)
            {
                this.RemoveAll(weak => !weak.IsAlive);
            }
        }

        private void RemoveAll(Predicate<WeakReference> match)
        {
            var items = this.weakList.Where(item => !match(item)).ToArray();

            this.weakList.Clear();

            this.weakList.AddRange(items);
        }
    }
}