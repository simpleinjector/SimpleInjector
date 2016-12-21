namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterInitializerTests
    {
        public interface ICommand
        {
            bool SendAsynchronously { get; set; }

            void Execute();
        }

        [TestMethod]
        public void RegisterInitializer_WithValidInitializer_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterInitializer<IUserRepository>(repistoryToInitialize => { });
        }

        [TestMethod]
        public void RegisterInitializer_WithNullArgument_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Action<IUserRepository> invalidInstanceInitializer = null;

            // Act
            Action action = () => container.RegisterInitializer<IUserRepository>(invalidInstanceInitializer);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetInstance_RequestingInstanceRegisteredWithGenericRegister_CallsTheCorrespondingInitializer()
        {
            // Arrange
            bool initializerWasCalled = false;

            var container = ContainerFactory.New();

            container.Register<IUserRepository, InMemoryUserRepository>();

            container.RegisterInitializer<IUserRepository>(repository => { initializerWasCalled = true; });

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsTrue(initializerWasCalled, "The initializer was never called.");
        }

        [TestMethod]
        public void RegisterInitializer_RegisteredTwiceForTheSameServiceType_CallsBothInitializers()
        {
            // Arrange
            bool firstInitializerWasCalled = false;
            bool secondInitializerWasCalled = false;

            var container = ContainerFactory.New();

            container.RegisterInitializer<IUserRepository>(repository => { firstInitializerWasCalled = true; });
            container.RegisterInitializer<IUserRepository>(repository => { secondInitializerWasCalled = true; });

            // Act
            container.GetInstance<SqlUserRepository>();

            // Assert
            Assert.IsTrue(firstInitializerWasCalled, "The first initializer was never called.");
            Assert.IsTrue(secondInitializerWasCalled, "The second initializer was never called.");
        }

        [TestMethod]
        public void RegisterInitializer_CalledOnALockedContainer_ThrowsExceptedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // This call locks the container
            container.GetInstance<PluginImpl>();

            // Act
            Action action = () => container.RegisterInitializer<IUserRepository>(repositoryToInitialize => { });
            
            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void GetInstance_OnUnregisteredTypeThatInheritsFromRegisteredServiceType_InitializesCorrectly()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterInitializer<ICommand>(commandToInitialize =>
            {
                commandToInitialize.SendAsynchronously = true;
            });

            // Act
            var command = container.GetInstance<ConcreteCommand>();

            // Assert
            Assert.IsTrue(command.SendAsynchronously, "The instance was not initialized correctly.");
        }

        [TestMethod]
        public void GetInstance_OnUnregisteredTypeWithDependency_InitializesDependencyCorrectly()
        {
            // Arrange
            var container = ContainerFactory.New();

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

        [TestMethod]
        public void GetInstance_OnUnregisteredTypeThatImplementsMultipleRegisteredServiceTypes_InitializesCorrectly()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterInitializer<ICommand>(commandToInitialize =>
            {
                commandToInitialize.SendAsynchronously = true;
            });

            container.RegisterInitializer<CommandBase>(commandToInitialize =>
            {
                commandToInitialize.Clock = container.GetInstance<ITimeProvider>();
            });

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            // Act
            // ConcreteCommand inherits from CommandBase and implements ICommand.
            var command = container.GetInstance<ConcreteCommand>();

            // Assert
            Assert.IsTrue(command.SendAsynchronously, "Instance initializer for ICommand was not executed.");
            Assert.IsNotNull(command.Clock, "Instance initializer for CommandBase was not executed.");
        }

        [TestMethod]
        public void GetInstance_RegisteredSingletonThatInheritsFromRegisteredServiceType_InitializesCorrectly()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ConcreteCommand>(Lifestyle.Singleton);

            container.RegisterInitializer<ICommand>(commandToInitialize =>
            {
                commandToInitialize.SendAsynchronously = true;
            });

            // Act
            var command = container.GetInstance<ConcreteCommand>();

            // Assert
            Assert.IsTrue(command.SendAsynchronously, "The instance was not initialized correctly.");
        }

        [TestMethod]
        public void GetInstance_OnUnregisteredTypeThatInheritsFromRegisteredServiceType_CallsInitializerForEachInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

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

        [TestMethod]
        public void GetInstance_RegisteredSingletonThatInheritsFromRegisteredServiceType_CallInitializerOnlyOnce()
        {
            // Arrange
            int numberOfCalls = 0;

            var container = ContainerFactory.New();

            container.Register<ConcreteCommand>(Lifestyle.Singleton);

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

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnOnInstanceRegisteredAsSingleInstance_CallsTheInitializerJustOnce()
        {
            // Arrange
            int actualNumberOfCalls = 0;

            var container = ContainerFactory.New();

            container.RegisterInitializer<ICommand>(c => { actualNumberOfCalls++; });

            container.RegisterSingleton<ICommand>(new ConcreteCommand());

            // Act
            // Request the instance 4 times
            container.GetInstance<ICommand>();
            container.GetInstance<ICommand>();
            container.GetInstance<ICommand>();
            container.GetInstance<ICommand>();

            // The registered delegate for type RegisterInitializerTests+ICommand threw an exception. 
            // Attempt by method 'Expression.CreateLambda(Type, Expression, String, Boolean, ReadOnlyCollection`1)' 
            // to access method 'Expression`1>.Create(Expression, String, Boolean, ReadOnlyCollection`1)' failed.

            // Assert
            Assert.AreEqual(1, actualNumberOfCalls,
                "The container will even call the initializer on instances that are passed in from the " +
                "outside, but in the case of RegisterSingleton, the initializer should only be called once.");
        }

        [TestMethod]
        public void GetInstance_RequestingATransientTypeThatIsRegisteredUsingADelegate_NeverCallsTheInitializerForTheImplementation()
        {
            // Arrange
            bool initializerWasCalled = false;

            var container = ContainerFactory.New();

            container.RegisterInitializer<ConcreteCommand>(c => { initializerWasCalled = true; });

            container.Register<ICommand>(() => new ConcreteCommand());

            // Act
            container.GetInstance<ICommand>();

            // Assert
            Assert.IsFalse(initializerWasCalled,
                "Since only ICommand is 'staticly' available information, the initializer for " + 
                "ConcreteCommand should never fire. The performance hit would be too big.");
        }

        [TestMethod]
        public void GetInstance_RequestingASingletonTypeThatIsRegisteredUsingADelegate_NeverCallsTheInitializerForTheImplementation()
        {
            // Arrange
            bool initializerWasCalled = false;

            var container = ContainerFactory.New();

            container.RegisterInitializer<ConcreteCommand>(c => { initializerWasCalled = true; });

            container.Register<ICommand>(() => new ConcreteCommand(), Lifestyle.Singleton);

            // Act
            container.GetInstance<ICommand>();

            // Assert
            Assert.IsFalse(initializerWasCalled,
                "Since only ICommand is 'statically' available information, the initializer for " +
                "ConcreteCommand should never fire. Although the performance hit would be a one-time hit, " +
                "users would expect this behavior to be the same as when registering a transient.");
        }
        
        [TestMethod]
        public void GetInstance_RequestingASingletonTypeThatIsRegisteredUsingAnInstance_NeverCallsTheInitializerForTheImplementation()
        {
            // Arrange
            bool initializerWasCalled = false;

            var container = ContainerFactory.New();

            container.RegisterInitializer<ConcreteCommand>(c => { initializerWasCalled = true; });

            container.RegisterSingleton<ICommand>(new ConcreteCommand());

            // Act
            container.GetInstance<ICommand>();

            // Assert
            Assert.IsFalse(initializerWasCalled,
                "Since only ICommand is 'statically' available information, the initializer for " +
                "ConcreteCommand should never fire. Although the performance hit would be a one-time hit, " +
                "users would expect this behavior to be the same as when registering a transient.");
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnOnInstanceRegisteredAsSingleFunc_CallsTheInitializerJustOnce()
        {
            // Arrange
            int expectedTimesDelegateGetsCalled = 4;
            int actualTimesDelegateGotCalled = 0;

            var container = ContainerFactory.New();

            container.RegisterInitializer<ICommand>(c => { actualTimesDelegateGotCalled++; });

            var singleton = new ConcreteCommand();

            container.Register<ICommand>(() => singleton);

            // Act
            container.GetInstance<ICommand>();
            container.GetInstance<ICommand>();
            container.GetInstance<ICommand>();
            container.GetInstance<ICommand>();

            // Assert
            Assert.AreEqual(expectedTimesDelegateGetsCalled, actualTimesDelegateGotCalled,
                "The container will even call the initializer on instances that are passed in from the " +
                "outside, and since the delegate is registered as transient, the initializer should be " +
                "called each time.");
        }

        [TestMethod]
        public void GetInstance_OnTypeCreatedByTheContainer_InitializesInstance()
        {
            // Arrange
            bool delegateCalled = false;
            var container = ContainerFactory.New();

            container.RegisterInitializer<CommandBase>(c => { delegateCalled = true; });

            // ConcreteCommand inherits from CommandBase.
            container.Register<ICommand>(() => container.GetInstance<ConcreteCommand>());

            // Act
            container.GetInstance<ICommand>();

            // Assert
            Assert.IsTrue(delegateCalled, "The instance initializer should be called on types that " +
                "are created within the control of the container.");
        }

        [TestMethod]
        public void GetInstance_MultipleRegisteredInitializers_RunsInitializersInOrderOfRegistration()
        {
            // Arrange
            int index = 1;
            int initializer1Index = 0;
            int initializer2Index = 0;
            int initializer3Index = 0;

            var container = ContainerFactory.New();

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

            public ICommand Command { get; }
        }
    }
}