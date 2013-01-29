namespace SimpleInjector.Tests.Unit
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class RegisterInitializerTests
    {
        private interface ICommand
        {
            bool SendAsynchronously { get; set; }

            void Execute();
        }

        [Test]
        public void RegisterInitializer_WithValidInitializer_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterInitializer<IUserRepository>(repistoryToInitialize => { });
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterInitializer_WithNullArgument_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            Action<IUserRepository> invalidInstanceInitializer = null;

            // Act
            container.RegisterInitializer<IUserRepository>(invalidInstanceInitializer);
        }

        [Test]
        public void GetInstance_RequestingInstanceRegisteredWithGenericRegister_CallsTheCorrespondingInitializer()
        {
            // Arrange
            bool initializerWasCalled = false;

            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>();

            container.RegisterInitializer<IUserRepository>(repository => { initializerWasCalled = true; });

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsTrue(initializerWasCalled, "The initializer was never called.");
        }

        [Test]
        public void RegisterInitializer_RegisteredTwiceForTheSameServiceType_CallsBothInitializers()
        {
            // Arrange
            bool firstInitializerWasCalled = false;
            bool secondInitializerWasCalled = false;

            var container = new Container();

            container.RegisterInitializer<IUserRepository>(repository => { firstInitializerWasCalled = true; });
            container.RegisterInitializer<IUserRepository>(repository => { secondInitializerWasCalled = true; });

            // Act
            container.GetInstance<SqlUserRepository>();

            // Assert
            Assert.IsTrue(firstInitializerWasCalled, "The first initializer was never called.");
            Assert.IsTrue(secondInitializerWasCalled, "The second initializer was never called.");
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterInitializer_CalledOnALockedContainer_ThrowsExceptedException()
        {
            // Arrange
            var container = new Container();

            // This call locks the container
            container.GetInstance<PluginImpl>();

            // Act
            container.RegisterInitializer<IUserRepository>(repositoryToInitialize => { });
        }

        [Test]
        public void GetInstance_OnUnregisteredTypeThatInheritsFromRegisteredServiceType_InitializesCorrectly()
        {
            // Arrange
            var container = new Container();

            container.RegisterInitializer<ICommand>(commandToInitialize =>
            {
                commandToInitialize.SendAsynchronously = true;
            });

            // Act
            var command = container.GetInstance<ConcreteCommand>();

            // Assert
            Assert.IsTrue(command.SendAsynchronously, "The instance was not initialized correctly.");
        }

        [Test]
        public void GetInstance_OnUnregisteredTypeWithDependency_InitializesDependencyCorrectly()
        {
            // Arrange
            var container = new Container();

            container.RegisterInitializer<ICommand>(commandToInitialize =>
            {
                commandToInitialize.SendAsynchronously = true;
            });

            container.Register<ICommand, ConcreteCommand>();

            // Act
            // CommandClient is a concrete class with a ICommand dependency
            var client = container.GetInstance<CommandClient>();

            var command = (ConcreteCommand)client.Command;

            // Assert
            Assert.IsTrue(command.SendAsynchronously, "The instance was not initialized correctly.");
        }

        [Test]
        public void GetInstance_OnUnregisteredTypeThatImplementsMultipleRegisteredServiceTypes_InitializesCorrectly()
        {
            // Arrange
            var container = new Container();

            container.RegisterInitializer<ICommand>(commandToInitialize =>
            {
                commandToInitialize.SendAsynchronously = true;
            });

            container.RegisterInitializer<CommandBase>(commandToInitialize =>
            {
                commandToInitialize.Clock = container.GetInstance<ITimeProvider>();
            });

            container.RegisterSingle<ITimeProvider, RealTimeProvider>();

            // Act
            // ConcreteCommand inherits from CommandBase and implements ICommand.
            var command = container.GetInstance<ConcreteCommand>();

            // Assert
            Assert.IsTrue(command.SendAsynchronously, "Instance initializer for ICommand was not executed.");
            Assert.IsNotNull(command.Clock, "Instance initializer for CommandBase was not executed.");
        }

        [Test]
        public void GetInstance_RegisteredSingletonThatInheritsFromRegisteredServiceType_InitializesCorrectly()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ConcreteCommand>();

            container.RegisterInitializer<ICommand>(commandToInitialize =>
            {
                commandToInitialize.SendAsynchronously = true;
            });

            // Act
            var command = container.GetInstance<ConcreteCommand>();

            // Assert
            Assert.IsTrue(command.SendAsynchronously, "The instance was not initialized correctly.");
        }

        [Test]
        public void GetInstance_OnUnregisteredTypeThatInheritsFromRegisteredServiceType_CallsInitializerForEachInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterInitializer<ICommand>(commandToInitialize =>
            {
                commandToInitialize.SendAsynchronously = true;
            });

            // Act
            container.GetInstance<ConcreteCommand>();
            var command = container.GetInstance<ConcreteCommand>();

            // Assert
            Assert.IsTrue(command.SendAsynchronously, "The instance was not initialized correctly.");
        }

        [Test]
        public void GetInstance_RegisteredSingletonThatInheritsFromRegisteredServiceType_CallInitializerOnlyOnce()
        {
            // Arrange
            int numberOfCalls = 0;

            var container = new Container();

            container.RegisterSingle<ConcreteCommand>();

            container.RegisterInitializer<ICommand>(commandToInitialize =>
            {
                numberOfCalls++;
            });

            // Act
            container.GetInstance<ConcreteCommand>();
            container.GetInstance<ConcreteCommand>();
            container.GetInstance<ConcreteCommand>();

            // Assert
            Assert.AreEqual(1, numberOfCalls, 
                "The initializer was expected to be called just once for singletons.");
        }

        [Test]
        public void GetInstance_OnInstanceNotCreatedByTheContainer1_DoesNotCallInitializer()
        {
            // Arrange
            bool delegateCalled = false;
            var container = new Container();

            container.RegisterInitializer<ICommand>(c => { delegateCalled = true; });

            container.RegisterSingle<ICommand>(new ConcreteCommand());

            // Act
            container.GetInstance<ICommand>();

            // Assert
            Assert.IsFalse(delegateCalled, "The instance initializer should not be called on types that " +
                "are created outside of the control of the container.");
        }

        [Test]
        public void GetInstance_OnInstanceNotCreatedByTheContainer2_DoesNotCallInitializer()
        {
            // Arrange
            bool delegateCalled = false;
            var container = new Container();

            container.RegisterInitializer<ICommand>(c => { delegateCalled = true; });

            var singleton = new ConcreteCommand();
            container.Register<ICommand>(() => singleton);

            // Act
            container.GetInstance<ICommand>();

            // Assert
            Assert.IsFalse(delegateCalled, "The instance initializer should not be called on types that " +
                "are created outside of the control of the container.");
        }

        [Test]
        public void GetInstance_OnTypeCreatedByTheContainer_InitializesInstance()
        {
            // Arrange
            bool delegateCalled = false;
            var container = new Container();

            container.RegisterInitializer<CommandBase>(c => { delegateCalled = true; });

            // ConcreteCommand inherits from CommandBase.
            container.Register<ICommand>(() => container.GetInstance<ConcreteCommand>());

            // Act
            container.GetInstance<ICommand>();

            // Assert
            Assert.IsTrue(delegateCalled, "The instance initializer should be called on types that " +
                "are created within the control of the container.");
        }

        [Test]
        public void GetInstance_MultipleRegisteredInitializers_RunsInitializersInOrderOfRegistration()
        {
            // Arrange
            int index = 1;
            int initializer1Index = 0;
            int initializer2Index = 0;
            int initializer3Index = 0;

            var container = new Container();

            container.RegisterInitializer<ICommand>(c => { initializer1Index = index++; });
            container.RegisterInitializer<ConcreteCommand>(c => { initializer3Index = index++; });
            container.RegisterInitializer<CommandBase>(c => { initializer2Index = index++; });
            
            // Act
            container.GetInstance<ConcreteCommand>();

            // Assert
            Assert.AreEqual(1, initializer1Index, "ICommand initializer did not run in expected order.");
            Assert.AreEqual(2, initializer3Index, "ConcreteCommand initializer did not run in expected order.");
            Assert.AreEqual(3, initializer2Index, "CommandBase initializer did not run in expected order.");
        }

        private abstract class CommandBase : ICommand
        {
            public bool SendAsynchronously { get; set; }

            public ITimeProvider Clock { get; set; }

            public abstract void Execute();
        }

        private sealed class ConcreteCommand : CommandBase
        {
            public override void Execute()
            {
            }
        }

        private sealed class CommandClient
        {
            public CommandClient(ICommand command)
            {
                this.Command = command;
            }

            public ICommand Command { get; private set; }
        }
    }
}