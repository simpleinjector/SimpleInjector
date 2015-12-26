namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetServiceTests
    {
        [TestMethod]
        public void GetService_RequestingARegisteredType_ReturnsExpectedInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            var expectedInstance = new InMemoryUserRepository();

            container.RegisterSingleton<IUserRepository>(expectedInstance);

            // Act
            var actualInstance = ((IServiceProvider)container).GetService(typeof(IUserRepository));

            // Assert
            Assert.AreEqual(expectedInstance, actualInstance, "The IServiceProvider.GetService method did " +
                "not return the expected instance.");
        }

        [TestMethod]
        public void GetService_RequestingANonregisteredType_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var actualInstance = ((IServiceProvider)container).GetService(typeof(IUserRepository));

            // Assert
            Assert.IsNull(actualInstance, "The contract of the IServiceProvider states that it returns " +
                "null when no registration is found.");
        }

        [TestMethod]
        public void GetService_RequestingANonregisteredType2_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var actualInstance = ((IServiceProvider)container).GetService(typeof(string));

            // Assert
            Assert.IsNull(actualInstance, "The contract of the IServiceProvider states that it returns " +
                "null when no registration is found.");
        }

        [TestMethod]
        public void GetService_RequestingANonregisteredType3_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var actualInstance = ((IServiceProvider)container).GetService(typeof(int));

            // Assert
            Assert.IsNull(actualInstance, "The contract of the IServiceProvider states that it returns " +
                "null when no registration is found.");
        }

        [TestMethod]
        public void GetService_RequestingANonregisteredType_WillNotSuppressErrorsThrownFromUnregisteredTypeResolution()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Registration of an event that registers an invalid delegate, should make GetService fail.
            container.ResolveUnregisteredType += (sender, e) =>
            {
                if (e.UnregisteredServiceType == typeof(IUserRepository))
                {
                    Func<object> invalidDelegate = () => null;
                    e.Register(invalidDelegate);
                }
            };

            IServiceProvider serviceProvider = container;

            // Act
            Action action = () => serviceProvider.GetService(typeof(IUserRepository));

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void GetService_RequestingAUnregisteredTypeTwice_ReturnsNullSecondTime()
        {
            // Arrange
            IServiceProvider container = ContainerFactory.New();

            container.GetService(typeof(IUserRepository));

            // Act
            var actualInstance = container.GetService(typeof(IUserRepository));

            // Assert
            Assert.IsNull(actualInstance, "The contract of the IServiceProvider states that it returns " +
                "null when no registration is found.");
        }

        [TestMethod]
        public void GetService_RequestedOnUnregisteredInvalidType_ReturnsNull()
        {
            // Arrange
            Type invalidServiceType = typeof(ServiceWithUnregisteredDependencies);

            IServiceProvider container = ContainerFactory.New();

            // Act
            var registration = container.GetService(invalidServiceType);

            // Assert
            Assert.IsNull(registration);
        }

        [TestMethod]
        public void GetService_RequestedOnRegisteredInvalidType_ReturnsInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ServiceWithUnregisteredDependencies>();

            IServiceProvider provider = container;

            // Act
            Action action = () => provider.GetService(typeof(ServiceWithUnregisteredDependencies));

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }
    }
}