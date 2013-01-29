namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;

    [TestFixture]
    public class GetCurrentRegistrationsTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetCurrentRegistrations_Always_LocksTheContainer()
        {
            // Arrange
            var container = new Container();

            container.GetCurrentRegistrations();

            // Act
            container.Register<ITimeProvider, RealTimeProvider>();
        }

        [Test]
        public void GetCurrentRegistrations_TransientInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.Register<ITimeProvider, RealTimeProvider>();

            // Act
            var registations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(1, registations.Count());
        }

        [Test]
        public void GetCurrentRegistrations_TransientConcreteInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.Register<RealTimeProvider>();

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(RealTimeProvider));

            // Assert
            Assert.AreEqual(1, registrations.Count());
        }

        [Test]
        public void GetCurrentRegistrations_FuncTransientInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.Register<ITimeProvider>(() => new RealTimeProvider());

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(1, registrations.Count());
        }

        [Test]
        public void GetCurrentRegistrations_ImplicitlyRegisteredTransientInstance_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.GetInstance<RealTimeProvider>();

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(RealTimeProvider));

            // Assert
            Assert.AreEqual(1, registrations.Count());
        }

        [Test]
        public void GetCurrentRegistrations_SingleInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ITimeProvider, RealTimeProvider>();

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(1, registrations.Count());
        }

        [Test]
        public void GetCurrentRegistrations_SingleConcreteInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<RealTimeProvider>();

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(RealTimeProvider));

            // Assert
            Assert.AreEqual(1, registrations.Count());
        }

        [Test]
        public void GetCurrentRegistrations_FuncSingleInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ITimeProvider>(() => new RealTimeProvider());

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(1, registrations.Count());
        }

        [Test]
        public void GetCurrentRegistrations_SingleInstanceRegisteredByObject_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ITimeProvider>(new RealTimeProvider());

            // Act
            var registrations = container.GetCurrentRegistrations()
                .Where(r => r.ServiceType == typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(1, registrations.Count());
        }

        [Test]
        public void GetCurrentRegistrations_InstanceResolvedUsingUnregisteredTypeResolution_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

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

        [Test]
        public void GetCurrentRegistrations_InvalidTypeRequestedFromGetRegistration_InvalidTypeIsNotReturned()
        {
            // Arrange
            Type invalidType = typeof(ServiceWithUnregisteredDependencies);

            var container = new Container();

            // This will force the creation and caching of the InstanceProducer of the invalidType.
            container.GetRegistration(invalidType);

            // Act
            var registrations = container.GetCurrentRegistrations();
           
            // Assert
            var actualRegistration = registrations.SingleOrDefault(r => r.ServiceType == invalidType);

            Assert.IsNull(actualRegistration, "IInstanceProducer returned while it shouldn't be, because " +
                "invalid registrations (for unregistered types) should not be returned.");
        }
    }
}