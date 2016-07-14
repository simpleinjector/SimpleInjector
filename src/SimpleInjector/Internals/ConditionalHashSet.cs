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

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;

    internal sealed class ConditionalHashSet<T> where T : class
    {
        private readonly List<WeakReference> list = new List<WeakReference>();

        public IEnumerable<T> Keys
        {
            get
            {
                lock (this.list)
                {
                    var keys = new HashSet<T>(ReferenceEqualityComparer<T>.Instance);

                    foreach (var weakReference in this.list)
                    {
                        var item = (T)weakReference.Target;

                        if (!object.ReferenceEquals(null, item))
                        {
                            keys.Add(item);
                        }
                    }

                    return keys;
                }
            }
        }

        // Add is O(1)
        public void Add(T item)
        {
            Requires.IsNotNull(item, nameof(item));

            lock (this.list)
            {
                // NOTE: item can exist multiple times in the list
                this.list.Add(new WeakReference(item));
            }
        }

        // Remove is O(n^2)
        public void Remove(T item)
        {
            Requires.IsNotNull(item, nameof(item));

            lock (this.list)
            {
                RemoveItem(item);
                Shrink();
            }
        }

        private void RemoveItem(T item)
        {
            // Always loop the complete list: item can exist multiple times.
            for (int index = this.list.Count - 1; index >= 0; index--)
            {
                if (object.ReferenceEquals(item, this.list[index].Target))
                {
                    this.list.RemoveAt(index);
                }
            }
        }

        private void Shrink()
        {
            for (int index = this.list.Count - 1; index >= 0; index--)
            {
                if (!this.list[index].IsAlive)
                {
                    this.list.RemoveAt(index);
                }
            }
        }
    }
}