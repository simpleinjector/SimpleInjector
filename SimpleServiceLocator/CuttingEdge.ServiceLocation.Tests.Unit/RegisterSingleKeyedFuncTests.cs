using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class RegisterSingleKeyedFuncTests
    {
        [TestMethod]
        public void RegisterSingleKeyedFunc_RequestingAnInstance_ContainerReturnsInstanceSuccesfully()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>(key => new Katana());

            // Act
            var weapon = container.GetInstance<IWeapon>("any key");

            // Assert
            Assert.IsNotNull(weapon);
        }

        [TestMethod]
        public void RegisterSingleKeyedFunc_RequestingMultipleInstances_ContainerAlwaysReturnsSameInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>(key => new Katana());

            // Act
            object weapon1 = container.GetInstance<IWeapon>("same key");
            object weapon2 = container.GetInstance<IWeapon>("same key");

            // Assert
            Assert.AreEqual(weapon1, weapon2, "When requesting multiple instances of a type that has been " +
                "registered by RegisterSingleByKey<T>(Func<string, T>) should always result in the same " +
                "instance for that key.");
        }

        [TestMethod]
        public void RegisterSingleKeyedFunc_RequestingMultipleInstances_ContainerCallDelegateOnlyOnce()
        {
            // Arrange
            int expectedNumberOfCalls = 1;
            int actualNumberOfCalls = 0;
            var container = new SimpleServiceLocator();

            container.RegisterSingleByKey<IWeapon>(key =>
            {
                actualNumberOfCalls++;
                return new Katana();
            });

            // Act
            container.GetInstance<IWeapon>("same key");
            container.GetInstance<IWeapon>("same key");
            container.GetInstance<IWeapon>("same key");

            // Assert
            Assert.AreEqual(expectedNumberOfCalls, actualNumberOfCalls, "The delegate was called more than " +
                "once. The container must ensure the delegate is called just once during the apps lifetime.");
        }

        [TestMethod]
        public void RegisterSingleKeyedFunc_RequestingMultipleKeys_ContainerCallDelegateOnlyPerKey()
        {
            // Arrange
            int expectedNumberOfCalls = 2;
            int actualNumberOfCalls = 0;
            var container = new SimpleServiceLocator();

            container.RegisterSingleByKey<IWeapon>(key =>
            {
                actualNumberOfCalls++;
                return new Katana();
            });

            // Act
            container.GetInstance<IWeapon>("key");
            container.GetInstance<IWeapon>("other key");

            // Assert
            Assert.AreEqual(expectedNumberOfCalls, actualNumberOfCalls, "The delegate was not called once " +
                "per key. The container must ensure the delegate is called just once during the apps " +
                "lifetime for each key.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleKeyedFunc_WithNullFunc_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            Func<string, IWeapon> keyedInstanceCreator = null;

            // Act
            container.RegisterSingleByKey<IWeapon>(keyedInstanceCreator);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterSingleKeyedFunc_CalledTwiceOnSameTypeAndKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>(key => new Katana());

            // Act
            container.RegisterSingleByKey<IWeapon>(key => new Tanto());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "A certain keyed type should only be able to be registered once.")]
        public void RegisterSingleKeyedFunc_CalledAfterRegisterByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterByKey<Warrior>(key => new Ninja(null));

            // Act
            container.RegisterSingleByKey<Warrior>(key => new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "The container should get locked after a call to GetInstance.")]
        public void RegisterSingleKeyedFunc_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Tanto());
            container.GetInstance<IWeapon>();

            // Act
            container.RegisterSingleByKey<Warrior>(key => new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "The container should get locked after a call to GetAllInstances.")]
        public void RegisterSingleKeyedFunc_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();
            var count = weapons.Count();

            // Act
            container.RegisterSingleByKey<Warrior>(key => new Samurai(null));
        }

        [TestMethod]
        public void RegisterSingleKeyedFunc_TypeRegisteredUsingRegisterSingleInstance_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            // This is valid behavior, because this allows the user to register a default (key-less) instance
            // and multiple keyed instances.
            container.RegisterSingleByKey<IWeapon>(key => new Tanto());

            // Assert
            Assert.IsNotNull(container.GetInstance<IWeapon>("any key"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Registration of Func<string, T> should fail after a (string, T) was registered.")]
        public void RegisterSingleKeyedFunc_TypeRegisteredUsingRegisterSingleInstanceByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>("Katana", new Katana());

            // Act
            // This is invalid behavior, because this would make the user's configuration hard to follow and
            // it won't be clear to the users which of these registrations should go before the other.
            container.RegisterSingleByKey<IWeapon>(key => new Tanto());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "Registration of Func<string, T> should fail after a (string, Func<T>) was registered.")]
        public void RegisterSingleKeyedFunc_TypeRegisteredUsingRegisterSingleFuncByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>("Katana", () => new Katana());

            // Act
            // This is invalid behavior, because this would make the user's configuration hard to follow and
            // it won't be clear to the users which of these registrations should go before the other.
            container.RegisterSingleByKey<IWeapon>(key => new Tanto());
        }
    }
}
