using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.Tests.Unit
{
    [TestClass]
    public class GetRegistrationTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetRegistration_Always_LocksTheContainer()
        {
            // Arrange
            var container = new Container();

            container.GetRegistration(typeof(ITimeProvider));

            // Act
            container.Register<ITimeProvider, RealTimeProvider>();

            // Assert
            Assert.Fail("The container should get locked during the call to GetRegistration, because a " +
                "user can call the GetInstance() and BuildExpression() methods on the returned instance. " +
                "BuildExpression can internally call GetInstance and the first call to GetInstance should " +
                "always lock the container for reasons of correctness.");
        }

        [TestMethod]
        public void GetRegistration_TransientInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.Register<ITimeProvider, RealTimeProvider>();

            // Act
            var provider = container.GetRegistration(typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_TransientConcreteInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.Register<RealTimeProvider>();

            // Act
            var provider = container.GetRegistration(typeof(RealTimeProvider));

            // Assert
            Assert.AreEqual(typeof(RealTimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_FuncTransientInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.Register<ITimeProvider>(() => new RealTimeProvider());

            // Act
            var provider = container.GetRegistration(typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_ImplicitlyRegisteredTransientInstance_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.GetInstance<RealTimeProvider>();

            // Act
            var provider = container.GetRegistration(typeof(RealTimeProvider));

            // Assert
            Assert.AreEqual(typeof(RealTimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_SingleInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ITimeProvider, RealTimeProvider>();

            // Act
            var provider = container.GetRegistration(typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_SingleConcreteInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<RealTimeProvider>();

            // Act
            var provider = container.GetRegistration(typeof(RealTimeProvider));

            // Assert
            Assert.AreEqual(typeof(RealTimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_FuncSingleInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ITimeProvider>(() => new RealTimeProvider());

            // Act
            var provider = container.GetRegistration(typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_SingleInstanceRegisteredByObject_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ITimeProvider>(new RealTimeProvider());

            // Act
            var provider = container.GetRegistration(typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_InstanceResolvedUsingUnregisteredTypeResolution_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = new Container();

            container.ResolveUnregisteredType += (sender, e) =>
            {
                e.Register(() => new RealTimeProvider());
            };

            container.GetInstance<ITimeProvider>();

            // Act
            var provider = container.GetRegistration(typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistratoin_OnOnregisteredConcreteConstructableType_ReturnsInstance()
        {
            // Arrange
            var container = new Container();

            // Act
            var registration = container.GetRegistration(typeof(IDisposable));

            // Assert
            Assert.IsNull(registration);
        }
        
        [TestMethod]
        public void GetRegistratoin_OnOnregisteredString_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            // Act
            var registration = container.GetRegistration(typeof(string));

            // Assert
            Assert.IsNull(registration);
        }

        [TestMethod]
        public void GetRegistratoin_OnValueType_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            // Act
            var registration = container.GetRegistration(typeof(int));

            // Assert
            Assert.IsNull(registration);
        }
    }
}