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

        protected override Registration CreateRegistrationCore(Type t, Container c) => new Reg(this, c, t);
        protected override Registration CreateRegistrationCore<T>(Func<T> ic, Container c) => new Reg<T>(this, c, ic);

        private class Reg<T> : Registration where T : class
        {
            private readonly Func<T> creator;
            public Reg(Lifestyle l, Container c, Func<T> creator) : base(l, c) => this.creator = creator;

            public override Type ImplementationType => typeof(T);
            public override Expression BuildExpression() => this.BuildTransientExpression(this.creator);
        }

        private class Reg : Registration
        {
            public Reg(Lifestyle l, Container c, Type t) : base(l, c) => this.ImplementationType = t;
            public override Type ImplementationType { get; }
            public override Expression BuildExpression() => this.BuildTransientExpression();
        }
    }
}