namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public static class DictionaryExtensions
    {
        public static void RegisterDictionary<TKey, TService>(
            this ContainerCollectionRegistrator registrator, Func<TService, TKey> keySelector)
                where TService : class
        {
            registrator.Container.RegisterInstance<IReadOnlyDictionary<TKey, TService>>(
                new ResolveDictionary<TKey, TService>(registrator.Container, keySelector));
        }

        private sealed class ResolveDictionary<TKey, TService> : IReadOnlyDictionary<TKey, TService>
            where TService : class
        {
            private readonly Lazy<IDictionary<TKey, Func<TService>>> lazy;

            public ResolveDictionary(Container container, Func<TService, TKey> keySelector)
            {
                // The lazy ensures that the dictionary is initialized lazily.
                this.lazy = new Lazy<IDictionary<TKey, Func<TService>>>(() =>
                {
                    // Initializing lazily is needed, because all instances need to be created to determine
                    // their key, but as those instances (or their dependencies) could be scoped, an active
                    // scope needs to exist for this. This is an unfortunate downside from having a key
                    // selector that requires an instance.
                    var services = container.GetInstance<IReadOnlyList<TService>>();

                    var dictionary = new Dictionary<TKey, Func<TService>>();

                    for (int i = 0; i < services.Count; i++)
                    {
                        TService service = services[i];
                        TKey key = keySelector(service);

                        // i needs to be assigned to index to prevent, otherwise when wrapped inside the
                        // closure, i will always have the value of services.Count.
                        int index = i;
                        Func<TService> instanceCreator = () => services[index];

                        dictionary.Add(key, instanceCreator);
                    }

                    return dictionary;
                });
            }

            public IEnumerable<TKey> Keys => this.Dictionary.Keys;
            public IEnumerable<TService> Values => this.Dictionary.Values.Select(v => v());
            public int Count => this.Dictionary.Count;

            private IDictionary<TKey, Func<TService>> Dictionary => this.lazy.Value;

            public TService this[TKey key] => this.Dictionary[key]();

            public bool ContainsKey(TKey key) => this.Dictionary.ContainsKey(key);

            public IEnumerator<KeyValuePair<TKey, TService>> GetEnumerator()
            {
                foreach (var pair in this.Dictionary)
                {
                    yield return new KeyValuePair<TKey, TService>(pair.Key, pair.Value());
                }
            }

            public bool TryGetValue(TKey key, out TService value)
            {
                if (this.Dictionary.TryGetValue(key, out Func<TService> f))
                {
                    value = f();
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }
    }
}