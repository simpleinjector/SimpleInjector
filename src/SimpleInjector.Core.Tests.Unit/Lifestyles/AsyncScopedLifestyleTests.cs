namespace SimpleInjector.Extensions.ExecutionContextScoping.Tests.Unit
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
    using SimpleInjector.Lifestyles;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class AsyncScopedLifestyleTests
    {
        [TestMethod]
        public void Async_AsyncScopedLifestyle_Nesting()
        {
            var container = new Container();

            container.Register<DisposableCommand>(new AsyncScopedLifestyle());

            DisposableCommand cmd;
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                cmd = container.GetInstance<DisposableCommand>();
                Outer(container, cmd).Wait();

                Assert.IsFalse(cmd.HasBeenDisposed);
            }

            Assert.IsTrue(cmd.HasBeenDisposed);
        }

        [TestMethod]
        public void GetInstance_RegistrationUsingAsyncScopedLifestyle_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new AsyncScopedLifestyle());

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                container.GetInstance<ICommand>();
            }
        }

        [TestMethod]
        public void GetInstance_RegistrationUsingFuncAsyncScopedLifestyle_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand>(() => new ConcreteCommand(), new AsyncScopedLifestyle());

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                container.GetInstance<ICommand>();
            }
        }

        [TestMethod]
        public void BeginScope_WithoutAnyAsyncScopedLifestyleRegistrations_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            using (var scope = AsyncScopedLifestyle.BeginScope(container))
            {
                container.GetInstance<ConcreteCommand>();
            }
        }

        [TestMethod]
        public void BeginScope_WithoutAnyAsyncScopedLifestyleRegistrations_Succeeds2()
        {
            // Arrange
            var container = new Container();

            // Act
            using (AsyncScopedLifestyle.BeginScope(container))
            {
            }
        }

        [TestMethod]
        public void ContainerVerify_WithAsyncScopedLifestyleRegistrationInOpenGeneric_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(IGeneric<>), typeof(Generic<>), new AsyncScopedLifestyle());

            container.Register<ClassDependingOn<IGeneric<int>>>();

            // Act
            container.Verify();
        }

        [TestMethod]
        public void ContainerVerify_WithHybridAsyncScopedLifestyleRegistrationInOpenGeneric_Succeeds()
        {
            // Arrange
            var container = new Container();

            var hybrid = Lifestyle.CreateHybrid(() => false, new AsyncScopedLifestyle(), new AsyncScopedLifestyle());

            container.Register(typeof(IGeneric<>), typeof(Generic<>), hybrid);

            container.Register<ClassDependingOn<IGeneric<int>>>();

            // Act
            container.Verify();
        }

        [TestMethod]
        public void GetCurrentAsyncScopedLifestyle_InsideANestedAsyncScopedLifestyle_ReturnsTheInnerMostScope()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            using (Scope outerScope = AsyncScopedLifestyle.BeginScope(container))
            {
                using (Scope innerScope = AsyncScopedLifestyle.BeginScope(container))
                {
                    Assert.IsFalse(object.ReferenceEquals(outerScope, innerScope), "Test setup failed.");

                    // Act
                    Scope currentScope = Lifestyle.Scoped.GetCurrentScope(container);

                    // Assert
                    Assert.IsTrue(object.ReferenceEquals(innerScope, currentScope));
                }
            }
        }

        [TestMethod]
        public void GetInstance_WithoutAsyncScopedLifestyle_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new AsyncScopedLifestyle());

            try
            {
                // Act
                container.GetInstance<ICommand>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.ExceptionMessageContains(@"
                    The ConcreteCommand is registered as 'Async Scoped' lifestyle, but the instance is
                    requested outside the context of an active (Async Scoped) scope."
                    .TrimInside(),
                    ex);
            }
        }

        [TestMethod]
        public void ContainerVerify_WithoutAsyncScopedLifestyle_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new AsyncScopedLifestyle());

            // Act
            container.Verify();
        }

        [TestMethod]
        public void GetInstance_WithinAsyncScopedLifestyle_ReturnsInstanceOfExpectedType()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new AsyncScopedLifestyle());

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                var actualInstance = container.GetInstance<ICommand>();

                // Assert
                AssertThat.IsInstanceOfType(typeof(ConcreteCommand), actualInstance);
            }
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesWithinSingleAsyncScopedLifestyle_ReturnsASingleInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new AsyncScopedLifestyle());

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                var firstInstance = container.GetInstance<ICommand>();
                var secondInstance = container.GetInstance<ICommand>();

                // Assert
                Assert.IsTrue(object.ReferenceEquals(firstInstance, secondInstance));
            }
        }

        [TestMethod]
        public void GetInstance_CalledWithinNestedSingleAsyncScopedLifestyles_ReturnsAnInstancePerScope()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new AsyncScopedLifestyle());

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                var firstInstance = container.GetInstance<ICommand>();

                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    var secondInstance = container.GetInstance<ICommand>();

                    // Assert
                    Assert.IsFalse(object.ReferenceEquals(firstInstance, secondInstance));
                }
            }
        }

        [TestMethod]
        public void GetInstance_CalledWithinSameAsyncScopedLifestyleWithOtherScopesInBetween_ReturnsASingleInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new AsyncScopedLifestyle());

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                var firstInstance = container.GetInstance<ICommand>();

                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    container.GetInstance<ICommand>();
                }

                var secondInstance = container.GetInstance<ICommand>();

                // Assert
                Assert.IsTrue(object.ReferenceEquals(firstInstance, secondInstance));
            }
        }

        [TestMethod]
        public void AsyncScopedLifestyleDispose_RegisterAsyncScopedLifestyleWithDisposal_EnsuresInstanceGetDisposedAfterAsyncScopedLifestyleEnds()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposableCommand>(new AsyncScopedLifestyle());

            DisposableCommand command;

            // Act
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                command = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsTrue(command.HasBeenDisposed, "The instance was expected to be disposed.");
        }

        [TestMethod]
        public void AsyncScopedLifestyleDispose_TransientDisposableObject_DoesNotDisposeInstanceAfterAsyncScopedLifestyleEnds()
        {
            // Arrange
            var container = new Container();

            // Transient
            container.Register<ICommand, DisposableCommand>();

            DisposableCommand command;

            // Act
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                command = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsFalse(command.HasBeenDisposed,
                "The execution scope should not dispose objects that are not explicitly marked as such, since " +
                "this would allow the scope to accidentally dispose singletons.");
        }

        [TestMethod]
        public void AsyncScopedLifestyleDispose_WithInstanceExplicitlyRegisteredForDisposal_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();
            var scopedLifestyle = new AsyncScopedLifestyle();

            // Transient
            container.Register<ICommand, DisposableCommand>();

            container.RegisterInitializer<DisposableCommand>(instance =>
            {
                Scope scope = scopedLifestyle.GetCurrentScope(container);

                // The following line explicitly registers the transient DisposableCommand for disposal when
                // the execution context scope ends.
                scope.RegisterForDisposal(instance);
            });

            DisposableCommand command;

            // Act
            using (AsyncScopedLifestyle.BeginScope(container))
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
            using (var scope = AsyncScopedLifestyle.BeginScope(container))
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
        public void AsyncScopedLifestyleDispose_OnAsyncScopedLifestyledObject_EnsuresInstanceGetDisposedAfterAsyncScopedLifestyle()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, DisposableCommand>(new AsyncScopedLifestyle());

            DisposableCommand command;

            // Act
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                command = container.GetInstance<ICommand>() as DisposableCommand;
            }

            // Assert
            Assert.IsTrue(command.HasBeenDisposed, "The execution scoped instance was expected to be disposed.");
        }

        [TestMethod]
        public void GetInstance_OnAsyncScopedLifestyledObject_WillNotBeDisposedDuringAsyncScopedLifestyle()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposableCommand>(new AsyncScopedLifestyle());

            // Act
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                var command = container.GetInstance<DisposableCommand>();

                // Assert
                Assert.IsFalse(command.HasBeenDisposed, "The instance should not be disposed inside the scope.");
            }
        }

        [TestMethod]
        public void GetInstance_WithinAAsyncScopedLifestyle_NeverDisposesASingleton()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposableCommand>(Lifestyle.Singleton);

            container.Register<ICommand, DisposableCommand>(new AsyncScopedLifestyle());

            DisposableCommand singleton;

            // Act
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                singleton = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsFalse(singleton.HasBeenDisposed, "Singletons should not be disposed.");
        }

        [TestMethod]
        public void AsyncScopedLifestyleDispose_RegisteredConcereteWithExplicitDisposal_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposableCommand>(new AsyncScopedLifestyle());

            DisposableCommand instanceToDispose;

            // Act
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                instanceToDispose = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsTrue(instanceToDispose.HasBeenDisposed);
        }

        [TestMethod]
        public void AsyncScopedLifestyleDispose_RegisteredWithExplicitDisposal_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<IDisposable, DisposableCommand>(new AsyncScopedLifestyle());

            DisposableCommand instanceToDispose;

            // Act
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                instanceToDispose = container.GetInstance<IDisposable>() as DisposableCommand;
            }

            // Assert
            Assert.IsTrue(instanceToDispose.HasBeenDisposed);
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnAAsyncScopedLifestyleServiceWithinASingleScope_DisposesThatInstanceOnce()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposableCommand>(new AsyncScopedLifestyle());

            DisposableCommand command;

            // Act
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                command = container.GetInstance<DisposableCommand>();

                container.GetInstance<DisposableCommand>();
                container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.AreEqual(1, command.DisposeCount, "Dispose should be called exactly once.");
        }

        [TestMethod]
        public void GetInstance_ResolveMultipleAsyncScopedLifestyledServicesWithStrangeEqualsImplementations_CorrectlyDisposesAllInstances()
        {
            // Arrange
            var container = new Container();

            var lifestyle = new AsyncScopedLifestyle();

            container.Register<DisposableCommandWithOverriddenEquality1>(lifestyle);
            container.Register<DisposableCommandWithOverriddenEquality2>(lifestyle);

            // Act
            DisposableCommandWithOverriddenEquality1 command1;
            DisposableCommandWithOverriddenEquality2 command2;

            // Act
            using (AsyncScopedLifestyle.BeginScope(container))
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
                "should be completely independent to this implementation. ";

            Assert.AreEqual(1, command1.DisposeCount, assertMessage + "command1");
            Assert.AreEqual(1, command2.DisposeCount, assertMessage + "command2");
        }

        [TestMethod]
        public void RegisterAsyncScopedLifestyle_CalledAfterInitialization_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            // This locks the container.
            container.GetInstance<ConcreteCommand>();

            try
            {
                // Act
                container.Register<ICommand, ConcreteCommand>(new AsyncScopedLifestyle());

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
        public void BeginScope_WithNullArgument_ThrowsExpectedException()
        {
            // Act
            Action action = () => AsyncScopedLifestyle.BeginScope(null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetInstance_AsyncScopedLifestyledInstanceWithInitializer_CallsInitializerOncePerAsyncScopedLifestyle()
        {
            // Arrange
            int initializerCallCount = 0;

            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new AsyncScopedLifestyle());

            container.RegisterInitializer<ICommand>(command => { initializerCallCount++; });

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                container.GetInstance<ICommand>();
                container.GetInstance<ICommand>();
            }

            // Assert
            Assert.AreEqual(1, initializerCallCount, "The initializer for ICommand is expected to get fired once.");
        }

        [TestMethod]
        public void GetInstance_AsyncScopedLifestyledFuncInstanceWithInitializer_CallsInitializerOncePerAsyncScopedLifestyle()
        {
            // Arrange
            int initializerCallCount = 0;

            var container = new Container();

            container.Register<ICommand>(() => new ConcreteCommand(), new AsyncScopedLifestyle());

            container.RegisterInitializer<ICommand>(command => { initializerCallCount++; });

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                container.GetInstance<ICommand>();
                container.GetInstance<ICommand>();
            }

            // Assert
            Assert.AreEqual(1, initializerCallCount, "The initializer for ICommand is expected to get fired once.");
        }

        [TestMethod]
        public void GetInstance_OnDecoratedAsyncScopedLifestyledInstance_WrapsTheInstanceWithTheDecorator()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new AsyncScopedLifestyle());

            container.RegisterDecorator(typeof(ICommand), typeof(CommandDecorator));

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                ICommand instance = container.GetInstance<ICommand>();

                // Assert
                AssertThat.IsInstanceOfType(typeof(CommandDecorator), instance);

                var decorator = (CommandDecorator)instance;

                AssertThat.IsInstanceOfType(typeof(ConcreteCommand), decorator.DecoratedInstance);
            }
        }

        [TestMethod]
        public void GetInstance_CalledTwiceInOneScopeForDecoratedAsyncScopedLifestyledInstance_WrapsATransientDecoratorAroundAAsyncScopedLifestyledInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, ConcreteCommand>(new AsyncScopedLifestyle());

            container.RegisterDecorator(typeof(ICommand), typeof(CommandDecorator));

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                var decorator1 = (CommandDecorator)container.GetInstance<ICommand>();
                var decorator2 = (CommandDecorator)container.GetInstance<ICommand>();

                // Assert
                Assert.IsFalse(object.ReferenceEquals(decorator1, decorator2),
                    "The decorator should be transient.");

                Assert.IsTrue(object.ReferenceEquals(decorator1.DecoratedInstance, decorator2.DecoratedInstance),
                    "The decorated instance should be scoped per execution context. It seems to be transient.");
            }
        }

        [TestMethod]
        public void GetInstance_CalledTwiceInOneScopeForDecoratedAsyncScopedLifestyledInstance2_WrapsATransientDecoratorAroundAAsyncScopedLifestyledInstance()
        {
            // Arrange
            var container = new Container();

            // Same as previous test, but now with RegisterDecorator called first.
            container.RegisterDecorator(typeof(ICommand), typeof(CommandDecorator));

            container.Register<ICommand, ConcreteCommand>(new AsyncScopedLifestyle());

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                var decorator1 = (CommandDecorator)container.GetInstance<ICommand>();
                var decorator2 = (CommandDecorator)container.GetInstance<ICommand>();

                // Assert
                Assert.IsFalse(object.ReferenceEquals(decorator1, decorator2),
                    "The decorator should be transient but seems to have a scoped lifestyle.");

                Assert.IsTrue(object.ReferenceEquals(decorator1.DecoratedInstance, decorator2.DecoratedInstance),
                    "The decorated instance should be scoped per execution context. It seems to be transient.");
            }
        }
                
        [TestMethod]
        public void ContainerVerify_WithWhenScopeEndsRegistration_Succeeds()
        {
            // Arrange
            var container = new Container();

            var lifestyle = new AsyncScopedLifestyle();

            container.Register<ICommand, DisposableCommand>(lifestyle);

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                lifestyle.WhenScopeEnds(container, () => { });
            });

            // Act
            container.Verify();
        }

        [TestMethod]
        public void AsyncScopedLifestyleDispose_WithWhenScopeEndsRegistration_CallsTheRegisteredAction()
        {
            // Arrange
            int actionCallCount = 0;

            var container = new Container();

            var lifestyle = new AsyncScopedLifestyle();

            container.Register<DisposableCommand, DisposableCommand>(lifestyle);

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                lifestyle.WhenScopeEnds(container, () => { actionCallCount++; });
            });

            var scope = AsyncScopedLifestyle.BeginScope(container);

            try
            {
                container.GetInstance<DisposableCommand>();
            }
            finally
            {
                // Act
                scope.Dispose();
            }

            // Assert
            Assert.AreEqual(1, actionCallCount, "Delegate is expected to be called exactly once.");
        }

        [TestMethod]
        public void AsyncScopedLifestyleDispose_WithWhenScopeEndsRegistration_CallsTheRegisteredActionBeforeCallingDispose()
        {
            // Arrange
            bool delegateHasBeenCalled = false;
            DisposableCommand instanceToDispose = null;

            var container = new Container();

            var lifestyle = new AsyncScopedLifestyle();

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

            var scope = AsyncScopedLifestyle.BeginScope(container);

            try
            {
                instanceToDispose = container.GetInstance<DisposableCommand>();
            }
            finally
            {
                // Act
                scope.Dispose();
            }

            // Assert
            Assert.IsTrue(delegateHasBeenCalled, "Delegate is expected to be called.");
        }

        [TestMethod]
        public void AsyncScopedLifestyleDispose_WithTransientRegisteredForDisposal_DisposesThatInstance()
        {
            // Arrange
            DisposableCommand transientInstanceToDispose = null;

            var container = new Container();

            var lifestyle = new AsyncScopedLifestyle();

            container.RegisterInitializer<DisposableCommand>(command =>
            {
                lifestyle.RegisterForDisposal(container, command);
            });

            var scope = AsyncScopedLifestyle.BeginScope(container);

            try
            {
                transientInstanceToDispose = container.GetInstance<DisposableCommand>();
            }
            finally
            {
                // Act
                scope.Dispose();
            }

            // Assert
            Assert.IsTrue(transientInstanceToDispose.HasBeenDisposed);
        }

        [TestMethod]
        public void WhenScopeEnds_NullContainerArgument_ThrowsException()
        {
            // Arrange
            Container invalidArgument = null;

            var lifestyle = new AsyncScopedLifestyle();

            // Act
            Action action = () => lifestyle.WhenScopeEnds(invalidArgument, () => { });

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void WhenScopeEnds_NullActionArgument_ThrowsException()
        {
            // Arrange
            Action invalidArgument = null;

            var lifestyle = new AsyncScopedLifestyle();

            // Act
            Action action = () => lifestyle.WhenScopeEnds(new Container(), invalidArgument);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void WhenScopeEnds_CalledOutOfTheContextOfAAsyncScopedLifestyle_ThrowsException()
        {
            // Arrange
            var lifestyle = new AsyncScopedLifestyle();

            var container = new Container();

            container.Register<ConcreteCommand>(new AsyncScopedLifestyle());

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
                    "This method can only be called within the context of an active (Async Scoped) scope."),
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
            container.RegisterSingleton<Action<object>>(instance => actualOrderOfDisposal.Add(instance.GetType()));

            // Outer depends on Middle that depends on Inner. 
            // Registration is deliberately made in a different order to prevent that the order of
            // registration might influence the order of disposing.
            var lifestyle = new AsyncScopedLifestyle();

            container.Register<Middle>(lifestyle);
            container.Register<Inner>(lifestyle);
            container.Register<Outer>(lifestyle);

            var scope = AsyncScopedLifestyle.BeginScope(container);

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
            container.RegisterSingleton<Action<object>>(instance => actualOrderOfDisposal.Add(instance.GetType()));

            // Middle depends on Inner that depends on property PropertyDependency. 
            // Registration is deliberately made in a different order to prevent that the order of
            // registration might influence the order of disposing.
            var lifestyle = new AsyncScopedLifestyle();

            container.Register<PropertyDependency>(lifestyle);
            container.Register<Middle>(lifestyle);
            container.Register<Inner>(lifestyle);

            // Act
            var scope = AsyncScopedLifestyle.BeginScope(container);

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
        public void GetCurrentAsyncScopedLifestyle_AfterMiddleScopeDisposedWhileInnerScopeNotDisposed_ReturnsOuterScope()
        {
            // Arrange
            var container = new Container();
            var lifestyle = new AsyncScopedLifestyle();

            var instanceToDispose = new DisposableCommand();

            container.Register<DisposableCommand>(lifestyle);

            using (Scope outerScope = AsyncScopedLifestyle.BeginScope(container))
            {
                Scope middleScope = AsyncScopedLifestyle.BeginScope(container);

                Scope innerScope = AsyncScopedLifestyle.BeginScope(container);

                middleScope.Dispose();

                // Act
                Scope actualScope = lifestyle.GetCurrentScope(container);

                // Assert
                Assert.AreSame(outerScope, actualScope);
            }
        }

        [TestMethod]
        public void GetCurrentAsyncScopedLifestyle_DisposingTheMiddleScopeBeforeInnerScope_ReturnsOuterScope()
        {
            // Arrange
            var container = new Container();
            var lifestyle = new AsyncScopedLifestyle();

            var instanceToDispose = new DisposableCommand();

            container.Register<DisposableCommand>(lifestyle);

            using (Scope outerScope = AsyncScopedLifestyle.BeginScope(container))
            {
                Scope middleScope = AsyncScopedLifestyle.BeginScope(container);

                Scope innerScope = AsyncScopedLifestyle.BeginScope(container);

                middleScope.Dispose();
                innerScope.Dispose();

                // Act
                Scope actualScope = lifestyle.GetCurrentScope(container);

                // Assert
                Assert.AreSame(outerScope, actualScope,
                    "Since the middle scope is already disposed, the current scope should be the outer.");
            }
        }

        [TestMethod]
        public void GetCurrentAsyncScopedLifestyle_DisposingAnInnerScope_ShouldNeverCauseToBeSetToInnerScope()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var instanceToDispose = new DisposableCommand();

            container.Register<DisposableCommand>(Lifestyle.Scoped);

            using (Scope outerScope = AsyncScopedLifestyle.BeginScope(container))
            {
                Scope outerMiddleScope = AsyncScopedLifestyle.BeginScope(container);

                Scope innerMiddleScope = AsyncScopedLifestyle.BeginScope(container);

                Scope innerScope = AsyncScopedLifestyle.BeginScope(container);

                // This will cause GetCurrentAsyncScopedLifestyle to become outerScope.
                outerMiddleScope.Dispose();

                // This should not cause BeginScope to change
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
        public void GetInstance_WithPossibleObjectGraphOptimizableRegistration_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, DisposableCommand>(new AsyncScopedLifestyle());

            container.Register<ClassDependingOn<ICommand>>();

            // This registration can be optimized to prevent ICommand from being requested more than once from
            // the AsyncScopedLifestyleRegistration.GetInstance
            container.Register<ClassDependingOn<ClassDependingOn<ICommand>, ClassDependingOn<ICommand>>>();

            using (AsyncScopedLifestyle.BeginScope(container))
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

            container.Register<ICommand, ConcreteCommand>(new AsyncScopedLifestyle());

            var factory = Expression.Lambda<Func<ICommand>>(
                container.GetRegistration(typeof(ICommand)).BuildExpression())
                .Compile();

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                factory();
            }
        }

        [TestMethod]
        public void Dispose_ObjectRegisteredForDisposalUsingRequestedCurrentLifetimeScope_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var instanceToDispose = new DisposableCommand();

            container.Register<DisposableCommand>(Lifestyle.Scoped);

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                var command = container.GetInstance<DisposableCommand>();

                command.Disposing += s =>
                {
                    Lifestyle.Scoped.RegisterForDisposal(container, instanceToDispose);
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

            using (AsyncScopedLifestyle.BeginScope(container))
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

            using (AsyncScopedLifestyle.BeginScope(container))
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

            public override int GetHashCode() => this.HashCode;

            public override bool Equals(object obj) => this.GetHashCode() == obj.GetHashCode();
        }

        public class CommandDecorator : ICommand
        {
            public CommandDecorator(ICommand decorated)
            {
                this.DecoratedInstance = decorated;
            }

            public ICommand DecoratedInstance { get; }

            public void Execute()
            {
            }
        }

        private sealed class InjectProperties<TAttribute> : IPropertySelectionBehavior
            where TAttribute : Attribute
        {
            public bool SelectProperty(Type t, PropertyInfo p) => p.GetCustomAttribute<TAttribute>() != null;
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