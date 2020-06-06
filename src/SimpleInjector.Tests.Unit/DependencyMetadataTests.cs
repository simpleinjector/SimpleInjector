namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector;

    // Tests for #393.
    [TestClass]
    public class DependencyMetadataTests
    {
        [TestMethod]
        public void InstanceProducerOrInjectedMetadata_AndRetrievedRegistration_AreTheSame()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>(Lifestyle.Singleton);

            var expectedProducer = container.GetRegistration(typeof(ICommandHandler<RealCommand>));

            // Act
            var service = container.GetInstance<MetadataWrapper<ICommandHandler<RealCommand>>>();

            // Assert
            Assert.AreSame(expectedProducer, service.Metadata.Dependency,
                "If these types weren't the same, it would become pretty hard to give all the guarantees. " +
                "It would, for instance, easily cause a singleton decorator to be wrapped multiple times " +
                "around the same singleton implementation. Same holds for other other ways intercepting.");
        }

        [TestMethod]
        public void AnInjectedInstanceProducerForANonGenericRegistration_AndRetrievedRegistration_AreTheSame()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(
                typeof(ICommandHandler<RealCommand>), typeof(RealCommandHandler), Lifestyle.Singleton);

            var expectedProducer = container.GetRegistration(typeof(ICommandHandler<RealCommand>));

            // Act
            var service = container.GetInstance<MetadataWrapper<ICommandHandler<RealCommand>>>();

            // Assert
            Assert.AreSame(expectedProducer, service.Metadata.Dependency);
        }

        [TestMethod]
        public void ResolvingFromMetadata_ForARegisteredService_ResultsInTheSameSingletonInstanceAsANormalResolve()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>(Lifestyle.Singleton);

            ICommandHandler<RealCommand> expectedHandler =
                container.GetInstance<ICommandHandler<RealCommand>>();

            // Act
            var service = container.GetInstance<MetadataWrapper<ICommandHandler<RealCommand>>>();

            ICommandHandler<RealCommand> actualHandler = service.Metadata.GetInstance();

            // Assert
            Assert.AreSame(expectedHandler, actualHandler);
        }

        [TestMethod]
        public void ResolvingFromMetadata_WrappedInSingletonDecorator_ResultsInTheSameSingletonInstanceAsANormalResolve()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>(Lifestyle.Singleton);
            container.RegisterDecorator(
                typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Singleton);

            ICommandHandler<RealCommand> expectedHandler =
                container.GetInstance<ICommandHandler<RealCommand>>();

            // Act
            var service = container.GetInstance<MetadataWrapper<ICommandHandler<RealCommand>>>();

            ICommandHandler<RealCommand> actualHandler = service.Metadata.GetInstance();

            // Assert
            Assert.AreSame(expectedHandler, actualHandler);
        }

        [TestMethod]
        public void InjectingMetadata_ForADecoratedInstance_AllowsRetrievingTheImplementationType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>(Lifestyle.Singleton);
            container.RegisterDecorator(
                typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Transient);

            // Act
            var service = container.GetInstance<MetadataWrapper<ICommandHandler<RealCommand>>>();

            // Assert
            AssertThat.AreEqual(
                expectedType: typeof(RealCommandHandler),
                actualType: service.Metadata.ImplementationType);
        }

        [TestMethod]
        public void InjectingMetadataAfterVerifying_ForADecoratedInstance_AllowsRetrievingTheImplementationType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>(Lifestyle.Singleton);
            container.RegisterDecorator(
                typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Transient);

            // Act
            var service = container.GetInstance<MetadataWrapper<ICommandHandler<RealCommand>>>();
            container.Verify();

            // Assert
            AssertThat.AreEqual(
                expectedType: typeof(RealCommandHandler),
                actualType: service.Metadata.ImplementationType);
        }

        [TestMethod]
        public void ResolvingMetadata_ForRegisteredService_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ILogger, ConsoleLogger>();

            // Act
            // Being able to resolve the metadata is not a hard requirement, but we get this behavior out of
            // the box. Not allowing this is more work. But since this is allowed, we must ensure it stays
            // this way.
            var metadata = container.GetInstance<DependencyMetadata<ILogger>>();

            // Assert
            Assert.IsNotNull(metadata);
        }

        [TestMethod]
        public void ResolvingMetadata_ForADecoratedInstance_AllowsRetrievingTheImplementationType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>(Lifestyle.Singleton);
            container.RegisterDecorator(
                typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Transient);

            // Act
            var metadata = container.GetInstance<DependencyMetadata<ICommandHandler<RealCommand>>>();

            // Assert
            AssertThat.AreEqual(
                expectedType: typeof(RealCommandHandler),
                actualType: metadata.ImplementationType);
        }

        [TestMethod]
        public void ResolvingMetadataAfterVerification_ForADecoratedInstance_AllowsRetrievingTheImplementationType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>(Lifestyle.Singleton);
            container.RegisterDecorator(
                typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Transient);

            container.Verify();

            // Act
            var metadata = container.GetInstance<DependencyMetadata<ICommandHandler<RealCommand>>>();

            // Assert
            AssertThat.AreEqual(
                expectedType: typeof(RealCommandHandler),
                actualType: metadata.ImplementationType);
        }

        [TestMethod]
        public void InjectingReadOnlyListOfMetadata_ForDecoratedInstances_InjectsTheExpectedInstanceProducers()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append(typeof(IEventHandler<>), typeof(NotifyCustomer), Lifestyle.Singleton);
            container.Collection.Append(typeof(IEventHandler<>), typeof(DetermineNewWarehouseInventory));

            container.RegisterDecorator(typeof(IEventHandler<>), typeof(EventHandlerDecorator<>));

            // Act
            var service = container.GetInstance<IReadOnlyListMetadata<IEventHandler<OrderShipped>>>();

            container.Verify();

            var metadatas = service.Metadatas;
            var notifyHandler1 = (EventHandlerDecorator<OrderShipped>)metadatas[0].GetInstance();
            var notifyHandler2 = (EventHandlerDecorator<OrderShipped>)metadatas[0].GetInstance();
            var determineHandler1 = (EventHandlerDecorator<OrderShipped>)metadatas[1].GetInstance();
            var determineHandler2 = (EventHandlerDecorator<OrderShipped>)metadatas[1].GetInstance();

            // Assert
            Assert.AreEqual(2, metadatas.Count);
            Assert.AreNotSame(notifyHandler1, notifyHandler2, "Expected transient decorator");
            Assert.AreSame(notifyHandler1.Decoratee, notifyHandler2.Decoratee, "NotifyCustomer should be singleton");
            Assert.AreNotSame(determineHandler1, determineHandler2, "Expected transient decorator");
            Assert.AreNotSame(determineHandler1.Decoratee, determineHandler2.Decoratee, "Expected transient decorator");
        }

        [TestMethod]
        public void InjectingEnumerableOfMetadata_ForDecoratedInstances_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append(typeof(IEventHandler<>), typeof(NotifyCustomer), Lifestyle.Singleton);
            container.Collection.Append(typeof(IEventHandler<>), typeof(DetermineNewWarehouseInventory));

            container.RegisterDecorator(typeof(IEventHandler<>), typeof(EventHandlerDecorator<>));

            // Act
            var service = container.GetInstance<EnumerableMetadata<IEventHandler<OrderShipped>>>();

            container.Verify();

            var metadatas = service.Metadatas;
            var notifyDecorator = (EventHandlerDecorator<OrderShipped>)metadatas.First().GetInstance();
            var determineDecorator = (EventHandlerDecorator<OrderShipped>)metadatas.Last().GetInstance();

            // Assert
            Assert.AreEqual(2, metadatas.Count());
            AssertThat.IsInstanceOfType<NotifyCustomer>(notifyDecorator.Decoratee);
            AssertThat.IsInstanceOfType<DetermineNewWarehouseInventory>(determineDecorator.Decoratee);
        }

        // The following test shows the most likely use case for the metadata-injection feature (#393).
        // Where a collection of decorated (generic) instances are registered, but they must be filtered based
        // on their implementation type. DependencyMetadata<T> provides access to this implementation type.
        [TestMethod]
        public void ConsumerDependingOnListOfMetadata_CanIterateThatCollectionInItsConstructor_WithoutCausingVerificationErrors()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append(typeof(IEventHandler<>), typeof(NotifyCustomer));
            container.Collection.Append(typeof(IEventHandler<>), typeof(DetermineNewWarehouseInventory));
            container.RegisterDecorator(typeof(IEventHandler<>), typeof(EventHandlerDecorator<>));

            container.RegisterSingleton<MessageProcessor<OrderShipped>>();

            container.Verify();

            var processor = container.GetInstance<MessageProcessor<OrderShipped>>();

            // Act
            processor.Process(new OrderShipped(), typeof(NotifyCustomer));
            processor.Process(new OrderShipped(), typeof(DetermineNewWarehouseInventory));
        }

        // Events
        internal class OrderShipped
        {
        }

        // Handler implementations
        internal class NotifyCustomer : IEventHandler<OrderShipped>
        {
        }

        internal class DetermineNewWarehouseInventory : IEventHandler<OrderShipped>
        {
        }

        internal class EventHandlerDecorator<TEvent> : IEventHandler<TEvent>
        {
            public EventHandlerDecorator(IEventHandler<TEvent> decoratee)
            {
                this.Decoratee = decoratee;
            }

            public IEventHandler<TEvent> Decoratee { get; }
        }

        internal class MessageProcessor<T>
        {
            private readonly Dictionary<Type, DependencyMetadata<IEventHandler<T>>> metadata;

            public MessageProcessor(ICollection<DependencyMetadata<IEventHandler<T>>> metadata)
            {
                this.metadata = metadata.ToDictionary(p => p.ImplementationType);
            }

            public void Process(T message, Type handlerType)
            {
                // we would normally call .Handle(message) here.
                this.metadata[handlerType].GetInstance();
            }
        }

        internal class MetadataWrapper<TDependency>
            where TDependency : class
        {
            public MetadataWrapper(DependencyMetadata<TDependency> metadata) => this.Metadata = metadata;

            public DependencyMetadata<TDependency> Metadata { get; }
        }

        internal class IReadOnlyListMetadata<TDependency>
            where TDependency : class
        {
            public IReadOnlyListMetadata(IReadOnlyList<DependencyMetadata<TDependency>> metadatas) =>
                this.Metadatas = metadatas;

            public IReadOnlyList<DependencyMetadata<TDependency>> Metadatas { get; }
        }

        internal class EnumerableMetadata<TDependency>
            where TDependency : class
        {
            public EnumerableMetadata(IEnumerable<DependencyMetadata<TDependency>> metadatas) =>
                this.Metadatas = metadatas;

            public IEnumerable<DependencyMetadata<TDependency>> Metadatas { get; }
        }
    }
}