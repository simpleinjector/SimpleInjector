namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests for testing registering open generic types.</summary>
    [TestClass]
    public partial class RegisterOpenGenericTests
    {
        [TestMethod]
        public void RegisterOpenGeneric_WithValidArguments_ReturnsExpectedTypeOnGetInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.Register(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Assert
            var impl = container.GetInstance<IService<int, string>>();

            AssertThat.IsInstanceOfType(typeof(ServiceImpl<int, string>), impl);
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithConcreteType_ReturnsExpectedTypeOnGetInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.Register(typeof(ServiceImpl<,>), typeof(ServiceImpl<,>));

            // Assert
            var impl = container.GetInstance<ServiceImpl<int, string>>();

            AssertThat.IsInstanceOfType(typeof(ServiceImpl<int, string>), impl);
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithValidArguments_ReturnsNewInstanceOnEachRequest()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), typeof(ServiceImpl<,>));

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

            container.Register(typeof(IService<,>), typeof(ServiceImpl<,>), Lifestyle.Transient);

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

            container.Register(typeof(IService<,>), typeof(ServiceImpl<,>), Lifestyle.Singleton);

            // Act
            var instance1 = container.GetInstance<IService<int, string>>();
            var instance2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreEqual(instance1, instance2, "Singleton object is expected to be returned.");
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithClosedServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Register(typeof(IService<int, string>), typeof(ServiceImpl<,>));

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterOpenGeneric_OpenServiceTypeClosedImplementation_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), typeof(ServiceImpl<int, int>));

            // Act
            container.GetInstance<IService<int, int>>();
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithNonRelatedTypes_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Register(typeof(IService<,>), typeof(Func<,>));

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void GetInstance_WithDependedRegisterOpenGenericRegistrations_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // The DefaultValidator<T> contains an IService<T, int> as constructor argument.
            container.Register(typeof(IDoStuff<>), typeof(DefaultStuffDoer<>));
            container.Register(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Act
            var validator = container.GetInstance<IDoStuff<string>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(DefaultStuffDoer<string>), validator);
            AssertThat.IsInstanceOfType(typeof(ServiceImpl<string, int>), validator.Service);
        }

        [TestMethod]
        public void RegisterSingleOpenGeneric_WithValidArguments_ReturnsExpectedTypeOnGetInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.Register(typeof(IService<,>), typeof(ServiceImpl<,>), Lifestyle.Singleton);

            // Assert
            var impl = container.GetInstance<IService<int, string>>();

            AssertThat.IsInstanceOfType(typeof(ServiceImpl<int, string>), impl);
        }

        [TestMethod]
        public void RegisterSingleOpenGeneric_WithValidArguments_ReturnsNewInstanceOnEachRequest()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IService<,>), typeof(ServiceImpl<,>), Lifestyle.Singleton);

            // Act
            var instance1 = container.GetInstance<IService<int, string>>();
            var instance2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreEqual(instance1, instance2, "Singleton object is expected to be returned.");
        }

        [TestMethod]
        public void RegisterSingleOpenGeneric_WithClosedServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () =>
                container.Register(typeof(IService<int, string>), typeof(ServiceImpl<,>), Lifestyle.Singleton);

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterSingleOpenGeneric_WithNonRelatedTypes_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Register(typeof(IService<,>), typeof(Func<,>), Lifestyle.Singleton);

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void GetInstance_WithDependedRegisterSingleOpenGenericRegistrations_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // The DefaultValidator<T> contains an IService<T, int> as constructor argument.
            container.Register(typeof(IDoStuff<>), typeof(DefaultStuffDoer<>), Lifestyle.Singleton);
            container.Register(typeof(IService<,>), typeof(ServiceImpl<,>), Lifestyle.Singleton);

            // Act
            var validator = container.GetInstance<IDoStuff<string>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(DefaultStuffDoer<string>), validator);
            AssertThat.IsInstanceOfType(typeof(ServiceImpl<string, int>), validator.Service);
        }

        [TestMethod]
        public void GetInstance_CalledOnMultipleClosedImplementationsOfTypeRegisteredWithRegisterSingleOpenGeneric_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IValidate<>), typeof(NullValidator<>), Lifestyle.Singleton);

            // Act
            container.GetInstance<IValidate<int>>();
            container.GetInstance<IValidate<double>>();
        }

        [TestMethod]
        public void GetInstance_OnUnregisteredNonGenericConcreteTypeWithRegisterOpenGenericRegistration_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Register registers the ResolveUnregisteredType and this event will get raised before
            // trying to resolve an unregistered concrete type. Therefore it is important to check whether
            // the registered delegate will not fail when it is called with an non-generic type.
            container.Register(typeof(IService<,>), typeof(ServiceImpl<,>));

            // Act
            // Resolve an unregistered concrete non-generic type.
            container.GetInstance<ConcreteCommand>();
        }

        [TestMethod]
        public void GetRegistration_TypeSatisfyingGenericWhereConstraint_ReturnsInstanceProducer()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IEventHandler<>), typeof(AuditableEventEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<AuditableEvent>));

            // Assert
            Assert.IsNotNull(producer);

            AssertThat.IsInstanceOfType(typeof(AuditableEventEventHandler<AuditableEvent>), producer.GetInstance(), "if we resolve IEventHandler<AuditableEvent> then WhereConstraintEventHandler<AuditableEvent> should be activated");
        }

        [TestMethod]
        public void GetRegistration_TypeNotSatisfyingGenericWhereConstraint_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IEventHandler<>), typeof(AuditableEventEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<DefaultConstructorEvent>));

            // Act
            Assert.IsNull(producer, "The Event type does not satisfy the type constraints on the " +
                "registered event handler and the container should return null.");
        }

        [TestMethod]
        public void GetRegistration_TypeSatistyingGenericWhereConstraintWithStruct_ReturnsExpectedProducer()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IEventHandler<>), typeof(AuditableEventEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<StructEvent>));

            // Assert
            Assert.IsNotNull(producer,
                "if we resolve IEventHandler<StructEvent> then WhereConstraintEventHandler<StructEvent> " +
                "should be activated");

            AssertThat.IsInstanceOfType(typeof(AuditableEventEventHandler<StructEvent>), producer.GetInstance(), "if we resolve IEventHandler<StructEvent> then WhereConstraintEventHandler<StructEvent> " +
                "should be activated");
        }

        [TestMethod]
        public void GetRegistration_TypeSatisfyingGenericNewConstraint_ReturnsInstanceProducer()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IEventHandler<>), typeof(NewConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<DefaultConstructorEvent>));

            // Assert
            Assert.IsNotNull(producer);

            AssertThat.IsInstanceOfType(typeof(NewConstraintEventHandler<DefaultConstructorEvent>), producer.GetInstance(), "if we resolve IEventHandler<DefaultConstructorEvent> then NewConstraintEventHandler<DefaultConstructorEvent> should be activated");
        }

        [TestMethod]
        public void GetRegistration_TypeNotSatisfyingGenericNewConstraint_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IEventHandler<>), typeof(NewConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<NoDefaultConstructorEvent>));

            // Act
            Assert.IsNull(producer, "The Event type does not satisfy the type constraints on the " +
                "registered event handler and the container should return null.");
        }

        [TestMethod]
        public void GetRegistration_TypeSatisfyingGenericClassConstraint_ReturnsInstanceProducer()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IEventHandler<>), typeof(ClassConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<ClassEvent>));

            // Assert
            Assert.IsNotNull(producer);

            AssertThat.IsInstanceOfType(typeof(ClassConstraintEventHandler<ClassEvent>), producer.GetInstance(), "if we resolve IEventHandler<ClassEvent> then ClassConstraintEventHandler<ClassEvent> should be activated");
        }

        [TestMethod]
        public void GetRegistration_TypeNotSatisfyingGenericClassConstraint_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IEventHandler<>), typeof(ClassConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<StructEvent>));

            // Act
            Assert.IsNull(producer, "The Event type does not satisfy the type constraints on the " +
                "registered event handler and the container should return null.");
        }

        [TestMethod]
        public void GetRegistration_TypeSatisfyingGenericStructConstraint_ReturnsInstanceProducer()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IEventHandler<>), typeof(StructConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<StructEvent>));

            // Assert
            Assert.IsNotNull(producer);

            AssertThat.IsInstanceOfType(typeof(StructConstraintEventHandler<StructEvent>), producer.GetInstance(), "if we resolve IEventHandler<StructEvent> then StructConstraintEventHandler<StructEvent> should be activated");
        }

        [TestMethod]
        public void GetRegistration_TypeNotSatisfyingGenericStructConstraint_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IEventHandler<>), typeof(StructConstraintEventHandler<>));

            // Act
            var producer = container.GetRegistration(typeof(IEventHandler<ClassEvent>));

            // Act
            Assert.IsNull(producer, "The Event type does not satisfy the type constraints on the " +
                "registered event handler and the container should return null.");
        }

        [TestMethod]
        public void GetRegistration_TypeNotSatisfyingGenericStructConstraint2_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IEventHandler<>), typeof(StructConstraintEventHandler<>));

            // Act
            // Although Nullable<T> is a value type, the actual C# 'struct' constraint is the CLR 
            // 'not nullable value type' constraint.
            var producer = container.GetRegistration(typeof(IEventHandler<StructEvent?>));

            // Act
            Assert.IsNull(producer, "The Event type does not satisfy the type constraints on the " +
                "registered event handler and the container should return null.");
        }

        [TestMethod]
        public void GetRegistration_TypeSatisfyingTrickyGenericConstraint_ReturnsInstanceProducer()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IDictionary<,>), typeof(MonoDictionary<>));

            // Act
            var producer = container.GetRegistration(typeof(IDictionary<int, int>));

            // Assert
            Assert.IsNotNull(producer);

            AssertThat.IsInstanceOfType(typeof(MonoDictionary<int>), producer.GetInstance(), "if we resolve IDictionary<int, int> then MonoDictionary<int> should be activated");
        }

        [TestMethod]
        public void GetRegistration_TypeNotSatisfyingTrickyGenericConstraint_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IDictionary<,>), typeof(MonoDictionary<>));

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

            // NullableProducer<T> : IProducer<Nullable<T>>, IProducer<IValidate<T>>, IProducer<double> where T : struct
            container.Register(typeof(IProducer<>), typeof(NullableProducer<>));

            // Act
            var producer = container.GetRegistration(typeof(IProducer<int?>));

            // Assert
            Assert.IsNotNull(producer,
                "if we resolve IProducer<int?> then NullableProducer<int> should be activated");

            AssertThat.IsInstanceOfType(typeof(NullableProducer<int>), producer.GetInstance(), "if we resolve IProducer<int?> then NullableProducer<int> should be activated");
        }

        [TestMethod]
        public void GetRegistration_TypeNotSatisfyingNastyGenericConstraint_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IProducer<>), typeof(NullableProducer<>));

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

            container.Register(typeof(IService<,>), typeof(ServiceImplWithTypesArgsSwapped<,>));

            // Act
            var impl = container.GetInstance<IService<object, int>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(ServiceImplWithTypesArgsSwapped<int, object>), impl);
        }

        [TestMethod]
        public void RegisterOpenGeneric_RegisterOpenGenericWithImplementationWithTypeArgumentThatHasNoMapping_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Register(typeof(IDictionary<,>), typeof(SneakyMonoDictionary<,>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "SneakyMonoDictionary<T, TUnused> contains unresolvable type arguments",
                action);
        }

        [TestMethod]
        public void GetInstance_MultipleOpenGenericRegistrationsWithNestedTypesForSameService_ResolvesInstancesCorrectly()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(IQueryHandler<,>), typeof(QueryHandlerWithNestedType1<>));
            container.Register(typeof(IQueryHandler<,>), typeof(QueryHandlerWithNestedType2<>));

            // Act
            var instance1 = container.GetInstance<IQueryHandler<GenericQuery1<string>, string>>();
            var instance2 = container.GetInstance<IQueryHandler<GenericQuery2<string>, string>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(QueryHandlerWithNestedType1<string>), instance1);
            AssertThat.IsInstanceOfType(typeof(QueryHandlerWithNestedType2<string>), instance2);
        }

        [TestMethod]
        public void GetRegistration_RegisterPartialOpenGenericWithImplementationWithTypeArgumentThatHasNoMappingFilledIn_ReturnsExpectedType()
        {
            // Arrange
            var container = ContainerFactory.New();

            // SneakyMonoDictionary<T, object>
            var implementationType = typeof(SneakyMonoDictionary<,>).MakeGenericType(
                typeof(SneakyMonoDictionary<,>).GetGenericArguments().First(),
                typeof(object));

            container.Register(typeof(IDictionary<,>), implementationType);

            // Act
            // SneakyMonoDictionary implements Dictionary<T, T>, so requesting this should succeed.
            var instance = container.GetInstance<IDictionary<int, int>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(SneakyMonoDictionary<int, object>), instance, "SneakyMonoDictionary implements Dictionary<T, T>, so requesting an IDictionary<int, int> " +
                "should succeed because we filled in TUnused with System.Object.");
        }

        [TestMethod]
        public void GetRegistration_PartialOpenGenericNastynessPart1_ReturnsExpectedType()
        {
            // Arrange
            var container = ContainerFactory.New();

            var openImplementationType = typeof(Implementation<,,,>);
            var arguments = openImplementationType.GetGenericArguments();

            // TestDictionary<X, object, string, Y> -> IInterface<X, X, Y>
            var parialOpenImplementationType =
                openImplementationType.MakeGenericType(arguments[0], typeof(object), typeof(string), arguments[3]);

            container.Register(typeof(IInterface<,,>), parialOpenImplementationType);

            // Act
            var instance = container.GetInstance<IInterface<int, int, double>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Implementation<int, object, string, double>), instance);
        }

        [TestMethod]
        public void GetRegistration_PartialOpenGenericNastynessPart2_ReturnsExpectedType()
        {
            // Arrange
            var container = ContainerFactory.New();

            var openImplementationType = typeof(Implementation<,,,>);
            var arguments = openImplementationType.GetGenericArguments();

            // TestDictionary<X, object, string, object> -> IInterface<X, X, object>
            var parialOpenImplementationType =
                openImplementationType.MakeGenericType(arguments[0], typeof(object), typeof(string), typeof(object));

            container.Register(typeof(IInterface<,,>), parialOpenImplementationType);

            // Act
            var instance = container.GetInstance<IInterface<int, int, object>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Implementation<int, object, string, object>), instance);
        }

        [TestMethod]
        public void GetRegistration_RegisterOpenGenericWithImplementationWithTypeArgumentThatHasNoMapping_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            var implementationType = typeof(SneakyMonoDictionary<,>).MakeGenericType(
                typeof(SneakyMonoDictionary<,>).GetGenericArguments().First(),
                typeof(object));

            container.Register(typeof(IDictionary<,>), implementationType);

            // Act
            var producer = container.GetRegistration(typeof(IDictionary<int, object>));

            // Assert
            Assert.IsNull(producer,
                "SneakyMonoDictionary implements Dictionary<T, T>, so there is no mapping from " +
                "IDictionary<int, object> to SneakyMonoDictionary<T, TUnused>, even with Unused filled in.");
        }

        [TestMethod]
        public void GetInstance_RegisterOpenGenericWithImplementationThatContainsTypeArgumentInConstraint_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // class Baz : IBar<Bar>
            // class Foo<T1, T2> : IFoo<T1> where T1 : IBar<T2>
            container.Register(typeof(IFoo<>), typeof(Foo<,>));

            // Act
            var instance = container.GetInstance<IFoo<Baz>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Foo<Baz, Bar>), instance,
                "Register should be able to see that 'T2' is of type 'Bar'.");
        }

        [TestMethod]
        public void RegisterOpenGeneric_ImplementationWithMultipleConstructors_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Register(typeof(IService<,>), typeof(ServiceImplWithMultipleCtors<,>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(@"
                For the container to be able to create ServiceImplWithMultipleCtors<TA, TB>
                it should have only one public constructor: it has 2."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterSingleOpenGeneric_ImplementationWithMultipleConstructors_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Register(
                typeof(IService<,>), 
                typeof(ServiceImplWithMultipleCtors<,>), 
                Lifestyle.Singleton);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(@"
                For the container to be able to create ServiceImplWithMultipleCtors<TA, TB> 
                it should have only one public constructor: it has 2.".TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_RegisterOpenGenericWithRegistrationWithMissingDependency_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // DefaultStuffDoer depends on IService<T, int> but this isn't registered.
            container.Register(typeof(IDoStuff<>), typeof(DefaultStuffDoer<>));

            // Act
            Action action = () => container.GetInstance<IDoStuff<bool>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The constructor of type DefaultStuffDoer<Boolean> contains the parameter with name 'service'
                and type IService<Boolean, Int32> that is not registered."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_RegisterSingleOpenGenericWithRegistrationWithMissingDependency_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // DefaultStuffDoer depends on IService<T, int> but this isn't registered.
            container.Register(typeof(IDoStuff<>), typeof(DefaultStuffDoer<>), Lifestyle.Singleton);

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
                    The constructor of type DefaultStuffDoer<Boolean> contains the parameter 
                    with name 'service' and type IService<Boolean, Int32> that is not registered."
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
            container.Register(typeof(IService<,>), typeof(ServiceWhereTInIsTOut<,>));

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
            container.Register(typeof(IService<,>), typeof(ServiceWhereTInIsTOut<,>));

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
            container.Register(
                typeof(NewConstraintEventHandler<>),
                typeof(NewConstraintEventHandler<>),
                Lifestyle.Singleton);

            // Act
            var t1 = container.GetInstance<NewConstraintEventHandler<int>>();
            var t2 = container.GetInstance<NewConstraintEventHandler<int>>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(t1, t2));
        }

        [TestMethod]
        public void GetInstance_OnRegisteredPartialGenericType1_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            var partialOpenGenericType = typeof(ClassConstraintEventHandler<>).MakeGenericType(typeof(List<>));

            container.Register(typeof(IEventHandler<>), partialOpenGenericType);

            // Act
            container.GetInstance<IEventHandler<List<int>>>();
        }

        [TestMethod]
        public void GetInstance_OnRegisteredPartialGenericType2_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // ClassConstraintEventHandler<Tuple<int, List<T>>>
            var partialOpenGenericType = typeof(ClassConstraintEventHandler<>).MakeGenericType(
                typeof(Tuple<,>).MakeGenericType(
                    typeof(int),
                    typeof(List<>)));

            container.Register(typeof(IEventHandler<>), partialOpenGenericType);

            // Act
            var registration = container.GetRegistration(typeof(IEventHandler<Tuple<int, List<double>>>));

            // Assert
            Assert.IsNotNull(registration);
        }

        [TestMethod]
        public void GetRegistration_OnRegisteredPartialGenericTypeThatDoesNotMatch1_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            var partialOpenGenericType = typeof(ClassConstraintEventHandler<>).MakeGenericType(typeof(List<>));

            container.Register(typeof(IEventHandler<>), partialOpenGenericType);

            // Act
            var registration = container.GetRegistration(typeof(IEventHandler<Collection<int>>));

            // Assert
            Assert.IsNull(registration, "The type does not match and should not be resolved.");
        }

        [TestMethod]
        public void GetRegistration_OnRegisteredPartialGenericTypeThatDoesNotMatch2_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // ClassConstraintEventHandler<Tuple<int, List<T>>>
            var partialOpenGenericType = typeof(ClassConstraintEventHandler<>).MakeGenericType(
                typeof(Tuple<,>).MakeGenericType(
                    typeof(int),
                    typeof(List<>)));

            container.Register(typeof(IEventHandler<>), partialOpenGenericType);

            // Act
            var registration = container.GetRegistration(typeof(IEventHandler<Tuple<double, List<int>>>));

            // Assert
            Assert.IsNull(registration, "The type does not match and should not be resolved.");
        }

        [TestMethod]
        public void GetRegistration_OnRegisteredPartialGenericTypeThatDoesNotMatch3_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // ClassConstraintEventHandler<Tuple<int, List<T>>>
            var partialOpenGenericType = typeof(ClassConstraintEventHandler<>).MakeGenericType(
                typeof(Tuple<,>).MakeGenericType(
                    typeof(int),
                    typeof(List<>)));

            container.Register(typeof(IEventHandler<>), partialOpenGenericType);

            // Act
            var registration = container.GetRegistration(typeof(IEventHandler<Tuple<double, object>>));

            // Assert
            Assert.IsNull(registration, "The type does not match and should not be resolved.");
        }

        [TestMethod]
        public void GetRegistration_OnRegisteredPartialGenericTypeThatDoesNotMatch4_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // ClassConstraintEventHandler<Tuple<Nullable<T>, object>>
            var partialOpenGenericType = typeof(ClassConstraintEventHandler<>).MakeGenericType(
                typeof(Tuple<,>).MakeGenericType(
                    typeof(Nullable<>),
                    typeof(object)));

            container.Register(typeof(IEventHandler<>), partialOpenGenericType);

            // Act
            var registration = container.GetRegistration(typeof(IEventHandler<Tuple<int?, List<int>>>));

            // Assert
            Assert.IsNull(registration, "The type does not match and should not be resolved.");
        }

        [TestMethod]
        public void GetRegistration_OnRegisteredPartialGenericTypeThatDoesNotMatch5_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // ClassConstraintEventHandler<Tuple<Nullable<T>, List<int>>>
            var partialOpenGenericType = typeof(ClassConstraintEventHandler<>).MakeGenericType(
                typeof(Tuple<,>).MakeGenericType(
                    typeof(Nullable<>),
                    typeof(List<int>)));

            container.Register(typeof(IEventHandler<>), partialOpenGenericType);

            // Act
            var registration = container.GetRegistration(typeof(IEventHandler<Tuple<int?, Collection<int>>>));

            // Assert
            Assert.IsNull(registration, "The type does not match and should not be resolved.");
        }

        [TestMethod]
        public void GetRegistration_OnRegisteredPartialGenericTypeThatDoesNotMatch6_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // ClassConstraintEventHandler<Tuple<Nullable<T>, List<int>>>
            var partialOpenGenericType = typeof(ClassConstraintEventHandler<>).MakeGenericType(
                typeof(Tuple<,>).MakeGenericType(
                    typeof(Nullable<>),
                    typeof(List<int>)));

            container.Register(typeof(IEventHandler<>), partialOpenGenericType);

            // Act
            var registration = container.GetRegistration(typeof(IEventHandler<Tuple<int?, string>>));

            // Assert
            Assert.IsNull(registration, "The type does not match and should not be resolved.");
        }

        [TestMethod]
        public void RegisterOpenGeneric_SupplyingATypeWithAGenericArgumentThatCanNotBeMappedToTheBaseType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            // ValidatorWithUnusedTypeArgument<T, TUnused>
            Action action = () => container.Register(
                typeof(IValidate<>),
                typeof(ValidatorWithUnusedTypeArgument<,>));

            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "contains unresolvable type arguments",
                action,
                "Registration should fail, because the framework should detect that the implementation " +
                "contains a generic type argument that can never be resolved.");
        }

        [TestMethod]
        public void ExtensionHelper_ContainsGenericParameter_WorksAsExpected()
        {
            // Arrange
            Type open = typeof(OpenGenericWithPredicate1<>);
            Type closed = typeof(OpenGenericWithPredicate1<int>);

            // Assert
            Assert.IsTrue(open.ContainsGenericParameter());
            Assert.IsFalse(closed.ContainsGenericParameter());
        }

        // This is a regression test. This test fails on .NET 4.0 and 4.5 builds (but not on PCL).
        [TestMethod]
        public void GetInstance_RegistrationOfTypeWithDeductableTypeArgument_ResolvesTheTypeCorrectly()
        {
            // Arrange
            Type expectedType =
                typeof(UpdateCommandHandler<SpecialEntity, UpdateCommand<SpecialEntity>>);

            var container = new Container();

            // UpdateCommandHandler<TEntity, TCommand> has generic type constraints, allowing the TEntity to 
            // be deduced by the IComandHandler<T> interface.
            container.Register(typeof(ICommandHandler<>), typeof(UpdateCommandHandler<,>));

            // Act
            var actualInstance = container.GetInstance<ICommandHandler<UpdateCommand<SpecialEntity>>>();

            // Assert
            AssertThat.IsInstanceOfType(expectedType, actualInstance);
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithPartialOpenGenericServiceType_ThrowsExpectedMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Register(
                typeof(IService<,>).MakePartialOpenGenericType(firstArgument: typeof(int)),
                typeof(ServiceImpl<,>));

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentException>("serviceType", action);

            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(@"
                The supplied type 'IService<Int32, TB>' is a partially-closed generic type, which is not 
                supported by this method. Please supply the open generic type 'IService<,>' instead."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterOpenGeneric_ImplementationThatOverlapsWithPreviousNonGenericRegistration_ThrowsExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IGeneric<int>, GenericType<int>>();

            // Act
            Action action = () => container.Register(typeof(IGeneric<>), typeof(GenericType<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "that overlaps with the registration for GenericType<T>",
                action);
        }

        [TestMethod]
        public void RegisterConditional_ImplementationThatOverlapsWithPreviousNonGenericRegistration_StillSucceeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IGeneric<int>, GenericType<int>>();

            // Act
            // It's impossible to check at this stage whether there is overlap, so we need to do this at the
            // time we build the object graph.
            container.RegisterConditional(typeof(IGeneric<>), typeof(GenericClassType<>), c => true);
        }

        [TestMethod]
        public void RegisterClosedGeneric_ImplementationThatOverlapsWithPreviousOpenGenericRegistration_ThrowsExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IGeneric<>), typeof(GenericType<>));

            // Act
            Action action = () => container.Register<IGeneric<int>, GenericType<int>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                There is already an open generic registration for IGeneric<T> (with implementation 
                GenericType<T>) that overlaps with the registration of IGeneric<Int32> that you are trying to 
                make. If your intention is to use GenericType<T> as fallback registration, please instead
                call: RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), c => !c.Handled)."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterClosedGeneric_ImplementationThatOverlapsWithPreviousClosedGenericRegistration_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IGeneric<int>), typeof(GenericType<int>));

            // Act
            Action action = () => container.Register(typeof(IGeneric<int>), typeof(GenericType<int>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                Type IGeneric<Int32> has already been registered. If your intention is to resolve a collection 
                of IGeneric<Int32> implementations, use the RegisterCollection overloads."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterClosedGeneric_ImplementationThatOverlapsWithPreviousPartialOpenGenericRegistrationW_ThrowsExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IGeneric<>), typeof(GenericType<>).MakeGenericType(typeof(List<>)));

            // Act
            Action action = () => container.Register<IGeneric<List<int>>, GenericType<List<int>>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                There is already an open generic registration for IGeneric<T> (with implementation 
                GenericType<List<T>>) that overlaps with the registration of IGeneric<List<Int32>> that you 
                are trying to make. If your intention is to use GenericType<List<T>> as fallback 
                registration, please instead call: 
                RegisterConditional(typeof(IGeneric<>), typeof(GenericType<>), c => !c.Handled)."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterNonGeneric_ImplementationThatOverlapsWithPreviousOpenGenericRegistration_ThrowsExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IGeneric<>), typeof(GenericType<>));

            // Act
            Action action = () => container.Register<IGeneric<int>, IntGenericType>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "overlaps with the registration of IGeneric<Int32>",
                action);
        }

        [TestMethod]
        public void RegisterOpenGeneric_ImplementationThatDoesntOverlapsWithPreviousNonGenericRegistrationBecauseOfTypeConstraint_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IGeneric<int>, GenericType<int>>();

            container.Register(typeof(IGeneric<>), typeof(GenericClassType<>));

            // Act
            container.GetInstance<IGeneric<int>>();
            container.GetInstance<IGeneric<string>>();
        }

        [TestMethod]
        public void RegisterOpenGeneric_ImplementationWithTypeConstraintThatOverlapsWithPreviousNonGenericRegistration_ThrowsExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IGeneric<string>, GenericType<string>>();

            // It would be lovely if we would be able to detect that these two registrations overlapped, but
            // this too complex to do. So we check during resolve.
            container.Register(typeof(IGeneric<>), typeof(GenericClassType<>));

            // Act
            Action action = () => container.GetInstance<IGeneric<string>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Multiple applicable registrations found for IGeneric<String>",
                action);
        }

        [TestMethod]
        public void RegisterOpenGeneric_ImplementationWithTypeConstraintThatOverlapsWithPreviousOpenGenericRegistration_ThrowsExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IGeneric<>), typeof(GenericType<>));

            // Act
            Action action = () => container.Register(typeof(IGeneric<>), typeof(GenericClassType<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "overlaps with the registration",
                action);
        }

        [TestMethod]
        public void RegisterOpenGeneric_ImplementationThatOverlapsWithPreviousOpenGenericRegistrationWithTypeConstraint_ThrowsExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IGeneric<>), typeof(GenericClassType<>));

            // Act
            Action action = () => container.Register(typeof(IGeneric<>), typeof(GenericType<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "overlaps with the registration for GenericType<T>",
                action);
        }

        [TestMethod]
        public void RegisterOpenGeneric_ImplementationThatOverlapsWithPreviousOpenGenericRegistrationForSameType_ThrowsExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IGeneric<>), typeof(GenericClassType<>));

            // Act
            Action action = () => container.Register(typeof(IGeneric<>), typeof(GenericClassType<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "overlaps with the registration for GenericClassType<TClass>",
                action);
        }

        [TestMethod]
        public void RegisterOpenGeneric_MultipleImplementationsWithNonOverlappingTypeConstraints_Succeed()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IGeneric<>), typeof(GenericClassType<>));
            container.Register(typeof(IGeneric<>), typeof(GenericStructType<>));

            // Act
            container.GetInstance<IGeneric<int>>();
            container.GetInstance<IGeneric<string>>();
        }

        [TestMethod]
        public void RegisterOpenGeneric_MultipleRegistrationsWithOverlappingGenericTypeConstraints_ThrowsExceptionWhenResolved()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IGeneric<>), typeof(GenericDisposableClassType<>));

            // These two registrations will technically overlap and in this case resolving a IGeneric<T> where
            // the T implements IDisposable will never work, because the next registration will get applied
            // as well. Still, we allow this scenario, because it is really hard to check these conditions.
            // So instead we check this when the object graph is built.
            container.Register(typeof(IGeneric<>), typeof(GenericClassType<>));

            // Act
            Action action = () => container.GetInstance<IGeneric<IDisposable>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Multiple applicable registrations found for IGeneric<IDisposable>.",
                action);
        }
        
        // This is a regression test: This is a bug in v2.8's RegisterOpenGeneric extension method.
        [TestMethod]
        public void GetInstance_RegisterConditionalWithTypeWithCyclicDependency_DoesNotCauseAStackOverflow()
        {
            // Arrange
            int recursiveCount = 0;

            var container = new Container();

            container.RegisterConditional(typeof(ICommandHandler<>), typeof(CyclicDependencyCommandHandler<>), c =>
            {
                if (recursiveCount++ > 10)
                {
                    Assert.Fail("Recursive loop detected.");
                }

                return !c.Handled;
            });

            // Act
            Action action = () => container.GetInstance(typeof(ICommandHandler<RealCommand>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "CyclicDependencyCommandHandler<RealCommand> is directly or indirectly depending on itself",
                action);
        }

        // This verifies a bug in v3.2.2 reported here: https://github.com/simpleinjector/SimpleInjector/issues/316
        [TestMethod]
        public void GetInstance_TypeArgumentWithConstraintThatCanResultInMultipleVersionsOfOtherArguments_Succeeds()
        {
            // Arrange
            var container = new Container();
                        
            container.Register(typeof(IQueryDispatcher<,>), typeof(QueryDispatcher<,>));
            container.RegisterCollection(typeof(IQueryHandler<,>), new[] 
            {
                typeof(MultipleResultsIntQueryHandler),
                typeof(MultipleResultsBoolQueryHandler)
            });

            // Act
            // NOTE: MultipleResultsQuery implements both IQuery<bool> and IQuery<int>.
            container.GetInstance<IQueryDispatcher<MultipleResultsQuery, int>>();
            container.GetInstance<IQueryDispatcher<MultipleResultsQuery, bool>>();
        }

        [TestMethod]
        public void Register_OverridingNonConditionalOpenGenericTypeWithClassTypeConstraintOnAbstraction_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(IClassConstraintedGeneric<>), typeof(ClassConstraintedGeneric<>));

            container.Options.AllowOverridingRegistrations = true;

            // Act
            container.Register(typeof(IClassConstraintedGeneric<>), typeof(ClassConstraintedGeneric2<>));

            // Assert
            AssertThat.IsInstanceOfType(
                expectedType: typeof(ClassConstraintedGeneric2<object>),
                actualInstance: container.GetInstance<IClassConstraintedGeneric<object>>());
        }

        [TestMethod]
        public void Register_OverridingNonConditionalOpenGenericTypeWithNewTypeConstraintOnAbstraction_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(INewConstraintedGeneric<>), typeof(NewConstraintedGeneric1<>));

            container.Options.AllowOverridingRegistrations = true;

            // Act
            container.Register(typeof(INewConstraintedGeneric<>), typeof(NewConstraintedGeneric2<>));

            // Assert
            AssertThat.IsInstanceOfType(
                expectedType: typeof(NewConstraintedGeneric2<int>),
                actualInstance: container.GetInstance<INewConstraintedGeneric<int>>());
        }
    }

    public interface IQueryDispatcher<TQuery, TResult> { }

    public class QueryDispatcher<TQuery, TResult> : IQueryDispatcher<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        public QueryDispatcher(IEnumerable<IQueryHandler<TQuery, TResult>> collection) { }
    }

    public class MultipleResultsQuery : IQuery<bool>, IQuery<int> { }

    public class MultipleResultsIntQueryHandler : IQueryHandler<MultipleResultsQuery, int> { }
    public class MultipleResultsBoolQueryHandler : IQueryHandler<MultipleResultsQuery, bool> { }

    public class CyclicDependencyCommandHandler<TCommand> : ICommandHandler<TCommand>
    {
        private readonly ICommandHandler<TCommand> recursive;

        public CyclicDependencyCommandHandler(ICommandHandler<TCommand> recursive)
        {
            this.recursive = recursive;
        }

        public void Handle(TCommand command)
        {
        }
    }

    public sealed class DefaultStuffDoer<T> : IDoStuff<T>
    {
        public DefaultStuffDoer(IService<T, int> service)
        {
            this.Service = service;
        }

        public IService<T, int> Service { get; }
    }

    public class SpecialEntity
    {
    }

    public class UpdateCommand<TEntity>
    {
    }

    public class UpdateCommandHandler<TEntity, TCommand> : ICommandHandler<TCommand>
        where TEntity : SpecialEntity
        where TCommand : UpdateCommand<TEntity>
    {
        public void Handle(TCommand command)
        {
        }
    }
}