namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class KeyedRegistrations<TKey, TService> : IEnumerable<TService> where TService : class
    {
        private readonly List<InstanceProducer> producers = new List<InstanceProducer>();
        private readonly Dictionary<TKey, InstanceProducer> keyedProducers;
        private readonly Container container;

        public KeyedRegistrations(Container container) : this(container, EqualityComparer<TKey>.Default)
        {
        }

        public KeyedRegistrations(Container container, IEqualityComparer<TKey> comparer)
        {
            this.container = container;
            this.keyedProducers = new Dictionary<TKey, InstanceProducer>(comparer);
        }
        
        public TService GetInstance(TKey key)
        {
            return (TService)this.keyedProducers[key].GetInstance();
        }

        public void Register<TImplementation>(TKey key) where TImplementation : TService
        {
            this.Register(typeof(TImplementation), key);
        }

        public void Register(Type implementationType, TKey key)
        {
            this.Register(implementationType, key, this.GetDefaultLifestyle(implementationType));
        }

        public void Register(Type implementationType, TKey key, Lifestyle lifestyle)
        {
            this.Register(lifestyle.CreateRegistration(implementationType, this.container), key);
        }
        
        public void Register(Func<TService> instanceCreator, TKey key)
        {
            this.Register(instanceCreator, key, this.GetDefaultLifestyle(typeof(TService)));
        }

        public void Register(Func<TService> instanceCreator, TKey key, Lifestyle lifestyle)
        {
            this.Register(lifestyle.CreateRegistration(typeof(TService), instanceCreator, this.container), key);
        }

        public InstanceProducer GetRegistration(TKey key)
        {
            return this.keyedProducers[key];
        }

        public IEnumerator<TService> GetEnumerator()
        {
            foreach (var producer in this.producers)
            {
                yield return (TService)producer.GetInstance();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void Register(Registration registration, TKey key)
        {
            var producer = new InstanceProducer(typeof(TService), registration);

            this.keyedProducers.Add(key, producer);
            this.producers.Add(producer);
        }

        private Lifestyle GetDefaultLifestyle(Type implementationType)
        {
            return this.container.Options.LifestyleSelectionBehavior
                .SelectLifestyle(implementationType);
        }
    }
}