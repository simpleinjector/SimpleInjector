using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.Extensions.Tests.Unit
{
    [TestClass]
    public class OpenGenericRegistrationExtensionsTests
    {
        // This is the open generic interface that will be used as service type.
        public interface IService<TA, TB>
        {
        }

        public interface IValidate<T>
        {
            void Validate(T instance);
        }

        public interface IDoStuff<T>
        {
            IService<T, int> Service { get; }
        }

        public interface IEventHandler<TEvent> 
        {
            void Handle(TEvent @event);
        }

        public interface IAuditableEvent 
        { 
        }

        public interface IProducer<T>
        {
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithValidArguments_ReturnsExpectedTypeOnGetInstance()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Assert
            var impl = container.GetInstance<IService<int, string>>();

            Assert.IsInstanceOfType(impl, typeof(ServiceImpl<int, string>));
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithValidArguments_ReturnsNewInstanceOnEachRequest()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Act
            var instance1 = container.GetInstance<IService<int, string>>();
            var instance2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreNotEqual(instance1, instance2, "Transient objects are expected to be returned.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterOpenGeneric_WithClosedServiceType_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterOpenGeneric(typeof(IService<int, string>), typeof(ServiceImpl<,>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterOpenGeneric_WithClosedImplementation_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<int, int>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterOpenGeneric_WithNonRelatedTypes_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(Func<,>));

            container.GetInstance<IService<int, string>>();
        }

        [TestMethod]
        public void GetInstance_WithDependedRegisterOpenGenericRegistrations_Succeeds()
        {
            // Arrange
            var container = new Container();

            // The DefaultValidator<T> contains an IService<T, int> as constructor argument.
            container.RegisterOpenGeneric(typeof(IDoStuff<>), typeof(DefaultStuffDoer<>));
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Act
            var validator = container.GetInstance<IDoStuff<string>>();

            // Assert
            Assert.IsInstanceOfType(validator, typeof(DefaultStuffDoer<string>));
            Assert.IsInstanceOfType(validator.Service, typeof(ServiceImpl<string, int>));
        }

        [TestMethod]
        public void RegisterSingleOpenGeneric_WithValidArguments_ReturnsExpectedTypeOnGetInstance()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Assert
            var impl = container.GetInstance<IService<int, string>>();

            Assert.IsInstanceOfType(impl, typeof(ServiceImpl<int, string>));
        }

        [TestMethod]
        public void RegisterSingleOpenGeneric_WithValidArguments_ReturnsNewInstanceOnEachRequest()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Act
            var instance1 = container.GetInstance<IService<int, string>>();
            var instance2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreEqual(instance1, instance2, "Singleton object is expected to be returned.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterSingleOpenGeneric_WithClosedServiceType_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterSingleOpenGeneric(typeof(IService<int, string>), typeof(ServiceImpl<,>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterSingleOpenGeneric_WithClosedImplementation_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<int, int>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterSingleOpenGeneric_WithNonRelatedTypes_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(Func<,>));
        }

        [TestMethod]
        public void GetInstance_WithDependedRegisterSingleOpenGenericRegistrations_Succeeds()
        {
            // Arrange
            var container = new Container();

            // The DefaultValidator<T> contains an IService<T, int> as constructor argument.
            container.RegisterSingleOpenGeneric(typeof(IDoStuff<>), typeof(DefaultStuffDoer<>));
            container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Act
            var validator = container.GetInstance<IDoStuff<string>>();

            // Assert
            Assert.IsInstanceOfType(validator, typeof(DefaultStuffDoer<string>));
            Assert.IsInstanceOfType(validator.Service, typeof(ServiceImpl<string, int>));
        }

        [TestMethod]
        public void GetInstance_CalledOnMultipleClosedImplementationsOfTypeRegisteredWithRegisterSingleOpenGeneric_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingleOpenGeneric(typeof(IValidate<>), typeof(NullValidator<>));

            // Act
            container.GetInstance<IValidate<int>>();
            container.GetInstance<IValidate<double>>();
        }

        [TestMethod]
        public void GetInstance_OnUnregisteredNonGenericConcreteTypeWithRegisterOpenGenericRegistration_Succeeds()
        {
            // Arrange
            var container = new Container();

            // RegisterOpenGeneric registers the ResolveUnregisteredType and this event will get raised before
            // trying to resolve an unregistered concrete type. Therefore it is important to check whether
            // the registered delegate will not fail when it is called with an non-generic type.
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Act
            // Resolve an unregisterd concrete non-generic type.
            container.GetInstance<ConcreteCommand>();
        }

        [TestMethod]
        public void GetRegistration_TypeSatisfyingGenericWhereConstraint_ReturnsInstanceProducer()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(WhereConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<AuditableEvent>));

            // Assert
            Assert.IsNotNull(producer);

            Assert.IsInstanceOfType(producer.GetInstance(), typeof(WhereConstraintEventHandler<AuditableEvent>),
                "if we resolve IEventHandler<AuditableEvent> then WhereConstraintEventHandler<AuditableEvent> should be activated");
        }

        [TestMethod]
        public void GetRegistration_TypeNotSatisfyingGenericWhereConstraint_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(WhereConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<DefaultConstructorEvent>));

            // Act
            Assert.IsNull(producer, "The Event type does not satisfy the type constraints on the " +
                "registered  event handler and the container should return null.");
        }

        [TestMethod]
        public void GetRegistration_TypeSatistyingGenericWhereConstraintWithStruct_ReturnsExpectedProducer()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(WhereConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<StructEvent>));

            // Assert
            Assert.IsNotNull(producer,
                "if we resolve IEventHandler<StructEvent> then WhereConstraintEventHandler<StructEvent> " +
                "should be activated");

            Assert.IsInstanceOfType(producer.GetInstance(), typeof(WhereConstraintEventHandler<StructEvent>),
                "if we resolve IEventHandler<StructEvent> then WhereConstraintEventHandler<StructEvent> " +
                "should be activated");
        }

        [TestMethod]
        public void GetRegistration_TypeSatisfyingGenericNewConstraint_ReturnsInstanceProducer()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(NewConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<DefaultConstructorEvent>));

            // Assert
            Assert.IsNotNull(producer);

            Assert.IsInstanceOfType(producer.GetInstance(), typeof(NewConstraintEventHandler<DefaultConstructorEvent>),
                "if we resolve IEventHandler<DefaultConstructorEvent> then NewConstraintEventHandler<DefaultConstructorEvent> should be activated");
        }

        [TestMethod]
        public void GetRegistration_TypeNotSatisfyingGenericNewConstraint_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(NewConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<NoDefaultConstructorEvent>));

            // Act
            Assert.IsNull(producer, "The Event type does not satisfy the type constraints on the " +
                "registered  event handler and the container should return null.");
        }
          
        [TestMethod]
        public void GetRegistration_TypeSatisfyingGenericClassConstraint_ReturnsInstanceProducer()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(ClassConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<ClassEvent>));

            // Assert
            Assert.IsNotNull(producer);

            Assert.IsInstanceOfType(producer.GetInstance(), typeof(ClassConstraintEventHandler<ClassEvent>),
                "if we resolve IEventHandler<ClassEvent> then ClassConstraintEventHandler<ClassEvent> should be activated");
        }

        [TestMethod]
        public void GetRegistration_TypeNotSatisfyingGenericClassConstraint_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(ClassConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<StructEvent>));

            // Act
            Assert.IsNull(producer, "The Event type does not satisfy the type constraints on the " +
                "registered  event handler and the container should return null.");
        }

        [TestMethod]
        public void GetRegistration_TypeSatisfyingGenericStructConstraint_ReturnsInstanceProducer()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(StructConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<StructEvent>));

            // Assert
            Assert.IsNotNull(producer);

            Assert.IsInstanceOfType(producer.GetInstance(), typeof(StructConstraintEventHandler<StructEvent>),
                "if we resolve IEventHandler<StructEvent> then StructConstraintEventHandler<StructEvent> should be activated");
        }

        [TestMethod]
        public void GetRegistration_TypeNotSatisfyingGenericStructConstraint_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(StructConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<ClassEvent>));

            // Act
            Assert.IsNull(producer, "The Event type does not satisfy the type constraints on the " +
                "registered  event handler and the container should return null.");
        }

        [TestMethod]
        public void GetRegistration_TypeNotSatisfyingGenericStructConstraint2_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(StructConstraintEventHandler<>));

            // Act
            // Although Nullable<T> is a value type, the actual C# 'struct' constraint is the CLR 
            // 'not nullable value type' constraint.
            var producer = container.GetRegistration(typeof(IEventHandler<StructEvent?>));

            // Act
            Assert.IsNull(producer, "The Event type does not satisfy the type constraints on the " +
                "registered  event handler and the container should return null.");
        }

        [TestMethod]
        public void GetRegistration_TypeSatisfyingTrickyGenericConstraint_ReturnsInstanceProducer()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IDictionary<,>), typeof(MonoDictionary<>));

            // Act
            var producer = container.GetRegistration(typeof(IDictionary<int, int>));

            // Assert
            Assert.IsNotNull(producer);

            Assert.IsInstanceOfType(producer.GetInstance(), typeof(MonoDictionary<int>),
                "if we resolve IDictionary<int, int> then MonoDictionary<int> should be activated");
        }

        [TestMethod]
        public void GetRegistration_TypeNotSatisfyingTrickyGenericConstraint_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IDictionary<,>), typeof(MonoDictionary<>));

            // Act
            var producer = container.GetRegistration(typeof(IDictionary<int, double>));

            // Assert
            Assert.IsNull(producer);
        }

        [TestMethod]
        public void GetRegistration_TypeSatisfyingNastyGenericConstraint_ReturnsInstanceProducer()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IProducer<>), typeof(NullableProducer<>));

            // Act
            var producer = container.GetRegistration(typeof(IProducer<int?>));

            // Assert
            Assert.IsNotNull(producer, 
                "if we resolve IProducer<int?> then NullableProducer<int> should be activated");

            Assert.IsInstanceOfType(producer.GetInstance(), typeof(NullableProducer<int>),
                "if we resolve IProducer<int?> then NullableProducer<int> should be activated");
        }

        [TestMethod]
        public void GetRegistration_TypeNotSatisfyingNastyGenericConstraint_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IProducer<>), typeof(NullableProducer<>));

            // Act
            var producer = container.GetRegistration(typeof(IProducer<int>));

            // Assert
            Assert.IsNull(producer, "resolving IProducer<int> should ignore NullableProducer<T>");
        }

        [TestMethod]
        public void GetInstance_RegisterOpenGenericWithImplementationWithTypeArgumentsSwapped_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImplWithTypesArgsSwapped<,>));

            // Act
            var impl = container.GetInstance<IService<object, int>>();

            // Assert
            Assert.IsInstanceOfType(impl, typeof(ServiceImplWithTypesArgsSwapped<int, object>));
        }

        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior()
        {
            // Arrange
            var container = new Container();

            container.RegisterOpenGeneric(typeof(IDictionary<,>), typeof(SneakyMonoDictionary<,>));

            // Act
            var producer = container.GetRegistration(typeof(IDictionary<int, object>));

            // Assert
            Assert.IsNull(producer, "resolving IDictionary<int, object> should ignore " +
                "SneakyMonoDictionary<T, Unused> because there is no mapping to Unused.");
        }

        public struct StructEvent : IAuditableEvent
        {
        }

        public class DefaultConstructorEvent
        {
            public DefaultConstructorEvent()
            {
            }
        }

        public class NoDefaultConstructorEvent
        {
            public NoDefaultConstructorEvent(IValidate<int> dependency)
            {
            }
        }

        public class ClassEvent
        {
        }

        public class AuditableEvent : IAuditableEvent
        {
        }

        public class WhereConstraintEventHandler<TEvent> : IEventHandler<TEvent> 
            where TEvent : IAuditableEvent
        {
            public void Handle(TEvent @event)
            {
            }
        }

        public class NewConstraintEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : new()
        {
            public void Handle(TEvent @event)
            {
            }
        }

        public class StructConstraintEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : struct
        {
            public void Handle(TEvent @event)
            {
            }
        }

        public class ClassConstraintEventHandler<TClassEvent> : IEventHandler<TClassEvent> 
            where TClassEvent : class
        {
            public void Handle(TClassEvent @event)
            {
            }
        }

        public class MonoDictionary<T> : Dictionary<T, T> 
        {
        }

        public class SneakyMonoDictionary<T, TUnused> : Dictionary<T, T>
        {
        }

        // Note: This class deliberately implements a second IProducer. This will verify wether the code can
        // handle types with multiple versions of the same interface.
        public class NullableProducer<T> : IProducer<T?>, IProducer<IValidate<T>>, IProducer<double>
            where T : struct 
        { 
        }

        public sealed class ServiceImpl<TA, TB> : IService<TA, TB>
        {
        }

        // The type constraint will prevent the type from being created when the arguments are ordered
        // incorrectly.
        public sealed class ServiceImplWithTypesArgsSwapped<B, A> : IService<A, B>
            where B : struct where A : class
        {
        }

        public sealed class DefaultStuffDoer<T> : IDoStuff<T>
        {
            public DefaultStuffDoer(IService<T, int> service)
            {
                this.Service = service;
            }

            public IService<T, int> Service { get; private set; }
        }

        public sealed class NullValidator<T> : IValidate<T>
        {
            public void Validate(T instance)
            {
                // Do nothing.
            }
        }
    }
}