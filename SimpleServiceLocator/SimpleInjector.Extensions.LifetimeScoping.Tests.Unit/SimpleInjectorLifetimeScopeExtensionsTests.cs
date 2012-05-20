namespace SimpleInjector.Extensions.LifetimeScoping.Tests.Unit
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SimpleInjectorLifetimeScopeExtensionsTests
    {
        public interface ICommand
        {
            void Execute();
        }

        [TestMethod]
        public void BeginLifetimeScope_WithoutAnyLifetimeScopeRegistrations_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.EnableLifetimeScoping();

            // Act
            using (var scope = container.BeginLifetimeScope())
            {
                container.GetInstance<ConcreteCommand>();
            }
        }

        [TestMethod]
        public void BeginLifetimeScope_WithoutAnyLifetimeScopeRegistrationsAndWithoutExplicitlyEnablingLifetimeScoping_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.BeginLifetimeScope();

                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(
                    "please make sure the EnableLifetimeScoping extension method is called"),
                    "Actual message: " + ex.Message);
            }
        }

        [TestMethod]
        public void RegisterLifetimeScope_CalledASingleTime_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterLifetimeScope<ConcreteCommand>();
        }

        [TestMethod]
        public void RegisterLifetimeScope_CalledMultipleTimes_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterLifetimeScope<ConcreteCommand>();
            container.RegisterLifetimeScope<ICommand>(() => new ConcreteCommand());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetCurrentLifetimeScope_WithNullContainerArgument_ThrowsExpectedException()
        {
            // Arrange
            Container container = null;

            // Act
            container.GetCurrentLifetimeScope();
        }

        [TestMethod]
        public void GetCurrentLifetimeScope_OutsideTheContextOfALifetimeScope_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            container.EnableLifetimeScoping();

            // Act
            var currentScope = container.GetCurrentLifetimeScope();

            // Assert
            Assert.IsNull(currentScope);
        }

        [TestMethod]
        public void GetCurrentLifetimeScope_OutsideTheContextOfALifetimeScopeWithoutScopingEnabled_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.GetCurrentLifetimeScope();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(
                    "please make sure the EnableLifetimeScoping extension method is called"),
                    "Actual message: " + ex.Message);
            }
        }

        [TestMethod]
        public void GetCurrentLifetimeScope_InsideTheContextOfALifetimeScope_ReturnsThatScope()
        {
            // Arrange
            var container = new Container();

            container.EnableLifetimeScoping();

            using (var scope = container.BeginLifetimeScope())
            {
                // Act
                var currentScope = container.GetCurrentLifetimeScope();

                // Assert
                Assert.IsNotNull(currentScope);
                Assert.IsTrue(object.ReferenceEquals(scope, currentScope));
            }
        }

        [TestMethod]
        public void GetCurrentLifetimeScope_InsideANestedLifetimeScope_ReturnsTheInnerMostScope()
        {
            // Arrange
            var container = new Container();

            container.EnableLifetimeScoping();

            using (var outerScope = container.BeginLifetimeScope())
            {
                using (var innerScope = container.BeginLifetimeScope())
                {
                    Assert.IsFalse(object.ReferenceEquals(outerScope, innerScope), "Test setup failed.");

                    // Act
                    var currentScope = container.GetCurrentLifetimeScope();

                    // Assert
                    Assert.IsTrue(object.ReferenceEquals(innerScope, currentScope));
                }
            }
        }

        [TestMethod]
        public void GetInstance_WithoutLifetimeScope_ReturnsSingleton()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            // Act
            var firstInstance = container.GetInstance<ICommand>();

            using (container.BeginLifetimeScope())
            {
                container.GetInstance<ICommand>();
            }

            var secondInstance = container.GetInstance<ICommand>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(firstInstance, secondInstance));
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
        public void GetInstance_WithinLifetimeScope_ReturnsADifferentInstanceThanTheOneCreatedOutsideThatScope()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            // Act
            var firstInstance = container.GetInstance<ICommand>();

            using (container.BeginLifetimeScope())
            {
                var secondInstance = container.GetInstance<ICommand>();

                // Assert
                Assert.IsFalse(object.ReferenceEquals(firstInstance, secondInstance));
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
                Assert.IsTrue(object.ReferenceEquals(firstInstance, secondInstance));
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
                    Assert.IsFalse(object.ReferenceEquals(firstInstance, secondInstance));
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
                Assert.IsTrue(object.ReferenceEquals(firstInstance, secondInstance));
            }
        }

        [TestMethod]
        public void LifetimeScopeDispose_RegisterLifetimeScopeWithDisposal_EnsuresInstanceGetDisposedAfterLifetimeScopeEnds()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<DisposableCommand>();

            DisposableCommand command;

            // Act
            using (container.BeginLifetimeScope())
            {
                command = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsTrue(command.HasBeenDisposed, "The instance was expected to be disposed.");
        }

        [TestMethod]
        public void LifetimeScopeDispose_TransientDisposableObject_DoesNotDisposeInstanceAfterLifetimeScopeEnds()
        {
            // Arrange
            var container = new Container();

            container.EnableLifetimeScoping();

            // Transient
            container.Register<ICommand, DisposableCommand>();

            DisposableCommand command;

            // Act
            using (container.BeginLifetimeScope())
            {
                command = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsFalse(command.HasBeenDisposed,
                "The lifetime scope should not dispose objects that are not explicitly marked as such, since " +
                "this would allow the scope to accidentally dispose singletons.");
        }

        [TestMethod]
        public void LifetimeScopeDispose_WithInstanceExplicitlyRegisteredForDisposal_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            container.EnableLifetimeScoping();

            // Transient
            container.Register<ICommand, DisposableCommand>();

            container.RegisterInitializer<DisposableCommand>(instance =>
            {
                var scope = container.GetCurrentLifetimeScope();

                // The following line explictly registers the transient DisposableCommand for disposal when
                // the lifetime scope ends.
                scope.RegisterForDisposal(instance);
            });

            DisposableCommand command;

            // Act
            using (container.BeginLifetimeScope())
            {
                command = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsTrue(command.HasBeenDisposed,
                "The transient instance was expected to be disposed, because it was registered for disposal.");
        }

        [TestMethod]
        public void RegisterForDisposal_WithNullArgument_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.EnableLifetimeScoping();

            // Act
            using (var scope = container.BeginLifetimeScope())
            {
                try
                {
                    scope.RegisterForDisposal(null);

                    Assert.Fail("Exception expected.");
                }
                catch (ArgumentNullException)
                {
                    // This exception is expected.
                }
            }
        }

        [TestMethod]
        public void LifetimeScopeDispose_OnLifetimeScopedObject_EnsuresInstanceGetDisposedAfterLifetimeScope()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, DisposableCommand>();

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
        public void GetInstance_OnLifetimeScopedObject_WillNotBeDisposedDuringLifetimeScope()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<DisposableCommand>();

            // Act
            using (container.BeginLifetimeScope())
            {
                var command = container.GetInstance<DisposableCommand>();

                // Assert
                Assert.IsFalse(command.HasBeenDisposed, "The instance should not be disposed inside the scope.");
            }
        }

        [TestMethod]
        public void GetInstance_WithinALifetimeScope_NeverDisposesASingleton()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<DisposableCommand>();

            container.RegisterLifetimeScope<IDisposable, DisposableCommand>();

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
        public void RegisterLifetimeScope_CalledAfterInitialization_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            // This locks the container.
            container.GetInstance<ConcreteCommand>();

            try
            {
                // Act
                container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("The Container can't be changed"),
                    "Actual: " + ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BeginLifetimeScope_WithNullArgument_ThrowsExpectedException()
        {
            SimpleInjectorLifetimeScopeExtensions.BeginLifetimeScope(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterLifetimeScopeTConcrete_WithNullArgument_ThrowsExpectedException()
        {
            SimpleInjectorLifetimeScopeExtensions.RegisterLifetimeScope<ConcreteCommand>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterLifetimeScopeTServiceTImplementation_WithNullArgument_ThrowsExpectedException()
        {
            SimpleInjectorLifetimeScopeExtensions.RegisterLifetimeScope<ICommand, ConcreteCommand>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterLifetimeScopeTServiceFunc_WithNullContainerArgument_ThrowsExpectedException()
        {
            SimpleInjectorLifetimeScopeExtensions.RegisterLifetimeScope<ICommand>(null, () => null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterLifetimeScopeTServiceFunc_WithNullFuncArgument_ThrowsExpectedException()
        {
            SimpleInjectorLifetimeScopeExtensions.RegisterLifetimeScope<ICommand>(new Container(), null);
        }

        public class ConcreteCommand : ICommand
        {
            public void Execute()
            {
            }
        }

        public class DisposableCommand : ICommand, IDisposable
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