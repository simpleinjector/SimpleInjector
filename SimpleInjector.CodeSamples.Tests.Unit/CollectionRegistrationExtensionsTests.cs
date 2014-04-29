namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CollectionRegistrationExtensionsTests
    {
        [TestMethod]
        public void GetInstance_OnTypeWithArrayArgumentAfterCallingRegisterArrays_CreatesTypeAsEspected()
        {
            // Arrange
            var container = new Container();

            container.AllowToResolveArraysAndLists();

            // Act
            var deposite = container.GetInstance<CompositeCommand>();
        }

        [TestMethod]
        public void GetInstance_OnTypeWithArrayArgumentAfterCallingRegisterArrays_InjectsExpectedNumberOfArguments()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll<ICommand>(new ConcreteCommand());

            container.AllowToResolveArraysAndLists();

            // Act
            var composite = container.GetInstance<CompositeCommand>();

            // Assert
            Assert.IsNotNull(composite.Commands);
            Assert.AreEqual(1, composite.Commands.Length);
        }

        [TestMethod]
        public void GetInstance_OnTypeWithArrayArgumentAfterCallingRegisterArrays_InjectsExpectedElement()
        {
            // Arrange
            var expectedCommand = new ConcreteCommand();

            var container = new Container();

            container.RegisterAll<ICommand>(expectedCommand);

            container.AllowToResolveArraysAndLists();

            // Act
            var composite = container.GetInstance<CompositeCommand>();

            // Assert
            Assert.AreEqual(expectedCommand, composite.Commands[0]);
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnSameType_InjectsANewArrayOnEachRequest()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll<ICommand>(new ConcreteCommand());

            container.AllowToResolveArraysAndLists();

            // Act
            var composite = container.GetInstance<CompositeCommand>();

            composite.Commands[0] = null;

            composite = container.GetInstance<CompositeCommand>();

            // Assert
            Assert.IsNotNull(composite.Commands[0],
                "The element in the array is expected NOT to be null. When it is null, it means that the " +
                "array has been cached.");
        }

        [TestMethod]
        public void GetInstance_Always_InjectsAFreshArrayInstance()
        {
            // Arrange
            var container = new Container();

            List<ICommand> commands = new List<ICommand>();

            // Add a first command
            commands.Add(new ConcreteCommand());

            container.RegisterAll<ICommand>(commands);

            container.AllowToResolveArraysAndLists();

            container.GetInstance<CompositeCommand>();

            // Add yet another command
            commands.Add(new ConcreteCommand());

            // Act
            var composite = container.GetInstance<CompositeCommand>();

            // Assert
            Assert.AreEqual(2, composite.Commands.Length, "The IEnumerable<ICommand> collection should be " +
                "cached by its reference, and not by its current content, because that content is allowed " +
                "to change.");
        }

        [TestMethod]
        public void GetInstance_OnConcreteTypeWithIListArgumentAfterAllowToResolveArrays_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.AllowToResolveArraysAndLists();

            // Act
            var collection = container.GetInstance<CommandCollection>();

            // Assert
            Assert.IsNotNull(collection.Commands);
            Assert.AreEqual(0, collection.Commands.Count);
        }

        private sealed class CompositeCommand
        {
            public CompositeCommand(ICommand[] commands)
            {
                this.Commands = commands;
            }

            public ICommand[] Commands { get; private set; }
        }

        private sealed class CommandCollection
        {
            public CommandCollection(IList<ICommand> commands)
            {
                this.Commands = commands;
            }

            public IList<ICommand> Commands { get; private set; }
        }
    }
}