namespace SimpleInjector.Extensions.LifetimeScoping.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;
    using SimpleInjector.Tests.Unit;

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

            container.Register(typeof(IGeneric<>), typeof(Generic<>), new LifetimeScopeLifestyle());

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

            container.Register(typeof(IGeneric<>), typeof(Generic<>), hybrid);

            container.Register<ClassDependingOn<IGeneric<int>>>();

            // Act
            container.Verify();
        }

        [TestMethod]
        public void GetCurrentLifetimeScope_InsideANestedLifetimeScope_ReturnsTheInnerMostScope()
        {
            // Arrange
            var container = new Container();

            var lifestyle = new LifetimeScopeLifestyle();

            using (var outerScope = container.BeginLifetimeScope())
            {
                using (var innerScope = container.BeginLifetimeScope())
                {
                    Assert.IsFalse(object.ReferenceEquals(outerScope, innerScope), "Test setup failed.");

                    // Act
                    var currentScope = lifestyle.GetCurrentScope(container);

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

            container.Register<ICommand, ConcreteCommand>(new LifetimeScopeLifestyle());

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
        public void GetInstance_WithinLifetimeScope_ReturnsInstanceOfExpectedType()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new LifetimeScopeLifestyle());

            using (container.BeginLifetimeScope())
            {
                // Act
                var actualInstance = container.GetInstance<ICommand>();

                // Assert
                AssertThat.IsInstanceOfType(typeof(ConcreteCommand), actualInstance);
            }
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesWithinSingleLifetimeScope_ReturnsASingleInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new LifetimeScopeLifestyle());

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

            container.Register<ICommand, ConcreteCommand>(new LifetimeScopeLifestyle());

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

            container.Register<ICommand, ConcreteCommand>(new LifetimeScopeLifestyle());

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

            container.Register<DisposableCommand>(new LifetimeScopeLifestyle());

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

            var lifestyle = new LifetimeScopeLifestyle();

            // Transient
            container.Register<ICommand, DisposableCommand>();

            container.RegisterInitializer<DisposableCommand>(instance =>
            {
                Scope scope = lifestyle.GetCurrentScope(container);

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

            using (var scope = container.BeginLifetimeScope())
            {
                // Act
                Action action = () => scope.RegisterForDisposal(null);

                // Assert
                AssertThat.Throws<ArgumentNullException>(action);
            }
        }

        [TestMethod]
        public void LifetimeScopeDispose_OnLifetimeScopedObject_EnsuresInstanceGetDisposedAfterLifetimeScope()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, DisposableCommand>(new LifetimeScopeLifestyle());

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

            container.Register<DisposableCommand>(new LifetimeScopeLifestyle());

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

            container.Register<DisposableCommand>(Lifestyle.Singleton);

            container.Register<ICommand, DisposableCommand>(new LifetimeScopeLifestyle());

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
        public void LifetimeScopeDispose_RegisteredConcerete_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposableCommand>(new LifetimeScopeLifestyle());

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
        public void LifetimeScopeDispose_Registered_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<IDisposable, DisposableCommand>(new LifetimeScopeLifestyle());

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
        public void GetInstance_CalledMultipleTimesOnALifetimeScopeServiceWithinASingleScope_DisposesThatInstanceOnce()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposableCommand>(new LifetimeScopeLifestyle());

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
            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();

            container.Register<DisposableCommandWithOverriddenEquality1>(Lifestyle.Scoped);
            container.Register<DisposableCommandWithOverriddenEquality2>(Lifestyle.Scoped);

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
                container.Register<ICommand, ConcreteCommand>(new LifetimeScopeLifestyle());

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
        public void BeginLifetimeScope_WithNullArgument_ThrowsExpectedException()
        {
            // Act
            Action action = () => SimpleInjectorLifetimeScopeExtensions.BeginLifetimeScope(null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        // This behavior has changed; we used to disallow this and throw an exception. Disposing a lifetime
        // scope on a different thread however is not necessarily a problem, so we removed this limitation.
        [TestMethod]
        public void LifetimeScopeDispose_ExecutedOnDifferentThreadThanItWasStarted_DoesNotThrowExceptionException()
        {
            // Arrange
            var container = new Container();

            var scope = container.BeginLifetimeScope();

            // Act
            Task.Factory.StartNew(() => scope.Dispose()).Wait();
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
        public void LifetimeScopeDispose_WithTransientRegisteredForDisposal_DisposesThatInstance()
        {
            // Arrange
            DisposableCommand transientInstanceToDispose = null;

            var container = new Container();

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
        public void WhenScopeEnds_NullContainerArgument_ThrowsException()
        {
            // Arrange
            var lifestyle = new LifetimeScopeLifestyle();

            // Act
            Action action = () => lifestyle.WhenScopeEnds(null, () => { });

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void WhenScopeEnds_NullActionArgument_ThrowsException()
        {
            // Arrange
            var lifestyle = new LifetimeScopeLifestyle();

            // Act
            Action action = () => lifestyle.WhenScopeEnds(new Container(), null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void WhenScopeEnds_CalledOutOfTheContextOfALifetimeScope_ThrowsException()
        {
            // Arrange
            var lifestyle = new LifetimeScopeLifestyle();

            var container = new Container();

            container.Register<ConcreteCommand>(lifestyle);

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
            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();

            // Outer, Middle and Inner all depend on Func<object> and call it when disposed.
            // This way we can check in which order the instances are disposed.
            container.RegisterSingleton<Action<object>>(instance => actualOrderOfDisposal.Add(instance.GetType()));

            // Outer depends on Middle that depends on Inner. 
            // Registration is deliberately made in a different order to prevent that the order of
            // registration might influence the order of disposing.
            container.Register<Middle>(Lifestyle.Scoped);
            container.Register<Inner>(Lifestyle.Scoped);
            container.Register<Outer>(Lifestyle.Scoped);

            Scope scope = container.BeginLifetimeScope();

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
            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();

            // Allow PropertyDependency to be injected as property on Inner 
            container.Options.PropertySelectionBehavior = new InjectProperties<ImportAttribute>();

            // PropertyDependency, Middle and Inner all depend on Func<object> and call it when disposed.
            // This way we can check in which order the instances are disposed.
            container.RegisterSingleton<Action<object>>(instance => actualOrderOfDisposal.Add(instance.GetType()));

            // Middle depends on Inner that depends on property PropertyDependency. 
            // Registration is deliberately made in a different order to prevent that the order of
            // registration might influence the order of disposing.
            container.Register<PropertyDependency>(Lifestyle.Scoped);
            container.Register<Middle>(Lifestyle.Scoped);
            container.Register<Inner>(Lifestyle.Scoped);

            // Act
            Scope scope = container.BeginLifetimeScope();

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
        public void GetInstance_WithPossibleObjectGraphOptimizableRegistration_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, DisposableCommand>(new LifetimeScopeLifestyle());

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

        [TestMethod]
        public void BuildExpression_ManuallyCompiledToDelegate_CanBeExecutedSuccessfully()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new LifetimeScopeLifestyle());

            var factory = Expression.Lambda<Func<ICommand>>(
                container.GetRegistration(typeof(ICommand)).BuildExpression())
                .Compile();

            using (container.BeginLifetimeScope())
            {
                // Act
                factory();
            }
        }
                
        [TestMethod]
        public void GetCurrentLifetimeScope_AfterMiddleScopeDisposedWhileInnerScopeNotDisposed_ReturnsOuterScope()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();

            var instanceToDispose = new DisposableCommand();

            using (var outerScope = container.BeginLifetimeScope())
            {
                var middleScope = container.BeginLifetimeScope();

                var innerScope = container.BeginLifetimeScope();

                middleScope.Dispose();

                // Act
                Scope actualScope = Lifestyle.Scoped.GetCurrentScope(container);

                string scopeName =
                    object.ReferenceEquals(actualScope, null) ? "null" :
                    object.ReferenceEquals(actualScope, innerScope) ? "inner" :
                    object.ReferenceEquals(actualScope, middleScope) ? "middle" :
                    object.ReferenceEquals(actualScope, outerScope) ? "outer" : 
                    "other";

                // Assert
                Assert.AreSame(outerScope, actualScope, "Expected: outer. Actual: " + scopeName + " scope.");
            }
        }

        [TestMethod]
        public void GetCurrentLifetimeScope_DisposingTheMiddleScopeBeforeInnerScope_ReturnsOuterScope()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();

            var instanceToDispose = new DisposableCommand();

            using (Scope outerScope = container.BeginLifetimeScope())
            {
                Scope middleScope = container.BeginLifetimeScope();

                Scope innerScope = container.BeginLifetimeScope();

                middleScope.Dispose();
                innerScope.Dispose();

                // Act
                Scope actualScope = Lifestyle.Scoped.GetCurrentScope(container);

                // Assert
                Assert.AreSame(outerScope, actualScope,
                    "Since the middle scope is already disposed, the current scope should be the outer.");
            }
        }

        [TestMethod]
        public void GetCurrentLifetimeScope_DisposingAnInnerScope_ShouldNeverCauseToBeSetToInnerScope()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();

            var instanceToDispose = new DisposableCommand();

            using (var outerScope = container.BeginLifetimeScope())
            {
                var outerMiddleScope = container.BeginLifetimeScope();

                var innerMiddleScope = container.BeginLifetimeScope();

                var innerScope = container.BeginLifetimeScope();

                // This will cause GetCurrentLifetimeScope to become outerScope.
                outerMiddleScope.Dispose();

                // This should not cause BeginLifetimeScope to change
                innerScope.Dispose();

                // Act
                Scope actualScope = Lifestyle.Scoped.GetCurrentScope(container);

                // Assert
                Assert.AreSame(outerScope, actualScope,
                    "Even though the inner middle scope never got disposed, the inner scope should not " +
                    "this scope upon disposal. The outer scope should retain focus.");
            }
        }
        
        [TestMethod]
        public void Dispose_ObjectRegisteredForDisposalUsingRequestedCurrentLifetimeScope_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();

            var instanceToDispose = new DisposableCommand();

            container.Register<DisposableCommand>(new LifetimeScopeLifestyle());

            using (container.BeginLifetimeScope())
            {
                var command = container.GetInstance<DisposableCommand>();

                command.Disposing += s => Lifestyle.Scoped.RegisterForDisposal(container, instanceToDispose);

                // Act
            }

            // Assert
            Assert.IsTrue(instanceToDispose.HasBeenDisposed);
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

            public bool HasBeenDisposed => this.DisposeCount > 0;

            public void Dispose()
            {
                this.DisposeCount++;

                this.Disposing?.Invoke(this);
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

    public class DisposableLogger : ILogger, IDisposable
    {
        private static readonly Action<DisposableLogger> Empty = l => { };

        private readonly Action<DisposableLogger> disposing;

        public DisposableLogger(Action<DisposableLogger> disposing = null)
        {
            this.disposing = disposing ?? Empty;
        }

        public void Log(string message)
        {
        }

        public void Dispose()
        {
            this.disposing(this);
        }
    }
}