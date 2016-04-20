using System;
using System.Linq.Expressions;

namespace SimpleInjector.CodeSamples
{
    /// <summary>
    /// Injects a new instance for each dependency. Behaves the same as Transient but prevents any lifestyle
    /// mismatches from being reported. Use with care!!!
    /// </summary>
    public class InstancePerDependencyLifestyle : Lifestyle
    {
        public InstancePerDependencyLifestyle() : base("Instance Per Dependency")
        {
        }

        // Prevents lifestyle mismatches
        protected override int Length => int.MaxValue;

        protected override Registration CreateRegistrationCore<TService, TImplementation>(Container container) => 
            new InstancePerDependencyLifestyleRegistration<TService, TImplementation>(this, container);

        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container) => 
            new InstancePerDependencyLifestyleRegistration<TService>(this, container, instanceCreator);

        private sealed class InstancePerDependencyLifestyleRegistration<TService> : Registration
            where TService : class
        {
            private readonly Func<TService> instanceCreator;

            public InstancePerDependencyLifestyleRegistration(Lifestyle lifestyle, Container container,
                Func<TService> instanceCreator)
                : base(lifestyle, container)
            {
                this.instanceCreator = instanceCreator;
            }

            public override Type ImplementationType => typeof(TService);

            public override Expression BuildExpression() => this.BuildTransientExpression(this.instanceCreator);
        }

        private class InstancePerDependencyLifestyleRegistration<TService, TImplementation> : Registration
            where TImplementation : class, TService
            where TService : class
        {
            internal InstancePerDependencyLifestyleRegistration(Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
            }

            public override Type ImplementationType => typeof(TImplementation);

            public override Expression BuildExpression() => this.BuildTransientExpression<TService, TImplementation>();
        }
    }
}
