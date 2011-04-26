using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.CodeSamples.Tests.Unit
{
    [TestClass]
    public class ResolvingExtensionsTests
    {
        [TestMethod]
        public void TryGetInstanceOfT_ServiceTypeRegistered_ReturnsTrue()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ICommand>(new ConcreteCommand());

            // Act
            ICommand command;

            bool found = container.TryGetInstance<ICommand>(out command);

            // Assert
            Assert.IsTrue(found, "TryGetInstance<T> is expected to return true when the type is registered.");
        }

        [TestMethod]
        public void TryGetInstanceOfT_ServiceTypeRegistered_ReturnsInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ICommand>(new ConcreteCommand());

            // Act
            ICommand command;

            container.TryGetInstance<ICommand>(out command);

            // Assert
            Assert.IsNotNull(command, "TryGetInstance<T> is expected to return an instance when the type is registered.");
        }

        [TestMethod]
        public void TryGetInstanceOfT_ServiceTypeNotRegistered_ReturnsFalse()
        {
            // Arrange
            var container = new Container();

            // Act
            ICommand command;

            bool found = container.TryGetInstance<ICommand>(out command);

            // Assert
            Assert.IsFalse(found, "TryGetInstance<T> is expected to return false when the type is not registered.");
        }

        [TestMethod]
        public void TryGetInstanceOfT_ServiceTypeNotRegistered_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            // Act
            ICommand command;

            container.TryGetInstance<ICommand>(out command);

            // Assert
            Assert.IsNull(command, "TryGetInstance<T> is expected to return null when the type is not registered.");
        }

        [TestMethod]
        public void TryGetInstance_ServiceTypeRegistered_ReturnsTrue()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ICommand>(new ConcreteCommand());

            // Act
            object command;

            bool found = container.TryGetInstance(typeof(ICommand), out command);

            // Assert
            Assert.IsTrue(found, "TryGetInstance is expected to return true when the type is registered.");
        }

        [TestMethod]
        public void TryGetInstance_ServiceTypeRegistered_ReturnsInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ICommand>(new ConcreteCommand());

            // Act
            object command;

            container.TryGetInstance(typeof(ICommand), out command);

            // Assert
            Assert.IsNotNull(command, "TryGetInstance is expected to return an instance when the type is registered.");
        }

        [TestMethod]
        public void TryGetInstance_ServiceTypeNotRegistered_ReturnsFalse()
        {
            // Arrange
            var container = new Container();

            // Act
            object command;

            bool found = container.TryGetInstance(typeof(ICommand), out command);

            // Assert
            Assert.IsFalse(found, "TryGetInstance is expected to return false when the type is not registered.");
        }

        [TestMethod]
        public void TryGetInstance_ServiceTypeNotRegistered_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            // Act
            object command;

            container.TryGetInstance(typeof(ICommand), out command);

            // Assert
            Assert.IsNull(command, "TryGetInstance is expected to return null when the type is not registered.");
        }

        [TestMethod]
        public void TryGetInstance_ServiceTypeRegistered_ReturnsInstanceOfExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ICommand>(new ConcreteCommand());

            // Act
            object command;

            container.TryGetInstance(typeof(ICommand), out command);

            // Assert
            Assert.IsInstanceOfType(command, typeof(ConcreteCommand));
        }
    }
}