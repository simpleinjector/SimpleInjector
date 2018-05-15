namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class KeyedFactory<TKey, TService> : IDictionary<TKey, TService> where TService : class
    {
        private readonly Container container;
        private readonly Dictionary<TKey, int> keyToIndexMap;
        
        private IList<TService> instances;

        public KeyedFactory(Container container) : this(container, null)
        {
        }

        public KeyedFactory(Container container, IEqualityComparer<TKey> comparer)
        {
            this.container = container;
            this.keyToIndexMap = new Dictionary<TKey, int>(comparer ?? EqualityComparer<TKey>.Default);
        }

        public ICollection<TKey> Keys
        {
            get { return this.keyToIndexMap.Keys; }
        }

        public ICollection<TService> Values
        {
            get { return this.Instances; }
        }

        public int Count
        {
            get { return this.Instances.Count; }
        }

        bool ICollection<KeyValuePair<TKey, TService>>.IsReadOnly
        {
            get { return true; }
        }

        private IList<TService> Instances
        {
            get
            {
                if (this.instances == null)
                {
                    this.instances = (IList<TService>)this.container.GetAllInstances<TService>();
                }

                return this.instances;
            }
        }

        public TService this[TKey key]
        {
            get { return this.Instances[this.keyToIndexMap[key]]; }
            set { throw new NotSupportedException(); }
        }

        public KeyedFactoryRegistration BeginRegistrations()
        {
            return new KeyedFactoryRegistration(this);
        }

        public bool ContainsKey(TKey key)
        {
            return this.keyToIndexMap.ContainsKey(key);
        }

        bool IDictionary<TKey, TService>.Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(TKey key, out TService value)
        {
            int index;

            if (this.keyToIndexMap.TryGetValue(key, out index))
            {
                value = this.Instances[index];
                return true;
            }
            else
            {
                value = default(TService);
                return false;
            }
        }

        void IDictionary<TKey, TService>.Add(TKey key, TService value)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TService>>.Add(KeyValuePair<TKey, TService> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TService>>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<TKey, TService>>.Contains(KeyValuePair<TKey, TService> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TService>>.CopyTo(KeyValuePair<TKey, TService>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<TKey, TService>>.Remove(KeyValuePair<TKey, TService> item)
        {
            throw new NotSupportedException();
        }

        IEnumerator<KeyValuePair<TKey, TService>> IEnumerable<KeyValuePair<TKey, TService>>.GetEnumerator()
        {
            foreach (var kvp in this.keyToIndexMap)
            {
                yield return new KeyValuePair<TKey, TService>(kvp.Key, this.Instances[kvp.Value]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TService>>)this).GetEnumerator();
        }

        private void RegisterImplementations(List<Tuple<TKey, Type, Lifestyle>> implementations)
        {
            // Register as list.
            this.container.Collection.Register<TService>(
                from tuple in implementations
                select tuple.Item2);

            foreach (var tuple in implementations)
            {
                if (tuple.Item3 == null)
                {
                    this.container.Register(tuple.Item2, tuple.Item2);
                }
                else
                {
                    this.container.Register(tuple.Item2, tuple.Item2, tuple.Item3);
                }
            }

            int index = 0;

            foreach (var tuple in implementations)
            {
                this.keyToIndexMap[tuple.Item1] = index++;
            }
        }

        public sealed class KeyedFactoryRegistration : IDisposable
        {
            private readonly KeyedFactory<TKey, TService> factory;
            private readonly List<Tuple<TKey, Type, Lifestyle>> implementations;
            private bool disposed;

            public KeyedFactoryRegistration(KeyedFactory<TKey, TService> factory)
            {
                this.factory = factory;
                this.implementations = new List<Tuple<TKey, Type, Lifestyle>>();
            }

            public void Register<TImplementation>(TKey key) where TImplementation : TService
            {
                this.Register(typeof(TImplementation), key);
            }

            public void Register<TImplementation>(TKey key, Lifestyle lifestyle)
                where TImplementation : TService
            {
                this.Register(typeof(TImplementation), key, lifestyle);
            }

            public void Register(Type implementationType, TKey key)
            {
                this.Register(implementationType, key, null);
            }

            public void Register(Type implementationType, TKey key, Lifestyle lifestyle)
            {
                this.ThrowWhenDisposed();

                if (!typeof(TService).IsAssignableFrom(implementationType))
                {
                    throw new ArgumentException(implementationType.Name + " is not a subtype.");
                }

                if (this.implementations.Any(tuple => tuple.Item2 == implementationType))
                {
                    throw new ArgumentException(implementationType.Name + " is already registered.");
                }

                this.implementations.Add(Tuple.Create(key, implementationType, lifestyle));
            }

            public void Dispose()
            {
                if (!this.disposed)
                {
                    this.factory.RegisterImplementations(this.implementations);

                    this.disposed = true;
                }
            }

            private void ThrowWhenDisposed()
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }
            }
        }
    }
}