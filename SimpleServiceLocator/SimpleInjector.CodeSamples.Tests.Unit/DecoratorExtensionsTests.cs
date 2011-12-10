namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Linq.Expressions;

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

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(HandlerDecorator1<>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(HandlerDecorator1<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_OnDecoratedType_GetsHandledAsExpected()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingle<ILogger>(logger);

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(RealCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(HandlerDecorator1<>));

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

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(HandlerDecorator1<>));
            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(HandlerDecorator2<>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(HandlerDecorator2<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances_GetsHandledAsExpected()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingle<ILogger>(logger);

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(RealCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(HandlerDecorator1<>));
            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(HandlerDecorator2<>));

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
            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(HandlerDecorator1<>));

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

        [TestMethod]
        public void GetInstance_DecoratorPredicateReturnsFalse_DoesNotDecorateInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), 
                c => false);

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(StubCommandHandler));
        }

        [TestMethod]
        public void GetInstance_DecoratorPredicateReturnsTrue_DecoratesInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>),
                c => true);

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(StubDecorator1<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_OnDecoratedType_CallsThePredicateWithTheExpectedServiceType()
        {
            // Arrange
            Type expectedPredicateServiceType = typeof(ICommandHandler<RealCommand>);
            Type actualPredicateServiceType = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c =>
            {
                actualPredicateServiceType = c.ServiceType;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(expectedPredicateServiceType, actualPredicateServiceType);
        }

        [TestMethod]
        public void GetInstance_OnDecoratedTransient_CallsThePredicateWithTheExpectedImplementationType()
        {
            // Arrange
            Type expectedPredicateImplementationType = typeof(StubCommandHandler);
            Type actualPredicateImplementationType = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c =>
            {
                actualPredicateImplementationType = c.ImplementationType;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(expectedPredicateImplementationType, actualPredicateImplementationType);
        }

        [TestMethod]
        public void GetInstance_OnDecoratedTransientWithInitializer_CallsThePredicateWithTheExpectedImplementationType()
        {
            // Arrange
            Type expectedPredicateImplementationType = typeof(StubCommandHandler);
            Type actualPredicateImplementationType = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterInitializer<StubCommandHandler>(handlerToInitialize => { });

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c =>
            {
                actualPredicateImplementationType = c.ImplementationType;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(expectedPredicateImplementationType, actualPredicateImplementationType);
        }

        [TestMethod]
        public void GetInstance_OnDecoratedSingleton_CallsThePredicateWithTheExpectedImplementationType()
        {
            // Arrange
            Type expectedPredicateImplementationType = typeof(StubCommandHandler);
            Type actualPredicateImplementationType = null;

            var container = new Container();

            container.RegisterSingle<ICommandHandler<RealCommand>>(new StubCommandHandler());

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c =>
            {
                actualPredicateImplementationType = c.ImplementationType;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(expectedPredicateImplementationType, actualPredicateImplementationType);
        }

        [TestMethod]
        public void GetInstance_OnDecoratedTypeRegisteredWithFuncDelegate_CallsThePredicateWithTheImplementationTypeEqualsServiceType()
        {
            // Arrange
            Type expectedPredicateImplementationType = typeof(ICommandHandler<RealCommand>);
            Type actualPredicateImplementationType = null;

            var container = new Container();

            // Because we registere a Func<TServiceType> there is no way we can determine the implementation 
            // type. In that case the ImplementationType should equal the ServiceType.
            container.Register<ICommandHandler<RealCommand>>(() => new StubCommandHandler());

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c =>
            {
                actualPredicateImplementationType = c.ImplementationType;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(expectedPredicateImplementationType, actualPredicateImplementationType);
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances_CallsThePredicateWithTheExpectedImplementationType()
        {
            // Arrange
            Type expectedPredicateImplementationType = typeof(StubCommandHandler);
            Type actualPredicateImplementationType = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>));
            
            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator2<>), c =>
            {
                actualPredicateImplementationType = c.ImplementationType;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(expectedPredicateImplementationType, actualPredicateImplementationType);
        }

        [TestMethod]
        public void GetInstance_OnDecoratedType_CallsThePredicateWithAnExpression()
        {
            // Arrange
            Expression actualPredicateExpression = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c =>
            {
                actualPredicateExpression = c.Expression;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsNotNull(actualPredicateExpression);
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances_SuppliesADifferentExpressionToTheSecondPredicate()
        {
            // Arrange
            Expression predicateExpressionOnFirstCall = null;
            Expression predicateExpressionOnSecondCall = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c =>
            {
                predicateExpressionOnFirstCall = c.Expression;
                return true;
            });

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator2<>), c =>
            {
                predicateExpressionOnSecondCall = c.Expression;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreNotEqual(predicateExpressionOnFirstCall, predicateExpressionOnSecondCall,
                "The predicate was expected to change, because the first decorator has been applied.");
        }

        [TestMethod]
        public void GetInstance_OnDecoratedType_SuppliesNoAppliedDecoratorsToThePredicate()
        {
            // Arrange
            Type[] appliedDecorators = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator2<>), c =>
            {
                appliedDecorators = c.AppliedDecorators;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(0, appliedDecorators.Length);
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances1_SuppliesNoAppliedDecoratorsToThePredicate()
        {
            // Arrange
            Type[] appliedDecorators = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator2<>), c =>
            {
                appliedDecorators = c.AppliedDecorators;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(1, appliedDecorators.Length);
            Assert.AreEqual(typeof(StubDecorator1<RealCommand>), appliedDecorators[0]);
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances2_SuppliesNoAppliedDecoratorsToThePredicate()
        {
            // Arrange
            Type[] appliedDecorators = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>));
            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator2<>));
            container.RegisterOpenGenericDecorator(typeof(ICommandHandler<>), typeof(StubDecorator2<>), c =>
            {
                appliedDecorators = c.AppliedDecorators;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(2, appliedDecorators.Length);
            Assert.AreEqual(typeof(StubDecorator1<RealCommand>), appliedDecorators[0]);
            Assert.AreEqual(typeof(StubDecorator2<RealCommand>), appliedDecorators[1]);
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

        private class StubDecorator1<T> : ICommandHandler<T>
        {
            public StubDecorator1(ICommandHandler<T> wrapped)
            {
            }

            public void Handle(T command)
            {
            }
        }

        private class StubDecorator2<T> : ICommandHandler<T>
        {
            public StubDecorator2(ICommandHandler<T> wrapped)
            {
            }

            public void Handle(T command)
            {
            }
        }

        private class HandlerDecorator1<T> : ICommandHandler<T>
        {
            private readonly ICommandHandler<T> wrapped;
            private readonly ILogger logger;

            public HandlerDecorator1(ICommandHandler<T> wrapped, ILogger logger)
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

        private class HandlerDecorator2<T> : ICommandHandler<T>
        {
            private readonly ICommandHandler<T> wrapped;
            private readonly ILogger logger;

            public HandlerDecorator2(ICommandHandler<T> wrapped, ILogger logger)
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