namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetCurrentRegistrationsTests
    {
        [TestMethod]
        public void GetCurrentRegistrations_Never_LocksTheContainer()
        {
            // Arrange
            var container = ContainerFactory.New();

            // In previous versions of the framework a call to Verify() didn't lock the container. This meant
            // that GetCurrentRegistrations could return registrations that where automatically registered by
            // the container (created during the verification process). Since GetCurrentRegistrations can only
            // return valid registrations, those auto registered registrations have to be checked by building
            // their expressions. For this we had to lock the container.
            // In v2, Verify() locks the container. This means that when GetCurrentRegistrations when the
            // container is not locked, there are no auto registered instances and we won't have to build
            // any expressions and we therefore don't need to lock.
            // Still, the container has to be locked when BuildExpression is called on a returned 
            // InstanceProducer.
            container.GetCurrentRegistrations();

            // Act
            container.Register<ITimeProvider, RealTimeProvider>();
        }

        [TestMethod]
        public void Verify_Always_LocksTheContainer()
        {
            // Arrange
            var container = ContainerFactory.New();

            // This test is a duplicate from the test in the VerifyTests class, but it's essential for the
            // correctness of GetCurrentRegistrations. That's why it's placed directly after the test that
            // checks if GetCurrentRegistrations doesn't lock the container. When Verify() doesn't lock the
            // container, this test will fail, and you're probably reading this because this test failed :-)
            // Look at the previous test in this class.
            container.Verify();

            // Act
            Action action = () => container.Register<ITimeProvider, RealTimeProvider>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "container can't be changed", action);
        }

        [TestMethod]
        public void BuildExpression_Always_LocksTheContainer()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ILogger, NullLogger>();

            container.GetCurrentRegistrations().First().BuildExpression();

            // Act
            Action action = () => container.Register<ITimeProvider, RealTimeProvider>();

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void GetCurrentRegistrations_TransientInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ITimeProvider, RealTimeProvider>();

            // Act
            var registations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(1, registations.Count());
        }

        [TestMethod]
        public void GetCurrentRegistrations_TransientConcreteInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<RealTimeProvider>();

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(RealTimeProvider));

            // Assert
            Assert.AreEqual(1, registrations.Count());
        }

        [TestMethod]
        public void GetCurrentRegistrations_FuncTransientInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ITimeProvider>(() => new RealTimeProvider());

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(1, registrations.Count());
        }

        [TestMethod]
        public void GetCurrentRegistrations_ImplicitlyRegisteredTransientInstance_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.GetInstance<RealTimeProvider>();

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(RealTimeProvider));

            // Assert
            Assert.AreEqual(1, registrations.Count());
        }

        [TestMethod]
        public void GetCurrentRegistrations_SingleInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(1, registrations.Count());
        }

        [TestMethod]
        public void GetCurrentRegistrations_SingleConcreteInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<RealTimeProvider>(Lifestyle.Singleton);

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(RealTimeProvider));

            // Assert
            Assert.AreEqual(1, registrations.Count());
        }

        [TestMethod]
        public void GetCurrentRegistrations_FuncSingleInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ITimeProvider>(() => new RealTimeProvider(), Lifestyle.Singleton);

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(ITimeProvider))
                .ToArray();

            // Assert
            Assert.AreEqual(1, registrations.Length);
        }

        [TestMethod]
        public void GetCurrentRegistrations_SingleInstanceRegisteredByObject_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<ITimeProvider>(new RealTimeProvider());

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(1, registrations.Count());
        }

        [TestMethod]
        public void GetCurrentRegistrations_InstanceResolvedUsingUnregisteredTypeResolution_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (sender, e) =>
            {
                e.Register(() => new RealTimeProvider());
            };

            container.GetInstance<ITimeProvider>();

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(1, registrations.Count());
        }

        [TestMethod]
        public void GetCurrentRegistrations_InvalidTypeRequestedFromGetRegistration_InvalidTypeIsNotReturned()
        {
            // Arrange
            Type invalidType = typeof(ServiceWithUnregisteredDependencies);

            var container = ContainerFactory.New();

            // This will force the creation and caching of the InstanceProducer of the invalidType.
            container.GetRegistration(invalidType);

            // Act
            var registrations = container.GetCurrentRegistrations();
           
            // Assert
            var actualRegistration = registrations.SingleOrDefault(r => r.ServiceType == invalidType);

            Assert.IsNull(actualRegistration, "IInstanceProducer returned while it shouldn't be, because " +
                "invalid registrations (for unregistered types) should not be returned.");
        }

        [TestMethod]
        public void CreateProducer_ThatIsReferenced_EndsUpInTheCurrentRegistrationsList()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var producer = Lifestyle.Transient.CreateProducer<ITimeProvider>(typeof(RealTimeProvider), container);

            // Assert
            Assert.IsTrue(container.GetCurrentRegistrations().Contains(producer),
                "A created producer should end up in the list of CurrentRegistrations, because both " +
                "the Verify() and the Diagnostic Services depend on this behavior.");

            GC.KeepAlive(producer);
        }

        [TestMethod]
        public void CreateProducer_ThatBecomesUnreferenced_DoesNotEndsUpInTheCurrentRegistrationsList()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var producer = Lifestyle.Transient.CreateProducer<ITimeProvider, RealTimeProvider>(container);

            // Remove the reference
            producer = null;

            GC.Collect();

            // Assert
            Assert.IsFalse(container.GetCurrentRegistrations().Contains(producer),
                "A created producer should NOT end up in the list of CurrentRegistrations when the " +
                "application doesn't reference it and the GC ran, because that will cause a memory leak " +
                "and might cause an OutOfMemoryException.");
        }
    }
}