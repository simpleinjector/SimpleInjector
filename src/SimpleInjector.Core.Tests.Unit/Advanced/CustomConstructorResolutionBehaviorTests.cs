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
        public void Register_NullReturningConstructorResolutionBehavior_ThrowsExpressiveErrorMessage()
        {
            // Arrange
            var container = new Container();

            container.Options.ConstructorResolutionBehavior = new NullReturningConstructorResolutionBehavior();

            // Act
            Action action = () => container.Register<RealTimeProvider>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(@"
                The CustomConstructorResolutionBehaviorTests.NullReturningConstructorResolutionBehavior that 
                was registered through Container.Options.ConstructorResolutionBehavior returned a null 
                reference after its GetConstructor method was supplied with implementationType 
                'RealTimeProvider'. IConstructorResolutionBehavior.GetConstructor implementations should 
                never return null, but should throw a SimpleInjector.ActivationException with an expressive 
                message instead."
                .TrimInside(),
                action);
        }

        private class NullReturningConstructorResolutionBehavior : IConstructorResolutionBehavior
        {
            public ConstructorInfo GetConstructor(Type implementationType) => null;
        }
    }
}