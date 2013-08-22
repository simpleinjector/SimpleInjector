namespace SimpleInjector.Advanced.Internal
{
    // Unfortunately we had to make this class public to allow emitting an on the fly type that inherits
    // from this class.
    // This class behaves a lot like the ReadOnlyCollection<T> in the sense that it doesn't support changes
    // to the collection. In the future we might be able to implement IReadOnlyList<out T>, but this interface
    // is new in .NET 4.5 and Simple Injector needs to stay compatible with .NET 4.0 for quite some time.
    // The ReadOnlyContainerControlledCollectionTypeBuilder solves this.
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>This class is not meant for public use.</summary>
    /// <typeparam name="T">The type T.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", 
        MessageId = "Indexable",
        Justification = "I think it's a word.")]
    public abstract class IndexableCollection<T> : IList<T>
    {
        /// <summary>Gets the number of elements.</summary>
        /// <value>The number of elements.</value>
        public abstract int Count { get; }

        /// <summary>Gets a value indicating whether true.</summary>
        /// /// <value>The true value.</value>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>Please, Do not use.</summary>
        /// <param name="index">Do not use.</param>
        /// <returns>Really, Do not use.</returns>
        public abstract T this[int index] { get; set; }

        /// <summary>Please, Do not use.</summary>
        /// <param name="item">Do not use.</param>
        /// <returns>Really, Do not use.</returns>
        public int IndexOf(T item)
        {
            throw GetNotSupportedException();
        }

        /// <summary>Please, Do not use.</summary>
        /// <param name="index">Do not use.</param>
        /// <param name="item">Really, Do not use.</param>
        public void Insert(int index, T item)
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        /// <summary>Please, Do not use.</summary>
        /// <param name="index">Do not use.</param>
        public void RemoveAt(int index)
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        /// <summary>Please, Do not use.</summary>
        /// <param name="item">Do not use.</param>
        public void Add(T item)
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        /// <summary>Do not use.</summary>
        public void Clear()
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        /// <summary>Please, Do not use.</summary>
        /// <param name="item">Do not use.</param>
        /// <returns>Really, Do not use.</returns>
        public bool Contains(T item)
        {
            throw GetNotSupportedException();
        }

        /// <summary>Please, Do not use.</summary>
        /// <param name="array">Do not use.</param>
        /// <param name="arrayIndex">Really, Do not use.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        /// <summary>Please, Do not use.</summary>
        /// <param name="item">Do not use.</param>
        /// <returns>Really, Do not use.</returns>
        public bool Remove(T item)
        {
            throw GetNotSupportedBecauseCollectionIsReadOnlyException();
        }

        /// <summary>Do not use.</summary>
        /// <returns>Really, Do not use.</returns>
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal static NotSupportedException GetNotSupportedBecauseCollectionIsReadOnlyException()
        {
            return new NotSupportedException("Collection is read-only.");
        }

        private static NotSupportedException GetNotSupportedException()
        {
            return new NotSupportedException();
        }
    }
}