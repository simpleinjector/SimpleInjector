namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DefaultConstructorResolutionBehaviorTests
    {
        [TestMethod]
        public void GetConstructor_WithNullArgument1_ThrowsException()
        {
            // Arrange
            var behavior = GetContainerOptions().ConstructorResolutionBehavior;

            // Act
            Action action = () => behavior.GetConstructor(null, typeof(TypeWithSinglePublicDefaultConstructor));

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetConstructor_WithNullArgument2_ThrowsException()
        {
            // Arrange
            var behavior = GetContainerOptions().ConstructorResolutionBehavior;

            // Act
            Action action = () => behavior.GetConstructor(typeof(TypeWithSinglePublicDefaultConstructor), null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void IsRegistrationPhase_InstancesResolvedFromTheContainer_ReturnsFalse()
        {
            // Arrange
            var behavior = GetContainerOptions().ConstructorResolutionBehavior;

            // Act
            var constructor = behavior.GetConstructor(typeof(TypeWithSinglePublicDefaultConstructor),
                typeof(TypeWithSinglePublicDefaultConstructor));

            // Assert
            Assert.IsNotNull(constructor, "The constructor was expected to be returned.");
        }

        [TestMethod]
        public void GetConstructor_TypeWithMultiplePublicConstructors_ThrowsExpectedException()
        {
            // Arrange
            var behavior = GetContainerOptions().ConstructorResolutionBehavior;

            try
            {
                // Act
                behavior.GetConstructor(typeof(TypeWithMultiplePublicConstructors),
                    typeof(TypeWithMultiplePublicConstructors));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains("For the container to be able to create " +
                    "DefaultConstructorResolutionBehaviorTests.TypeWithMultiplePublicConstructors it " +
                    "should have only one public constructor: it has 2.", ex.Message);
            }
        }

        [TestMethod]
        public void GetConstructor_TypeWithSingleInternalConstructor_ThrowsExpectedException()
        {
            // Arrange
            var behavior = GetContainerOptions().ConstructorResolutionBehavior;

            // Act
            Action action = () => behavior.GetConstructor(typeof(TypeWithSingleInternalConstructor),
                typeof(TypeWithSingleInternalConstructor));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "For the container to be able to create " +
                "DefaultConstructorResolutionBehaviorTests.TypeWithSingleInternalConstructor it should " +
                "have only one public constructor: it has none.",
                action);
        }

        private static ContainerOptions GetContainerOptions()
        {
            return new Container().Options;
        }

        private class TypeWithSinglePublicDefaultConstructor
        {
            public TypeWithSinglePublicDefaultConstructor()
            {
            }
        }

        private class TypeWithMultiplePublicConstructors
        {
            public TypeWithMultiplePublicConstructors()
            {
            }

            public TypeWithMultiplePublicConstructors(int a)
            {
            }
        }

        private class TypeWithSingleInternalConstructor
        {
            internal TypeWithSingleInternalConstructor()
            {
            }
        }
    }
}