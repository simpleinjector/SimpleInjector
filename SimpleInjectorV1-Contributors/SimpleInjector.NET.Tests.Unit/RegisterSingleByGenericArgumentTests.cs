namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NUnit.Framework;

    [TestFixture]
    public class RegisterSingleByGenericArgumentTests
    {
        [Test]
        public void RegisterSingleByGenericArgument_WithValidGenericArguments_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterSingle<IUserRepository, SqlUserRepository>();
        }

        [Test]
        public void GetInstance_OnRegisteredType_ReturnsInstanceOfExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<IUserRepository, SqlUserRepository>();

            // Act
            var instance = container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsInstanceOf<SqlUserRepository>(instance);
        }

        [Test]
        public void GetInstance_OnRegisteredType_ReturnsANewInstanceOnEachCall()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<IUserRepository, InMemoryUserRepository>();

            // Act
            var instance1 = container.GetInstance<IUserRepository>();
            var instance2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(instance1, instance2, "RegisterSingle<TService, TImplementation>() should " +
                "return singleton objects.");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterSingleByGenericArgument_GenericArgumentOfInvalidType_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Register<object, ConcreteTypeWithValueTypeConstructorArgument>();
        }

        [Test]
        public void RegisterSingleByGenericArgument_GenericArgumentOfInvalidType_ThrowsExceptionWithExpectedParam()
        {
            // Arrange
            string expectedParamName = "TImplementation";

            var container = new Container();

            try
            {
                // Act
                container.Register<object, ConcreteTypeWithValueTypeConstructorArgument>();

                // Assert
                Assert.Fail("Registration of ConcreteTypeWithValueTypeConstructorArgument should fail.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.ExceptionContainsParamName(ex, expectedParamName);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterSingleByGenericArgument_CalledAfterTheContainerWasLocked_ThrowsException()
        {
            // Arrange
            var container = new Container();

            container.GetInstance<PluginManager>();

            // Act
            container.RegisterSingle<IUserRepository, SqlUserRepository>();
        }
    }
}