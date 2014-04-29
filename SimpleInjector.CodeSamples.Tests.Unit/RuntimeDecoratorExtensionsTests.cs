namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Extensions;

    [TestClass]
    public class RuntimeDecoratorExtensionsTests
    {
        [TestMethod]
        public void GetInstance_OnRegisterRuntimeDecoratorRegistration_DecorationCanBeChangedDynamically()
        {
            // Arrange
            var container = new Container();

            bool decorateHandler = false;

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            container.RegisterRuntimeDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>),
                c => decorateHandler);

            // Act
            var handler1 = container.GetInstance<ICommandHandler<RealCommand>>();

            // Runtime switch
            decorateHandler = true;

            var handler2 = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler1, typeof(NullCommandHandler<RealCommand>));
            Assert.IsInstanceOfType(handler2, typeof(CommandHandlerDecorator<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_OnRegisterRuntimeDecoratorRegistrationAndSingletonProxy_DecorationCanBeChangedDynamically()
        {
            // Arrange
            var container = new Container();

            bool decorateHandler = false;

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            container.RegisterRuntimeDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>),
                c => decorateHandler);

            container.RegisterSingleDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerProxy<>));

            // Act
            var handler1 = 
                ((CommandHandlerProxy<RealCommand>)container.GetInstance<ICommandHandler<RealCommand>>())
                .DecorateeFactory();

            // Runtime switch
            decorateHandler = true;

            var handler2 = 
                ((CommandHandlerProxy<RealCommand>)container.GetInstance<ICommandHandler<RealCommand>>())
                .DecorateeFactory();

            // Assert
            Assert.IsInstanceOfType(handler1, typeof(NullCommandHandler<RealCommand>));
            Assert.IsInstanceOfType(handler2, typeof(CommandHandlerDecorator<RealCommand>));
        }
    }
}