namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterSingleByTypeNonGenericTests
    {
        [TestMethod]
        public void RegisterSingleByTypeNonGeneric_ValidRegistration_GetInstanceReturnsExpectedType()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterSingle(typeof(IUserRepository), typeof(SqlUserRepository));

            // Assert
            AssertThat.IsInstanceOfType(typeof(SqlUserRepository), container.GetInstance<IUserRepository>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleByTypeNonGeneric_NullServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type invalidServiceType = null;

            // Act
            container.RegisterSingle(invalidServiceType, typeof(SqlUserRepository));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSingleByTypeNonGeneric_NullImplementationType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type invalidImplementationType = null;

            // Act
            container.RegisterSingle(typeof(IUserRepository), invalidImplementationType);
        }

        [TestMethod]
        public void RegisterSingleByTypeNonGeneric_ValidRegistration_GetInstanceAlwaysReturnsSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            object impl = new SqlUserRepository();

            // Act
            container.RegisterSingle(typeof(IUserRepository), typeof(SqlUserRepository));

            var instance1 = container.GetInstance<IUserRepository>();
            var instance2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(instance1, instance2, "RegisterSingle should register singleton.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterSingleByTypeNonGeneric_InstanceThatDoesNotImplementServiceType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterSingle(typeof(IUserRepository), typeof(object));
        }

        [TestMethod]
        public void RegisterSingleByTypeNonGeneric_ImplementationIsServiceType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterSingle(typeof(SqlUserRepository), typeof(SqlUserRepository));
        }

        [TestMethod]
        public void RegisterSingleByTypeNonGeneric_ImplementationIsServiceType_ImplementationCanBeResolvedByItself()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle(typeof(SqlUserRepository), typeof(SqlUserRepository));

            // Act
            container.GetInstance<SqlUserRepository>();
        }

        [TestMethod]
        public void RegisterSingleByTypeNonGeneric_ImplementationIsServiceType_RegistersTheTypeAsSingleton()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle(typeof(SqlUserRepository), typeof(SqlUserRepository));

            // Act
            var instance1 = container.GetInstance<SqlUserRepository>();
            var instance2 = container.GetInstance<SqlUserRepository>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(instance1, instance2));
        }

        [TestMethod]
        public void RegisterSingleByTypeNonGeneric_OpenGenericServiceType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                // Act
                container.RegisterSingle(typeof(IDictionary<,>), typeof(Dictionary<,>));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains("The supplied type ") &&
                    ex.Message.Contains(" is an open generic type."), "Actual: " + ex.Message);
            }
        }

        [TestMethod]
        public void RegisterSingleByTypeNonGeneric_ValueTypeImplementation_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();
            try
            {
                // Act
                container.RegisterSingle(typeof(object), typeof(int));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains("The supplied type ") &&
                    ex.Message.Contains("is not a reference type. Only reference types are supported."),
                    "Actual: " + ex.Message);
            }
        }
    }
}