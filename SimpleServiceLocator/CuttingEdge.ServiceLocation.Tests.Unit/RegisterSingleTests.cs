using System;
using System.Linq;

using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class RegisterSingleTests
    {
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