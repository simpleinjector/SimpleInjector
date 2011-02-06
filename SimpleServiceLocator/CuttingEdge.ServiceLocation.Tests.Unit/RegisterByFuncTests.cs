using System;
using System.Linq;

using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class RegisterByFuncTests
    {
        [TestMethod]
        public void RegisterByFunc_WithNullKeyCalledAfterRegisterByKey_ReturnDefaultInstance()
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
        public void RegisterByFunc_WithNullKeyCalledAfterRegisterSingleByKey_ReturnsDefaultInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Note that the key is not used: a new knife will be returned on every call.
            container.RegisterSingleByKey<IWeapon>("knife", new Tanto());
            container.Register<IWeapon>(() => new Katana());

            // Act
            var instance = container.GetInstance<IWeapon>(null);

            // Assert
            // GetInstance(null) will never use the instance register with RegisterSingleByKey.
            Assert.IsInstanceOfType(instance, typeof(Katana));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterByFunc_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<Warrior>(() => new Samurai(null));

            // Act
            container.Register<Warrior>(() => new Ninja(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterByFunc_CalledAfterRegisterSingleOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            container.Register<IWeapon>(() => new Tanto());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterByFunc_AfterCallingGetInstance_ThrowsException()
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
        public void RegisterByFunc_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();

            // Only during iterating the collection, will the underlying container be called. This is a
            // Common Service Locator thing.
            var count = weapons.Count();

            // Act
            container.Register<Warrior>(() => new Samurai(null));
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
                    case "knife":
                        return new Tanto();
                    default:
                        // When name unknown, return null.
                        return null;
                }
            });

            // Act
            container.GetInstance<IWeapon>("sword");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterByFunc_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            Func<IWeapon> invalidInstanceCreator = null;

            // Act
            container.Register<IWeapon>(invalidInstanceCreator);
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByKey_RegisteredWithFuncReturningNull_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<IWeapon>(() => null);

            // Act
            container.GetInstance<IWeapon>();
        }

        [TestMethod]
        public void GetInstance_ForConcreteUnregisteredTypeWithDependencyRegisteredWithRegister_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // This registration will make the DelegateBuilder call the 
            // SingletonInstanceProducer.BuildExpression method.
            container.Register<IWeapon>(() => new Katana());

            // Act
            container.GetInstance<Samurai>();
        }
    }
}