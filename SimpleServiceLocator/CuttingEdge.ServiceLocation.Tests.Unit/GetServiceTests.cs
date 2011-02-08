using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class GetServiceTests
    {
        [TestMethod]
        public void GetService_RequestingARegisteredType_ReturnsExpectedInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            var expectedInstance = new Tanto();

            container.RegisterSingle<IWeapon>(expectedInstance);

            // Act
            var actualInstance = ((IServiceProvider)container).GetService(typeof(IWeapon));

            // Assert
            Assert.AreEqual(expectedInstance, actualInstance, "The IServiceProvider.GetService method did " +
                "not return the expected instance.");
        }

        [TestMethod]
        public void GetService_RequestingANonregisteredType_ReturnsNull()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            var actualInstance = ((IServiceProvider)container).GetService(typeof(IWeapon));

            // Assert
            Assert.IsNull(actualInstance, "The contract of the IServiceProvider states that it returns " +
                "null when no registration is found.");
        }
    }
}