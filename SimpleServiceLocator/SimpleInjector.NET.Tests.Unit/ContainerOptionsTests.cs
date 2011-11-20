namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ContainerOptionsTests
    {
        [TestMethod]
        public void AllowOverridingRegistrations_WhenNotSet_IsFalse()
        {
            // Arrange
            var options = new ContainerOptions();

            // Assert
            Assert.IsFalse(options.AllowOverridingRegistrations,
                "The default value must be false, because this is the behavior users will expect.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AllowOverridingRegistrations_False_ContainerDoesNotAllowOverringRegistrations()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

            container.Register<IUserRepository, SqlUserRepository>();

            // Act
            container.Register<IUserRepository, InMemoryUserRepository>();
        }
        
        [TestMethod]
        public void AllowOverridingRegistrations_False_ContainerThrowsExpectedExceptionMessage()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

            container.Register<IUserRepository, SqlUserRepository>();

            try
            {
                // Act
                container.Register<IUserRepository, InMemoryUserRepository>();
            }
            catch (InvalidOperationException ex)
            {
                // Assert
                Assert.IsTrue(ex.Message.Contains("ContainerOptions"), "Actual: " + ex);
                Assert.IsTrue(ex.Message.Contains("AllowOverridingRegistrations"), "Actual: " + ex);
            }
        }

        [TestMethod]
        public void AllowOverridingRegistrations_True_ContainerDoesNotAllowOverringRegistrations()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = true
            });

            container.Register<IUserRepository, SqlUserRepository>();

            // Act
            container.Register<IUserRepository, InMemoryUserRepository>();

            // Assert
            Assert.IsInstanceOfType(container.GetInstance<IUserRepository>(), typeof(InMemoryUserRepository));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AllowOverridingRegistrations_False_ContainerDoesNotAllowOverringCollections()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

            container.RegisterAll<IUserRepository>(new SqlUserRepository());

            // Act
            container.RegisterAll<IUserRepository>(new InMemoryUserRepository());
        }

        [TestMethod]
        public void AllowOverridingRegistrations_True_ContainerDoesNotAllowOverringCollections()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = true
            });

            container.RegisterAll<IUserRepository>(new SqlUserRepository());

            // Act
            container.RegisterAll<IUserRepository>(new InMemoryUserRepository());

            // Assert
            var instance = container.GetAllInstances<IUserRepository>().Single();
            Assert.IsInstanceOfType(instance, typeof(InMemoryUserRepository));
        }
    }
}
