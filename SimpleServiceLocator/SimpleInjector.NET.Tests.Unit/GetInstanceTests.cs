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
        public void GetInstanceByType_CalledOnRegisteredButInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = new Container();

            container.Register<ServiceWithUnregisteredDependencies>();

            try
            {
                // Act
                container.GetInstance(typeof(ServiceWithUnregisteredDependencies));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(@"
                    The constructor of the type ServiceWithUnregisteredDependencies contains the parameter 
                    of type IDisposable with name 'a' that is not registered."
                    .TrimInside(),
                    ex.Message);
            }
        }

        [TestMethod]
        public void GetInstanceByType_CalledOnUnregisteredConcreteButInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.GetInstance(typeof(ServiceWithUnregisteredDependencies));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(@"
                    The constructor of the type ServiceWithUnregisteredDependencies contains the parameter 
                    of type IDisposable with name 'a' that is not registered."
                    .TrimInside(),
                    ex.Message);
            }
        }

        [TestMethod]
        public void GetInstanceGeneric_CalledOnUnregisteredInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.GetInstance<ServiceWithUnregisteredDependencies>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(@"
                    The constructor of the type ServiceWithUnregisteredDependencies contains the parameter 
                    of type IDisposable with name 'a' that is not registered."
                    .TrimInside(),
                    ex.Message);
            }
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

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstance_OnObjectWhileUnregistered_ThrowsActivationException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.GetInstance<object>();
        }

        //// Seems like there are tests missing, but all other cases are already covered by other test classes.
    }
}