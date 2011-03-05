using System;
using System.Threading;

using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class CyclicDependencyValidatorTests
    {
        private interface IOne
        {
        }

        private interface ITwo
        {
        }

        [TestMethod]
        public void GetInstance_RequestingTypeDependingOnItself_ThrowsExceptionWithExpectedMessage()
        {
            string expectedExcpetionMessage = "The configuration is invalid. The type " + typeof(A).FullName +
                " is directly or indirectly depending on itself.";

            try
            {
                // Arrange
                var container = new SimpleServiceLocator();

                // Act
                container.GetInstance<A>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(expectedExcpetionMessage), "The exception message did " +
                    "not contain the exception string. Actual: " + ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_RequestingTransientTypeDependingIndirectlyOnItself_Throws()
        {
            try
            {
                // Arrange
                var container = new SimpleServiceLocator();

                // Act
                // Graph: TransientInstanceProducer<A> -> B
                //        TransientInstanceProducer<B> -> A
                container.GetInstance<A>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
            }
        }
            
        [TestMethod]
        public void GetInstance_RequestingTransientTypeDependingIndirectlyOnItself_AlsoFailsOnConsecutiveCalls()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            try
            {
                container.GetInstance<A>();
            }
            catch (ActivationException)
            {
            }

            try
            {
                // Act
                container.GetInstance<A>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
            }
        }

        [TestMethod]
        public void GetInstance_RequestingTransientTypeDependingIndirectlyOnItselfViaASingletonType_Throws()
        {
            try
            {
                // Arrange
                var container = new SimpleServiceLocator();

                container.RegisterSingle<B>();

                // Act
                // Graph: TransientInstanceProducer<A> -> B
                //        FuncSingletonInstanceProducer<B> + TransientInstanceProducer<B> -> A
                container.GetInstance<A>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
            }
        }

        [TestMethod]
        public void GetInstance_RequestingSingletonTypeDependingIndirectlyOnItselfViaTransientType_Throws()
        {
            try
            {
                // Arrange
                var container = new SimpleServiceLocator();

                container.RegisterSingle<A>();

                // Act
                // Graph: FuncSingletonInstanceProducer<A> + TransientInstanceProducer<A> -> B
                //        TransientInstanceProducer<B> -> A
                container.GetInstance<A>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
            }
        }

        [TestMethod]
        public void GetInstance_RequestingSingletonTypeDependingIndirectlyOnItselfViaSingletonType_Throws()
        {
            try
            {
                // Arrange
                var container = new SimpleServiceLocator();

                // Graph: FuncSingletonInstanceProducer<A> + TransientInstanceProducer<A> -> B
                //        FuncSingletonInstanceProducer<B> + TransientInstanceProducer<B> -> A
                container.RegisterSingle<A>();
                container.RegisterSingle<B>();

                // Act
                container.GetInstance<A>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
            }
        }

        [TestMethod]
        public void GetInstance_RequestingTransientTypeDependingIndirectlyOnItselfThroughInterfaces_Throws()
        {
            try
            {
                // Arrange
                var container = new SimpleServiceLocator();

                // Graph: TransientInstanceProducer<One> -> ITwo
                //        TransientInstanceProducer<Two> -> IOne
                container.Register<IOne, One>();
                container.Register<ITwo, Two>();

                // Act
                container.GetInstance<IOne>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
            }
        }

        [TestMethod]
        public void GetInstance_RequestingSingletonTypeDependingIndirectlyOnItselfThroughInterfaces_Throws()
        {
            try
            {
                // Arrange
                var container = new SimpleServiceLocator();

                // Graph: FuncSingletonInstanceProducer<IOne> + TransientInstanceProducer<One> ->
                //        FuncSingletonInstanceProducer<ITwo> + TransientInstanceProducer<Two> -> IOne
                container.RegisterSingle<IOne, One>();
                container.RegisterSingle<ITwo, Two>();

                // Act
                container.GetInstance<IOne>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
            }
        }

        [TestMethod]
        public void GetInstance_RequestingTypeDependingIndirectlyOnItselfThroughDelegateRegistration_Throws()
        {
            try
            {
                // Arrange
                var container = new SimpleServiceLocator();

                container.Register<IOne>(() => new One(container.GetInstance<ITwo>()));
                container.Register<ITwo, Two>();

                // Act
                container.GetInstance<IOne>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
            }
        }

        [TestMethod]
        public void GetInstance_RequestingTypeDependingDirectlyOnItselfThroughDelegateRegistration_Throws()
        {
            try
            {
                // Arrange
                var container = new SimpleServiceLocator();

                container.Register<IOne>(() => container.GetInstance<IOne>());

                // Act
                container.GetInstance<IOne>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
            }
        }

        [TestMethod]
        public void GetInstance_CalledSimultaneouslyToRequestTheSameType_ShouldNotTriggerTheRecursionProtection()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.Register<IWeapon>(() =>
            {
                // We wait a bit here to make sure all three threads run this method simultaneously.
                Thread.Sleep(200);
                return new Katana();
            });
            
            // Act
            var thread2 = ThreadWrapper.StartNew(() => container.GetInstance<IWeapon>());
            var thread3 = ThreadWrapper.StartNew(() => container.GetInstance<IWeapon>());

            // Also run on this thread.
            container.GetInstance<IWeapon>();

            // Assert
            Assert_FinishedWithoutExceptions(thread2);
            Assert_FinishedWithoutExceptions(thread3);
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnRegisteredTypeWithFailingDelegate_WillAlwaysFailWithTheSameException()
        {
            // Arrange
            string expectedExceptionMessage = 
                "The registered delegate for type CuttingEdge.ServiceLocation.Tests.Unit.IWeapon threw an " + 
                "exception.";

            var container = new SimpleServiceLocator();

            // We let the registered delegate throw an exception.
            container.Register<IWeapon>(() => { throw new InvalidOperationException(); });

            try
            {
                container.GetInstance<IWeapon>();

                Assert.Fail("Test setup fail.");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains(expectedExceptionMessage), "Test setup failed.");
            }

            // Act
            try
            {
                container.GetInstance<IWeapon>();

                Assert.Fail("GetInstance was expected to throw an exception.");
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsTrue(ex.Message.Contains(expectedExceptionMessage), "The GetInstance is expected " +
                    "to always fail with the same exception. When this is not the case, this indicates " +
                    "that recursion detection went off. Actual message: " + ex.Message);
            }            
        }

        [TestMethod]
        public void GetInstance_RecursiveDependencyInTransientInitializer_ThrowsMeaningfulError()
        {
            // Arrange
            string expectedMessage = "The configuration is invalid. The type " + typeof(Samurai).FullName +
                " is directly or indirectly depending on itself.";

            var container = new SimpleServiceLocator();

            container.Register<IWeapon, Katana>();

            container.Register<Samurai>(createdSamurai =>
            {
                container.GetInstance<Samurai>();
            });

            try
            {
                // Act
                container.GetInstance<Samurai>();

                // Assert
                Assert.Fail("Verify is expected to throw an exception.");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains(expectedMessage), "The actual message did not contain " +
                    "the expected message. Actual: " + ex.Message);
            }
        }

        private static void Assert_FinishedWithoutExceptions(ThreadWrapper thread)
        {
            thread.Join();
        }

        #region Direct dependency

        private sealed class A
        {
            public A(B b)
            {
            }
        }

        private sealed class B
        {
            public B(A a)
            {
            }
        }

        #endregion

        #region Dependency through interface

        private sealed class One : IOne
        {
            public One(ITwo two)
            {
            }
        }

        private sealed class Two : ITwo
        {
            public Two(IOne one)
            {
            }
        }

        #endregion

        private sealed class ThreadWrapper
        {
            private readonly Thread thread;
            private Exception exception;

            public ThreadWrapper(Action action)
            {
                this.thread = new Thread(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        lock (this)
                        {
                            this.exception = ex;
                        }
                    }
                });
            }

            public static ThreadWrapper StartNew<T>(Func<T> action)
            {
                return StartNew(() => { action(); });
            }

            public static ThreadWrapper StartNew(Action action)
            {
                var thread = new ThreadWrapper(action);
                thread.Start();
                return thread;
            }

            public void Start()
            {
                this.thread.Start();
            }

            public void Join()
            {
                this.thread.Join();

                lock (this)
                {
                    if (this.exception != null)
                    {
                        throw this.exception;
                    }
                }
            }
        }
    }
}