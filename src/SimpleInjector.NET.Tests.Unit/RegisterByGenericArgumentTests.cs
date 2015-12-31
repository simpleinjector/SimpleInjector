namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    [TestClass]
    public class RegisterByGenericArgumentTests
    {
        [TestMethod]
        public void RegisterByGenericArgument_WithValidGenericArguments_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.Register<IUserRepository, SqlUserRepository>();
        }

        [TestMethod]
        public void GetInstance_OnRegisteredType_ReturnsInstanceOfExpectedType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

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

            container.Register<IUserRepository, InMemoryUserRepository>();

            // Act
            var instance1 = container.GetInstance<IUserRepository>();
            var instance2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreNotEqual(instance1, instance2, "Register<TService, TImplementation>() should " + 
                "return transient objects.");
        }

        [TestMethod]
        public void RegisterByGenericArgument_GenericArgumentOfInvalidType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Register<object, ConcreteTypeWithValueTypeConstructorArgument>();

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterByGenericArgument_GenericArgumentOfInvalidType_ThrowsExceptionWithExpectedParamName()
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
                AssertThat.ExceptionContainsParamName(expectedParamName, ex);
            }
        }

        [TestMethod]
        public void RegisterByGenericArgument_CalledAfterTheContainerWasLocked_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<IPlugin>();

            container.GetInstance<PluginManager>();

            // Act
            Action action = () => container.Register<IUserRepository, SqlUserRepository>();

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void RegisterGenericWithLifestyle_SuppliedAnImplementationWithTwoConstructors_ThrowsAnArgumentException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => 
                container.Register<ConcreteTypeWithMultiplePublicConstructors, ConcreteTypeWithMultiplePublicConstructors>(
                    Lifestyle.Transient);

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }
        
        [TestMethod]
        public void RegisterByGenericArgument_RegisteringCovarientType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICovariant<object>, CovariantImplementation<string>>();

            // Act
            var instance = container.GetInstance<ICovariant<object>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(CovariantImplementation<string>), instance);
        }
    }
}