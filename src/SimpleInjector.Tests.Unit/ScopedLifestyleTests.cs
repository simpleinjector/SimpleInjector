namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Lifestyles;

    [TestClass]
    public class ScopedLifestyleTests
    {
        private static readonly string CannotAccessADisposedObjectMessage;

        [DebuggerStepThrough]
        static ScopedLifestyleTests()
        {
            try
            {
                throw new ObjectDisposedException("Foo");
            }
            catch (ObjectDisposedException ex)
            {
                CannotAccessADisposedObjectMessage = ex.Message.Substring(0, ex.Message.IndexOf('.'));
            }
        }

        [TestMethod]
        public void Dispose_ListWithOneItem_DisposesItem()
        {
            // Arrange
            var scope = new Scope(new Container());

            var disposable = new DisposableObject();

            Assert.IsFalse(disposable.IsDisposedOnce, "Test setup");

            var disposables = new List<IDisposable> { disposable };

            scope.RegisterForDisposal(disposable);

            // Act
            scope.Dispose();

            // Assert
            Assert.IsTrue(disposable.IsDisposedOnce);
        }

        [TestMethod]
        public void Dispose_MultipleItems_DisposesAllItems()
        {
            // Arrange
            var scope = new Scope(new Container());

            var disposables = new List<DisposableObject>
            {
                new DisposableObject(),
                new DisposableObject(),
                new DisposableObject()
            };

            disposables.ForEach(scope.RegisterForDisposal);

            // Act
            scope.Dispose();

            // Assert
            Assert.IsTrue(disposables.All(d => d.IsDisposedOnce));
        }

        [TestMethod]
        public void Dispose_MultipleItems_DisposesAllItemsInReversedOrder()
        {
            // Arrange
            var scope = new Scope(new Container());

            var disposedItems = new List<DisposableObject>();

            var disposables = new List<DisposableObject>
            {
                new DisposableObject(disposedItems.Add),
                new DisposableObject(disposedItems.Add),
                new DisposableObject(disposedItems.Add)
            };

            disposables.ForEach(scope.RegisterForDisposal);

            // Act
            scope.Dispose();

            disposedItems.Reverse();

            // Assert
            Assert.IsTrue(disposedItems.SequenceEqual(disposables));
        }

        [TestMethod]
        public void Dispose_WithMultipleItemsThatThrow_StillDisposesAllItems()
        {
            // Arrange
            var scope = new Scope(new Container());

            var disposables = new List<DisposableObject>
            {
                new DisposableObject(new Exception()),
                new DisposableObject(new Exception()),
                new DisposableObject(new Exception())
            };

            disposables.ForEach(scope.RegisterForDisposal);

            try
            {
                // Act
                scope.Dispose();

                // Assert
                Assert.Fail("Exception was expected to bubble up.");
            }
            catch
            {
                Assert.IsTrue(disposables.All(d => d.IsDisposedOnce));
            }
        }

        [TestMethod]
        public void Dispose_WithMultipleItemsThatThrow_WillBubbleUpTheLastThrownException()
        {
            // Arrange
            var scope = new Scope(new Container());

            Exception lastThrownException = new Exception();

            var disposables = new List<DisposableObject>
            { 
                // Since the objects are disposed in reverse order, the first object is disposed last, and
                // this exception is expected to bubble up.
                new DisposableObject(exceptionToThrow: lastThrownException),
                new DisposableObject(exceptionToThrow: new Exception()),
                new DisposableObject(exceptionToThrow: new Exception()),
                new DisposableObject(),
                new DisposableObject(exceptionToThrow: new Exception()),
                new DisposableObject(exceptionToThrow: new Exception())
            };

            disposables.ForEach(scope.RegisterForDisposal);

            try
            {
                // Act
                scope.Dispose();

                // Assert
                Assert.Fail("Exception was expected to bubble up.");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(object.ReferenceEquals(lastThrownException, ex));
            }
        }

        [TestMethod]
        public void Dispose_RegisteredActionAndRegisteredDisposableObject_CallsActionFirst()
        {
            // Arrange
            int actionCount = 0;
            int disposeCount = 0;

            var scope = new Scope(new Container());

            scope.RegisterForDisposal(new DisposableObject(_ =>
            {
                Assert.AreEqual(2, actionCount);
                disposeCount++;
            }));

            scope.WhenScopeEnds(() =>
            {
                Assert.AreEqual(0, disposeCount);
                actionCount++;
            });

            scope.WhenScopeEnds(() =>
            {
                Assert.AreEqual(0, disposeCount);
                actionCount++;
            });

            scope.RegisterForDisposal(new DisposableObject(_ =>
            {
                Assert.AreEqual(2, actionCount);
                disposeCount++;
            }));

            // Act
            scope.Dispose();

            // Assert
            Assert.AreEqual(2, actionCount);
            Assert.AreEqual(2, disposeCount);
        }

        [TestMethod]
        public void Dispose_WithThrowingAction_StillDisposesInstance()
        {
            // Arrange
            bool disposed = false;

            var scope = new Scope(new Container());

            scope.WhenScopeEnds(() =>
            {
                throw new Exception();
            });

            scope.RegisterForDisposal(new DisposableObject(_ =>
            {
                disposed = true;
            }));

            try
            {
                // Act
                scope.Dispose();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch
            {
                Assert.IsTrue(disposed);
            }
        }

        [TestMethod]
        public void Dispose_ScopedItemResolvedDuringScopeEndAction_GetsDisposed()
        {
            // Arrange
            var disposable = new DisposableObject();

            var scope = new Scope(new Container());

            var container = new Container();

            container.Register<DisposableObject>(() => disposable, new FakeScopedLifestyle(scope));

            scope.WhenScopeEnds(() => container.GetInstance<DisposableObject>());

            // Act
            scope.Dispose();

            // Assert
            Assert.IsTrue(disposable.IsDisposedOnce);
        }

        [TestMethod]
        public void Dispose_WhenDisposeEndsActionRegisteredDuringDisposal_CallsTheRegisteredDelegate()
        {
            // Arrange
            bool actionCalled = false;

            var disposable = new DisposableObject();

            var scope = new Scope(new Container());

            var container = new Container();

            container.Register<DisposableObject>(() => disposable, new FakeScopedLifestyle(scope));
            container.RegisterInitializer<DisposableObject>(instance =>
            {
                scope.WhenScopeEnds(() => actionCalled = true);
            });

            scope.WhenScopeEnds(() => container.GetInstance<DisposableObject>());

            // Act
            scope.Dispose();

            // Assert
            Assert.IsTrue(actionCalled);
        }

        [TestMethod]
        public void RegisterForDisposal_CalledOnDisposedScope_ThrowsObjectDisposedException()
        {
            // Arrange
            var scope = new Scope(new Container());

            scope.Dispose();

            // Act
            Action action = () => scope.RegisterForDisposal(new DisposableObject());

            // Assert
            AssertThat.Throws<ObjectDisposedException>(action,
                "Calling RegisterForDisposal should throw an ObjectDisposedException.");
        }

        [TestMethod]
        public void WhenScopeEnds_CalledOnDisposedScope_ThrowsObjectDisposedException()
        {
            // Arrange
            var scope = new Scope(new Container());

            scope.Dispose();

            // Act
            Action action = () => scope.WhenScopeEnds(() => { });

            // Assert
            AssertThat.Throws<ObjectDisposedException>(action,
                "Calling WhenScopeEnds should throw an ObjectDisposedException.");
        }

        [TestMethod]
        public void Dispose_ScopedInstanceResolvedDuringDisposingScope_CreatesInstanceWithExpectedLifestyle()
        {
            // Arrange
            IPlugin plugin1 = null;
            IPlugin plugin2 = null;

            var container = new Container();

            var scope = new Scope(container);

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            container.Register<IPlugin>(() => new DisposablePlugin(), scopedLifestyle);

            container.Register<DisposableObject>(() => new DisposableObject(disposing: _ =>
            {
                plugin1 = container.GetInstance<IPlugin>();
                plugin2 = container.GetInstance<IPlugin>();
            }), scopedLifestyle);

            container.GetInstance<DisposableObject>();

            // Act
            scope.Dispose();

            // Assert
            Assert.IsNotNull(plugin1);
            Assert.AreSame(plugin1, plugin2);
        }

        [TestMethod]
        public void Dispose_ScopedInstanceResolvedDuringDisposingScope_DisposesThisInstanceLast()
        {
            // Arrange
            var disposedObjects = new List<object>();

            var container = new Container();

            var scope = new Scope(container);

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            container.Register<IPlugin>(() => new DisposablePlugin(disposedObjects.Add), scopedLifestyle);
            container.Register<IDisposable>(() => new DisposableObject(disposedObjects.Add), scopedLifestyle);
            container.Register<DisposableObject>(() => new DisposableObject(_ =>
            {
                container.GetInstance<IPlugin>();
            }), scopedLifestyle);

            container.GetInstance<IDisposable>();
            container.GetInstance<DisposableObject>();

            // Act
            scope.Dispose();

            // Assert
            AssertThat.IsInstanceOfType(typeof(DisposablePlugin), disposedObjects.Last(), "Since the disposable logger is requested during disposal of the scope, it should be " +
                "disposed after all other already created services have been disposed. " +
                "In the given case, disposing DisposableLogger before the IDisposable registration becomes " +
                "a problem when that registration starts using ILogger in its dispose method as well.");
        }

        [TestMethod]
        public void Dispose_InstanceResolvedDuringDisposingScopeRegisteringEndAction_CallsThisAction()
        {
            // Arrange
            bool actionCalled = false;

            var container = new Container();

            var scope = new Scope(container);

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            container.Register<DisposableObject>(() => new DisposableObject(_ =>
            {
                container.GetInstance<IPlugin>();
            }), scopedLifestyle);

            container.Register<IPlugin>(() =>
            {
                scope.WhenScopeEnds(() => actionCalled = true);
                return new DisposablePlugin();
            }, Lifestyle.Transient);

            container.GetInstance<DisposableObject>();

            // Act
            scope.Dispose();

            // Assert
            Assert.IsTrue(actionCalled);
        }

        [TestMethod]
        public void Dispose_ExceptionThrownDuringDisposalAfterResolvingNewInstance_DisposesThatInstance()
        {
            // Arrange
            bool newlyResolvedInstanceDisposed = false;

            var container = new Container();

            var scope = new Scope(container);

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            container.Register<IPlugin>(
                () => new DisposablePlugin(disposing: _ => newlyResolvedInstanceDisposed = true),
                scopedLifestyle);

            container.Register<DisposableObject>(() => new DisposableObject(disposing: _ =>
                {
                    container.GetInstance<IPlugin>();
                    throw new Exception("Bang!");
                }), scopedLifestyle);

            container.GetInstance<DisposableObject>();

            try
            {
                // Act
                scope.Dispose();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch
            {
                Assert.IsTrue(newlyResolvedInstanceDisposed);
            }
        }

        [TestMethod]
        public void Dispose_ExceptionThrownDuringDisposalAfterResolvingNewInstance_DoesNotCallAnyNewActions()
        {
            // Arrange
            bool scopeEndActionCalled = false;

            var container = new Container();

            var scope = new Scope(container);

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            container.Register<IPlugin>(() => new DisposablePlugin());
            container.RegisterInitializer<IPlugin>(
                plugin => scope.WhenScopeEnds(() => scopeEndActionCalled = true));

            container.Register<DisposableObject>(() => new DisposableObject(_ =>
            {
                container.GetInstance<IPlugin>();
                throw new Exception("Bang!");
            }), scopedLifestyle);

            try
            {
                // Act
                scope.Dispose();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch
            {
                Assert.IsFalse(scopeEndActionCalled,
                    "In case of an exception, no actions will be further executed. This lowers the change " +
                    "the new exceptions are thrown from other actions that cover up the original exception.");
            }
        }

        [TestMethod]
        public void Dispose_RecursiveResolveTriggeredInDispose_ThrowsDescriptiveException()
        {
            // Arrange
            var container = new Container();

            var scope = new Scope(container);

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            container.Register<IPlugin>(() =>
            {
                var plugin = new DisposablePlugin(disposing: _ =>
                {
                    // Recursive dependency
                    // Although really bad practice, this must not cause an infinit spin or a stackoverflow.
                    container.GetInstance<IPlugin>();
                });

                scope.RegisterForDisposal(plugin);

                return plugin;
            });

            container.GetInstance<IPlugin>();

            // Act
            Action action = () => scope.Dispose();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The registered delegate for type IPlugin threw an exception. A recursive registration of 
                Action or IDisposable instances was detected during disposal of the scope. 
                This is possibly caused by a component that is directly or indirectly depending on itself"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void Dispose_RecursiveResolveTriggeredDuringEndScopeAction_ThrowsDescriptiveException()
        {
            // Arrange
            var container = new Container();

            var scope = new Scope(container);

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            container.Register<IPlugin>(() =>
            {
                scope.WhenScopeEnds(() =>
                {
                    container.GetInstance<IPlugin>();
                });

                return new DisposablePlugin();
            });

            container.GetInstance<IPlugin>();

            // Act
            Action action = () => scope.Dispose();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The registered delegate for type IPlugin threw an exception. A recursive registration of 
                Action or IDisposable instances was detected during disposal of the scope. 
                This is possibly caused by a component that is directly or indirectly depending on itself"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void Dispose_RecursiveResolveTriggeredDuringEndScopeAction_StillDisposesRegisteredDisposables()
        {
            // Arrange
            var container = new Container();

            var scope = new Scope(container);

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            var disposable = new DisposableObject();

            scope.RegisterForDisposal(disposable);

            container.Register<IPlugin>(() =>
            {
                scope.WhenScopeEnds(() =>
                {
                    container.GetInstance<IPlugin>();
                });

                return new DisposablePlugin();
            });

            container.GetInstance<IPlugin>();

            try
            {
                // Act
                scope.Dispose();

                Assert.Fail("Exception expected.");
            }
            catch
            {
                // Assert
                Assert.IsTrue(disposable.IsDisposedOnce,
                    "Even though there was a recursion detected when executing Action delegates, all " +
                    "registered disposables should still get disposed.");
            }
        }

        [TestMethod]
        public void GetInstance_CalledOnADisposedScope_ThrowsObjectDisposedException()
        {
            // Arrange
            var container = new Container();

            var scope = new Scope(container);

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            container.Register<IPlugin>(() => new DisposablePlugin(), scopedLifestyle);

            container.GetInstance<IPlugin>();

            scope.Dispose();

            // Act
            Action action = () => container.GetInstance<IPlugin>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                CannotAccessADisposedObjectMessage,
                action);
        }

        [TestMethod]
        public void GetCurrentScope_CalledOutsideOfVerification_ReturnsNull()
        {
            // Arrange
            var scopedLifestyle = new FakeScopedLifestyle(scope: null);

            // Act
            var actualScope = scopedLifestyle.GetCurrentScope(new Container());

            // Assert
            Assert.IsNull(actualScope);
        }

        [TestMethod]
        public void Verify_VerifyingAScopedInstance_DisposesThatInstance()
        {
            // Arrange
            var disposable = new DisposableObject();

            var scopedLifestyle = new FakeScopedLifestyle(scope: null);

            var container = new Container();

            container.Register<IDisposable>(() => disposable, scopedLifestyle);

            // Act
            container.Verify();

            // Assert
            Assert.IsTrue(disposable.IsDisposedOnce);
        }

        [TestMethod]
        public void GetCurrentScope_CalledDuringVerification_ReturnsAScope()
        {
            // Arrange
            Scope scopeDuringVerification = null;

            var scopedLifestyle = new FakeScopedLifestyle(scope: null);

            var container = new Container();

            container.Register<IDisposable>(() => new DisposableObject(), scopedLifestyle);

            container.RegisterInitializer<IDisposable>(_ =>
            {
                scopeDuringVerification = scopedLifestyle.GetCurrentScope(container);
            });

            // Act
            container.Verify();

            // Assert
            Assert.IsNotNull(scopeDuringVerification);
        }

        [TestMethod]
        public void Verify_RegisterForDisposalCalledDuringVerification_EnsuresDisposalOfRegisteredDisposable()
        {
            // Arrange
            var disposable = new DisposableObject();

            var scopedLifestyle = new FakeScopedLifestyle(scope: null);

            var container = new Container();

            container.Register<IPlugin>(() => new DisposablePlugin());

            container.RegisterInitializer<IPlugin>(_ =>
            {
                scopedLifestyle.RegisterForDisposal(container, disposable);
            });

            // Act
            container.Verify();

            // Assert
            Assert.IsTrue(disposable.IsDisposedOnce);
        }

        [TestMethod]
        public void Verify_WhenScopeEndsCalledDuringVerification_WontCallActionWhenFinished()
        {
            // Arrange
            bool actionCalled = false;

            var scopedLifestyle = new FakeScopedLifestyle(scope: null);

            var container = new Container();

            container.Register<IPlugin>(() => new DisposablePlugin());

            container.RegisterInitializer<IPlugin>(_ =>
            {
                scopedLifestyle.WhenScopeEnds(container, () => actionCalled = true);
            });

            // Act
            container.Verify();

            // Assert
            Assert.IsFalse(actionCalled,
                "Any registered action delegates should not be called during verification " +
                "(but registration should succeed). Users are allowed to do any ");
        }

        [TestMethod]
        public void Verify_ExecutedWithinActiveScope_CreatesScopedInstancesInItsOwnScope()
        {
            // Arrange
            DisposablePlugin plugin = null;

            var container = new Container();

            var scopedLifestyle = new FakeScopedLifestyle(new Scope(container));

            container.Register<ServiceDependingOn<IPlugin>>();
            container.Register<IPlugin, DisposablePlugin>(scopedLifestyle);
            container.RegisterInitializer<DisposablePlugin>(p => plugin = p);

            // By resolving IPlugin here, it becomes cached within the created Scope.
            container.GetInstance<IPlugin>();
            plugin = null;

            // Act
            // When calling verify, we expect DisposablePlugin to be created again.
            container.Verify();

            // Assert
            Assert.IsNotNull(plugin);
        }

        [TestMethod]
        public void GetCurrentScope_CalledOnSameThreadDuringVerification_ReturnsTheVerificationScope()
        {
            // Arrange
            Scope verificationScope = null;

            var container = new Container();

            var scope = new Scope(container);
            var scopedLifestyle = new FakeScopedLifestyle(scope);

            container.Register<IPlugin, DisposablePlugin>(scopedLifestyle);
            container.RegisterInitializer<DisposablePlugin>(p =>
            {
                verificationScope = scopedLifestyle.GetCurrentScope(container);
            });

            // Act
            // When calling verify, we expect DisposablePlugin to be created again.
            container.Verify();

            // Assert
            Assert.AreNotSame(verificationScope, scope);
            Assert.IsTrue(verificationScope.ToString().Contains("VerificationScope"));
        }

        [TestMethod]
        public void Verify_ExecutedWithinActiveScope_DisposesThatInstanceWhenVerifyFinishes()
        {
            // Arrange
            DisposablePlugin plugin = null;

            var container = new Container();

            var lifestyle = new FakeScopedLifestyle(new Scope(container));

            container.Register<IPlugin, DisposablePlugin>(lifestyle);
            container.RegisterInitializer<DisposablePlugin>(p => plugin = p);

            // Act
            container.Verify();

            // Assert
            Assert.IsNotNull(plugin);
            Assert.IsTrue(plugin.IsDisposedOnce);
        }

        [TestMethod]
        public void GetInstance_ResolvingScopedInstanceWhileDifferentThreadIsVerifying_DoesNotResolveInstanceFromVerificationScope()
        {
            // Arrange
            DisposablePlugin verifiedPlugin = null;
            DisposablePlugin backgroundResolvedPlugin = null;
            Task task = null;

            var container = new Container();

            var scope = new Scope(container);
            var lifestyle = new FakeScopedLifestyle(scope);

            container.Register<IPlugin, DisposablePlugin>(lifestyle);
            container.RegisterInitializer<DisposablePlugin>(p =>
            {
                verifiedPlugin = p;

                // Resolve on a different thread (so not during verification)
                task = Task.Run(() =>
                {
                    backgroundResolvedPlugin = (DisposablePlugin)container.GetInstance<IPlugin>();
                });

                Thread.Sleep(150);
            });

            // Act
            container.Verify();

            task.Wait();

            // Assert
            Assert.IsFalse(backgroundResolvedPlugin.IsDisposedOnce,
                "Since this instance isn't resolved during verification, but within an active scope, " +
                "The instance should not have been disposed here.");

            scope.Dispose();

            Assert.IsTrue(backgroundResolvedPlugin.IsDisposedOnce, "Now it should have been disposed.");
        }

        [TestMethod]
        public void GetCurrentScope_CalledDuringVerificationOnADifferentThread_ReturnsARealScope()
        {
            // Arrange
            Scope backgroundRequestedScope = null;
            Task task = null;

            var container = new Container();

            var scope = new Scope(container);
            var lifestyle = new FakeScopedLifestyle(scope);

            container.Register<IPlugin, DisposablePlugin>(lifestyle);
            container.RegisterInitializer<DisposablePlugin>(p =>
            {
                // Resolve on a different thread (so not during verification)
                task = Task.Run(() =>
                {
                    backgroundRequestedScope = lifestyle.GetCurrentScope(container);
                });

                Thread.Sleep(150);
            });

            // Act
            container.Verify();

            task.Wait();

            // Assert
            Assert.AreSame(scope, backgroundRequestedScope,
                "Since the background thread does not run verify, the returned scope should not be the " +
                "verification scope.");
        }

        [TestMethod]
        public void Verify_TransientServiceDependingOnScope_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ServiceDependingOn<Scope>>(Lifestyle.Transient);

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_ScopedServiceDependingOnScope_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ServiceDependingOn<Scope>>(new ThreadScopedLifestyle());

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_SingletonServiceDependingOnScope_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ServiceDependingOn<Scope>>(Lifestyle.Singleton);

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(
                "ServiceDependingOn<Scope> (Singleton) depends on Scope (Scoped)", action);
        }

        [TestMethod]
        public void GetInstance_SingletonServiceDependingOnScope_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = new FakeScopedLifestyle(new Scope(container));

            container.Register<ServiceDependingOn<Scope>>(Lifestyle.Singleton);

            // Act
            Action action = () => container.GetInstance<ServiceDependingOn<Scope>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "ServiceDependingOn<Scope> (Singleton) depends on Scope (Scoped)", action);
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInstanceDependingOnScopeWithAnActiveLifetimeScopeAndDefaultScopedLifestyleSet_InjectsTheActiveScope()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            Scope activeScope = ThreadScopedLifestyle.BeginScope(container);

            // Act
            Scope injectedScope = container.GetInstance<ServiceDependingOn<Scope>>().Dependency;

            // Assert
            Assert.AreSame(activeScope, injectedScope);
        }

        [TestMethod]
        public void GetInstance_RequestingScopeWithActiveLifetimeScopeAndDefaultScopedLifestyleSet_ResolvesTheActiveScope()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            Scope activeScope = ThreadScopedLifestyle.BeginScope(container);

            // Act
            var resolvedScope = container.GetInstance<Scope>();

            // Assert
            Assert.AreSame(activeScope, resolvedScope);
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInstanceDependingOnScopeWithoutAnActiveScopeAndDefaultScopedLifestyleSet_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            // Act
            Action action = () => container.GetInstance<ServiceDependingOn<Scope>>();

            // Act
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>("There is no active scope",
                action);
        }

        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior()
        {
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            container.Register<ServiceDependingOn<Scope>>();

            // Act
            container.Verify();
        }

        private class DisposablePlugin : IPlugin, IDisposable
        {
            private readonly Action<DisposablePlugin> disposing;

            public DisposablePlugin()
            {
            }

            internal DisposablePlugin(Action<DisposablePlugin> disposing = null)
            {
                this.disposing = disposing;
            }

            public int DisposeCount { get; private set; }

            public bool IsDisposedOnce => this.DisposeCount == 1;

            public void Dispose()
            {
                this.DisposeCount++;

                this.disposing?.Invoke(this);
            }
        }

        private sealed class DisposableObject : IDisposable
        {
            private Action<DisposableObject> disposing;

            public DisposableObject()
            {
            }

            public DisposableObject(Action<DisposableObject> disposing)
            {
                this.disposing = disposing;
            }

            public DisposableObject(Exception exceptionToThrow)
            {
                this.ExceptionToThrow = exceptionToThrow;
            }

            public Exception ExceptionToThrow { get; }

            public int DisposeCount { get; private set; }

            public bool IsDisposedOnce => this.DisposeCount == 1;

            public void Dispose()
            {
                this.DisposeCount++;

                this.disposing?.Invoke(this);

                if (this.ExceptionToThrow != null)
                {
                    throw this.ExceptionToThrow;
                }
            }
        }

        private sealed class FakeScopedLifestyle : ScopedLifestyle
        {
            private readonly Scope scope;

            public FakeScopedLifestyle(Scope scope)
                : base("Fake Scope")
            {
                this.scope = scope;
            }

            protected internal override Func<Scope> CreateCurrentScopeProvider(Container c) => () => this.scope;
        }
    }
}