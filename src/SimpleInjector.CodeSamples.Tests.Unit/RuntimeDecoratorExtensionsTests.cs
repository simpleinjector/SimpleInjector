namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Tests.Unit;

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
            AssertThat.IsInstanceOfType(typeof(NullCommandHandler<RealCommand>), handler1);
            AssertThat.IsInstanceOfType(typeof(CommandHandlerDecorator<RealCommand>), handler2);
        }

        [TestMethod]
        public void GetInstance_OnRegisterSingletonRuntimeDecoratorRegistration_CreatesTheDecoratorAsSingleton()
        {
            // Arrange
            var container = new Container();

            bool decorateHandler = false;

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>(Lifestyle.Singleton);

            container.RegisterRuntimeDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>),
                Lifestyle.Singleton, c => decorateHandler);

            // Act
            var handler1 = container.GetInstance<ICommandHandler<RealCommand>>();

            // Runtime switch
            decorateHandler = true;

            var handler2 = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(NullCommandHandler<RealCommand>), handler1);
            AssertThat.IsInstanceOfType(typeof(CommandHandlerDecorator<RealCommand>), handler2);
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerProxy<>), Lifestyle.Singleton);

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
            AssertThat.IsInstanceOfType(typeof(NullCommandHandler<RealCommand>), handler1);
            AssertThat.IsInstanceOfType(typeof(CommandHandlerDecorator<RealCommand>), handler2);
        }

        [TestMethod]
        public void GetInstance_OnRegisterRuntimeDecoratorRegistrationWithOverriddenLifestyleSelectionBehavior_CreatesTheDecoratorWithExpectedLifestyler()
        {
            // Arrange
            var container = new Container();

            container.Options.LifestyleSelectionBehavior = 
                new CustomLifestyleSelectionBehavior(Lifestyle.Singleton);

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            container.RegisterRuntimeDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>),
                c => true);

            // Act
            var handler1 = container.GetInstance<ICommandHandler<RealCommand>>();
            var handler2 = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreSame(handler1, handler2);
        }

        private sealed class CustomLifestyleSelectionBehavior : ILifestyleSelectionBehavior
        {
            private readonly Lifestyle lifestyle;

            public CustomLifestyleSelectionBehavior(Lifestyle lifestyle)
            {
                this.lifestyle = lifestyle;
            }

            public Lifestyle SelectLifestyle(Type implementationType)
            {
                return this.lifestyle;
            }
        }
    }
}