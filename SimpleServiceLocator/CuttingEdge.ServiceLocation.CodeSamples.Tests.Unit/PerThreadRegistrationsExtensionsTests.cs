using System;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.CodeSamples.Tests.Unit
{
    [TestClass]
    public class PerThreadRegistrationsExtensionsTests
    {
        [TestMethod]
        public void RegisterPerThread_GetInstanceCalledMultipleTimesOnSingleThread_ReturnsSameInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterPerThread<ICommand>(() => new ConcreteCommand());

            // Act
            var instance1 = container.GetInstance<ICommand>();
            var instance2 = container.GetInstance<ICommand>();

            // Assert
            Assert.IsTrue(Object.ReferenceEquals(instance1, instance2),
                "GetInstance should result in same instance being returned for a single thread.");
        }

        [TestMethod]
        public void RegisterPerThread_GetInstanceCalledOnMultipleThreads_ReturnsMultipleInstances()
        {
            // Arrange
            var container = new Container();

            container.RegisterPerThread<ICommand>(() => new ConcreteCommand());

            // Act
            ICommand instance1 = null;
            var thread1 = new Thread(() =>
            {
                instance1 = container.GetInstance<ICommand>();
                Thread.Sleep(100);
            });

            ICommand instance2 = null;
            var thread2 = new Thread(() =>
            {
                instance2 = container.GetInstance<ICommand>();
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