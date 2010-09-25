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
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterByKeyedFunc_WithNullString_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            string invalidKey = null;
            Func<IWeapon> validInstanceCreator = () => new Katana();

            // Act
            container.RegisterByKey<IWeapon>(invalidKey, validInstanceCreator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterByKeyedFunc_WithEmptyString_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var invalidKey = string.Empty;
            Func<IWeapon> validInstanceCreator = () => new Katana();

            // Act
            container.RegisterByKey<IWeapon>(invalidKey, validInstanceCreator);
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
    }
}