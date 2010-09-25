using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class RegisterAllTests
    {
        [TestMethod]
        public void GetAllInstances_WithRegisteredList_ReturnsExpectedList()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weaponsToRegister = new IWeapon[] { new Tanto(), new Katana() };
            container.RegisterAll<IWeapon>(weaponsToRegister);

            // Act
            var weapons = container.GetAllInstances<IWeapon>();

            // Assert
            Assert.IsNotNull(weapons, "This method MUST NOT return null.");
            Assert.AreEqual(2, weapons.Count(), "Collection is expected to contain two values.");
        }

        [TestMethod]
        public void GetAllInstances_NoInstancesRegistered_ReturnsEmptyCollection()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            var weapons = container.GetAllInstances<IWeapon>();

            // Assert
            Assert.IsNotNull(weapons, "This method MUST NOT return null.");
            Assert.AreEqual(0, weapons.Count(),
                "If no instances of the requested type are available, this method MUST return an " +
                "enumerator of length 0 instead of throwing an exception.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterAll_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Tanto());
            container.GetInstance<IWeapon>();

            // Act
            container.RegisterAll<IWeapon>(new IWeapon[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetAllInstances.")]
        public void RegisterAll_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();
            var count = weapons.Count();

            // Act
            container.RegisterAll<IWeapon>(new IWeapon[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterAll_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterAll<IWeapon>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterAll_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = new IWeapon[] { new Tanto(), new Katana() };
            container.RegisterAll<IWeapon>(weapons);

            // Act
            container.RegisterAll<IWeapon>(weapons);
        }
    }
}