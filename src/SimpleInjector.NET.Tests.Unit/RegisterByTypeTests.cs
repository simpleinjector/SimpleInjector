namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterByTypeTests
    {
        [TestMethod]
        public void RegisterByType_ValidArguments_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type validServiceType = typeof(IUserRepository);
            Type validImplementation = typeof(SqlUserRepository);

            // Act
            container.Register(validServiceType, validImplementation);

            // Assert
            var instance = container.GetInstance(validServiceType);

            AssertThat.IsInstanceOfType(validImplementation, instance);
        }

        [TestMethod]
        public void RegisterByType_NullServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type invalidServiceType = null;
            Type validImplementation = typeof(SqlUserRepository);

            // Act
            Action action = () => container.Register(invalidServiceType, validImplementation);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterByType_NullImplementation_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type validServiceType = typeof(IUserRepository);
            Type invalidImplementation = null;

            // Act
            Action action = () => container.Register(validServiceType, invalidImplementation);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterByType_ServiceTypeAndImplementationSameType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type implementation = typeof(SqlUserRepository);

            // Act
            container.Register(implementation, implementation);
        }

        [TestMethod]
        public void RegisterByType_ImplementationIsServiceType_ImplementationCanBeResolvedByItself()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(SqlUserRepository), typeof(SqlUserRepository));

            // Act
            container.GetInstance<SqlUserRepository>();
        }

        [TestMethod]
        public void RegisterByType_ImplementationIsServiceType_RegistersTheTypeAsTransient()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(SqlUserRepository), typeof(SqlUserRepository));

            // Act
            var instance1 = container.GetInstance<SqlUserRepository>();
            var instance2 = container.GetInstance<SqlUserRepository>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(instance1, instance2));
        }

        [TestMethod]
        public void RegisterByType_RegisteringCovarientType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(ICovariant<object>), typeof(CovariantImplementation<string>));

            // Act
            var instance = container.GetInstance<ICovariant<object>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(CovariantImplementation<string>), instance);
        }
    }
}