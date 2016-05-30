namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    [TestClass]
    public class ScopeTests
    {
        [TestMethod]
        public void GetItem_NoValueSet_ReturnsNull()
        {
            // Arrange
            object key = new object();

            var scope = new Scope(new Container());

            // Act
            object item = scope.GetItem(key);

            // Assert
            Assert.IsNull(item);
        }

        [TestMethod]
        public void GetItem_WithValueSet_ReturnsThatItem()
        {
            // Arrange
            object key = new object();
            object expectedItem = new object();

            var scope = new Scope(new Container());

            scope.SetItem(key, expectedItem);

            // Act
            object actualItem = scope.GetItem(key);

            // Assert
            Assert.AreSame(expectedItem, actualItem);
        }

        [TestMethod]
        public void GetItem_WithValueSetInOneContainer_DoesNotReturnThatItemInAnotherContainer()
        {
            // Arrange
            object key = new object();
            object expectedItem = new object();

            var container = new Container();
            var scope1 = new Scope(container);
            var scope2 = new Scope(container);

            scope1.SetItem(key, expectedItem);

            // Act
            object actualItem = scope2.GetItem(key);

            // Assert
            Assert.IsNull(actualItem, "The items dictionary is expected to be bound to the scope. Not the container!");
        }

        [TestMethod]
        public void GetItem_WithValueSetTwice_ReturnsLastItem()
        {
            // Arrange
            object key = new object();
            object firstItem = new object();
            object expectedItem = new object();

            var scope = new Scope(new Container());

            scope.SetItem(key, firstItem);
            scope.SetItem(key, expectedItem);

            // Act
            object actualItem = scope.GetItem(key);

            // Assert
            Assert.AreSame(expectedItem, actualItem);
        }

        [TestMethod]
        public void GetItem_WithValueReset_ReturnsNull()
        {
            // Arrange
            object key = new object();

            var scope = new Scope(new Container());

            scope.SetItem(key, new object());
            scope.SetItem(key, null);

            // Act
            object item = scope.GetItem(key);

            // Assert
            // This test looks odd, but under the cover the item is removed from the collection when null
            // is supplied to prevent the dictionary from ever increasing, but we have to test this code path.
            Assert.IsNull(item, "When a value is overridden with null, it is expected to return null.");
        }

        [TestMethod]
        public void GetItem_WithNullKey_ThrowsException()
        {
            // Arrange
            var scope = new Scope(new Container());

            // Act
            Action action = () => scope.GetItem(null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void SetItem_WithNullKey_ThrowsException()
        {
            // Arrange
            var scope = new Scope(new Container());

            // Act
            Action action = () => scope.SetItem(null, new object());

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }
    }
}