using System;
using System.Linq;

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

        // This test is important, because the PropertyInjector class splits the injection of a class with
        // many properties into multiple delegates.
        [TestMethod]
        public void InjectProperties_TypeWithManyMappableProperties_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, PluginImpl>();

            var instance = new ManyPropertiesService();

            // Act
            container.InjectProperties(instance);

            // Assert
            var uninjectedProperties =
                from property in instance.GetType().GetProperties()
                where property.GetValue(instance, null) == null
                select property.Name;

            Assert.IsFalse(uninjectedProperties.Any(), "All properties were expected to be injected. " +
                "Uninjected properties: " + string.Join(", ", uninjectedProperties.ToArray()));
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

        [TestMethod]
        public void InjectProperties_TypeRegisteredWithUnregisteredTypeResolution_InjectsThatType()
        {
            // Arrange
            var expectedInstance = new PluginImpl();

            var container = new Container();

            container.ResolveUnregisteredType += (sender, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IPlugin))
                {
                    e.Register(() => expectedInstance);
                }
            };

            var instance = new Service();

            // Act
            container.InjectProperties(instance);

            // Assert
            Assert.AreEqual(expectedInstance, instance.Plugin1,
                "Although IPlugin wasn't registered explicitly, it is expected to get resolved, because " +
                "of the registered unregistered type resolution event.");
        }
       
        public class Service
        {
            public ITimeProvider TimeProvider { get; set; }

            public IPlugin Plugin1 { get; set; }

            public IPlugin Plugin2 { get; set; }

            public IUserRepository ReadOnlyRepository { get; private set; }

            internal UserServiceBase InternalUserService { get; set; }
        }
        
        public class ManyPropertiesService
        {
            public IPlugin Plugin01 { get; set; }

            public IPlugin Plugin02 { get; set; }

            public IPlugin Plugin03 { get; set; }

            public IPlugin Plugin04 { get; set; }

            public IPlugin Plugin05 { get; set; }

            public IPlugin Plugin06 { get; set; }

            public IPlugin Plugin07 { get; set; }

            public IPlugin Plugin08 { get; set; }

            public IPlugin Plugin09 { get; set; }

            public IPlugin Plugin10 { get; set; }

            public IPlugin Plugin11 { get; set; }

            public IPlugin Plugin12 { get; set; }

            public IPlugin Plugin13 { get; set; }

            public IPlugin Plugin14 { get; set; }

            public IPlugin Plugin15 { get; set; }

            public IPlugin Plugin16 { get; set; }

            public IPlugin Plugin17 { get; set; }

            public IPlugin Plugin18 { get; set; }

            public IPlugin Plugin19 { get; set; }

            public IPlugin Plugin20 { get; set; }
        }
    }
}