using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    /// <summary>
    /// Tests for the <see cref="SimpleServiceLocator"/>.
    /// </summary>
    [TestClass]
    public class SimpleServiceLocatorTests
    {
        [TestMethod]
        public void GetInstance_InstanceSetWithRegisterSingle_AlwaysReturnsSameInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            var instance1 = container.GetInstance<IWeapon>();
            var instance2 = container.GetInstance<IWeapon>();

            // Assert
            Assert.IsNotNull(instance1, "GetInstance should never return null.");
            Assert.AreEqual(instance1, instance2, "Values should reference the same instance.");
        }

        [TestMethod]
        public void GetInstance_InstanceSetWithRegister_AlwaysReturnsANewInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());
            container.Register<Warrior>(() =>
                {
                    return new Samurai(container.GetInstance<IWeapon>());
                });

            // Act
            var instance1 = container.GetInstance<Warrior>();
            var instance2 = container.GetInstance<Warrior>();

            // Assert
            Assert.AreNotEqual(instance1, instance2, "Values should reference different instances.");
        }

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
        public void GetInstanceByKey_InstanceSetWithRegisterByKey_AlwaysReturnsANewInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>("Ninja", new Tanto());
            container.RegisterByKey<Warrior>(name => new Ninja(container.GetInstance<IWeapon>(name)));

            // Act
            var weapon = container.GetInstance<Warrior>("Ninja").Weapon;

            // Assert
            Assert.IsInstanceOfType(weapon, typeof(Tanto));
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstance_NoRegisteredInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            
            // Act
            container.GetInstance<IWeapon>();
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByKey_NoRegisteredInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.GetInstance<IWeapon>("Tanto");
        }

        [TestMethod]
        public void GetInstanceByKey_WithNullKey_ReturnsDefaultInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var defaultInstance = new Katana();
            container.RegisterSingle<IWeapon>(defaultInstance);
            container.RegisterSingleByKey<IWeapon>("Ninja", new Tanto());

            // Act
            var weapon = container.GetInstance<IWeapon>(null);

            // Assert
            Assert.IsInstanceOfType(weapon, defaultInstance.GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByKey_NoInstanceByThatKeyRegisteredWithRegisterByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterByKey<IWeapon>(key =>
                {
                    switch (key)
                    {
                        case "Tanto":
                            return new Tanto();
                        default:
                            // When name unknown, return null.
                            return null;
                    }
                });

            // Act
            container.GetInstance<IWeapon>("Katana");
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

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByKey_NullRegisteredWithRegister_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<IWeapon>(() => null);

            // Act
            container.GetInstance<IWeapon>();
        }

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void Register_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.Register<IWeapon>(null);
        }

        [TestMethod]
        public void Register_WithNullKeyCalledAfterRegisterByKey_ReturnDefaultInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Note that the key is not used: a new Tanto will be returned on every call.
            container.RegisterByKey<IWeapon>(key => new Tanto());
            Func<IWeapon> defaultInstance = () => new Katana();
            container.Register<IWeapon>(defaultInstance);

            // Act
            var instance = container.GetInstance<IWeapon>(null);

            // Assert
            // GetInstance(null) will never use the Func<string, T> registered with RegisterByKey.
            Assert.IsInstanceOfType(instance, typeof(Katana));
        }

        [TestMethod]
        public void Register_WithNullKeyCalledAfterRegisterSingleByKey_ReturnsDefaultInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            
            // Note that the key is not used: a new Tanto will be returned on every call.
            container.RegisterSingleByKey<IWeapon>("Tanto", new Tanto());
            container.Register<IWeapon>(() => new Katana());

            // Act
            var instance = container.GetInstance<IWeapon>(null);

            // Assert
            // GetInstance(null) will never use the instance register with RegisterSingleByKey.
            Assert.IsInstanceOfType(instance, typeof(Katana));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterByKey_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterByKey<IWeapon>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterByKey_WithNullString_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            string invalidKey = null;

            // Act
            container.RegisterSingleByKey<IWeapon>(invalidKey, new Katana());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterByKey_WithEmptyString_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var invalidKey = string.Empty;

            // Act
            container.RegisterSingleByKey<IWeapon>(invalidKey, new Katana());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void Register_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<Warrior>(() => new Samurai(null));

            // Act
            container.Register<Warrior>(() => new Ninja(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void Register_CalledAfterRegisterSingleOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            container.Register<IWeapon>(() => new Tanto());
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException), "RegisterByKey will only get called with a key that's not null.")]
        public void GetInstance_WithNullKeyCalledAfterRegisterByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            
            // Note that the key is not used: a new Tanto will be returned on every call.
            container.RegisterByKey<IWeapon>(key => new Tanto());

            // Act
            var instance = container.GetInstance<IWeapon>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingle_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterSingle<IWeapon>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterSingle_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            container.RegisterSingle<IWeapon>(new Tanto());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterSingle_CalledAfterRegisterOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<Warrior>(() => new Samurai(null));

            // Act
            container.RegisterSingle<Warrior>(new Ninja(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once by key.")]
        public void RegisterByKey_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterByKey<Warrior>(key => new Samurai(null));

            // Act
            container.RegisterByKey<Warrior>(key => new Ninja(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once by key.")]
        public void RegisterByKey_CalledAfterRegisterSingleByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<Warrior>("Samurai", new Samurai(null));

            // Act
            container.RegisterByKey<Warrior>(key => new Ninja(null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleByKey_WithNullKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterSingleByKey<IWeapon>(null, new Katana());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleByKey_WithNullInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterSingleByKey<IWeapon>("valid key", null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once by key.")]
        public void RegisterSingleByKey_CalledTwiceOnSameTypeAndKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>("Weapon", new Katana());

            // Act
            container.RegisterSingleByKey<IWeapon>("Weapon", new Tanto());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once by key.")]
        public void RegisterSingleByKey_CalledAfterRegisterByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterByKey<Warrior>(key => new Ninja(null));

            // Act
            container.RegisterSingleByKey<Warrior>("Samurai", new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void Register_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Tanto());
            container.GetInstance<IWeapon>();

            // Act
            container.Register<Warrior>(() => new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetAllInstances.")]
        public void Register_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();

            // Only during iterating the collection, will the underlying container be called.
            var count = weapons.Count();

            // Act
            container.Register<Warrior>(() => new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterSingle_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Tanto());
            container.GetInstance<IWeapon>();

            // Act
            container.RegisterSingle<Warrior>(new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetAllInstances.")]
        public void RegisterSingle_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();
            var count = weapons.Count();

            // Act
            container.RegisterSingle<Warrior>(new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterByKey_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Tanto());
            container.GetInstance<IWeapon>();

            // Act
            container.RegisterByKey<Warrior>(key => new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetAllInstances.")]
        public void RegisterByKey_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();
            var count = weapons.Count();

            // Act
            container.RegisterByKey<Warrior>(key => new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterSingleByKey_AfterCallingGetInstance_ThrowsException()
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
        public void RegisterSingleByKey_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();
            var count = weapons.Count();

            // Act
            container.RegisterSingleByKey<Warrior>("Samurai", new Samurai(null));
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

            // Acty
            container.RegisterAll<IWeapon>(weapons);
        }

        [TestMethod]
        public void Validate_WithEmptyConfiguration_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            
            // Act
            container.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void Validate_WithFailingFunc_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<IWeapon>(() =>
                {
                    throw new ArgumentNullException();
                });

            // Act
            container.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void Validate_RegisteredCollectionWithNullElements_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterAll<IWeapon>(new IWeapon[] { null });

            // Act
            container.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void Validate_FailingCollection_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            IEnumerable<IWeapon> weapons =
                from nullWeapon in Enumerable.Repeat<IWeapon>(null, 1)
                where nullWeapon.ToString() == "This line fails with an NullReferenceException"
                select nullWeapon;

            container.RegisterAll<IWeapon>(weapons);

            // Act
            container.Validate();
        }
    }
}