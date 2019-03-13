namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ContainerScopeTests
    {
        [TestMethod]
        public void GetItem_NoValueSet_ReturnsNull()
        {
            // Arrange
            object key = new object();

            var container = ContainerFactory.New();

            // Act
            object item = container.ContainerScope.GetItem(key);

            // Assert
            Assert.IsNull(item);
        }

        [TestMethod]
        public void GetItem_WithValueSet_ReturnsThatItem()
        {
            // Arrange
            object key = new object();
            object expectedItem = new object();

            var container = ContainerFactory.New();

            container.ContainerScope.SetItem(key, expectedItem);

            // Act
            object actualItem = container.ContainerScope.GetItem(key);

            // Assert
            Assert.AreSame(expectedItem, actualItem);
        }

        [TestMethod]
        public void GetItem_WithValueSetInOneContainer_DoesNotReturnThatItemInAnotherContainer()
        {
            // Arrange
            object key = new object();
            object expectedItem = new object();

            var container1 = ContainerFactory.New();
            var container2 = ContainerFactory.New();

            container1.ContainerScope.SetItem(key, expectedItem);

            // Act
            object actualItem = container2.ContainerScope.GetItem(key);

            // Assert
            Assert.IsNull(actualItem, "The items dictionary is expected to be container bound. Not static!");
        }

        [TestMethod]
        public void GetItem_WithValueSetTwice_ReturnsLastItem()
        {
            // Arrange
            object key = new object();
            object firstItem = new object();
            object expectedItem = new object();

            var container = ContainerFactory.New();

            container.ContainerScope.SetItem(key, firstItem);
            container.ContainerScope.SetItem(key, expectedItem);

            // Act
            object actualItem = container.ContainerScope.GetItem(key);

            // Assert
            Assert.AreSame(expectedItem, actualItem);
        }

        [TestMethod]
        public void GetItem_WithValueReset_ReturnsNull()
        {
            // Arrange
            object key = new object();

            var container = ContainerFactory.New();

            container.ContainerScope.SetItem(key, new object());
            container.ContainerScope.SetItem(key, null);

            // Act
            object item = container.ContainerScope.GetItem(key);

            // Assert
            // This test looks odd, but under the cover the item is removed from the collection when null
            // is supplied to prevent the dictionary from ever increasing, but we have to test this code path.
            Assert.IsNull(item, "When a value is overridden with null, it is expected to return null.");
        }

        [TestMethod]
        public void GetItem_WithNullKey_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.ContainerScope.GetItem(null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void SetItem_WithNullKey_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.ContainerScope.SetItem(null, new object());

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetOrSetItem_WithNullKey_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.ContainerScope.GetOrSetItem<object>(null, (_, __) => null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetOrSetItem_WithNullFactory_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.ContainerScope.GetOrSetItem<object>(new object(), null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetOrSetItem_WithValueArguments_DoesNotReturnNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var instance = container.ContainerScope.GetOrSetItem(new object(), (_, __) => new object());

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void GetOrSetItem_CalledTwiceForSameKey_ReturnsTheSameValue()
        {
            // Arrange
            var key = new object();

            var container = ContainerFactory.New();

            // Act
            var instance1 = container.ContainerScope.GetOrSetItem(key, (_, __) => new object());
            var instance2 = container.ContainerScope.GetOrSetItem(key, (_, __) => new object());

            // Assert
            Assert.AreSame(instance1, instance2);
        }
    }
}