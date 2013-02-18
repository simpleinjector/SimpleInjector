namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    // Test test class contains the left over tests that don't fit into any of the other test classes.
    [TestClass]
    public class ContainerTests
    {
        [TestMethod]
        public void Equals_OnSameInstance_ReturnsTrue()
        {
            // Arrange
            var container = new Container();

            // Act
            var result = container.Equals(container);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_OnDifferentInstance_ReturnsFalse()
        {
            // Arrange
            var container1 = new Container();
            var container2 = new Container();

            // Act
            var result = container1.Equals(container2);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ToString_Always_ReturnsExpectedValue()
        {
            // Arrange
            var container = new Container();

            // Act
            string result = container.ToString();

            // Assert
            Assert.AreEqual("SimpleInjector.Container", result);
        }

        [TestMethod]
        public void GetHashCode_Always_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.GetHashCode();
        }

        [TestMethod]
        public void GetType_Always_ReturnsTheExpectedType()
        {
            // Arrange
            var container = new Container();

            // Act
            Type type = container.GetType();

            // Assert
            Assert.AreEqual(typeof(Container), type);
        }
    }
}