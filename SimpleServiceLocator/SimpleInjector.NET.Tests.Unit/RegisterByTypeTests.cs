namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterByTypeTests
    {
        [TestMethod]
        public void RegisterByType_ValidArguments_Succeeds()
        {
            // Arrange
            var container = new Container();

            Type validServiceType = typeof(IUserRepository);
            Type validImplementation = typeof(SqlUserRepository);

            // Act
            container.Register(validServiceType, validImplementation);

            // Assert
            var instance = container.GetInstance(validServiceType);

            Assert.IsInstanceOfType(instance, validImplementation);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterByType_NullServiceType_ThrowsException()
        {
            // Arrange
            var container = new Container();

            Type invalidServiceType = null;
            Type validImplementation = typeof(SqlUserRepository);

            // Act
            container.Register(invalidServiceType, validImplementation);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterByType_NullImplementation_ThrowsException()
        {
            // Arrange
            var container = new Container();

            Type validServiceType = typeof(IUserRepository);
            Type invalidImplementation = null;

            // Act
            container.Register(validServiceType, invalidImplementation);
        }

        [TestMethod]
        public void RegisterByType_ServiceTypeAndImplementationSameType_Succeeds()
        {
            // Arrange
            var container = new Container();

            Type implementation = typeof(SqlUserRepository);

            // Act
            container.Register(implementation, implementation);
        }

        [TestMethod]
        public void RegisterByType_ImplementationIsServiceType_ImplementationCanBeResolvedByItself()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(SqlUserRepository), typeof(SqlUserRepository));

            // Act
            container.GetInstance<SqlUserRepository>();
        }

        [TestMethod]
        public void RegisterByType_ImplementationIsServiceType_RegistersTheTypeAsTransient()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(SqlUserRepository), typeof(SqlUserRepository));

            // Act
            var instance1 = container.GetInstance<SqlUserRepository>();
            var instance2 = container.GetInstance<SqlUserRepository>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(instance1, instance2));
        }
    }
}