namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CyclicDependencyValidatorTests
    {
        public interface INode
        {
        }

        public interface INodeFactory
        {
        }

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
                The configuration is invalid. The type CyclicDependencyValidatorTests.A is directly or
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
            container.Register<B>(Lifestyle.Singleton);

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

            container.Register<A>(Lifestyle.Singleton);

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
            container.Register<A>(Lifestyle.Singleton);
            container.Register<B>(Lifestyle.Singleton);

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
            container.Register<IOne, One>(Lifestyle.Singleton);
            container.Register<ITwo, Two>(Lifestyle.Singleton);

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
        public void Verify_DelegateRegistrationDependingOnItself_Throws()
        {
            // Arrange
            var container = new Container();

            // One depends on ITwo
            container.Register<One>();

            int count = 0;

            container.Register<ITwo>(() =>
            {
                if (count++ > 5)
                {
                    Assert.Fail("Stack overflow detected");
                }

                return container.GetInstance<ITwo>();
            });

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>("depending on itself", action);
        }
        
        [TestMethod]
        public void GetInstance_DelegateRegistrationDependingOnItself_Throws()
        {
            // Arrange
            var container = new Container();

            // One depends on ITwo
            container.Register<One>();

            int count = 0;

            container.Register<ITwo>(() =>
            {
                if (count++ > 5)
                {
                    Assert.Fail("Stack overflow detected");
                }

                return container.GetInstance<ITwo>();
            });

            // Act
            Action action = () => container.GetInstance<One>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>("depending on itself", action);
        }
        
        [TestMethod]
        public void GetInstance_DelegateRegistrationDependingIndirectlyOnItselfThroughRootType_Throws()
        {
            // Arrange
            var container = new Container();

            // One depends on ITwo
            container.Register<One>();

            int count = 0;

            container.Register<ITwo>(() =>
            {
                if (count++ > 5)
                {
                    Assert.Fail("Stack overflow detected");
                }

                return new Two(container.GetInstance<One>());
            });

            container.GetRegistration(typeof(One)).BuildExpression();

            // Act
            Action action = () => container.GetInstance<One>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>("depending on itself", action);
        }

        [TestMethod]
        public void GetInstance_RequestingRootTypeDependingIndirectlyOnItselfThroughDelegateRegistration_Throws()
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
        public void GetInstance_RequestingOnRootTypeDependingIndirectlyOnItselfThroughDelegateRegistration_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ComponentDependingOn<IOne>>();
            container.Register<IOne>(() => new One(container.GetInstance<ITwo>()));
            container.Register<ITwo>(() => new Two(container.GetInstance<IOne>()));

            try
            {
                // Act
                container.GetInstance<ComponentDependingOn<IOne>>();

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
            container.Register<ITimeProvider>(() => { throw new NullReferenceException(); }, Lifestyle.Singleton);

            Action arrangeAction = () => container.GetInstance<ITimeProvider>();

            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(expectedMessage, arrangeAction, 
                "Test setup failed.");

            // Act
            Action action = () => container.GetInstance<ITimeProvider>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(expectedMessage, action,
                    "Repeating calls to a failing FuncSingletonInstanceProducer should result in the same " +
                    "exception being thrown every time.");
        }

        [TestMethod]
        public void IteratingOverAnCollectionOfServices_ElementOfTheCollectionDependsOnTheCollection_ThrowsExpectedException()
        {
            // Arrange
            CompositeService.ResetStackoverflowProtection();

            var container = new Container();

            container.Register<IService, CompositeService>(Lifestyle.Singleton);

            // CompositeService is also part of the collection making it indirectly depending on itself.
            container.RegisterCollection<IService>(new[] { typeof(Service), typeof(CompositeService) });

            // Act
            Action action = () => container.GetAllInstances<IService>().ToArray();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "The configuration is invalid. The type CyclicDependencyValidatorTests.CompositeService " + 
                "is directly or indirectly depending on itself.",
                action);
        }

        [TestMethod]
        public void Verify_DelayedCyclicReference_ShouldSucceed()
        {
            // Arrange
            var container = new Container();

            // NodeOne depends on INodeFactory
            container.RegisterCollection<INode>(new[] { typeof(NodeOne) });

            // NodeFactory depends on IEnumerable<INode>
            container.Register<INodeFactory, NodeFactory>();

            // Act
            // Simple Injector's goal is to prevent stack overflow exceptions when building up object graphs. 
            // Since the creation of the INode types are delayed since an IEnumerable<T> is injected into the 
            // NodeFactory (note that injecting an IEnumerable<T> does trigger the creation of its 
            // instances; iterating the collection does), this can be revolved fine and there will be no
            // stack overflow. It is therefore not Simple Injector's job to disallow such construct. Having the 
            // circular reference in the code might be a problem, but the design might also be intentional and
            // could work just fine. Simple Injector should allow this.
            container.Verify();

            // Extra check in case Verify doesn't do its job.
            container.GetAllInstances<INode>().ToArray();
        }

        // NOTE: In previous versions the .Verify() in this test would not throw an exception, but a bug fix
        // caused this behavior to change and now an exception is thrown.
        [TestMethod]
        public void Verify_DelayedCyclicReferenceWithDecorator_StillThrowsException()
        {
            // Arrange
            var container = new Container();

            container.Register<NodeOne>();

            // NodeOne depends on INodeFactory
            container.RegisterCollection<INode>(new[] { typeof(NodeOne) });

            // With the previous releases of Simple Injector the use of a decorator would cause collections to
            // be built less lazily, i.e. all collection elements would be compiled upon injection. This would
            // cause the Verify complain about the cyclic reference, which should not happen. This is why we
            // have this extra test with the decorator. For more information about why this should succeed,
            // see the comment on the previous test.
            container.RegisterDecorator(typeof(INode), typeof(NodeDecorator));

            // NodeFactory depends on IEnumerable<INode>
            container.Register<INodeFactory, NodeFactory>();

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "NodeOne is directly or indirectly depending on itself.",
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInstanceWithACyclicDependencyGraph_ThrowsAnExceptionDescribingTheExactCycle()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IX, XDependingOn<ServiceDependingOn<IY>>>();
            container.Register<IY, YDependingOn<ServiceDependingOn<IX>>>();

            // Act
            Action action = () => container.GetInstance<ComponentDependingOn<IX>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                   XDependingOn<ServiceDependingOn<IY>> 
                -> ServiceDependingOn<IY> 
                -> YDependingOn<ServiceDependingOn<IX>>
                -> ServiceDependingOn<IX>
                -> XDependingOn<ServiceDependingOn<IY>>"
                .TrimInside(),
                action);
        }

        private static void Assert_FinishedWithoutExceptions(ThreadWrapper thread)
        {
            thread.Join();
        }

        #region Composite dependency

        public abstract class IService
        {
        }

        public class Service : IService
        {
        }

        public class CompositeService : IService
        {
            [ThreadStatic]
            private static int recursionCountDown;

            public CompositeService(IEnumerable<IService> availableServices)
            {
                recursionCountDown--;

                if (recursionCountDown <= 0)
                {
                    throw new InvalidOperationException("Recursive operation detected :-(.");
                }

                availableServices.ToArray();
            }

            public static void ResetStackoverflowProtection()
            {
                recursionCountDown = 100;
            }
        }

        #endregion

        #region Cyclic Dependency Through IEnumerable<T>

        public class NodeOne : INode
        {
            public NodeOne(INodeFactory nodeFactory)
            {
            }
        }

        public class NodeDecorator : INode
        {
            public NodeDecorator(INode node)
            {
            }
        }

        public class NodeFactory : INodeFactory
        {
            public NodeFactory(IEnumerable<INode> nodes)
            {
            }
        }

        #endregion

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

            public static ThreadWrapper StartNew<T>(Func<T> action) => StartNew(() => { action(); });

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

    public interface IX
    {
    }

    public interface IY
    {
    }

    public class ServiceDependingOn<TDependency>
    {
        public readonly TDependency Dependency;

        public ServiceDependingOn(TDependency dependency)
        {
            this.Dependency = dependency;
        }
    }

    public class XDependingOn<TDependency> : IX
    {
        public XDependingOn(TDependency dependency)
        {
        }
    }

    public class YDependingOn<TDependency> : IY
    {
        public YDependingOn(TDependency dependency)
        {
        }
    }
}