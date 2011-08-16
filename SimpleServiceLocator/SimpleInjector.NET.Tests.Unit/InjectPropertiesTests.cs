using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.Tests.Unit
{
    [TestClass]
    public class InjectPropertiesTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InjectProperties_NullInstance_ThrowsExpectedException()
        {
            // Arrange
            Container container = new Container();

            Service instance = null;

            // Act
            container.InjectProperties(instance);
        }

        [TestMethod]
        public void InjectProperties_WithEmptyContainer_DoesNotInjectAnything()
        {
            // Arrange
            var container = new Container();

            var instance = new Service();

            // Act
            container.InjectProperties(instance);

            // Assert
            Assert.IsNull(instance.TimeProvider, "TimeProvider property expected to be null.");
            Assert.IsNull(instance.Plugin1, "Plugin1 property expected to be null.");
            Assert.IsNull(instance.Plugin2, "Plugin2 property expected to be null.");
            Assert.IsNull(instance.ReadOnlyRepository, "Repository property expected to be null.");
            Assert.IsNull(instance.InternalUserService, "UserService property expected to be null.");
        }

        [TestMethod]
        public void InjectProperties_ContainerWithPartialRegistration_DoesInjectSingleProperty()
        {
            // Arrange
            var container = new Container();

            container.Register<ITimeProvider, RealTimeProvider>();

            var instance = new Service();

            // Act
            container.InjectProperties(instance);

            // Assert
            Assert.IsNotNull(instance.TimeProvider, "TimeProvider property expected to be set.");
        }

        [TestMethod]
        public void InjectProperties_TypeRegisteredThatMapsToMultipleProperties_InjectsDoesInjectBothProperties()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, PluginImpl>();

            var instance = new Service();

            // Act
            container.InjectProperties(instance);

            // Assert
            Assert.IsNotNull(instance.Plugin1, "Plugin1 property expected to be set.");
            Assert.IsNotNull(instance.Plugin2, "Plugin2 property expected to be set.");
        }

        [TestMethod]
        public void InjectProperties_TypeRegisteredForReadOnlyProperty_DoesNotInjectProperty()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, SqlUserRepository>();

            var instance = new Service();

            // Act
            container.InjectProperties(instance);

            // Assert
            Assert.IsNull(instance.ReadOnlyRepository, "Repository property expected to be null.");
        }

        [TestMethod]
        public void InjectProperties_TypeRegisteredForInternalProperty_DoesNotInjectProperty()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, SqlUserRepository>();

            var instance = new Service();

            // Act
            container.InjectProperties(instance);

            // Assert
            Assert.IsNull(instance.InternalUserService, "UserService property expected to be null.");
        }

        public class Service
        {
            public ITimeProvider TimeProvider { get; set; }

            public IPlugin Plugin1 { get; set; }

            public IPlugin Plugin2 { get; set; }

            public IUserRepository ReadOnlyRepository { get; private set; }

            internal UserServiceBase InternalUserService { get; set; }
        }
    }
}