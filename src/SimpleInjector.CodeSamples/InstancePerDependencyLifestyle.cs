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
        protected override Registration CreateRegistrationCore<T>(Func<T> ic, Container c) =>
            new Reg(this, c, typeof(T), ic);

        private class Reg : Registration
        {
            private readonly Func<object> creator;

            public Reg(Lifestyle l, Container c, Type t, Func<object> ic = null) : base(l, c)
            {
                this.ImplementationType = t;
                this.creator = ic;
            }

            public override Type ImplementationType { get; }
            public override Expression BuildExpression() => this.BuildTransientExpression(this.creator);
        }
    }
}