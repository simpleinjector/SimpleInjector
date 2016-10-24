namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq.Expressions;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Allows registering instances with a <see cref="Lifestyle.Transient">Transient</see> lifestyle, while
    /// allowing them to get disposed on the boundary of a supplied <see cref="ScopedLifestyle"/>.
    /// </summary>
    public class DisposableTransientLifestyle : Lifestyle
    {
        private static readonly object ItemKey = new object();
        private readonly ScopedLifestyle scopedLifestyle;

        public DisposableTransientLifestyle(ScopedLifestyle scopedLifestyle)
            : base("Transient (Disposes on " + scopedLifestyle.Name + " boundary)")
        {
            this.scopedLifestyle = scopedLifestyle;
        }

        // Same length as Transient.
        protected override int Length => 1;

        public static void EnableForContainer(Container container)
        {
            bool alreadyInitialized = container.GetItem(ItemKey) != null;

            if (!alreadyInitialized)
            {
                AddGlobalDisposableInitializer(container);

                container.SetItem(ItemKey, ItemKey);
            }
        }

        protected override Registration CreateRegistrationCore<TService, TImplementation>(Container c) =>
            new DisposableRegistration<TService, TImplementation>(this.scopedLifestyle, this, c);

        protected override Registration CreateRegistrationCore<TService>(Func<TService> ic, Container c) =>
            new DisposableRegistration<TService>(this.scopedLifestyle, this, c, ic);

        private static void TryEnableTransientDisposalOrThrow(Container container)
        {
            bool alreadyInitialized = container.GetItem(ItemKey) != null;

            if (!alreadyInitialized)
            {
                if (container.IsLocked())
                {
                    throw new InvalidOperationException(
                        "Please make sure DisposableTransientLifestyle.EnableForContainer(Container) is " +
                        "called during initialization.");
                }

                EnableForContainer(container);
            }
        }

        private static void AddGlobalDisposableInitializer(Container container) =>
            container.RegisterInitializer(RegisterForDisposal, ShouldApplyInitializer);

        private static bool ShouldApplyInitializer(InitializationContext context) => 
            context.Registration is DisposableRegistration;

        private static void RegisterForDisposal(InstanceInitializationData data)
        {
            IDisposable instance = data.Instance as IDisposable;

            if (instance != null)
            {
                var registation = (DisposableRegistration)data.Context.Registration;
                registation.ScopedLifestyle.RegisterForDisposal(registation.Container, instance);
            }
        }

        private sealed class DisposableRegistration<TService> : DisposableRegistration where TService : class
        {
            private readonly Func<TService> instanceCreator;

            internal DisposableRegistration(ScopedLifestyle s, Lifestyle l, Container c, Func<TService> ic) : base(s, l, c)
            {
                this.instanceCreator = ic;
            }

            public override Type ImplementationType => typeof(TService);
            public override Expression BuildExpression() => this.BuildTransientExpression(this.instanceCreator);
        }

        private class DisposableRegistration<TService, TImpl> : DisposableRegistration
            where TImpl : class, TService
            where TService : class
        {
            internal DisposableRegistration(ScopedLifestyle s, Lifestyle l, Container c) : base(s, l, c) { }

            public override Type ImplementationType => typeof(TImpl);
            public override Expression BuildExpression() => this.BuildTransientExpression<TService, TImpl>();
        }

        private abstract class DisposableRegistration : Registration
        {
            internal readonly ScopedLifestyle ScopedLifestyle;

            protected DisposableRegistration(ScopedLifestyle s, Lifestyle l, Container c) : base(l, c)
            {
                this.ScopedLifestyle = s;

                TryEnableTransientDisposalOrThrow(c);
            }
        }
    }
}