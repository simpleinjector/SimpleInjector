using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class UnregisteredTypeTests
    {
        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstance_UnregisteredAbstractType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.GetInstance<IWeapon>();
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByKey_UnregisteredAbstractType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.GetInstance<IWeapon>("Tanto");
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByKey_UnregisteredConcreteType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.Register<IWeapon>(() => new Katana());

            // Act
            container.GetInstance<ConcreteTypeWithConcreteTypeConstructorArgument>("Tanto");
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteType_CanStillBeCreated()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            // Samurai is a concrete class with a constructor with a single argument of type IWeapon.
            var instance = container.GetInstance<Samurai>();

            // Arrange
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteType_AlwaysReturnsANewInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            object instance1 = container.GetInstance<Samurai>();
            object instance2 = container.GetInstance<Samurai>();

            // Assert
            Assert.AreNotEqual(instance1, instance2, "Values should reference different instances.");
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithConcreteConstructorArguments_CanStillBeCreated()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            // SamuraiWrapper is a concrete class with a constructor with a single argument of concrete type 
            // Samurai.
            var instance = container.GetInstance<ConcreteTypeWithConcreteTypeConstructorArgument>();

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithMultiplePublicConstructors_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            try
            {
                // Act
                container.GetInstance<ConcreteTypeWithMultiplePublicConstructors>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ActivationException ex)
            {
                string message = ex.Message;

                Assert.IsTrue(message.Contains(typeof(ConcreteTypeWithMultiplePublicConstructors).FullName),
                    "The exception message should contain the name of the type. Actual message: " + message);

                Assert.IsTrue(message.Contains("type should contain exactly one public constructor"),
                    "The exception message should describe the actual problem. Actual message: " + message);
            }
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithConstructorWithInvalidArguments_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            try
            {
                // Act
                // Because we did not register the IWeapon interface, GetInstance<Samurai> should fail.
                container.GetInstance<Samurai>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ActivationException ex)
            {
                string message = ex.Message;

                Assert.IsTrue(message.Contains(typeof(Samurai).FullName),
                    "The exception message should contain the name of the type. Actual message: " + message);

                Assert.IsTrue(message.Contains(typeof(IWeapon).FullName),
                    "The exception message should contain the missing constructor argument. " +
                    "Actual message: " + message);

                Assert.IsTrue(message.Contains("Please ensure IWeapon is registered in the container"),
                    "(1) The exception message should give a solution to solve the problem. " +
                    "Actual message: " + message);

                Assert.IsTrue(message.Contains("register the type Samurai directly"),
                    "(2) The exception message should give a solution to solve the problem. " +
                    "Actual message: " + message);
            }
        }

        [TestMethod]
        public void GetInstance_WithUnregisteredGenericTypeDefinition_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            try
            {
                // Act
                container.GetInstance(typeof(GenericType<>));

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(typeof(GenericType<>).Name));
            }
        }
    }
}