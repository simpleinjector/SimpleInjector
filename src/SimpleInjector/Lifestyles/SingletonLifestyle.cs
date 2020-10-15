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
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Internals;

    /// <summary>
    /// The singleton lifestyle.
    /// </summary>
    public sealed class SingletonLifestyle : Lifestyle
    {
        // Oh, the irony. Here the Singleton Design Pattern is applied to the Singleton Lifestyle.
        internal static readonly SingletonLifestyle Instance = new SingletonLifestyle();

        private SingletonLifestyle() : base("Singleton")
        {
        }

        /// <inheritdoc />
        public override int Length => 1000;

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of the
        /// specified <paramref name="instance"/>. Properties, decorators, interceptors, and initializers
        /// may be applied to this instance.
        /// </summary>
        /// <param name="instanceType">The type of the instance.</param>
        /// <param name="instance">The instance to create a registration for.</param>
        /// <param name="container">The <see cref="Container"/> instance for which a
        /// <see cref="Registration"/> must be created.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when on of the supplied arguments is a null
        /// reference.</exception>
        public Registration CreateRegistration(Type instanceType, object instance, Container container)
        {
            Requires.IsNotNull(instanceType, nameof(instanceType));
            Requires.IsNotNull(instance, nameof(instance));
            Requires.IsNotNull(container, nameof(container));

            // The instance type check is done inside CreateSingleInstanceRegistration.
            return CreateSingleInstanceRegistration(
                serviceType: instanceType,
                implementationType: instance!.GetType(),
                instance: instance!,
                container: container);
        }

        // NOTE: This overload is needed because the addition of the (Type, object, Container) overload caused
        // C# to always pick that overload over the (Type, Func<object>, Container) overload in the base class.
        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of the
        /// specified <paramref name="serviceType"/>  using the supplied <paramref name="instanceCreator"/>
        /// with the caching as specified by this lifestyle.
        /// </summary>
        /// <param name="serviceType">The interface or base type that can be used to retrieve the instances.</param>
        /// <param name="instanceCreator">The delegate that will be responsible for creating new instances.</param>
        /// <param name="container">The <see cref="Container"/> instance for which a
        /// <see cref="Registration"/> must be created.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when on of the supplied arguments is a null
        /// reference.</exception>
        public new Registration CreateRegistration(
            Type serviceType, Func<object> instanceCreator, Container container)
        {
            Lifestyle lifestyle = this;

            return lifestyle.CreateRegistration(serviceType, instanceCreator, container);
        }

        // This method seems somewhat redundant, since the exact same can be achieved by calling
        // Lifetime.Singleton.CreateRegistration(serviceType, () => instance, container). Calling that method
        // however, will trigger the ExpressionBuilding event later on where the supplied Expression is an
        // InvocationExpression calling that internally created Func<TService>. By having this extra method
        // (and the extra SingletonInstanceLifestyleRegistration class), we can ensure that the
        // ExpressionBuilding event is called with a ConstantExpression, which is much more intuitive to
        // anyone handling that event.
        internal static Registration CreateSingleInstanceRegistration(
            Type serviceType, Type implementationType, object instance, Container container)
        {
            Requires.IsNotNull(instance, nameof(instance));

            // Fixes #589
            // In case of a COM object, we override the implementation type, because otherwise our internal
            // type checks (that call Type.IsAssignableFrom) would fail.
#if !NETSTANDARD1_0 && !NETSTANDARD1_3
            if (implementationType.IsCOMObject)
            {
                implementationType = serviceType;
            }
            else
            {
                Requires.ServiceIsAssignableFromImplementation(serviceType, instance.GetType(), nameof(serviceType));
            }
#endif

            return new SingletonInstanceRegistration(
                serviceType, implementationType, instance, container);
        }

        internal static InstanceProducer CreateUncontrolledCollectionProducer(
            Type itemType, IEnumerable collection, Container container) =>
            new InstanceProducer(
                typeof(IEnumerable<>).MakeGenericType(itemType),
                CreateUncontrolledCollectionRegistration(itemType, collection, container));

        internal static Registration CreateUncontrolledCollectionRegistration(
            Type itemType, IEnumerable collection, Container container)
        {
            Type enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);

            // collection.GetType() could be anything, including internal types, so we don't want to use
            // that.
            Type implementationType = enumerableType;

            var registration = CreateSingleInstanceRegistration(
                serviceType: enumerableType,
                implementationType: implementationType,
                instance: collection,
                container: container);

            registration.IsCollection = true;

            return registration;
        }

        internal static bool IsSingletonInstanceRegistration(Registration registration) =>
            registration is SingletonInstanceRegistration;

        /// <inheritdoc />
        protected internal override Registration CreateRegistrationCore(Type concreteType, Container container)
        {
            Requires.IsNotNull(concreteType, nameof(concreteType));
            Requires.IsNotNull(container, nameof(container));

            return new SingletonRegistration(container, concreteType);
        }

        /// <inheritdoc />
        protected internal override Registration CreateRegistrationCore<TService>(
            Func<TService> instanceCreator, Container container)
        {
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            return new SingletonRegistration(container, typeof(TService), instanceCreator);
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

        private sealed class SingletonInstanceRegistration : Registration
        {
            private readonly object locker = new object();

            private object instance;
            private bool initialized;

            internal SingletonInstanceRegistration(
                Type serviceType, Type implementationType, object instance, Container container)
                : base(Lifestyle.Singleton, container, implementationType)
            {
                this.instance = instance;
                this.ServiceType = serviceType;
            }

            public Type ServiceType { get; }

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

        private sealed class SingletonRegistration : Registration
        {
            private readonly object locker = new object();

            private object? interceptedInstance;

            public SingletonRegistration(
                Container container, Type implementationType, Func<object>? instanceCreator = null)
                : base(Lifestyle.Singleton, container, implementationType, instanceCreator)
            {
            }

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
                            this.interceptedInstance = this.GetInterceptedInstanceWithNullCheck();
                            this.TryRegisterForDisposal(this.interceptedInstance);
                        }
                    }
                }

                return this.interceptedInstance;
            }

            private object TryRegisterForDisposal(object instance)
            {
                if (!this.SuppressDisposal)
                {
                    if (instance is IDisposable disposable)
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

                return instance;
            }

            private object GetInterceptedInstanceWithNullCheck()
            {
                // The BuildTransientExpression might cause the instance to be intercepted and properties
                // to be injected.
                Expression expression = this.BuildTransientExpression();

                return this.Execute(this.Compile(expression))
                    ?? throw new ActivationException(
                        StringResources.DelegateForTypeReturnedNull(this.ImplementationType));
            }

            // Implements #553 Allows detection of Lifestyle Mismatches when iterated inside constructor.
            private object Execute(Func<object> instanceCreator)
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
                                this.ImplementationType,
                                args.Producer,
                                matchingRelationship);

                            // At this point, an injected ContainerControlledCollection<T> has notified the
                            // listener about the creation of one of its elements. This has happened during
                            // the construction of this (Singleton) instance, which might cause Lifestyle
                            // Mismatches. That's why this is added as a known relationship. This way
                            // diagnostics can verify the relationship.
                            var relationship = new KnownRelationship(
                                    implementationType: this.ImplementationType,
                                    lifestyle: this.Lifestyle,
                                    consumer: matchingRelationship?.Consumer ?? InjectionConsumerInfo.Root,
                                    dependency: args.Producer);

                            relationship.AddAdditionalInformation(
                                DiagnosticType.LifestyleMismatch, additionalInformation);

                            this.AddRelationship(relationship);
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

            private Func<object> Compile(Expression expression)
            {
                try
                {
                    // Compile without optimizations, because they are not needed for Singletons.
                    var lambda = expression as LambdaExpression ?? Expression.Lambda(expression);

                    return (Func<object>)lambda.Compile();
                }
                catch (Exception ex)
                {
                    string message = StringResources.ErrorWhileBuildingDelegateFromExpression(
                        this.ImplementationType, expression, ex);

                    throw new ActivationException(message, ex);
                }
            }
        }
    }
}