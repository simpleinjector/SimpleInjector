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
        public void GetCurrentRegistrations_TransientInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.Register<ITimeProvider, RealTimeProvider>();

            // Act
            var provider = container.GetCurrentRegistrations().Single();

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetCurrentRegistrations_TransientConcreteInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.Register<RealTimeProvider>();

            // Act
            var provider = container.GetCurrentRegistrations().Single();

            // Assert
            Assert.AreEqual(typeof(RealTimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetCurrentRegistrations_FuncTransientInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.Register<ITimeProvider>(() => new RealTimeProvider());

            // Act
            var provider = container.GetCurrentRegistrations().Single();

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetCurrentRegistrations_ImplicitlyRegisteredTransientInstance_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.GetInstance<RealTimeProvider>();

            // Act
            var provider = container.GetCurrentRegistrations().Single();

            // Assert
            Assert.AreEqual(typeof(RealTimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetCurrentRegistrations_SingleInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ITimeProvider, RealTimeProvider>();

            // Act
            var provider = container.GetCurrentRegistrations().Single();

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetCurrentRegistrations_SingleConcreteInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<RealTimeProvider>();

            // Act
            var provider = container.GetCurrentRegistrations().Single();

            // Assert
            Assert.AreEqual(typeof(RealTimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetCurrentRegistrations_FuncSingleInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ITimeProvider>(() => new RealTimeProvider());

            // Act
            var provider = container.GetCurrentRegistrations().Single();

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetCurrentRegistrations_SingleInstanceRegisteredByObject_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ITimeProvider>(new RealTimeProvider());

            // Act
            var provider = container.GetCurrentRegistrations().Single();

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
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
            var provider = container.GetCurrentRegistrations().Single();

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

    }
}
