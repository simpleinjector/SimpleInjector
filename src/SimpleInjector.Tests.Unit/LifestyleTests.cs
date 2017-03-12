namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LifestyleTests
    {
        [TestMethod]
        public void Transient_Always_ReturnsAnInstance()
        {
            Assert.IsNotNull(Lifestyle.Transient);
        }

        [TestMethod]
        public void Transient_Always_ReturnsTheSameInstance()
        {
            // Act
            var transient1 = Lifestyle.Transient;
            var transient2 = Lifestyle.Transient;

            Assert.IsTrue(object.ReferenceEquals(transient1, transient2),
                "Lifestyle implementations must be tread-safe, so from a performance perspective, the " +
                "Lifestyle.Transient property must return a singleton.");
        }

        [TestMethod]
        public void Singleton_Always_ReturnsAnInstance()
        {
            Assert.IsNotNull(Lifestyle.Singleton);
        }

        [TestMethod]
        public void Singleton_Always_ReturnsTheSameInstance()
        {
            // Act
            var singleton1 = Lifestyle.Singleton;
            var singleton2 = Lifestyle.Singleton;

            Assert.IsTrue(object.ReferenceEquals(singleton1, singleton2),
                "Lifestyle implementations must be tread-safe, so from a performance perspective, the " +
                "Lifestyle.Singleton property must return a singleton.");
        }

        [TestMethod]
        public void Ctor_NullArgument_ThrowsExpectedException()
        {
            Action action = () => new FakeLifestyle(null);

            AssertThat.ThrowsWithParamName<ArgumentNullException>("name", action);
        }

        [TestMethod]
        public void Ctor_EmptyStringArgument_ThrowExpectedException()
        {
            Action action = () => new FakeLifestyle(string.Empty);

            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>("Value can not be empty.", action);
        }

        [TestMethod]
        public void Ctor_EmptyStringArgument_ThrowExpectedArgumentName()
        {
            Action action = () => new FakeLifestyle(string.Empty);

            AssertThat.ThrowsWithParamName<ArgumentException>("name", action);
        }

        [TestMethod]
        public void GetInstance_OnProducerCreatedUsingLifestyleCreateProducer1_ReturnsExpectedInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            var producer = Lifestyle.Transient.CreateProducer<IPlugin, PluginImpl>(container);

            // This allows verifying whether the InstanceProducer's service type is set correctly
            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            // Act
            IPlugin instance = producer.GetInstance();

            // Assert
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), instance);
            AssertThat.IsInstanceOfType(typeof(PluginImpl), ((PluginDecorator)instance).Decoratee);
        }

        [TestMethod]
        public void GetInstance_OnProducerCreatedUsingLifestyleCreateProducer2_ReturnsExpectedInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            var producer = Lifestyle.Transient.CreateProducer<IPlugin>(() => new PluginImpl(), container);

            // This allows verifying whether the InstanceProducer's service type is set correctly
            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            // Act
            var instance = producer.GetInstance();

            // Assert
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), instance);
            AssertThat.IsInstanceOfType(typeof(PluginImpl), ((PluginDecorator)instance).Decoratee);
        }

        [TestMethod]
        public void GetInstance_OnProducerCreatedUsingLifestyleCreateProducer3_ReturnsExpectedInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            var producer = Lifestyle.Transient.CreateProducer(typeof(IPlugin), typeof(PluginImpl), container);

            // This allows verifying whether the InstanceProducer's service type is set correctly
            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            // Act
            var instance = producer.GetInstance();

            // Assert
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), instance);
            AssertThat.IsInstanceOfType(typeof(PluginImpl), ((PluginDecorator)instance).Decoratee);
        }

        [TestMethod]
        public void CreateRegistrationConcreteGeneric_WithNullArgument1_ThrowsArgumentNullException()
        {
            // Act
            Action action = () => Lifestyle.Transient.CreateRegistration(null, new Container());

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("concreteType", action);
        }

        [TestMethod]
        public void CreateRegistrationConcreteGeneric_WithNullArgument2_ThrowsArgumentNullException()
        {
            // Act
            Action action = () => Lifestyle.Transient.CreateRegistration(typeof(PluginImpl), null);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("container", action);
        }

        [TestMethod]
        public void CreateRegistrationConcreteGeneric_WithNullArgument_ThrowsArgumentNullException()
        {
            // Act
            Action action = () => Lifestyle.Transient.CreateRegistration<PluginImpl>(null);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("container", action);
        }

        [TestMethod]
        public void CreateRegistrationConcreteNonGeneric_WithValidArguments_Succeeds()
        {
            // Act
            var registration = Lifestyle.Transient.CreateRegistration(typeof(PluginImpl), new Container());

            // Assert
            Assert.IsNotNull(registration);
        }

        [TestMethod]
        public void CreateRegistrationConcreteGeneric_WithValidArguments_Succeeds()
        {
            // Act
            var registration = Lifestyle.Transient.CreateRegistration<PluginImpl>(new Container());

            // Assert
            Assert.IsNotNull(registration);
        }

        [TestMethod]
        public void CreateProducer_SuppliedWithOpenGenericServiceType_ThrowsExpectedException()
        {
            // Arrange
            Container container = new Container();
            Lifestyle lifestyle = Lifestyle.Transient;

            // Act
            Action action = () => lifestyle.CreateProducer(typeof(ICommandHandler<>), 
                typeof(StubCommandHandler), container);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied type ICommandHandler<TCommand> is an open generic type.",
                action);
            AssertThat.ThrowsWithParamName("serviceType", action);
        }

        [TestMethod]
        public void CreateProducer_SuppliedWithOpenGenericImplementationType_ThrowsExpectedException()
        {
            // Arrange
            Container container = new Container();
            Lifestyle lifestyle = Lifestyle.Transient;

            // Act
            Action action = () => lifestyle.CreateProducer(typeof(object),
                typeof(NullCommandHandler<>), container);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied type NullCommandHandler<T> is an open generic type.",
                action);
            AssertThat.ThrowsWithParamName("implementationType", action);
        }
        
        [TestMethod]
        public void CreateProducerTService_SuppliedWithOpenGenericImplementationType_ThrowsExpectedException()
        {
            // Arrange
            Container container = new Container();
            Lifestyle lifestyle = Lifestyle.Transient;

            // Act
            Action action = () => lifestyle.CreateProducer<object>(typeof(NullCommandHandler<>), container);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied type NullCommandHandler<T> is an open generic type.",
                action);
            AssertThat.ThrowsWithParamName("implementationType", action);
        }

        [TestMethod]
        public void CreateRegistrationTConcrete_SuppliedWithOpenGenericConcreteType_ThrowsExpectedException()
        {
            // Arrange
            Container container = new Container();
            Lifestyle lifestyle = Lifestyle.Transient;

            // Act
            Action action = () => lifestyle.CreateRegistration(typeof(NullCommandHandler<>), container);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied type NullCommandHandler<T> is an open generic type.",
                action);
            AssertThat.ThrowsWithParamName("concreteType", action);
        }

        [TestMethod]
        public void CreateRegistration_CalledWithValueType_ThrowsExpectedException()
        {
            // Arrange
            Container container = new Container();
            Lifestyle lifestyle = Lifestyle.Transient;

            // Act
            Action action = () => lifestyle.CreateRegistration(typeof(int), container);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied type Int32 is not a reference type. Only reference types are supported.",
                action);
        }

        [TestMethod]
        public void CreateRegistration_SuppliedWithOpenGenericImplementationType_ThrowsExpectedException()
        {
            // Arrange
            Container container = new Container();
            Lifestyle lifestyle = Lifestyle.Transient;

            // Act
            Action action = () => lifestyle.CreateRegistration(typeof(NullCommandHandler<>), container);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied type NullCommandHandler<T> is an open generic type.",
                action);
            AssertThat.ThrowsWithParamName("concreteType", action);
        }

        [TestMethod]
        public void CreateRegistrationFunc_SuppliedWithOpenGenericServiceType_ThrowsExpectedException()
        {
            // Arrange
            Container container = new Container();
            Lifestyle lifestyle = Lifestyle.Transient;

            // Act
            Action action = () => lifestyle.CreateRegistration(typeof(ICommandHandler<>), () => null, container);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied type ICommandHandler<TCommand> is an open generic type.",
                action);
            AssertThat.ThrowsWithParamName("serviceType", action);
        }

        private sealed class FakeLifestyle : Lifestyle
        {
            public FakeLifestyle(string name)
                : base(name)
            {
            }

            public override int Length
            {
                get { throw new NotImplementedException(); }
            }

            protected internal override Registration CreateRegistrationCore<TConcrete>(Container container)
            {
                throw new NotImplementedException();
            }

            protected internal override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
                Container container)
            {
                throw new NotImplementedException();
            }
        }
    }
}