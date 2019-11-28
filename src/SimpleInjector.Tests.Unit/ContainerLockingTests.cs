namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ContainerLockingTests
    {
        [TestMethod]
        public void GetInstance_ThatLocksTheContainer_RaisesTheContainerLockingEvent()
        {
            // Arrange
            bool eventCalled = false;

            var container = ContainerFactory.New();

            container.Register<ILogger, NullLogger>();

            container.Options.ContainerLocking += (s, e) =>
            {
                eventCalled = true;
            };

            // Act
            container.GetInstance<ILogger>();

            // Assert
            Assert.IsTrue(eventCalled);
        }

        [TestMethod]
        public void GetAllInstances_ThatLocksTheContainer_RaisesTheContainerLockingEvent()
        {
            // Arrange
            bool eventCalled = false;

            var container = ContainerFactory.New();

            container.Collection.Append<ILogger, NullLogger>();

            container.Options.ContainerLocking += (s, e) =>
            {
                eventCalled = true;
            };

            // Act
            container.GetAllInstances<ILogger>();

            // Assert
            Assert.IsTrue(eventCalled);
        }

        [TestMethod]
        public void ProducerGetInstance_ThatLocksTheContainer_RaisesTheContainerLockingEvent()
        {
            // Arrange
            bool eventCalled = false;

            var container = ContainerFactory.New();

            var producer = Lifestyle.Singleton.CreateProducer<ILogger, NullLogger>(container);

            container.Options.ContainerLocking += (s, e) =>
            {
                eventCalled = true;
            };

            // Act
            producer.GetInstance();

            // Assert
            Assert.IsTrue(eventCalled);
        }

        [TestMethod]
        public void ALockingCall_OnAnAlreadyLockedContainer_DoesNotRaisesTheContainerLockingEventAgain()
        {
            // Arrange
            bool eventCalled = false;

            var container = ContainerFactory.New();

            container.Register<ILogger, NullLogger>();

            container.Options.ContainerLocking += (s, e) =>
            {
                eventCalled = true;
            };

            // GetInstance is a locking call, which raises the event.
            container.GetInstance<ILogger>();

            // Reset
            eventCalled = false;

            // Act
            container.GetInstance<ILogger>();

            // Assert
            Assert.IsFalse(eventCalled);
        }

        [TestMethod]
        public void ContainerLockingEvent_WhenRaised_CanMakeLockingCallsOfItsOwn()
        {
            // Arrange
            int eventCount = 0;

            var container = ContainerFactory.New();

            container.Register<ILogger, NullLogger>();

            container.Options.ContainerLocking += (s, e) =>
            {
                eventCount++;

                Assert.AreEqual(expected: 1, actual: eventCount,
                    "A call to a locking method (e.g. GetInstance) should not cause the event to be re-raised.");

                container.GetInstance<ILogger>();
            };

            // Act
            container.GetInstance<ILogger>();
        }
        
        [TestMethod]
        public void ContainerLockingEvent_WhenRaised_CanMakeRegistrations()
        {
            // Arrange
            int eventCount = 0;

            var container = ContainerFactory.New();

            container.Register<ILogger, NullLogger>();

            container.Options.ContainerLocking += (s, e) =>
            {
                eventCount++;

                Assert.AreEqual(expected: 1, actual: eventCount,
                    "A call to a locking method (e.g. GetInstance) should not cause the event to be re-raised.");

                container.Register<ITimeProvider, RealTimeProvider>();
            };

            // Triggers the event and registers ITimeProvider
            container.GetInstance<ILogger>();

            // Act
            // ITimeProvider can now be resolved.
            container.GetInstance<ITimeProvider>();
        }

        [TestMethod]
        public void ContainerLockingEvent_WhenRaised_CanMakeResolveUnregisteredTypeRegistrations()
        {
            // Arrange
            int eventCount = 0;

            var container = ContainerFactory.New();

            container.Register<ILogger, NullLogger>();

            container.Options.ContainerLocking += (s, e) =>
            {
                eventCount++;

                Assert.AreEqual(expected: 1, actual: eventCount,
                    "A call to a locking method (e.g. GetInstance) should not cause the event to be re-raised.");

                container.ResolveUnregisteredType += (_, e1) =>
                {
                    if (e1.UnregisteredServiceType == typeof(ITimeProvider))
                    {
                        e1.Register(() => new RealTimeProvider());
                    }
                };
            };

            // Triggers the event and registers ITimeProvider
            container.GetInstance<ILogger>();

            // Act
            // ITimeProvider can now be resolved.
            container.GetInstance<ITimeProvider>();
        }

        [TestMethod]
        public void GetInstance_CalledAfterContainerLockingIsRaisedWhileThrowingAnException_StillLocksTheContainer()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ILogger, NullLogger>();

            container.Options.ContainerLocking += (s, e) =>
            {
                throw new Exception();
            };

            // Act
            // GetInstance should lock the container, even though ContainerLocking throws an exception
            AssertThat.Throws<Exception>(() => container.GetInstance<ILogger>());

            // Assert
            Assert.IsTrue(container.IsLocked,
                "Container is expected to get locked; even when ContainerLocking throws an exception.");
        }

        [TestMethod]
        public void GetInstance_CalledAfterContainerLockingIsRaisedWhileThrowingAnException_StillDisallowsMakingNewRegistrations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ILogger, NullLogger>();

            container.Options.ContainerLocking += (s, e) =>
            {
                throw new Exception();
            };

            // GetInstance should lock the container, even though ContainerLocking throws an exception
            AssertThat.Throws<Exception>(() => container.GetInstance<ILogger>(), "Setup");

            // Act
            Action action = () => container.Register<ITimeProvider, RealTimeProvider>();

            // Assert
            AssertThat.Throws<InvalidOperationException>(
                action,
                "Container is expected to get locked; even when ContainerLocking throws an exception.");
        }
    }
}