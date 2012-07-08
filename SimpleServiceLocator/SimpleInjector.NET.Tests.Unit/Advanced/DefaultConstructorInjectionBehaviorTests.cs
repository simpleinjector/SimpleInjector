namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DefaultConstructorInjectionBehaviorTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildParameterExpression_WithNullArgument_ThrowsExpectedException()
        {
            // Arrange
            var behavior = new ContainerOptions().ConstructorInjectionBehavior;

            // Act
            behavior.BuildParameterExpression(null);
        }
    }
}