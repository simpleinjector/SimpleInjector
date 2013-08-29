namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    [TestClass]
    public class RegisterByFuncTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterByFunc_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register<UserServiceBase>(() => new RealUserService(null));

            // Act
            container.Register<UserServiceBase>(() => new FakeUserService(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void RegisterByFunc_CalledAfterRegisterSingleOnSameType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterSingle<IUserRepository>(new SqlUserRepository());

            // Act
            container.Register<IUserRepository>(() => new InMemoryUserRepository());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterByFunc_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
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
            var container = ContainerFactory.New();
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
            var container = ContainerFactory.New();

            Func<IUserRepository> invalidInstanceCreator = null;

            // Act
            container.Register<IUserRepository>(invalidInstanceCreator);
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstance_RegisteredWithFuncReturningNull_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register<IUserRepository>(() => null);

            // Act
            container.GetInstance<IUserRepository>();
        }
        
        [TestMethod]
        public void GetInstance_SubTypeRegisteredWithFuncReturningNull_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register<IUserRepository>(() => null);

            try
            {
                // Act
                container.GetInstance<RealUserService>();

                // Assert
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
        public void GetInstance_TypeRegisteredWithFuncReturningNullWhilePropertiesBeingInjected_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Ensure properties of type ITimeProvider will be injected.
            container.Options.PropertySelectionBehavior = new InjectPropertyOfType<ITimeProvider>();

            container.RegisterSingle<ITimeProvider, RealTimeProvider>();

            // MyPlugin contains a TimeProvider property of type ITimeProvider.
            container.Register<PluginWithDependencyOfType<ITimeProvider>>(() => null);

            try
            {
                // Act
                container.GetInstance<PluginWithDependencyOfType<ITimeProvider>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(
                    "The registered delegate for type PluginWithDependencyOfType<ITimeProvider> returned null."),
                    "Actual: " + ex.Message);
            }
        }

        private sealed class InjectPropertyOfType<T> : IPropertySelectionBehavior
        {
            public bool SelectProperty(Type serviceType, PropertyInfo propertyInfo)
            {
                return propertyInfo.PropertyType == typeof(T);
            }
        }

        [TestMethod]
        public void GetInstance_ForConcreteUnregisteredTypeWithDependencyRegisteredWithRegister_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

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

            var container = ContainerFactory.New();
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
        
        [TestMethod]
        public void RegisterByFunc_ValidArguments_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type validServiceType = typeof(IUserRepository);
            Func<object> instanceCreator = () => new SqlUserRepository();

            // Act
            container.Register(validServiceType, instanceCreator);

            // Assert
            var instance = container.GetInstance(validServiceType);

            Assert.IsInstanceOfType(instance, typeof(SqlUserRepository));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterByFunc_NullInstanceCreator_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type validServiceType = typeof(IUserRepository);
            Func<object> invalidInstanceCreator = null;

            // Act
            container.Register(validServiceType, invalidInstanceCreator);
        }
    }
}