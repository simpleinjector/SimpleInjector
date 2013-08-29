namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    [TestClass]
    public class AdvancedExtensionsTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsLocked_WithNullArgument_ThrowsException()
        {
            // Act
            AdvancedExtensions.IsLocked(null);
        }

        [TestMethod]
        public void GetInitializer_NoInitializerRegisteredForRequestedType_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var initializer = AdvancedExtensions.GetInitializer<IDisposable>(container);

            // Assert
            Assert.IsNull(initializer);
        }

        [TestMethod]
        public void GetInitializer_InitializerRegisteredForRequestedType_ReturnsADelegate()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterInitializer<IDisposable>(d => { });

            // Act
            var initializer = AdvancedExtensions.GetInitializer<IDisposable>(container);

            // Assert
            Assert.IsNotNull(initializer);
        }

        [TestMethod]
        public void GetInitializer_CallingTheReturnedDelegate_CallsTheRegisteredDelegate()
        {
            // Arrange
            bool called = false;

            var container = ContainerFactory.New();

            container.RegisterInitializer<IDisposable>(d => { called = true; });

            // Act
            var initializer = AdvancedExtensions.GetInitializer<IDisposable>(container);

            initializer(null);

            // Assert
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void GetInitializer_CallingTheReturnedDelegateWithTwoDelegatesRegistered_CallsTheRegisteredDelegates()
        {
            // Arrange
            bool called1 = false;
            bool called2 = false;

            var container = ContainerFactory.New();

            container.RegisterInitializer<IDisposable>(d => { called1 = true; });
            container.RegisterInitializer<IDisposable>(d => { called2 = true; });

            // Act
            var initializer = AdvancedExtensions.GetInitializer<IDisposable>(container);

            initializer(null);

            // Assert
            Assert.IsTrue(called1);
            Assert.IsTrue(called2);
        }

        [TestMethod]
        public void GetInitializer_CallingTheReturnedDelegate_CallsTheDelegateWithTheExpectedInstance()
        {
            // Arrange
            object actualInstance = null;

            var container = ContainerFactory.New();

            container.RegisterInitializer<object>(d => { actualInstance = d; });

            // Act
            var initializer = AdvancedExtensions.GetInitializer<object>(container);

            object expectedInstance = new object();

            initializer(expectedInstance);

            // Assert
            Assert.IsTrue(object.ReferenceEquals(expectedInstance, actualInstance));
        }

        [TestMethod]
        public void AppendToCollection_WithValidArguments_Suceeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.AppendToCollection(typeof(object), CreateRegistration(container));
        }
        
        [TestMethod]
        public void AppendToCollection_WithNullContainerArgument_ThrowsException()
        {
            // Arrange
            Container invalidContainer = null;

            // Act
            Action action =
                () => invalidContainer.AppendToCollection(typeof(object), CreateRegistration(new Container()));

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("container", action);
        }

        [TestMethod]
        public void AppendToCollection_WithNullServiceTypeArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type invalidServiceType = null;

            // Act
            Action action = 
                () => container.AppendToCollection(invalidServiceType, CreateRegistration(container));

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("serviceType", action);
        }

        [TestMethod]
        public void AppendToCollection_WithNullRegistrationArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Registration invalidRegistration = null;

            // Act
            Action action = () => container.AppendToCollection(typeof(object), invalidRegistration);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("registration", action);
        }

        [TestMethod]
        public void AppendToCollection_WithRegistrationForDifferentContainer_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var differentContainer = new Container();

            Registration invalidRegistration = CreateRegistration(differentContainer);

            // Act
            Action action = () => container.AppendToCollection(typeof(object), invalidRegistration);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentException>("registration", action);
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied Registration belongs to a different container.", action);
        }

        [TestMethod]
        public void AppendToCollection_ForUnregisteredCollection_ResolvesThatRegistrationWhenRequested()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration = Lifestyle.Transient.CreateRegistration<IPlugin, PluginImpl>(container);

            container.AppendToCollection(typeof(IPlugin), registration);            

            // Act
            var instance = container.GetAllInstances<IPlugin>().Single();

            // Assert
            Assert.IsInstanceOfType(instance, typeof(PluginImpl));
        }

        [TestMethod]
        public void AppendToCollection_CalledTwice_ResolvesBothRegistrationsWhenRequested()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration1 = Lifestyle.Transient.CreateRegistration<IPlugin, PluginImpl>(container);
            var registration2 = Lifestyle.Transient.CreateRegistration<IPlugin, PluginImpl2>(container);

            container.AppendToCollection(typeof(IPlugin), registration1);
            container.AppendToCollection(typeof(IPlugin), registration2);

            // Act
            var instances = container.GetAllInstances<IPlugin>().ToArray();

            // Assert
            Assert.IsInstanceOfType(instances[0], typeof(PluginImpl));
            Assert.IsInstanceOfType(instances[1], typeof(PluginImpl2));
        }

        [TestMethod]
        public void AppendToCollection_CalledAfterRegisterAllWithTypes_CombinedAllRegistrationsWhenRequested()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(PluginImpl));

            var registration = Lifestyle.Transient.CreateRegistration<IPlugin, PluginImpl2>(container);

            container.AppendToCollection(typeof(IPlugin), registration);

            // Act
            var instances = container.GetAllInstances<IPlugin>().ToArray();

            // Assert
            Assert.IsInstanceOfType(instances[0], typeof(PluginImpl));
            Assert.IsInstanceOfType(instances[1], typeof(PluginImpl2));
        }

        [TestMethod]
        public void AppendToCollection_CalledAfterRegisterAllWithRegistration_CombinedAllRegistrationsWhenRequested()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration1 = Lifestyle.Transient.CreateRegistration<IPlugin, PluginImpl>(container);
            var registration2 = Lifestyle.Transient.CreateRegistration<IPlugin, PluginImpl2>(container);

            container.RegisterAll(typeof(IPlugin), new[] { registration1 });

            container.AppendToCollection(typeof(IPlugin), registration2);

            // Act
            var instances = container.GetAllInstances<IPlugin>().ToArray();

            // Assert
            Assert.IsInstanceOfType(instances[0], typeof(PluginImpl));
            Assert.IsInstanceOfType(instances[1], typeof(PluginImpl2));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AppendToCollection_CalledAfterTheFirstItemIsRequested_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration1 = Lifestyle.Transient.CreateRegistration<IPlugin, PluginImpl>(container);
            var registration2 = Lifestyle.Transient.CreateRegistration<IPlugin, PluginImpl2>(container);

            container.AppendToCollection(typeof(IPlugin), registration1);

            var instances = container.GetAllInstances<IPlugin>().ToArray();

            // Act
            container.AppendToCollection(typeof(IPlugin), registration2);
        }

        [TestMethod]
        public void AppendToCollection_OnContainerUncontrolledCollection_ThrowsExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IPlugin> containerUncontrolledCollection = new[] { new PluginImpl() };

            container.RegisterAll<IPlugin>(containerUncontrolledCollection);

            var registration = Lifestyle.Transient.CreateRegistration<IPlugin, PluginImpl>(container);

            // Act
            Action action = () => container.AppendToCollection(typeof(IPlugin), registration);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(@"
                appending registrations to these collections is not supported. Please register the collection
                with one of the other RegisterAll overloads is appending is required."
                .TrimInside(),
                action);
        }

        private static Registration CreateRegistration(Container container)
        {
            return Lifestyle.Transient.CreateRegistration<IPlugin, PluginImpl>(container);
        }
    }
}