using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class GetInstanceByKeyTests
    {
        [TestMethod]
        public void GetInstanceByKey_WithNullKey_ReturnsANonKeyedInstance()
        {
            // Arrange
            var expectedInstance = new Katana();

            string nullKey = null;

            var container = new SimpleServiceLocator();

            container.RegisterSingle<IWeapon>(expectedInstance);

            // Act
            var actualInstance = container.GetInstance(typeof(IWeapon), nullKey);

            // Assert
            Assert.AreEqual(expectedInstance, actualInstance,
                "The contract of the Common Service Locator dictates that a null key should resolve a " +
                "non-keyed instance.");
        }

        [TestMethod]
        public void GetInstanceByKey_WithEmptyKey_ReturnsANonKeyedInstance()
        {
            // Arrange
            var expectedMessage = "No keyed registration for type";

            string emptyKey = string.Empty;

            var container = new SimpleServiceLocator();

            container.RegisterSingle<IWeapon>(new Katana());

            try
            {
                // Act
                var actualInstance = container.GetInstance(typeof(IWeapon), emptyKey);

                // Assert
                Assert.Fail("This call is expected to fail, because there is no keyed registration.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(expectedMessage),
                    "The exception message was not descriptive enough. Actual message: " + ex.Message);
            }
        }
    }
}