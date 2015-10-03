namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterSingletonTests
    {
        [TestMethod]
        public void RegisterSingleton_WithValidType_ContainerAlwaysReturnsSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<IUserRepository, SqlUserRepository>();

            // Act
            var instance1 = container.GetInstance<IUserRepository>();
            var instance2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsNotNull(instance1, "GetInstance should never return null.");
            Assert.AreEqual(instance1, instance2, "Values should reference the same instance.");
        }

        [TestMethod]
        public void RegisterSingletonFunc_WithValidDelegate_ContainerAlwaysReturnsSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<IUserRepository>(() => new SqlUserRepository());

            // Act
            var instance1 = container.GetInstance<IUserRepository>();
            var instance2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsNotNull(instance1, "GetInstance should never return null.");
            Assert.AreEqual(instance1, instance2, "Values should reference the same instance.");
        }

        [TestMethod]
        public void RegisterSingletonNonGeneric_WithValidType_ContainerAlwaysReturnsSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton(typeof(IUserRepository), typeof(SqlUserRepository));

            // Act
            var instance1 = container.GetInstance<IUserRepository>();
            var instance2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsNotNull(instance1, "GetInstance should never return null.");
            Assert.AreEqual(instance1, instance2, "Values should reference the same instance.");
        }

        [TestMethod]
        public void RegisterSingletonNonGenericFunc_WithValidDelegate_ContainerAlwaysReturnsSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton(typeof(IUserRepository), () => new SqlUserRepository());

            // Act
            var instance1 = container.GetInstance<IUserRepository>();
            var instance2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsNotNull(instance1, "GetInstance should never return null.");
            Assert.AreEqual(instance1, instance2, "Values should reference the same instance.");
        }
    }
}