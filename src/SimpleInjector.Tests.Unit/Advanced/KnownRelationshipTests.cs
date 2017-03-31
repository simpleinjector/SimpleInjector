namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;
    
    [TestClass]
    public class KnownDependencyTests
    {
        [TestMethod]
        public void Ctor_WithValidParameters_Succeeds()
        {
            // Arrange
            var validParameters = ValidKnownRelationshipParameters();

            // Act
            CreateKnownRelationship(validParameters);
        }

        [TestMethod]
        public void Ctor_WithValidParameters_CreatesInstanceWithExpectedProperties()
        {
            // Arrange
            var validParameters = ValidKnownRelationshipParameters();

            // Act
            var dependency = CreateKnownRelationship(validParameters);

            // Assert
            Assert.AreEqual(
                new
                {
                    ParentLifestyle = dependency.Lifestyle,
                    ParentImplementationType = dependency.ImplementationType,
                    ChildRegistration = dependency.Dependency,
                },
                new
                {
                    ParentLifestyle = validParameters.Lifestyle,
                    ParentImplementationType = validParameters.ImplementationType,
                    ChildRegistration = validParameters.Dependency,
                });
        }

        [TestMethod]
        public void Ctor_WithNullParentImplementationType_ThrowsExpectedException()
        {
            // Arrange
            var invalidParameters = ValidKnownRelationshipParameters();

            invalidParameters.ImplementationType = null;

            // Act
            Action action = () => CreateKnownRelationship(invalidParameters);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("implementationType", action);
        }

        [TestMethod]
        public void Ctor_WithNullParentLifestyle_ThrowsExpectedException()
        {
            // Arrange
            var invalidParameters = ValidKnownRelationshipParameters();

            invalidParameters.Lifestyle = null;

            // Act
            Action action = () => CreateKnownRelationship(invalidParameters);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("lifestyle", action);
        }

        [TestMethod]
        public void Ctor_WithNullChildRegistration_ThrowsExpectedException()
        {
            // Arrange
            var invalidParameters = ValidKnownRelationshipParameters();

            invalidParameters.Dependency = null;

            // Act
            Action action = () => CreateKnownRelationship(invalidParameters);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("dependency", action);
        }

        [TestMethod]
        public void Equals_ComparedWithNull_ReturnsFalse()
        {
            // Arrange
            var relationship = CreateValidKnownRelationship();

            // Act
            bool result = relationship.Equals(null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_ComparedWithSameInstance_ReturnsTrue()
        {
            // Arrange
            var relationship = CreateValidKnownRelationship();

            // Act
            bool result = relationship.Equals(relationship);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_ComparedWithOtherObjectWithSameContent_ReturnsTrue()
        {
            // Arrange
            var parameters = ValidKnownRelationshipParameters();

            var relationship = CreateKnownRelationship(parameters);
            var anotherRelationshipWithSameContent = CreateKnownRelationship(parameters);
            
            // Act
            bool result = relationship.Equals(anotherRelationshipWithSameContent);

            // Assert
            Assert.IsTrue(result);
        }

        private static KnownRelationship CreateValidKnownRelationship() => 
            CreateKnownRelationship(ValidKnownRelationshipParameters());

        private static KnownRelationship CreateKnownRelationship(KnownDependencyConstructorParameters paramaters) => 
            new KnownRelationship(
                implementationType: paramaters.ImplementationType,
                lifestyle: paramaters.Lifestyle,
                dependency: paramaters.Dependency);

        private static KnownDependencyConstructorParameters ValidKnownRelationshipParameters() => 
            new KnownDependencyConstructorParameters
            {
                Lifestyle = Lifestyle.Transient,
                ImplementationType = typeof(RealTimeProvider),
                Dependency = ContainerFactory.New().GetRegistration(typeof(Container))
            };

        private class KnownDependencyConstructorParameters
        {
            public Type ImplementationType { get; set; }

            public Lifestyle Lifestyle { get; set; }

            public InstanceProducer Dependency { get; set; }
        }
    }
}