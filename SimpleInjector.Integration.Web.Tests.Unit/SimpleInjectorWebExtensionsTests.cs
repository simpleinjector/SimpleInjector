namespace SimpleInjector.Integration.Web.Tests.Unit
{
    using System;
    using System.Web;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    using SimpleInjector.Extensions;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class SimpleInjectorWebExtensionsTests
    {
        [TestMethod]
        public void RegisterPerWebRequest_CalledASingleTime_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterPerWebRequest<ConcreteCommand>();
        }

        [TestMethod]
        public void RegisterPerWebRequest_CalledMultipleTimes_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterPerWebRequest<ConcreteCommand>();
            container.RegisterPerWebRequest<ICommand>(() => new ConcreteCommand());
        }

        [TestMethod]
        public void Verify_WithNoHttpContext_Succeeds()
        {
            // Arrange
            Assert.IsNull(HttpContext.Current, "Test setup failed.");

            var container = new Container();

            container.RegisterPerWebRequest<ConcreteCommand>();

            // Act
            container.Verify();
        }

        [TestMethod]
        public void GetInstance_WithinHttpContext_ReturnsInstanceOfExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterPerWebRequest<ICommand, ConcreteCommand>();

            using (new HttpContextScope())
            {
                // Act
                var actualInstance = container.GetInstance<ICommand>();

                // Assert
                AssertThat.IsInstanceOfType(typeof(ConcreteCommand), actualInstance);
            }
        }

        [TestMethod]
        public void GetInstance_WithNoHttpContext_FailsWithExpectedException()
        {
            // Arrange
            Assert.IsNull(HttpContext.Current, "Test setup failed.");

            var container = new Container();

            container.RegisterPerWebRequest<ICommand, ConcreteCommand>();

            try
            {
                // Act
                container.GetInstance<ICommand>();

                // Assert
                Assert.Fail("Exception expected");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(typeof(ICommand).Name), "Actual: " + ex.Message);
                Assert.IsTrue(
                    ex.Message.Contains("the instance is requested outside the context of a Web Request"),
                    "Actual: " + ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesWithinSingleLifetimeScope_ReturnsASingleInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterPerWebRequest<ICommand, ConcreteCommand>();

            using (new HttpContextScope())
            {
                // Act
                var firstInstance = container.GetInstance<ICommand>();
                var secondInstance = container.GetInstance<ICommand>();

                // Assert
                Assert.IsTrue(object.ReferenceEquals(firstInstance, secondInstance));
            }
        }

        [TestMethod]
        public void RegisterPerWebRequest_WithDisposal_EnsuresInstanceGetDisposedAfterRequestEnds()
        {
            // Arrange
            var container = new Container();

            container.RegisterPerWebRequest<DisposableCommand>();

            DisposableCommand command;

            // Act
            using (new HttpContextScope())
            {
                command = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsTrue(command.HasBeenDisposed, "The instance was expected to be disposed.");
        }

        [TestMethod]
        public void RegisterPerWebRequestDispose_TransientDisposableObject_DoesNotDisposeInstanceAfterLifetimeScopeEnds()
        {
            // Arrange
            var container = new Container();

            // Transient
            container.Register<ICommand, DisposableCommand>();

            DisposableCommand command;

            // Act
            using (new HttpContextScope())
            {
                command = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsFalse(command.HasBeenDisposed,
                "The lifetime scope should not dispose objects that are not explicitly marked as such, since " +
                "this would allow the scope to accidentally dispose singletons.");
        }

        [TestMethod]
        public void GetInstance_OnPerWebRequesstInstance_WillNotBeDisposedDuringTheRequest()
        {
            // Arrange
            var container = new Container();

            container.RegisterPerWebRequest<DisposableCommand>();

            // Act
            using (new HttpContextScope())
            {
                var command = container.GetInstance<DisposableCommand>();

                // Assert
                Assert.IsFalse(command.HasBeenDisposed, "The instance should not be disposed inside the scope.");
            }
        }

        [TestMethod]
        public void GetInstance_WithinAWebRequest_NeverDisposesASingleton()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposableCommand>(Lifestyle.Singleton);

            container.RegisterPerWebRequest<IDisposable, DisposableCommand>();

            DisposableCommand singleton;

            // Act
            using (new HttpContextScope())
            {
                singleton = container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.IsFalse(singleton.HasBeenDisposed, "Singletons should not be disposed.");
        }
        
        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnAPerWebRequestServiceWithinASingleScope_DisposesThatInstanceOnce()
        {
            // Arrange
            var container = new Container();

            container.RegisterPerWebRequest<DisposableCommand>();

            DisposableCommand command;

            // Act
            using (new HttpContextScope())
            {
                command = container.GetInstance<DisposableCommand>();

                container.GetInstance<DisposableCommand>();
                container.GetInstance<DisposableCommand>();
            }

            // Assert
            Assert.AreEqual(1, command.DisposeCount, "Dispose should be called exactly once.");
        }
        
        [TestMethod]
        public void GetInstance_ResolveMultipleWebRequestServicesWithStrangeEqualsImplementations_CorrectlyDisposesAllInstances()
        {
            // Arrange
            var container = new Container();

            container.RegisterPerWebRequest<DisposableCommandWithOverriddenEquality1>();
            container.RegisterPerWebRequest<DisposableCommandWithOverriddenEquality2>();

            // Act
            DisposableCommandWithOverriddenEquality1 command1;
            DisposableCommandWithOverriddenEquality2 command2;

            // Act
            using (new HttpContextScope())
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
        public void RegisterPerWebRequest_CalledAfterInitialization_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            // This locks the container.
            container.GetInstance<ConcreteCommand>();

            try
            {
                // Act
                container.RegisterPerWebRequest<ICommand, ConcreteCommand>();

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
        public void RegisterLifetimeScopeTConcrete_WithNullArgument_ThrowsExpectedException()
        {
            // Act
            Action action = () =>
                SimpleInjectorWebExtensions.RegisterPerWebRequest<ConcreteCommand>(null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterLifetimeScopeTServiceTImplementation_WithNullArgument_ThrowsExpectedException()
        {
            // Act
            Action action = () =>
                SimpleInjectorWebExtensions.RegisterPerWebRequest<ICommand, ConcreteCommand>(null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterLifetimeScopeTServiceFunc_WithNullContainerArgument_ThrowsExpectedException()
        {
            // Act
            Action action = () =>
                SimpleInjectorWebExtensions.RegisterPerWebRequest<ICommand>(null, () => null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterLifetimeScopeTServiceFunc_WithNullFuncArgument_ThrowsExpectedException()
        {
            // Act
            Action action = () =>
                SimpleInjectorWebExtensions.RegisterPerWebRequest<ICommand>(new Container(), null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetInstance_PerWebRequestInstanceWithInitializer_CallsInitializerOncePerWebRequest()
        {
            // Arrange
            int callCount = 0;

            var container = new Container();

            container.RegisterPerWebRequest<ICommand, ConcreteCommand>();

            container.RegisterInitializer<ICommand>(command => { callCount++; });

            using (new HttpContextScope())
            {
                // Act
                container.GetInstance<ICommand>();
                container.GetInstance<ICommand>();
            }

            // Assert
            Assert.AreEqual(1, callCount, "The initializer for ICommand is expected to get fired once.");
        }

        [TestMethod]
        public void GetInstance_PerWebRequestFuncInstanceWithInitializer_CallsInitializerOncePerWebRequest()
        {
            // Arrange
            int callCount = 0;

            var container = new Container();

            container.RegisterPerWebRequest<ICommand>(() => new ConcreteCommand());

            container.RegisterInitializer<ICommand>(command => { callCount++; });

            using (new HttpContextScope())
            {
                // Act
                container.GetInstance<ICommand>();
                container.GetInstance<ICommand>();
            }

            // Assert
            Assert.AreEqual(1, callCount, "The initializer for ICommand is expected to get fired once.");
        }

        [TestMethod]
        public void GetInstance_OnDecoratedLifetimeScopedInstance_WrapsTheInstanceWithTheDecorator()
        {
            // Arrange
            var container = new Container();

            container.RegisterPerWebRequest<ICommand, ConcreteCommand>();

            container.RegisterDecorator(typeof(ICommand), typeof(CommandDecorator));

            using (new HttpContextScope())
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
        public void GetInstance_CalledTwiceInOneScopeForDecoratedWebRequestInstance_WrapsATransientDecoratorAroundAWebRequestInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterPerWebRequest<ICommand, ConcreteCommand>();

            container.RegisterDecorator(typeof(ICommand), typeof(CommandDecorator));

            using (new HttpContextScope())
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
        public void GetInstance_CalledTwiceInOneScopeForDecoratedWebRequestInstance2_WrapsATransientDecoratorAroundAWebRequestInstance()
        {
            // Arrange
            var container = new Container();

            // Same as previous test, but now with RegisterDecorator called first.
            container.RegisterDecorator(typeof(ICommand), typeof(CommandDecorator));

            container.RegisterPerWebRequest<ICommand, ConcreteCommand>();

            using (new HttpContextScope())
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
        public void WhenWebRequestEnds_BothAScopeEndsActionAndDisposableIntanceRegistered_InstancesGetDisposedLast()
        {
            // Arrange
            string expectedOrder = "[scope ending] [instance disposed]";

            string actualOrder = string.Empty;

            ScopedLifestyle lifestyle = new WebRequestLifestyle();

            var container = new Container();

            var scope = new HttpContextScope();

            try
            {
                lifestyle.RegisterForDisposal(container,
                    new DisposableAction(() => actualOrder += "[instance disposed]"));

                lifestyle.WhenScopeEnds(container, () => actualOrder += "[scope ending] ");
            }
            finally
            {
                // Act
                scope.Dispose();
            }

            // Assert
            Assert.AreEqual(expectedOrder, actualOrder,
                "Instances should get disposed after all 'scope ends' actions are executed, since those " +
                "delegates might still need to access those instances.");
        }

        public sealed class DisposableAction : IDisposable
        {
            private readonly Action calledOnDispose;

            public DisposableAction(Action calledOnDispose)
            {
                this.calledOnDispose = calledOnDispose;
            }

            public void Dispose()
            {
                this.calledOnDispose();
            }
        }
    }
}