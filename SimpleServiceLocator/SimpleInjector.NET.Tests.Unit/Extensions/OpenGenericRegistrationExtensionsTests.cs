namespace SimpleInjector.Tests.Unit.Extensions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Extensions;

    public interface IStruct<T> where T : struct
    {
    }

    public interface IFoo<T>
    {
    }

    public interface IBar<T>
    {
    }

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

    [TestClass]
    public class OpenGenericRegistrationExtensionsTests
    {
        [TestMethod]
        public void RegisterOpenGeneric_WithValidArguments_ReturnsExpectedTypeOnGetInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Assert
            var impl = container.GetInstance<IService<int, string>>();

            Assert.IsInstanceOfType(impl, typeof(ServiceImpl<int, string>));
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithConcreteType_ReturnsExpectedTypeOnGetInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterOpenGeneric(typeof(ServiceImpl<,>), typeof(ServiceImpl<,>));

            // Assert
            var impl = container.GetInstance<ServiceImpl<int, string>>();

            Assert.IsInstanceOfType(impl, typeof(ServiceImpl<int, string>));
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithValidArguments_ReturnsNewInstanceOnEachRequest()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Act
            var instance1 = container.GetInstance<IService<int, string>>();
            var instance2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreNotEqual(instance1, instance2, "Transient objects are expected to be returned.");
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithValidArguments_RespectsGivenLifestyle1()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>), Lifestyle.Transient);

            // Act
            var instance1 = container.GetInstance<IService<int, string>>();
            var instance2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreNotEqual(instance1, instance2, "Transient objects are expected to be returned.");
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithValidArguments_RespectsGivenLifestyle2()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<,>), Lifestyle.Singleton);

            // Act
            var instance1 = container.GetInstance<IService<int, string>>();
            var instance2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreEqual(instance1, instance2, "Singleton object is expected to be returned.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterOpenGeneric_WithClosedServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterOpenGeneric(typeof(IService<int, string>), typeof(ServiceImpl<,>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterOpenGeneric_WithClosedImplementation_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<int, int>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterOpenGeneric_WithNonRelatedTypes_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(Func<,>));

            container.GetInstance<IService<int, string>>();
        }

        [TestMethod]
        public void GetInstance_WithDependedRegisterOpenGenericRegistrations_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

            // Act
            container.RegisterSingleOpenGeneric(typeof(IService<int, string>), typeof(ServiceImpl<,>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterSingleOpenGeneric_WithClosedImplementation_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<int, int>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterSingleOpenGeneric_WithNonRelatedTypes_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(Func<,>));
        }

        [TestMethod]
        public void GetInstance_WithDependedRegisterSingleOpenGenericRegistrations_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

            container.RegisterSingleOpenGeneric(typeof(IValidate<>), typeof(NullValidator<>));

            // Act
            container.GetInstance<IValidate<int>>();
            container.GetInstance<IValidate<double>>();
        }

        [TestMethod]
        public void GetInstance_OnUnregisteredNonGenericConcreteTypeWithRegisterOpenGenericRegistration_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

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
            var container = ContainerFactory.New();

            container.RegisterOpenGeneric(typeof(IProducer<>), typeof(NullableProducer<>));

            // Act
            var producer = container.GetRegistration(typeof(IProducer<int>));

            // Assert
            Assert.IsNull(producer, "resolving IProducer<int> should ignore NullableProducer<T>. Type: " +
                (producer ?? new object()).GetType().FullName);
        }

        [TestMethod]
        public void GetInstance_RegisterOpenGenericWithImplementationWithTypeArgumentsSwapped_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImplWithTypesArgsSwapped<,>));

            // Act
            var impl = container.GetInstance<IService<object, int>>();

            // Assert
            Assert.IsInstanceOfType(impl, typeof(ServiceImplWithTypesArgsSwapped<int, object>));
        }

        [TestMethod]
        public void GetRegistration_RegisterOpenGenericWithImplementationWithTypeArgumentThatHasNoMapping_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterOpenGeneric(typeof(IDictionary<,>), typeof(SneakyMonoDictionary<,>));

            // Act
            var producer = container.GetRegistration(typeof(IDictionary<int, object>));

            // Assert
            Assert.IsNull(producer, "Resolving IDictionary<int, object> should ignore " +
                "SneakyMonoDictionary<T, Unused> because there is no mapping to Unused.");
        }

        [TestMethod]
        public void GetInstance_RegisterOpenGenericWithImplementationThatContainsTypeArgumentInConstraint_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // class Baz : IBar<Bar>
            // class Foo<T1, T2> : IFoo<T1> where T1 : IBar<T2>
            container.RegisterOpenGeneric(typeof(IFoo<>), typeof(Foo<,>));

            // Act
            var instance = container.GetInstance<IFoo<Baz>>();

            // Assert
            Assert.IsInstanceOfType(instance, typeof(Foo<Baz, Bar>),
                "The RegisterOpenGeneric should be able to see that 'T2' is of type 'Bar'.");
        }

        [TestMethod]
        public void RegisterOpenGeneric_ImplementationWithMultipleConstructors_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                // Act
                container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImplWithMultipleCtors<,>));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(@"
                    For the container to be able to create 
                    OpenGenericRegistrationExtensionsTests+ServiceImplWithMultipleCtors<TA, TB>, 
                    it should contain exactly one public constructor, but it has 2.".TrimInside(),
                    ex.Message);
            }
        }

        [TestMethod]
        public void RegisterSingleOpenGeneric_ImplementationWithMultipleConstructors_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                // Act
                container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(ServiceImplWithMultipleCtors<,>));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(@"
                    For the container to be able to create 
                    OpenGenericRegistrationExtensionsTests+ServiceImplWithMultipleCtors<TA, TB>, 
                    it should contain exactly one public constructor, but it has 2.".TrimInside(),
                    ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_RegisterOpenGenericWithRegistrationWithMissingDependency_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // DefaultStuffDoer depends on IService<T, int> but this isn't registered.
            container.RegisterOpenGeneric(typeof(IDoStuff<>), typeof(DefaultStuffDoer<>));

            try
            {
                // Act
                container.GetInstance<IDoStuff<bool>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.ExceptionMessageContains(@"
                    There was an error in the registration of open generic type IDoStuff<T>. 
                    Failed to build a registration for type DefaultStuffDoer<Boolean>.".TrimInside(),
                    ex);

                AssertThat.ExceptionMessageContains(@"                                                                     
                    The constructor of the type DefaultStuffDoer<Boolean> 
                    contains the parameter of type IService<Boolean, Int32> with name 'service' that 
                    is not registered.".TrimInside(),
                    ex);
            }
        }

        [TestMethod]
        public void GetInstance_RegisterSingleOpenGenericWithRegistrationWithMissingDependency_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // DefaultStuffDoer depends on IService<T, int> but this isn't registered.
            container.RegisterSingleOpenGeneric(typeof(IDoStuff<>), typeof(DefaultStuffDoer<>));

            try
            {
                // Act
                container.GetInstance<IDoStuff<bool>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(@"
                    There was an error in the registration of open generic type IDoStuff<T>. 
                    Failed to build a registration for type DefaultStuffDoer<Boolean>."
                    .TrimInside(),
                    ex.Message);

                AssertThat.StringContains(@"                                                                     
                    The constructor of the type DefaultStuffDoer<Boolean> contains the parameter 
                    of type IService<Boolean, Int32>  with name 'service' that is not registered."
                    .TrimInside(),
                    ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_RegisterOpenGenericWithInheritingTypeConstraintsAndMatchingRequest_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceWhereTInIsTOut<,>));

            // Act
            // Since TIn : TOut and IDisposable : object, GetInstance should succeed.
            container.GetInstance<IService<IDisposable, object>>();
        }

        [TestMethod]
        public void GetRegistration_RegisterOpenGenericWithInheritingTypeConstraintsAndNonMatchingRequest_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceWhereTInIsTOut<,>));

            // Act
            var registration = container.GetRegistration(typeof(IService<object, IDisposable>));

            // Assert
            Assert.IsNull(registration,
                "Since TIn : TOut but object does not inherit from IDisposable, GetRegistration should return null.");
        }
        
        [TestMethod]
        public void GetInstance_SingleOpenGenericOnConcreteType_AlwaysReturnSameInstance()
        {
            // Arrange
            var container = new Container();

            // Service type is the same as implementation
            container.RegisterSingleOpenGeneric(
                typeof(NewConstraintEventHandler<>), 
                typeof(NewConstraintEventHandler<>));

            // Act
            var t1 = container.GetInstance<NewConstraintEventHandler<int>>();
            var t2 = container.GetInstance<NewConstraintEventHandler<int>>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(t1, t2));
        }

#if SILVERLIGHT
        [TestMethod]
        public void GetInstance_OnInternalTypeRegisteredAsOpenGeneric_ThrowsDescriptiveExceptionMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(InternalEventHandler<>));

            try
            {
                // Act
                container.GetInstance<IEventHandler<int>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.ExceptionMessageContains("InternalEventHandler<Int32>", ex);
                AssertThat.ExceptionMessageContains("The security restrictions of your application's " + 
                    "sandbox do not permit the creation of this type.", ex);    
            }
        }
#endif

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

        public sealed class ServiceImplWithMultipleCtors<TA, TB> : IService<TA, TB>
        {
            public ServiceImplWithMultipleCtors()
            {
            }

            public ServiceImplWithMultipleCtors(int x)
            {
            }
        }

        public sealed class ServiceImplWithDependency<TA, TB> : IService<TA, TB>
        {
            public ServiceImplWithDependency(IProducer<int> producer)
            {
            }
        }

        // The type constraint will prevent the type from being created when the arguments are ordered
        // incorrectly.
        public sealed class ServiceImplWithTypesArgsSwapped<B, A> : IService<A, B>
            where B : struct
            where A : class
        {
        }

        public sealed class NullValidator<T> : IValidate<T>
        {
            public void Validate(T instance)
            {
                // Do nothing.
            }
        }

        public class Bar
        {
        }

        public class Baz : IBar<Bar>
        {
        }

        public class Foo<T1, T2> : IFoo<T1> where T1 : IBar<T2>
        {
        }

        public class ServiceWhereTInIsTOut<TA, TB> : IService<TA, TB> where TA : TB
        {
        }

        internal class InternalEventHandler<TEvent> : IEventHandler<TEvent>
        {
            public void Handle(TEvent @event)
            {
            }
        }
    }

    public sealed class DefaultStuffDoer<T> : IDoStuff<T>
    {
        public DefaultStuffDoer(IService<T, int> service)
        {
            this.Service = service;
        }

        public IService<T, int> Service { get; private set; }
    }
}