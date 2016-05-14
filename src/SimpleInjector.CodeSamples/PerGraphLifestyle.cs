﻿namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Threading;
    using SimpleInjector.Advanced;

    public class PerGraphLifestyle : ScopedLifestyle
    {
        private static readonly object Key = new object();

        // This lifestyle should not dispose instances, because they outlive their scope.
        public PerGraphLifestyle() : base("Per Graph", disposeInstances: false) { }

        protected override int Length => 2;

        public static void EnableFor(Container container)
        {
            if (container.GetItem(Key) == null)
            {
                container.SetItem(Key, new ThreadLocal<Scope>());
                container.Options.RegisterResolveInterceptor(ApplyGraphScope, context => true);
            }
        }

        protected override Func<Scope> CreateCurrentScopeProvider(Container c) => () => GetScopeInternal(c);

        protected override Scope GetCurrentScopeCore(Container container) => GetScopeInternal(container);

        private static Scope GetScopeInternal(Container container)
        {
            var currentScope = (ThreadLocal<Scope>)container.GetItem(Key);

            if (currentScope == null)
            {
                throw new InvalidOperationException("Call Container.Options.EnablePerResolveLifestyle() first.");
            }

            return currentScope.Value;
        }

        private static object ApplyGraphScope(InitializationContext context, Func<object> getInstance)
        {
            var threadLocal = (ThreadLocal<Scope>)context.Registration.Container.GetItem(Key);

            var original = threadLocal.Value;

            try
            {
                threadLocal.Value = new Scope(context.Registration.Container);
                return getInstance();
            }
            finally
            {
                threadLocal.Value = original;
            }
        }
    }
}