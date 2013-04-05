namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RuntimeDecoratorExtensionsTests
    {
        public interface ICommandHandler<TCommand> 
        {
        }

        [TestMethod]
        public void GetInstance_OnRegisterRuntimeDecoratorRegistration_DecorationCanBeChangedDynamically()
        {
            // Arrange
            var container = new Container();

            bool decorateHandler = false;

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            container.RegisterRuntimeDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>),
                context => decorateHandler);

            // Act
            var handler1 = container.GetInstance<ICommandHandler<RealCommand>>();

            // Runtime switch
            decorateHandler = true;

            var handler2 = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler1, typeof(NullCommandHandler<RealCommand>));
            Assert.IsInstanceOfType(handler2, typeof(CommandHandlerDecorator<RealCommand>));
        }
        
        public class RealCommand 
        {
        }

        public class NullCommandHandler<TCommand> : ICommandHandler<TCommand> 
        {
        }

        public class CommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
        {
            public CommandHandlerDecorator(ICommandHandler<TCommand> decoratee)
            {
            }
        }
    }
}