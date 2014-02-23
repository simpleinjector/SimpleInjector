namespace SimpleInjector.Extensions.ExecutionContextScoping.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    [TestClass]
    public class SimpleInjectorExecutionContextScopeExtensionsTests
    {
        [TestMethod]
        public void Async_ExecutionContextScope_Nesting()
        {
            var container = new Container();

            container.Register<DisposableCommand>(new ExecutionContextScopeLifestyle());

            DisposableCommand cmd;
            using (container.BeginExecutionContextScope())
            {
                // Act
                cmd = container.GetInstance<DisposableCommand>();
                Outer(container, cmd).Wait();

                Assert.IsFalse(cmd.HasBeenDisposed);
            }

            Assert.IsTrue(cmd.HasBeenDisposed);
        }

        [TestMethod]
        public void GetInstance_RegistrationUsingExecutionContextScopeLifestyle_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new ExecutionContextScopeLifestyle());

            using (container.BeginExecutionContextScope())
            {
                // Act
                container.GetInstance<ICommand>();
            }
        }

        [TestMethod]
        public void GetInstance_RegistrationUsingFuncExecutionContextScopeLifestyle_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand>(() => new ConcreteCommand(), new ExecutionContextScopeLifestyle());

            using (container.BeginExecutionContextScope())
            {
                // Act
                container.GetInstance<ICommand>();
            }
        }

        [TestMethod]
        public void BeginExecutionContextScope_WithoutAnyExecutionContextScopeRegistrations_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            using (var scope = container.BeginExecutionContextScope())
            {
                container.GetInstance<ConcreteCommand>();
            }
        }

        [TestMethod]
        public void BeginExecutionContextScope_WithoutAnyExecutionContextScopeRegistrations_Succeeds2()
        {
            // Arrange
            var container = new Container();

            // Act
            container.BeginExecutionContextScope();
        }

        [TestMethod]
        public void Verify_WithExecutionContextScopeRegistrationInOpenGeneric_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IGeneric<>), typeof(Generic<>), new ExecutionContextScopeLifestyle());

            container.Register<ClassDependingOn<IGeneric<int>>>();

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_WithHybridExecutionContextScopeRegistrationInOpenGeneric_Succeeds()
        {
            // Arrange
            var container = new Container();

            var hybrid = Lifestyle.CreateHybrid(() => false, new ExecutionContextScopeLifestyle(), new ExecutionContextScopeLifestyle());

            container.RegisterOpenGeneric(typeof(IGeneric<>), typeof(Generic<>), hybrid);

            container.Register<ClassDependingOn<IGeneric<int>>>();

            // Act
            container.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetCurrentExecutionContextScope_WithNullContainerArgument_ThrowsExpectedException()
        {
            // Arrange
            Container container = null;

            // Act
            container.GetCurrentExecutionContextScope();
        }

        [TestMethod]
        public void GetCurrentExecutionContextScope_OutsideTheContextOfAExecutionContextScope_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            // Act
            var currentScope = container.GetCurrentExecutionContextScope();

            // Assert
            Assert.IsNull(currentScope);
        }

        [TestMethod]
        public void GetCurrentExecutionContextScope_OutsideTheContextOfAExecutionContextScopeWithoutScopingEnabled_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            // Act
            var scope = container.GetCurrentExecutionContextScope();

            // Assert
            Assert.IsNull(scope);
        }

        [TestMethod]
        public void GetCurrentExecutionContextScope_InsideTheContextOfAExecutionContextScope_ReturnsThatScope()
        {
            // Arrange
            var container = new Container();

            using (var scope = container.BeginExecutionContextScope())
            {
                // Act
                var currentScope = container.GetCurrentExecutionContextScope();

                // Assert
                Assert.IsNotNull(currentScope);
                Assert.IsTrue(object.ReferenceEquals(scope, currentScope));
            }
        }

        [TestMethod]
        public void GetCurrentExecutionContextScope_InsideANestedExecutionContextScope_ReturnsTheInnerMostScope()
        {
            // Arrange
            var container = new Container();

            using (var outerScope = container.BeginExecutionContextScope())
            {
                using (var innerScope = container.BeginExecutionContextScope())
                {
                    Assert.IsFalse(object.ReferenceEquals(outerScope, innerScope), "Test setup failed.");

                    // Act
                    var currentScope = container.GetCurrentExecutionContextScope();

                    // Assert
                    Assert.IsTrue(object.ReferenceEquals(innerScope, currentScope));
                }
            }
        }

        [TestMethod]
        public void GetInstance_WithoutExecutionContextScope_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new ExecutionContextScopeLifestyle());

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
                    "The ICommand is registered as 'Execution Context Scope' lifestyle, but the instance is requested " +
                    "outside the context of a Execution Context Scope."),
                    "Actual message: " + ex.Message);
            }
        }

        [TestMethod]
        public void Verify_WithoutExecutionContextScope_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new ExecutionContextScopeLifestyle());

            // Act
            container.Verify();
        }

        [TestMethod]
        public void GetInstance_WithinExecutionContextScope_ReturnsInstanceOfExpectedType()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new ExecutionContextScopeLifestyle());

            using (container.BeginExecutionContextScope())
            {
                // Act
                var actualInstance = container.GetInstance<ICommand>();

                // Assert
                Assert.IsInstanceOfType(actualInstance, typeof(ConcreteCommand));
            }
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesWithinSingleExecutionContextScope_ReturnsASingleInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new ExecutionContextScopeLifestyle());

            using (container.BeginExecutionContextScope())
            {
                // Act
                var firstInstance = container.GetInstance<ICommand>();
                var secondInstance = container.GetInstance<ICommand>();

                // Assert
                Assert.IsTrue(object.ReferenceEquals(firstInstance, secondInstance));
            }
        }

        [TestMethod]
        public void GetInstance_CalledWithinNestedSingleExecutionContextScopes_ReturnsAnInstancePerScope()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new ExecutionContextScopeLifestyle());

            using (container.BeginExecutionContextScope())
            {
                // Act
                var firstInstance = container.GetInstance<ICommand>();

                using (container.BeginExecutionContextScope())
                {
                    var secondInstance = container.GetInstance<ICommand>();

                    // Assert
                    Assert.IsFalse(object.ReferenceEquals(firstInstance, secondInstance));
                }
            }
        }

        [TestMethod]
        public void GetInstance_CalledWithinSameExecutionContextScopeWithOtherScopesInBetween_ReturnsASingleInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new ExecutionContextScopeLifestyle());

            using (container.BeginExecutionContextScope())
            {
                // Act
                var firstInstance = container.GetInstance<ICommand>();

                using (container.BeginExecutionContextScope())
                {
                    container.GetInstance<ICommand>();
                }

                var secondInstance = container.GetInstance<ICommand>();

                // Assert
                Assert.IsTrue(object.ReferenceEquals(firstInstance, secondInstance));
            }
        }

        [TestMethod]
        public void ExecutionContextScopeDispose_RegisterExecutionContextScopeWithDisposal_EnsuresInstanceGetDisposedAfterExecutionContextScopeEnds()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposableCommand>(new ExecutionContextScopeLifestyle());

            DisposableCommand command;

            // Act
            using (container.BeginExecutionContextScope())
            {
                command = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsTrue(command.HasBeenDisposed, "The instance was expected to be disposed.");
        }

        [TestMethod]
        public void ExecutionContextScopeDispose_TransientDisposableObject_DoesNotDisposeInstanceAfterExecutionContextScopeEnds()
        {
            // Arrange
            var container = new Container();

            // Transient
            container.Register<ICommand, DisposableCommand>();

            DisposableCommand command;

            // Act
            using (container.BeginExecutionContextScope())
            {
                command = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsFalse(command.HasBeenDisposed,
                "The lifetime scope should not dispose objects that are not explicitly marked as such, since " +
                "this would allow the scope to accidentally dispose singletons.");
        }

        [TestMethod]
        public void ExecutionContextScopeDispose_WithInstanceExplicitlyRegisteredForDisposal_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            // Transient
            container.Register<ICommand, DisposableCommand>();

            container.RegisterInitializer<DisposableCommand>(instance =>
            {
                var scope = container.GetCurrentExecutionContextScope();

                // The following line explictly registers the transient DisposableCommand for disposal when
                // the lifetime scope ends.
                scope.RegisterForDisposal(instance);
            });

            DisposableCommand command;

            // Act
            using (container.BeginExecutionContextScope())
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

            // Act
            using (var scope = container.BeginExecutionContextScope())
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
        public void ExecutionContextScopeDispose_OnExecutionContextScopedObject_EnsuresInstanceGetDisposedAfterExecutionContextScope()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, DisposableCommand>(new ExecutionContextScopeLifestyle());

            DisposableCommand command;

            // Act
            using (container.BeginExecutionContextScope())
            {
                command = container.GetInstance<ICommand>() as DisposableCommand;
            }

            // Assert
            Assert.IsTrue(command.HasBeenDisposed, "The lifetime scoped instance was expected to be disposed.");
        }

        [TestMethod]
        public void GetInstance_OnExecutionContextScopedObject_WillNotBeDisposedDuringExecutionContextScope()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposableCommand>(new ExecutionContextScopeLifestyle());

            // Act
            using (container.BeginExecutionContextScope())
            {
                var command = container.GetInstance<DisposableCommand>();

                // Assert
                Assert.IsFalse(command.HasBeenDisposed, "The instance should not be disposed inside the scope.");
            }
        }

        [TestMethod]
        public void GetInstance_WithinAExecutionContextScope_NeverDisposesASingleton()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<DisposableCommand>();

            container.Register<ICommand, DisposableCommand>(new ExecutionContextScopeLifestyle());

            DisposableCommand singleton;

            // Act
            using (container.BeginExecutionContextScope())
            {
                singleton = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsFalse(singleton.HasBeenDisposed, "Singletons should not be disposed.");
        }

        [TestMethod]
        public void ExecutionContextScopeDispose_RegisteredConcereteWithExplicitDisposal_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposableCommand>(new ExecutionContextScopeLifestyle(true));

            DisposableCommand instanceToDispose;

            // Act
            using (container.BeginExecutionContextScope())
            {
                instanceToDispose = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsTrue(instanceToDispose.HasBeenDisposed);
        }

        [TestMethod]
        public void ExecutionContextScopeDispose_RegisteredConcereteWithoutDisposal_DoesNotDisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposableCommand>(new ExecutionContextScopeLifestyle(false));

            DisposableCommand instanceToDispose;

            // Act
            using (container.BeginExecutionContextScope())
            {
                instanceToDispose = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsFalse(instanceToDispose.HasBeenDisposed);
        }

        [TestMethod]
        public void ExecutionContextScopeDispose_RegisteredWithExplicitDisposal_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<IDisposable, DisposableCommand>(new ExecutionContextScopeLifestyle(true));

            DisposableCommand instanceToDispose;

            // Act
            using (container.BeginExecutionContextScope())
            {
                instanceToDispose = container.GetInstance<IDisposable>() as DisposableCommand;
            }

            // Assert
            Assert.IsTrue(instanceToDispose.HasBeenDisposed);
        }

        [TestMethod]
        public void ExecutionContextScopeDispose_RegisteredWithoutDisposal_DoesNotDisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<IDisposable, DisposableCommand>(new ExecutionContextScopeLifestyle(false));

            DisposableCommand instanceToDispose;

            // Act
            using (container.BeginExecutionContextScope())
            {
                instanceToDispose = container.GetInstance<IDisposable>() as DisposableCommand;
            }

            // Assert
            Assert.IsFalse(instanceToDispose.HasBeenDisposed);
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnAExecutionContextScopeServiceWithinASingleScope_DisposesThatInstanceOnce()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposableCommand>(new ExecutionContextScopeLifestyle());

            DisposableCommand command;

            // Act
            using (container.BeginExecutionContextScope())
            {
                command = container.GetInstance<DisposableCommand>();

                container.GetInstance<DisposableCommand>();
                container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.AreEqual(1, command.DisposeCount, "Dispose should be called exactly once.");
        }

        [TestMethod]
        public void GetInstance_ResolveMultipleExecutionContextScopedServicesWithStrangeEqualsImplementations_CorrectlyDisposesAllInstances()
        {
            // Arrange
            var container = new Container();

            var lifestyle = new ExecutionContextScopeLifestyle();

            container.Register<DisposableCommandWithOverriddenEquality1>(lifestyle);
            container.Register<DisposableCommandWithOverriddenEquality2>(lifestyle);

            // Act
            DisposableCommandWithOverriddenEquality1 command1;
            DisposableCommandWithOverriddenEquality2 command2;

            // Act
            using (container.BeginExecutionContextScope())
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
        public void RegisterExecutionContextScope_CalledAfterInitialization_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            // This locks the container.
            container.GetInstance<ConcreteCommand>();

            try
            {
                // Act
                container.Register<ICommand, ConcreteCommand>(new ExecutionContextScopeLifestyle());

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
        public void BeginExecutionContextScope_WithNullArgument_ThrowsExpectedException()
        {
            SimpleInjectorExecutionContextScopeExtensions.BeginExecutionContextScope(null);
        }
        
        [TestMethod]
        public void GetInstance_ExecutionContextScopedInstanceWithInitializer_CallsInitializerOncePerExecutionContextScope()
        {
            // Arrange
            int initializerCallCount = 0;

            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new ExecutionContextScopeLifestyle());

            container.RegisterInitializer<ICommand>(command => { initializerCallCount++; });

            using (container.BeginExecutionContextScope())
            {
                // Act
                container.GetInstance<ICommand>();
                container.GetInstance<ICommand>();
            }

            // Assert
            Assert.AreEqual(1, initializerCallCount, "The initializer for ICommand is expected to get fired once.");
        }

        [TestMethod]
        public void GetInstance_ExecutionContextScopedFuncInstanceWithInitializer_CallsInitializerOncePerExecutionContextScope()
        {
            // Arrange
            int initializerCallCount = 0;

            var container = new Container();

            container.Register<ICommand>(() => new ConcreteCommand(), new ExecutionContextScopeLifestyle());

            container.RegisterInitializer<ICommand>(command => { initializerCallCount++; });

            using (container.BeginExecutionContextScope())
            {
                // Act
                container.GetInstance<ICommand>();
                container.GetInstance<ICommand>();
            }

            // Assert
            Assert.AreEqual(1, initializerCallCount, "The initializer for ICommand is expected to get fired once.");
        }

        [TestMethod]
        public void GetInstance_OnDecoratedExecutionContextScopedInstance_WrapsTheInstanceWithTheDecorator()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new ExecutionContextScopeLifestyle());

            container.RegisterDecorator(typeof(ICommand), typeof(CommandDecorator));

            using (container.BeginExecutionContextScope())
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
        public void GetInstance_CalledTwiceInOneScopeForDecoratedExecutionContextScopedInstance_WrapsATransientDecoratorAroundAExecutionContextScopedInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new ExecutionContextScopeLifestyle());

            container.RegisterDecorator(typeof(ICommand), typeof(CommandDecorator));

            using (container.BeginExecutionContextScope())
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
        public void GetInstance_CalledTwiceInOneScopeForDecoratedExecutionContextScopedInstance2_WrapsATransientDecoratorAroundAExecutionContextScopedInstance()
        {
            // Arrange
            var container = new Container();

            // Same as previous test, but now with RegisterDecorator called first.
            container.RegisterDecorator(typeof(ICommand), typeof(CommandDecorator));

            container.Register<ICommand, ConcreteCommand>(new ExecutionContextScopeLifestyle());

            using (container.BeginExecutionContextScope())
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
        public void ExecutionContextScope_TwoScopedRegistationsForTheSameServiceType_CreatesTwoInstances()
        {
            // Arrange
            var lifestyle = new ExecutionContextScopeLifestyle();

            var container = new Container();

            var reg1 = lifestyle.CreateRegistration<ICommand, DisposableCommand>(container);
            var reg2 = lifestyle.CreateRegistration<ICommand, DisposableCommand>(container);

            container.AppendToCollection(typeof(ICommand), reg1);
            container.AppendToCollection(typeof(ICommand), reg2);

            using (container.BeginExecutionContextScope())
            {
                // Act
                var commands = container.GetAllInstances<ICommand>().Cast<DisposableCommand>().ToArray();

                // Assert
                Assert.AreNotSame(commands[0], commands[1], "Two instances were expected.");
            }
        }

        [TestMethod]
        public void ExecutionContextScopeDispose_TwoScopedRegistationsForTheSameServiceType_DisposesBothInstances()
        {
            // Arrange
            var disposedInstances = new HashSet<object>();

            var lifestyle = new ExecutionContextScopeLifestyle();

            var container = new Container();

            var reg1 = lifestyle.CreateRegistration<ICommand, DisposableCommand>(container);
            var reg2 = lifestyle.CreateRegistration<ICommand, DisposableCommand>(container);

            container.AppendToCollection(typeof(ICommand), reg1);
            container.AppendToCollection(typeof(ICommand), reg2);

            using (container.BeginExecutionContextScope())
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

            var lifestyle = new ExecutionContextScopeLifestyle();

            container.Register<ICommand, DisposableCommand>(lifestyle);

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                lifestyle.WhenScopeEnds(container, () => { });
            });

            // Act
            container.Verify();
        }

        [TestMethod]
        public void ExecutionContextScopeDispose_WithWhenScopeEndsRegistration_CallsTheRegisteredAction()
        {
            // Arrange
            int actionCallCount = 0;

            var container = new Container();

            var lifestyle = new ExecutionContextScopeLifestyle();

            container.Register<DisposableCommand, DisposableCommand>(lifestyle);

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                lifestyle.WhenScopeEnds(container, () => { actionCallCount++; });
            });

            using (container.BeginExecutionContextScope())
            {
                container.GetInstance<DisposableCommand>();

                // Act
            }

            // Assert
            Assert.AreEqual(1, actionCallCount, "Delegate is expected to be called exactly once.");
        }

        [TestMethod]
        public void ExecutionContextScopeDispose_WithWhenScopeEndsRegistration_CallsTheRegisteredActionBeforeCallingDispose()
        {
            // Arrange
            bool delegateHasBeenCalled = false;
            DisposableCommand instanceToDispose = null;

            var container = new Container();

            var lifestyle = new ExecutionContextScopeLifestyle();

            container.Register<DisposableCommand, DisposableCommand>(lifestyle);

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                lifestyle.WhenScopeEnds(container, () =>
                {
                    Assert.IsFalse(command.HasBeenDisposed,
                        "The action should be called before disposing the instance, because users are " +
                        "to use those instances.");
                    delegateHasBeenCalled = true;
                });
            });

            using (container.BeginExecutionContextScope())
            {
                instanceToDispose = container.GetInstance<DisposableCommand>();

                // Act
            }

            // Assert
            Assert.IsTrue(delegateHasBeenCalled, "Delegate is expected to be called.");
        }

        [TestMethod]
        public void ExecutionContextScopeDispose_WithTransientRegisteredForDisposal_DisposesThatInstance()
        {
            // Arrange
            DisposableCommand transientInstanceToDispose = null;

            var container = new Container();

            var lifestyle = new ExecutionContextScopeLifestyle();

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                lifestyle.RegisterForDisposal(container, command);
            });

            using (container.BeginExecutionContextScope())
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
            var lifestyle = new ExecutionContextScopeLifestyle();

            // Act
            lifestyle.WhenScopeEnds(null, () => { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenScopeEnds_NullActionArgument_ThrowsException()
        {
            // Arrange
            var lifestyle = new ExecutionContextScopeLifestyle();

            // Act
            lifestyle.WhenScopeEnds(new Container(), null);
        }

        [TestMethod]
        public void WhenScopeEnds_CalledOutOfTheContextOfAExecutionContextScope_ThrowsException()
        {
            // Arrange
            var lifestyle = new ExecutionContextScopeLifestyle();

            var container = new Container();

            container.Register<ConcreteCommand>(new ExecutionContextScopeLifestyle());

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
                    "This method can only be called within the context of an active Execution Context Scope."),
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
            var lifestyle = new ExecutionContextScopeLifestyle();

            container.Register<Middle>(lifestyle);
            container.Register<Inner>(lifestyle);
            container.Register<Outer>(lifestyle);

            var scope = container.BeginExecutionContextScope();

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
            var lifestyle = new ExecutionContextScopeLifestyle();

            container.Register<PropertyDependency>(lifestyle);
            container.Register<Middle>(lifestyle);
            container.Register<Inner>(lifestyle);

            // Act
            var scope = container.BeginExecutionContextScope();

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

            container.Register<DisposableCommand>(new ExecutionContextScopeLifestyle());

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                commands[command] = !IsDisposed;

                command.Disposing += _ =>
                {
                    commands[command] = IsDisposed;
                };
            });

            var outerScope = container.BeginExecutionContextScope();

            container.GetInstance<DisposableCommand>();

            container.BeginExecutionContextScope();

            container.GetInstance<DisposableCommand>();

            container.BeginExecutionContextScope();

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

            container.Register<DisposableCommand>(new ExecutionContextScopeLifestyle());

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                commands[command] = !IsDisposed;

                command.Disposing += _ =>
                {
                    commands[command] = IsDisposed;
                };
            });

            var outerScope = container.BeginExecutionContextScope();

            try
            {
                container.GetInstance<DisposableCommand>();

                var middelScope = container.BeginExecutionContextScope();

                container.GetInstance<DisposableCommand>();

                container.BeginExecutionContextScope();

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

            container.Register<ICommand, DisposableCommand>(new ExecutionContextScopeLifestyle());

            container.Register<ClassDependingOn<ICommand>>();

            // This registration can be optimized to prevent ICommand from being requested more than once from
            // the ExecutionContextScopeLifestyleRegistration.GetInstance
            container.Register<ClassDependingOn<ClassDependingOn<ICommand>, ClassDependingOn<ICommand>>>();

            using (container.BeginExecutionContextScope())
            {
                // Act
                var instance =
                    container.GetInstance<ClassDependingOn<ClassDependingOn<ICommand>, ClassDependingOn<ICommand>>>();
            }
        }

        [TestMethod]
        public void BuildExpression_ManuallyCompiledToDelegate_CanBeExecutedSuccessfully()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new ExecutionContextScopeLifestyle());

            var factory = Expression.Lambda<Func<ICommand>>(
                container.GetRegistration(typeof(ICommand)).BuildExpression())
                .Compile();

            using (container.BeginExecutionContextScope())
            {
                // Act
                factory();
            }
        }
        
        [TestMethod]
        public void Dispose_OnlyDisposingMostOuterScopeWhileMostInnerScopeDisposeThrows_AlsoDisposesMiddleScopes()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposableCommand>(new ExecutionContextScopeLifestyle());

            var outerScope = container.BeginExecutionContextScope();
            var middleScope = container.BeginExecutionContextScope();

            var middleScopeInstance = container.GetInstance<DisposableCommand>();

            var innerScope = container.BeginExecutionContextScope();

            var innerScopeInstance = container.GetInstance<DisposableCommand>();

            Assert.AreNotSame(middleScopeInstance, innerScopeInstance, "Test setup fail.");

            innerScopeInstance.Disposing += (s) =>
            {
                throw new Exception("Bang!");
            };

            try
            {
                // Act
                outerScope.Dispose();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch
            {
            }

            Assert.IsTrue(middleScopeInstance.HasBeenDisposed,
                "The LifetimeScope must ensure that all inner scopes are disposed, even if one of those " +
                "inner scopes throws an exception.");
        }

        [TestMethod]
        public void Dispose_ObjectRegisteredForDisposalUsingRequestedCurrentLifetimeScope_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            var instanceToDispose = new DisposableCommand();

            container.Register<DisposableCommand>(new ExecutionContextScopeLifestyle());

            using (container.BeginExecutionContextScope())
            {
                var command = container.GetInstance<DisposableCommand>();

                command.Disposing += s =>
                {
                    container.GetCurrentExecutionContextScope().RegisterForDisposal(instanceToDispose);
                };

                // Act
            }

            // Assert
            Assert.IsTrue(instanceToDispose.HasBeenDisposed);
        }
        
        private static async Task Inner(Container container, ICommand command)
        {
            DisposableCommand cmd1, cmd2;

            await Task.Yield();

            cmd1 = container.GetInstance<DisposableCommand>();
            Assert.AreSame(command, cmd1);

            using (container.BeginExecutionContextScope())
            {
                // Act
                cmd2 = container.GetInstance<DisposableCommand>();

                Assert.AreNotSame(command, cmd2);
                Assert.IsFalse(cmd2.HasBeenDisposed);
            }

            Assert.IsTrue(cmd2.HasBeenDisposed);
        }

        private static async Task Outer(Container container, ICommand command)
        {
            DisposableCommand cmd1, cmd2;

            await Task.Yield();

            cmd1 = container.GetInstance<DisposableCommand>();
            Assert.AreSame(command, cmd1);

            using (container.BeginExecutionContextScope())
            {
                // Act
                cmd2 = container.GetInstance<DisposableCommand>();
                Assert.AreNotSame(command, cmd2);

                var t1 = Inner(container, cmd2);
                var t2 = Inner(container, cmd2);

                await Task.WhenAll(t1, t2).ConfigureAwait(false);
                Assert.IsFalse(cmd2.HasBeenDisposed);
            }

            Assert.IsFalse(cmd1.HasBeenDisposed);
            Assert.IsTrue(cmd2.HasBeenDisposed);
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