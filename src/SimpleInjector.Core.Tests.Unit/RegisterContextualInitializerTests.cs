namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

    [TestClass]
    public class RegisterContextualInitializerTests
    {
        private static readonly Predicate<InitializerContext> TruePredicate = c => true;
        private static readonly Predicate<InitializerContext> FalsePredicate = c => true;

        [TestMethod]
        public void BuildExpression_ContainerWithoutRegisterInitializerApplied_DoesNotApplyEventToExpression()
        {
            // Registration
            var container = new Container();

            // Act
            var expression = container.GetRegistration(typeof(RealTimeProvider)).BuildExpression();

            // Assert
            Assert.AreEqual("new RealTimeProvider()", expression.ToString(),
                "The event should not be applied to the expression, since that would cause performance " +
                "penalty, much like a RegisterInitializer<object>(() => { }) would cause.");
        }

        [TestMethod]
        public void GetInstance_OnUnregisteredConcreteTypeWithoutDependencies_CallsInstanceCreatedOnce()
        {
            // Registration
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = new Container();

            container.RegisterInitializer(context => actualCallCount++, TruePredicate);

            // Act
            container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount);
        }
        
        [TestMethod]
        public void GetInstance_CalledOnInitializerWithPredicateReturningFalse_CallsPredicateOnceAndDelegateNever()
        {
            // Registration
            int expectedCallCount = 0;
            int actualCallCount = 0;
            int expectedPredicateCallCount = 1;
            int actualPredicateCallCount = 0;

            var container = new Container();

            container.RegisterInitializer(context => actualCallCount++, context =>
            {
                actualPredicateCallCount++;
                return false;
            });

            // Act
            container.GetInstance<RealTimeProvider>();
            container.GetInstance<RealTimeProvider>();
            container.GetInstance<RealTimeProvider>();
            container.GetInstance<RealTimeProvider>();
            container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount);
            Assert.AreEqual(expectedPredicateCallCount, actualPredicateCallCount);
        }

        [TestMethod]
        public void GetInstance_CalledOnInitializerWithPredicateReturningTrue_CallsPredicateOnceAndDelegateForEachCreatedInstance()
        {
            // Registration
            int expectedCallCount = 5;
            int actualCallCount = 0;
            int expectedPredicateCallCount = 1;
            int actualPredicateCallCount = 0;

            var container = new Container();

            container.RegisterInitializer(context => actualCallCount++, context =>
            {
                actualPredicateCallCount++;
                return true;
            });

            // Act
            container.GetInstance<RealTimeProvider>();
            container.GetInstance<RealTimeProvider>();
            container.GetInstance<RealTimeProvider>();
            container.GetInstance<RealTimeProvider>();
            container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount);
            Assert.AreEqual(expectedPredicateCallCount, actualPredicateCallCount);
        }

        [TestMethod]
        public void GetInstance_CalledTwiceOnUnregisteredConcreteTypeWithoutDependencies_CallsInstanceCreatedTwice()
        {
            // Registration
            int expectedCallCount = 2;
            int actualCallCount = 0;

            var container = new Container();

            container.RegisterInitializer(context => actualCallCount++, TruePredicate);

            // Act
            container.GetInstance<RealTimeProvider>();
            container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount);
        }

        [TestMethod]
        public void GetInstance_CalledTwiceOnSingletonWithoutDependencies_CallsInstanceCreatedOnce()
        {
            // Registration
            int expectedCallCount = 1;
            int actualCallCount = 0;

            var container = new Container();

            container.Register<RealTimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            container.RegisterInitializer(context => actualCallCount++, TruePredicate);

            // Act
            container.GetInstance<RealTimeProvider>();
            container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount);
        }

        [TestMethod]
        public void GetInstance_OnUnregisteredConcreteTypeWithoutDependencies_CallsInstanceCreatedOnceWithExpectedType()
        {
            // Registration
            var actualContexts = new List<InstanceInitializationData>();

            var container = new Container();

            container.RegisterInitializer(actualContexts.Add, TruePredicate);

            // Act
            var producer = container.GetRegistration(typeof(RealTimeProvider));

            object instance = container.GetInstance<RealTimeProvider>();

            // Assert
            var actualContext = actualContexts.First().Context;

            Assert.AreSame(producer.Registration, actualContext.Registration);
            Assert.AreSame(instance, actualContexts.First().Instance);
        }

        [TestMethod]
        public void GetInstance_OnRegisteredConcreteTransientTypeWithoutDependencies_CallsInstanceCreatedWithExpectedType()
        {
            // Registration
            var actualContexts = new List<InstanceInitializationData>();

            var container = new Container();

            container.Register<RealTimeProvider>();

            container.RegisterInitializer(actualContexts.Add, TruePredicate);

            // Act
            object instance = container.GetInstance<RealTimeProvider>();

            // Assert
            var actualContext = actualContexts.First().Context;

            Assert.AreSame(container.GetRegistration(typeof(RealTimeProvider)).Registration, actualContext.Registration);
            Assert.AreSame(instance, actualContexts.First().Instance);
        }

        [TestMethod]
        public void GetInstance_RegistrationWithDecorator_CallsInstanceCreatedForBothTheInstanceAndTheDecorator()
        {
            // Arrange
            var actualContexts = new List<InstanceInitializationData>();

            var container = new Container();

            container.Register<RealTimeProvider>();

            container.RegisterInitializer(actualContexts.Add, TruePredicate);

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var decorator = container.GetInstance<ICommandHandler<RealCommand>>() as RealCommandHandlerDecorator;

            // Assert
            Assert.AreEqual(2, actualContexts.Count, "Two event args were expected.");

            AssertThat.IsInstanceOfType(typeof(StubCommandHandler), actualContexts.First().Instance);

            AssertThat.IsInstanceOfType(typeof(RealCommandHandlerDecorator), actualContexts.Second().Instance);

            Assert.AreSame(decorator, actualContexts.Second().Instance);
        }

        [TestMethod]
        public void GetInstance_CalledTwiceForSingletonRegistrationWithTransientDecorator_CallsEventOnceForInstanceTwiceForDecorator()
        {
            // Arrange
            var actualContexts = new List<InstanceInitializationData>();

            var container = new Container();

            container.Register<RealTimeProvider>();

            container.RegisterInitializer(actualContexts.Add, TruePredicate);

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(Lifestyle.Singleton);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var decorator1 = container.GetInstance<ICommandHandler<RealCommand>>() as RealCommandHandlerDecorator;
            var decorator2 = container.GetInstance<ICommandHandler<RealCommand>>() as RealCommandHandlerDecorator;

            // Assert
            Assert.AreEqual(3, actualContexts.Count, "Three event args were expected.");

            AssertThat.IsInstanceOfType(typeof(StubCommandHandler), actualContexts.First().Instance);

            Assert.AreSame(decorator1.Decorated, actualContexts.First().Instance);

            Assert.AreEqual(
                expected: typeof(RealCommandHandlerDecorator), 
                actual: actualContexts.Second().Context.Registration.ImplementationType);

            Assert.AreSame(decorator1, actualContexts.Second().Instance);

            Assert.AreEqual(
                expected: typeof(RealCommandHandlerDecorator), 
                actual: actualContexts.Last().Context.Registration.ImplementationType);
            
            Assert.AreSame(decorator2, actualContexts.Last().Instance);
        }

        [TestMethod]
        public void GetInstance_CalledForDecoratedCollection_CallsEventForBothTheInstanceAndTheDecorator()
        {
            // Arrange
            var actualContexts = new List<InstanceInitializationData>();

            var container = new Container();

            container.RegisterCollection<ICommandHandler<RealCommand>>(new[] { typeof(StubCommandHandler) });

            container.RegisterInitializer(actualContexts.Add, TruePredicate);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var decorator = container.GetAllInstances<ICommandHandler<RealCommand>>().Single()
                as RealCommandHandlerDecorator;

            // Assert
            // TODO: The container should actually call InstanceCreated for the IEnumerable<T> as well.
            Assert.AreEqual(2, actualContexts.Count, "Two event args were expected.");

            Assert.AreEqual(typeof(StubCommandHandler), actualContexts.First().Context.Registration.ImplementationType);
            Assert.AreEqual(typeof(RealCommandHandlerDecorator), actualContexts.Second().Context.Registration.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_CalledForDecoratedUncontrolledCollection_CallsEventForBothTheInstanceAndTheDecorator()
        {
            // Arrange
            var actualContexts = new List<InstanceInitializationData>();

            var container = new Container();

            // Container uncontrolled collection
            IEnumerable<ICommandHandler<RealCommand>> handlers = new ICommandHandler<RealCommand>[]
            {
                new StubCommandHandler(),
            };

            container.RegisterCollection<ICommandHandler<RealCommand>>(handlers);

            container.RegisterInitializer(actualContexts.Add, TruePredicate);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var producer = container.GetRegistration(typeof(IEnumerable<ICommandHandler<RealCommand>>));

            var decorator = container.GetAllInstances<ICommandHandler<RealCommand>>().Single()
                as RealCommandHandlerDecorator;

            // Assert
            Assert.AreEqual(2, actualContexts.Count, "Two event args were expected.");

            Assert.AreSame(producer.Registration, actualContexts.First().Context.Registration);

            Assert.AreEqual(
                typeof(IEnumerable<ICommandHandler<RealCommand>>), 
                actualContexts.First().Context.Registration.ImplementationType);

            Assert.AreEqual(
                typeof(RealCommandHandlerDecorator),
                actualContexts.Second().Context.Registration.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_ForHybridLifestyledRegistration_CallsInstanceCreated()
        {
            // Arrange
            var actualContexts = new List<InstanceInitializationData>();

            var container = new Container();

            var hybrid = Lifestyle.CreateHybrid(() => true, Lifestyle.Transient, Lifestyle.Singleton);

            container.Register<RealTimeProvider, RealTimeProvider>(hybrid);

            container.RegisterInitializer(actualContexts.Add, TruePredicate);

            // Act
            container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreEqual(2, actualContexts.Count, 
                "Both the singleton and transient instance should have been triggered.");

            // Act
            actualContexts.Clear();

            container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreEqual(1, actualContexts.Count, "A transient should have been created.");
        }

        [TestMethod]
        public void GetInstance_ForCustomLifestyledRegistration_CallsInstanceCreated()
        {
            // Arrange
            var actualContexts = new List<InstanceInitializationData>();

            var container = new Container();

            var custom = 
                Lifestyle.CreateCustom("Custom", transientInstanceCreator => transientInstanceCreator);

            container.Register<RealTimeProvider, RealTimeProvider>(custom);

            container.RegisterInitializer(actualContexts.Add, TruePredicate);

            // Act
            container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreEqual(1, actualContexts.Count);
        }

        [TestMethod]
        public void GetInstance_OpenGenericRegistration_CallsInstanceCreated()
        {
            // Arrange
            var actualContexts = new List<InstanceInitializationData>();

            var container = new Container();

            container.Register(typeof(IValidate<>), typeof(NullValidator<>));

            container.RegisterInitializer(actualContexts.Add, TruePredicate);

            // Act
            var instance = container.GetInstance<IValidate<int>>();

            // Assert
            Assert.AreEqual(1, actualContexts.Count);

            Assert.AreEqual(
                expected: container.GetRegistration(typeof(IValidate<int>)).Registration,
                actual: actualContexts.First().Context.Registration);

            Assert.AreSame(instance, actualContexts.First().Instance);
        }

        [TestMethod]
        public void Equals_TwoEmptyInstanceInitializationContext_AreConsideredEqual()
        {
            // Arrange
            var a = new InstanceInitializationData();
            var b = new InstanceInitializationData();

            // Assert
            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void Equals_TwoInstanceInitializationContextWithSameValues_AreConsideredEqual()
        {
            // Arrange
            var context = CreateDummyInitializationContext();
            object instance = new object();

            var a = new InstanceInitializationData(context, instance);
            var b = new InstanceInitializationData(context, instance);

            // Assert
            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void Equals_TwoInstanceInitializationContextWithSameInstanceButDifferentRegistrations_AreNotConsideredEqual()
        {
            // Arrange
            object instance = new object();

            var a = new InstanceInitializationData(CreateDummyInitializationContext(), instance);
            var b = new InstanceInitializationData(CreateDummyInitializationContext(), instance);

            // Assert
            Assert.AreNotEqual(a, b);
        }

        private static InitializerContext CreateDummyInitializationContext()
        {
            var registration = new ExpressionRegistration(Expression.Constant(null), new Container());

            return new InitializerContext(registration);
        }
    }
}