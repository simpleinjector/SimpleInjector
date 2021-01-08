namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class TopLevelClassWithFailingTypeInitializer
    {
        static TopLevelClassWithFailingTypeInitializer()
        {
            throw new Exception("<Inner exception>.");
        }
    }

    // #812
    [TestClass]
    public class TypeInitializationErrorTests
    {
        [TestMethod]
        public void GetInstance_ResolvingATypeWithTypeInitializationException_ThrowsExpressiveException()
        {
            // Act
            var container = new Container();

            container.Register<TopLevelClassWithFailingTypeInitializer>();

            // Act
            Action action = () => container.GetInstance<TopLevelClassWithFailingTypeInitializer>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "The type initializer for TopLevelClassWithFailingTypeInitializer threw an " +
                "exception. <Inner exception>.",
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingANestedTypeWithTypeInitializationException_ThrowsExpressiveException()
        {
            // Act
            var container = new Container();

            container.Register<NestedClassWithFailingTypeInitializer>();

            // Act
            Action action = () => container.GetInstance<NestedClassWithFailingTypeInitializer>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "The type initializer for " +
                $"{nameof(TypeInitializationErrorTests)}.NestedClassWithFailingTypeInitializer " +
                "threw an exception. <Inner exception>.",
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingAGenericNestedTypeWithTypeInitializationException_ThrowsExpressiveException()
        {
            // Act
            var container = new Container();

            container.Register(typeof(NestedClassWithFailingTypeInitializer<>));

            // Act
            Action action = () => container.GetInstance<NestedClassWithFailingTypeInitializer<object>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "The type initializer for " +
                $"{nameof(TypeInitializationErrorTests)}.NestedClassWithFailingTypeInitializer<object> " +
                "threw an exception. <Inner exception>.",
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingASingletonWithTypeInitializationException_ThrowsExpressiveException()
        {
            // Act
            var container = new Container();

            container.RegisterSingleton<NestedClassWithFailingTypeInitializer>();

            // Act
            Action action = () => container.GetInstance<NestedClassWithFailingTypeInitializer>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "The type initializer for " +
                $"{nameof(TypeInitializationErrorTests)}.NestedClassWithFailingTypeInitializer " +
                "threw an exception. <Inner exception>.",
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingADecoratedInstanceWithInitializationException_ThrowsExpressiveException()
        {
            // Act
            var container = new Container();

            container.Register<INonGenericService, NestedClassWithFailingTypeInitializer>();
            container.RegisterDecorator<INonGenericService, NonGenericServiceDecorator>();

            // Act
            Action action = () => container.GetInstance<INonGenericService>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "The type initializer for " +
                $"{nameof(TypeInitializationErrorTests)}.NestedClassWithFailingTypeInitializer " +
                "threw an exception. <Inner exception>.",
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingADecoratorWithTypeInitializationException_ThrowsExpressiveException()
        {
            // Act
            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(DecoratorWithFailingTypeInitializer<>));

            // Act
            Action action = () => container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "The type initializer for " +
                $"{nameof(TypeInitializationErrorTests)}.DecoratorWithFailingTypeInitializer<RealCommand> " +
                "threw an exception. <Inner exception>.",
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingANestedDecoratorWithTypeInitializationException_ThrowsExpressiveException()
        {
            // Act
            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(DecoratorWithFailingTypeInitializer<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>));

            // Act
            Action action = () => container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "The type initializer for " +
                $"{nameof(TypeInitializationErrorTests)}.DecoratorWithFailingTypeInitializer<RealCommand> " +
                "threw an exception. <Inner exception>.",
                action);
        }

        public class NestedClassWithFailingTypeInitializer : INonGenericService
        {
            static NestedClassWithFailingTypeInitializer()
            {
                throw new Exception("<Inner exception>.");
            }

            public void DoSomething()
            {
            }
        }

        public class NestedClassWithFailingTypeInitializer<T>
        {
            static NestedClassWithFailingTypeInitializer()
            {
                throw new Exception("<Inner exception>.");
            }
        }

        public class DecoratorWithFailingTypeInitializer<T> : ICommandHandler<T>
        {
            static DecoratorWithFailingTypeInitializer()
            {
                throw new Exception("<Inner exception>.");
            }

            public DecoratorWithFailingTypeInitializer(ICommandHandler<T> decoratee)
            {
            }
        }
    }
}