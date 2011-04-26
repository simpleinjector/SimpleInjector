using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.Tests.Unit
{
    [TestClass]
    public class UnregisteredTypeTests
    {
        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstance_UnregisteredAbstractType_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.GetInstance<IUserRepository>();
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteType_CanStillBeCreated()
        {
            // Arrange
            var container = new Container();
            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // Act
            // RealUserService is concrete with a constructor with a single argument of type IUserRepository.
            var instance = container.GetInstance<RealUserService>();

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteType_AlwaysReturnsANewInstance()
        {
            // Arrange
            var container = new Container();
            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // Act
            object instance1 = container.GetInstance<RealUserService>();
            object instance2 = container.GetInstance<RealUserService>();

            // Assert
            Assert.AreNotEqual(instance1, instance2, "Values should reference different instances.");
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithConcreteConstructorArguments_CanStillBeCreated()
        {
            // Arrange
            var container = new Container();
            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // Act
            // Get the concrete class with a constructor with the argument of concrete type RealUserService.
            var instance = container.GetInstance<ConcreteTypeWithConcreteTypeConstructorArgument>();

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithMultiplePublicConstructors_ThrowsException()
        {
            // Arrange
            var container = new Container();

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
            var container = new Container();

            try
            {
                // Act
                container.GetInstance<RealUserService>();

                // Assert
                Assert.Fail("Because we did not register the IUserRepository interface, " +
                    "GetInstance<RealUserService> should fail.");
            }
            catch (ActivationException ex)
            {
                string message = ex.Message;

                AssertThat.StringContains(typeof(RealUserService).FullName, ex.Message,
                    "The exception message should contain the name of the type.");

                AssertThat.StringContains(typeof(IUserRepository).FullName, ex.Message,
                    "The exception message should contain the missing constructor argument.");

                AssertThat.StringContains("Please ensure " + typeof(IUserRepository).Name +
                    " is registered in the container", ex.Message,
                    "(1) The exception message should give a solution to solve the problem.");

                AssertThat.StringContains("register the type " + typeof(RealUserService).Name + " directly", 
                    ex.Message,
                    "(2) The exception message should give a solution to solve the problem.");
            }
        }

        [TestMethod]
        public void GetInstance_WithUnregisteredGenericTypeDefinition_ThrowsException()
        {
            // Arrange
            var container = new Container();

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
            var container = new Container();

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IUserRepository))
                {
                    e.Register(() => new SqlUserRepository());
                }
            };

            // Act
            // RealUserService contains an constructor argument of IUserRepository.
            container.GetInstance<RealUserService>();
        }

        [TestMethod]
        public void GetInstance_OnConcreteTypeWithValueTypeConstructorArgument_FailsWithExpectedException()
        {
            // Arrange
            string expectedMessage = typeof(ConcreteTypeWithValueTypeConstructorArgument).Name + " contains" +
                " parameter 'intParam' of type System.Int32 which can not be used for constructor injection.";

            var container = new Container();

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

            var container = new Container();

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
            var container = new Container();

            container.Register<IUserRepository>(() => { throw new InvalidOperationException("Bla."); });

            try
            {
                // Act
                container.GetInstance<RealUserService>();

                Assert.Fail("This call is expected to fail, because RealUserService depends on IUserRepository.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(typeof(RealUserService).Name, ex.Message);
                AssertThat.StringContains(typeof(IUserRepository).Name, ex.Message);
                AssertThat.StringContains("Bla", ex.Message);
            }            
        }
    }
}