namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;

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
            var container = new Container();

            // Act
            var initializer = AdvancedExtensions.GetInitializer<IDisposable>(container);

            // Assert
            Assert.IsNull(initializer);
        }

        [TestMethod]
        public void GetInitializer_InitializerRegisteredForRequestedType_ReturnsADelegate()
        {
            // Arrange
            var container = new Container();

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

            var container = new Container();

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

            var container = new Container();

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

            var container = new Container();

            container.RegisterInitializer<object>(d => { actualInstance = d; });

            // Act
            var initializer = AdvancedExtensions.GetInitializer<object>(container);

            object expectedInstance = new object();

            initializer(expectedInstance);

            // Assert
            Assert.IsTrue(object.ReferenceEquals(expectedInstance, actualInstance));
        }
    }
}