namespace SimpleInjector.Integration.Web.Tests.Unit
{
    using System;
    using System.Threading;
    using System.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SimpleInjectorWebExtensionsTests
    {
        public interface ICommand
        {
            void Execute();
        }

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
                Assert.IsInstanceOfType(actualInstance, typeof(ConcreteCommand));
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
                    ex.Message.Contains("the instance is requested outside the context of a HttpContext"),
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
        public void RegisterPerWebRequestDispose_TransientInstanceWithRegisterForDisposal_DisposesThatInstance()
        {
            // Arrange
            var container = new Container();

            // Transient
            container.Register<ICommand, DisposableCommand>();

            container.RegisterInitializer<DisposableCommand>(SimpleInjectorWebExtensions.RegisterForDisposal);

            DisposableCommand command;

            // Act
            using (new HttpContextScope())
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
            using (new HttpContextScope())
            {
                try
                {
                    SimpleInjectorWebExtensions.RegisterForDisposal(null);

                    Assert.Fail("Exception expected.");
                }
                catch (ArgumentNullException)
                {
                    // This exception is expected.
                }
            }
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

            container.RegisterSingle<DisposableCommand>();

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
                Assert.IsTrue(ex.Message.Contains("The Container can't be changed"),
                    "Actual: " + ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterLifetimeScopeTConcrete_WithNullArgument_ThrowsExpectedException()
        {
            SimpleInjectorWebExtensions.RegisterPerWebRequest<ConcreteCommand>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterLifetimeScopeTServiceTImplementation_WithNullArgument_ThrowsExpectedException()
        {
            SimpleInjectorWebExtensions.RegisterPerWebRequest<ICommand, ConcreteCommand>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterLifetimeScopeTServiceFunc_WithNullContainerArgument_ThrowsExpectedException()
        {
            SimpleInjectorWebExtensions.RegisterPerWebRequest<ICommand>(null, () => null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterLifetimeScopeTServiceFunc_WithNullFuncArgument_ThrowsExpectedException()
        {
            SimpleInjectorWebExtensions.RegisterPerWebRequest<ICommand>(new Container(), null);
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