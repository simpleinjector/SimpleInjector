namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    [TestClass]
    public class DefaultConstructorInjectionBehaviorTests
    {
        [TestMethod]
        public void BuildParameterExpression_WithNullArgument_ThrowsExpectedException()
        {
            // Arrange
            var behavior = GetContainerOptions().ConstructorInjectionBehavior;

            // Act
            Action action = () => behavior.BuildParameterExpression(null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
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

        [TestMethod]
        public void Verify_TValueTypeParameter_ThrowsExpectedException()
        {
            // Arrange
            string expectedString = string.Format(@"
                The constructor of type {0}.{1} contains parameter 'intArgument' of type Int32 which can not 
                be used for constructor injection because it is a value type.",
                this.GetType().Name,
                typeof(TypeWithSinglePublicConstructorWithValueTypeParameter).Name)
                .TrimInside();

            var behavior = new Container().Options.ConstructorInjectionBehavior;

            var constructor =
                typeof(TypeWithSinglePublicConstructorWithValueTypeParameter).GetConstructors().Single();

            var invalidParameter = constructor.GetParameters().Single();

            try
            {
                // Act
                behavior.Verify(invalidParameter);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(expectedString, ex.Message);
            }
        }

        [TestMethod]
        public void Verify_StringTypeParameter_ThrowsExpectedException()
        {
            // Arrange
            string expectedString = string.Format(@"
                The constructor of type {0}.{1} contains parameter 'stringArgument' of type String which can 
                not be used for constructor injection.",
                this.GetType().Name,
                typeof(TypeWithSinglePublicConstructorWithStringTypeParameter).Name)
                .TrimInside();

            var behavior = new Container().Options.ConstructorInjectionBehavior;

            var constructor =
                typeof(TypeWithSinglePublicConstructorWithStringTypeParameter).GetConstructors().Single();

            var invalidParameter = constructor.GetParameters().Single();

            try
            {
                // Act
                behavior.Verify(invalidParameter);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(expectedString, ex.Message);
            }
        }

        private static ContainerOptions GetContainerOptions()
        {
            return new Container().Options;
        }

        private class TypeWithSinglePublicConstructorWithValueTypeParameter
        {
            public TypeWithSinglePublicConstructorWithValueTypeParameter(int intArgument)
            {
            }
        }

        private class TypeWithSinglePublicConstructorWithStringTypeParameter
        {
            public TypeWithSinglePublicConstructorWithStringTypeParameter(string stringArgument)
            {
            }
        }

        private sealed class FakeConstructorInjectionBehavior : IConstructorInjectionBehavior
        {
            public Expression ExpressionToReturnFromBuildParameterExpression { get; set; }

            public Expression BuildParameterExpression(ParameterInfo parameter)
            {
                return this.ExpressionToReturnFromBuildParameterExpression;
            }
            
            public void Verify(ParameterInfo parameter)
            {
            }
        }
    }
}