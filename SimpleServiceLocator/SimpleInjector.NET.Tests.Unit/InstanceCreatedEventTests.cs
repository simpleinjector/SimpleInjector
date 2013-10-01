namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Extensions;
    using SimpleInjector.Lifestyles;
    using SimpleInjector.Tests.Unit.Extensions;

    [TestClass]
    public class InstanceCreatedEventTests
    {
        [TestMethod]
        public void BuildExpression_ContainerWithoutInstanceCreatedEventApplied_DoesNotApplyEventToExpression()
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

            container.InstanceCreated += (s, e) => actualCallCount++;

            // Act
            container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreEqual(expectedCallCount, actualCallCount);
        }

        [TestMethod]
        public void GetInstance_CalledTwiceOnUnregisteredConcreteTypeWithoutDependencies_CallsInstanceCreatedTwice()
        {
            // Registration
            int expectedCallCount = 2;
            int actualCallCount = 0;

            var container = new Container();

            container.InstanceCreated += (s, e) => actualCallCount++;

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

            container.InstanceCreated += (s, e) => actualCallCount++;

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
            var args = new List<Tuple<InstanceProducer, InstanceCreatedEventArgs>>();

            var container = new Container();

            container.InstanceCreated += (s, e) => args.Add(Tuple.Create(s, e));

            // Act
            var producer = container.GetRegistration(typeof(RealTimeProvider));

            object instance = container.GetInstance<RealTimeProvider>();

            // Assert
            var expected = new InstanceCreatedEventArgs(
                producer.Registration,
                instance);

            Assert.AreEqual(producer, args.First().Item1);

            Assert.AreEqual(
                expected: new InstanceCreatedEventArgs(
                    producer.Registration,
                    instance),
                actual: args.First().Item2);
        }

        [TestMethod]
        public void GetInstance_OnRegisteredConcreteTransientTypeWithoutDependencies_CallsInstanceCreatedWithExpectedType()
        {
            // Registration
            var args = new List<InstanceCreatedEventArgs>();

            var container = new Container();

            container.Register<RealTimeProvider>();

            container.InstanceCreated += (s, e) => args.Add(e);

            // Act
            object instance = container.GetInstance<RealTimeProvider>();

            // Assert
            var expected = new InstanceCreatedEventArgs(
                container.GetRegistration(typeof(RealTimeProvider)).Registration,
                instance);

            Assert.AreEqual(expected, args.First());
        }

        [TestMethod]
        public void GetInstance_RegistrationWithDecorator_CallsInstanceCreatedForBothTheInstanceAndTheDecorator()
        {
            // Arrange
            var args = new List<InstanceCreatedEventArgs>();

            var container = new Container();

            container.Register<RealTimeProvider>();

            container.InstanceCreated += (s, e) => args.Add(e);

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>();
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var decorator = container.GetInstance<ICommandHandler<RealCommand>>() as RealCommandHandlerDecorator;

            // Assert
            Assert.AreEqual(2, args.Count, "Two event args were expected.");

            Assert.AreEqual(
                expected: new InstanceCreatedEventArgs(
                    container.GetRegistration(typeof(ICommandHandler<RealCommand>)).Registration,
                    decorator.Decorated),
                actual: args.First());

            Assert.AreEqual(typeof(RealCommandHandlerDecorator), args.Second().Registration.ImplementationType);
            Assert.AreSame(decorator, args.Second().Instance);
        }

        [TestMethod]
        public void GetInstance_CalledTwiceForSingletonRegistrationWithTransientDecorator_CallsEventOnceForInstanceTwiceForDecorator()
        {
            // Arrange
            var args = new List<InstanceCreatedEventArgs>();

            var container = new Container();

            container.Register<RealTimeProvider>();

            container.InstanceCreated += (s, e) => args.Add(e);

            container.RegisterSingle<ICommandHandler<RealCommand>, StubCommandHandler>();
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var decorator1 = container.GetInstance<ICommandHandler<RealCommand>>() as RealCommandHandlerDecorator;
            var decorator2 = container.GetInstance<ICommandHandler<RealCommand>>() as RealCommandHandlerDecorator;

            // Assert
            Assert.AreEqual(3, args.Count, "Three event args were expected.");

            Assert.AreEqual(
                expected: new InstanceCreatedEventArgs(
                    container.GetRegistration(typeof(ICommandHandler<RealCommand>)).Registration,
                    decorator1.Decorated),
                actual: args.First());

            Assert.AreEqual(typeof(RealCommandHandlerDecorator), args.Second().Registration.ImplementationType);
            Assert.AreSame(decorator1, args.Second().Instance);

            Assert.AreEqual(typeof(RealCommandHandlerDecorator), args.Last().Registration.ImplementationType);
            Assert.AreSame(decorator2, args.Last().Instance);
        }

        [TestMethod]
        public void GetInstance_CalledForDecoratedCollection_CallsEventForBothTheInstanceAndTheDecorator()
        {
            // Arrange
            var args = new List<InstanceCreatedEventArgs>();

            var container = new Container();

            container.RegisterAll<ICommandHandler<RealCommand>>(typeof(StubCommandHandler));

            container.InstanceCreated += (s, e) => args.Add(e);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var decorator = container.GetAllInstances<ICommandHandler<RealCommand>>().Single()
                as RealCommandHandlerDecorator;

            // Assert
            // TODO: The container should actually call InstanceCreated for the IEnumerable<T> as well.
            Assert.AreEqual(2, args.Count, "Two event args were expected.");
            Assert.AreEqual(typeof(StubCommandHandler), args.First().Registration.ImplementationType);
            Assert.AreEqual(typeof(RealCommandHandlerDecorator), args.Second().Registration.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_CalledForDecoratedUncontrolledCollection_CallsEventForBothTheInstanceAndTheDecorator()
        {
            // Arrange
            var args = new List<Tuple<InstanceProducer, InstanceCreatedEventArgs>>();

            var container = new Container();

            // Container uncontrolled collection
            IEnumerable<ICommandHandler<RealCommand>> handlers = new ICommandHandler<RealCommand>[]
            {
                new StubCommandHandler(),
            };

            container.RegisterAll<ICommandHandler<RealCommand>>(handlers);

            container.InstanceCreated += (s, e) => args.Add(Tuple.Create(s, e));

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(RealCommandHandlerDecorator));

            // Act
            var producer = container.GetRegistration(typeof(IEnumerable<ICommandHandler<RealCommand>>));

            var decorator = container.GetAllInstances<ICommandHandler<RealCommand>>().Single()
                as RealCommandHandlerDecorator;

            // Assert
            Assert.AreEqual(2, args.Count, "Two event args were expected.");

            Assert.AreSame(producer, args.First().Item1);
            Assert.AreSame(producer, args.Second().Item1);

            Assert.AreEqual(typeof(IEnumerable<ICommandHandler<RealCommand>>), args.First().Item2.Registration.ImplementationType);
            Assert.AreEqual(typeof(RealCommandHandlerDecorator), args.Second().Item2.Registration.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_ForHybridLifestyledRegistration_CallsInstanceCreated()
        {
            // Arrange
            var args = new List<InstanceCreatedEventArgs>();

            var container = new Container();

            var hybrid = Lifestyle.CreateHybrid(() => true, Lifestyle.Transient, Lifestyle.Singleton);

            container.Register<RealTimeProvider, RealTimeProvider>(hybrid);

            container.InstanceCreated += (s, e) => args.Add(e);

            // Act
            container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreEqual(2, args.Count, "Both the singleton and transient instance should have been triggered.");

            // Act
            args.Clear();

            container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreEqual(1, args.Count, "A transient should have been created.");
        }

        [TestMethod]
        public void GetInstance_ForCustomLifestyledRegistration_CallsInstanceCreated()
        {
            // Arrange
            var args = new List<InstanceCreatedEventArgs>();

            var container = new Container();

            var custom = Lifestyle.CreateCustom("Custom", transientInstanceCreator => transientInstanceCreator);

            container.Register<RealTimeProvider, RealTimeProvider>(custom);

            container.InstanceCreated += (s, e) => args.Add(e);

            // Act
            container.GetInstance<RealTimeProvider>();

            // Assert
            Assert.AreEqual(1, args.Count);
        }

        [TestMethod]
        public void GetInstance_OpenGenericRegistration_CallsInstanceCreated()
        {
            // Arrange
            var args = new List<InstanceCreatedEventArgs>();

            var container = new Container();

            container.RegisterOpenGeneric(typeof(IValidate<>), typeof(NullValidator<>));

            container.InstanceCreated += (s, e) => args.Add(e);

            // Act
            var instance = container.GetInstance<IValidate<int>>();

            // Assert
            Assert.AreEqual(1, args.Count);

            Assert.AreEqual(
                expected: new InstanceCreatedEventArgs(
                    container.GetRegistration(typeof(IValidate<int>)).Registration,
                    instance),
                actual: args.First());
        }

        [TestMethod]
        public void Equals_TwoEmptyInstanceCreatedEventArgs_AreConsideredEqual()
        {
            // Arrange
            var a = new InstanceCreatedEventArgs();
            var b = new InstanceCreatedEventArgs();

            // Assert
            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void Equals_TwoInstanceCreatedEventArgsWithSameValues_AreConsideredEqual()
        {
            // Arrange
            var registration = CreateDummyRegistration();
            object instance = new object();

            var a = new InstanceCreatedEventArgs(registration, instance);
            var b = new InstanceCreatedEventArgs(registration, instance);

            // Assert
            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void Equals_TwoInstanceCreatedEventArgsWithNullRegistrationAndSameInstance_AreConsideredEqual()
        {
            // Arrange
            object instance = new object();
            var a = new InstanceCreatedEventArgs(null, instance);
            var b = new InstanceCreatedEventArgs(null, instance);

            // Assert
            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void Equals_TwoInstanceCreatedEventArgsWithNullRegistrationDifferentInstances_AreNotConsideredEqual()
        {
            // Arrange
            var a = new InstanceCreatedEventArgs(null, new object());
            var b = new InstanceCreatedEventArgs(null, new object());

            // Assert
            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void Equals_TwoInstanceCreatedEventArgsWithSameInstanceButDifferentRegistrations_AreNotConsideredEqual()
        {
            // Arrange
            object instance = new object();

            var a = new InstanceCreatedEventArgs(CreateDummyRegistration(), instance);
            var b = new InstanceCreatedEventArgs(CreateDummyRegistration(), instance);

            // Assert
            Assert.AreNotEqual(a, b);
        }

        private static Registration CreateDummyRegistration()
        {
            return new ExpressionRegistration(Expression.Constant(null), new Container());
        }
    }
}