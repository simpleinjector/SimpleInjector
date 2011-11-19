namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LifetimeScopeExtensionsTests
    {
        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstance_WithoutLifetimeScope_ThrowsException()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            // Act
            container.GetInstance<ICommand>();
        }

        [TestMethod]
        public void GetInstance_WithinLifetimeScope_ReturnsInstanceOfExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            using (container.BeginLifetimeScope())
            {
                // Act
                var actualInstance = container.GetInstance<ICommand>();

                // Assert
                Assert.IsInstanceOfType(actualInstance, typeof(ConcreteCommand));
            }
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesWithinSingleLifetimeScope_ReturnsASingleInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            using (container.BeginLifetimeScope())
            {
                // Act
                var firstInstance = container.GetInstance<ICommand>();
                var secondInstance = container.GetInstance<ICommand>();

                // Assert
                Assert.AreEqual(firstInstance, secondInstance);
            }
        }

        [TestMethod]
        public void GetInstance_CalledWithinNestedSingleLifetimeScopes_ReturnsAnInstancePerScope()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            using (container.BeginLifetimeScope())
            {
                // Act
                var firstInstance = container.GetInstance<ICommand>();

                using (container.BeginLifetimeScope())
                {
                    var secondInstance = container.GetInstance<ICommand>();

                    // Assert
                    Assert.AreNotEqual(firstInstance, secondInstance);
                }
            }
        }

        [TestMethod]
        public void GetInstance_CalledWithinSameLifetimeScopeWithOtherScopesInBetween_ReturnsASingleInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            using (container.BeginLifetimeScope())
            {
                // Act
                var firstInstance = container.GetInstance<ICommand>();

                using (container.BeginLifetimeScope())
                {
                    container.GetInstance<ICommand>();
                }

                var secondInstance = container.GetInstance<ICommand>();

                // Assert
                Assert.AreEqual(firstInstance, secondInstance);
            }
        }

        [TestMethod]
        public void MarkAsDisposable_OnTransientObject_EnsuresInstanceGetDisposedAfterLifetimeScope()
        {
            // Arrange
            var container = new Container();

            container.DisposeWhenLifetimeScopeEnds<DisposableCommand>();

            DisposableCommand command;

            // Act
            using (container.BeginLifetimeScope())
            {
                command = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsTrue(command.HasBeenDisposed, "The transient instance was expected to be disposed.");
        }

        [TestMethod]
        public void MarkAsDisposable_OnLifetimeScopedObject_EnsuresInstanceGetDisposedAfterLifetimeScope()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, DisposableCommand>();

            container.DisposeWhenLifetimeScopeEnds<DisposableCommand>();

            DisposableCommand command;

            // Act
            using (container.BeginLifetimeScope())
            {
                command = container.GetInstance<ICommand>() as DisposableCommand;
            }

            // Assert
            Assert.IsTrue(command.HasBeenDisposed, "The lifetime scoped instance was expected to be disposed.");
        }
        
        [TestMethod]
        public void MarkAsDisposable_OnTransientObject_WillNotBeDisposedDuringLifetimeScope()
        {
            // Arrange
            var container = new Container();

            container.DisposeWhenLifetimeScopeEnds<DisposableCommand>();

            // Act
            using (container.BeginLifetimeScope())
            {
                var command = container.GetInstance<DisposableCommand>();

                // Assert
                Assert.IsFalse(command.HasBeenDisposed, "The instance should not be disposed inside the scope.");
            }
        }

        [TestMethod]
        public void MarkAsDisposable_OnTransientObjectRegisteredByInterface_EnsuresInstanceGetDisposedAfterLifetimeScope()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, DisposableCommand>();

            container.DisposeWhenLifetimeScopeEnds<DisposableCommand>();

            DisposableCommand command;

            // Act
            using (container.BeginLifetimeScope())
            {
                command = container.GetInstance<ICommand>() as DisposableCommand;
            }

            // Assert
            Assert.IsTrue(command.HasBeenDisposed, "The transient instance was expected to be disposed.");
        }

        [TestMethod]
        public void GetInstance_CalledOutsideBeginLifetimeScopeRequestingAnInstanceMarkedAsDisposable_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.DisposeWhenLifetimeScopeEnds<DisposableCommand>();

            // Act
            // We expect this call to succeed. While this could hide configuration errors, throwing an
            // exception would disallow scenario's where all IDisposable objects, created inside the scope,
            // would be disposed.
            container.GetInstance<DisposableCommand>();
        }

        [TestMethod]
        public void GetInstance_WithinALifetimeScope_NeverDisposesASingleton()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<DisposableCommand>();

            // This triggers disposal of all IDisposable objects.
            container.DisposeWhenLifetimeScopeEnds<IDisposable>();

            // Verify must be called for this to succeed, 
            // because this triggers the Action<IDisposable> on DisposableCommand.
            container.Verify();

            DisposableCommand singleton;

            // Act
            using (container.BeginLifetimeScope())
            {
                singleton = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsFalse(singleton.HasBeenDisposed, "Singletons should not be disposed.");
        }

        [TestMethod]
        public void GetInstance_WithinALifetimeScope_AlwaysDisposesLifetimeScoped()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, DisposableCommand>();

            // This triggers disposal of all IDisposable objects.
            container.DisposeWhenLifetimeScopeEnds<IDisposable>();

            // The assert should be true, even if we call Verify.
            container.Verify();

            DisposableCommand lifetimeScopedObject;

            // Act
            using (container.BeginLifetimeScope())
            {
                lifetimeScopedObject = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsTrue(lifetimeScopedObject.HasBeenDisposed, "Lifetime scope object should be disposed.");
        }

        private class DisposableCommand : ICommand, IDisposable
        {
            public bool HasBeenDisposed { get; private set; }

            public void Dispose()
            {
                this.HasBeenDisposed = true;
            }

            public void Execute()
            {
            }
        }
    }
}