namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScopedLifestyleTests
    {
        [TestMethod]
        public void Dispose_ListWithOneItem_DisposesItem()
        {
            // Arrange
            var scope = new Scope();

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
            var scope = new Scope();

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
            var scope = new Scope();

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
            var scope = new Scope();

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
            var scope = new Scope();

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

            var scope = new Scope();
            
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

            var scope = new Scope();

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
        public void DisposeInstances_WithNullArgument_ThrowsExpectedException()
        {
            // Act
            Action action = () => ScopedLifestyle.DisposeInstances(null);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("disposables", action);
        }

        [TestMethod]
        public void DisposeInstances_ListWithOneItem_DisposesItem()
        {
            // Arrange
            var disposable = new DisposableObject();

            var disposables = new List<IDisposable> { disposable };

            // Act
            ScopedLifestyle.DisposeInstances(disposables);

            // Assert
            Assert.AreEqual(1, disposable.DisposeCount);
        }

        [TestMethod]
        public void DisposeInstances_ListWithMultipleItems_DisposesAllItems()
        {
            // Arrange
            var disposables = new List<DisposableObject> 
            { 
                new DisposableObject(),
                new DisposableObject(),
                new DisposableObject()
            };

            // Act
            ScopedLifestyle.DisposeInstances(disposables.Cast<IDisposable>().ToList());

            // Assert
            Assert.IsTrue(disposables.All(d => d.DisposeCount == 1));
        }

        [TestMethod]
        public void DisposeInstances_ListWithMultipleItems_DisposesAllItemsInReversedOrder()
        {
            // Arrange
            var disposedItems = new List<DisposableObject>();

            var disposables = new List<DisposableObject> 
            { 
                new DisposableObject(disposedItems.Add),
                new DisposableObject(disposedItems.Add),
                new DisposableObject(disposedItems.Add)
            };

            // Act
            ScopedLifestyle.DisposeInstances(disposables.Cast<IDisposable>().ToList());

            disposedItems.Reverse();

            // Assert
            Assert.IsTrue(disposedItems.SequenceEqual(disposables));
        }

        [TestMethod]
        public void DisposeInstances_WithMultipleItemsThatThrow_StillDisposesAllItems()
        {
            // Arrange
            var disposables = new List<DisposableObject> 
            { 
                new DisposableObject(new Exception()),
                new DisposableObject(new Exception()),
                new DisposableObject(new Exception())
            };

            try
            {
                // Act
                ScopedLifestyle.DisposeInstances(disposables.Cast<IDisposable>().ToList());

                // Assert
                Assert.Fail("Exception was expected to bubble up.");
            }
            catch
            {
                Assert.IsTrue(disposables.All(d => d.DisposeCount == 1));
            }
        }

        [TestMethod]
        public void DisposeInstances_WithMultipleItemsThatThrow_WillBubbleUpTheLastThrownException()
        {
            // Arrange
            var disposables = new List<DisposableObject> 
            { 
                new DisposableObject(new Exception()),
                new DisposableObject(new Exception()),
                new DisposableObject(new Exception()),
                new DisposableObject(),
                new DisposableObject(new Exception()),
                new DisposableObject(new Exception())
            };

            try
            {
                // Act
                ScopedLifestyle.DisposeInstances(disposables.Cast<IDisposable>().ToList());

                // Assert
                Assert.Fail("Exception was expected to bubble up.");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(object.ReferenceEquals(disposables.First().ExceptionToThrow, ex));
            }
        }

        [TestMethod]
        public void Dispose_ScopedItemResolvedDuringScopeEndAction_GetsDisposed()
        {
            // Arrange
            var disposable = new DisposableObject();

            var scope = new Scope();

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

            var scope = new Scope();

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
            var scope = new Scope();

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
            var scope = new Scope();

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

            var scope = new Scope();

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            var container = new Container();

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

            var scope = new Scope();

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            var container = new Container();

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
            Assert.IsInstanceOfType(disposedObjects.Last(), typeof(DisposablePlugin),
                "Since the disposable logger is requested during disposal of the scope, it should be " +
                "disposed after all other already created services have been disposed. " +
                "In the given case, disposing DisposableLogger before the IDisposable registration becomes " +
                "a problem when that registration starts using ILogger in its dispose method as well.");
        }

        [TestMethod]
        public void Dispose_InstanceResolvedDuringDisposingScopeRegisteringEndAction_CallsThisAction()
        {
            // Arrange
            bool actionCalled = false;

            var scope = new Scope();

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            var container = new Container();

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

            var scope = new Scope();

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            var container = new Container();

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

            var scope = new Scope();

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            var container = new Container();

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
            var scope = new Scope();

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            var container = new Container();

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
            var scope = new Scope();

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            var container = new Container();

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
            var scope = new Scope();

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            var container = new Container();

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
            var scope = new Scope();

            var scopedLifestyle = new FakeScopedLifestyle(scope);

            var container = new Container();

            container.Register<IPlugin>(() => new DisposablePlugin(), scopedLifestyle);

            container.GetInstance<IPlugin>();

            scope.Dispose();

            // Act
            Action action = () => container.GetInstance<IPlugin>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Cannot access a disposed object.",
                action);
        }

        private class DisposablePlugin : IPlugin, IDisposable
        {
            private readonly Action<DisposablePlugin> disposing;

            public DisposablePlugin(Action<DisposablePlugin> disposing = null)
            {
                this.disposing = disposing;
            }

            public void Dispose()
            {
                if (this.disposing != null)
                {
                    this.disposing(this);
                }
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

            public Exception ExceptionToThrow { get; private set; }

            public int DisposeCount { get; private set; }

            public bool IsDisposedOnce 
            {
                get { return this.DisposeCount == 1; } 
            }

            public void Dispose()
            {
                this.DisposeCount++;

                if (this.disposing != null)
                {
                    this.disposing(this);
                }

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

            protected internal override Func<Scope> CreateCurrentScopeProvider(Container container)
            {
                return () => this.scope;
            }

            protected override int Length
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}