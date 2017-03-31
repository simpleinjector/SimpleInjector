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

        protected override Registration CreateRegistrationCore<TConcrete>(Container container) => 
            new InstancePerDependencyRegistration<TConcrete>(this, container);

        protected override Registration CreateRegistrationCore<TService>(
            Func<TService> instanceCreator, Container container) => 
            new InstancePerDependencyRegistration<TService>(this, container, instanceCreator);

        private class InstancePerDependencyRegistration<TImpl> : Registration where TImpl : class
        {
            private readonly Func<TImpl> creator;

            public InstancePerDependencyRegistration(Lifestyle lifestyle, Container container, 
                Func<TImpl> creator = null) : base(lifestyle, container)
            {
                this.creator = creator;
            }

            public override Type ImplementationType => typeof(TImpl);
            public override Expression BuildExpression() =>
                this.creator == null
                    ? this.BuildTransientExpression()
                    : this.BuildTransientExpression(this.creator);
        }
    }
}