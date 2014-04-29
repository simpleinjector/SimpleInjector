namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Advanced;

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

        [TestMethod]
        public void ConstructorInjectionBehavior_CustomBehaviorThatReturnsNull_ThrowsExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.ConstructorInjectionBehavior = new FakeConstructorInjectionBehavior
            {
                ExpressionToReturnFromBuildParameterExpression = null
            };

            container.Register<IUserRepository, SqlUserRepository>();

            try
            {
                // Act
                // RealUserService depends on IUserRepository
                container.GetInstance<RealUserService>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.ExceptionMessageContains(
                    "FakeConstructorInjectionBehavior that was registered through " + 
                    "Container.Options.ConstructorInjectionBehavior returned a null reference", ex);
                AssertThat.ExceptionMessageContains(
                    "argument of type IUserRepository with name 'repository' from the constructor of type " + 
                    "RealUserService", ex);
            }
        }

        private sealed class FakeConstructorInjectionBehavior : IConstructorInjectionBehavior
        {
            public Expression ExpressionToReturnFromBuildParameterExpression { get; set; }

            public Expression BuildParameterExpression(ParameterInfo parameter)
            {
                return this.ExpressionToReturnFromBuildParameterExpression;
            }
        }
    }
}