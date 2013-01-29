namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    [TestFixture]
    public class RegisterSingleByFuncTests
    {
        [Test]
        public void RegisterSingleByFunc_WithValidArgument_Succeeds()
        {
            // Arrange
            var container = new Container();
            Func<IUserRepository> validDelegate = () => new SqlUserRepository();

            // Act
            container.RegisterSingle<IUserRepository>(validDelegate);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleByFunc_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new Container();
            Func<IUserRepository> invalidDelegate = null;

            // Act
            container.RegisterSingle<IUserRepository>(invalidDelegate);
        }

        [Test]
        public void Validate_ValidRegisterSingleByFuncRegistration_Succeeds()
        {
            // Arrange
            var container = new Container();
            Func<IUserRepository> validDelegate = () => new SqlUserRepository();
            container.RegisterSingle<IUserRepository>(validDelegate);

            // Act
            container.Verify();
        }

        [Test]
        public void Validate_InValidRegisterSingleByFuncRegistration_ThrowsExpectedExceptionMessage()
        {
            // Arrange
            string expectedMessage = "The registered delegate for type IUserRepository returned null";

            var container = new Container();
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

        [Test]
        [ExpectedException(typeof(InvalidOperationException), UserMessage = "A certain type can only be registered once.")]
        public void RegisterSingleByFunc_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new Container();
            container.RegisterSingle<IUserRepository>(() => new SqlUserRepository());

            // Act
            container.RegisterSingle<IUserRepository>(() => new InMemoryUserRepository());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), UserMessage = "A certain type can only be registered once.")]
        public void RegisterSingleByFunc_CalledAfterRegisterOnSameType_ThrowsException()
        {
            // Arrange
            var container = new Container();
            container.Register<UserServiceBase>(() => new RealUserService(null));

            // Act
            container.RegisterSingle<UserServiceBase>(() => new FakeUserService(null));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), UserMessage = "The container should get locked after a call to GetInstance.")]
        public void RegisterSingleByFunc_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new Container();
            container.RegisterSingle<IUserRepository>(() => new InMemoryUserRepository());
            container.GetInstance<IUserRepository>();

            // Act
            container.RegisterSingle<UserServiceBase>(() => new RealUserService(null));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), UserMessage = "The container should get locked after a call to GetAllInstances.")]
        public void RegisterSingleByFunc_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new Container();
            var repositories = container.GetAllInstances<IUserRepository>();

            // Calling count will iterate the collections. 
            // The container will only get locked when the first item is retrieved.
            var count = repositories.Count();

            // Act
            container.RegisterSingle<UserServiceBase>(() => new RealUserService(null));
        }

        [Test]
        public void RegisterSingleByFunc_RegisteringDelegate_WillNotCallTheDelegate()
        {
            // Arrange
            int numberOfTimesDelegateWasCalled = 0;

            var container = new Container();

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

        [Test]
        public void RegisterSingleByFunc_CallingGetInstanceMultipleTimes_WillOnlyCallDelegateOnce()
        {
            // Arrange
            const int ExpectedNumberOfCalles = 1;
            int actualNumberOfCalls = 0;

            var container = new Container();
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

        [Test]
        public void GetInstance_ForConcreteUnregisteredTypeWithDependencyRegisteredWithRegisterSingleFunc_Succeeds()
        {
            // Arrange
            var container = new Container();

            // This registration will make the DelegateBuilder call the 
            // FuncSingletonInstanceProducer.BuildExpression method.
            container.RegisterSingle<IUserRepository>(() => new SqlUserRepository());

            // Act
            container.GetInstance<RealUserService>();
        }

        [Test]
        public void GetInstance_ThrowingDelegateRegisteredUsingRegisterSingleByFuncOfRootType_ThrowsActivationExceptionWithExpectedInnerException()
        {
            // Arrange
            var expectedInnerException = new InvalidOperationException();

            var container = new Container();
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

        [Test]
        public void GetInstance_ThrowingDelegateRegisteredUsingRegisterSingleByFuncOfNonRootType_ThrowsActivationExceptionWithExpectedExceptionMessage()
        {
            // Arrange
            var container = new Container();

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

        [Test]
        public void GetInstance_DelegateReturningNullRegisteredUsingRegisterSingleByFuncOfNonRootType_ThrowsActivationExceptionWithExpectedExceptionMessage()
        {
            // Arrange
            var container = new Container();

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

                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }
    }
}