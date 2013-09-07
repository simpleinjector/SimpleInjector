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
                this.weakList.RemoveAll(weak => !weak.IsAlive);
            }
        }
    }
}