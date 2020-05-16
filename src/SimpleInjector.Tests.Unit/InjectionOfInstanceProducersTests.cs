namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector;

    // Tests for #393.
    [TestClass]
    public class InjectionOfInstanceProducersTests
    {
        [TestMethod]
        public void AnInjectedInstanceProducer_AndRetrievedRegistration_AreTheSame()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>(Lifestyle.Singleton);

            var expectedProducer = container.GetRegistration(typeof(ICommandHandler<RealCommand>));

            // Act
            var service = container.GetInstance<ServiceDependingOn<InstanceProducer<ICommandHandler<RealCommand>>>>();
            InstanceProducer actualProducer = service.Dependency;

            // Assert
            Assert.AreSame(expectedProducer, actualProducer,
                "If these types weren't the same, it would become pretty hard to give all the guarantees. " +
                "It would, for instance, easily cause a singleton decorator to be wrapped multiple times " +
                "around the same singleton implementation. Same holds for other other ways intercepting.");
        }

        [TestMethod]
        public void AnInjectedInstanceProducerForANonGenericRegistration_AndRetrievedRegistration_AreTheSame()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(ICommandHandler<RealCommand>), typeof(RealCommandHandler), Lifestyle.Singleton);

            var expectedProducer = container.GetRegistration(typeof(ICommandHandler<RealCommand>));

            // Act
            var service = container.GetInstance<ServiceDependingOn<InstanceProducer<ICommandHandler<RealCommand>>>>();
            InstanceProducer actualProducer = service.Dependency;

            // Assert
            // Question: And what about this? Now Register is non-generic. Should the container build a
            // InstanceProducer<T> for this? What if the service type is internal? Would tht fail in partial
            // trust?
            Assert.AreSame(expectedProducer, actualProducer);
        }

        [TestMethod]
        public void ResolvingFromAnInjectedInstanceProducer_ResultsInTheSameSingletonInstanceAsANormalResolve()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>(Lifestyle.Singleton);

            var expectedHandler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Act
            var service = container.GetInstance<ServiceDependingOn<InstanceProducer<ICommandHandler<RealCommand>>>>();
            InstanceProducer producer = service.Dependency;
            var actualHandler = producer.GetInstance();

            // Assert
            Assert.AreSame(expectedHandler, actualHandler);
        }

        [TestMethod]
        public void ResolvingFromAnInjectedInstanceProducer_WrappedInSingletonDecorator_ResultsInTheSameSingletonInstanceAsANormalResolve()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>(Lifestyle.Singleton);
            container.RegisterDecorator(
                typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Singleton);

            var expectedHandler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Act
            var service = container.GetInstance<ServiceDependingOn<InstanceProducer<ICommandHandler<RealCommand>>>>();
            InstanceProducer producer = service.Dependency;
            var actualHandler = producer.GetInstance();

            // Assert
            Assert.AreSame(expectedHandler, actualHandler);
        }

        [TestMethod]
        public void InjectingAnInstanceProducer_ForADecoratedInstance_AllowsRetrievingTheImplementationType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>(Lifestyle.Singleton);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Transient);

            // Act
            var service = container.GetInstance<ServiceDependingOn<InstanceProducer<ICommandHandler<RealCommand>>>>();

            InstanceProducer<ICommandHandler<RealCommand>> producer = service.Dependency;

            // Assert
            AssertThat.AreEqual(
                expectedType: typeof(RealCommandHandler),
                actualType: producer.ImplementationType);
        }

        [TestMethod]
        public void InjectingAnInstanceProducerAfterVerifying_ForADecoratedInstance_AllowsRetrievingTheImplementationType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>(Lifestyle.Singleton);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Transient);

            // Act
            var service = container.GetInstance<ServiceDependingOn<InstanceProducer<ICommandHandler<RealCommand>>>>();
            container.Verify();

            InstanceProducer<ICommandHandler<RealCommand>> producer = service.Dependency;

            // Assert
            AssertThat.AreEqual(
                expectedType: typeof(RealCommandHandler),
                actualType: producer.ImplementationType);
        }

        [TestMethod]
        public void ResolvingInstanceProducer_ForADecoratedInstance_AllowsRetrievingTheImplementationType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>(Lifestyle.Singleton);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Transient);

            // Act
            var producer = container.GetInstance<InstanceProducer<ICommandHandler<RealCommand>>>();

            // Assert
            AssertThat.AreEqual(
                expectedType: typeof(RealCommandHandler),
                actualType: producer.ImplementationType);
        }

        [TestMethod]
        public void ResolvingInstanceProducerAfterVerification_ForADecoratedInstance_AllowsRetrievingTheImplementationType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>(Lifestyle.Singleton);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Transient);

            container.Verify();

            // Act
            var producer = container.GetInstance<InstanceProducer<ICommandHandler<RealCommand>>>();

            // Assert
            AssertThat.AreEqual(
                expectedType: typeof(RealCommandHandler),
                actualType: producer.ImplementationType);
        }

        [TestMethod]
        public void InjectingReadOnlyListInstanceProducers_ForDecoratedInstances_InjectsTheExpectedInstanceProducers()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append(typeof(IEventHandler<>), typeof(NotifyCustomer), Lifestyle.Singleton);
            container.Collection.Append(typeof(IEventHandler<>), typeof(DetermineNewWarehouseInventory));

            container.RegisterDecorator(typeof(IEventHandler<>), typeof(EventHandlerDecorator<>));

            // Act
            var service =
                container.GetInstance<ServiceDependingOn<IReadOnlyList<InstanceProducer<IEventHandler<OrderShipped>>>>>();

            container.Verify();

            var producers = service.Dependency;
            var notifyHandler1 = (EventHandlerDecorator<OrderShipped>)producers[0].GetInstance();
            var notifyHandler2 = (EventHandlerDecorator<OrderShipped>)producers[0].GetInstance();
            var determineHandler1 = (EventHandlerDecorator<OrderShipped>)producers[1].GetInstance();
            var determineHandler2 = (EventHandlerDecorator<OrderShipped>)producers[1].GetInstance();

            // Assert
            Assert.AreEqual(2, producers.Count);
            Assert.AreNotSame(notifyHandler1, notifyHandler2, "Expected transient decorator");
            Assert.AreSame(notifyHandler1.Decoratee, notifyHandler2.Decoratee, "NotifyCustomer should be singleton");
            Assert.AreNotSame(determineHandler1, determineHandler2, "Expected transient decorator");
            Assert.AreNotSame(determineHandler1.Decoratee, determineHandler2.Decoratee, "Expected transient decorator");
        }

        [TestMethod]
        public void InjectingEnumerableInstanceProducers_ForDecoratedInstances_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append(typeof(IEventHandler<>), typeof(NotifyCustomer), Lifestyle.Singleton);
            container.Collection.Append(typeof(IEventHandler<>), typeof(DetermineNewWarehouseInventory));

            container.RegisterDecorator(typeof(IEventHandler<>), typeof(EventHandlerDecorator<>));

            // Act
            var service =
                container.GetInstance<ServiceDependingOn<IEnumerable<InstanceProducer<IEventHandler<OrderShipped>>>>>();

            container.Verify();

            var producers = service.Dependency;
            var notifyHandler = (EventHandlerDecorator<OrderShipped>)producers.First().GetInstance();
            var determineHandler = (EventHandlerDecorator<OrderShipped>)producers.Last().GetInstance();

            // Assert
            Assert.AreEqual(2, producers.Count());
        }

        // The following test shows the most likely use case for the InstanceProducer-injection feature (#393).
        // Where a collection of decorated (generic) instances are registered, but they must be filtered based
        // on their implementation type. InstanceProducer<T> provides access to this implementation type.
        [TestMethod]
        public void ConsumerDependingOnListOfInstanceProducers_CanIterateThatCollectionInItsConstructor_WithoutCausingVerificationErrors()
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
            private readonly Dictionary<Type, InstanceProducer<IEventHandler<T>>> producers;

            public MessageProcessor(ICollection<InstanceProducer<IEventHandler<T>>> handlerProducers)
            {
                this.producers = handlerProducers.ToDictionary(p => p.ImplementationType);
            }

            public void Process(T message, Type handlerType)
            {
                // we would normally call .Handle(message) here.
                this.producers[handlerType].GetInstance();
            }
        }
    }
}