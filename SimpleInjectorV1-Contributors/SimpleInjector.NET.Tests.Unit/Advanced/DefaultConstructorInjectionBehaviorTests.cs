namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class DefaultConstructorInjectionBehaviorTests
    {
        [Test]
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