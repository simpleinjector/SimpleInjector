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
        [TestMethod]
        public void RegisterWithConstructor_WithValidArgument_Succeeds()
        {
            // Arrange
            var container = new Container();
            
            // Act
            container.RegisterWithConstructor<ICommand>(() => new MultipleConstructorsCommand((ILogger)null));
        }

        [TestMethod]
        public void GetInstance_CalledAfterValidRegistration_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ILogger, NullLogger>();
            container.RegisterWithConstructor<ICommand>(() => new MultipleConstructorsCommand((ILogger)null));

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

            public MultipleConstructorsCommand(ICommand command)
            {
            }

            public MultipleConstructorsCommand(ILogger logger, ICommand command)
            {
            }

            public void Execute()
            {
            }
        }
    }
}
