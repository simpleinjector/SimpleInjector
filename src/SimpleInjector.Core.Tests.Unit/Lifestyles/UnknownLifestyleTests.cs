namespace SimpleInjector.Tests.Unit.Lifestyles
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnknownLifestyleTests
    {
        [TestMethod]
        public void Instance_Always_ReturnsAValue()
        {
            Assert.IsNotNull(Lifestyle.Unknown);
        }

        [TestMethod]
        public void Instance_Always_ReturnsTheSameInstance()
        {
            // Act
            var instance1 = Lifestyle.Unknown;
            var instance2 = Lifestyle.Unknown;

            // Assert
            Assert.IsTrue(object.ReferenceEquals(instance1, instance2), "For performance and correctness, " + 
                "the Instance field must always return the same instance.");
        }

        [TestMethod]
        public void ComponentLength_Always_ReturnsTheSameLengthAsTheSingletonLifestyle()
        {
            Assert.AreEqual(Lifestyle.Singleton.ComponentLength(null), Lifestyle.Unknown.ComponentLength(null));
        }

        [TestMethod]
        public void DependencyLength_Always_ReturnsTheSameLengthAsTheTransientLifestyle()
        {
            Assert.AreEqual(Lifestyle.Transient.ComponentLength(null), Lifestyle.Unknown.DependencyLength(null));
        }

        [TestMethod]
        public void CreateRegistrationTImplementation_Always_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                // Act
                Lifestyle.Unknown.CreateRegistration<string>(container);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains(
                    "The unknown lifestyle does not allow creation of registrations.", ex);
            }
        }

        [TestMethod]
        public void CreateRegistrationTService_Always_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                // Act
                Lifestyle.Unknown.CreateRegistration<IDisposable>(() => null, container);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains(
                    "The unknown lifestyle does not allow creation of registrations.", ex);
            }
        }
    }
}