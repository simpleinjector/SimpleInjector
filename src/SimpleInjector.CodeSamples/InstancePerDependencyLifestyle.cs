namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Injects a new instance for each dependency. Behaves the same as Transient but 
    /// prevents any lifestyle mismatches from being reported. Use with care!!!
    /// </summary>
    public class InstancePerDependencyLifestyle : Lifestyle
    {
        public InstancePerDependencyLifestyle() : base("Instance Per Dependency") { }

        // MaxValue prevents lifestyle mismatches when injected into a component, while 
        // allowing instances to depend on singletons.
        protected override int Length => 1000;

        protected override Registration CreateRegistrationCore<TService, TImplementation>(
            Container container) => 
            new InstancePerDependencyRegistration<TService, TImplementation>(this, container);

        protected override Registration CreateRegistrationCore<TService>(
            Func<TService> instanceCreator, Container container) => 
            new InstancePerDependencyRegistration<TService>(this, container, instanceCreator);

        private class InstancePerDependencyRegistration<T> : Registration where T : class
        {
            private readonly Func<T> creator;

            public InstancePerDependencyRegistration(Lifestyle lifestyle, Container container, 
                Func<T> creator) : base(lifestyle, container)
            {
                this.creator = creator;
            }

            public override Type ImplementationType => typeof(T);
            public override Expression BuildExpression() => BuildTransientExpression(creator);
        }

        private class InstancePerDependencyRegistration<T, TImpl> : Registration
            where T : class where TImpl : class, T
        {
            public InstancePerDependencyRegistration(Lifestyle lifestyle, Container container)
                : base(lifestyle, container) { }
            public override Type ImplementationType => typeof(TImpl);
            public override Expression BuildExpression() => 
                BuildTransientExpression<T, TImpl>();
        }
    }
}