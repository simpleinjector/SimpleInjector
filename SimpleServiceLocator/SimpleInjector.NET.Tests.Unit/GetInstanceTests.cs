namespace SimpleInjector.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetInstanceTests
    {
        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByType_CalledOnUnregisteredInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.GetInstance(typeof(ServiceWithUnregisteredDependencies));
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByType_CalledOnRegisteredInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = new Container();

            container.Register<ServiceWithUnregisteredDependencies>();

            // Act
            container.GetInstance(typeof(ServiceWithUnregisteredDependencies));
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceGeneric_CalledOnUnregisteredInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.GetInstance<ServiceWithUnregisteredDependencies>();
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceGeneric_CalledOnRegisteredInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = new Container();

            container.Register<ServiceWithUnregisteredDependencies>();

            // Act
            container.GetInstance<ServiceWithUnregisteredDependencies>();
        }

        //// Seems like there are tests missing, but all other cases are already covered by other test classes.
    }
}