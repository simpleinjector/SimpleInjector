using System;

using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class RegisterFuncByKeyTests
    {
        [TestMethod]
        public void RegisterFuncByKey_WithValidArguments_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            string validKey = "katana";
            Func<IWeapon> validInstanceCreator = () => new Katana();

            // Act
            container.RegisterByKey<IWeapon>(validKey, validInstanceCreator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFuncByKey_WithNullKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            string invalidKey = null;
            Func<IWeapon> validInstanceCreator = () => new Katana();

            // Act
            container.RegisterByKey<IWeapon>(invalidKey, validInstanceCreator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterFuncByKey_WithNullFunc_ThrowException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            string validKey = "katana";
            Func<IWeapon> invalidInstanceCreator = null;

            // Act
            container.RegisterByKey<IWeapon>(validKey, invalidInstanceCreator);
        }

        [TestMethod]
        public void RegisterFuncByKey_ValidRegistration_ContainerCallsDelegateOnEachRequest()
        {
            // Arrange
            const int ExpectedNumberOfCalls = 2;
            int actualNumberOfCalls = 0;
            var container = new SimpleServiceLocator();

            Func<IWeapon> instanceCreator = () =>
            {
                actualNumberOfCalls++;
                return new Katana();
            };

            container.RegisterByKey<IWeapon>("katana", instanceCreator);

            // Act
            container.GetInstance<IWeapon>("katana");
            container.GetInstance<IWeapon>("katana");

            // Assert
            Assert.AreEqual(ExpectedNumberOfCalls, actualNumberOfCalls,
                "The container is expected to call the delegate on each call to GetInstance.");
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void RegisterFuncByKey_RequestingAnUnregisteredKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterByKey<IWeapon>("katana", () => new Katana());

            // Act
            // This call is expected to fail.
            container.GetInstance<IWeapon>("tanto");
        }

        [TestMethod]
        public void RegisterFuncByKey_CalledAfterRegisterFunc_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.Register<IWeapon>(() => new Katana());

            // Act
            // Registration of keyed instance of a specific service type can be mixed with a key-less 
            // registrations.
            container.RegisterByKey<IWeapon>("tanto", () => new Tanto());

            // Assert
            Assert.IsInstanceOfType(container.GetInstance<IWeapon>(), typeof(Katana));
            Assert.IsInstanceOfType(container.GetInstance<IWeapon>("tanto"), typeof(Tanto));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Calling RegisterByKey<T>(string, Func<T>) " +
            "should fail when RegisterByKey<T>(Func<string, T>) is already called for the same T.")]
        public void RegisterFuncByKey_CalledAfterRegisterKeyedFunc_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterByKey<IWeapon>(key => new Katana());

            // Act
            // This call is expected to fail, because allowing this behavior would make the API less
            // transparent. These methods are mutually exclusive.
            container.RegisterByKey<IWeapon>("tanto", () => new Tanto());
        }
    }
}