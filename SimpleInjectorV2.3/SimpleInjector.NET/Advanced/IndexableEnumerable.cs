namespace SimpleInjector.Advanced
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    // This class behaves a lot like the ReadOnlyCollection<T> in the sense that it doesn't support changes
    // to the collection. In the future we might be able to implement IReadOnlyList<out T>, but this interface
    // is new in .NET 4.5 and Simple Injector needs to stay compatible with .NET 4.0 for quite some time.
    internal abstract class IndexableEnumerable<T> : IList<T>
    {
        public abstract int Count { get; }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public abstract T this[int index] { get; set; }

        public int IndexOf(T item)
        {
            throw GetNotSupportedException();
        }

        public void Insert(int index, T item)
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        public void RemoveAt(int index)
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }
        
        public void Add(T item)
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        public void Clear()
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        public bool Contains(T item)
        {
            throw GetNotSupportedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        public bool Remove(T item)
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        
        protected static NotSupportedException GetNotSupportedBecauseCollectionIsReadOnlyException()
        {
            return new NotSupportedException("Collection is read-only.");
        }

        private static NotSupportedException GetNotSupportedException()
        {
            return new NotSupportedException();
        }
    }
}