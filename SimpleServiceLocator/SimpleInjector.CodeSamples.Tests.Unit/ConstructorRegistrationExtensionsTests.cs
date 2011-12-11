namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConstructorRegistrationExtensionsTests
    {
        private interface ISomeDependency
        {
        }

        [TestMethod]
        public void RegisterWithConstructor_WithValidArgument_Succeeds()
        {
            // Arrange
            var container = new Container();
            
            // Act
            container.Register<ICommand, MultipleConstructorsCommand>(ConstructorSelector.MostParameters);
        }

        [TestMethod]
        public void GetInstance_CalledAfterValidRegistration_Succeeds1()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, MultipleConstructorsCommand>(ConstructorSelector.LeastParameters);

            // Act
            var command = container.GetInstance<ICommand>();
        }

        [TestMethod]
        public void GetInstance_CalledAfterValidRegistration_Succeeds2()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ILogger, NullLogger>();
            container.RegisterSingle<ISomeDependency, SomeDependency>();
            container.Register<ICommand, MultipleConstructorsCommand>(ConstructorSelector.MostParameters);

            // Act
            var command = container.GetInstance<ICommand>();
        }

        private sealed class MultipleConstructorsCommand : ICommand
        {
            public MultipleConstructorsCommand()
            {
            }

            public MultipleConstructorsCommand(ILogger logger)
            {
            }

            public MultipleConstructorsCommand(ISomeDependency command)
            {
            }

            public MultipleConstructorsCommand(ILogger logger, ISomeDependency command)
            {
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