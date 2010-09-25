using System;
using System.Linq;

using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    /// <summary>
    /// Tests for using objects with the singleton objects with the <see cref="SimpleServiceLocator"/>.
    /// </summary>
    [TestClass]
    public class SingletonTests
    {
        [TestMethod]
        public void RegisterSingleByInstance_WithValidType_ContainerAlwaysReturnsSameInstance()
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleByInstance_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            IWeapon invalidInstance = null;

            // Act
            container.RegisterSingle<IWeapon>(invalidInstance);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterSingleByInstance_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            container.RegisterSingle<IWeapon>(new Tanto());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterSingleByInstance_CalledAfterRegisterOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<Warrior>(() => new Samurai(null));

            // Act
            container.RegisterSingle<Warrior>(new Ninja(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterSingleByInstance_AfterCallingGetInstance_ThrowsException()
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
        public void RegisterSingleByInstance_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();

            // Calling count will iterate the collections. 
            // The container will only get locked when the first item is retrieved.
            var count = weapons.Count();

            // Act
            container.RegisterSingle<Warrior>(new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleByDelegate_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            Func<IWeapon> invalidDelegate = null;

            // Act
            container.RegisterSingle<IWeapon>(invalidDelegate);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterSingleByDelegate_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(() => new Katana());

            // Act
            container.RegisterSingle<IWeapon>(() => new Tanto());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterSingleByDelegate_CalledAfterRegisterOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<Warrior>(() => new Samurai(null));

            // Act
            container.RegisterSingle<Warrior>(() => new Ninja(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterSingleByDelegate_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(() => new Tanto());
            container.GetInstance<IWeapon>();

            // Act
            container.RegisterSingle<Warrior>(() => new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetAllInstances.")]
        public void RegisterSingleByDelegate_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();

            // Calling count will iterate the collections. 
            // The container will only get locked when the first item is retrieved.
            var count = weapons.Count();

            // Act
            container.RegisterSingle<Warrior>(() => new Samurai(null));
        }

        [TestMethod]
        public void RegisterSingleByDelegate_RegisteringDelegate_WillNotCallTheDelegate()
        {
            // Arrange
            int numberOfTimesDelegateWasCalled = 0;

            var container = new SimpleServiceLocator();

            // Act
            container.RegisterSingle<IWeapon>(() =>
            {
                numberOfTimesDelegateWasCalled++;
                return new Katana();
            });

            // Assert
            Assert.AreEqual(0, numberOfTimesDelegateWasCalled,
                "The RegisterSingle method should not call the delegate, because users may need objects " +
                "that are not yet registered. Users are allowed to register dependent objects in random order.");
        }

        [TestMethod]
        public void RegisterSingleByDelegate_CallingGetInstanceMultipleTimes_WillOnlyCallDelegateOnce()
        {
            // Arrange
            const int ExpectedNumberOfCalles = 1;
            int actualNumberOfCalls = 0;

            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(() =>
            {
                actualNumberOfCalls++;
                return new Katana();
            });

            // Act
            container.GetInstance<IWeapon>();
            container.GetInstance<IWeapon>();
            container.GetInstance<IWeapon>();

            // Assert
            Assert.AreEqual(ExpectedNumberOfCalles, actualNumberOfCalls,
                "The RegisterSingle method should register the object in such a way that the delegate will " +
                "only get called once during the lifetime of the application. Not more.");
        }

        [TestMethod]
        public void RegisterSingle_RegisteringAConcreteType_ReturnAnInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<IWeapon>(() => new Katana());

            // Act
            container.RegisterSingle<Samurai>();

            // Assert
            var samurai = container.GetInstance<Samurai>();

            Assert.IsNotNull(samurai, "The container should not return null.");
        }

        [TestMethod]
        public void RegisterSingle_RegisteringAConcreteType_AlwaysReturnsSameInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<IWeapon>(() => new Katana());

            // Act
            container.RegisterSingle<Samurai>();

            // Assert
            var s1 = container.GetInstance<Samurai>();
            var s2 = container.GetInstance<Samurai>();

            Assert.IsTrue(Object.ReferenceEquals(s1, s2), "Always the same instance was expected to be returned.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The abstract type IWeapon was not expected to be registered successfully.")]
        public void RegisterSingle_RegisteringANonConcreteType_WillThrowAnArgumentException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterSingle<IWeapon>();
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

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstance_TypeRegisteredUsingRegisterSingleKeyedFuncReturningNullInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>(key => null);

            // Act
            container.GetInstance<IWeapon>("any key");
        }
    }
}