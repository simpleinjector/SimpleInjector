using System;
using System.Linq;

using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class RegisterByKeyedFuncTests
    {
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterByKeyedFunc_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterByKey<IWeapon>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once by key.")]
        public void RegisterByKeyedFunc_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterByKey<Warrior>(key => new Samurai(null));

            // Act
            container.RegisterByKey<Warrior>(key => new Ninja(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once by key.")]
        public void RegisterByKeyedFunc_CalledAfterRegisterSingleByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<Warrior>("Samurai", new Samurai(null));

            // Act
            container.RegisterByKey<Warrior>(key => new Ninja(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterByKeyedFunc_AfterCallingGetInstance_ThrowsException()
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
        public void RegisterByKeyedFunc_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();
            var count = weapons.Count();

            // Act
            container.RegisterByKey<Warrior>(key => new Samurai(null));
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
        public void GetInstance_ContainerRegisteredWithThrowingDelegate_ThrowsExpectedExceptionMessage()
        {
            // Arrange
            string expectedMessage = "The registered delegate for type " +
                "CuttingEdge.ServiceLocation.Tests.Unit.IWeapon threw an exception.";

            var container = new SimpleServiceLocator();

            Func<string, IWeapon> invalidDelegate = key => { throw new NullReferenceException(); };

            container.RegisterByKey<IWeapon>(invalidDelegate);

            try
            {
                // Act
                container.GetInstance<IWeapon>("any key");

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(expectedMessage), "Actual message: " + ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_ThrowingDelegateRegisteredUsingRegisterByKeyedFunc_ThrowsActivationExceptionWithExpectedInnerException()
        {
            // Arrange
            var expectedInnerException = new InvalidOperationException();

            var container = new SimpleServiceLocator();
            container.RegisterByKey<IWeapon>(key => { throw expectedInnerException; });

            try
            {
                // Act
                container.GetInstance<IWeapon>("any key :-)");

                // Assert
                Assert.Fail("The GetInstance method was expected to fail, because of the faulty registration.");
            }
            catch (ActivationException ex)
            {
                Assert.AreEqual(expectedInnerException, ex.InnerException,
                    "The exception thrown by the registered delegate is expected to be wrapped in the " +
                    "thrown ActivationException.");
            }
        }
    }
}