namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    [TestClass]
    public class CustomConstructorResolutionBehaviorTests
    {
        [TestMethod]
        public void Register_ConstructorResolutionBehaviorThatReturnsBothNullCtorAndNullErrorMessage_ThrowsExpressiveErrorMessage()
        {
            // Arrange
            var container = new Container();

            container.Options.ConstructorResolutionBehavior = new FakeConstructorResolutionBehavior
            {
                ErrorMessage = null,
                ConstructorToReturn = null
            };

            // Act
            Action action = () => container.Register<RealTimeProvider>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>($@"
                For the container to be able to create {nameof(RealTimeProvider)} it should have a constructor
                that can be called, but according to the customly configured IConstructorResolutionBehavior of
                type {typeof(FakeConstructorResolutionBehavior).ToFriendlyName()}, there is no selectable
                constructor. The {typeof(FakeConstructorResolutionBehavior).ToFriendlyName()}, however, 
                didn't supply a reason why."
                .TrimInside(),
                action);
        }

        private class FakeConstructorResolutionBehavior : IConstructorResolutionBehavior
        {
            public string ErrorMessage { get; set; }
            public ConstructorInfo ConstructorToReturn { get; set; }

            public ConstructorInfo TryGetConstructor(Type implementationType, out string errorMessage)
            {
                errorMessage = this.ErrorMessage;
                return this.ConstructorToReturn;
            }
        }
    }
}