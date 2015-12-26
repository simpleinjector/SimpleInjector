namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq.Expressions;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Allows registering instances with a <see cref="Lifestyle.Transient">Transient</see> lifestyle, while
    /// allowing them to get disposed on the boundary of a supplied <see cref="ScopedLifestyle"/>
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

        protected override int Length
        {
            // Same length as Transient.
            get { return 1; }
        }

        [DebuggerStepThrough]
        public static void EnableForContainer(Container container)
        {
            bool alreadyInitialized = container.GetItem(ItemKey) != null;

            if (!alreadyInitialized)
            {
                if (container.IsLocked())
                {
                    // By calling into the container a nice expressive exception about locking is thrown.
                    container.GetInstance<Container>();
                }

                AddGlobalDisposableInitializer(container);

                container.SetItem(ItemKey, ItemKey);
            }
        }

        [DebuggerStepThrough]
        protected override Registration CreateRegistrationCore<TService, TImplementation>(Container container)
        {
            TryEnableTransientDisposalOrThrow(container);

            var registration = 
                new DisposableTransientLifestyleRegistration<TService, TImplementation>(this, container);

            registration.ScopedLifestyle = this.scopedLifestyle;

            return registration;
        }

        [DebuggerStepThrough]
        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator, 
            Container container)
        {
            TryEnableTransientDisposalOrThrow(container);

            var registration =
                new DisposableTransientLifestyleRegistration<TService>(this, container, instanceCreator);

            registration.ScopedLifestyle = this.scopedLifestyle;

            return registration;
        }

        [DebuggerStepThrough]
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

        private static void AddGlobalDisposableInitializer(Container container)
        {
            Predicate<InitializationContext> shouldRunInstanceInitializer =
                context => context.Registration is DisposableTransientRegistration;

            Action<InstanceInitializationData> instanceInitializer = data =>
            {
                IDisposable instance = data.Instance as IDisposable;

                if (instance != null)
                {
                    var registation = (DisposableTransientRegistration)data.Context.Registration;

                    var scope = registation.ScopedLifestyle.GetCurrentScope(container);

                    if (scope == null)
                    {
                        ThrowOutsideScopeException(data.Context.Producer.ServiceType, registation);
                    }

                    scope.RegisterForDisposal(instance);
                }
            };

            container.RegisterInitializer(instanceInitializer, shouldRunInstanceInitializer);
        }

        private static void ThrowOutsideScopeException(Type serviceType,
            DisposableTransientRegistration registation)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                "The {0} is registered as '{1}' lifestyle, but the instance is requested " +
                "outside the context of a {2}.",
                serviceType.FullName,
                registation.Lifestyle.Name,
                registation.ScopedLifestyle.Name));
        }
        
        private abstract class DisposableTransientRegistration : Registration
        {
            protected DisposableTransientRegistration(Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
            }

            internal ScopedLifestyle ScopedLifestyle { get; set; }
        }

        private sealed class DisposableTransientLifestyleRegistration<TService> 
            : DisposableTransientRegistration
            where TService : class
        {
            private readonly Func<TService> instanceCreator;

            internal DisposableTransientLifestyleRegistration(Lifestyle lifestyle, Container container, 
                Func<TService> instanceCreator)
                : base(lifestyle, container)
            {
                this.instanceCreator = instanceCreator;
            }

            public override Type ImplementationType
            {
                get { return typeof(TService); }
            }

            public override Expression BuildExpression()
            {
                return this.BuildTransientExpression<TService>(this.instanceCreator);
            }
        }

        private class DisposableTransientLifestyleRegistration<TService, TImplementation> 
            : DisposableTransientRegistration
            where TImplementation : class, TService
            where TService : class
        {
            internal DisposableTransientLifestyleRegistration(Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
            }

            public override Type ImplementationType
            {
                get { return typeof(TImplementation); }
            }

            public override Expression BuildExpression()
            {
                return this.BuildTransientExpression<TService, TImplementation>();
            }
        }
    }
}