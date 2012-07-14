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
    }
}