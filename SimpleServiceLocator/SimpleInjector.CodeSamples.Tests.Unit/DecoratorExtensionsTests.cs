namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Extensions;

    [TestClass]
    public class DecoratorExtensionsTests
    {
        public interface ICommandHandler<TCommand>
        {
            void Handle(TCommand command);
        }

        [TestMethod]
        public void GetInstance_OnDecoratedType_ReturnsTheDecorator()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingle<ILogger>(logger);

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(RealCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(Decorator1Handler<>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(Decorator1Handler<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_OnDecoratedType_GetsHandledAsExpected()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingle<ILogger>(logger);

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(RealCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(Decorator1Handler<>));

            // Act
            container.GetInstance<ICommandHandler<RealCommand>>().Handle(new RealCommand());

            // Assert
            Assert.AreEqual("Begin1 RealCommand End1", logger.Message);
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances_ReturnsLastRegisteredDecorator()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingle<ILogger>(logger);

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(RealCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(Decorator1Handler<>));
            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(Decorator2Handler<>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(Decorator2Handler<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances_GetsHandledAsExpected()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingle<ILogger>(logger);

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(RealCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(Decorator1Handler<>));
            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(Decorator2Handler<>));

            // Act
            container.GetInstance<ICommandHandler<RealCommand>>().Handle(new RealCommand());

            // Assert
            Assert.AreEqual("Begin2 Begin1 RealCommand End1 End2", logger.Message);
        }

        [TestMethod]
        public void GetInstance_DecoratorWithMissingDependency_ThrowAnExceptionWithADescriptiveMessage()
        {
            // Arrange
            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            // Decorator1Handler depends on ILogger, but ILogger is not registered.
            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(Decorator1Handler<>));

            try
            {
                // Act
                var handler = container.GetInstance<ICommandHandler<RealCommand>>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("ILogger"), "Actual message: " + ex.Message);
            }
        }

        private sealed class FakeLogger : ILogger
        {
            public string Message { get; private set; }

            public void Log(string message)
            {
                this.Message += message;
            }
        }

        private class RealCommand 
        {
        }

        private class StubCommandHandler : ICommandHandler<RealCommand>
        {
            public void Handle(RealCommand command)
            {
            }
        }

        private class RealCommandHandler : ICommandHandler<RealCommand>
        {
            private readonly ILogger logger;

            public RealCommandHandler(ILogger logger)
            {
                this.logger = logger;
            }

            public void Handle(RealCommand command)
            {
                this.logger.Log("RealCommand");
            }
        }

        private class Decorator1Handler<T> : ICommandHandler<T>
        {
            private readonly ICommandHandler<T> wrapped;
            private readonly ILogger logger;

            public Decorator1Handler(ICommandHandler<T> wrapped, ILogger logger)
            {
                this.wrapped = wrapped;
                this.logger = logger;
            }

            public void Handle(T command)
            {
                this.logger.Log("Begin1 ");
                this.wrapped.Handle(command);
                this.logger.Log(" End1");
            }
        }

        private class Decorator2Handler<T> : ICommandHandler<T>
        {
            private readonly ICommandHandler<T> wrapped;
            private readonly ILogger logger;

            public Decorator2Handler(ICommandHandler<T> wrapped, ILogger logger)
            {
                this.wrapped = wrapped;
                this.logger = logger;
            }

            public void Handle(T command)
            {
                this.logger.Log("Begin2 ");
                this.wrapped.Handle(command);
                this.logger.Log(" End2");
            }
        }
    }
}