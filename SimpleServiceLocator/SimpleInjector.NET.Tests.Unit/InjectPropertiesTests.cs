using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.Tests.Unit
{
    /// <content>Tests for injecting properties.</content>
    [TestClass]
    public partial class InjectPropertiesTests
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
            Assert.IsNull(instance.TimeProvider, "TimeProvider property was expected to be null.");
            Assert.IsNull(instance.Plugin1, "Plugin1 property was expected to be null.");
            Assert.IsNull(instance.Plugin2, "Plugin2 property was expected to be null.");
            Assert.IsNull(instance.ReadOnlyRepository, "Repository property was expected to be null.");
            Assert.IsNull(instance.InternalUserService, "UserService property was expected to be null.");
        }

        [TestMethod]
        public void InjectProperties_ContainerWithRegistrationForASingleProperty_DoesInjectSingleProperty()
        {
            // Arrange
            var container = new Container();

            container.Register<ITimeProvider, RealTimeProvider>();

            var instance = new Service();

            // Act
            container.InjectProperties(instance);

            // Assert
            Assert.IsNotNull(instance.TimeProvider, "TimeProvider property was expected to be set.");
        }

        [TestMethod]
        public void InjectProperties_TypeThatMapsToMultipleProperties_InjectsDoesInjectBothProperties()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, PluginImpl>();

            var instance = new Service();

            // Act
            container.InjectProperties(instance);

            // Assert
            Assert.IsNotNull(instance.Plugin1, "Plugin1 property was expected to be set.");
            Assert.IsNotNull(instance.Plugin2, "Plugin2 property was expected to be set.");
        }

        [TestMethod]
        public void InjectProperties_TypeWithThreeMappableProperties_InjectsDoesInjectAllThreeProperties()
        {
            // Arrange
            var container = new Container();

            container.Register<ITimeProvider, RealTimeProvider>();
            container.Register<IPlugin, PluginImpl>();

            var instance = new Service();

            // Act
            container.InjectProperties(instance);

            // Assert
            Assert.IsNotNull(instance.Plugin1, "Plugin1 property was expected to be set.");
            Assert.IsNotNull(instance.Plugin2, "Plugin2 property was expected to be set.");
            Assert.IsNotNull(instance.TimeProvider, "TimeProvider property was expected to be set.");
        }

        [TestMethod]
        public void InjectProperties_TypeWithManyMappableProperties_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, PluginImpl>();

            var instance = new NinePropertiesService();

            // Act
            container.InjectProperties(instance);

            // Assert
            Assert.IsNotNull(instance.Plugin1, "Plugin1 property was expected to be set.");
            Assert.IsNotNull(instance.Plugin2, "Plugin2 property was expected to be set.");
            Assert.IsNotNull(instance.Plugin3, "Plugin3 property was expected to be set.");
            Assert.IsNotNull(instance.Plugin4, "Plugin4 property was expected to be set.");
            Assert.IsNotNull(instance.Plugin5, "Plugin5 property was expected to be set.");
            Assert.IsNotNull(instance.Plugin6, "Plugin6 property was expected to be set.");
            Assert.IsNotNull(instance.Plugin7, "Plugin7 property was expected to be set.");
            Assert.IsNotNull(instance.Plugin8, "Plugin8 property was expected to be set.");
            Assert.IsNotNull(instance.Plugin9, "Plugin9 property was expected to be set.");
        }

        [TestMethod]
        public void InjectProperties_TypeForReadOnlyProperty_DoesNotInjectProperty()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, SqlUserRepository>();

            var instance = new Service();

            // Act
            container.InjectProperties(instance);

            // Assert
            Assert.IsNull(instance.ReadOnlyRepository, "Repository property was expected to be null.");
        }

        [TestMethod]
        public void InjectProperties_TypeWithInternalProperty_DoesNotInjectThatInternalProperty()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, SqlUserRepository>();

            var instance = new Service();

            // Act
            container.InjectProperties(instance);

            // Assert
            Assert.IsNull(instance.InternalUserService, "UserService property was expected to be null.");
        }
       
        public class Service
        {
            public ITimeProvider TimeProvider { get; set; }

            public IPlugin Plugin1 { get; set; }

            public IPlugin Plugin2 { get; set; }

            public IUserRepository ReadOnlyRepository { get; private set; }

            internal UserServiceBase InternalUserService { get; set; }
        }

        public class NinePropertiesService
        {
            public IPlugin Plugin1 { get; set; }

            public IPlugin Plugin2 { get; set; }

            public IPlugin Plugin3 { get; set; }

            public IPlugin Plugin4 { get; set; }

            public IPlugin Plugin5 { get; set; }

            public IPlugin Plugin6 { get; set; }

            public IPlugin Plugin7 { get; set; }

            public IPlugin Plugin8 { get; set; }

            public IPlugin Plugin9 { get; set; }
        }
    }
}