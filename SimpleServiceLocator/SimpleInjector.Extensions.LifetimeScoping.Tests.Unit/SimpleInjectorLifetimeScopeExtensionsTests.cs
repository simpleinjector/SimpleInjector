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
        public void GetInstance_RegistrationUsingLifetimeScopeLifestyle_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new LifetimeScopeLifestyle());

            using (container.BeginLifetimeScope())
            {
                // Act
                container.GetInstance<ICommand>();
            }
        }
        
        [TestMethod]
        public void GetInstance_RegistrationUsingFuncLifetimeScopeLifestyle_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand>(() => new ConcreteCommand(), new LifetimeScopeLifestyle());

            using (container.BeginLifetimeScope())
            {
                // Act
                container.GetInstance<ICommand>();
            }
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
        public void GetInstance_WithoutLifetimeScope_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            try
            {
                // Act
                var firstInstance = container.GetInstance<ICommand>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(
                    "The ICommand is registered as 'LifetimeScope', but the instance is requested outside " +
                    "the context of a lifetime scope. Make sure you call container.BeginLifetimeScope() first."),
                    "Actual message: " + ex.Message);
            }
        }
        
        [TestMethod]
        public void Verify_WithoutLifetimeScope_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            // Act
            container.Verify();
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
        public void GetInstance_CalledMultipleTimesOnALifetimeScopeServiceWithinASingleScope_DisposesThatInstanceOnce()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<DisposableCommand>();

            DisposableCommand command;

            // Act
            using (container.BeginLifetimeScope())
            {
                command = container.GetInstance<DisposableCommand>();

                container.GetInstance<DisposableCommand>();
                container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.AreEqual(1, command.DisposeCount, "Dispose should be called exactly once.");
        }

        [TestMethod]
        public void GetInstance_ResolveMultipleLifetimeScopedServicesWithStrangeEqualsImplementations_CorrectlyDisposesAllInstances()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<DisposableCommandWithOverriddenEquality1>();
            container.RegisterLifetimeScope<DisposableCommandWithOverriddenEquality2>();

            // Act
            DisposableCommandWithOverriddenEquality1 command1;
            DisposableCommandWithOverriddenEquality2 command2;

            // Act
            using (container.BeginLifetimeScope())
            {
                command1 = container.GetInstance<DisposableCommandWithOverriddenEquality1>();
                command2 = container.GetInstance<DisposableCommandWithOverriddenEquality2>();

                // Give both instances the same hash code. Both have an equals implementation that compared
                // using the hash code, which make them look like they're the same instance.
                command1.HashCode = 1;
                command2.HashCode = 1;
            }

            // Assert
            string assertMessage =
                "Dispose is expected to be called on this command, even when it contains a GetHashCode and " +
                "Equals implementation that is totally screwed up, since storing disposable objects, " +
                "should be completely independant to this implementation. ";

            Assert.AreEqual(1, command1.DisposeCount, assertMessage + "command1");
            Assert.AreEqual(1, command2.DisposeCount, assertMessage + "command2");
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
        
        [TestMethod]
        public void GetInstance_LifetimeScopedInstanceWithInitializer_CallsInitializerOncePerLifetimeScope()
        {
            // Arrange
            int initializerCallCount = 0;

            var container = new Container();

            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            container.RegisterInitializer<ICommand>(command => { initializerCallCount++; });

            using (container.BeginLifetimeScope())
            {
                // Act
                container.GetInstance<ICommand>();
                container.GetInstance<ICommand>();
            }

            // Assert
            Assert.AreEqual(1, initializerCallCount, "The initializer for ICommand is expected to get fired once.");
        }

        [TestMethod]
        public void GetInstance_LifetimeScopedFuncInstanceWithInitializer_CallsInitializerOncePerLifetimeScope()
        {
            // Arrange
            int initializerCallCount = 0;

            var container = new Container();

            container.RegisterLifetimeScope<ICommand>(() => new ConcreteCommand());

            container.RegisterInitializer<ICommand>(command => { initializerCallCount++; });

            using (container.BeginLifetimeScope())
            {
                // Act
                container.GetInstance<ICommand>();
                container.GetInstance<ICommand>();
            }

            // Assert
            Assert.AreEqual(1, initializerCallCount, "The initializer for ICommand is expected to get fired once.");
        }

        [TestMethod]
        public void RegisterLifetimeScope_CalledAfterVerifyWithAllowOverridingRegistrationsSetToTrue_ShouldResolveAsExpected()
        {
            // Arrange
            var container = new Container();

            container.Options.AllowOverridingRegistrations = true;

            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            container.Verify();

            // Act
            // This call should not replace the registrered LifetimeScopeManager. If it was changed, this test
            // will fail.
            container.RegisterLifetimeScope<IDisposable, DisposableCommand>();

            ICommand command1, command2;

            using (container.BeginLifetimeScope())
            {
                // Resolve the registration that was made before the call to Verify
                command1 = container.GetInstance<ICommand>();
                command2 = container.GetInstance<ICommand>();
            }

            // Assert
            Assert.IsTrue(object.ReferenceEquals(command1, command2));
        }

        [TestMethod]
        public void EnableLifetimeScoping_CalledAfterVerifyWithAllowOverridingRegistrationsSetToTrue_ShouldResolveAsExpected()
        {
            // Arrange
            var container = new Container();

            container.Options.AllowOverridingRegistrations = true;
            
            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            container.Verify();

            // Act
            // This call should not replace the registrered LifetimeScopeManager. If it was changed, this test
            // will fail.
            container.EnableLifetimeScoping();

            ICommand command1, command2;

            using (container.BeginLifetimeScope())
            {
                // Resolve the registration that was made before the call to Verify
                command1 = container.GetInstance<ICommand>();
                command2 = container.GetInstance<ICommand>();
            }

            // Assert
            Assert.IsTrue(object.ReferenceEquals(command1, command2));
        }

        [TestMethod]
        public void EnableLifetimeScoping_AllowOverridingRegistrationsIsTrue_AllowOverridingRegistrationsRemainsTrue()
        {
            // Arrange
            var container = new Container();

            container.Options.AllowOverridingRegistrations = true;

            // Act
            container.EnableLifetimeScoping();

            // Assert
            Assert.IsTrue(container.Options.AllowOverridingRegistrations);
        }

        [TestMethod]
        public void EnableLifetimeScoping_CalledAgainAfterAllowOverridingRegistrationsIsTrue_AllowOverridingRegistrationsRemainsTrue()
        {
            // Arrange
            var container = new Container();

            container.EnableLifetimeScoping();

            container.Options.AllowOverridingRegistrations = true;

            // Act
            container.EnableLifetimeScoping();

            // Assert
            Assert.IsTrue(container.Options.AllowOverridingRegistrations);
        }

        [TestMethod]
        public void GetInstance_OnDecoratedLifetimeScopedInstance_WrapsTheInstanceWithTheDecorator()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            container.RegisterDecorator(typeof(ICommand), typeof(CommandDecorator));

            using (container.BeginLifetimeScope())
            {
                // Act
                ICommand instance = container.GetInstance<ICommand>();

                // Assert
                Assert.IsInstanceOfType(instance, typeof(CommandDecorator));

                var decorator = (CommandDecorator)instance;

                Assert.IsInstanceOfType(decorator.DecoratedInstance, typeof(ConcreteCommand));
            }
        }

        [TestMethod]
        public void GetInstance_CalledTwiceInOneScopeForDecoratedLifetimeScopedInstance_WrapsATransientDecoratorAroundALifetimeScopedInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            container.RegisterDecorator(typeof(ICommand), typeof(CommandDecorator));

            using (container.BeginLifetimeScope())
            {
                // Act
                var decorator1 = (CommandDecorator)container.GetInstance<ICommand>();
                var decorator2 = (CommandDecorator)container.GetInstance<ICommand>();

                // Assert
                Assert.IsFalse(object.ReferenceEquals(decorator1, decorator2),
                    "The decorator should be transient.");

                Assert.IsTrue(object.ReferenceEquals(decorator1.DecoratedInstance, decorator2.DecoratedInstance),
                    "The decorated instance should be scoped per lifetime. It seems to be transient.");
            }
        }

        [TestMethod]
        public void GetInstance_CalledTwiceInOneScopeForDecoratedLifetimeScopedInstance2_WrapsATransientDecoratorAroundALifetimeScopedInstance()
        {
            // Arrange
            var container = new Container();

            // Same as previous test, but now with RegisterDecorator called first.
            container.RegisterDecorator(typeof(ICommand), typeof(CommandDecorator));

            container.RegisterLifetimeScope<ICommand, ConcreteCommand>();

            using (container.BeginLifetimeScope())
            {
                // Act
                var decorator1 = (CommandDecorator)container.GetInstance<ICommand>();
                var decorator2 = (CommandDecorator)container.GetInstance<ICommand>();

                // Assert
                Assert.IsFalse(object.ReferenceEquals(decorator1, decorator2),
                    "The decorator should be transient but seems to have a scoped lifetime.");

                Assert.IsTrue(object.ReferenceEquals(decorator1.DecoratedInstance, decorator2.DecoratedInstance),
                    "The decorated instance should be scoped per lifetime. It seems to be transient.");
            }
        }

        public class ConcreteCommand : ICommand
        {
            public void Execute()
            {
            }
        }

        public class DisposableCommand : ICommand, IDisposable
        {
            public int DisposeCount { get; private set; }

            public bool HasBeenDisposed
            {
                get { return this.DisposeCount > 0; }
            }

            public void Dispose()
            {
                this.DisposeCount++;
            }

            public void Execute()
            {
            }
        }

        public class DisposableCommandWithOverriddenEquality1 : DisposableCommandWithOverriddenEquality
        {
        }

        public class DisposableCommandWithOverriddenEquality2 : DisposableCommandWithOverriddenEquality
        {
        }

        public abstract class DisposableCommandWithOverriddenEquality : ICommand, IDisposable
        {
            public int HashCode { get; set; }

            public int DisposeCount { get; private set; }

            public void Dispose()
            {
                this.DisposeCount++;
            }

            public void Execute()
            {
            }

            public override int GetHashCode()
            {
                return this.HashCode;
            }

            public override bool Equals(object obj)
            {
                return this.GetHashCode() == obj.GetHashCode();
            }
        }

        public class CommandDecorator : ICommand
        {
            public CommandDecorator(ICommand decorated)
            {
                this.DecoratedInstance = decorated;
            }

            public ICommand DecoratedInstance { get; private set; }

            public void Execute()
            {
            }
        }
    }
}