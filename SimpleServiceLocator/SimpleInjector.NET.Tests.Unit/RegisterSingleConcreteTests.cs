using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.Tests.Unit
{
    [TestClass]
    public class RegisterSingleConcreteTests
    {
        [TestMethod]
        public void RegisterSingle_RegisteringAConcreteType_ReturnAnInstance()
        {
            // Arrange
            var container = new Container();
            container.Register<IUserRepository>(() => new SqlUserRepository());

            // Act
            container.RegisterSingle<RealUserService>();

            // Assert
            var userService = container.GetInstance<RealUserService>();

            Assert.IsNotNull(userService, "The container should not return null.");
        }

        [TestMethod]
        public void RegisterSingle_RegisteringAConcreteType_AlwaysReturnsSameInstance()
        {
            // Arrange
            var container = new Container();
            container.Register<IUserRepository>(() => new SqlUserRepository());

            // Act
            container.RegisterSingle<RealUserService>();

            // Assert
            var s1 = container.GetInstance<RealUserService>();
            var s2 = container.GetInstance<RealUserService>();

            Assert.IsTrue(Object.ReferenceEquals(s1, s2), "Always the same instance was expected to be returned.");
        }

        [TestMethod]
        public void RegisterSingle_RegisteringANonConcreteType_ThrowsAnArgumentExceptionWithExpectedMessage()
        {
            // Arrange
            string expectedMessage = typeof(IUserRepository).Name + " is not a concrete type.";

            var container = new Container();

            try
            {
                // Act
                container.RegisterSingle<IUserRepository>();

                Assert.Fail("The abstract type was not expected to be registered successfully.");
            }
            catch (ArgumentException ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException), "No subtype was expected.");
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void RegisterSingle_RegisteringANonConcreteType_ThrowsAnArgumentExceptionWithExpectedParamName()
        {
            // Arrange
            string expectedParameterName = "TConcrete";

            var container = new Container();

            try
            {
                // Act
                container.RegisterSingle<IUserRepository>();

                Assert.Fail("The abstract type was not expected to be registered successfully.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.ExceptionContainsParamName(ex, expectedParameterName);
            }
        }

        [TestMethod]
        public void RegisterSingle_WithIncompleteSingletonRegistration_Succeeds()
        {
            // Arrange
            var container = new Container();

            // UserServiceBase dependants on IUserRepository.
            container.RegisterSingle<UserServiceBase>(() => container.GetInstance<RealUserService>());

            // Act
            // UserController dependants on UserServiceBase. 
            // Registration should succeed even though UserServiceBase is not registered yet.
            container.RegisterSingle<UserController>();
        }
    }
}