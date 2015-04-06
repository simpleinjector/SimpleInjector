namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterSingleByFuncTests
    {
        [TestMethod]
        public void RegisterSingleByFunc_WithValidArgument_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();
            Func<IUserRepository> validDelegate = () => new SqlUserRepository();

            // Act
            container.RegisterSingle<IUserRepository>(validDelegate);
        }

        [TestMethod]
        public void RegisterSingleByFunc_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            Func<IUserRepository> invalidDelegate = null;

            // Act
            Action action = () => container.RegisterSingle<IUserRepository>(invalidDelegate);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void Validate_ValidRegisterSingleByFuncRegistration_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();
            Func<IUserRepository> validDelegate = () => new SqlUserRepository();
            container.RegisterSingle<IUserRepository>(validDelegate);

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Validate_InValidRegisterSingleByFuncRegistration_ThrowsExpectedExceptionMessage()
        {
            // Arrange
            string expectedMessage = "The registered delegate for type IUserRepository returned null";

            var container = ContainerFactory.New();
            Func<IUserRepository> invalidDelegate = () => null;
            container.RegisterSingle<IUserRepository>(invalidDelegate);

            try
            {
                // Act
                container.Verify();

                // Arrange
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void RegisterSingleByFunc_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterSingle<IUserRepository>(() => new SqlUserRepository());

            // Act
            Action action = () => container.RegisterSingle<IUserRepository>(() => new InMemoryUserRepository());

            // Assert
            AssertThat.Throws<InvalidOperationException>(action, "A certain type can only be registered once.");
        }

        [TestMethod]
        public void RegisterSingleByFunc_CalledAfterRegisterOnSameType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register<UserServiceBase>(() => new RealUserService(null));

            // Act
            Action action = () => container.RegisterSingle<UserServiceBase>(() => new FakeUserService(null));

            // Assert
            AssertThat.Throws<InvalidOperationException>(action, "A certain type can only be registered once.");
        }

        [TestMethod]
        public void RegisterSingleByFunc_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterSingle<IUserRepository>(() => new InMemoryUserRepository());
            container.GetInstance<IUserRepository>();

            // Act
            Action action = () => container.RegisterSingle<UserServiceBase>(() => new RealUserService(null));

            // Assert
            AssertThat.Throws<InvalidOperationException>(action, "The container should get locked after a call to GetInstance.");
        }

        [TestMethod]
        public void RegisterSingleByFunc_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            var repositories = container.GetAllInstances<IUserRepository>();

            // Calling count will iterate the collections. 
            // The container will only get locked when the first item is retrieved.
            var count = repositories.Count();

            // Act
            Action action = () => container.RegisterSingle<UserServiceBase>(() => new RealUserService(null));

            // Assert
            AssertThat.Throws<InvalidOperationException>(action, "The container should get locked after a call to GetAllInstances.");
        }

        [TestMethod]
        public void RegisterSingleByFunc_RegisteringDelegate_WillNotCallTheDelegate()
        {
            // Arrange
            int numberOfTimesDelegateWasCalled = 0;

            var container = ContainerFactory.New();

            // Act
            container.RegisterSingle<IUserRepository>(() =>
            {
                numberOfTimesDelegateWasCalled++;
                return new SqlUserRepository();
            });

            // Assert
            Assert.AreEqual(0, numberOfTimesDelegateWasCalled,
                "The RegisterSingle method should not call the delegate, because users may need objects " +
                "that are not yet registered. Users are allowed to register dependent objects in random order.");
        }

        [TestMethod]
        public void RegisterSingleByFunc_CallingGetInstanceMultipleTimes_WillOnlyCallDelegateOnce()
        {
            // Arrange
            const int ExpectedNumberOfCalles = 1;
            int actualNumberOfCalls = 0;

            var container = ContainerFactory.New();
            container.RegisterSingle<IUserRepository>(() =>
            {
                actualNumberOfCalls++;
                return new SqlUserRepository();
            });

            // Act
            container.GetInstance<IUserRepository>();
            container.GetInstance<IUserRepository>();
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(ExpectedNumberOfCalles, actualNumberOfCalls,
                "The RegisterSingle method should register the object in such a way that the delegate will " +
                "only get called once during the lifetime of the application. Not more.");
        }

        [TestMethod]
        public void GetInstance_ForConcreteUnregisteredTypeWithDependencyRegisteredWithRegisterSingleFunc_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // This registration will make the DelegateBuilder call the 
            // FuncSingletonInstanceProducer.BuildExpression method.
            container.RegisterSingle<IUserRepository>(() => new SqlUserRepository());

            // Act
            container.GetInstance<RealUserService>();
        }

        [TestMethod]
        public void GetInstance_ThrowingDelegateRegisteredUsingRegisterSingleByFuncOfRootType_ThrowsActivationExceptionWithExpectedInnerException()
        {
            // Arrange
            var expectedInnerException = new InvalidOperationException();

            var container = ContainerFactory.New();
            container.RegisterSingle<IUserRepository>(() => { throw expectedInnerException; });

            try
            {
                // Act
                container.GetInstance<IUserRepository>();

                // Assert
                Assert.Fail("The GetInstance method was expected to fail, because of the faulty registration.");
            }
            catch (ActivationException ex)
            {
                Assert.IsNotNull(ex.InnerException);
                Assert.IsTrue(object.ReferenceEquals(expectedInnerException, ex.InnerException),
                    "The exception thrown by the registered delegate is expected to be available in the " +
                    "InnerException property of the thrown ActivationException. The actual InnerException " +
                    "type is " + ex.InnerException.GetType().Name);
            }
        }

        [TestMethod]
        public void GetInstance_ThrowingDelegateRegisteredUsingRegisterSingleByFuncOfNonRootType_ThrowsActivationExceptionWithExpectedExceptionMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository>(() => { throw new Exception("Bla."); });

            try
            {
                // Act
                container.GetInstance<RealUserService>();

                // Assert
                Assert.Fail("The GetInstance method was expected to fail, because of the faulty registration.");
            }
            catch (ActivationException ex)
            {
                string expectedMessage = 
                    "The registered delegate for type IUserRepository threw an exception. Bla.";

                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_DelegateReturningNullRegisteredUsingRegisterSingleByFuncOfNonRootType_ThrowsActivationExceptionWithExpectedExceptionMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository>(() => null);

            try
            {
                // Act
                container.GetInstance<RealUserService>();

                // Assert
                Assert.Fail("The GetInstance method was expected to fail, because of the faulty registration.");
            }
            catch (ActivationException ex)
            {
                string expectedMessage = "The registered delegate for type IUserRepository returned null.";

                AssertThat.ExceptionMessageContains(expectedMessage, ex);
            }
        }

                [TestMethod]
        public void RegisterSingleByFuncNonGeneric_ValidArguments_GetInstanceAlwaysReturnsSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            Func<object> instanceCreator = () => new SqlUserRepository();

            // Act
            container.RegisterSingle(typeof(IUserRepository), instanceCreator);

            var instance1 = container.GetInstance<IUserRepository>();
            var instance2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(instance1, instance2, "RegisterSingle should register singleton.");
        }

        [TestMethod]
        public void RegisterSingleByFuncNonGeneric_NullServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type invalidServiceType = null;
            Func<object> validInstanceCreator = () => new SqlUserRepository();

            // Act
            Action action = () => container.RegisterSingle(invalidServiceType, validInstanceCreator);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterSingleByFuncNonGeneric_NullInstanceCreator_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type validServiceType = typeof(IUserRepository);
            Func<object> invalidInstanceCreator = null;

            // Act
            Action action = () => container.RegisterSingle(validServiceType, invalidInstanceCreator);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }
    }
}