namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using SimpleInjector;

    // WARNING: Using this extension method could considerably lower the performance. 
    // Use with care.
    public static class ImplicitPropertyInjectionExtensions
    {
        private static Dictionary<Type, InstanceInitializer> instanceInitializers =
            new Dictionary<Type, InstanceInitializer>();

        public static void AllowImplicitPropertyInjection(this Container container)
        {
            container.AllowImplicitPropertyInjectionOn<object>();
        }

        public static void AllowImplicitPropertyInjectionOn<TService>(
            this Container container) where TService : class
        {
            Action<TService> initializer =
                new ImplicitPropertyInjector<TService>(container).InitializeInstance;

            container.RegisterInitializer<TService>(initializer);
        }

        private sealed class ImplicitPropertyInjector<TService>
        {
            private readonly Container container;

            public ImplicitPropertyInjector(Container container)
            {
                this.container = container;
            }

            [DebuggerStepThrough]
            public void InitializeInstance(TService instance)
            {
                var initializer = this.GetInstanceInitializerFor(instance.GetType());
                initializer.InitializeInstance(instance);
            }

            [DebuggerStepThrough]
            private InstanceInitializer GetInstanceInitializerFor(Type type)
            {
                var snapshot = instanceInitializers;

                InstanceInitializer initializer;

                if (!snapshot.TryGetValue(type, out initializer))
                {
                    initializer = new InstanceInitializer(this.container);

                    StoreInitializerInCache(type, initializer, snapshot);
                }

                return initializer;
            }

            [DebuggerStepThrough]
            private static void StoreInitializerInCache(Type type, 
                InstanceInitializer initializer,
                Dictionary<Type, InstanceInitializer> snapshot)
            {
                var copy = new Dictionary<Type, InstanceInitializer>(snapshot);
                copy[type] = initializer;
                instanceInitializers = copy;
            }
        }

        private sealed class PropertyInitializer
        {
            internal PropertyInfo Property { get; set; }

            internal IInstanceProducer Producer { get; set; }

            internal bool CanBeUsed
            {
                get { return this.Producer != null && this.Property.CanWrite; }
            }

            internal void InjectProperty(object instance)
            {
                this.Property.SetValue(instance, this.Producer.GetInstance(), null);
            }
        }

        private sealed class InstanceInitializer
        {
            private readonly Container container;

            private PropertyInitializer[] initializers;

            public InstanceInitializer(Container container)
            {
                this.container = container;
            }

            [DebuggerStepThrough]
            internal void InitializeInstance(object instance)
            {
                if (this.initializers == null)
                {
                    this.initializers = 
                        this.GetPropertyInitializersFor(instance.GetType());
                }

                for (int i = 0; i < this.initializers.Length; i++)
                {
                    this.initializers[i].InjectProperty(instance);
                }
            }

            [DebuggerStepThrough]
            private PropertyInitializer[] GetPropertyInitializersFor(Type type)
            {
                var initializers = new List<PropertyInitializer>();

                foreach (var property in type.GetProperties())
                {
                    var initializer = this.GetPropertyInitializerFor(property);

                    if (initializer.CanBeUsed)
                    {
                        initializers.Add(initializer);
                    }
                }

                return initializers.ToArray();
            }

            [DebuggerStepThrough]
            private PropertyInitializer GetPropertyInitializerFor(PropertyInfo property)
            {
                return new PropertyInitializer
                {
                    Property = property,
                    Producer = this.container.GetRegistration(property.PropertyType)
                };
            }
        }
    }
}