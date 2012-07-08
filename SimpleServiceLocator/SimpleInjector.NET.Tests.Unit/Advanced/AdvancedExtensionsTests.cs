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
        public void GetConstructorResolutionBehavior_WithNullArgument_ThrowsException()
        {
            // Act
            AdvancedExtensions.GetConstructorResolutionBehavior(null);
        }

        [TestMethod]
        public void GetConstructorResolutionBehavior_WithValidArgument_ReturnsAValue()
        {
            // Act
            var behavior = AdvancedExtensions.GetConstructorResolutionBehavior(new Container());

            // Assert
            Assert.IsNotNull(behavior);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetConstructorVerificationBehavior_WithNullArgument_ThrowsException()
        {
            // Act
            AdvancedExtensions.GetConstructorVerificationBehavior(null);
        }

        [TestMethod]
        public void GetConstructorVerificationBehavior_WithValidArgument_ReturnsAValue()
        {
            // Act
            var behavior = AdvancedExtensions.GetConstructorVerificationBehavior(new Container());

            // Assert
            Assert.IsNotNull(behavior);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetConstructorInjectionBehavior_WithNullArgument_ThrowsException()
        {
            // Act
            AdvancedExtensions.GetConstructorInjectionBehavior(null);
        }

        [TestMethod]
        public void GetConstructorInjectionBehavior_WithValidArgument_ReturnsAValue()
        {
            // Act
            var behavior = AdvancedExtensions.GetConstructorInjectionBehavior(new Container());

            // Assert
            Assert.IsNotNull(behavior);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsLocked_WithNullArgument_ThrowsException()
        {
            // Act
            AdvancedExtensions.IsLocked(null);
        }
    }
}