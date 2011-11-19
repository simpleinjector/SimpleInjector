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
            var container = new Container();

            var expectedInstance = new InMemoryUserRepository();

            container.RegisterSingle<IUserRepository>(expectedInstance);

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
            var container = new Container();

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
            var container = new Container();

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
            var container = new Container();

            // Act
            var actualInstance = ((IServiceProvider)container).GetService(typeof(int));

            // Assert
            Assert.IsNull(actualInstance, "The contract of the IServiceProvider states that it returns " +
                "null when no registration is found.");
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetService_RequestingANonregisteredType_WillNotSuppressErrorsThrownFromUnregisteredTypeResolution()
        {
            // Arrange
            var container = new Container();

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
            var actualInstance = ((IServiceProvider)container).GetService(typeof(IUserRepository));
        }

        [TestMethod]
        public void GetService_RequestingAUnregisteredTypeTwice_ReturnsNullSecondTime()
        {
            // Arrange
            var container = new Container();

            // Act
            ((IServiceProvider)container).GetService(typeof(IUserRepository));
            var actualInstance = ((IServiceProvider)container).GetService(typeof(IUserRepository));

            // Assert
            Assert.IsNull(actualInstance, "The contract of the IServiceProvider states that it returns " +
                "null when no registration is found.");
        }
    }
}