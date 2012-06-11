namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Advanced;

    [TestClass]
    public class ConstructorResolutionBehaviorTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetConstructor_WithNullArgument_ThrowsException()
        {
            // Arrange
            var behavior = new ContainerOptions().ConstructorResolutionBehavior;

            // Act
            behavior.GetConstructor(null);
        }

        [TestMethod]
        public void IsRegistrationPhase_BehaviorNotPartOfAnyContainerOptions_ReturnsTrue()
        {
            // Arrange
            var behavior = new FakeConstructorResolutionBehavior();

            // Assert
            Assert.IsTrue(behavior.IsRegistrationPhase);
        }

        [TestMethod]
        public void IsRegistrationPhase_BehaviorNotPartOfAnyContainer_ReturnsTrue()
        {
            // Arrange
            var behavior = new FakeConstructorResolutionBehavior();

            // Act
            var options = new ContainerOptions
            {
                ConstructorResolutionBehavior = behavior
            };

            // Assert
            Assert.IsTrue(behavior.IsRegistrationPhase);
        }

        [TestMethod]
        public void IsRegistrationPhase_NoRegistrationsMadeToContainer_ReturnsTrue()
        {
            // Arrange
            var behavior = new FakeConstructorResolutionBehavior();

            var container = new Container(new ContainerOptions { ConstructorResolutionBehavior = behavior });

            // Assert
            Assert.IsTrue(behavior.IsRegistrationPhase);
        }

        [TestMethod]
        public void IsRegistrationPhase_RegistrationsMadeToContainerButNoInstancesResolved_ReturnsTrue()
        {
            // Arrange
            var behavior = new FakeConstructorResolutionBehavior();

            var container = new Container(new ContainerOptions { ConstructorResolutionBehavior = behavior });

            // Assert
            Assert.IsTrue(behavior.IsRegistrationPhase);
        }

        [TestMethod]
        public void IsRegistrationPhase_InstancesResolvedFromTheContainer_ReturnsFalse()
        {
            // Arrange
            var behavior = new FakeConstructorResolutionBehavior();

            var container = new Container(new ContainerOptions { ConstructorResolutionBehavior = behavior });

            // Act
            container.GetInstance<SomeConcreteType>();

            // Assert
            Assert.IsFalse(behavior.IsRegistrationPhase);
        }

        private class FakeConstructorResolutionBehavior : ConstructorResolutionBehavior
        {
            public FakeConstructorResolutionBehavior()
                : base()
            {
            }

            public new bool IsRegistrationPhase
            {
                get { return base.IsRegistrationPhase; }
            }

            public override System.Reflection.ConstructorInfo GetConstructor(Type type)
            {
                return type.GetConstructors()[0];
            }
        }

        private class SomeConcreteType
        {
            public SomeConcreteType()
            {
            }
        }
    }
}