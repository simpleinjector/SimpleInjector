using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class RegisterAllTests
    {
        [TestMethod]
        public void GetInstance_TypeWithEnumerableAsConstructorArguments_InjectsExpectedTypes()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterAll<IPlugin>(new PluginImpl(), new PluginImpl(), new PluginImpl());
            
            // Act
            // PluginManager has a ctor with an IEnumerable<IPlugin> argument.
            var manager = container.GetInstance<PluginManager>();

            // Assert
            Assert.AreEqual(3, manager.Plugins.Length);
        }

        [TestMethod]
        public void GetInstance_EnumerableTypeRegisteredWithRegisterSingle_InjectsExpectedTypes()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            IPlugin[] plugins = new IPlugin[] { new PluginImpl(), new PluginImpl(), new PluginImpl() };

            // RegisterSingle<IEnumerable<T>> should have the same effect as RegisterAll<T>
            container.RegisterSingle<IEnumerable<IPlugin>>(plugins);

            // Act
            // PluginManager has a ctor with an IEnumerable<IPlugin> argument.
            var manager = container.GetInstance<PluginManager>();

            // Assert
            Assert.AreEqual(3, manager.Plugins.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterSingle_WithEnumerableCalledAfterRegisterAllWithSameType_Fails()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterAll<IPlugin>(new PluginImpl());

            // Act
            container.RegisterSingle<IEnumerable<IPlugin>>(new IPlugin[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Register_WithEnumerableCalledAfterRegisterAllWithSameType_Fails()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterAll<IPlugin>(new PluginImpl());

            // Act
            container.Register<IEnumerable<IPlugin>>(() => new IPlugin[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterAll_WithEnumerableCalledAfterRegisterSingleWithSameType_Fails()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingle<IEnumerable<IPlugin>>(new IPlugin[0]);
            
            // Act
            container.RegisterAll<IPlugin>(new PluginImpl());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterAll_WithEnumerableCalledAfterRegisterWithSameType_Fails()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.Register<IEnumerable<IPlugin>>(() => new IPlugin[0]);

            // Act
            container.RegisterAll<IPlugin>(new PluginImpl());
        }

        [TestMethod]
        public void GetAllInstances_ListRegisteredUsingEnumerable_ReturnsExpectedList()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            IEnumerable<IWeapon> weaponsToRegister = new IWeapon[] { new Tanto(), new Katana() };
            container.RegisterAll<IWeapon>(weaponsToRegister);

            // Act
            var weapons = container.GetAllInstances<IWeapon>();

            // Assert
            Assert.IsNotNull(weapons, "This method MUST NOT return null.");
            Assert.AreEqual(2, weapons.Count(), "Collection is expected to contain two values.");
        }

        [TestMethod]
        public void GetAllInstances_ListRegisteredUsingParams_ReturnsExpectedList()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            
            container.RegisterAll<IWeapon>(new Tanto(), new Katana());

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
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterAll_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Tanto());
            container.GetInstance<IWeapon>();

            // Act
            container.RegisterAll<IWeapon>(new IWeapon[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetAllInstances.")]
        public void RegisterAll_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();
            var count = weapons.Count();

            // Act
            container.RegisterAll<IWeapon>(new IWeapon[0]);
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

            // Act
            container.RegisterAll<IWeapon>(weapons);
        }
    }
}