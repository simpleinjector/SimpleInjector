using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class RegisterSingleInstanceByKeyTests
    {
        [TestMethod]
        public void GetInstanceByKey_InstanceSetWithRegisterSingleByKey_AlwaysReturnsSameInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var key = "Sword";
            container.RegisterSingleByKey<IWeapon>(key, new Katana());

            // Act
            var instance1 = container.GetInstance<IWeapon>(key);
            var instance2 = container.GetInstance<IWeapon>(key);

            // Assert
            Assert.IsNotNull(instance1, "GetInstance should never return null.");
            Assert.AreEqual(instance1, instance2, "Values should reference the same instance.");
        }

        [TestMethod]
        public void RegisterSingleInstanceByKey_WithValidKey_Succeeds()
        {
            // Arrange
            string key = "valid key";
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterSingleByKey<IWeapon>(key, new Katana());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleInstanceByKey_WithNullKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            string invalidKey = null;

            // Act
            container.RegisterSingleByKey<IWeapon>(invalidKey, new Katana());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterSingleInstanceByKey_WithEmptyKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            string invalidKey = string.Empty;

            // Act
            container.RegisterSingleByKey<IWeapon>(invalidKey, new Katana());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleInstanceByKey_WithNullInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            IWeapon instance = null;

            // Act
            container.RegisterSingleByKey<IWeapon>("valid key", instance);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once by key.")]
        public void RegisterSingleInstanceByKey_CalledTwiceOnSameTypeAndKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>("Weapon", new Katana());

            // Act
            container.RegisterSingleByKey<IWeapon>("Weapon", new Tanto());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once by key.")]
        public void RegisterSingleInstanceByKey_CalledAfterRegisterByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterByKey<Warrior>(key => new Ninja(null));

            // Act
            container.RegisterSingleByKey<Warrior>("Samurai", new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterSingleInstanceByKey_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Tanto());
            container.GetInstance<IWeapon>();

            // Act
            container.RegisterSingleByKey<Warrior>("Samurai", new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetAllInstances.")]
        public void RegisterSingleInstanceByKey_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();
            var count = weapons.Count();

            // Act
            container.RegisterSingleByKey<Warrior>("Samurai", new Samurai(null));
        }

        [TestMethod]
        public void RegisterSingleInstanceByKey_TypeRegisteredUsingRegisterSingleInstance_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            // This is valid behavior, because this allows the user to register a default (key-less) instance
            // and multiple keyed instances.
            container.RegisterSingleByKey<IWeapon>("Tanto", new Tanto());
        }

        [TestMethod]
        public void GetInstanceByKey_WithNullKey_ReturnsDefaultInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            var expectedInstance = new Katana();
            container.RegisterSingle<IWeapon>(expectedInstance);
            container.RegisterSingleByKey<IWeapon>("Ninja", new Tanto());

            // Act
            var returnedInstance = container.GetInstance<IWeapon>(null);

            // Assert
            Assert.AreEqual(expectedInstance, returnedInstance);
        }
    }
}