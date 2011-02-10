using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class RegisterSingleByGenericArgumentTests
    {
        [TestMethod]
        public void RegisterSingleByGenericArgument_WithValidGenericArguments_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterSingle<IWeapon, Katana>();
        }

        [TestMethod]
        public void GetInstance_OnRegisteredType_ReturnsInstanceOfExpectedType()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingle<IWeapon, Katana>();

            // Act
            var instance = container.GetInstance<IWeapon>();

            // Assert
            Assert.IsInstanceOfType(instance, typeof(Katana));
        }

        [TestMethod]
        public void GetInstance_OnRegisteredType_ReturnsANewInstanceOnEachCall()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingle<IWeapon, Tanto>();

            // Act
            var instance1 = container.GetInstance<IWeapon>();
            var instance2 = container.GetInstance<IWeapon>();

            // Assert
            Assert.AreEqual(instance1, instance2, "RegisterSingle<TService, TImplementation>() should " +
                "return singleton objects.");
        }

        [TestMethod]
        public void RegisterSingleByGenericArgument_GenericArgumentOfInvalidType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            try
            {
                // Act
                container.Register<object, ConcreteTypeWithValueTypeConstructorArgument>();

                // Assert
                Assert.Fail("Registration of ConcreteTypeWithValueTypeConstructorArgument should fail.");
            }
            catch (ArgumentException ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException), "No subtype was expected.");

                Assert.AreEqual(ex.ParamName, "TImplementation");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterSingleByGenericArgument_CalledAfterTheContainerWasLocked_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.GetInstance<PluginManager>();

            // Act
            container.RegisterSingle<IWeapon, Katana>();
        }
    }
}