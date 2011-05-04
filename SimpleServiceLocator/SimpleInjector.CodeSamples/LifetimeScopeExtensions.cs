namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;

    using SimpleInjector;

    // Differences with Autofac:
    // 1. In Autofac you call scope.Resolve while here you should just call container.Resolve. In other words, 
    //    this implementation works much like the TransactionScope.
    // 2. In Autofac the container acts like a scope as well, resolving 'per lifetime scoped' objects is the
    //    same as resolving singletons in the container. This implementation throws an exception when
    //    resolving 'per lifetime scoped' outside the context of a lifetime scope.
    // 3. By default, Autofac will track all types that implement IDisposable. We can mimic this behavior
    //    by calling container.MarkAsDisposable<IDisposable>();
    // 4. In Autofac there are many advanced scenario's possible.
    public static class LifetimeScopeExtensions
    {
        private const string NoLifetimeScopeMessage = "The type {0} is " +
            "registered as 'lifetime scope' and can't be resolved " +
            "outside the context of a BeginLifetimeScope call.";

        [ThreadStatic]
        private static Stack<LifetimeScope> scopes;

        private static LifetimeScope CurrentScope
        {
            [DebuggerStepThrough]
            get
            {
                var scopes = LifetimeScopeExtensions.scopes;
                return scopes != null && scopes.Count > 0 ? scopes.Peek() : null;
            }
        }

        public static IDisposable BeginLifetimeScope(this Container container)
        {
            return new LifetimeScope().Begin();
        }

        public static void RegisterLifetimeScope<TService, TImplementation>(
            this Container container)
            where TImplementation : class, TService
            where TService : class
        {
            // We must use the ResolveUnregisteredType to prevent the registered
            // delegate from firing during Verify().
            container.ResolveUnregisteredType += (sender, e) =>
            {
                if (e.UnregisteredServiceType == typeof(TService))
                {
                    e.Register(() =>
                    {
                        var scope = CurrentScope;

                        if (scope == null)
                        {
                            throw new ActivationException(string.Format(
                                CultureInfo.InvariantCulture,
                                NoLifetimeScopeMessage, typeof(TService)));
                        }

                        return scope.GetInstance<TImplementation>(container);
                    });
                }
            };
        }

        public static void DisposeWhenLifetimeScopeEnds<TDisposable>(
            this Container container)
            where TDisposable : class, IDisposable
        {
            container.RegisterInitializer<TDisposable>(disposable =>
            {
                var scope = CurrentScope;

                if (scope != null)
                {
                    scope.RegisterForDisposal(disposable);
                }
                else
                {
                    // Instances that are created outside the scope of a lifetime
                    // scope will most likely be singletons or transients created 
                    // during the call to Verify(). We have no way of knowing
                    // whether we can dispose it. Therefore, we simply continue.
                }
            });
        }

        private sealed class LifetimeScope : IDisposable
        {
            private readonly Dictionary<Type, object> instances = 
                new Dictionary<Type, object>();

            private bool disposed;

            private List<IDisposable> disposables;

            void IDisposable.Dispose()
            {
                if (!this.disposed)
                {
                    this.disposed = true;

                    scopes.Pop();

                    if (this.disposables != null)
                    {
                        this.disposables.ForEach(d => d.Dispose());
                    }
                }
            }

            internal LifetimeScope Begin()
            {
                if (scopes == null)
                {
                    scopes = new Stack<LifetimeScope>();
                }

                scopes.Push(this);

                return this;
            }

            internal TService GetInstance<TService>(Container container)
                where TService : class
            {
                object instance;

                if (!this.instances.TryGetValue(typeof(TService), out instance))
                {
                    this.instances[typeof(TService)] = 
                        instance = container.GetInstance<TService>();
                }

                return (TService)instance;
            }

            internal void RegisterForDisposal(IDisposable disposable)
            {
                var disposables = 
                    this.disposables ?? (this.disposables = new List<IDisposable>());

                disposables.Add(disposable);
            }
        }
    }
}