using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.Tests.Unit
{
    [TestClass]
    public class GetCurrentRegistrationsTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetCurrentRegistrations_Always_LocksTheContainer()
        {
            // Arrange
            var container = new Container();

            container.GetCurrentRegistrations();

            // Act
            container.Register<ITimeProvider, RealTimeProvider>();
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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
    }
}