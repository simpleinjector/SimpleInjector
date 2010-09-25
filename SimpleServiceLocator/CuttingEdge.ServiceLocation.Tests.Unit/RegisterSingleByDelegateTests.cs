using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class RegisterSingleByDelegateTests
    {
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
    }
}
