namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            Assert.IsTrue(object.ReferenceEquals(instance1, instance2),
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
            Exception exception1 = null;
            var thread1 = new Thread(() =>
            {
                try
                {
                    instance1 = container.GetInstance<ICommand>();
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    exception1 = ex;
                }
            });

            ICommand instance2 = null;
            Exception exception2 = null;
            var thread2 = new Thread(() =>
            {
                try
                {
                    instance2 = container.GetInstance<ICommand>();
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    exception2 = ex;
                }
            });

            thread1.Start();
            thread2.Start();
            thread1.Join();
            thread2.Join();

            // Assert
            if (exception1 != null)
            {
                Assert.Fail("Thread1 threw an exception: " + exception1.Message);
            }

            if (exception2 != null)
            {
                Assert.Fail("Thread1 threw an exception: " + exception2.Message);
            }

            Assert.IsFalse(object.ReferenceEquals(instance1, instance2),
                "GetInstance should result in different instances being returned for each thread.");
        }
    }
}