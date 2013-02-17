#if DEBUG
namespace SimpleInjector.Tests.Unit.Diagnostics
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
            var validParameters = ValidKnownDependencyParameters();

            // Act
            CreateKnownDependency(validParameters);
        }

        [TestMethod]
        public void Ctor_WithValidParameters_CreatesInstanceWithExpectedProperties()
        {
            // Arrange
            var validParameters = ValidKnownDependencyParameters();

            // Act
            var dependency = CreateKnownDependency(validParameters);

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
            var invalidParameters = ValidKnownDependencyParameters();

            invalidParameters.ImplementationType = null;

            // Act
            Action action = () => CreateKnownDependency(invalidParameters);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("implementationType", action);
        }

        [TestMethod]
        public void Ctor_WithNullParentLifestyle_ThrowsExpectedException()
        {
            // Arrange
            var invalidParameters = ValidKnownDependencyParameters();

            invalidParameters.Lifestyle = null;

            // Act
            Action action = () => CreateKnownDependency(invalidParameters);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("lifestyle", action);
        }

        [TestMethod]
        public void Ctor_WithNullChildRegistration_ThrowsExpectedException()
        {
            // Arrange
            var invalidParameters = ValidKnownDependencyParameters();

            invalidParameters.Dependency = null;

            // Act
            Action action = () => CreateKnownDependency(invalidParameters);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("dependency", action);
        }

        private static KnownRelationship CreateKnownDependency(KnownDependencyConstructorParameters paramaters)
        {
            return new KnownRelationship(
                implementationType: paramaters.ImplementationType,
                lifestyle: paramaters.Lifestyle,
                dependency: paramaters.Dependency);
        }

        private static KnownDependencyConstructorParameters ValidKnownDependencyParameters()
        {
            return new KnownDependencyConstructorParameters
            {
                Lifestyle = Lifestyle.Transient,
                ImplementationType = typeof(RealTimeProvider),
                Dependency = (new Container()).GetRegistration(typeof(Container))
            };
        }

        private class KnownDependencyConstructorParameters
        {
            public Type ImplementationType { get; set; }

            public Lifestyle Lifestyle { get; set; }

            public InstanceProducer Dependency { get; set; }
        }
    }
}
#endif