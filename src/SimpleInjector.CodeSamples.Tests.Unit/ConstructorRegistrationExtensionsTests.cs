namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class ConstructorRegistrationExtensionsTests
    {
        private interface ISomeDependency
        {
        }

        [TestMethod]
        public void Register_RegisterConstructorSelectorConvention_CanNotRegisterTypeWithMultipleConstructors()
        {
            // Arrange
            var container = new Container();

            var convention = container.RegisterConstructorSelectorConvention();
            
            // Act
            Action action = () => container.Register<ICommand, MultipleConstructorsCommand>();

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterWithConstructor_WithValidArgument_Succeeds()
        {
            // Arrange
            var container = new Container();

            var convention = container.RegisterConstructorSelectorConvention();
            
            // Act
            convention.Register<ICommand, MultipleConstructorsCommand>(ConstructorSelector.MostParameters);
        }

        [TestMethod]
        public void GetInstance_TypeRegisteredWithSpecificConstructor_CreatesThatTypeUsingThatConstructor1()
        {
            // Arrange
            var container = new Container();

            var convention = container.RegisterConstructorSelectorConvention();

            convention.Register<ICommand, MultipleConstructorsCommand>(ConstructorSelector.LeastParameters);

            // Act
            var command = (MultipleConstructorsCommand)container.GetInstance<ICommand>();

            // Assert
            Assert.AreEqual(MultipleConstructorsCommand.LeastParametersConstructor, command.ConstructorType);
        }

        [TestMethod]
        public void GetInstance_TypeRegisteredWithSpecificConstructor_CreatesThatTypeUsingThatConstructor2()
        {
            // Arrange
            var container = new Container();

            var convention = container.RegisterConstructorSelectorConvention();

            container.Register<ILogger, NullLogger>(Lifestyle.Singleton);
            container.Register<ISomeDependency, SomeDependency>(Lifestyle.Singleton);

            convention.Register<ICommand, MultipleConstructorsCommand>(ConstructorSelector.MostParameters);

            // Act
            var command = (MultipleConstructorsCommand)container.GetInstance<ICommand>();

            // Assert
            Assert.AreEqual(MultipleConstructorsCommand.MostParametersConstructor, command.ConstructorType);
        }

        private sealed class MultipleConstructorsCommand : ICommand
        {
            public const string LeastParametersConstructor = "LeastParameters";
            public const string MostParametersConstructor = "MostParameters";

            public readonly string ConstructorType;

            public MultipleConstructorsCommand()
            {
                this.ConstructorType = LeastParametersConstructor;
            }

            public MultipleConstructorsCommand(ILogger logger)
            {
            }

            public MultipleConstructorsCommand(ISomeDependency command)
            {
            }

            public MultipleConstructorsCommand(ILogger logger, ISomeDependency command)
            {
                this.ConstructorType = MostParametersConstructor;
            }

            public void Execute()
            {
            }
        }

        private sealed class SomeDependency : ISomeDependency
        {
        }
    }
}