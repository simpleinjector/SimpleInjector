namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterByFuncTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterByFunc_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new Container();
            container.Register<UserServiceBase>(() => new RealUserService(null));

            // Act
            container.Register<UserServiceBase>(() => new FakeUserService(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterByFunc_CalledAfterRegisterSingleOnSameType_ThrowsException()
        {
            // Arrange
            var container = new Container();
            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // Act
            container.Register<IUserRepository>(() => new InMemoryUserRepository());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterByFunc_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new Container();
            container.RegisterSingle<IUserRepository>(new InMemoryUserRepository());
            container.GetInstance<IUserRepository>();

            // Act
            container.Register<UserServiceBase>(() => new RealUserService(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetAllInstances.")]
        public void RegisterByFunc_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new Container();
            var repositories = container.GetAllInstances<IUserRepository>();

            // Only during iterating the collection, will the underlying container be called. This is a
            // Common Service Locator thing.
            var count = repositories.Count();

            // Act
            container.Register<UserServiceBase>(() => new RealUserService(null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterByFunc_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new Container();

            Func<IUserRepository> invalidInstanceCreator = null;

            // Act
            container.Register<IUserRepository>(invalidInstanceCreator);
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstance_RegisteredWithFuncReturningNull_ThrowsException()
        {
            // Arrange
            var container = new Container();
            container.Register<IUserRepository>(() => null);

            // Act
            container.GetInstance<IUserRepository>();
        }
        
        [TestMethod]
        public void GetInstance_SubTypeRegisteredWithFuncReturningNull_ThrowsException()
        {
            // Arrange
            var container = new Container();
            container.Register<IUserRepository>(() => null);

            try
            {
                // Act
                container.GetInstance<RealUserService>();

                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(
                    "The registered delegate for type IUserRepository returned null."),
                    "Actual: " + ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_ForConcreteUnregisteredTypeWithDependencyRegisteredWithRegister_Succeeds()
        {
            // Arrange
            var container = new Container();

            // This registration will make the DelegateBuilder call the 
            // SingletonInstanceProducer.BuildExpression method.
            container.Register<IUserRepository>(() => new SqlUserRepository());

            // Act
            container.GetInstance<RealUserService>();
        }

        [TestMethod]
        public void GetInstance_ThrowingDelegateRegisteredUsingRegisterByFunc_ThrowsActivationExceptionWithExpectedInnerException()
        {
            // Arrange
            var expectedInnerException = new InvalidOperationException();

            var container = new Container();
            container.Register<IUserRepository>(() => { throw expectedInnerException; });

            try
            {
                // Act
                container.GetInstance<IUserRepository>();

                // Assert
                Assert.Fail("The GetInstance method was expected to fail, because of the faulty registration.");
            }
            catch (ActivationException ex)
            {
                Assert.AreEqual(expectedInnerException, ex.InnerException,
                    "The exception thrown by the registered delegate is expected to be wrapped in the " +
                    "thrown ActivationException.");
            }
        }
    }
}