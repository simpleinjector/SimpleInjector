namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using Lifestyles;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DisposableTransientLifestyleTests
    {
        public interface IUserRepository : IDisposable
        {
        }

        public interface IValidator<T>
        {
            void Validate(T instance);
        }

        [TestMethod]
        public void DisposingScope_WithDisposableTransientCreatedWithinScope_DisposesThatInstance()
        {
            // Arrange
            DisposableUserRepository instanceToDispose;

            var container = new Container();

            var lifestyle = new DisposableTransientLifestyle(new ThreadScopedLifestyle());

            container.Register<IUserRepository, DisposableUserRepository>(lifestyle);

            var scope = ThreadScopedLifestyle.BeginScope(container);

            instanceToDispose = (DisposableUserRepository)container.GetInstance<IUserRepository>();

            Assert.IsFalse(instanceToDispose.Disposed, "Test setup failed");

            // Act
            scope.Dispose();

            // Assert
            Assert.IsTrue(instanceToDispose.Disposed);
        }

        [TestMethod]
        public void DisposingScope_WithMultipleDisposableTransientCreatedWithinScope_DisposesThoseInstances()
        {
            // Arrange
            DisposableUserRepository instanceToDispose1;
            DisposableUserRepository instanceToDispose2;

            var container = new Container();

            var lifestyle = new DisposableTransientLifestyle(new ThreadScopedLifestyle());

            container.Register<IUserRepository, DisposableUserRepository>(lifestyle);

            var scope = ThreadScopedLifestyle.BeginScope(container);

            instanceToDispose1 = (DisposableUserRepository)container.GetInstance<IUserRepository>();
            instanceToDispose2 = (DisposableUserRepository)container.GetInstance<IUserRepository>();

            // Act
            scope.Dispose();

            // Assert
            Assert.AreNotSame(instanceToDispose1, instanceToDispose2);
            Assert.IsTrue(instanceToDispose1.Disposed);
            Assert.IsTrue(instanceToDispose2.Disposed);
        }

        [TestMethod]
        public void DisposingScope_WithNormalTransientCreatedWithinScope_DoesNotDisposeThatInstance()
        {
            // Arrange
            DisposableUserRepository instanceToDispose;

            var container = new Container();

            var lifestyle = new DisposableTransientLifestyle(new ThreadScopedLifestyle());

            container.Register<IUserRepository, DisposableUserRepository>(Lifestyle.Transient);

            var scope = ThreadScopedLifestyle.BeginScope(container);

            instanceToDispose = (DisposableUserRepository)container.GetInstance<IUserRepository>();

            // Act
            scope.Dispose();

            // Assert
            Assert.IsFalse(instanceToDispose.Disposed);
        }

        [TestMethod]
        public void DisposingScope_ResolvingAnOpenGenericType_DisposesThatInstance()
        {
            // Arrange
            DisposableValidator<object> instanceToDispose;

            var container = new Container();

            // Call to EnableTransientDisposal is needed in case open-generic.
            DisposableTransientLifestyle.EnableForContainer(container);

            var lifestyle = new DisposableTransientLifestyle(new ThreadScopedLifestyle());

            // NOTE: Open-generic types are special, because they are resolved through unregistered type
            // resolution. This test will fail if the DisposableTransientLifestyle registers the initializer
            // lazily.
            container.Register(typeof(IValidator<>), typeof(DisposableValidator<>), lifestyle);

            var scope = ThreadScopedLifestyle.BeginScope(container);

            instanceToDispose = (DisposableValidator<object>)container.GetInstance<IValidator<object>>();

            // Act
            scope.Dispose();

            // Assert
            Assert.IsTrue(instanceToDispose.Disposed);
        }

        [TestMethod]
        public void ResolvingDisposableTransientInstance_OutsideAScope_ThrowsExpectedException()
        {
            // Arrange
            const string ExpectedExceptionMessage =
                "This method can only be called within the context of an active (Thread Scoped) scope.";

            var container = new Container();

            var lifestyle = new DisposableTransientLifestyle(new ThreadScopedLifestyle());

            container.Register<IUserRepository, DisposableUserRepository>(lifestyle);

            try
            {
                // Act
                container.GetInstance<IUserRepository>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains(ExpectedExceptionMessage), "Actual: " + ex.Message);
            }
        }
        
        [TestMethod]
        public void ResolvingDisposableTransientInstance_WithEnableTransientDisposalCalledTwice_DisposesTheInstanceOnce()
        {
            // Arrange
            DisposableUserRepository instanceToDispose;

            var container = new Container();

            DisposableTransientLifestyle.EnableForContainer(container);
            DisposableTransientLifestyle.EnableForContainer(container);

            var lifestyle = new DisposableTransientLifestyle(new ThreadScopedLifestyle());

            container.Register<IUserRepository, DisposableUserRepository>(lifestyle);

            var scope = ThreadScopedLifestyle.BeginScope(container);

            instanceToDispose = (DisposableUserRepository)container.GetInstance<IUserRepository>();

            Assert.IsFalse(instanceToDispose.Disposed, "Test setup failed");

            // Act
            scope.Dispose();

            // Assert
            Assert.AreEqual(1, instanceToDispose.DisposeCount);
        }

        [TestMethod]
        public void RegisteringAnGenericDisposableTransient_EnableTransientDisposalNotCalled_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            var lifestyle = new DisposableTransientLifestyle(new ThreadScopedLifestyle());

            container.Register(typeof(IValidator<>), typeof(DisposableValidator<>), lifestyle);

            try
            {
                // Act
                container.GetInstance<IValidator<object>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(
                    ex.InnerException.Message.Contains(
                        "Please make sure DisposableTransientLifestyle.EnableForContainer(Container) is called"),
                    "Actual: " + ex.InnerException.Message);
            }
        }

        public class DisposableUserRepository : IUserRepository
        {
            public bool Disposed { get; private set; }

            public int DisposeCount { get; private set; }
            
            public void Dispose()
            {
                this.Disposed = true;
                this.DisposeCount++;
            }
        }

        public class DisposableValidator<T> : IValidator<T>, IDisposable
        {
            public bool Disposed { get; private set; }

            public void Validate(T instance)
            {
            }

            public void Dispose()
            {
                this.Disposed = true;
            }
        }
    }
}