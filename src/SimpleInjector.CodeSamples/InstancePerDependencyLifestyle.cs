namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Injects a new instance for each dependency. Behaves the same as Transient but 
    /// prevents any lifestyle mismatches from being reported. Instances will not be disposed! 
    /// Use with care -or rather- don't use AT ALL!!!
    /// </summary>
    public class InstancePerDependencyLifestyle : Lifestyle
    {
        public InstancePerDependencyLifestyle() : base("Instance Per Dependency") { }

        // Returning Singleton.Length prevents lifestyle mismatches when injected into a 
        // component, while allowing instances to depend on singletons.
        public override int Length => Singleton.Length;

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
            public override Expression BuildExpression() => 
                this.BuildTransientExpression(this.creator);
        }

        private class InstancePerDependencyRegistration<T, TImpl> : Registration
            where T : class where TImpl : class, T
        {
            public InstancePerDependencyRegistration(Lifestyle lifestyle, Container container)
                : base(lifestyle, container) { }
            public override Type ImplementationType => typeof(TImpl);
            public override Expression BuildExpression() =>
                this.BuildTransientExpression<T, TImpl>();
        }
    }
}