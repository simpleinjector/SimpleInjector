namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TryGetInstanceTests
    {
        [TestMethod]
        public void TryGetInstance_RequestingARegisteredType_ReturnsExpectedInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            var expectedInstance = new InMemoryUserRepository();

            container.RegisterInstance<IUserRepository>(expectedInstance);

            // Act
            container.TryGetInstance(typeof(IUserRepository), out var actualInstance);

            // Assert
            Assert.AreEqual(expectedInstance, actualInstance, "The IServiceProvider.GetService method did " +
                "not return the expected instance.");
        }

        [TestMethod]
        public void TryGetInstance_RequestingANonregisteredType_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.TryGetInstance(typeof(IUserRepository), out var actualInstance);

            // Assert
            Assert.IsNull(actualInstance, "The contract of the IServiceProvider states that it returns " +
                "null when no registration is found.");
        }

        [TestMethod]
        public void TryGetInstance_RequestingANonregisteredType2_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.TryGetInstance(typeof(string), out var actualInstance);

            // Assert
            Assert.IsNull(actualInstance, "The contract of the IServiceProvider states that it returns " +
                "null when no registration is found.");
        }

        [TestMethod]
        public void TryGetInstance_RequestingANonregisteredType3_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.TryGetInstance(typeof(int), out var actualInstance);

            // Assert
            Assert.IsNull(actualInstance, "The contract of the IServiceProvider states that it returns " +
                "null when no registration is found.");
        }

        [TestMethod]
        public void TryGetInstance_RequestingANonregisteredType_WillNotSuppressErrorsThrownFromUnregisteredTypeResolution()
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

            // Act
            Action action = () => container.TryGetInstance(typeof(IUserRepository), out var instance);

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void TryGetInstance_RequestingAUnregisteredTypeTwice_ReturnsNullSecondTime()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.TryGetInstance(typeof(IUserRepository), out _);

            // Act
            container.TryGetInstance(typeof(IUserRepository), out var actualInstance);

            // Assert
            Assert.IsNull(actualInstance, "The contract of the IServiceProvider states that it returns " +
                "null when no registration is found.");
        }

        [TestMethod]
        public void TryGetInstance_RequestedOnUnregisteredInvalidType_ReturnsNull()
        {
            // Arrange
            Type invalidServiceType = typeof(ServiceWithUnregisteredDependencies);

            var container = ContainerFactory.New();

            // Act
            container.TryGetInstance(invalidServiceType, out var registration);

            // Assert
            Assert.IsNull(registration);
        }

        [TestMethod]
        public void TryGetInstance_RequestedOnRegisteredInvalidType_ReturnsInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ServiceWithUnregisteredDependencies>();

            // Act
            Action action = () => container.TryGetInstance(typeof(ServiceWithUnregisteredDependencies), out _);

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }
    }
}