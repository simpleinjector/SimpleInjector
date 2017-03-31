namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterConcreteTests
    {
        [TestMethod]
        public void Register_RegisteringAConcreteType_ReturnAnInstance()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register<IUserRepository>(() => new SqlUserRepository());

            // Act
            container.Register<RealUserService>();

            // Assert
            var service = container.GetInstance<RealUserService>();

            Assert.IsNotNull(service, "The container should not return null.");
        }

        [TestMethod]
        public void Register_RegisteringAConcreteType_AlwaysReturnsANewInstance()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register<IUserRepository>(() => new SqlUserRepository());

            // Act
            container.Register<RealUserService>();

            // Assert
            var s1 = container.GetInstance<RealUserService>();
            var s2 = container.GetInstance<RealUserService>();

            Assert.IsFalse(object.ReferenceEquals(s1, s2), "Always a new instance was expected to be returned.");
        }

        [TestMethod]
        public void Register_RegisteringANonConcreteType_ThrowsAnArgumentExceptionWithExpectedMessage()
        {
            // Arrange
            string expectedMessage = typeof(IUserRepository).Name + " is not a concrete type.";

            var container = ContainerFactory.New();

            try
            {
                // Act
                container.Register<IUserRepository>();

                Assert.Fail("The abstract type was not expected to be registered successfully.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.IsInstanceOfType(typeof(ArgumentException), ex, "No subtype was expected.");
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void Register_AbstractTypeWithSinglePublicConstructor_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage = @"
                The given type RegisterConcreteTests.AbstractTypeWithSinglePublicConstructor is not a concrete
                type. Please use one of the other overloads to register this type.
                ".TrimInside();

            var container = ContainerFactory.New();

            try
            {
                // Act
                container.Register<AbstractTypeWithSinglePublicConstructor>();

                Assert.Fail("The abstract type was not expected to be registered successfully.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.ExceptionMessageContains(expectedMessage, ex);
            }
        }

        [TestMethod]
        public void Register_RegisteringANonConcreteType_ThrowsAnArgumentExceptionWithExpectedParamName()
        {
            // Arrange
            string expectedParameterName = "TConcrete";

            var container = ContainerFactory.New();

            try
            {
                // Act
                container.Register<IUserRepository>();

                Assert.Fail("The abstract type was not expected to be registered successfully.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.ExceptionContainsParamName(expectedParameterName, ex);
            }
        }

        [TestMethod]
        public void RegisterWithLifestyle_RegisteringTransaction_ReturnsNewInstanceOnEachCall()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<RealTimeProvider>(Lifestyle.Transient);

            // Act
            var instance1 = container.GetInstance<RealTimeProvider>();
            var instance2 = container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreNotSame(instance1, instance2);
        }

        [TestMethod]
        public void RegisterWithLifestyle_RegisteringSingleton_ReturnsSameInstanceOnEveryCall()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<RealTimeProvider>(Lifestyle.Singleton);

            // Act
            var instance1 = container.GetInstance<RealTimeProvider>();
            var instance2 = container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        [TestMethod]
        public void Register_WithIncompleteSingletonRegistration_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // RealUserService is dependant on IUserRepository.
            container.Register<UserServiceBase>(() => container.GetInstance<RealUserService>());

            // Act
            // UserController is dependant on UserServiceBase. 
            // Registration should succeed even though IUserRepository is not registered yet.
            container.Register<UserController>();
        }
        
        [TestMethod]
        public void RegisterConcrete_ValidArguments_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.Register(typeof(SqlUserRepository));

            // Assert
            var instance = container.GetInstance(typeof(SqlUserRepository));

            AssertThat.IsInstanceOfType(typeof(SqlUserRepository), instance);
        }

        [TestMethod]
        public void RegisterConcrete_NullConcrete_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type invalidConcrete = null;

            // Act
            Action action = () => container.Register(invalidConcrete);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterConcrete_ConcreteIsNotAConstructableType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type invalidConcrete = typeof(ServiceImplWithTwoConstructors);

            // Act
            Action action = () => container.Register(invalidConcrete);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "it should have only one public constructor",
                action);
        }

        [TestMethod]
        public void RegisterConcrete_RegisteringADelegate_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            // Act
            Action action = () => container.Register<Func<object>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The given type Func<Object> is not a concrete type. " +
                "Please use one of the other overloads to register this type.", action);
        }
        
        public abstract class AbstractTypeWithSinglePublicConstructor
        {
            public AbstractTypeWithSinglePublicConstructor()
            {
            }
        }
        
        public sealed class ServiceImplWithTwoConstructors
        {
            public ServiceImplWithTwoConstructors()
            {
            }

            public ServiceImplWithTwoConstructors(IDisposable dependency)
            {
            }
        }
    }
}