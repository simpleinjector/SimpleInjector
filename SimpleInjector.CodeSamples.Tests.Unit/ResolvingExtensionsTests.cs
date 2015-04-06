namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class ResolvingExtensionsTests
    {
        [TestMethod]
        public void CanGetInstanceGeneric_OnRegisteredType_ReturnsTrue()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>();

            // Act
            bool result = container.CanGetInstance<ICommand>();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanGetInstanceNonGeneric_OnRegisteredType_ReturnsTrue()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>();

            // Act
            bool result = container.CanGetInstance(typeof(ICommand));

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanGetInstanceGeneric_OnOnregisteredAbstractType_ReturnsFalse()
        {
            // Arrange
            var container = new Container();

            // Act
            bool result = container.CanGetInstance<ICommand>();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanGetInstanceNonGeneric_OnOnregisteredAbstractType_ReturnsFalse()
        {
            // Arrange
            var container = new Container();

            // Act
            bool result = container.CanGetInstance(typeof(ICommand));

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanGetInstanceGeneric_OnOnregisteredString_ReturnsFalse()
        {
            // Arrange
            var container = new Container();

            // Act
            bool result = container.CanGetInstance<string>();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanGetInstanceGeneric_OnValueType_ReturnsFalse()
        {
            // Arrange
            var container = new Container();

            // Act
            bool result = container.CanGetInstance<int>();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanGetInstanceGeneric_OnOnregisteredConcreteType_ReturnsTrue()
        {
            // Arrange
            var container = new Container();

            // Act
            bool result = container.CanGetInstance<ConcreteCommand>();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanGetInstanceGeneric_TypeResolvedByUnregisteredTypeResolution_ReturnsTrue()
        {
            // Arrange
            var container = new Container();

            container.ResolveUnregisteredType += (sender, e) =>
            {
                if (e.UnregisteredServiceType == typeof(ICommand))
                {
                    e.Register(() => new ConcreteCommand());
                }
            };

            // Act
            bool result = container.CanGetInstance<ICommand>();

            // Assert
            Assert.IsTrue(result);
        }

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
            AssertThat.IsInstanceOfType(typeof(ConcreteCommand), command);
        }
    }
}