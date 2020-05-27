// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using SimpleInjector.Advanced;
    using SimpleInjector.Internals;

    internal sealed class SingletonLifestyle : Lifestyle
    {
        internal SingletonLifestyle() : base("Singleton")
        {
        }

        public override int Length => 1000;

        // This method seems somewhat redundant, since the exact same can be achieved by calling
        // Lifetime.Singleton.CreateRegistration(serviceType, () => instance, container). Calling that method
        // however, will trigger the ExpressionBuilding event later on where the supplied Expression is an
        // InvocationExpression calling that internally created Func<TService>. By having this extra method
        // (and the extra SingletonInstanceLifestyleRegistration class), we can ensure that the
        // ExpressionBuilding event is called with a ConstantExpression, which is much more intuitive to
        // anyone handling that event.
        internal static Registration CreateSingleInstanceRegistration(
            Type serviceType, object instance, Container container, Type? implementationType = null)
        {
            Requires.IsNotNull(instance, nameof(instance));

            // Fixes #589
            // In case of a COM object, we override the implementation type, because otherwise our internal
            // type checks (that call Type.IsAssignableFrom) would fail.
#if !NETSTANDARD1_0 && !NETSTANDARD1_3
            if (implementationType != null && implementationType.IsCOMObject)
            {
                implementationType = serviceType;
            }
#endif

            return new SingletonInstanceLifestyleRegistration(
                serviceType, implementationType ?? serviceType, instance, container);
        }

        internal static InstanceProducer CreateUncontrolledCollectionProducer(
            Type itemType, IEnumerable collection, Container container) =>
            InstanceProducer.Create(
                typeof(IEnumerable<>).MakeGenericType(itemType),
                CreateUncontrolledCollectionRegistration(itemType, collection, container));

        internal static Registration CreateUncontrolledCollectionRegistration(
            Type itemType, IEnumerable collection, Container container)
        {
            Type enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);

            var registration = CreateSingleInstanceRegistration(enumerableType, collection, container);

            registration.IsCollection = true;

            return registration;
        }

        internal static bool IsSingletonInstanceRegistration(Registration registration) =>
            registration is SingletonInstanceLifestyleRegistration;

        protected internal override Registration CreateRegistrationCore<TConcrete>(Container container) =>
            new SingletonLifestyleRegistration<TConcrete>(container);

        protected internal override Registration CreateRegistrationCore<TService>(
            Func<TService> instanceCreator, Container container)
        {
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            return new SingletonLifestyleRegistration<TService>(container, instanceCreator);
        }

        private static ConstantExpression BuildConstantExpression(object instance, Type implementationType)
        {
            // Fixes #589
            // Internally, Expression.Constant just does a simple Type.IsAssignableFrom check, which returns
            // false for COM objects. Unfortunately, the IsCOMObject property is only available in .NET
            // Standard 2.0 and up.
#if !NETSTANDARD1_0 && !NETSTANDARD1_3
            if (instance.GetType().IsCOMObject)
            {
                return Expression.Constant(instance);
            }
#endif

            return Expression.Constant(instance, implementationType);
        }

        private sealed class SingletonInstanceLifestyleRegistration : Registration
        {
            private readonly object locker = new object();

            private object instance;
            private bool initialized;

            internal SingletonInstanceLifestyleRegistration(
                Type serviceType, Type implementationType, object instance, Container container)
                : base(Lifestyle.Singleton, container)
            {
                this.instance = instance;
                this.ServiceType = serviceType;
                this.ImplementationType = implementationType;
            }

            public Type ServiceType { get; }
            public override Type ImplementationType { get; }

            public override Expression BuildExpression() =>
                SingletonLifestyle.BuildConstantExpression(
                    this.GetInitializedInstance(), this.ImplementationType);

            private object GetInitializedInstance()
            {
                if (!this.initialized)
                {
                    lock (this.locker)
                    {
                        if (!this.initialized)
                        {
                            this.instance = this.GetInjectedInterceptedAndInitializedInstance();

                            this.initialized = true;
                        }
                    }
                }

                return this.instance;
            }

            private object GetInjectedInterceptedAndInitializedInstance()
            {
                try
                {
                    return this.GetInjectedInterceptedAndInitializedInstanceInternal();
                }
                catch (MemberAccessException ex)
                {
                    throw new ActivationException(
                        StringResources.UnableToResolveTypeDueToSecurityConfiguration(
                            this.ImplementationType, ex));
                }
            }

            private object GetInjectedInterceptedAndInitializedInstanceInternal()
            {
                Expression expression =
                    SingletonLifestyle.BuildConstantExpression(this.instance, this.ImplementationType);

                // NOTE: We pass on producer.ServiceType as the implementation type for the following three
                // methods. This will the initialization to be only done based on information of the service
                // type; not on that of the implementation. Although now the initialization could be
                // incomplete, this behavior is consistent with the initialization of
                // Register<TService>(Func<TService>, Lifestyle), which doesn't have the proper static type
                // information available to use the implementation type.
                // TODO: This behavior should be reconsidered, because now it is incompatible with
                // Register<TService, TImplementation>(Lifestyle). So the question is, do we consider
                // RegisterInstance<TService>(TService) to be similar to Register<TService>(Func<TService>)
                // or to Register<TService, TImplementation>()? See: #353.
                expression = this.WrapWithPropertyInjector(this.ServiceType, expression);
                expression = this.InterceptInstanceCreation(this.ServiceType, expression);
                expression = this.WrapWithInitializer(this.ServiceType, expression);

                // PERF: We don't need to compile a delegate in case all we have is a constant.
                if (expression is ConstantExpression constantExpression)
                {
                    return constantExpression.Value;
                }

                Delegate initializer = Expression.Lambda(expression).Compile();

                // This delegate might return a different instance than the originalInstance (caused by a
                // possible interceptor).
                return initializer.DynamicInvoke();
            }
        }

        private sealed class SingletonLifestyleRegistration<TImplementation> : Registration
            where TImplementation : class
        {
            private readonly object locker = new object();
            private readonly Func<TImplementation>? instanceCreator;

            private object? interceptedInstance;

            public SingletonLifestyleRegistration(
                Container container, Func<TImplementation>? instanceCreator = null)
                : base(Lifestyle.Singleton, container)
            {
                this.instanceCreator = instanceCreator;
            }

            public override Type ImplementationType => typeof(TImplementation);

            public override Expression BuildExpression() =>
                SingletonLifestyle.BuildConstantExpression(
                    this.GetInterceptedInstance(), this.ImplementationType);

            private object GetInterceptedInstance()
            {
                // Even though the InstanceProducer takes a lock before calling Registration.BuildExpression
                // we need to take a lock here, because multiple InstanceProducer instances could reference
                // the same Registration and call this code in parallel.
                if (this.interceptedInstance is null)
                {
                    lock (this.locker)
                    {
                        if (this.interceptedInstance is null)
                        {
                            this.interceptedInstance = this.CreateInstanceWithNullCheck();

                            if (!this.SuppressDisposal)
                            {
                                if (this.interceptedInstance is IDisposable disposable)
                                {
                                    this.Container.ContainerScope.RegisterForDisposal(disposable);
                                }
                                else
                                {
                                    // In case an instance implements both IDisposable and IAsyncDisposable,
                                    // it should only be registered once and RegisterForDisposal(IDisposable)
                                    // can be used. That will still allows DisposeAsync to be called.
#if NET461 || NETSTANDARD2_0 || NETSTANDARD2_1
                                    if (this.interceptedInstance is IAsyncDisposable asyncDisposable)
                                    {
                                        this.Container.ContainerScope.RegisterForDisposal(asyncDisposable);
                                    }
#endif
                                }
                            }
                        }
                    }
                }

                return this.interceptedInstance;
            }

            private TImplementation CreateInstanceWithNullCheck()
            {
                Expression expression =
                    this.instanceCreator is null
                        ? this.BuildTransientExpression()
                        : this.BuildTransientExpression(this.instanceCreator);

                Func<TImplementation> func = CompileExpression(expression);

                TImplementation instance = this.CreateInstance(func);

                EnsureInstanceIsNotNull(instance);

                return instance;
            }

            // Implements #553 Allows detection of Lifestyle Mismatches when iterated inside constructor.
            private TImplementation CreateInstance(Func<TImplementation> instanceCreator)
            {
                var isCurrentThread = new ThreadLocal<bool> { Value = true };

                // Create a listener that can spot when an injected stream is iterated during construction.
                var listener = this.CreateCollectionUsedDuringConstructionListener(isCurrentThread);

                try
                {
                    ControlledCollectionHelper.AddServiceCreatedListener(listener);

                    return instanceCreator();
                }
                finally
                {
                    ControlledCollectionHelper.RemoveServiceCreatedListener(listener);
                    isCurrentThread.Dispose();
                }
            }

            private Action<ServiceCreatedListenerArgs> CreateCollectionUsedDuringConstructionListener(
                ThreadLocal<bool> isCurrentThread)
            {
                return args =>
                {
                    // Only handle when an inner registration hasn't handled this yet.
                    if (!args.Handled)
                    {
                        // Only handle when the call originates from the same thread, as calls from different
                        // threads mean the listener is not triggered from this specific instanceCreator.
                        if (isCurrentThread.Value)
                        {
                            args.Handled = true;
                            var matchingRelationship = this.FindMatchingCollectionRelationship(args.Producer);

                            var additionalInformation = StringResources.CollectionUsedDuringConstruction(
                                typeof(TImplementation),
                                args.Producer,
                                matchingRelationship);

                            // At this point, an injected ContainerControlledCollection<T> has notified the
                            // listener about the creation of one of its elements. This has happened during
                            // the construction of this (Singleton) instance, which might cause Lifestyle
                            // Mismatches. That's why this is added as a known relationship. This way
                            // diagnostics can verify the relationship.
                            this.AddRelationship(
                                new KnownRelationship(
                                    implementationType: typeof(TImplementation),
                                    lifestyle: this.Lifestyle,
                                    consumer: matchingRelationship?.Consumer ?? InjectionConsumerInfo.Root,
                                    dependency: args.Producer,
                                    additionalInformation: additionalInformation));
                        }
                    }
                };
            }

            private KnownRelationship FindMatchingCollectionRelationship(
                InstanceProducer collectionItemProducer) =>
                this.FindMatchingControlledCollectionRelationship(collectionItemProducer)
                ?? this.FindMatchingMutableCollectionRelationship(collectionItemProducer);

            private KnownRelationship FindMatchingControlledCollectionRelationship(
                InstanceProducer collectionItemProducer)
            {
                return (
                    from relationship in this.GetRelationships()
                    let producer = relationship.Dependency
                    where producer.IsContainerControlledCollection()
                    let controlledElementType = producer.GetContainerControlledCollectionElementType()
                    where controlledElementType == collectionItemProducer.ServiceType
                    select relationship)
                    .FirstOrDefault();
            }

            private KnownRelationship FindMatchingMutableCollectionRelationship(
                InstanceProducer collectionItemProducer)
            {
                return (
                    from relationship in this.GetRelationships()
                    let producer = relationship.Dependency
                    let dependencyType = producer.ServiceType
                    where typeof(List<>).IsGenericTypeDefinitionOf(dependencyType)
                        || typeof(Collection<>).IsGenericTypeDefinitionOf(dependencyType)
                        || dependencyType.IsArray
                    let elementType = dependencyType.GetGenericArguments().FirstOrDefault()
                        ?? dependencyType.GetElementType()
                    where elementType == collectionItemProducer.ServiceType
                    select relationship)
                    .FirstOrDefault();
            }

            private static Func<TImplementation> CompileExpression(Expression expression)
            {
                try
                {
                    // Don't call BuildTransientDelegate, because that might do optimizations that are simply
                    // not needed, since the delegate will be called just once.
                    return CompilationHelpers.CompileLambda<TImplementation>(expression);
                }
                catch (Exception ex)
                {
                    string message = StringResources.ErrorWhileBuildingDelegateFromExpression(
                        typeof(TImplementation), expression, ex);

                    throw new ActivationException(message, ex);
                }
            }

            private static void EnsureInstanceIsNotNull(object instance)
            {
                if (instance is null)
                {
                    throw new ActivationException(
                        StringResources.DelegateForTypeReturnedNull(typeof(TImplementation)));
                }
            }
        }
    }
}