namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterSingleByGenericArgumentTests
    {
        [TestMethod]
        public void RegisterSingleByGenericArgument_WithValidGenericArguments_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterSingle<IUserRepository, SqlUserRepository>();
        }

        [TestMethod]
        public void GetInstance_OnRegisteredType_ReturnsInstanceOfExpectedType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository, SqlUserRepository>();

            // Act
            var instance = container.GetInstance<IUserRepository>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(SqlUserRepository), instance);
        }

        [TestMethod]
        public void GetInstance_OnRegisteredType_ReturnsANewInstanceOnEachCall()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<IUserRepository, InMemoryUserRepository>();

            // Act
            var instance1 = container.GetInstance<IUserRepository>();
            var instance2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(instance1, instance2, "RegisterSingle<TService, TImplementation>() should " +
                "return singleton objects.");
        }

        [TestMethod]
        public void RegisterSingleByGenericArgument_GenericArgumentOfInvalidType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Register<object, ConcreteTypeWithValueTypeConstructorArgument>();

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterSingleByGenericArgument_GenericArgumentOfInvalidType_ThrowsExceptionWithExpectedParam()
        {
            // Arrange
            string expectedParamName = "TImplementation";

            var container = ContainerFactory.New();

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

        [TestMethod]
        public void RegisterSingleByGenericArgument_CalledAfterTheContainerWasLocked_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.GetInstance<PluginManager>();

            // Act
            Action action = () => container.RegisterSingle<IUserRepository, SqlUserRepository>();

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }
    }
}