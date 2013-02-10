namespace SimpleInjector.Tests.Unit.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Diagnostics;
    using SimpleInjector.Extensions;
    using SimpleInjector.Lifestyles;

    public interface ILogger
    {
        void Log(string message);
    }

    public interface ISpecialCommand
    {
    }

    public interface ICommandHandler<TCommand>
    {
        void Handle(TCommand command);
    }

    public interface INonGenericService
    {
        void DoSomething();
    }

    public struct StructCommand
    {
    }

    [TestClass]
    public class DecoratorExtensionsTests
    {
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
        public void GetInstance_OnDecoratedNonGenericSingleton_ReturnsTheDecoratedService()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<INonGenericService, RealNonGenericService>();
            container.RegisterDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator));

            // Act
            var service = container.GetInstance<INonGenericService>();

            // Assert
            Assert.IsInstanceOfType(service, typeof(NonGenericServiceDecorator));

            var decorator = (NonGenericServiceDecorator)service;

            Assert.IsInstanceOfType(decorator.DecoratedService, typeof(RealNonGenericService));
        }

        [TestMethod]
        public void GetInstance_SingleInstanceWrappedByATransientDecorator_ReturnsANewDecoratorEveryTime()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<INonGenericService, RealNonGenericService>();
            container.RegisterDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator));

            // Act
            var decorator1 = (NonGenericServiceDecorator)container.GetInstance<INonGenericService>();
            var decorator2 = (NonGenericServiceDecorator)container.GetInstance<INonGenericService>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(decorator1, decorator2),
                "A new decorator should be created on each call to GetInstance().");
            Assert.IsTrue(object.ReferenceEquals(decorator1.DecoratedService, decorator2.DecoratedService),
                "The same instance should be wrapped on each call to GetInstance().");
        }

        [TestMethod]
        public void GetInstance_OnDecoratedNonGenericType_DecoratesInstanceWithExpectedLifeTime()
        {
            // Arrange
            var container = new Container();

            // Register as transient
            container.Register<INonGenericService, RealNonGenericService>();
            container.RegisterDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator));

            // Act
            var decorator1 = (NonGenericServiceDecorator)container.GetInstance<INonGenericService>();
            var decorator2 = (NonGenericServiceDecorator)container.GetInstance<INonGenericService>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(decorator1.DecoratedService, decorator2.DecoratedService),
                "The decorated instance is expected to be a transient.");
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

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(@"
                    The supplied decorator NonGenericServiceDecorator<T> is an open
                    generic type definition".TrimInside(),
                    ex.Message);

                AssertThat.ExceptionContainsParamName(ex, "decoratorType");
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

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(RealCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(TransactionHandlerDecorator<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_WithExplicitGenericImplementionRegisteredAsDecoratorThatMatchesTheRequestedService1_ReturnsTheDecorator()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ICommandHandler<RealCommand>>(new RealCommandHandler());

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<RealCommand>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(TransactionHandlerDecorator<RealCommand>));
        }

        [TestMethod]
        public void RegisterDecorator_WithClosedGenericServiceAndOpenGenericDecorator_FailsWithExpectedException()
        {
            // Arrange
            string expectedMessage = @"
                Registering a closed generic service type with an open generic decorator is not supported. 
                Instead, register the service type as open generic, and the decorator as closed generic 
                type."
                .TrimInside();

            var container = new Container();

            container.RegisterSingle<ICommandHandler<RealCommand>>(new RealCommandHandler());

            try
            {
                // Act
                container.RegisterDecorator(
                    typeof(ICommandHandler<RealCommand>),
                    typeof(TransactionHandlerDecorator<>));

                // Assert
                Assert.Fail("Exception excepted.");
            }
            catch (NotSupportedException ex)
            {
                AssertThat.ExceptionMessageContains(expectedMessage, ex);
            }
        }

        [TestMethod]
        public void GetInstance_WithExplicitGenericImplementionRegisteredAsDecoratorThatMatchesTheRequestedService2_ReturnsTheDecorator()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ICommandHandler<RealCommand>>(new RealCommandHandler());

            container.RegisterDecorator(
                typeof(ICommandHandler<RealCommand>),
                typeof(TransactionHandlerDecorator<RealCommand>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(TransactionHandlerDecorator<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_WithExplicitGenericImplementionRegisteredAsDecoratorThatDoesNotMatchTheRequestedService1_ReturnsTheServiceItself()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ICommandHandler<RealCommand>>(new RealCommandHandler());

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<int>));

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

            container.RegisterSingle<ICommandHandler<RealCommand>>(new RealCommandHandler());

            container.RegisterDecorator(typeof(ICommandHandler<int>), typeof(TransactionHandlerDecorator<int>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(RealCommandHandler));
        }

        [TestMethod]
        public void GetInstance_NonGenericDecoratorForMatchingClosedGenericServiceType_ReturnsTheNonGenericDecorator()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ICommandHandler<RealCommand>>(new RealCommandHandler());

            Type closedGenericServiceType = typeof(ICommandHandler<RealCommand>);
            Type nonGenericDecorator = typeof(RealCommandHandlerDecorator);

            container.RegisterDecorator(closedGenericServiceType, nonGenericDecorator);

            // Act
            var decorator = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(decorator, nonGenericDecorator);
        }

        [TestMethod]
        public void GetInstance_NonGenericDecoratorForNonMatchingClosedGenericServiceType_ThrowsAnException()
        {
            // Arrange
            var container = new Container();

            Type nonMathcingClosedGenericServiceType = typeof(ICommandHandler<int>);

            // Decorator implements ICommandHandler<RealCommand>
            Type nonGenericDecorator = typeof(RealCommandHandlerDecorator);

            try
            {
                // Act
                container.RegisterDecorator(nonMathcingClosedGenericServiceType, nonGenericDecorator);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(
                    "The supplied type RealCommandHandlerDecorator does not " +
                    "implement ICommandHandler<Int32>", ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_OnDecoratedType_GetsHandledAsExpected()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingle<ILogger>(logger);

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(LoggingRealCommandHandler));

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

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(RealCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LogExceptionCommandHandlerDecorator<>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(LogExceptionCommandHandlerDecorator<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances_GetsHandledAsExpected()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingle<ILogger>(logger);

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(LoggingRealCommandHandler));

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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>), c => false);

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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>),
                c => true);

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(TransactionHandlerDecorator<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_OnDecoratedType_CallsThePredicateWithTheExpectedServiceType()
        {
            // Arrange
            Type expectedPredicateServiceType = typeof(ICommandHandler<RealCommand>);
            Type actualPredicateServiceType = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>), c =>
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>), c =>
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>), c =>
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
        public void GetInstance_SingletonDecoratorWithInitializer_ShouldReturnSingleton()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();

            container.RegisterSingleDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            container.RegisterInitializer<AsyncCommandHandlerProxy<RealCommand>>(handler => { });

            // Act
            var handler1 = container.GetInstance<ICommandHandler<RealCommand>>();
            var handler2 = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler1, typeof(AsyncCommandHandlerProxy<RealCommand>));

            Assert.IsTrue(object.ReferenceEquals(handler1, handler2),
                "GetInstance should always return the same instance, since AsyncCommandHandlerProxy is " +
                "registered as singleton.");
        }

        [TestMethod]
        public void GetInstance_OnDecoratedSingleton_CallsThePredicateWithTheExpectedImplementationType()
        {
            // Arrange
            Type expectedPredicateImplementationType = typeof(StubCommandHandler);
            Type actualPredicateImplementationType = null;

            var container = new Container();

            container.RegisterSingle<ICommandHandler<RealCommand>>(new StubCommandHandler());

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>), c =>
            {
                actualPredicateImplementationType = c.ImplementationType;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.AreEqual(expectedPredicateImplementationType, actualPredicateImplementationType);
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>), c =>
            {
                actualPredicateImplementationType = c.ImplementationType;
                return true;
            });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.AreEqual(expectedPredicateImplementationType, actualPredicateImplementationType);
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances_CallsThePredicateWithTheExpectedImplementationType()
        {
            // Arrange
            Type expectedPredicateImplementationType = typeof(StubCommandHandler);
            Type actualPredicateImplementationType = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(LogExceptionCommandHandlerDecorator<>), c =>
                {
                    actualPredicateImplementationType = c.ImplementationType;
                    return true;
                });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.AreEqual(expectedPredicateImplementationType, actualPredicateImplementationType);
        }

        [TestMethod]
        public void GetInstance_OnDecoratedType_CallsThePredicateWithAnExpression()
        {
            // Arrange
            Expression actualPredicateExpression = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>), c =>
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>), c =>
            {
                predicateExpressionOnFirstCall = c.Expression;
                return true;
            });

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(LogExceptionCommandHandlerDecorator<>), c =>
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

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(LogExceptionCommandHandlerDecorator<>), c =>
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(LogExceptionCommandHandlerDecorator<>), c =>
                {
                    appliedDecorators = c.AppliedDecorators;
                    return true;
                });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(1, appliedDecorators.Count());
            Assert.AreEqual(typeof(TransactionHandlerDecorator<RealCommand>), appliedDecorators.First());
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances2_SuppliesNoAppliedDecoratorsToThePredicate()
        {
            // Arrange
            IEnumerable<Type> appliedDecorators = null;

            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(LogExceptionCommandHandlerDecorator<>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(LogExceptionCommandHandlerDecorator<>), c =>
                {
                    appliedDecorators = c.AppliedDecorators;
                    return true;
                });

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(2, appliedDecorators.Count());
            Assert.AreEqual(typeof(TransactionHandlerDecorator<RealCommand>), appliedDecorators.First());
            Assert.AreEqual(typeof(LogExceptionCommandHandlerDecorator<RealCommand>), appliedDecorators.Second());
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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ClassConstraintHandlerDecorator<>));

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
                    typeof(InvalidDecoratorCommandHandlerDecorator<>));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(@"
                    its constructor should have a single argument of type 
                    ICommandHandler<TCommand>".TrimInside(),
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
                AssertThat.StringContains(@"
                    The supplied type KeyValuePair<TKey, TValue> does not implement 
                    ICommandHandler<TCommand>.
                    ".TrimInside(),
                    ex.Message);
            }
        }

        [TestMethod]
        public void RegisterDecorator_SupplyingAConcreteNonGenericType_ShouldSucceed()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorSupplyingAConcreteNonGenericType_ReturnsExpectedDecorator1()
        {
            // Arrange
            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StubCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(RealCommandHandlerDecorator));
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorSupplyingAConcreteNonGenericTypeThatDoesNotMatch_DoesNotReturnThatDecorator()
        {
            // Arrange
            var container = new Container();

            // StructCommandHandler implements ICommandHandler<StructCommand>
            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), typeof(StructCommandHandler));

            // ConcreteCommandHandlerDecorator implements ICommandHandler<RealCommand>
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

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

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(RealCommandHandlerDecorator));
        }

        [TestMethod]
        public void RegisterDecorator_NonGenericDecoratorWithFuncAsConstructorArgument_InjectsAFactoryThatCreatesNewInstancesOfTheDecoratedType()
        {
            // Arrange
            var container = new Container();

            container.Register<INonGenericService, RealNonGenericService>();

            container.RegisterDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecoratorWithFunc));

            var decorator = (NonGenericServiceDecoratorWithFunc)container.GetInstance<INonGenericService>();

            Func<INonGenericService> factory = decorator.DecoratedServiceCreator;

            // Act
            // Execute the factory twice.
            INonGenericService instance1 = factory();
            INonGenericService instance2 = factory();

            // Assert
            Assert.IsInstanceOfType(instance1, typeof(RealNonGenericService),
                "The injected factory is expected to create instances of type RealNonGenericService.");

            Assert.IsFalse(object.ReferenceEquals(instance1, instance2),
                "The factory is expected to create transient instances, since that is how " +
                "RealNonGenericService is registered.");
        }

        [TestMethod]
        public void RegisterDecorator_GenericDecoratorWithFuncAsConstructorArgument_InjectsAFactoryThatCreatesNewInstancesOfTheDecoratedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<ILogger>(new FakeLogger());

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LogExceptionCommandHandlerDecorator<>));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            // Act
            var handler =
                (AsyncCommandHandlerProxy<RealCommand>)container.GetInstance<ICommandHandler<RealCommand>>();

            Func<ICommandHandler<RealCommand>> factory = handler.DecorateeFactory;

            // Execute the factory twice.
            ICommandHandler<RealCommand> instance1 = factory();
            ICommandHandler<RealCommand> instance2 = factory();

            // Assert
            Assert.IsInstanceOfType(instance1, typeof(LogExceptionCommandHandlerDecorator<RealCommand>),
                "The injected factory is expected to create instances of type " +
                "LogAndContinueCommandHandlerDecorator<RealCommand>.");

            Assert.IsFalse(object.ReferenceEquals(instance1, instance2),
                "The factory is expected to create transient instances.");
        }

        [TestMethod]
        public void RegisterDecorator_CalledWithDecoratorTypeWithBothAFuncAndADecorateeParameter_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.RegisterDecorator(typeof(INonGenericService),
                    typeof(NonGenericServiceDecoratorWithBothDecorateeAndFunc));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains("its constructor should have a single argument of type " +
                    "INonGenericService or Func<INonGenericService>",
                    ex.Message);
            }
        }

        [TestMethod]
        public void RegisterDecorator_RegisteringAClassThatWrapsADifferentClosedTypeThanItImplements_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                // BadCommandHandlerDecorator1<T> implements ICommandHandler<int> but wraps ICommandHandler<T>
                container.RegisterDecorator(typeof(ICommandHandler<>),
                    typeof(BadCommandHandlerDecorator1));

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(
                    @"its constructor should have an argument of one of the following types:
                    ICommandHandler<Int32>, Func<ICommandHandler<Int32>>".TrimInside(),
                    ex.Message);

                AssertThat.ExceptionContainsParamName(ex, "decoratorType");
            }
        }

        [TestMethod]
        public void RegisterDecorator_RegisteringADecoratorWithAnUnresolvableTypeArgument_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                // BadCommandHandlerDecorator2<T, TUnresolved> contains an unmappable type argument TUnresolved.
                container.RegisterDecorator(typeof(ICommandHandler<>),
                    typeof(BadCommandHandlerDecorator2<,>));

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(
                    @"contains unresolvable type arguments.",
                    ex.Message);

                AssertThat.ExceptionContainsParamName(ex, "decoratorType");
            }
        }

        [TestMethod]
        public void GetInstance_TypeRegisteredWithRegisterSingleDecorator_AlwaysReturnsTheSameInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<INonGenericService, RealNonGenericService>();

            container.RegisterSingleDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator));

            // Act
            var decorator1 = container.GetInstance<INonGenericService>();
            var decorator2 = container.GetInstance<INonGenericService>();

            // Assert
            Assert.IsInstanceOfType(decorator1, typeof(NonGenericServiceDecorator));

            Assert.IsTrue(object.ReferenceEquals(decorator1, decorator2),
                "Since the decorator is registered as singleton, GetInstance should always return the same " +
                "instance.");
        }

        [TestMethod]
        public void GetInstance_TypeRegisteredWithRegisterSingleDecoratorPredicate_AlwaysReturnsTheSameInstance()
        {
            // Arrange
            var container = new Container();

            container.Register<INonGenericService, RealNonGenericService>();

            container.RegisterSingleDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator),
                c => true);

            // Act
            var decorator1 = container.GetInstance<INonGenericService>();
            var decorator2 = container.GetInstance<INonGenericService>();

            // Assert
            Assert.IsInstanceOfType(decorator1, typeof(NonGenericServiceDecorator));

            Assert.IsTrue(object.ReferenceEquals(decorator1, decorator2),
                "Since the decorator is registered as singleton, GetInstance should always return the same " +
                "instance.");
        }

        [TestMethod]
        public void Verify_DecoratorRegisteredThatCanNotBeResolved_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();

            // LoggingHandlerDecorator1 depends on ILogger, which is not registered.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LoggingHandlerDecorator1<>));

            try
            {
                // Act
                container.Verify();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains(@"
                    The constructor of the type LoggingHandlerDecorator1<RealCommand> 
                    contains the parameter of type ILogger with name 'logger' that is 
                    not registered.".TrimInside(), ex);
            }
        }

        [TestMethod]
        public void GetInstance_DecoratorRegisteredTwiceAsSingleton_WrapsTheDecorateeTwice()
        {
            // Arrange
            var container = new Container();

            // Uses the RegisterAll<T>(IEnumerable<T>) that registers a dynamic list.
            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

            // Register the same decorator twice. 
            container.RegisterSingleDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionHandlerDecorator<>));

            container.RegisterSingleDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionHandlerDecorator<>));

            // Act
            var decorator1 = (TransactionHandlerDecorator<RealCommand>)
                container.GetInstance<ICommandHandler<RealCommand>>();

            var decorator2 = decorator1.Decorated;

            // Assert
            Assert.IsInstanceOfType(decorator2, typeof(TransactionHandlerDecorator<RealCommand>),
                "Since the decorator is registered twice, it should wrap the decoratee twice.");

            var decoratee = ((TransactionHandlerDecorator<RealCommand>)decorator2).Decorated;

            Assert.IsInstanceOfType(decoratee, typeof(StubCommandHandler));
        }

        [TestMethod]
        public void HybridLifestyleRegistration_WithDecorator_DecoratesTheInstance()
        {
            // Arrange
            var hybrid = new HybridLifestyle(() => true, Lifestyle.Transient, Lifestyle.Singleton);

            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(hybrid);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(RealCommandHandlerDecorator));
        }

        [TestMethod]
        public void HybridLifestyleRegistration_WithTransientDecorator_AppliesTransientDecorator()
        {
            // Arrange
            var hybrid = new HybridLifestyle(() => false, Lifestyle.Singleton, Lifestyle.Singleton);

            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(hybrid);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handler1 = container.GetInstance<ICommandHandler<RealCommand>>();
            var handler2 = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(handler1, handler2), "Decorator should be transient.");
        }

        [TestMethod]
        public void HybridLifestyleRegistration_WithTransientDecorator_DoesNotApplyDecoratorMultipleTimes()
        {
            // Arrange
            var hybrid = new HybridLifestyle(() => false, Lifestyle.Singleton, Lifestyle.Singleton);

            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(hybrid);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handler = (RealCommandHandlerDecorator)container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(handler.Decorated, typeof(StubCommandHandler));
        }

        [TestMethod]
        public void HybridLifestyleRegistration_WithTransientDecorator_LeavesTheLifestyleInTact1()
        {
            // Arrange
            var hybrid = new HybridLifestyle(() => false, Lifestyle.Singleton, Lifestyle.Singleton);

            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(hybrid);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handler1 = (RealCommandHandlerDecorator)container.GetInstance<ICommandHandler<RealCommand>>();
            var handler2 = (RealCommandHandlerDecorator)container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(handler1.Decorated, handler2.Decorated),
                "The wrapped instance should have the expected lifestyle (singleton in this case).");
        }

        [TestMethod]
        public void HybridLifestyleRegistration_WithTransientDecorator_LeavesTheLifestyleInTact2()
        {
            // Arrange
            var hybrid = new HybridLifestyle(() => false, Lifestyle.Transient, Lifestyle.Transient);

            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(hybrid);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handler1 = (RealCommandHandlerDecorator)container.GetInstance<ICommandHandler<RealCommand>>();
            var handler2 = (RealCommandHandlerDecorator)container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(handler1.Decorated, handler2.Decorated),
                "The wrapped instance should have the expected lifestyle (transient in this case).");
        }

#if DEBUG
        [TestMethod]
        public void GetRegistration_TransientInstanceDecoratedWithTransientDecorator_ContainsTheExpectedRelationship()
        {
            // Arrange
            var expectedRelationship = new RelationshipInfo
            {
                Lifestyle = Lifestyle.Transient,
                ImplementationType = typeof(RealCommandHandlerDecorator),
                Dependency = new DependencyInfo(typeof(ICommandHandler<RealCommand>), Lifestyle.Transient)
            };

            var container = new Container();

            // StubCommandHandler has no dependencies.
            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

            // RealCommandHandlerDecorator only has ICommandHandler<RealCommand> as dependency.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            container.Verify();

            // Act
            var actualRelationship = container.GetRegistration(typeof(ICommandHandler<RealCommand>))
                .GetRelationships()
                .Single();

            // Assert
            Assert.IsTrue(expectedRelationship.Equals(actualRelationship));
        }

        [TestMethod]
        public void GetRegistration_TransientInstanceDecoratedWithSingletonDecorator_ContainsTheExpectedRelationship()
        {
            // Arrange
            var expectedRelationship = new RelationshipInfo
            {
                Lifestyle = Lifestyle.Singleton,
                ImplementationType = typeof(RealCommandHandlerDecorator),
                Dependency = new DependencyInfo(typeof(ICommandHandler<RealCommand>), Lifestyle.Transient)
            };

            var container = new Container();

            // StubCommandHandler has no dependencies.
            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

            // RealCommandHandlerDecorator only has ICommandHandler<RealCommand> as dependency.
            container.RegisterSingleDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            container.Verify();

            // Act
            var actualRelationship = container.GetRegistration(typeof(ICommandHandler<RealCommand>))
                .GetRelationships()
                .Single();

            // Assert
            Assert.IsTrue(expectedRelationship.Equals(actualRelationship));
        }

        [TestMethod]
        public void GetRegistration_DecoratorWithNormalDependency_ContainsTheExpectedRelationship()
        {
            // Arrange
            var expectedRelationship1 = new RelationshipInfo
            {
                ImplementationType = typeof(LoggingHandlerDecorator1<RealCommand>),
                Lifestyle = Lifestyle.Transient,
                Dependency = new DependencyInfo(typeof(ILogger), Lifestyle.Singleton)
            };

            var expectedRelationship2 = new RelationshipInfo
            {
                ImplementationType = typeof(LoggingHandlerDecorator1<RealCommand>),
                Lifestyle = Lifestyle.Transient,
                Dependency = new DependencyInfo(typeof(ICommandHandler<RealCommand>), Lifestyle.Transient)
            };

            var container = new Container();

            container.RegisterSingle<ILogger, FakeLogger>();

            // StubCommandHandler has no dependencies.
            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

            // LoggingHandlerDecorator1 takes a dependency on ILogger.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LoggingHandlerDecorator1<>));

            container.Verify();

            // Act
            var relationships =
                container.GetRegistration(typeof(ICommandHandler<RealCommand>)).GetRelationships();

            // Assert
            // I'm too lazy to split this up in two tests :-)
            Assert.AreEqual(1, relationships.Count(actual => expectedRelationship1.Equals(actual)));
            Assert.AreEqual(1, relationships.Count(actual => expectedRelationship2.Equals(actual)));
        }

        [TestMethod]
        public void GetRegistration_SingletonInstanceWithTransientDecoratorWithSingletonDecorator_ContainsExpectedRelationships()
        {
            // Arrange
            var expectedRelationship1 = new RelationshipInfo
            {
                ImplementationType = typeof(TransactionHandlerDecorator<RealCommand>),
                Lifestyle = Lifestyle.Singleton,
                Dependency = new DependencyInfo(typeof(ICommandHandler<RealCommand>), Lifestyle.Transient)
            };

            var expectedRelationship2 = new RelationshipInfo
            {
                ImplementationType = typeof(RealCommandHandlerDecorator),
                Lifestyle = Lifestyle.Transient,
                Dependency = new DependencyInfo(typeof(ICommandHandler<RealCommand>), Lifestyle.Singleton)
            };

            var container = new Container();

            container.RegisterSingle<ILogger, FakeLogger>();

            // StubCommandHandler has no dependencies.
            container.RegisterSingle<ICommandHandler<RealCommand>, StubCommandHandler>();

            // RealCommandHandlerDecorator only takes a dependency on ICommandHandler<RealCommand>
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // TransactionHandlerDecorator<T> only takes a dependency on ICommandHandler<T>
            container.RegisterSingleDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));

            container.Verify();

            // Act
            var relationships =
                container.GetRegistration(typeof(ICommandHandler<RealCommand>)).GetRelationships();

            // Assert
            Assert.AreEqual(1, relationships.Count(actual => expectedRelationship1.Equals(actual)));
        }
#endif

        [TestMethod]
        public void Lifestyle_TransientRegistrationDecoratedWithSingletonDecorator_GetsLifestyleOfDecorator()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(Lifestyle.Transient);

            container.RegisterSingleDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));

            var registration = container.GetRegistration(typeof(ICommandHandler<RealCommand>));

            // Act
            container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(Lifestyle.Singleton, registration.Lifestyle);
        }

        [TestMethod]
        public void Lifestyle_SingletonRegistrationDecoratedWithTransientDecorator_GetsLifestyleOfDecorator()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(Lifestyle.Singleton);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));

            var registration = container.GetRegistration(typeof(ICommandHandler<RealCommand>));

            // Act
            container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.AreEqual(Lifestyle.Transient, registration.Lifestyle);
        }

        [TestMethod]
        public void GetInstance_DecoratedInstance_DecoratorGoesThroughCompletePipeLineIncludingExpressionBuilding()
        {
            // Arrange
            var typesBuilding = new List<Type>();

            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(Lifestyle.Singleton);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));

            container.ExpressionBuilding += (s, e) =>
            {
                typesBuilding.Add(((NewExpression)e.Expression).Constructor.DeclaringType);
            };

            // Act
            container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsTrue(typesBuilding.Any(type => type == typeof(TransactionHandlerDecorator<RealCommand>)),
                "The decorator is expected to go through the complete pipeline, including ExpressionBuilding.");
        }

        [TestMethod]
        public void RegisterDecorator_DecoratorWithGenericTypeConstraintOtherThanTheClassConstraint_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            // Somehow the "where T : class" always works, while things like "where T : struct" or 
            // "where T : ISpecialCommand" (used here) doesn't.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(SpecialCommandHandlerDecorator<>));
        }

        [TestMethod]
        public void RegisterDecorator_DecoratorWithGenericTypeConstraint_WrapsTypesThatAdhereToTheConstraint()
        {
            // Arrange
            var container = new Container();

            // SpecialCommand implements ISpecialCommand
            container.Register<ICommandHandler<SpecialCommand>, NullCommandHandler<SpecialCommand>>();

            // SpecialCommandHandlerDecorator has a "where T : ISpecialCommand" constraint.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(SpecialCommandHandlerDecorator<>));

            // Act
            var specialHandler = container.GetInstance<ICommandHandler<SpecialCommand>>();

            // Assert
            Assert.IsInstanceOfType(specialHandler, typeof(SpecialCommandHandlerDecorator<SpecialCommand>));
        }

        [TestMethod]
        public void RegisterDecorator_DecoratorWithGenericTypeConstraint_DoesNotWrapTypesThatNotAdhereToTheConstraint()
        {
            // Arrange
            var container = new Container();

            // RealCommand does not implement ISpecialCommand
            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            // SpecialCommandHandlerDecorator has a "where T : ISpecialCommand" constraint.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(SpecialCommandHandlerDecorator<>));

            // Act
            var realHandler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsInstanceOfType(realHandler, typeof(NullCommandHandler<RealCommand>));
        }
    }

    public class DependencyInfo
    {
        public DependencyInfo(Type serviceType, Lifestyle lifestyle)
        {
            this.ServiceType = serviceType;
            this.Lifestyle = lifestyle;
        }

        public Type ServiceType { get; private set; }

        public Lifestyle Lifestyle { get; private set; }
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

    public class SpecialCommand : ISpecialCommand
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

    public class InvalidDecoratorCommandHandlerDecorator<T> : ICommandHandler<T>
    {
        // This is no decorator, since it lacks the ICommandHandler<T> parameter.
        public InvalidDecoratorCommandHandlerDecorator(ILogger logger)
        {
        }

        public void Handle(T command)
        {
        }
    }

    public class NullCommandHandler<T> : ICommandHandler<T>
    {
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
        public void Handle(RealCommand command)
        {
        }
    }

    public class LoggingRealCommandHandler : ICommandHandler<RealCommand>
    {
        private readonly ILogger logger;

        public LoggingRealCommandHandler(ILogger logger)
        {
            this.logger = logger;
        }

        public void Handle(RealCommand command)
        {
            this.logger.Log("RealCommand");
        }
    }

    public class RealCommandHandlerDecorator : ICommandHandler<RealCommand>
    {
        public RealCommandHandlerDecorator(ICommandHandler<RealCommand> decorated)
        {
            this.Decorated = decorated;
        }

        public ICommandHandler<RealCommand> Decorated { get; private set; }

        public void Handle(RealCommand command)
        {
        }
    }

    public class TransactionHandlerDecorator<T> : ICommandHandler<T>
    {
        public TransactionHandlerDecorator(ICommandHandler<T> decorated)
        {
            this.Decorated = decorated;
        }

        public ICommandHandler<T> Decorated { get; private set; }

        public void Handle(T command)
        {
        }
    }

    public class SpecialCommandHandlerDecorator<T> : ICommandHandler<T> where T : ISpecialCommand
    {
        public SpecialCommandHandlerDecorator(ICommandHandler<T> decorated)
        {
        }

        public void Handle(T command)
        {           
        }
    }

    public class LogExceptionCommandHandlerDecorator<T> : ICommandHandler<T>
    {
        private readonly ICommandHandler<T> decorated;

        public LogExceptionCommandHandlerDecorator(ICommandHandler<T> decorated)
        {
            this.decorated = decorated;
        }

        public void Handle(T command)
        {
            // called the decorated instance and log any exceptions (not important for these tests).
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

    public class AsyncCommandHandlerProxy<T> : ICommandHandler<T>
    {
        public AsyncCommandHandlerProxy(Container container, Func<ICommandHandler<T>> decorateeFactory)
        {
            this.DecorateeFactory = decorateeFactory;
        }

        public Func<ICommandHandler<T>> DecorateeFactory { get; private set; }

        public void Handle(T command)
        {
            // Run decorated instance on new thread (not important for these tests).
        }
    }

    public class LifetimeScopeCommandHandlerProxy<T> : ICommandHandler<T>
    {
        public LifetimeScopeCommandHandlerProxy(Func<ICommandHandler<T>> decorateeFactory,
            Container container)
        {
            this.DecorateeFactory = decorateeFactory;
        }

        public Func<ICommandHandler<T>> DecorateeFactory { get; private set; }

        public void Handle(T command)
        {
            // Start lifetime scope here (not important for these tests).
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

    // This is not a decorator, since the class implements ICommandHandler<int> but wraps ICommandHandler<T>
    public class BadCommandHandlerDecorator1 : ICommandHandler<int>
    {
        public BadCommandHandlerDecorator1(ICommandHandler<byte> handler)
        {
        }

        public void Handle(int command)
        {
        }
    }

    // This is not a decorator, since the class implements ICommandHandler<int> but wraps ICommandHandler<T>
    public class BadCommandHandlerDecorator2<T, TUnresolved> : ICommandHandler<T>
    {
        public BadCommandHandlerDecorator2(ICommandHandler<T> handler)
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

    public class NonGenericServiceDecoratorWithFunc : INonGenericService
    {
        public NonGenericServiceDecoratorWithFunc(Func<INonGenericService> decoratedCreator)
        {
            this.DecoratedServiceCreator = decoratedCreator;
        }

        public Func<INonGenericService> DecoratedServiceCreator { get; private set; }

        public void DoSomething()
        {
            this.DecoratedServiceCreator().DoSomething();
        }
    }

    public class NonGenericServiceDecoratorWithBothDecorateeAndFunc : INonGenericService
    {
        public NonGenericServiceDecoratorWithBothDecorateeAndFunc(INonGenericService decoratee,
            Func<INonGenericService> decoratedCreator)
        {
        }

        public void DoSomething()
        {
        }
    }

#if DEBUG
    internal class RelationshipInfo
    {
        public Type ImplementationType { get; set; }

        public Lifestyle Lifestyle { get; set; }

        public DependencyInfo Dependency { get; set; }

        public static bool EqualsTo(RelationshipInfo info, KnownRelationship other)
        {
            return
                info.ImplementationType == other.ImplementationType &&
                info.Lifestyle == other.Lifestyle &&
                info.Dependency.ServiceType == other.Dependency.ServiceType &&
                info.Dependency.Lifestyle == other.Dependency.Lifestyle;
        }

        public bool Equals(KnownRelationship other)
        {
            return EqualsTo(this, other);
        }
    }
#endif
}