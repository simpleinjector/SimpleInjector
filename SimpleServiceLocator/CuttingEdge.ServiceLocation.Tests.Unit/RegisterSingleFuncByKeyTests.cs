using System;
using System.Linq;

using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class RegisterSingleFuncByKeyTests
    {
        [TestMethod]
        public void RegisterSingleFuncByKey_RequestingAnInstance_ContainerReturnsInstanceSuccesfully()
        {
            // Arrange
            string key = "valid key";
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>(key, () => new Katana());

            // Act
            var weapon = container.GetInstance<IWeapon>(key);

            // Assert
            Assert.IsNotNull(weapon);
        }

        [TestMethod]
        public void RegisterSingleFuncByKey_RequestingMultipleInstances_ContainerAlwaysReturnsSameInstance()
        {
            // Arrange
            string key = "valid key";
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>(key, () => new Katana());

            // Act
            object weapon1 = container.GetInstance<IWeapon>(key);
            object weapon2 = container.GetInstance<IWeapon>(key);

            // Assert
            Assert.AreEqual(weapon1, weapon2, "When requesting multiple instances of a type that has been " +
                "registered by RegisterSingleByKey<T>(string, Func<T>) should always result in the same instance.");
        }

        [TestMethod]
        public void RegisterSingleFuncByKey_RequestingMultipleInstances_ContainerCallDelegateOnlyOnce()
        {
            // Arrange
            int expectedNumberOfCalls = 1;
            int actualNumberOfCalls = 0;
            string key = "valid key";
            var container = new SimpleServiceLocator();

            container.RegisterSingleByKey<IWeapon>(key, () =>
            {
                actualNumberOfCalls++;
                return new Katana();
            });

            // Act
            container.GetInstance<IWeapon>(key);
            container.GetInstance<IWeapon>(key);
            container.GetInstance<IWeapon>(key);

            // Assert
            Assert.AreEqual(expectedNumberOfCalls, actualNumberOfCalls, "The delegate was called more than " +
                "once. The container must ensure the delegate is called just once during the apps lifetime.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleFuncByKey_WithNullKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            string invalidKey = null;

            // Act
            container.RegisterSingleByKey<IWeapon>(invalidKey, () => new Katana());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterSingleFuncByKey_WithEmptyKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            string invalidKey = string.Empty;

            // Act
            container.RegisterSingleByKey<IWeapon>(invalidKey, () => new Katana());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleFuncByKey_WithNullFunc_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            Func<IWeapon> instanceCreator = null;

            // Act
            container.RegisterSingleByKey<IWeapon>("valid key", instanceCreator);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once by key.")]
        public void RegisterSingleFuncByKey_CalledTwiceOnSameTypeAndKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>("Weapon", () => new Katana());

            // Act
            container.RegisterSingleByKey<IWeapon>("Weapon", () => new Tanto());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once by key.")]
        public void RegisterSingleFuncByKey_CalledAfterRegisterByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterByKey<Warrior>(key => new Ninja(null));

            // Act
            container.RegisterSingleByKey<Warrior>("Samurai", () => new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterSingleFuncByKey_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Tanto());
            container.GetInstance<IWeapon>();

            // Act
            container.RegisterSingleByKey<Warrior>("Samurai", () => new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetAllInstances.")]
        public void RegisterSingleFuncByKey_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();
            var count = weapons.Count();

            // Act
            container.RegisterSingleByKey<Warrior>("Samurai", () => new Samurai(null));
        }

        [TestMethod]
        public void RegisterSingleFuncByKey_TypeRegisteredUsingRegisterSingleInstance_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            // This is valid behavior, because this allows the user to register a default (key-less) instance
            // and multiple keyed instances.
            container.RegisterSingleByKey<IWeapon>("Tanto", () => new Tanto());

            // Assert
            Assert.IsNotNull(container.GetInstance<IWeapon>("Tanto"));
        }

        [TestMethod]
        public void RegisterSingleFuncByKey_TypeRegisteredUsingRegisterSingleInstanceByKey_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>("Katana", new Katana());

            // Act
            // This is valid behavior, because this allows the user to register keyed instance of the same
            // type in multiple ways.
            container.RegisterSingleByKey<IWeapon>("Tanto", () => new Tanto());

            // Assert
            Assert.IsNotNull(container.GetInstance<IWeapon>("Katana"));
            Assert.IsNotNull(container.GetInstance<IWeapon>("Tanto"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Registration of (string, Func<T>) should fail after a Func<string, T>  was registered.")]
        public void RegisterSingleFuncByKey_TypeRegisteredUsingRegisterSingleKeyedFunc_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>(key => new Tanto());

            // Act
            // This is invalid behavior, because this would make the user's configuration hard to follow and
            // it won't be clear to the users which of these registrations should go before the other.
            container.RegisterSingleByKey<IWeapon>("Katana", () => new Katana());
        }
        
        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByKey_NoInstanceByThatKeyRegisteredWithRegisterSingleByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>("Tanto", new Tanto());

            // Act
            container.GetInstance<IWeapon>("Katana");
        }
    }
}