using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class FormattingTests
    {
        [TestMethod]
        public void FormatActivationExceptionMessage_NullException_ReturnsDefaultMessage()
        {
            // Arrange
            var simpleServiceLocator = new FakeSimpleServiceLocator();
            var key = "key";
            var expectedMessage =
                "Activation error occurred while trying to get instance of type Int32, key \"key\".";

            // Act
            var actualMessage = simpleServiceLocator.FormatActivationExceptionMessage(null, typeof(int), key);

            // Assert
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public void FormatActivationExceptionMessage_ExceptionWithMessage_ReturnsDefaultMessage()
        {
            // Arrange
            var simpleServiceLocator = new FakeSimpleServiceLocator();
            var exception = new InvalidOperationException("Some message.");
            var key = "key";
            var expectedMessage = "Activation error occurred while trying to get instance of type Int32, " +
                "key \"key\". Some message.";

            // Act
            var actualMessage =
                simpleServiceLocator.FormatActivationExceptionMessage(exception, typeof(int), key);

            // Assert
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public void FormatActivateAllExceptionMessage_NullException_ReturnsDefaultMessage()
        {
            // Arrange
            var simpleServiceLocator = new FakeSimpleServiceLocator();
            var expectedMessage =
                "Activation error occurred while trying to get all instances of type Int32.";

            // Act
            var actualMessage = simpleServiceLocator.FormatActivateAllExceptionMessage(null, typeof(int));

            // Assert
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public void FormatActivateAllExceptionMessage_ExceptionWithMessage_ReturnsDefaultMessage()
        {
            // Arrange
            var simpleServiceLocator = new FakeSimpleServiceLocator();
            var exception = new InvalidOperationException("Some message.");
            var expectedMessage =
                "Activation error occurred while trying to get all instances of type Int32. Some message.";

            // Act
            var actualMessage = simpleServiceLocator.FormatActivateAllExceptionMessage(exception, typeof(int));

            // Assert
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        private sealed class FakeSimpleServiceLocator : SimpleServiceLocator
        {
            public new string FormatActivateAllExceptionMessage(Exception actualException, Type serviceType)
            {
                return base.FormatActivateAllExceptionMessage(actualException, serviceType);
            }

            public new string FormatActivationExceptionMessage(Exception actualException, Type serviceType, string key)
            {
                return base.FormatActivationExceptionMessage(actualException, serviceType, key);
            }
        }
    }
}