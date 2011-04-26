using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.CodeSamples.Tests.Unit
{
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
        [ExpectedException(typeof(ActivationException))]
        public void GetInstance_WithoutLifetimeScope2_ThrowsException()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand>(() => container.GetInstance<ConcreteCommand>());

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
        public void GetInstance_WithinLifetimeScope2_ReturnsInstanceOfExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand>(() => new ConcreteCommand());

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
        public void GetInstance_CalledMultipleTimesWithinSingleLifetimeScope2_ReturnsASingleInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand>(() => new ConcreteCommand());

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
        public void GetInstance_CalledWithinNestedSingleLifetimeScopes2_ReturnsAnInstancePerScope()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand>(() => new ConcreteCommand());

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
        public void GetInstance_CalledWithinSameLifetimeScopeWithOtherScopesInBetween2_ReturnsASingleInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand>(() => new ConcreteCommand());

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

            container.MarkAsDisposable<DisposableCommand>();

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

            container.MarkAsDisposable<DisposableCommand>();

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
        public void MarkAsDisposable_OnTransientObject_WillNotBeDisposedDuringLifetimeScope()
        {
            // Arrange
            var container = new Container();

            container.MarkAsDisposable<DisposableCommand>();

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

            container.MarkAsDisposable<DisposableCommand>();

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
        public void GetInstance_CalledOutsideBeginLifetimeScopeRequestingAnInstanceMarkedAsDisposable_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage = "instance is requested outside the context of a BeginLifetimeScope";
            
            var container = new Container();

            container.MarkAsDisposable<DisposableCommand>();

            try
            {
                // Act
                container.GetInstance<DisposableCommand>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(expectedMessage), "Actual message: " + ex.Message);
            }
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