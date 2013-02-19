namespace SimpleInjector.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConsistencyTests
    {
        [TestMethod]
        public void GetInstance_CalledFromMultipleThreads_Succeeds()
        {
            // Arrange
            bool shouldCall = true;

            var container = ContainerFactory.New();

            // We 'abuse' ResolveUnregisteredType to simulate multi-threading. ResolveUnregisteredType is 
            // called during GetInstance, but before the IInstanceProvider<Concrete> is added to the
            // registrations dictionary.
            container.ResolveUnregisteredType += (s, e) =>
            {
                if (shouldCall)
                {
                    // Prevent stack overflow.
                    shouldCall = false;

                    // Simulate multi-threading.
                    container.GetInstance<Concrete>();
                }
            };

            // Act
            // What we in fact test here is whether the container correctly makes a snapshot of the 
            // registrations dictionary. This call would fail in that case, because the snapshot is needed
            // for consistency.
            container.GetInstance<Concrete>();
            
            // Assert
            Assert.IsFalse(shouldCall, "ResolveUnregisteredType was not called.");
        }

        private class Concrete
        {
        }
    }
}