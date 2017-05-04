namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnregisteredTypeTests
    {
        [TestMethod]
        public void GetInstance_UnregisteredAbstractType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetInstance<IUserRepository>();

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void GetInstance_UnregisteredAbstractType2_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // This call forces a different code path through the container.
            container.GetRegistration(typeof(IUserRepository));

            // Act
            Action action = () => container.GetInstance<IUserRepository>();

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void GetInstance_UnregisteredAbstractType3_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.GetRegistration(typeof(IUserRepository));

            // Act
            Action action = () => container.GetInstance(typeof(IUserRepository));

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void GetInstance_UnregisteredValueType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetInstance(typeof(int));

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteType_CanStillBeCreated()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterSingleton<IUserRepository>(new SqlUserRepository());

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
            var container = ContainerFactory.New();
            container.RegisterSingleton<IUserRepository>(new SqlUserRepository());

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
            var container = ContainerFactory.New();
            container.RegisterSingleton<IUserRepository>(new SqlUserRepository());

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
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetInstance<ConcreteTypeWithMultiplePublicConstructors>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                typeof(ConcreteTypeWithMultiplePublicConstructors).Name,
                action);

            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "should have only one public constructor: it has 2.",
                action);
        }

        [TestMethod]
        public void GetInstanceNonGeneric_UnregisteredConcreteTypeWithMultiplePublicConstructors_ThrowsExceptionWithNameOfType()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                // Act
                container.GetInstance(typeof(ConcreteTypeWithMultiplePublicConstructors));

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ActivationException ex)
            {
                string message = ex.Message;

                AssertThat.StringContains(typeof(ConcreteTypeWithMultiplePublicConstructors).Name, message);
            }
        }

        [TestMethod]
        public void GetInstanceNonGeneric_UnregisteredConcreteTypeWithMultiplePublicConstructors_ThrowsExceptionWithExpectedMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetInstance(typeof(ConcreteTypeWithMultiplePublicConstructors));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "should have only one public constructor: it has 2.",
                action,
                "The exception message should describe the actual problem.");
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithConstructorWithInvalidArguments_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

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

                AssertThat.ExceptionMessageContains(typeof(RealUserService).Name, ex,
                    "The exception message should contain the name of the type.");

                AssertThat.ExceptionMessageContains(typeof(IUserRepository).Name, ex,
                    "The exception message should contain the missing constructor argument.");

                AssertThat.ExceptionMessageContains(
                    "Please ensure IUserRepository is registered",
                    ex, "(1) The exception message should give a solution to solve the problem.");

                AssertThat.ExceptionMessageContains(@"
                    Please ensure IUserRepository is registered,
                    or change the constructor of RealUserService"
                    .TrimInside(),
                    ex,
                    "(2) The exception message should give a solution to solve the problem.");
            }
        }
        
        [TestMethod]
        public void GetRegistration_UnregisteredConcreteTypeWithConstructorWithInvalidArguments_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                // Act
                container.GetRegistration(typeof(RealUserService), throwOnFailure: true);

                // Assert
                Assert.Fail("Because we did not register the IUserRepository interface, " +
                    "GetRegistration should fail.");
            }
            catch (ActivationException ex)
            {
                string message = ex.Message;

                AssertThat.ExceptionMessageContains(typeof(RealUserService).Name, ex,
                    "The exception message should contain the name of the type.");

                AssertThat.ExceptionMessageContains(typeof(IUserRepository).Name, ex,
                    "The exception message should contain the missing constructor argument.");

                AssertThat.ExceptionMessageContains(
                    "Please ensure IUserRepository is registered",
                    ex, "(1) The exception message should give a solution to solve the problem.");

                AssertThat.ExceptionMessageContains(@"
                    Please ensure IUserRepository is registered,
                    or change the constructor of RealUserService"
                    .TrimInside(),
                    ex,
                    "(2) The exception message should give a solution to solve the problem.");
            }
        }

        [TestMethod]
        public void GetInstance_WithUnregisteredGenericTypeDefinition_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetInstance(typeof(GenericType<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "GenericType<T>",
                action);
        }

        [TestMethod]
        public void GetInstance_OnConcreteTypeWithConstructorArgumentOfResolvableType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

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
                " parameter 'intParam' of type Int32 which can not be used for constructor " +
                "injection because it is a value type.";

            var container = ContainerFactory.New();

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
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_OnConcreteTypeWithStringConstructorArgument_FailsWithExpectedException()
        {
            // Arrange
            string expectedMessage = typeof(ConcreteTypeWithStringConstructorArgument).Name + " contains pa" +
                "rameter 'stringParam' of type String which can not be used for constructor injection.";

            var container = ContainerFactory.New();

            try
            {
                // Act
                container.GetInstance<ConcreteTypeWithStringConstructorArgument>();

                // Assert
                Assert.Fail("The call was expected to fail.");
            }
            catch (ActivationException ex)
            {
                AssertThat.ExceptionMessageContains(expectedMessage, ex);
            }
        }

        [TestMethod]
        public void GetInstance_WithErrorInNestedDependency_ThrowsExceptionThatContainsAllTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

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

                // Note: the next line is removed. We optimized Func<T> registrations, and because of this
                // we miss the information about that type.
                // AssertThat.StringContains(typeof(IUserRepository).Name, ex.Message);
                AssertThat.StringContains("Bla", ex.Message);
            }
        }
    }
}