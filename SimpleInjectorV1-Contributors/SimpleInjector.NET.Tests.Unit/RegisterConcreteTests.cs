namespace SimpleInjector.Tests.Unit
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class RegisterConcreteTests
    {
        [Test]
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

        [Test]
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

            Assert.IsFalse(object.ReferenceEquals(s1, s2), "Always a new instance was expected to be returned.");
        }

        [Test]
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
                Assert.IsInstanceOf<ArgumentException>(ex,  "No subtype was expected.");
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [Test]
        public void Register_AbstractTypeWithSinglePublicConstructor_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage = @"
                The given type RegisterConcreteTests+AbstractTypeWithSinglePublicConstructor is not a concrete
                type. Please use one of the other overloads to register this type.
                ".TrimInside();

            var container = new Container();

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

        [Test]
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

        [Test]
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

        public abstract class AbstractTypeWithSinglePublicConstructor
        {
            public AbstractTypeWithSinglePublicConstructor()
            {
            }
        }
    }
}
