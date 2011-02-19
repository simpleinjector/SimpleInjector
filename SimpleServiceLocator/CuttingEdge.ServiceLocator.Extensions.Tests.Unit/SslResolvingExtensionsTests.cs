using CuttingEdge.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocator.Extensions.Tests.Unit
{
    [TestClass]
    public class SslResolvingExtensionsTests
    {
        [TestMethod]
        public void TryGetInstance_ServiceTypeRegistered_ReturnsTrue()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            IWeapon weapon;

            bool found = container.TryGetInstance(out weapon);

            // Assert
            Assert.IsTrue(found, "TryGetInstance is expected to return true when the type is registered.");
        }

        [TestMethod]
        public void TryGetInstance_ServiceTypeRegistered_ReturnsInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            IWeapon weapon;

            container.TryGetInstance<IWeapon>(out weapon);

            // Assert
            Assert.IsNotNull(weapon, "TryGetInstance is expected to return an instance when the type is registered.");
        }

        [TestMethod]
        public void TryGetInstance_ServiceTypeNotRegistered_ReturnsFalse()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            IWeapon weapon;

            bool found = container.TryGetInstance(out weapon);

            // Assert
            Assert.IsFalse(found, "TryGetInstance is expected to return false when the type is not registered.");
        }

        [TestMethod]
        public void TryGetInstance_ServiceTypeNotRegistered_ReturnsNull()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            IWeapon weapon;

            container.TryGetInstance(out weapon);

            // Assert
            Assert.IsNull(weapon, "TryGetInstance is expected to return null when the type is not registered.");
        }
    }
}