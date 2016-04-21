namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Injects a new instance for each dependency. Behaves the same as Transient but prevents any lifestyle
    /// mismatches from being reported. Use with care!!!
    /// </summary>
    public class InstancePerDependencyLifestyle : Lifestyle
    {
        public InstancePerDependencyLifestyle() : base("Instance Per Dependency") { }

        // MaxValue prevents lifestyle mismatches
        protected override int Length => int.MaxValue;

        protected override Registration CreateRegistrationCore<TService, TImplementation>(Container c) => 
            new InstancePerDependencyRegistration<TService, TImplementation>(this, c);

        protected override Registration CreateRegistrationCore<TService>(Func<TService> ic, Container c) => 
            new InstancePerDependencyRegistration<TService>(this, c, ic);

        private sealed class InstancePerDependencyRegistration<TService> : Registration where TService : class
        {
            private readonly Func<TService> ic;

            public InstancePerDependencyRegistration(Lifestyle l, Container c, Func<TService> ic) : base(l, c)
            {
                this.ic = ic;
            }

            public override Type ImplementationType => typeof(TService);
            public override Expression BuildExpression() => this.BuildTransientExpression(this.ic);
        }

        private class InstancePerDependencyRegistration<TService, TImpl> : Registration
            where TImpl : class, TService where TService : class
        {
            public InstancePerDependencyRegistration(Lifestyle l, Container c) : base(l, c) { }
            public override Type ImplementationType => typeof(TImpl);
            public override Expression BuildExpression() => this.BuildTransientExpression<TService, TImpl>();
        }
    }
}