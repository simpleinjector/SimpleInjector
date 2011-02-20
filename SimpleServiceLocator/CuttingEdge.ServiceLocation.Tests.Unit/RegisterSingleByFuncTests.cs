using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class RegisterSingleByFuncTests
    {
        [TestMethod]
        public void RegisterSingleByFun_WithValidArgument_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            Func<IWeapon> validDelegate = () => new Katana();

            // Act
            container.RegisterSingle<IWeapon>(validDelegate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleByFunc_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            Func<IWeapon> invalidDelegate = null;

            // Act
            container.RegisterSingle<IWeapon>(invalidDelegate);
        }

        [TestMethod]
        public void Validate_ValidRegisterSingleByFuncRegistration_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            Func<IWeapon> validDelegate = () => new Katana();
            container.RegisterSingle<IWeapon>(validDelegate);

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Validate_InValidRegisterSingleByFuncRegistration_ThrowsExpectedExceptionMessage()
        {
            // Arrange
            string expectedMessage = "The registered delegate for type " +
                "CuttingEdge.ServiceLocation.Tests.Unit.IWeapon returned null";

            var container = new SimpleServiceLocator();
            Func<IWeapon> invalidDelegate = () => null;
            container.RegisterSingle<IWeapon>(invalidDelegate);

            try
            {
                // Act
                container.Verify();

                // Arrange
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(expectedMessage), "Actual message: " + ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterSingleByFunc_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(() => new Katana());

            // Act
            container.RegisterSingle<IWeapon>(() => new Tanto());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterSingleByFunc_CalledAfterRegisterOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<Warrior>(() => new Samurai(null));

            // Act
            container.RegisterSingle<Warrior>(() => new Ninja(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterSingleByFunc_AfterCallingGetInstance_ThrowsException()
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
        public void RegisterSingleByFunc_AfterCallingGetAllInstances_ThrowsException()
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
        public void RegisterSingleByFunc_RegisteringDelegate_WillNotCallTheDelegate()
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
        public void RegisterSingleByFunc_CallingGetInstanceMultipleTimes_WillOnlyCallDelegateOnce()
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
        public void GetInstance_ForConcreteUnregisteredTypeWithDependencyRegisteredWithRegisterSingleFunc_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // This registration will make the DelegateBuilder call the 
            // FuncSingletonInstanceProducer.BuildExpression method.
            container.RegisterSingle<IWeapon>(() => new Katana());

            // Act
            container.GetInstance<Samurai>();
        }
    }
}