﻿namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Threading;
    using SimpleInjector.Advanced;

    public static class PerResolveLifestyleExtensions
    {
        public static void EnablePerResolveLifestyle(this ContainerOptions options)
        {
            PerResolveLifestyle.EnablePerResolveLifestyle(options.Container);
        }
    }

    public class PerResolveLifestyle : ScopedLifestyle
    {
        private static readonly object Key = new object();

        // This lifestyle should not dispose instances, because they outlive their scope.
        public PerResolveLifestyle()
            : base("Resolve", disposeInstances: false)
        {
        }

        protected override int Length
        {
            get { return 2; }
        }

        internal static void EnablePerResolveLifestyle(Container container)
        {
            if (container.GetItem(Key) != null)
            {
                throw new InvalidOperationException("Already enabled.");
            }

            container.SetItem(Key, new ThreadLocal<Scope>());

            container.Options.RegisterResolveInterceptor(ApplyResolveScope, context => true);
        }

        protected override Func<Scope> CreateCurrentScopeProvider(Container container)
        {
            return () => GetCurrentScopeInternal(container);
        }

        protected override Scope GetCurrentScopeCore(Container container)
        {
            return GetCurrentScopeInternal(container);
        }

        private static Scope GetCurrentScopeInternal(Container container)
        {
            var currentScope = (ThreadLocal<Scope>)container.GetItem(Key);

            if (currentScope == null)
            {
                throw new InvalidOperationException("Call Container.Options.EnablePerResolveLifestyle() first.");
            }

            return currentScope.Value;
        }

        private static object ApplyResolveScope(InitializationContext context, Func<object> getInstance)
        {
            var threadLocal = (ThreadLocal<Scope>)context.Registration.Container.GetItem(Key);

            var original = threadLocal.Value;
            var current = new Scope();

            try
            {
                threadLocal.Value = current;
                return getInstance();
            }
            finally
            {
                try
                {
                    current.Dispose();
                }
                finally
                {
                    threadLocal.Value = original;
                }
            }
        }
    }
}