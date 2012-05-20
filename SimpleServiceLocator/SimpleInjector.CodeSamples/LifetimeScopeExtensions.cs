namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=LifetimeScopeExtensions
    // These extension methods are provided for compatibility with .NET 3.5. For .NET 4.0 please use the
    // SimpleInjector.Extensions.LifetimeScoping dll or NuGet package).
    // Behavior:
    // 1. Calling container.BeginLifetimeScope will start a new scope and the scope ends when the returned
    //    object gets disposed. All 'lifetime scoped' objects that are requested from the container in between
    //    are singletons in that scope.
    // 2. A lifetime scope is thread-specific. Each new thread should call BeginLifetimeScope.
    // 3. Scopes can be nested. Each new scope gets its own new set of instances. Do not use lifetime scoping
    //    in a web request, since web requests can end at a different thread than where they were started.
    // 4. This implementation will not work correctly when multiple containers are involved. Instances are
    //    singleton within a scope on a thread. Multiple containers share their scopes.
    // 2. The container acts like a scope as well, resolving 'per lifetime scoped' objects outside any scope
    //    is the same as resolving singletons in the container.
    // 3. Objects that implement IDisposable are tracked and disposed when the scope ends.
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public static class SimpleInjectorLifetimeScopeExtensions
    {
        private const string LifetimeScopingIsNotEnabledMessage =
            "To enable lifetime scoping, please make sure the " +
            "EnableLifetimeScoping extension method is " +
            "called during the configuration of the container.";

        public static void EnableLifetimeScoping(
            this Container container)
        {
            try
            {
                container.RegisterSingle<LifetimeScopeManager>(
                    new LifetimeScopeManager());
            }
            catch (InvalidOperationException)
            {
            }
        }

        public static IDisposable BeginLifetimeScope(
            this Container container)
        {
            IServiceProvider provider = container;

            var manager =
                provider.GetService(typeof(LifetimeScopeManager))
                as LifetimeScopeManager;

            if (manager != null)
            {
                return manager.BeginLifetimeScope();
            }

            throw new InvalidOperationException(
                LifetimeScopingIsNotEnabledMessage);
        }

        public static void RegisterLifetimeScope<TConcrete>(
            this Container container)
            where TConcrete : class
        {
            container.Register<TConcrete>();
            container.EnableLifetimeScoping();
            AsLifetimeScope<TConcrete>(container);
        }

        public static void RegisterLifetimeScope<TService, TImpl>(
            this Container container)
            where TImpl : class, TService
            where TService : class
        {
            container.Register<TService, TImpl>();
            container.EnableLifetimeScoping();
            AsLifetimeScope<TService>(container);
        }

        public static void RegisterLifetimeScope<TService>(
            this Container container,
            Func<TService> instanceCreator)
            where TService : class
        {
            RegisterLifetimeScope<TService>(container,
                instanceCreator, true);
        }

        public static void RegisterLifetimeScope<TService>(
            this Container container,
            Func<TService> instanceCreator,
            bool disposeWhenLifetimeScopeEnds)
            where TService : class
        {
            container.Register<TService>(instanceCreator);
            container.EnableLifetimeScoping();
            AsLifetimeScope<TService>(container,
                disposeWhenLifetimeScopeEnds);
        }

        private static void AsLifetimeScope<TService>(
            Container c, bool dispose = true)
            where TService : class
        {
            var helper = new AsLifetimeScopeHelper<TService>(c)
            {
                DisposeWhenLifetimeScopeEnds = dispose
            };

            c.ExpressionBuilt += helper.ExpressionBuilt;
        }

        private sealed class LifetimeScopeManager
        {
            [ThreadStatic]
            private static Stack<LifetimeScope> threadStaticScopes;

            internal LifetimeScopeManager()
            {
            }

            internal LifetimeScope CurrentScope
            {
                get
                {
                    var scopes = threadStaticScopes;

                    return scopes != null && scopes.Count > 0
                        ? scopes.Peek() : null;
                }
            }

            internal LifetimeScope BeginLifetimeScope()
            {
                Stack<LifetimeScope> scopes = threadStaticScopes;

                if (scopes == null)
                {
                    threadStaticScopes =
                        scopes = new Stack<LifetimeScope>();
                }

                var scope = new LifetimeScope(this);

                scopes.Push(scope);

                return scope;
            }

            internal void EndLifetimeScope(LifetimeScope scope)
            {
                var scopes = threadStaticScopes;

                if (scopes != null && scopes.Contains(scope))
                {
                    while (scopes.Count > 0 && scopes.Pop() != scope)
                    {
                    }
                }
            }
        }

        private sealed class LifetimeScope : IDisposable
        {
            private readonly Dictionary<Type, object> instances =
                new Dictionary<Type, object>();

            private LifetimeScopeManager manager;
            private List<IDisposable> disposables;

            internal LifetimeScope(LifetimeScopeManager manager)
            {
                this.manager = manager;
            }

            public void Dispose()
            {
                if (this.manager != null)
                {
                    this.manager.EndLifetimeScope(this);

                    this.manager = null;

                    if (this.disposables != null)
                    {
                        this.disposables.ForEach(d => d.Dispose());
                    }

                    this.disposables = null;
                }
            }

            internal void RegisterForDisposal(IDisposable disposable)
            {
                if (this.disposables == null)
                {
                    this.disposables = new List<IDisposable>();
                }

                this.disposables.Add(disposable);
            }

            internal TService GetInstance<TService>(
                Func<TService> instanceCreator)
                where TService : class
            {
                object instance;

                if (!this.instances.TryGetValue(typeof(TService),
                    out instance))
                {
                    this.instances[typeof(TService)] =
                        instance = instanceCreator();
                }

                return (TService)instance;
            }
        }

        private sealed class AsLifetimeScopeHelper<TService>
            where TService : class
        {
            private readonly Container container;
            private TService singleton;
            private LifetimeScopeManager manager;
            private Func<TService> instanceCreator;

            internal AsLifetimeScopeHelper(Container container)
            {
                this.container = container;
            }

            internal bool DisposeWhenLifetimeScopeEnds { get; set; }

            internal void ExpressionBuilt(object sender,
                ExpressionBuiltEventArgs e)
            {
                if (e.RegisteredServiceType == typeof(TService))
                {
                    this.manager = this.container
                        .GetInstance<LifetimeScopeManager>();

                    this.instanceCreator =
                        Expression.Lambda<Func<TService>>(e.Expression)
                        .Compile();

                    Func<TService> scopedInstanceCreator =
                        this.CreateScopedInstance;

                    e.Expression = Expression.Invoke(
                        Expression.Constant(scopedInstanceCreator));
                }
            }

            private TService CreateScopedInstance()
            {
                var scope = this.manager.CurrentScope;

                if (scope != null)
                {
                    var instance =
                        scope.GetInstance(this.instanceCreator);

                    if (this.DisposeWhenLifetimeScopeEnds)
                    {
                        var disposable = instance as IDisposable;

                        if (disposable != null)
                        {
                            scope.RegisterForDisposal(disposable);
                        }
                    }

                    return instance;
                }

                return this.GetSingleton();
            }

            private TService GetSingleton()
            {
                if (this.singleton == null)
                {
                    lock (this)
                    {
                        if (this.singleton == null)
                        {
                            this.singleton = this.instanceCreator();
                        }
                    }
                }

                return this.singleton;
            }
        }
    }
}