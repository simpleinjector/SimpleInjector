using System;
using System.Threading;
using CuttingEdge.ServiceLocation;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocator.Extensions.Tests.Unit
{
    [TestClass]
    public class SslPerThreadRegistrationsExtensionsTests
    {
        [TestMethod]
        public void RegisterPerThread_GetInstanceCalledMultipleTimesOnSingleThread_ReturnsSameInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterPerThread<IWeapon>(() => new Katana());

            // Act
            var instance1 = container.GetInstance<IWeapon>();
            var instance2 = container.GetInstance<IWeapon>();

            // Assert
            Assert.IsTrue(Object.ReferenceEquals(instance1, instance2),
                "GetInstance should result in same instance being returned for a single thread.");
        }

        [TestMethod]
        public void RegisterPerThread_GetInstanceCalledOnMultipleThreads_ReturnsMultipleInstances()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterPerThread<IWeapon>(() => new Katana());

            // Act
            IWeapon instance1 = null;
            var thread1 = new Thread(() =>
            {
                instance1 = container.GetInstance<IWeapon>();
                Thread.Sleep(100);
            });

            IWeapon instance2 = null;
            var thread2 = new Thread(() =>
            {
                instance2 = container.GetInstance<IWeapon>();
                Thread.Sleep(100);
            });

            thread1.Start();
            thread2.Start();
            thread1.Join();
            thread2.Join();

            // Assert
            Assert.IsFalse(Object.ReferenceEquals(instance1, instance2),
                "GetInstance should result in different instances being returned for each thread.");
        }
    }
}