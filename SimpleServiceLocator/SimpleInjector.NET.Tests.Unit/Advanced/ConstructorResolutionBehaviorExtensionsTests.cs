namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Advanced;

    [TestClass]
    public class ConstructorResolutionBehaviorExtensionsTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetConstructorResolutionBehavior_WithNullArgument_ThrowsException()
        {
            // Act
            ConstructorResolutionBehaviorExtensions.GetConstructorResolutionBehavior(null);
        }
    }
}