using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.Tests.Unit
{
    [TestClass]
    public class RegisterConcreteTests
    {
        [TestMethod]
        public void Register_RegisteringAConcreteType_ReturnAnInstance()
        {
            // Arrange
            var container = new Container();
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
            var container = new Container();
            container.Register<IUserRepository>(() => new SqlUserRepository());

            // Act
            container.Register<RealUserService>();

            // Assert
            var s1 = container.GetInstance<RealUserService>();
            var s2 = container.GetInstance<RealUserService>();

            Assert.IsFalse(Object.ReferenceEquals(s1, s2), "Always a new instance was expected to be returned.");
        }

        [TestMethod]
        public void Register_RegisteringANonConcreteType_ThrowsAnArgumentExceptionWithExpectedMessage()
        {
            // Arrange
            string expectedMessage = typeof(IUserRepository).Name + " is not a concrete type.";

            var container = new Container();

            try
            {
                // Act
                container.Register<IUserRepository>();

                Assert.Fail("The abstract type was not expected to be registered successfully.");
            }
            catch (ArgumentException ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException), "No subtype was expected.");
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void Register_RegisteringANonConcreteType_ThrowsAnArgumentExceptionWithExpectedParamName()
        {
            // Arrange
            string expectedParameterName = "TConcrete";

            var container = new Container();

            try
            {
                // Act
                container.Register<IUserRepository>();

                Assert.Fail("The abstract type was not expected to be registered successfully.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.ExceptionContainsParamName(ex, expectedParameterName);
            }
        }

        [TestMethod]
        public void Register_WithIncompleteSingletonRegistration_Succeeds()
        {
            // Arrange
            var container = new Container();

            // RealUserService is dependant on IUserRepository.
            container.Register<UserServiceBase>(() => container.GetInstance<RealUserService>());

            // Act
            // UserController is dependant on UserServiceBase. 
            // Registration should succeed even though IUserRepository is not registered yet.
            container.Register<UserController>();
        }
    }
}
