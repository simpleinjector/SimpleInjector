namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Threading;
    using SimpleInjector.Advanced;

    public class PerGraphLifestyle : ScopedLifestyle
    {
        private static readonly object Key = new object();

        public PerGraphLifestyle() : base("Per Graph") { }

        public override int Length => Lifestyle.Transient.Length + 1;

        public static void EnableFor(Container container)
        {
            if (container.GetItem(Key) == null)
            {
                container.SetItem(Key, new ThreadLocal<Scope>());
                container.Options.RegisterResolveInterceptor(ApplyGraphScope, context => true);
            }
        }

        protected override Func<Scope> CreateCurrentScopeProvider(Container c) => () => GetScope(c);
        protected override Scope GetCurrentScopeCore(Container c) => GetScope(c);
        private static Scope GetScope(Container c) => ((ThreadLocal<Scope>)c.GetItem(Key))?.Value ?? Throw();

        private static object ApplyGraphScope(InitializationContext context, Func<object> instanceProducer)
        {
            var container = context.Registration.Container;
            var threadLocal = (ThreadLocal<Scope>)container.GetItem(Key);

            Scope original = threadLocal.Value;

            try
            {
                threadLocal.Value = new Scope(container);
                return instanceProducer();
            }
            finally
            {
                // We deliberately don't dispose the Scope here, since this lifestyle should not 
                // dispose instances, because they outlive their scope.
                // WARNING: Although per-graph instances are not disposed, the diagnostic sub system
                // will not warn in case a disposable instance is registered as per-graph.
                threadLocal.Value = original;
            }
        }

        private static Scope Throw()
        {
            throw new InvalidOperationException("Call PerGraphLifestyle.EnableFor(Container) first.");
        }
    }
}