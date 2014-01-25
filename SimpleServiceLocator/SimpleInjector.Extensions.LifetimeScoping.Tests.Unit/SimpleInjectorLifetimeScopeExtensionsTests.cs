namespace SimpleInjector.Extensions.LifetimeScoping.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    [TestClass]
    public class SimpleInjectorLifetimeScopeExtensionsTests
    {
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
        public void BeginLifetimeScope_WithoutAnyLifetimeScopeRegistrationsAndWithoutExplicitlyEnablingLifetimeScoping_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.BeginLifetimeScope();
        }
        
        [TestMethod]
        public void Verify_WithLifetimeScopeRegistrationInOpenGenericAndWithoutExplicitlyEnablingLifetimeScoping_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IGeneric<>), typeof(Generic<>), new LifetimeScopeLifestyle());

            container.Register<ClassDependingOn<IGeneric<int>>>();

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_WithHybridLifetimeScopeRegistrationInOpenGenericAndWithoutExplicitlyEnablingLifetimeScoping_Succeeds()
        {
            // Arrange
            var container = new Container();

            var hybrid = Lifestyle.CreateHybrid(() => false, new LifetimeScopeLifestyle(), new LifetimeScopeLifestyle());

            container.RegisterOpenGeneric(typeof(IGeneric<>), typeof(Generic<>), hybrid);

            container.Register<ClassDependingOn<IGeneric<int>>>();

            // Act
            container.Verify();
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

            // Act
            var scope = container.GetCurrentLifetimeScope();

            // Assert
            Assert.IsNull(scope);
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
                    "The ICommand is registered as 'Lifetime Scope' lifestyle, but the instance is requested " +
                    "outside the context of a Lifetime Scope."),
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
        public void LifetimeScopeDispose_RegisteredConcereteWithExplicitDisposal_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<DisposableCommand>(disposeWhenLifetimeScopeEnds: true);

            DisposableCommand instanceToDispose;

            // Act
            using (container.BeginLifetimeScope())
            {
                instanceToDispose = container.GetInstance<DisposableCommand>();
            }
            
            // Assert
            Assert.IsTrue(instanceToDispose.HasBeenDisposed);
        }

        [TestMethod]
        public void LifetimeScopeDispose_RegisteredConcereteWithoutDisposal_DoesNotDisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<DisposableCommand>(disposeWhenLifetimeScopeEnds: false);

            DisposableCommand instanceToDispose;

            // Act
            using (container.BeginLifetimeScope())
            {
                instanceToDispose = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsFalse(instanceToDispose.HasBeenDisposed);
        }

        [TestMethod]
        public void LifetimeScopeDispose_RegisteredWithExplicitDisposal_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<IDisposable, DisposableCommand>(disposeWhenLifetimeScopeEnds: true);

            DisposableCommand instanceToDispose;

            // Act
            using (container.BeginLifetimeScope())
            {
                instanceToDispose = container.GetInstance<IDisposable>() as DisposableCommand;
            }

            // Assert
            Assert.IsTrue(instanceToDispose.HasBeenDisposed);
        }

        [TestMethod]
        public void LifetimeScopeDispose_RegisteredWithoutDisposal_DoesNotDisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<IDisposable, DisposableCommand>(disposeWhenLifetimeScopeEnds: false);

            DisposableCommand instanceToDispose;

            // Act
            using (container.BeginLifetimeScope())
            {
                instanceToDispose = container.GetInstance<IDisposable>() as DisposableCommand;
            }

            // Assert
            Assert.IsFalse(instanceToDispose.HasBeenDisposed);
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
                Assert.IsTrue(ex.Message.Contains("The container can't be changed"),
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

        [TestMethod]
        public void LifetimeScopeDispose_ExecutedOnDifferentThreadThanItWasStarted_ThrowsExceptionException()
        {
            // Arrange
            var container = new Container();

            container.EnableLifetimeScoping();

            var scope = container.BeginLifetimeScope();
            try
            {
                // Act
                Task.Factory.StartNew(() => scope.Dispose()).Wait();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException.Message.Contains(
                    "It is not safe to use a LifetimeScope instance across threads."),
                    "Actual: ", ex.InnerException.Message);
            }
        }
        
        [TestMethod]
        public void LifetimeScope_TwoScopedRegistationsForTheSameServiceType_CreatesTwoInstances()
        {
            // Arrange
            var lifestyle = new LifetimeScopeLifestyle();

            var container = new Container();

            var reg1 = lifestyle.CreateRegistration<ICommand, DisposableCommand>(container);
            var reg2 = lifestyle.CreateRegistration<ICommand, DisposableCommand>(container);

            container.AppendToCollection(typeof(ICommand), reg1);
            container.AppendToCollection(typeof(ICommand), reg2);

            using (container.BeginLifetimeScope())
            {
                // Act
                var commands = container.GetAllInstances<ICommand>().Cast<DisposableCommand>().ToArray();

                // Assert
                Assert.AreNotSame(commands[0], commands[1], "Two instances were expected.");
            }
        }

        [TestMethod]
        public void LifetimeScopeDispose_TwoScopedRegistationsForTheSameServiceType_DisposesBothInstances()
        {
            // Arrange
            var disposedInstances = new HashSet<object>();

            var lifestyle = new LifetimeScopeLifestyle();

            var container = new Container();

            var reg1 = lifestyle.CreateRegistration<ICommand, DisposableCommand>(container);
            var reg2 = lifestyle.CreateRegistration<ICommand, DisposableCommand>(container);
            
            container.AppendToCollection(typeof(ICommand), reg1);
            container.AppendToCollection(typeof(ICommand), reg2);

            using (container.BeginLifetimeScope())
            {
                var commands = container.GetAllInstances<ICommand>().Cast<DisposableCommand>().ToArray();

                Assert.AreNotSame(commands[0], commands[1], "Test setup failed.");      

                commands[0].Disposing += sender => disposedInstances.Add(sender);
                commands[1].Disposing += sender => disposedInstances.Add(sender);

                // Act
            }

            // Assert
            Assert.AreEqual(2, disposedInstances.Count, "Two instances were expected to be disposed.");
        }

        [TestMethod]
        public void Verify_WithWhenScopeEndsRegistration_Succeeds()
        {
            // Arrange
            var container = new Container();

            var lifestyle = new LifetimeScopeLifestyle();

            container.Register<ICommand, DisposableCommand>(lifestyle);

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                lifestyle.WhenScopeEnds(container, () => { });
            });

            // Act
            container.Verify();
        }

        [TestMethod]
        public void LifetimeScopeDispose_WithWhenScopeEndsRegistration_CallsTheRegisteredAction()
        {
            // Arrange
            int actionCallCount = 0;

            var container = new Container();

            var lifestyle = new LifetimeScopeLifestyle();

            container.Register<DisposableCommand, DisposableCommand>(lifestyle);

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                lifestyle.WhenScopeEnds(container, () => { actionCallCount++; });
            });

            using (container.BeginLifetimeScope())
            {
                container.GetInstance<DisposableCommand>();

                // Act
            }

            // Assert
            Assert.AreEqual(1, actionCallCount, "Delegate is expected to be called exactly once.");            
        }
        
        [TestMethod]
        public void LifetimeScopeDispose_WithWhenScopeEndsRegistration_CallsTheRegisteredActionBeforeCallingDispose()
        {
            // Arrange
            bool delegateHasBeenCalled = false;
            DisposableCommand instanceToDispose = null;

            var container = new Container();

            var lifestyle = new LifetimeScopeLifestyle();

            container.Register<DisposableCommand, DisposableCommand>(lifestyle);

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                LifetimeScopeLifestyle.WhenCurrentScopeEnds(container, () => 
                {
                    Assert.IsFalse(command.HasBeenDisposed, 
                        "The action should be called before disposing the instance, because users are " +
                        "to use those instances.");
                    delegateHasBeenCalled = true;
                });
            });

            using (container.BeginLifetimeScope())
            {
                instanceToDispose = container.GetInstance<DisposableCommand>();

                // Act
            }

            // Assert
            Assert.IsTrue(delegateHasBeenCalled, "Delegate is expected to be called.");
        }

        [TestMethod]
        public void LifetimeScopeDispose_WithTransientRegisteredForDisposal_DisposesThatInstance()
        {
            // Arrange
            DisposableCommand transientInstanceToDispose = null;

            var container = new Container();

            container.EnableLifetimeScoping();

            var lifestyle = new LifetimeScopeLifestyle();

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                lifestyle.RegisterForDisposal(container, command);
            });

            using (container.BeginLifetimeScope())
            {
                transientInstanceToDispose = container.GetInstance<DisposableCommand>();

                // Act
            }

            // Assert
            Assert.IsTrue(transientInstanceToDispose.HasBeenDisposed);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenScopeEnds_NullContainerArgument_ThrowsException()
        {
            // Arrange
            var lifestyle = new LifetimeScopeLifestyle();

            // Act
            lifestyle.WhenScopeEnds(null, () => { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenScopeEnds_NullActionArgument_ThrowsException()
        {
            // Arrange
            var lifestyle = new LifetimeScopeLifestyle();

            // Act
            lifestyle.WhenScopeEnds(new Container(), null);
        }

        [TestMethod]
        public void WhenScopeEnds_CalledOutOfTheContextOfALifetimeScope_ThrowsException()
        {
            // Arrange
            var lifestyle = new LifetimeScopeLifestyle();

            var container = new Container();

            container.RegisterLifetimeScope<ConcreteCommand>();

            try
            {
                // Act
                lifestyle.WhenScopeEnds(container, () => { });

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(
                    "This method can only be called within the context of an active Lifetime Scope."),
                    "Actual: " + ex.Message);
            }
        }

        [TestMethod]
        public void WhenScopeEnds_WithMultipleDisposableComponentsDependingOnEachOther_DependsComponentsInExpectedOrder()
        {
            // Arrange
            var expectedOrderOfDisposal = new List<Type>
            {
                typeof(Outer),
                typeof(Middle),
                typeof(Inner),
            };

            var actualOrderOfDisposal = new List<Type>();

            var container = new Container();

            // Outer, Middle and Inner all depend on Func<object> and call it when disposed.
            // This way we can check in which order the instances are disposed.
            container.RegisterSingle<Action<object>>(instance => actualOrderOfDisposal.Add(instance.GetType()));

            // Outer depends on Middle that depends on Inner. 
            // Registration is deliberately made in a different order to prevent that the order of
            // registration might influence the order of disposing.
            container.RegisterLifetimeScope<Middle>();
            container.RegisterLifetimeScope<Inner>();
            container.RegisterLifetimeScope<Outer>();

            var scope = container.BeginLifetimeScope();

            try
            {
                // Resolve the outer most object.
                container.GetInstance<Outer>();
            }
            finally
            {
                // Act
                scope.Dispose();
            }

            // Assert
            Assert.IsTrue(
                expectedOrderOfDisposal.SequenceEqual(actualOrderOfDisposal),
                "Types were expected to be disposed in the following order: {0}, " +
                "but they actually were disposed in the order: {1}. " +
                "This order is important, because when a components gets disposed, it might still want to " +
                "call the components it depends on, but at that time those components are already disposed.",
                string.Join(", ", expectedOrderOfDisposal.Select(type => type.Name)),
                string.Join(", ", actualOrderOfDisposal.Select(type => type.Name)));
        }

        [TestMethod]
        public void WhenScopeEnds_WithMultipleDisposableComponentsAndPropertyDependencyDependingOnEachOther_DependsComponentsInExpectedOrder()
        {
            // Arrange
            var expectedOrderOfDisposal = new List<Type>
            {
                typeof(Middle),
                typeof(Inner),
                typeof(PropertyDependency),
            };

            var actualOrderOfDisposal = new List<Type>();

            var container = new Container();

            // Allow PropertyDependency to be injected as property on Inner 
            container.Options.PropertySelectionBehavior = new InjectProperties<ImportAttribute>();

            // PropertyDependency, Middle and Inner all depend on Func<object> and call it when disposed.
            // This way we can check in which order the instances are disposed.
            container.RegisterSingle<Action<object>>(instance => actualOrderOfDisposal.Add(instance.GetType()));

            // Middle depends on Inner that depends on property PropertyDependency. 
            // Registration is deliberately made in a different order to prevent that the order of
            // registration might influence the order of disposing.
            container.RegisterLifetimeScope<PropertyDependency>();
            container.RegisterLifetimeScope<Middle>();
            container.RegisterLifetimeScope<Inner>();

            // Act
            var scope = container.BeginLifetimeScope();

            try
            {
                // Resolve the outer most object.
                container.GetInstance<Middle>();
            }
            finally
            {
                // Act
                scope.Dispose();
            }

            // Assert
            Assert.IsTrue(
                expectedOrderOfDisposal.SequenceEqual(actualOrderOfDisposal),
                "Types were expected to be disposed in the following order: {0}, " +
                "but they actually were disposed in the order: {1}. " +
                "Since PropertyDependency is injected as property into Inner, it is important that " +
                "PropertyDependency is disposed after Inner.",
                string.Join(", ", expectedOrderOfDisposal.Select(type => type.Name)),
                string.Join(", ", actualOrderOfDisposal.Select(type => type.Name)));
        }

        [TestMethod]
        public void WhenScopeEnds_DisposingAnOuterScope_DisposalAllItsInnerScopesAsWell()
        {
            // Arrange
            const bool IsDisposed = true;

            var container = new Container();

            Dictionary<DisposableCommand, bool> commands = new Dictionary<DisposableCommand, bool>();

            container.RegisterLifetimeScope<DisposableCommand>();

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                commands[command] = !IsDisposed;

                command.Disposing += _ =>
                {
                    commands[command] = IsDisposed;
                };
            });

            var outerScope = container.BeginLifetimeScope();

            container.GetInstance<DisposableCommand>();

            container.BeginLifetimeScope();

            container.GetInstance<DisposableCommand>();

            container.BeginLifetimeScope();

            container.GetInstance<DisposableCommand>();

            Assert.AreEqual(3, commands.Count);
            Assert.IsFalse(commands.Any(c => c.Value == IsDisposed));

            // Act
            outerScope.Dispose();

            // Assert
            Assert.IsTrue(commands.All(c => c.Value == IsDisposed));
        }
        
        [TestMethod]
        public void WhenScopeEnds_DisposingAnMiddelScope_DisposalAllItsInnerScopesAsWellButNotTheOuterScope()
        {
            // Arrange
            const bool IsDisposed = true;

            var container = new Container();

            Dictionary<DisposableCommand, bool> commands = new Dictionary<DisposableCommand, bool>();

            container.RegisterLifetimeScope<DisposableCommand>();

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                commands[command] = !IsDisposed;

                command.Disposing += _ =>
                {
                    commands[command] = IsDisposed;
                };
            });

            var outerScope = container.BeginLifetimeScope();

            try
            {
                container.GetInstance<DisposableCommand>();

                var middelScope = container.BeginLifetimeScope();

                container.GetInstance<DisposableCommand>();

                container.BeginLifetimeScope();

                container.GetInstance<DisposableCommand>();

                // Act
                middelScope.Dispose();

                // Assert
                Assert.AreEqual(2, commands.Count(c => c.Value == IsDisposed));
            }
            finally
            {
                outerScope.Dispose();
            }
        }

        [TestMethod]
        public void GetInstance_WithPossibleObjectGraphOptimizableRegistration_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.RegisterLifetimeScope<ICommand, DisposableCommand>();

            container.Register<ClassDependingOn<ICommand>>();

            // This registration can be optimized to prevent ICommand from being requested more than once from
            // the LifetimeScopeLifestyleRegistration.GetInstance
            container.Register<ClassDependingOn<ClassDependingOn<ICommand>, ClassDependingOn<ICommand>>>();

            using (container.BeginLifetimeScope())
            {
                // Act
                var instance =
                    container.GetInstance<ClassDependingOn<ClassDependingOn<ICommand>, ClassDependingOn<ICommand>>>();
            }
        }
        
        public class ConcreteCommand : ICommand
        {
            public void Execute()
            {
            }
        }

        public class Generic<T> : IGeneric<T>
        {
        }

        public class ClassDependingOn<TDependency>
        {
            public ClassDependingOn(TDependency dependency)
            {
            }
        }

        public class ClassDependingOn<TDependency1, TDependency2>
        {
            public ClassDependingOn(TDependency1 dependency1, TDependency2 dependency2)
            {
            }
        }

        public class DisposableCommand : ICommand, IDisposable
        {
            public event Action<DisposableCommand> Disposing;

            public int DisposeCount { get; private set; }

            public bool HasBeenDisposed
            {
                get { return this.DisposeCount > 0; }
            }

            public void Dispose()
            {
                this.DisposeCount++;

                if (this.Disposing != null)
                {
                    this.Disposing(this);
                }
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
        
        private sealed class InjectProperties<TAttribute> : IPropertySelectionBehavior
            where TAttribute : Attribute
        {
            public bool SelectProperty(Type serviceType, PropertyInfo propertyInfo)
            {
                return propertyInfo.GetCustomAttribute<TAttribute>() != null;
            }
        }
    }

    public class Outer : DisposableBase
    {
        public Outer(Middle dependency, Action<object> disposing)
            : base(disposing)
        {
            Debug.WriteLine(this.GetType().Name + " created.");
        }
    }

    public class Middle : DisposableBase
    {
        public Middle(Inner dependency, Action<object> disposing)
            : base(disposing)
        {
            Debug.WriteLine(this.GetType().Name + " created.");
        }
    }

    public class Inner : DisposableBase
    {
        public Inner(Action<object> disposing)
            : base(disposing)
        {
            Debug.WriteLine(this.GetType().Name + " created.");
        }

        [Import]
        public PropertyDependency PropertyDependency { get; set; }
    }

    public class PropertyDependency : DisposableBase
    {
        public PropertyDependency(Action<object> disposing)
            : base(disposing)
        {
            Debug.WriteLine(this.GetType().Name + " created.");
        }
    }

    public abstract class DisposableBase : IDisposable
    {
        private readonly Action<object> disposing;

        protected DisposableBase(Action<object> disposing)
        {
            this.disposing = disposing;
        }

        public void Dispose()
        {
            this.disposing(this);
        }
    }
}