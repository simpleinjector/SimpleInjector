using System;
using System.Linq;
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
            int numberOfTimesDelegateWasCalled = 0;

            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(() =>
            {
                numberOfTimesDelegateWasCalled++;
                return new Katana();
            });

            // Act
            container.GetInstance<IWeapon>();
            container.GetInstance<IWeapon>();
            container.GetInstance<IWeapon>();

            // Assert
            Assert.AreEqual(1, numberOfTimesDelegateWasCalled,
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
        [ExpectedException(typeof(InvalidOperationException), "The abstract type IWeapon was not expected to be registered succesfully.")]
        public void RegisterSingle_RegisteringANonConcreteType_WillThrowAnArgumentException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterSingle<IWeapon>();
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

            IWeapon instance = null;

            // Act
            container.RegisterSingleByKey<IWeapon>("valid key", instance);
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
    }
}