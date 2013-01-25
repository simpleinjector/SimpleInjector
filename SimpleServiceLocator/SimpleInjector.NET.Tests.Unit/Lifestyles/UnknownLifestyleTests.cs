namespace SimpleInjector.Tests.Unit.Lifestyles
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Lifestyles;

    [TestClass]
    public class UnknownLifestyleTests
    {
        [TestMethod]
        public void Instance_Always_ReturnsAValue()
        {
            Assert.IsNotNull(UnknownLifestyle.Instance);
        }

        [TestMethod]
        public void Instance_Always_ReturnsTheSameInstance()
        {
            // Act
            var instance1 = UnknownLifestyle.Instance;
            var instance2 = UnknownLifestyle.Instance;

            // Assert
            Assert.IsTrue(object.ReferenceEquals(instance1, instance2), "For performance and correctness, " + 
                "the Instance field must always return the same instance.");
        }

#if DEBUG
        [TestMethod]
        public void ComponentLength_Always_ReturnsTheSameLengthAsTheSingletonLifestyle()
        {
            Assert.AreEqual(Lifestyle.Singleton.ComponentLength, UnknownLifestyle.Instance.ComponentLength);
        }

        [TestMethod]
        public void DependencyLength_Always_ReturnsTheSameLengthAsTheTransientLifestyle()
        {
            Assert.AreEqual(Lifestyle.Transient.ComponentLength, UnknownLifestyle.Instance.DependencyLength);
        }
#endif

        [TestMethod]
        public void CreateRegistrationTServiceTImplementation_Always_ThrowsException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                UnknownLifestyle.Instance.CreateRegistration<object, string>(container);

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
            var container = new Container();

            try
            {
                // Act
                UnknownLifestyle.Instance.CreateRegistration<IDisposable>(() => null, container);

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