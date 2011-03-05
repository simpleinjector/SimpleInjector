using System;

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
            container.GetInstance<IWeapon>("knife");
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByKey_UnregisteredConcreteType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.Register<IWeapon>(() => new Katana());

            // Act
            container.GetInstance<ConcreteTypeWithConcreteTypeConstructorArgument>("knife");
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

            // Assert
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
            // Get the concrete class with a constructor with a single argument of concrete type Samurai.
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

                Assert.IsTrue(message.Contains("should contain exactly one public constructor, but it has 2."),
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

        [TestMethod]
        public void GetInstance_OnConcreteTypeWithConstructorArgumentOfResolvableType_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IWeapon))
                {
                    e.Register(() => new Katana());
                }
            };

            // Act
            // Samurai contains an constructor argument of IWeapon
            var samurai = container.GetInstance<Samurai>();
        }

        [TestMethod]
        public void GetInstance_OnConcreteTypeWithValueTypeConstructorArgument_FailsWithExpectedException()
        {
            // Arrange
            string expectedMessage = typeof(ConcreteTypeWithValueTypeConstructorArgument).Name + " contains" +
                " parameter 'intParam' of type System.Int32 which can not be used for constructor injection.";

            var container = new SimpleServiceLocator();

            try
            {
                // Act
                // This type contains constructor with parameter "int intParam".
                container.GetInstance<ConcreteTypeWithValueTypeConstructorArgument>();

                // Assert
                Assert.Fail("The call was expected to fail.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(expectedMessage), "Actual message: " + ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_OnConcreteTypeWithStringConstructorArgument_FailsWithExpectedException()
        {
            // Arrange
            string expectedMessage = typeof(ConcreteTypeWithStringConstructorArgument).Name + " contains pa" +
                "rameter 'stringParam' of type System.String which can not be used for constructor injection.";

            var container = new SimpleServiceLocator();

            try
            {
                // Act
                container.GetInstance<ConcreteTypeWithStringConstructorArgument>();

                // Assert
                Assert.Fail("The call was expected to fail.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(expectedMessage), "Actual message: " + ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_WithErrorInNestedDependency_ThrowsExceptionThatContainsAllTypes()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.Register<IWeapon>(() => { throw new InvalidOperationException("Bla."); });

            try
            {
                // Act
                container.GetInstance<Samurai>();

                Assert.Fail("This call is expected to fail, because Samurai has a dependency on IWeapon.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Samurai"), "Message should contain 'Samurai': " + ex.Message);
                Assert.IsTrue(ex.Message.Contains("IWeapon"), "Message should contain 'IWeapon': " + ex.Message);
                Assert.IsTrue(ex.Message.Contains("Bla"), "Message should contain 'Bla': " + ex.Message);
            }            
        }
    }
}