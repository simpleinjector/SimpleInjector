namespace SimpleInjector.Extensions.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DecoratorExtensionsTests
    {
        public interface ILogger
        {
            void Log(string message);
        }

        public interface ICommandHandler<TCommand>
        {
            void Handle(TCommand command);
        }

        public interface INonGenericService
        {
            void DoSomething();
        }

        [TestMethod]
        public void GetInstance_OnDecoratedNonGenericType_ReturnsTheDecoratedService()
        {
            // Arrange
            var container = new Container();

            container.Register<INonGenericService, RealNonGenericService>();
            container.RegisterDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator));

            // Act
            var service = container.GetInstance<INonGenericService>();

            // Assert
            Assert.IsInstanceOfType(service, typeof(NonGenericServiceDecorator));

            var decorator = (NonGenericServiceDecorator)service;

            Assert.IsInstanceOfType(decorator.DecoratedService, typeof(RealNonGenericService));
        }

        [TestMethod]
        public void RegisterDecorator_RegisteringAnOpenGenericDecoratorWithANonGenericService_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.RegisterDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator<>));
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(
                    "The supplied decorator 'NonGenericServiceDecorator<T>' is an open generic type definition",
                    ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_OnNonGenericTypeDecoratedWithGenericDecorator_ReturnsTheDecoratedService()
        {
            // Arrange
            var container = new Container();

            container.Register<INonGenericService, RealNonGenericService>();
            container.RegisterDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator<int>));

            // Act
            var service = container.GetInstance<INonGenericService>();

            // Assert
            Assert.IsInstanceOfType(service, typeof(NonGenericServiceDecorator<int>));

            var decorator = (NonGenericServiceDecorator<int>)service;

            Assert.IsInstanceOfType(decorator.DecoratedService, typeof(RealNonGenericService));
        }

        [TestMethod]
        public void GetInstance_OnDecoratedType_ReturnsTheDecorator()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingle<ILogger>(logger);

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(RealCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LoggingHandlerDecorator1<>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(LoggingHandlerDecorator1<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_WithExplicitGenericImplementionRegisteredAsDecoratorThatMatchesTheRequestedService1_ReturnsTheDecorator()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ICommandHandler<RealCommand>>(new RealCommandHandler(null));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<RealCommand>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(StubDecorator1<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_WithExplicitGenericImplementionRegisteredAsDecoratorThatMatchesTheRequestedService2_ReturnsTheDecorator()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ICommandHandler<RealCommand>>(new RealCommandHandler(null));

            container.RegisterDecorator(typeof(ICommandHandler<RealCommand>), typeof(StubDecorator1<RealCommand>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(StubDecorator1<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_WithExplicitGenericImplementionRegisteredAsDecoratorThatDoesNotMatchTheRequestedService1_ReturnsTheServiceItself()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ICommandHandler<RealCommand>>(new RealCommandHandler(null));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<int>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(RealCommandHandler));
        }

        [TestMethod]
        public void GetInstance_WithExplicitGenericImplementionRegisteredAsDecoratorThatDoesNotMatchTheRequestedService2_ReturnsTheServiceItself()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingle<ICommandHandler<RealCommand>>(new RealCommandHandler(null));

            container.RegisterDecorator(typeof(ICommandHandler<int>), typeof(StubDecorator1<int>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(RealCommandHandler));
        }

        [TestMethod]
        public void GetInstance_OnDecoratedType_GetsHandledAsExpected()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingle<ILogger>(logger);

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(RealCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LoggingHandlerDecorator1<>));

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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LoggingHandlerDecorator1<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LoggingHandlerDecorator2<>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(LoggingHandlerDecorator2<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances_GetsHandledAsExpected()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingle<ILogger>(logger);

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(RealCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LoggingHandlerDecorator1<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LoggingHandlerDecorator2<>));

            // Act
            container.GetInstance<ICommandHandler<RealCommand>>().Handle(new RealCommand());

            // Assert
            Assert.AreEqual("Begin2 Begin1 RealCommand End1 End2", logger.Message);
        }

        [TestMethod]
        public void GetInstance_WithInitializerOnDecorator_InitializesThatDecorator()
        {
            // Arrange
            int expectedItem1Value = 1;
            string expectedItem2Value = "some value";

            var container = new Container();

            container.RegisterInitializer<HandlerDecoratorWithPropertiesBase>(decorator =>
            {
                decorator.Item1 = expectedItem1Value;
            });

            container.RegisterInitializer<HandlerDecoratorWithPropertiesBase>(decorator =>
            {
                decorator.Item2 = expectedItem2Value;
            });

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(HandlerDecoratorWithProperties<>));

            // Act
            var handler =
                (HandlerDecoratorWithPropertiesBase)container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(expectedItem1Value, handler.Item1, "Initializer did not run.");
            Assert.AreEqual(expectedItem2Value, handler.Item2, "Initializer did not run.");
        }

        [TestMethod]
        public void GetInstance_DecoratorWithMissingDependency_ThrowAnExceptionWithADescriptiveMessage()
        {
            // Arrange
            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            // Decorator1Handler depends on ILogger, but ILogger is not registered.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LoggingHandlerDecorator1<>));

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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c => false);

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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>),
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c =>
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c =>
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c =>
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c =>
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c =>
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator2<>), c =>
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c =>
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>), c =>
            {
                predicateExpressionOnFirstCall = c.Expression;
                return true;
            });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator2<>), c =>
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
            IEnumerable<Type> appliedDecorators = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator2<>), c =>
            {
                appliedDecorators = c.AppliedDecorators;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(0, appliedDecorators.Count());
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances1_SuppliesNoAppliedDecoratorsToThePredicate()
        {
            // Arrange
            IEnumerable<Type> appliedDecorators = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator2<>), c =>
            {
                appliedDecorators = c.AppliedDecorators;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(1, appliedDecorators.Count());
            Assert.AreEqual(typeof(StubDecorator1<RealCommand>), appliedDecorators.First());
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances2_SuppliesNoAppliedDecoratorsToThePredicate()
        {
            // Arrange
            IEnumerable<Type> appliedDecorators = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator1<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator2<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(StubDecorator2<>), c =>
            {
                appliedDecorators = c.AppliedDecorators;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(2, appliedDecorators.Count());
            Assert.AreEqual(typeof(StubDecorator1<RealCommand>), appliedDecorators.First());
            Assert.AreEqual(typeof(StubDecorator2<RealCommand>), appliedDecorators.Second());
        }

        [TestMethod]
        public void GetInstance_DecoratorThatSatisfiesRequestedTypesTypeConstraints_DecoratesThatInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(ClassConstraintHandlerDecorator<>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(ClassConstraintHandlerDecorator<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_DecoratorThatDoesNotSatisfyRequestedTypesTypeConstraints_DoesNotDecorateThatInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StructCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(ClassConstraintHandlerDecorator<>));

            // Act
            var handler = container.GetInstance<ICommandHandler<StructCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(StructCommandHandler));
        }

        [TestMethod]
        public void RegisterDecorator_DecoratorWithMultiplePublicConstructors_ThrowsException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.RegisterDecorator(typeof(ICommandHandler<>),
                    typeof(MultipleConstructorsCommandHandlerDecorator<>));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains("it should contain exactly one public constructor", ex.Message);
            }
        }

        [TestMethod]
        public void RegisterDecorator_SupplyingTypeThatIsNotADecorator_ThrowsException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.RegisterDecorator(typeof(ICommandHandler<>),
                    typeof(NoDecoratorCommandHandlerDecorator<>));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(
                    "its constructor should have a single argument of type ICommandHandler<TCommand>",
                    ex.Message);
            }
        }

        [TestMethod]
        public void RegisterDecorator_SupplyingAnUnrelatedType_FailsWithExpectedException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.RegisterDecorator(typeof(ICommandHandler<>), typeof(KeyValuePair<,>));
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(
                    "The supplied type 'KeyValuePair<TKey, TValue>' does not inherit from " +
                    "or implement 'ICommandHandler<TCommand>'.",
                    ex.Message);
            }
        }

        [TestMethod]
        public void RegisterDecorator_SupplyingAConcreteNonGenericType_ShouldSucceed()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ConcreteCommandHandlerDecorator));
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorSupplyingAConcreteNonGenericType_ReturnsExpectedDecorator1()
        {
            // Arrange
            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ConcreteCommandHandlerDecorator));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(ConcreteCommandHandlerDecorator));
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorSupplyingAConcreteNonGenericTypeThatDoesNotMatch_DoesNotReturnThatDecorator()
        {
            // Arrange
            var container = new Container();

            // StructCommandHandler implements ICommandHandler<StructCommand>
            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StructCommandHandler));

            // ConcreteCommandHandlerDecorator implements ICommandHandler<RealCommand>
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ConcreteCommandHandlerDecorator));

            // Act
            var handler = container.GetInstance<ICommandHandler<StructCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(StructCommandHandler));
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorSupplyingAConcreteNonGenericType_ReturnsExpectedDecorator2()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();
            container.Register<ICommandHandler<StructCommand>, StructCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ConcreteCommandHandlerDecorator));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(ConcreteCommandHandlerDecorator));
        }

        [TestMethod]
        public void GetAllInstances_TypeDecorated_ReturnsCollectionWithDecorators()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll(typeof(ICommandHandler<RealCommand>), new[] { typeof(StubCommandHandler) });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ConcreteCommandHandlerDecorator));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var handler = handlers.Single();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(ConcreteCommandHandlerDecorator));

            Assert.IsInstanceOfType(((ConcreteCommandHandlerDecorator)handler).DecoratedHandler,
                typeof(StubCommandHandler));
        }

        [TestMethod]
        public void GetAllInstances_TypeDecoratedWithMultipleDecorators_ReturnsCollectionWithDecorators()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ILogger>(new FakeLogger());

            container.RegisterAll(typeof(ICommandHandler<RealCommand>), new[] { typeof(StubCommandHandler) });

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LoggingHandlerDecorator1<>));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ConcreteCommandHandlerDecorator));

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<RealCommand>>();

            var handler = handlers.Single();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(ConcreteCommandHandlerDecorator));

            Assert.IsInstanceOfType(((ConcreteCommandHandlerDecorator)handler).DecoratedHandler,
                typeof(LoggingHandlerDecorator1<RealCommand>));
        }

        public struct StructCommand
        {
        }

        public sealed class FakeLogger : ILogger
        {
            public string Message { get; private set; }

            public void Log(string message)
            {
                this.Message += message;
            }
        }

        public class RealCommand
        {
        }

        public class MultipleConstructorsCommandHandlerDecorator<T> : ICommandHandler<T>
        {
            public MultipleConstructorsCommandHandlerDecorator()
            {
            }

            public MultipleConstructorsCommandHandlerDecorator(ICommandHandler<T> decorated)
            {
            }

            public void Handle(T command)
            {
            }
        }

        public class NoDecoratorCommandHandlerDecorator<T> : ICommandHandler<T>
        {
            public NoDecoratorCommandHandlerDecorator(ILogger logger)
            {
            }

            public void Handle(T command)
            {
            }
        }

        public class StubCommandHandler : ICommandHandler<RealCommand>
        {
            public void Handle(RealCommand command)
            {
            }
        }

        public class StructCommandHandler : ICommandHandler<StructCommand>
        {
            public void Handle(StructCommand command)
            {
            }
        }

        public class RealCommandHandler : ICommandHandler<RealCommand>
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

        public class ConcreteCommandHandlerDecorator : ICommandHandler<RealCommand>
        {
            public ConcreteCommandHandlerDecorator(ICommandHandler<RealCommand> decoratedHandler)
            {
                this.DecoratedHandler = decoratedHandler;
            }

            public ICommandHandler<RealCommand> DecoratedHandler { get; private set; }

            public void Handle(RealCommand command)
            {
            }
        }

        public class StubDecorator1<T> : ICommandHandler<T>
        {
            public StubDecorator1(ICommandHandler<T> wrapped)
            {
            }

            public void Handle(T command)
            {
            }
        }

        public class StubDecorator2<T> : ICommandHandler<T>
        {
            public StubDecorator2(ICommandHandler<T> wrapped)
            {
            }

            public void Handle(T command)
            {
            }
        }

        public class LoggingHandlerDecorator1<T> : ICommandHandler<T>
        {
            private readonly ICommandHandler<T> wrapped;
            private readonly ILogger logger;

            public LoggingHandlerDecorator1(ICommandHandler<T> wrapped, ILogger logger)
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

        public class LoggingHandlerDecorator2<T> : ICommandHandler<T>
        {
            private readonly ICommandHandler<T> wrapped;
            private readonly ILogger logger;

            public LoggingHandlerDecorator2(ICommandHandler<T> wrapped, ILogger logger)
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

        public class ClassConstraintHandlerDecorator<T> : ICommandHandler<T> where T : class
        {
            public ClassConstraintHandlerDecorator(ICommandHandler<T> wrapped)
            {
            }

            public void Handle(T command)
            {
            }
        }

        public class HandlerDecoratorWithPropertiesBase
        {
            public int Item1 { get; set; }

            public string Item2 { get; set; }
        }

        public class HandlerDecoratorWithProperties<T> : HandlerDecoratorWithPropertiesBase, ICommandHandler<T>
        {
            private readonly ICommandHandler<T> wrapped;

            public HandlerDecoratorWithProperties(ICommandHandler<T> wrapped)
            {
                this.wrapped = wrapped;
            }

            public void Handle(T command)
            {
            }
        }

        public class RealNonGenericService : INonGenericService
        {
            public void DoSomething()
            {
            }
        }

        public class NonGenericServiceDecorator : INonGenericService
        {
            public NonGenericServiceDecorator(INonGenericService decorated)
            {
                this.DecoratedService = decorated;
            }

            public INonGenericService DecoratedService { get; private set; }

            public void DoSomething()
            {
                this.DecoratedService.DoSomething();
            }
        }

        public class NonGenericServiceDecorator<T> : INonGenericService
        {
            public NonGenericServiceDecorator(INonGenericService decorated)
            {
                this.DecoratedService = decorated;
            }

            public INonGenericService DecoratedService { get; private set; }

            public void DoSomething()
            {
                this.DecoratedService.DoSomething();
            }
        }
    }
}