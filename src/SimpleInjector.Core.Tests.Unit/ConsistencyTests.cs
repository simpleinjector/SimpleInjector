namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq;
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

        [TestMethod]
        public void GCCollect_OnUnreferencedVerifiedContainersWithDecorators_CollectsThoseContainers()
        {
            // Arrange
            Func<Container> buildContainer = () =>
            {
                var container = new Container();
                container.Options.EnableDynamicAssemblyCompilation = false;
                container.Register<INonGenericService, RealNonGenericService>();

                container.RegisterDecorator<INonGenericService, NonGenericServiceDecorator>();

                container.GetInstance<INonGenericService>();

                container.Dispose();

                return container;
            };

            var containers =
                Enumerable.Range(0, 10).Select(_ => new WeakReference(buildContainer())).ToArray();

            // Act
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Assert
            Assert.AreEqual(0, containers.Count(c => c.IsAlive), "We've got a memory leak.");
        }

        private class Concrete
        {
        }
    }
}