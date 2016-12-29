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

        private interface IDisposableRegistration
        {
            ScopedLifestyle ScopedLifestyle { get; }
        }

        public override int Length => Transient.Length;

        public static void EnableForContainer(Container container)
        {
            bool alreadyInitialized = container.GetItem(ItemKey) != null;

            if (!alreadyInitialized)
            {
                AddGlobalDisposableInitializer(container);

                container.SetItem(ItemKey, ItemKey);
            }
        }

        protected override Registration CreateRegistrationCore<TConcrete>(Container c) =>
            new DisposableRegistration<TConcrete>(this.scopedLifestyle, this, c, null);

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

        private static bool ShouldApplyInitializer(InitializerContext context) => 
            context.Registration is IDisposableRegistration;

        private static void RegisterForDisposal(InstanceInitializationData data)
        {
            var instance = data.Instance as IDisposable;

            if (instance != null)
            {
                var registation = (IDisposableRegistration)data.Context.Registration;
                registation.ScopedLifestyle.RegisterForDisposal(data.Context.Registration.Container, instance);
            }
        }

        private sealed class DisposableRegistration<TImpl> : Registration, IDisposableRegistration 
            where TImpl : class
        {
            private readonly Func<TImpl> instanceCreator;

            internal DisposableRegistration(ScopedLifestyle s, Lifestyle l, Container c, Func<TImpl> ic) : base(l, c)
            {
                this.instanceCreator = ic;
                this.ScopedLifestyle = s;

                DisposableTransientLifestyle.TryEnableTransientDisposalOrThrow(c);
            }

            public override Type ImplementationType => typeof(TImpl);

            public ScopedLifestyle ScopedLifestyle { get; }

            public override Expression BuildExpression() =>
                this.instanceCreator == null
                    ? this.BuildTransientExpression()
                    : this.BuildTransientExpression(this.instanceCreator);
        }
    }
}