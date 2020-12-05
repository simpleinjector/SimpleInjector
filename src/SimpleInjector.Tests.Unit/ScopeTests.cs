namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Lifestyles;

    [TestClass]
    public class ScopeTests
    {
        [TestMethod]
        public void GetItem_NoValueSet_ReturnsNull()
        {
            // Arrange
            object key = new object();

            var scope = new Scope(new Container());

            // Act
            object item = scope.GetItem(key);

            // Assert
            Assert.IsNull(item);
        }

        [TestMethod]
        public void GetItem_WithValueSet_ReturnsThatItem()
        {
            // Arrange
            object key = new object();
            object expectedItem = new object();

            var scope = new Scope(new Container());

            scope.SetItem(key, expectedItem);

            // Act
            object actualItem = scope.GetItem(key);

            // Assert
            Assert.AreSame(expectedItem, actualItem);
        }

        [TestMethod]
        public void GetItem_WithValueSetInOneContainer_DoesNotReturnThatItemInAnotherContainer()
        {
            // Arrange
            object key = new object();
            object expectedItem = new object();

            var container = new Container();
            var scope1 = new Scope(container);
            var scope2 = new Scope(container);

            scope1.SetItem(key, expectedItem);

            // Act
            object actualItem = scope2.GetItem(key);

            // Assert
            Assert.IsNull(actualItem, "The items dictionary is expected to be bound to the scope. Not the container!");
        }

        [TestMethod]
        public void GetItem_WithValueSetTwice_ReturnsLastItem()
        {
            // Arrange
            object key = new object();
            object firstItem = new object();
            object expectedItem = new object();

            var scope = new Scope(new Container());

            scope.SetItem(key, firstItem);
            scope.SetItem(key, expectedItem);

            // Act
            object actualItem = scope.GetItem(key);

            // Assert
            Assert.AreSame(expectedItem, actualItem);
        }

        [TestMethod]
        public void GetItem_WithValueReset_ReturnsNull()
        {
            // Arrange
            object key = new object();

            var scope = new Scope(new Container());

            scope.SetItem(key, new object());
            scope.SetItem(key, null);

            // Act
            object item = scope.GetItem(key);

            // Assert
            // This test looks odd, but under the cover the item is removed from the collection when null
            // is supplied to prevent the dictionary from ever increasing, but we have to test this code path.
            Assert.IsNull(item, "When a value is overridden with null, it is expected to return null.");
        }

        [TestMethod]
        public void GetItem_WithNullKey_ThrowsException()
        {
            // Arrange
            var scope = new Scope(new Container());

            // Act
            Action action = () => scope.GetItem(null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void SetItem_WithNullKey_ThrowsException()
        {
            // Arrange
            var scope = new Scope(new Container());

            // Act
            Action action = () => scope.SetItem(null, new object());

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetService_OnRegisteredInstance_ReturnsThatInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ILogger, NullLogger>();

            IServiceProvider activeScope = new Scope(container);

            // Act
            var logger = activeScope.GetService(typeof(ILogger));

            // Assert
            Assert.IsNotNull(logger);
        }

        [TestMethod]
        public async Task DisposeScopeAsync_WithNonDisposableScopedInstance_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register<IFoo, NonDisposable>(Lifestyle.Scoped);

            var scope = AsyncScopedLifestyle.BeginScope(container);

            container.GetInstance<IFoo>();

            // Act
            await scope.DisposeScopeAsync();
        }

        [TestMethod]
        public async Task DisposeScopeAsync_WithSynchronousDisposableScopedInstance_DisposesThatInstance()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register<Disposable>(Lifestyle.Scoped);

            var scope = AsyncScopedLifestyle.BeginScope(container);

            var plugin = container.GetInstance<Disposable>();

            // Act
            await scope.DisposeScopeAsync();

            // Assert
            Assert.IsTrue(plugin.Disposed);
        }

        [TestMethod]
        public async Task DisposeScopeAsync_WithAsynchronousDisposableScopedInstance_DisposesThatInstance()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register<AsyncDisposable>(Lifestyle.Scoped);

            var scope = AsyncScopedLifestyle.BeginScope(container);

            var plugin = container.GetInstance<AsyncDisposable>();

            // Act
            await scope.DisposeScopeAsync();

            // Assert
            Assert.IsTrue(plugin.AsyncDisposed);
        }

        [TestMethod]
        public async Task DisposeScopeAsync_WithSyncAndAsyncDisposableScopedInstance_DisposesThatInstanceOnlyAsynchronously()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register<SyncAsyncDisposable>(Lifestyle.Scoped);

            var scope = AsyncScopedLifestyle.BeginScope(container);

            var plugin = container.GetInstance<SyncAsyncDisposable>();

            // Act
            await scope.DisposeScopeAsync();

            // Assert
            Assert.IsTrue(plugin.AsyncDisposed);
            Assert.IsFalse(plugin.SyncDisposed,
                "In case both interfaces are implemented, only DisposeAsync should be called. " +
                "The C# compiler acts this way as well.");
        }

        [TestMethod]
        public async Task DisposeScopeAsync_WithSyncAndAsyncDisposableScopedInstanceDeleteRegistration_DisposesThatInstanceOnlyAsynchronously()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register<IFoo>(() => new SyncAsyncDisposable(), Lifestyle.Scoped);

            var scope = AsyncScopedLifestyle.BeginScope(container);

            var plugin = container.GetInstance<IFoo>() as SyncAsyncDisposable;

            // Act
            await scope.DisposeScopeAsync();

            // Assert
            Assert.IsTrue(plugin.AsyncDisposed);
            Assert.IsFalse(plugin.SyncDisposed,
                "In case both interfaces are implemented, only DisposeAsync should be called. " +
                "The C# compiler acts this way as well.");
        }

        [TestMethod]
        public async Task DisposeScopeAsync_WithAsyncDisposableScopedDeleteRegistration_DisposesThatInstance()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register<SealedAsyncDisposable>(() => new SealedAsyncDisposable(), Lifestyle.Scoped);

            var scope = AsyncScopedLifestyle.BeginScope(container);

            var plugin = container.GetInstance<SealedAsyncDisposable>();

            // Act
            await scope.DisposeScopeAsync();

            // Assert
            Assert.IsTrue(plugin.AsyncDisposed);
        }

        [TestMethod]
        public async Task DisposeScopeAsync_RegisterForIDisposableOnMixedDisposable_DisposesThatInstanceOnlyAsynchronously()
        {
            // Arrange
            var container = ContainerFactory.New();

            var scope = new Scope(container);

            var plugin = new SyncAsyncDisposable();
            scope.RegisterForDisposal((IDisposable)plugin);

            // Act
            await scope.DisposeScopeAsync();

            // Assert
            Assert.IsTrue(plugin.AsyncDisposed);
            Assert.IsFalse(plugin.SyncDisposed,
                "In case both interfaces are implemented, only DisposeAsync should be called. " +
                "The C# compiler acts this way as well.");
        }

        [TestMethod]
        public async Task DisposeScopeAsync_WithAsyncDisposableSingleton_DisposesThatInstanceWhenContainerGetsDisposed()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IFoo>(() => new SyncAsyncDisposable(), Lifestyle.Singleton);

            var plugin = container.GetInstance<IFoo>() as SyncAsyncDisposable;

            Assert.IsFalse(plugin.AsyncDisposed, "Setup failed");

            // Act
            await container.DisposeContainerAsync();

            // Assert
            Assert.IsTrue(plugin.AsyncDisposed);
        }

        [TestMethod]
        public void Dispose_WithAsyncDisposable_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register<AsyncDisposable>(Lifestyle.Scoped);

            var scope = AsyncScopedLifestyle.BeginScope(container);

            var plugin = container.GetInstance<AsyncDisposable>();

            // Act
            Action action = () => scope.Dispose();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "AsyncDisposable only implements IAsyncDisposable, but not IDisposable. Make sure to call " +
                "Scope.DisposeScopeAsync() instead of Dispose().",
                action);
        }

        [TestMethod]
        public async Task GetAllDisposables_WithSyncAndAsyncDisposableInstances_ReturnsThoseInstances()
        {
            var container = ContainerFactory.New();
            container.Options.ResolveUnregisteredConcreteTypes = false;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register<NonDisposable>(Lifestyle.Scoped);
            container.Register<AsyncDisposable>(Lifestyle.Scoped);
            container.Register<SealedAsyncDisposable>(Lifestyle.Scoped);
            container.Register<SyncAsyncDisposable>(Lifestyle.Scoped);
            container.Register<Disposable>(Lifestyle.Scoped);

            var scope = AsyncScopedLifestyle.BeginScope(container);

            IAsyncDisposable disposable0 = container.GetInstance<SealedAsyncDisposable>();
            IAsyncDisposable disposable1 = container.GetInstance<SyncAsyncDisposable>();
            IAsyncDisposable disposable2 = container.GetInstance<AsyncDisposable>();
            IDisposable disposable3 = container.GetInstance<Disposable>();
            object nonDisposable = container.GetInstance<NonDisposable>();

            // Act
            object[] disposables = scope.GetAllDisposables();

            // Assert
            Assert.AreEqual(4, disposables.Length);
            Assert.AreSame(disposable0, disposables[0], disposables[0]?.ToString());
            Assert.AreSame(disposable1, disposables[1], disposables[1]?.ToString());
            Assert.AreSame(disposable2, disposables[2], disposables[2]?.ToString());
            Assert.AreSame(disposable3, disposables[3], disposables[3]?.ToString());

            await scope.DisposeScopeAsync();
        }

        [TestMethod]
        public async Task GetDisposables_WithSyncAndAsyncDisposableInstances_OnlyReturnsSyncDisposables()
        {
            var container = ContainerFactory.New();
            container.Options.ResolveUnregisteredConcreteTypes = false;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register<NonDisposable>(Lifestyle.Scoped);
            container.Register<AsyncDisposable>(Lifestyle.Scoped);
            container.Register<SealedAsyncDisposable>(Lifestyle.Scoped);
            container.Register<SyncAsyncDisposable>(Lifestyle.Scoped);
            container.Register<Disposable>(Lifestyle.Scoped);

            var scope = AsyncScopedLifestyle.BeginScope(container);

            container.GetInstance<NonDisposable>();
            container.GetInstance<SealedAsyncDisposable>();
            container.GetInstance<AsyncDisposable>();

            IDisposable service0 = container.GetInstance<SyncAsyncDisposable>();
            IDisposable service1 = container.GetInstance<Disposable>();

            // Act
            IDisposable[] disposables = scope.GetDisposables();

            // Assert
            Assert.AreEqual(2, disposables.Length);
            Assert.AreSame(service0, disposables[0], disposables[0]?.ToString());
            Assert.AreSame(service1, disposables[1], disposables[1]?.ToString());

            await scope.DisposeScopeAsync();
        }

        public interface IFoo { }

        public class NonDisposable : IFoo { }

        public class Disposable : IFoo, IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose() => this.Disposed = true;
        }

        public class AsyncDisposable : IFoo, IAsyncDisposable
        {
            public bool AsyncDisposed { get; private set; }

            public ValueTask DisposeAsync()
            {
                this.AsyncDisposed = true;
                return default;
            }
        }

        public sealed class SealedAsyncDisposable : AsyncDisposable { }

        public class SyncAsyncDisposable : IFoo, IAsyncDisposable, IDisposable
        {
            public bool SyncDisposed { get; private set; }
            public bool AsyncDisposed { get; private set; }

            public void Dispose() => this.SyncDisposed = true;

            public ValueTask DisposeAsync()
            {
                this.AsyncDisposed = true;
                return default;
            }
        }
    }
}