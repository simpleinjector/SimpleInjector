namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    [TestClass]
    public class LifestyleRegistrationTests
    {
        [TestMethod]
        public void BuildExpression_ReturningNull_ContainerWillThrowAnExpressiveExceptionMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            var invalidLifestyle = new FakeLifestyle();
            
            var invalidRegistration = new FakeRegistration(invalidLifestyle, container, typeof(RealTimeProvider))
            {
                ExpressionToReturn = null
            };

            invalidLifestyle.RegistrationToReturn = invalidRegistration;

            container.Register<ITimeProvider, RealTimeProvider>(invalidLifestyle);

            try
            {
                // Act
                container.GetInstance<ITimeProvider>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.ExceptionMessageContains(
                    "The FakeRegistration for the FakeLifestyle returned a null reference " + 
                    "from its BuildExpression method.", ex);
            }
        }

        [TestMethod]
        public void InitializeInstance_WithNullArgument_ThrowsArgumentNullException()
        {
            // Arrange
            object invalidInstance = null;

            var container = ContainerFactory.New();

            var registration = container.GetRegistration(typeof(Container)).Registration;

            // Act
            Action action = () => registration.InitializeInstance(invalidInstance);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("instance", action);
        }

        [TestMethod]
        public void InitializeInstance_WithIncompatibleType_ThrowExpectedException()
        {
            // Arrange
            object invalidInstance = new object();

            var container = ContainerFactory.New();

            var registration = container.GetRegistration(typeof(Container)).Registration;

            // Act
            Action action = () => registration.InitializeInstance(invalidInstance);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentException>("instance", action);
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied type Object does not inherit from Container.", 
                action);
        }

        [TestMethod]
        public void InitializeInstance_WithValidInstance_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration = container.GetRegistration(typeof(Container)).Registration;

            // Act
            registration.InitializeInstance(container);
        }

        [TestMethod]
        public void InitializeInstance_WithIncompatibleType2_ThrowExpectedException()
        {
            // Arrange
            object invalidInstance = new FakeTimeProvider();

            var container = ContainerFactory.New();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            var registration = container.GetRegistration(typeof(ITimeProvider)).Registration;

            // Act
            Action action = () => registration.InitializeInstance(invalidInstance);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied type FakeTimeProvider does not inherit from RealTimeProvider.",
                action);
        }
        
        [TestMethod]
        public void InitializeInstance_WithSubTypeOfRegistration_Succeeds()
        {
            // Arrange
            var validInstance = new RealTimeProviderSubType();

            var container = ContainerFactory.New();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            var registration = container.GetRegistration(typeof(ITimeProvider)).Registration;

            // Act
            registration.InitializeInstance(validInstance);
        }

        [TestMethod]
        public void InitializeInstance_WithPropertyInjection_InjectsProperty()
        {
            // Arrange
            var instanceToInitialize = new ServiceWithProperty<RealTimeProvider>();

            Assert.IsNull(instanceToInitialize.Dependency, "Test setup failed.");

            var container = ContainerFactory.New();

            container.Options.PropertySelectionBehavior = new PredicatePropertySelectionBehavior
            {
                Predicate = property => property.Name == "Dependency"
            };

            var registration = container.GetRegistration(instanceToInitialize.GetType()).Registration;

            // Act
            registration.InitializeInstance(instanceToInitialize);

            // Assert
            Assert.IsNotNull(instanceToInitialize.Dependency);
        }

        [TestMethod]
        public void InitializeInstance_WithPropertyInjection_GetsIntercepted()
        {
            // Arrange
            Expression interceptedExpression = null;

            var container = ContainerFactory.New();

            container.Options.PropertySelectionBehavior = new PredicatePropertySelectionBehavior
            {
                Predicate = property => property.Name == "Dependency"
            };

            container.ExpressionBuilding += (s, e) =>
            {
                interceptedExpression = e.Expression;
            };

            var registration = 
                container.GetRegistration(typeof(ServiceWithProperty<RealTimeProvider>)).Registration;

            // GetRegistration might trigger interception, so we have to reset that.
            interceptedExpression = null;

            // Act
            registration.InitializeInstance(new ServiceWithProperty<RealTimeProvider>());

            // Assert
            Assert.IsNotNull(interceptedExpression);
            Assert.AreEqual(interceptedExpression.Type, typeof(ServiceWithProperty<RealTimeProvider>));
        }

        [TestMethod]
        public void InitializeInstance_RegisteredInitializer_CallsInitializer()
        {
            // Arrange
            bool initializerCalled = false;

            var container = ContainerFactory.New();

            container.RegisterInitializer<ITimeProvider>(provider => { initializerCalled = true; });

            var registration = container.GetRegistration(typeof(RealTimeProvider)).Registration;

            // Act
            registration.InitializeInstance(new RealTimeProvider());

            // Assert
            Assert.IsTrue(initializerCalled, "The initializer should have been called.");
        }

        [TestMethod]
        public void CreateRegistrationWithGeneric_RegisteringCovarientType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            var producer = 
                Lifestyle.Transient.CreateProducer<ICovariant<object>, CovariantImplementation<string>>(
                    container);

            // Act
            ICovariant<object> instance = producer.GetInstance();

            // Assert
            AssertThat.IsInstanceOfType(typeof(CovariantImplementation<string>), instance);
        }

        [TestMethod]
        public void CreateRegistrationWithType_RegisteringCovarientType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            var producer = Lifestyle.Transient.CreateProducer(
                typeof(ICovariant<object>), typeof(CovariantImplementation<string>), container);

            // Act
            var instance = producer.GetInstance();

            // Assert
            AssertThat.IsInstanceOfType(typeof(CovariantImplementation<string>), instance);
        }

        public class ServiceWithProperty<TDependency>
        {
            public TDependency Dependency { get; set; }
        }

        private sealed class RealTimeProviderSubType : RealTimeProvider 
        {
        }
        
        private sealed class PredicatePropertySelectionBehavior : IPropertySelectionBehavior
        {
            public Predicate<PropertyInfo> Predicate { get; set; }

            public bool SelectProperty(Type type, PropertyInfo property) => this.Predicate(property);
        }
    }

    internal sealed class FakeLifestyle : Lifestyle
    {
        public FakeLifestyle() : base("Fake")
        {
        }

        public Registration RegistrationToReturn { get; set; }

        public override int Length => Transient.Length;

        protected internal override Registration CreateRegistrationCore<TConcrete>(Container container)
        {
            return this.RegistrationToReturn;
        }

        protected internal override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator, 
            Container container)
        {
            return this.RegistrationToReturn;
        }
    }

    internal sealed class FakeRegistration : Registration
    {
        public FakeRegistration(Lifestyle lifestyle, Container container, Type implementationType) 
            : base(lifestyle, container)
        {
            this.ImplementationType = implementationType;
        }

        public override Type ImplementationType { get; }

        public Expression ExpressionToReturn { get; set; }

        public override Expression BuildExpression() => this.ExpressionToReturn;
    }
}