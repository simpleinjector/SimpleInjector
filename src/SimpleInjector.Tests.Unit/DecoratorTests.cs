namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using Castle.DynamicProxy;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

    [TestClass]
    public class DecoratorTests
    {
        [TestMethod]
        public void GetInstance_OnRegisteredPartialGenericDecoratorType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<List<int>>, NullCommandHandler<List<int>>>();

            // ClassConstraintHandlerDecorator<List<T>>
            var partialOpenGenericDecoratorType =
                typeof(ClassConstraintHandlerDecorator<>).MakeGenericType(typeof(List<>));

            container.RegisterDecorator(typeof(ICommandHandler<>), partialOpenGenericDecoratorType);

            // Act
            var instance = container.GetInstance<ICommandHandler<List<int>>>();

            // Assert
            Assert.AreEqual(
                typeof(ClassConstraintHandlerDecorator<List<int>>).ToFriendlyName(),
                instance.GetType().ToFriendlyName(),
                "Decorator was not applied.");
        }

        [TestMethod]
        public void GetInstance_OnDecoratedNonGenericType_ReturnsTheDecoratedService()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<INonGenericService, RealNonGenericService>();
            container.RegisterDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator));

            // Act
            var service = container.GetInstance<INonGenericService>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(NonGenericServiceDecorator), service);

            var decorator = (NonGenericServiceDecorator)service;

            AssertThat.IsInstanceOfType(typeof(RealNonGenericService), decorator.DecoratedService);
        }

        [TestMethod]
        public void GetInstance_OnDecoratedNonGenericSingleton_ReturnsTheDecoratedService()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<INonGenericService, RealNonGenericService>(Lifestyle.Singleton);
            container.RegisterDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator));

            // Act
            var service = container.GetInstance<INonGenericService>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(NonGenericServiceDecorator), service);

            var decorator = (NonGenericServiceDecorator)service;

            AssertThat.IsInstanceOfType(typeof(RealNonGenericService), decorator.DecoratedService);
        }

        [TestMethod]
        public void GetInstance_SingleInstanceWrappedByATransientDecorator_ReturnsANewDecoratorEveryTime()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<INonGenericService, RealNonGenericService>(Lifestyle.Singleton);
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
            var container = ContainerFactory.New();

            // Register as transient
            container.Register<INonGenericService, RealNonGenericService>();
            container.RegisterDecorator<INonGenericService, NonGenericServiceDecorator>();

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
            var container = ContainerFactory.New();

            // Act
            Action action = () =>
                container.RegisterDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied decorator NonGenericServiceDecorator<T> is an open generic type definition",
                action);

            AssertThat.ThrowsWithParamName("decoratorType", action);
        }

        [TestMethod]
        public void GetInstance_OnNonGenericTypeDecoratedWithGenericDecorator_ReturnsTheDecoratedService()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<INonGenericService, RealNonGenericService>();
            container.RegisterDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator<int>));

            // Act
            var service = container.GetInstance<INonGenericService>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(NonGenericServiceDecorator<int>), service);

            var decorator = (NonGenericServiceDecorator<int>)service;

            AssertThat.IsInstanceOfType(typeof(RealNonGenericService), decorator.DecoratedService);
        }

        [TestMethod]
        public void GetInstance_OnDecoratedType_ReturnsTheDecorator()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = ContainerFactory.New();

            container.Register(typeof(ICommandHandler<RealCommand>), typeof(RealCommandHandler));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionHandlerDecorator<RealCommand>), handler);
        }

        [TestMethod]
        public void GetInstance_WithExplicitGenericImplementionRegisteredAsDecoratorThatMatchesTheRequestedService1_ReturnsTheDecorator()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<ICommandHandler<RealCommand>>(new RealCommandHandler());

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<RealCommand>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionHandlerDecorator<RealCommand>), handler);
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

            var container = ContainerFactory.New();

            container.RegisterSingleton<ICommandHandler<RealCommand>>(new RealCommandHandler());

            // Act
            Action action = () => container.RegisterDecorator(
                typeof(ICommandHandler<RealCommand>),
                typeof(TransactionHandlerDecorator<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(expectedMessage, action);
        }

        [TestMethod]
        public void GetInstance_WithExplicitGenericImplementionRegisteredAsDecoratorThatMatchesTheRequestedService2_ReturnsTheDecorator()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<ICommandHandler<RealCommand>>(new RealCommandHandler());

            container.RegisterDecorator(
                typeof(ICommandHandler<RealCommand>),
                typeof(TransactionHandlerDecorator<RealCommand>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionHandlerDecorator<RealCommand>), handler);
        }

        [TestMethod]
        public void GetInstance_WithExplicitGenericImplementionRegisteredAsDecoratorThatDoesNotMatchTheRequestedService1_ReturnsTheServiceItself()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<ICommandHandler<RealCommand>>(new RealCommandHandler());

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<int>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandler), handler);
        }

        [TestMethod]
        public void GetInstance_WithExplicitGenericImplementionRegisteredAsDecoratorThatDoesNotMatchTheRequestedService2_ReturnsTheServiceItself()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = ContainerFactory.New();

            container.RegisterSingleton<ICommandHandler<RealCommand>>(new RealCommandHandler());

            container.RegisterDecorator(typeof(ICommandHandler<int>), typeof(TransactionHandlerDecorator<int>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandler), handler);
        }

        [TestMethod]
        public void GetInstance_NonGenericDecoratorForMatchingClosedGenericServiceType_ReturnsTheNonGenericDecorator()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<ICommandHandler<RealCommand>>(new RealCommandHandler());

            Type closedGenericServiceType = typeof(ICommandHandler<RealCommand>);
            Type nonGenericDecorator = typeof(RealCommandHandlerDecorator);

            container.RegisterDecorator(closedGenericServiceType, nonGenericDecorator);

            // Act
            var decorator = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(nonGenericDecorator, decorator);
        }

        [TestMethod]
        public void GetInstance_NonGenericDecoratorForNonMatchingClosedGenericServiceType_ThrowsAnException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type nonMathcingClosedGenericServiceType = typeof(ICommandHandler<int>);

            // Decorator implements ICommandHandler<RealCommand>
            Type nonGenericDecorator = typeof(RealCommandHandlerDecorator);

            // Act
            Action action =
                () => container.RegisterDecorator(nonMathcingClosedGenericServiceType, nonGenericDecorator);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied type RealCommandHandlerDecorator does not implement ICommandHandler<Int32>",
                action);
        }

        [TestMethod]
        public void GetInstance_OnDecoratedType_GetsHandledAsExpected()
        {
            // Arrange
            var expectedTypeChain = new[] 
            { 
                typeof(LogExceptionCommandHandlerDecorator<RealCommand>),
                typeof(RealCommandHandler),
            };

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LogExceptionCommandHandlerDecorator<>));

            // Act
            var actualTypeChain = container.GetInstance<ICommandHandler<RealCommand>>().GetDecoratorTypeChain();

            // Assert
            AssertThat.SequenceEquals(expectedTypeChain, actualTypeChain);
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances_ReturnsLastRegisteredDecorator()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LogExceptionCommandHandlerDecorator<>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(LogExceptionCommandHandlerDecorator<RealCommand>), handler);
        }

        [TestMethod]
        public void GetInstance_OnTypeDecoratedByMultipleInstances_GetsHandledAsExpected()
        {
            // Arrange
            var expectedTypeChain = new[]
            {
                typeof(TransactionHandlerDecorator<RealCommand>),
                typeof(LogExceptionCommandHandlerDecorator<RealCommand>),
                typeof(RealCommandHandler),
            };

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LogExceptionCommandHandlerDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));

            // Act
            var actualTypeChain = container.GetInstance<ICommandHandler<RealCommand>>().GetDecoratorTypeChain();

            // Assert
            AssertThat.SequenceEquals(expectedTypeChain, actualTypeChain);
        }

        [TestMethod]
        public void GetInstance_WithInitializerOnDecorator_InitializesThatDecorator()
        {
            // Arrange
            int expectedItem1Value = 1;
            string expectedItem2Value = "some value";

            var container = ContainerFactory.New();

            container.RegisterInitializer<HandlerDecoratorWithPropertiesBase>(decorator =>
            {
                decorator.Item1 = expectedItem1Value;
            });

            container.RegisterInitializer<HandlerDecoratorWithPropertiesBase>(decorator =>
            {
                decorator.Item2 = expectedItem2Value;
            });

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

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
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

            // Decorator1Handler depends on ILogger, but ILogger is not registered.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LoggingHandlerDecorator1<>));

            // Act
            Action action = () => container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>("ILogger", action);
        }

        [TestMethod]
        public void GetInstance_DecoratorPredicateReturnsFalse_DoesNotDecorateInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>), c => false);

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(StubCommandHandler), handler);
        }

        [TestMethod]
        public void GetInstance_DecoratorPredicateReturnsTrue_DecoratesInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>),
                c => true);

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionHandlerDecorator<RealCommand>), handler);
        }

        [TestMethod]
        public void GetInstance_OnDecoratedType_CallsThePredicateWithTheExpectedServiceType()
        {
            // Arrange
            Type expectedPredicateServiceType = typeof(ICommandHandler<RealCommand>);
            Type actualPredicateServiceType = null;

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

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

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

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

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

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
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>), Lifestyle.Singleton);

            container.RegisterInitializer<AsyncCommandHandlerProxy<RealCommand>>(handler => { });

            // Act
            var handler1 = container.GetInstance<ICommandHandler<RealCommand>>();
            var handler2 = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(AsyncCommandHandlerProxy<RealCommand>), handler1);

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

            var container = ContainerFactory.New();

            container.RegisterSingleton<ICommandHandler<RealCommand>>(new StubCommandHandler());

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

            var container = ContainerFactory.New();

            // Because we register a Func<TServiceType> there is no way we can determine the implementation 
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

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(LogExceptionCommandHandlerDecorator<>), context =>
                {
                    actualPredicateImplementationType = context.ImplementationType;
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

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

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

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>), c =>
            {
                predicateExpressionOnFirstCall = c.Expression;
                return true;
            });

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(LogExceptionCommandHandlerDecorator<>), context =>
                {
                    predicateExpressionOnSecondCall = context.Expression;
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

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

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

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

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

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

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
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(ClassConstraintHandlerDecorator<>));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(ClassConstraintHandlerDecorator<RealCommand>), handler);
        }

        [TestMethod]
        public void GetInstance_DecoratorThatDoesNotSatisfyRequestedTypesTypeConstraints_DoesNotDecorateThatInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<StructCommand>, StructCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ClassConstraintHandlerDecorator<>));

            // Act
            var handler = container.GetInstance<ICommandHandler<StructCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(StructCommandHandler), handler);
        }

        [TestMethod]
        public void RegisterDecorator_DecoratorWithMultiplePublicConstructors_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(MultipleConstructorsCommandHandlerDecorator<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "it should have only one public constructor",
                action);
        }

        [TestMethod]
        public void RegisterDecorator_SupplyingTypeThatIsNotADecorator_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(InvalidDecoratorCommandHandlerDecorator<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(@"
                For the container to be able to use InvalidDecoratorCommandHandlerDecorator<T> as  
                a decorator, its constructor must include a single parameter of type 
                ICommandHandler<T>".TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterDecorator_SupplyingTypeThatIsNotADecorator_ThrowsException2()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(InvalidDecoratorCommandHandlerDecorator<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "does not currently exist in the constructor",
                action);
        }

        [TestMethod]
        public void RegisterDecorator_SupplyingTypeThatIsNotADecorator_ThrowsException3()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(InvalidCommandHandlerDecoratorWithTwoDecoratees<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "is defined multiple times in the constructor",
                action);
        }

        [TestMethod]
        public void RegisterDecorator_SupplyingAnUnrelatedType_FailsWithExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action =
                () => container.RegisterDecorator(typeof(ICommandHandler<>), typeof(KeyValuePair<,>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(@"
                    The supplied type KeyValuePair<TKey, TValue> does not implement 
                    ICommandHandler<TCommand>.
                    ".TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterDecorator_SupplyingAConcreteNonGenericType_ShouldSucceed()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorSupplyingAConcreteNonGenericType_ReturnsExpectedDecorator1()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandlerDecorator), handler);
        }

        [TestMethod]
        public void RegisterDecorator_SupplyingTypeThatIsNotADecorator_ThrowsException4()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(InvalidCommandHandlerDecoratorWithDecorateeAndFactory<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "is defined multiple times in the constructor",
                action);
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorSupplyingAConcreteNonGenericTypeThatDoesNotMatch_DoesNotReturnThatDecorator()
        {
            // Arrange
            var container = ContainerFactory.New();

            // StructCommandHandler implements ICommandHandler<StructCommand>
            container.Register<ICommandHandler<StructCommand>, StructCommandHandler>();

            // ConcreteCommandHandlerDecorator implements ICommandHandler<RealCommand>
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handler = container.GetInstance<ICommandHandler<StructCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(StructCommandHandler), handler);
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorSupplyingAConcreteNonGenericType_ReturnsExpectedDecorator2()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();
            container.Register<ICommandHandler<StructCommand>, StructCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandlerDecorator), handler);
        }

        [TestMethod]
        public void RegisterDecorator_NonGenericDecoratorWithFuncAsConstructorArgument_InjectsAFactoryThatCreatesNewInstancesOfTheDecoratedType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<INonGenericService, RealNonGenericService>();

            container.RegisterDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecoratorWithFunc));

            var decorator = (NonGenericServiceDecoratorWithFunc)container.GetInstance<INonGenericService>();

            Func<INonGenericService> factory = decorator.DecoratedServiceCreator;

            // Act
            // Execute the factory twice.
            INonGenericService instance1 = factory();
            INonGenericService instance2 = factory();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealNonGenericService), instance1, "The injected factory is expected to create instances of type RealNonGenericService.");

            Assert.IsFalse(object.ReferenceEquals(instance1, instance2),
                "The factory is expected to create transient instances, since that is how " +
                "RealNonGenericService is registered.");
        }

        [TestMethod]
        public void RegisterDecorator_GenericDecoratorWithFuncAsConstructorArgument_InjectsAFactoryThatCreatesNewInstancesOfTheDecoratedType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<ILogger>(new FakeLogger());

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
            AssertThat.IsInstanceOfType(typeof(LogExceptionCommandHandlerDecorator<RealCommand>), instance1, "The injected factory is expected to create instances of type " +
                "LogAndContinueCommandHandlerDecorator<RealCommand>.");

            Assert.IsFalse(object.ReferenceEquals(instance1, instance2),
                "The factory is expected to create transient instances.");
        }

        [TestMethod]
        public void RegisterDecorator_CalledWithDecoratorTypeWithBothAFuncAndADecorateeParameter_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterDecorator(typeof(INonGenericService),
                typeof(NonGenericServiceDecoratorWithBothDecorateeAndFunc));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "single parameter of type INonGenericService (or Func<INonGenericService>)",
                action);
        }

        [TestMethod]
        public void RegisterDecorator_RegisteringAClassThatWrapsADifferentClosedTypeThanItImplements_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            // BadCommandHandlerDecorator1<T> implements ICommandHandler<int> but wraps ICommandHandler<byte>
            Action action = () => container.RegisterDecorator(typeof(ICommandHandler<>),
                  typeof(BadCommandHandlerDecorator1));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(@"
                must include a single parameter of type ICommandHandler<Int32> (or Func<ICommandHandler<Int32>>)"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterDecorator_RegisteringADecoratorWithAnUnresolvableTypeArgument_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            // CommandHandlerDecoratorWithUnresolvableArgument<T, TUnresolved> contains a not-mappable 
            // type argument TUnresolved.
            Action action = () => container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(CommandHandlerDecoratorWithUnresolvableArgument<,>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "contains unresolvable type arguments.",
                action);

            AssertThat.ThrowsWithParamName("decoratorType", action);
        }

        [TestMethod]
        public void GetInstance_TypeRegisteredWithRegisterSingleDecorator_AlwaysReturnsTheSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<INonGenericService, RealNonGenericService>(Lifestyle.Singleton);

            container.RegisterDecorator<INonGenericService, NonGenericServiceDecorator>(Lifestyle.Singleton);

            // Act
            var decorator1 = container.GetInstance<INonGenericService>();
            var decorator2 = container.GetInstance<INonGenericService>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(NonGenericServiceDecorator), decorator1);

            Assert.AreSame(decorator1, decorator2,
                "Since the decorator is registered as singleton, GetInstance should always return the same " +
                "instance.");
        }

        [TestMethod]
        public void GetInstance_TypeRegisteredWithRegisterSingleDecoratorPredicate_AlwaysReturnsTheSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<INonGenericService, RealNonGenericService>(Lifestyle.Singleton);

            container.RegisterDecorator(typeof(INonGenericService), typeof(NonGenericServiceDecorator),
                Lifestyle.Singleton,
                c => true);

            // Act
            var decorator1 = container.GetInstance<INonGenericService>();
            var decorator2 = container.GetInstance<INonGenericService>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(NonGenericServiceDecorator), decorator1);

            Assert.IsTrue(object.ReferenceEquals(decorator1, decorator2),
                "Since the decorator is registered as singleton, GetInstance should always return the same " +
                "instance.");
        }

        [TestMethod]
        public void Verify_DecoratorRegisteredThatCanNotBeResolved_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();

            // LoggingHandlerDecorator1 depends on ILogger, which is not registered.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LoggingHandlerDecorator1<>));

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                The constructor of type LoggingHandlerDecorator1<RealCommand> 
                contains the parameter with name 'logger' and type ILogger that is 
                not registered.".TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_DecoratorRegisteredTwiceAsSingleton_WrapsTheDecorateeTwice()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(Lifestyle.Singleton);

            // Register the same decorator twice. 
            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionHandlerDecorator<>),
                Lifestyle.Singleton);

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionHandlerDecorator<>),
                Lifestyle.Singleton);

            // Act
            var decorator1 = (TransactionHandlerDecorator<RealCommand>)
                container.GetInstance<ICommandHandler<RealCommand>>();

            var decorator2 = decorator1.Decorated;

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionHandlerDecorator<RealCommand>), decorator2, 
                "Since the decorator is registered twice, it should wrap the decoratee twice.");

            var decoratee = ((TransactionHandlerDecorator<RealCommand>)decorator2).Decorated;

            AssertThat.IsInstanceOfType(typeof(StubCommandHandler), decoratee);
        }

        [TestMethod]
        public void HybridLifestyleRegistration_WithDecorator_DecoratesTheInstance()
        {
            // Arrange
            var hybrid = Lifestyle.CreateHybrid(() => true, Lifestyle.Transient, Lifestyle.Singleton);

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(hybrid);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandlerDecorator), handler);
        }

        [TestMethod]
        public void HybridLifestyleRegistration_WithTransientDecorator_AppliesTransientDecorator()
        {
            // Arrange
            var hybrid = Lifestyle.CreateHybrid(() => false, Lifestyle.Singleton, Lifestyle.Singleton);

            var container = ContainerFactory.New();

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
            var hybrid = Lifestyle.CreateHybrid(() => false, Lifestyle.Singleton, Lifestyle.Singleton);

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(hybrid);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handler = (RealCommandHandlerDecorator)container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(StubCommandHandler), handler.Decorated);
        }

        [TestMethod]
        public void HybridLifestyleRegistration_WithTransientDecorator_LeavesTheLifestyleInTact1()
        {
            // Arrange
            var hybrid = Lifestyle.CreateHybrid(() => false, Lifestyle.Singleton, Lifestyle.Singleton);

            var container = ContainerFactory.New();

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
            var hybrid = Lifestyle.CreateHybrid(() => false, Lifestyle.Transient, Lifestyle.Transient);

            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(hybrid);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var handler1 = (RealCommandHandlerDecorator)container.GetInstance<ICommandHandler<RealCommand>>();
            var handler2 = (RealCommandHandlerDecorator)container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(handler1.Decorated, handler2.Decorated),
                "The wrapped instance should have the expected lifestyle (transient in this case).");
        }

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

            var container = ContainerFactory.New();

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
            var hybrid = Lifestyle.CreateHybrid(() => true, Lifestyle.Transient, Lifestyle.Singleton);

            var expectedRelationships = new[]
            {   
                new RelationshipInfo
                {
                    Lifestyle = hybrid,
                    ImplementationType = typeof(RealCommandHandlerDecorator),
                    Dependency = new DependencyInfo(typeof(ICommandHandler<RealCommand>), Lifestyle.Transient)
                },
                new RelationshipInfo
                {
                    Lifestyle = Lifestyle.Singleton,
                    ImplementationType = typeof(RealCommandHandlerDecorator),
                    Dependency = new DependencyInfo(typeof(ICommandHandler<RealCommand>), hybrid)
                },
            };

            var container = ContainerFactory.New();
            container.Options.SuppressLifestyleMismatchVerification = true;

            // StubCommandHandler has no dependencies.
            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(
                Lifestyle.Transient);

            // RealCommandHandlerDecorator only has ICommandHandler<RealCommand> as dependency.
            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandHandlerDecorator),
                hybrid);

            // RealCommandHandlerDecorator only has ICommandHandler<RealCommand> as dependency.
            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(RealCommandHandlerDecorator),
                Lifestyle.Singleton);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var actualRelationships = container.GetRegistration(typeof(ICommandHandler<RealCommand>))
                .GetRelationships()
                .ToArray();

            // Assert
            Assert.IsTrue(
                actualRelationships.All(a => expectedRelationships.Any(e => e.Equals(a))),
                "actual: " + Environment.NewLine +
                string.Join(Environment.NewLine, actualRelationships.Select(r => RelationshipInfo.ToString(r))));
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

            var container = ContainerFactory.New();

            container.Register<ILogger, FakeLogger>(Lifestyle.Singleton);

            // StubCommandHandler has no dependencies.
            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();

            // LoggingHandlerDecorator1 takes a dependency on ILogger.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(LoggingHandlerDecorator1<>));

            container.Verify(VerificationOption.VerifyOnly);

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

            var container = ContainerFactory.New();
            container.Options.SuppressLifestyleMismatchVerification = true;

            container.Register<ILogger, FakeLogger>(Lifestyle.Singleton);

            // StubCommandHandler has no dependencies.
            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(Lifestyle.Singleton);

            // RealCommandHandlerDecorator only takes a dependency on ICommandHandler<RealCommand>
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // TransactionHandlerDecorator<T> only takes a dependency on ICommandHandler<T>
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>),
                Lifestyle.Singleton);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var relationships =
                container.GetRegistration(typeof(ICommandHandler<RealCommand>)).GetRelationships();

            // Assert
            Assert.AreEqual(1, relationships.Count(actual => expectedRelationship1.Equals(actual)));
        }

        // This test was written for work item https://simpleinjector.codeplex.com/workitem/20141.
        [TestMethod]
        public void GetRelationships_DecoratorDependingOnFuncDecorateeFactory_ReturnsRelationshipForThatFactory()
        {
            // Arrange
            var expectedRelationship = new RelationshipInfo
            {
                Lifestyle = Lifestyle.Singleton,
                ImplementationType = typeof(NonGenericServiceDecoratorWithFunc),
                Dependency = new DependencyInfo(typeof(Func<INonGenericService>), Lifestyle.Singleton)
            };

            var container = new Container();

            container.Register<INonGenericService, RealNonGenericService>(Lifestyle.Transient);

            container.RegisterDecorator(typeof(INonGenericService),
                typeof(NonGenericServiceDecoratorWithFunc), Lifestyle.Singleton);

            container.Verify();

            // Act
            var relationships = container.GetRegistration(typeof(INonGenericService)).GetRelationships();

            // Assert
            var actualRelationship = relationships.Single();

            Assert.IsTrue(expectedRelationship.Equals(actualRelationship),
                "actual: " + RelationshipInfo.ToString(actualRelationship));
        }

        [TestMethod]
        public void GetRelationships_DecoratorDependingOnTransientFuncDecorateeFactory_ReturnsRelationshipForThatFactory()
        {
            // Arrange
            var expectedRelationship = new RelationshipInfo
            {
                Lifestyle = Lifestyle.Transient,
                ImplementationType = typeof(NonGenericServiceDecoratorWithFunc),
                Dependency = new DependencyInfo(typeof(Func<INonGenericService>), Lifestyle.Singleton)
            };

            var container = new Container();

            container.Register<INonGenericService, RealNonGenericService>(Lifestyle.Transient);

            // Here we register the decorator as transient!
            container.RegisterDecorator(typeof(INonGenericService),
                typeof(NonGenericServiceDecoratorWithFunc), Lifestyle.Transient);

            container.Verify();

            // Act
            var relationships = container.GetRegistration(typeof(INonGenericService)).GetRelationships();

            // Assert
            var actualRelationship = relationships.Single();

            Assert.IsTrue(expectedRelationship.Equals(actualRelationship),
                "actual: " + RelationshipInfo.ToString(actualRelationship));
        }

        [TestMethod]
        public void Lifestyle_TransientRegistrationDecoratedWithSingletonDecorator_GetsLifestyleOfDecorator()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.SuppressLifestyleMismatchVerification = true;

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(Lifestyle.Transient);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>),
                Lifestyle.Singleton);

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
            var container = ContainerFactory.New();

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

            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

            // Act
            // Somehow the "where T : class" always works, while things like "where T : struct" or 
            // "where T : ISpecialCommand" (used here) doesn't.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(SpecialCommandHandlerDecorator<>));
        }

        [TestMethod]
        public void RegisterDecorator_DecoratorWithGenericTypeConstraint_WrapsTypesThatAdhereToTheConstraint()
        {
            // Arrange
            var container = ContainerFactory.New();

            // SpecialCommand implements ISpecialCommand
            container.Register<ICommandHandler<SpecialCommand>, NullCommandHandler<SpecialCommand>>();

            // SpecialCommandHandlerDecorator has a "where T : ISpecialCommand" constraint.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(SpecialCommandHandlerDecorator<>));

            // Act
            var specialHandler = container.GetInstance<ICommandHandler<SpecialCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(SpecialCommandHandlerDecorator<SpecialCommand>), specialHandler);
        }

        [TestMethod]
        public void RegisterDecorator_DecoratorWithGenericTypeConstraint_DoesNotWrapTypesThatNotAdhereToTheConstraint()
        {
            // Arrange
            var container = ContainerFactory.New();

            // RealCommand does not implement ISpecialCommand
            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            // SpecialCommandHandlerDecorator has a "where T : ISpecialCommand" constraint.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(SpecialCommandHandlerDecorator<>));

            // Act
            var realHandler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(NullCommandHandler<RealCommand>), realHandler);
        }

        [TestMethod]
        public void GetAllInstances_RegisteringADecoratorThatWrapsTheWholeCollection_WorksAsExpected()
        {
            // Arrange
            var container = new Container();

            container.RegisterCollection<ICommandHandler<RealCommand>>(new[] 
            {
                typeof(NullCommandHandler<RealCommand>),
                typeof(StubCommandHandler)
            });

            // EnumerableDecorator<T> decorated IEnumerable<T>
            container.RegisterDecorator(
                typeof(IEnumerable<ICommandHandler<RealCommand>>),
                typeof(EnumerableDecorator<ICommandHandler<RealCommand>>),
                Lifestyle.Singleton);

            // Act
            var collection = container.GetAllInstances<ICommandHandler<RealCommand>>();

            // Assert
            // Wrapping the collection itself instead of the individual elements allows you to apply a filter
            // to the elements, perhaps based on the user's role. I must admit that this is a quite bizarre
            // scenario, but it is currently supported (perhaps even by accident), so we need to have a test
            // to ensure it keeps being supported in the future.
            AssertThat.IsInstanceOfType(typeof(EnumerableDecorator<ICommandHandler<RealCommand>>), collection);
        }

        [TestMethod]
        public void GetRelationships_AddingRelationshipDuringBuildingOnDecoratorType_ContainsAddedRelationship()
        {
            // Arrange
            var container = ContainerFactory.New();

            var expectedRelationship = GetValidRelationship();

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            container.ExpressionBuilding += (s, e) =>
            {
                if (e.KnownImplementationType == typeof(RealCommandHandlerDecorator))
                {
                    e.KnownRelationships.Add(expectedRelationship);
                }
            };

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var relationships =
                container.GetRegistration(typeof(ICommandHandler<RealCommand>)).GetRelationships();

            // Assert
            Assert.IsTrue(relationships.Contains(expectedRelationship),
                "Any known relationships added to the decorator during the ExpressionBuilding event " +
                "should be added to the registration of the service type.");
        }

        // This is a regression test. This test failed on Simple Injector 2.0 to 2.2.3.
        [TestMethod]
        public void RegisterDecorator_AppliedToMultipleInstanceProducersWithTheSameServiceType_CallsThePredicateForEachImplementationType()
        {
            // Arrange
            Type serviceType = typeof(IPlugin);

            var implementationTypes = new List<Type>();

            var container = ContainerFactory.New();

            var prod1 = new InstanceProducer(serviceType,
                Lifestyle.Transient.CreateRegistration(typeof(PluginImpl), container));

            var prod2 = new InstanceProducer(serviceType,
                Lifestyle.Transient.CreateRegistration(typeof(PluginImpl2), container));

            container.RegisterDecorator(serviceType, typeof(PluginDecorator), context =>
            {
                implementationTypes.Add(context.ImplementationType);
                return true;
            });

            // Act
            var instance1 = prod1.GetInstance();
            var instance2 = prod2.GetInstance();

            // Assert
            string message = "The predicate was expected to be called with a context containing the " +
                "implementation type: ";

            Assert.AreEqual(2, implementationTypes.Count, "Predicate was expected to be called twice.");
            Assert.IsTrue(implementationTypes.Any(type => type == typeof(PluginImpl)),
                message + typeof(PluginImpl).Name);
            Assert.IsTrue(implementationTypes.Any(type => type == typeof(PluginImpl2)),
                message + typeof(PluginImpl2).Name);
        }

        [TestMethod]
        public void Verify_WithProxyDecoratorWrappingAnInvalidRegistration_ShouldFailWithExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>>(() =>
            {
                throw new Exception("Failure.");
            });

            // AsyncCommandHandlerProxy<T> depends on Func<ICommandHandler<T>>.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>));

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                The configuration is invalid. 
                Creating the instance for type ICommandHandler<RealCommand> failed.
                Failure."
                .TrimInside(),
                action,
                "Verification should fail because the Func<ICommandHandler<T>> is invalid.");
        }

        [TestMethod]
        public void GetInstance_DecoratorWithNestedGenericType_GetsAppliedCorrectly()
        {
            // Arrange
            var container = new Container();

            container.Register(
                typeof(IQueryHandler<CacheableQuery, ReadOnlyCollection<DayOfWeek>>),
                typeof(CacheableQueryHandler));

            container.Register(
                typeof(IQueryHandler<NonCacheableQuery, DayOfWeek[]>),
                typeof(NonCacheableQueryHandler));

            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(CacheableQueryHandlerDecorator<,>));

            // Act
            var handler1 = container.GetInstance<IQueryHandler<CacheableQuery, ReadOnlyCollection<DayOfWeek>>>();
            var handler2 = container.GetInstance<IQueryHandler<NonCacheableQuery, DayOfWeek[]>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(CacheableQueryHandlerDecorator<CacheableQuery, DayOfWeek>), handler1);
            AssertThat.IsInstanceOfType(typeof(NonCacheableQueryHandler), handler2);
        }

        [TestMethod]
        public void RegisterDecoratorWithFactory_AllValidParameters_Succeeds()
        {
            // Arrange
            var container = new Container();

            var validParameters = RegisterDecoratorFactoryParameters.CreateValid();

            // Act
            container.RegisterDecorator(validParameters);
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorWithDecoratorReturningOpenGenericType_WrapsTheServiceWithTheClosedDecorator()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(ICommandHandler<RealCommand>), typeof(RealCommandHandler));

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(ICommandHandler<>);
            parameters.DecoratorTypeFactory = context => typeof(TransactionHandlerDecorator<>);

            container.RegisterDecorator(parameters);

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionHandlerDecorator<RealCommand>), handler);
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorWithPredicateReturningFalse_DoesNotWrapTheServiceWithTheDecorator()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(ICommandHandler<RealCommand>), typeof(RealCommandHandler));

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.Predicate = context => false;
            parameters.ServiceType = typeof(ICommandHandler<>);
            parameters.DecoratorTypeFactory = context => typeof(TransactionHandlerDecorator<>);

            container.RegisterDecorator(parameters);

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(RealCommandHandler), handler);
        }

        [TestMethod]
        public void GetInstance_OnDifferentServiceTypeThanRegisteredDecorator_DoesNotCallSuppliedPredicate()
        {
            // Arrange
            bool predicateCalled = false;
            bool decoratorTypeFactoryCalled = false;

            var container = new Container();

            container.Register(typeof(ICommandHandler<RealCommand>), typeof(RealCommandHandler));

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(ICommandHandler<>);

            parameters.Predicate = context =>
            {
                predicateCalled = true;
                return false;
            };

            parameters.DecoratorTypeFactory = context =>
            {
                decoratorTypeFactoryCalled = true;
                return typeof(TransactionHandlerDecorator<>);
            };

            container.RegisterDecorator(parameters);

            // Act
            // Resolve some other type
            try
            {
                container.GetInstance<INonGenericService>();
            }
            catch
            {
                // This will fail since INonGenericService is not registered.
            }

            // Assert
            Assert.IsFalse(predicateCalled, "The predicate should not be called when a type is resolved " +
                "that doesn't match the given service type (ICommandHandler<TCommand> in this case).");
            Assert.IsFalse(decoratorTypeFactoryCalled, "The factory should not be called.");
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorWithPredicateReturningFalse_DoesNotCallTheFactory()
        {
            // Arrange
            bool decoratorTypeFactoryCalled = false;

            var container = new Container();

            container.Register(typeof(ICommandHandler<RealCommand>), typeof(RealCommandHandler));

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.Predicate = context => false;
            parameters.ServiceType = typeof(ICommandHandler<>);

            parameters.DecoratorTypeFactory = context =>
            {
                decoratorTypeFactoryCalled = true;
                return typeof(TransactionHandlerDecorator<>);
            };

            container.RegisterDecorator(parameters);

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            Assert.IsFalse(decoratorTypeFactoryCalled, @"
                The factory should not be called if the predicate returns false. This prevents the user from 
                having to do specific handling when the decorator type can't be constructed because of generic 
                type constraints.");
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorWithFactoryReturningTypeBasedOnImplementationType_WrapsTheServiceWithTheExpectedDecorator()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(INonGenericService), typeof(RealNonGenericService));

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(INonGenericService);
            parameters.DecoratorTypeFactory =
                context => typeof(NonGenericServiceDecorator<>).MakeGenericType(context.ImplementationType);

            container.RegisterDecorator(parameters);

            // Act
            var service = container.GetInstance<INonGenericService>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(NonGenericServiceDecorator<RealNonGenericService>), service);
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorReturningAnOpenGenericType_AppliesThatTypeOnlyWhenTypeConstraintsAreMet()
        {
            // Arrange
            var container = new Container();

            // SpecialCommand implements ISpecialCommand, but RealCommand does not.
            container.Register<ICommandHandler<SpecialCommand>, NullCommandHandler<SpecialCommand>>();
            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(ICommandHandler<>);

            // SpecialCommandHandlerDecorator has a "where T : ISpecialCommand" constraint.
            parameters.DecoratorTypeFactory = context => typeof(SpecialCommandHandlerDecorator<>);

            container.RegisterDecorator(parameters);

            // Act
            var handler1 = container.GetInstance<ICommandHandler<SpecialCommand>>();
            var handler2 = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(SpecialCommandHandlerDecorator<SpecialCommand>), handler1);
            AssertThat.IsInstanceOfType(typeof(NullCommandHandler<RealCommand>), handler2);
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorWithFactoryReturningAPartialOpenGenericType_WorksLikeACharm()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(ICommandHandler<>);

            // Here we make a partial open-generic type by filling in the TUnresolved.
            parameters.DecoratorTypeFactory = context =>
                typeof(CommandHandlerDecoratorWithUnresolvableArgument<,>)
                    .MakePartialOpenGenericType(
                        secondArgument: context.ImplementationType);

            container.RegisterDecorator(parameters);

            // Act
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(CommandHandlerDecoratorWithUnresolvableArgument<RealCommand, RealCommandHandler>), handler);
        }

        [TestMethod]
        public void GetInstance_WithClosedGenericServiceAndOpenGenericDecoratorReturnedByFactory_ReturnsDecoratedFactory()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(ICommandHandler<RealCommand>), typeof(RealCommandHandler));

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(ICommandHandler<RealCommand>);
            parameters.DecoratorTypeFactory = context => typeof(TransactionHandlerDecorator<>);

            container.RegisterDecorator(parameters);

            // Act
            // Registering an closed generic service with an open generic decorator isn't supported by the
            // 'normal' RegisterDecorator methods. This is a limitation in the underlying system. The system
            // can't easily verify whether the open-generic decorator is assignable from the closed-generic
            // service.
            // The factory-supplying version doesn't have this limitation, since the factory is only called
            // at resolve-time, which means there are no open-generic types to check. Everything is closed.
            // So long story short: the following call will (or should) succeed.
            var handler = container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(TransactionHandlerDecorator<RealCommand>), handler);
        }

        [TestMethod]
        public void GetInstance_WithClosedGenericServiceAndFactoryReturningIncompatibleClosedImplementation_FailsWithExpectedException()
        {
            // Arrange
            string expectedMessage = @"
                The registered type factory returned type TransactionHandlerDecorator<Int32> which
                does not implement ICommandHandler<RealCommand>"
                .TrimInside();

            var container = new Container();

            container.Register(typeof(ICommandHandler<RealCommand>), typeof(RealCommandHandler));

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(ICommandHandler<RealCommand>);
            parameters.DecoratorTypeFactory = context => typeof(TransactionHandlerDecorator<int>);

            // Since the creation of the decorator type is delayed, the call to RegisterDecorator can't
            // throw an exception.
            container.RegisterDecorator(parameters);

            // Act
            Action action = () => container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                expectedMessage, action);
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorWithFactoryReturningTypeWithMultiplePublicConstructors_ThrowsExceptedException()
        {
            // Arrange
            string expectedMessage = "it should have only one public constructor";

            var container = new Container();

            container.Register(typeof(ICommandHandler<RealCommand>), typeof(RealCommandHandler));

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(ICommandHandler<RealCommand>);
            parameters.DecoratorTypeFactory = context => typeof(MultipleConstructorsCommandHandlerDecorator<>);

            // Since the creation of the decorator type is delayed, the call to RegisterDecorator can't
            // throw an exception.
            container.RegisterDecorator(parameters);

            // Act
            Action action = () => container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(expectedMessage, action);
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorWithNonGenericServiceAndFactoryReturningAnOpenGenericDecoratorType_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage = @"The registered decorator type factory returned open generic type 
                NonGenericServiceDecorator<T> while the registered service type INonGenericService is not 
                generic, making it impossible for a closed generic decorator type to be constructed"
                .TrimInside();

            var container = new Container();

            container.Register(typeof(INonGenericService), typeof(RealNonGenericService));

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(INonGenericService);
            parameters.DecoratorTypeFactory = context => typeof(NonGenericServiceDecorator<>);

            // Since the creation of the decorator type is delayed, the call to RegisterDecorator can't
            // throw an exception.
            container.RegisterDecorator(parameters);

            // Act
            Action action = () => container.GetInstance<INonGenericService>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(expectedMessage, action);
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorWithFactoryReturningTypeThatIsNotADecorator_ThrowsExceptedException()
        {
            // Arrange
            string expectedMessage = @"
                For the container to be able to use InvalidDecoratorCommandHandlerDecorator<RealCommand> as  
                a decorator, its constructor must include a single parameter of type 
                ICommandHandler<RealCommand> (or Func<ICommandHandler<RealCommand>>)"
                .TrimInside();

            var container = new Container();

            container.Register(typeof(ICommandHandler<RealCommand>), typeof(RealCommandHandler));

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(ICommandHandler<RealCommand>);
            parameters.DecoratorTypeFactory = context => typeof(InvalidDecoratorCommandHandlerDecorator<>);

            // Since the creation of the decorator type is delayed, the call to RegisterDecorator can't
            // throw an exception.
            container.RegisterDecorator(parameters);

            // Act
            Action action = () => container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(expectedMessage, action);
        }

        [TestMethod]
        public void GetInstance_RegisterDecoratorWithFactoryReturningTypeWithUnresolvableArgument_ThrowsExceptedException()
        {
            // Arrange
            string expectedMessage =
                typeof(CommandHandlerDecoratorWithUnresolvableArgument<,>).ToFriendlyName() +
                " contains unresolvable type arguments.";

            var container = new Container();

            container.Register(typeof(ICommandHandler<RealCommand>), typeof(RealCommandHandler));

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.ServiceType = typeof(ICommandHandler<RealCommand>);

            // CommandHandlerDecoratorWithUnresolvableArgument<T, TUnresolved> contains an unmappable 
            // type argument TUnresolved.
            parameters.DecoratorTypeFactory =
                context => typeof(CommandHandlerDecoratorWithUnresolvableArgument<,>);

            // Since the creation of the decorator type is delayed, the call to RegisterDecorator can't
            // throw an exception.
            container.RegisterDecorator(parameters);

            // Act
            Action action = () => container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(expectedMessage, action);
        }

        [TestMethod]
        public void RegisterDecoratorWithFactory_InvalidDecoratorTypeFactory_ThrowsArgumentNullException()
        {
            // Arrange
            var container = new Container();

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.DecoratorTypeFactory = null;

            // Act
            Action action = () => container.RegisterDecorator(parameters);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("decoratorTypeFactory", action);
        }

        [TestMethod]
        public void RegisterDecoratorWithFactory_FactoryThatReturnsNull_ThrowsExpectedExceptionWhenResolving()
        {
            // Arrange
            string expectedExceptionMessage =
                "The decorator type factory delegate that was registered for service type " +
                "ICommandHandler<RealCommand> returned null.";

            var container = new Container();

            container.Register(typeof(ICommandHandler<RealCommand>), typeof(RealCommandHandler));

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.DecoratorTypeFactory = context => null;

            parameters.ServiceType = typeof(ICommandHandler<RealCommand>);

            container.RegisterDecorator(parameters);

            // Act
            Action action = () => container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(expectedExceptionMessage, action);
        }

        [TestMethod]
        public void RegisterDecoratorWithFactory_InvalidPredicate_ThrowsArgumentNullException()
        {
            // Arrange
            var container = new Container();

            var parameters = RegisterDecoratorFactoryParameters.CreateValid();

            parameters.Predicate = null;

            // Act
            Action action = () => container.RegisterDecorator(parameters);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("predicate", action);
        }

        [TestMethod]
        public void GetInstance_DecoratorDependingOnDecoratorPredicateContext_ContainsTheExpectedContext()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionHandlerDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ContextualHandlerDecorator<>));

            // Act
            var decorator =
                (ContextualHandlerDecorator<RealCommand>)container.GetInstance<ICommandHandler<RealCommand>>();

            DecoratorContext context = decorator.Context;

            // Assert
            Assert.AreSame(typeof(RealCommandHandler), context.ImplementationType);
            Assert.AreSame(typeof(TransactionHandlerDecorator<RealCommand>), context.AppliedDecorators.Single());
        }

        [TestMethod]
        public void GetInstance_DecoratorWithDecorateeAndDependencyOfTheSameOpenGenericType_ResolveCorrectly()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();
            container.Register<ICommandHandler<SpecialCommand>, NullCommandHandler<SpecialCommand>>();

            // Create: CommandHandlerDecoratorWithDependency<T, ICommandHandler<SpecialCommand>>
            Type decoratorType =
                typeof(CommandHandlerDecoratorWithDependency<,>).MakePartialOpenGenericType(
                    secondArgument: typeof(ICommandHandler<SpecialCommand>));

            // We need to prevent the nested command handler from being decorated.
            container.RegisterDecorator(typeof(ICommandHandler<>), decoratorType,
                context => context.ServiceType != typeof(ICommandHandler<SpecialCommand>));

            // Act
            // Here we resolve the following dependency chain:
            // new CommandHandlerDecoratorWithDependency<RealCommand, ICommandHandler<SpecialCommand>>(
            //     new NullCommandHandler<SpecialCommand>(), // the dependency
            //     new RealCommandHandler()) // the decoratee
            var decorator = container.GetInstance<ICommandHandler<RealCommand>>()
                as CommandHandlerDecoratorWithDependency<RealCommand, ICommandHandler<SpecialCommand>>;

            // Assert
            Assert.IsNotNull(decorator.Dependency);
        }

        [TestMethod]
        public void GetInstance_DecoratorWithDecorateeAndDependencyOfTheSameOpenGenericTypeCausingACyclicDependency_ThrowExpectedException1()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();
            container.Register<ICommandHandler<SpecialCommand>, NullCommandHandler<SpecialCommand>>();

            // Create: CommandHandlerDecoratorWithDependency<T, ICommandHandler<SpecialCommand>>
            Type decoratorType =
                typeof(CommandHandlerDecoratorWithDependency<,>).MakePartialOpenGenericType(
                    secondArgument: typeof(ICommandHandler<SpecialCommand>));

            // Here we allow the nested command handler to be decorated. And this should fail.
            container.RegisterDecorator(typeof(ICommandHandler<>), decoratorType, context => true);

            // Act
            // Here we try to resolve the following dependency chain:
            // new CommandHandlerDecoratorWithDependency<RealCommand, ICommandHandler<RealCommand>>(
            //     new CommandHandlerDecoratorWithDependency<SpecialCommand, ICommandHandler<SpecialCommand>>(
            //         new CommandHandlerDecoratorWithDependency<SpecialCommand, ICommandHandler<SpecialCommand>>(
            //             // stack overflow
            //     new RealCommandHandler()) // the decoratee
            Action action = () => container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                CommandHandlerDecoratorWithDependency<SpecialCommand, ICommandHandler<SpecialCommand>> is
                directly or indirectly depending on itself"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_DecoratorWithDecorateeAndDependencyOfTheSameOpenGenericTypeCausingACyclicDependency_ThrowExpectedException2()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommandHandler<RealCommand>, RealCommandHandler>();
            container.Register<ICommandHandler<SpecialCommand>, NullCommandHandler<SpecialCommand>>();

            // Create: CommandHandlerDecoratorWithDependency<T, ICommandHandler<RealCommand>>
            Type decoratorType =
                typeof(CommandHandlerDecoratorWithDependency<,>).MakePartialOpenGenericType(
                    secondArgument: typeof(ICommandHandler<RealCommand>));

            container.RegisterDecorator(typeof(ICommandHandler<>), decoratorType);

            // Act
            // Here we try to resolve the following dependency chain:
            // new CommandHandlerDecoratorWithDependency<RealCommand, ICommandHandler<RealCommand>>(
            //     new CommandHandlerDecoratorWithDependency<RealCommand, ICommandHandler<RealCommand>>(
            //         new CommandHandlerDecoratorWithDependency<RealCommand, ICommandHandler<RealCommand>>(
            //             // stack overflow
            //     new RealCommandHandler()) // the decoratee
            Action action = () => container.GetInstance<ICommandHandler<RealCommand>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                CommandHandlerDecoratorWithDependency<RealCommand, ICommandHandler<RealCommand>> is
                directly or indirectly depending on itself"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_TwoRegistrationsForSameServiceWithSingletonDecorator_EachRegistrationGetsItsOwnDecorator()
        {
            // Arrange
            var container = new Container();

            var consoleProducer = Lifestyle.Singleton.CreateProducer<ILogger, ConsoleLogger>(container);
            var nullProducer = Lifestyle.Singleton.CreateProducer<ILogger, NullLogger>(container);

            container.RegisterDecorator<ILogger, LoggerDecorator>(Lifestyle.Singleton);

            var consoleDecorator = (LoggerDecorator)consoleProducer.GetInstance();
            var nullDecorator = (LoggerDecorator)nullProducer.GetInstance();

            AssertThat.IsInstanceOfType(typeof(ConsoleLogger), consoleDecorator.Logger);
            AssertThat.IsInstanceOfType(typeof(NullLogger), nullDecorator.Logger);
        }

        [TestMethod]
        public void GetInstance_TwoRegistrationsForSameServiceWithScopedDecorator_EachRegistrationGetsItsOwnDecorator()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            var consoleProducer = Lifestyle.Scoped.CreateProducer<ILogger, ConsoleLogger>(container);
            var nullProducer = Lifestyle.Scoped.CreateProducer<ILogger, NullLogger>(container);

            container.RegisterDecorator<ILogger, LoggerDecorator>(Lifestyle.Scoped);

            using (ThreadScopedLifestyle.BeginScope(container))
            {
                var consoleDecorator = (LoggerDecorator)consoleProducer.GetInstance();
                var nullDecorator = (LoggerDecorator)nullProducer.GetInstance();

                AssertThat.IsInstanceOfType(typeof(ConsoleLogger), consoleDecorator.Logger);
                AssertThat.IsInstanceOfType(typeof(NullLogger), nullDecorator.Logger);
            }
        }

        [TestMethod]
        public void GetInstance_OnDecoratedTypeWhereDecoratorTypeHasNamelessParameters_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<ILogger, NullLogger>();

            // Castle Dynamic Proxy generates a type with a constructor with parameters where Name is null!
            // The decorator's constructor becomes: ctor(IInterceptor[], ILogger)
            var decoratorType = new DefaultProxyBuilder().CreateInterfaceProxyTypeWithTargetInterface(
                typeof(ILogger), Type.EmptyTypes, ProxyGenerationOptions.Default);

            container.RegisterDecorator(typeof(ILogger), decoratorType);

            // Inject IInterceptor[] in the decorator (needed because Castle spits out such type).
            container.RegisterConditional(typeof(IInterceptor[]),
                Lifestyle.Singleton.CreateRegistration(() => new IInterceptor[0], container),
                c => c.Consumer.ImplementationType == decoratorType);

            // Act
            var logger = container.GetInstance<ILogger>();

            // Assert
            AssertThat.IsInstanceOfType(decoratorType, logger);
        }

        private static KnownRelationship GetValidRelationship()
        {
            // Arrange
            var container = new Container();

            return new KnownRelationship(typeof(object), Lifestyle.Transient,
                container.GetRegistration(typeof(Container)));
        }

        public class DummyInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
            }
        }
    }

    public static class ContainerTestExtensions
    {
        internal static void RegisterDecorator(this Container container,
            RegisterDecoratorFactoryParameters parameters)
        {
            container.RegisterDecorator(parameters.ServiceType, parameters.DecoratorTypeFactory,
                parameters.Lifestyle, parameters.Predicate);
        }
    }

    public class RegisterDecoratorFactoryParameters
    {
        public static RegisterDecoratorFactoryParameters CreateValid()
        {
            return new RegisterDecoratorFactoryParameters
            {
                ServiceType = typeof(ICommandHandler<>),
                DecoratorTypeFactory = context => typeof(AsyncCommandHandlerProxy<>),
                Lifestyle = Lifestyle.Transient,
                Predicate = context => true,
            };
        }

        public Type ServiceType { get; set; }

        public Func<DecoratorPredicateContext, Type> DecoratorTypeFactory { get; set; }

        public Lifestyle Lifestyle { get; set; }

        public Predicate<DecoratorPredicateContext> Predicate { get; set; }
    }

    public class DependencyInfo
    {
        public DependencyInfo(Type serviceType, Lifestyle lifestyle)
        {
            this.ServiceType = serviceType;
            this.Lifestyle = lifestyle;
        }

        public Type ServiceType { get; }

        public Lifestyle Lifestyle { get; }
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

    public class InvalidCommandHandlerDecoratorWithTwoDecoratees<T> : ICommandHandler<T>
    {
        // This is not a decorator as it expects more than one ICommandHandler<T> parameter.
        public InvalidCommandHandlerDecoratorWithTwoDecoratees(ICommandHandler<T> decorated1,
            ICommandHandler<T> decorated2, ILogger logger)
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

        public INonGenericService DecoratedService { get; }

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

        public INonGenericService DecoratedService { get; }

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

        public Func<INonGenericService> DecoratedServiceCreator { get; }

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

    public class EnumerableDecorator<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> decoratedCollection;

        public EnumerableDecorator(IEnumerable<T> decoratedCollection)
        {
            this.decoratedCollection = decoratedCollection;
        }

        // Scenario: do some filtering here, based on the user's role.
        public IEnumerator<T> GetEnumerator() => this.decoratedCollection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public class InvalidCommandHandlerDecoratorWithDecorateeAndFactory<T> : ICommandHandler<T>
    {
        // This is not a decorator as it expects more than one ICommandHandler<T> parameter.
        public InvalidCommandHandlerDecoratorWithDecorateeAndFactory(ICommandHandler<T> decorated,
            Func<ICommandHandler<T>> decoratedFactory)
        {
        }

        public void Handle(T command)
        {
        }
    }

    public class CommandHandlerDecoratorWithDependency<TCommand, TDependency> : ICommandHandler<TCommand>
    {
        public readonly TDependency Dependency;
        public readonly ICommandHandler<TCommand> Decoratee;

        public CommandHandlerDecoratorWithDependency(
            TDependency dependency,
            ICommandHandler<TCommand> decoratee)
        {
            this.Dependency = dependency;
            this.Decoratee = decoratee;
        }

        public void Handle(TCommand command)
        {
        }
    }

    internal class RelationshipInfo
    {
        public Type ImplementationType { get; set; }

        public Lifestyle Lifestyle { get; set; }

        public DependencyInfo Dependency { get; set; }

        internal static bool EqualsTo(RelationshipInfo info, KnownRelationship other) => 
            info.ImplementationType == other.ImplementationType 
            && info.Lifestyle == other.Lifestyle 
            && info.Dependency.ServiceType == other.Dependency.ServiceType 
            && info.Dependency.Lifestyle == other.Dependency.Lifestyle;

        internal bool Equals(KnownRelationship other) => EqualsTo(this, other);

        internal static string ToString(KnownRelationship relationship) => 
            string.Format("ImplementationType: {0}, Lifestyle: {1}, Dependency: {2}",
                relationship.ImplementationType.ToFriendlyName(),
                relationship.Lifestyle.Name,
                string.Format("{{ ServiceType: {0}, Lifestyle: {1} }}",
                    relationship.Dependency.ServiceType.ToFriendlyName(),
                    relationship.Dependency.Lifestyle.Name));
    }
}