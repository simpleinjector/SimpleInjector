namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;

    using NUnit.Framework;

    using SimpleInjector.Advanced;

    [TestFixture]
    public class AdvancedExtensionsTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsLocked_WithNullArgument_ThrowsException()
        {
            // Act
            AdvancedExtensions.IsLocked(null);
        }
    }
}