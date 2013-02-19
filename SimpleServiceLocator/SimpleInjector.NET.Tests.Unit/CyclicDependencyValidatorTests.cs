namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            // Arrange
            string expectedExcpetionMessage = @"
                The configuration is invalid. The type CyclicDependencyValidatorTests+A is directly or
                indirectly depending on itself."
                .TrimInside();

            var container = ContainerFactory.New();

            try
            {
                // Act
                // Note: A depends on B which depends on A.
                container.GetInstance<A>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(expectedExcpetionMessage, ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_RequestingTransientTypeDependingIndirectlyOnItself_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                // Act
                // Note: A depends on B which depends on A.
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
            var container = ContainerFactory.New();

            try
            {
                // Note: A depends on B which depends on A.
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
                // This exception is expected.
            }
        }

        [TestMethod]
        public void GetInstance_RequestingTransientTypeDependingIndirectlyOnItselfViaASingletonType_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Note: A depends on B which depends on A.
            container.RegisterSingle<B>();

            try
            {
                // Act
                container.GetInstance<A>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
                // This exception is expected.
            }
        }

        [TestMethod]
        public void GetInstance_RequestingSingletonTypeDependingIndirectlyOnItselfViaTransientType_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<A>();

            try
            {
                // Act
                // Note: A depends on B which depends on A.
                container.GetInstance<A>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
                // This exception is expected.
            }
        }

        [TestMethod]
        public void GetInstance_RequestingSingletonTypeDependingIndirectlyOnItselfViaSingletonType_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Note: A depends on B which depends on A.
            container.RegisterSingle<A>();
            container.RegisterSingle<B>();

            try
            {
                // Act
                container.GetInstance<A>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
                // This exception is expected.
            }
        }

        [TestMethod]
        public void GetInstance_RequestingTransientTypeDependingIndirectlyOnItselfThroughInterfaces_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            // One depends on ITwo and Two depends on IOne.
            container.Register<IOne, One>();
            container.Register<ITwo, Two>();

            try
            {
                // Act
                container.GetInstance<IOne>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
                // This exception is expected.
            }
        }

        [TestMethod]
        public void GetInstance_RequestingSingletonTypeDependingIndirectlyOnItselfThroughInterfaces_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            // One depends on ITwo and Two depends on IOne.
            container.RegisterSingle<IOne, One>();
            container.RegisterSingle<ITwo, Two>();

            try
            {
                // Act
                container.GetInstance<IOne>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
                // This exception is expected.
            }
        }

        [TestMethod]
        public void GetInstance_RequestingTypeDependingIndirectlyOnItselfThroughDelegateRegistration_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IOne>(() => new One(container.GetInstance<ITwo>()));
            container.Register<ITwo, Two>();

            try
            {
                // Act
                container.GetInstance<IOne>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
                // This exception is expected.
            }
        }

        [TestMethod]
        public void GetInstance_RequestingTypeDependingDirectlyOnItselfThroughDelegateRegistration_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IOne>(() => container.GetInstance<IOne>());

            try
            {
                // Act
                container.GetInstance<IOne>();

                // Assert
                Assert.Fail("An exception was expected, because A depends indirectly on itself.");
            }
            catch (ActivationException)
            {
                // This exception is expected.
            }
        }

        [TestMethod]
        public void GetInstance_CalledSimultaneouslyToRequestTheSameType_ShouldNotTriggerTheRecursionProtection()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IUserRepository>(() =>
            {
                // We wait a bit here to make sure all three threads run this method simultaneously.
                Thread.Sleep(200);
                return new SqlUserRepository();
            });

            // Act
            var thread2 = ThreadWrapper.StartNew(() => container.GetInstance<IUserRepository>());
            var thread3 = ThreadWrapper.StartNew(() => container.GetInstance<IUserRepository>());

            // Also run on this thread.
            container.GetInstance<IUserRepository>();

            // Assert
            Assert_FinishedWithoutExceptions(thread2);
            Assert_FinishedWithoutExceptions(thread3);
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnRegisteredTypeWithFailingDelegate_WillAlwaysFailWithTheSameException()
        {
            // Arrange
            string expectedExceptionMessage =
                "The registered delegate for type IUserRepository threw an exception.";

            var container = ContainerFactory.New();

            // We let the registered delegate throw an exception.
            container.Register<IUserRepository>(() => { throw new InvalidOperationException(); });

            try
            {
                container.GetInstance<IUserRepository>();

                Assert.Fail("Test setup fail.");
            }
            catch (Exception ex)
            {
                AssertThat.StringContains(expectedExceptionMessage, ex.Message, "Test setup failed.");
            }

            // Act
            try
            {
                container.GetInstance<IUserRepository>();

                Assert.Fail("GetInstance was expected to throw an exception.");
            }
            catch (Exception ex)
            {
                // Assert
                AssertThat.StringContains(expectedExceptionMessage, ex.Message, "The GetInstance is " +
                    "expected to always fail with the same exception. When this is not the case, this " +
                    "indicates that recursion detection went off.");
            }
        }

        [TestMethod]
        public void GetInstance_RecursiveDependencyInTransientInitializer_ThrowsMeaningfulError()
        {
            // Arrange
            string expectedMessage = "The configuration is invalid. The type RealUserService " +
                "is directly or indirectly depending on itself.";

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            container.RegisterInitializer<RealUserService>(instanceToInitialize =>
            {
                container.GetInstance<RealUserService>();
            });

            try
            {
                // Act
                container.GetInstance<RealUserService>();

                // Assert
                Assert.Fail("Verify is expected to throw an exception.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnAnInvalidRegistrationOfSingletonFunc_ShouldNeverTriggerACyclicDependencyError()
        {
            // Arrange
            string expectedMessage =
                "The registered delegate for type ITimeProvider threw an exception.";

            var container = ContainerFactory.New();

            // This registration will use a FuncSingletonInstanceProducer under the covers.
            container.RegisterSingle<ITimeProvider>(() => { throw new NullReferenceException(); });

            try
            {
                container.GetInstance<ITimeProvider>();

                Assert.Fail("Test setup failed: Exception was expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(expectedMessage, ex.Message, "Test setup failed.");
            }

            try
            {
                // Act
                container.GetInstance<ITimeProvider>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(expectedMessage, ex.Message,
                    "Repeating calls to a failing FuncSingletonInstanceProducer should result in the same " +
                    "exception being thrown every time.");
            }
        }

        private static void Assert_FinishedWithoutExceptions(ThreadWrapper thread)
        {
            thread.Join();
        }

        #region Direct dependency

        public sealed class A
        {
            public A(B b)
            {
            }
        }

        public sealed class B
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