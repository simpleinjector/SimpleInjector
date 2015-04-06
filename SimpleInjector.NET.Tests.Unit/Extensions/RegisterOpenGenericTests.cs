namespace SimpleInjector.Tests.Unit.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Extensions;
    
    /// <summary>Tests for testing RegisterOpenGeneric.</summary>
    [TestClass]
    public partial class RegisterOpenGenericTests
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

            AssertThat.IsInstanceOfType(typeof(ServiceImpl<int, string>), impl);
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

            AssertThat.IsInstanceOfType(typeof(ServiceImpl<int, string>), impl);
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
        public void RegisterOpenGeneric_WithClosedServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterOpenGeneric(typeof(IService<int, string>), typeof(ServiceImpl<,>));

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithClosedImplementation_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<int, int>));

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterOpenGeneric_WithNonRelatedTypes_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
                        
            // Act
            Action action = () => container.RegisterOpenGeneric(typeof(IService<,>), typeof(Func<,>));

            // Assert
            AssertThat.Throws<ArgumentException>(action);
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
            AssertThat.IsInstanceOfType(typeof(DefaultStuffDoer<string>), validator);
            AssertThat.IsInstanceOfType(typeof(ServiceImpl<string, int>), validator.Service);
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

            AssertThat.IsInstanceOfType(typeof(ServiceImpl<int, string>), impl);
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
        public void RegisterSingleOpenGeneric_WithClosedServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => 
                container.RegisterSingleOpenGeneric(typeof(IService<int, string>), typeof(ServiceImpl<,>));

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterSingleOpenGeneric_WithClosedImplementation_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => 
                container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(ServiceImpl<int, int>));

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterSingleOpenGeneric_WithNonRelatedTypes_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterSingleOpenGeneric(typeof(IService<,>), typeof(Func<,>));

            // Assert
            AssertThat.Throws<ArgumentException>(action);
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
            AssertThat.IsInstanceOfType(typeof(DefaultStuffDoer<string>), validator);
            AssertThat.IsInstanceOfType(typeof(ServiceImpl<string, int>), validator.Service);
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
            // Resolve an unregistered concrete non-generic type.
            container.GetInstance<ConcreteCommand>();
        }

        [TestMethod]
        public void GetRegistration_TypeSatisfyingGenericWhereConstraint_ReturnsInstanceProducer()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(AuditableEventEventHandler<>));

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(AuditableEventEventHandler<>));

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(AuditableEventEventHandler<>));

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(NewConstraintEventHandler<>));

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(NewConstraintEventHandler<>));

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(ClassConstraintEventHandler<>));

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(ClassConstraintEventHandler<>));

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(StructConstraintEventHandler<>));

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(StructConstraintEventHandler<>));

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(StructConstraintEventHandler<>));

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

            container.RegisterOpenGeneric(typeof(IDictionary<,>), typeof(MonoDictionary<>));

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

            // NullableProducer<T> : IProducer<Nullable<T>>, IProducer<IValidate<T>>, IProducer<double> where T : struct
            container.RegisterOpenGeneric(typeof(IProducer<>), typeof(NullableProducer<>));

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
            AssertThat.IsInstanceOfType(typeof(ServiceImplWithTypesArgsSwapped<int, object>), impl);
        }

        [TestMethod]
        public void RegisterOpenGeneric_RegisterOpenGenericWithImplementationWithTypeArgumentThatHasNoMapping_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterOpenGeneric(typeof(IDictionary<,>), typeof(SneakyMonoDictionary<,>));
            
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

            container.RegisterOpenGeneric(typeof(IQueryHandler<,>), typeof(QueryHandlerWithNestedType1<>));
            container.RegisterOpenGeneric(typeof(IQueryHandler<,>), typeof(QueryHandlerWithNestedType2<>));

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
            
            container.RegisterOpenGeneric(typeof(IDictionary<,>), implementationType);

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

            container.RegisterOpenGeneric(typeof(IInterface<,,>), parialOpenImplementationType);

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

            container.RegisterOpenGeneric(typeof(IInterface<,,>), parialOpenImplementationType);

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
            
            container.RegisterOpenGeneric(typeof(IDictionary<,>), implementationType);

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
            container.RegisterOpenGeneric(typeof(IFoo<>), typeof(Foo<,>));

            // Act
            var instance = container.GetInstance<IFoo<Baz>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Foo<Baz, Bar>), instance, "The RegisterOpenGeneric should be able to see that 'T2' is of type 'Bar'.");
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
                    For the container to be able to create ServiceImplWithMultipleCtors<TA, TB>, 
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
                    For the container to be able to create ServiceImplWithMultipleCtors<TA, TB>, 
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
                    The constructor of type DefaultStuffDoer<Boolean> 
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
                    The constructor of type DefaultStuffDoer<Boolean> contains the parameter 
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

        [TestMethod]
        public void GetInstance_OnRegisteredPartialGenericType1_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            var partialOpenGenericType = typeof(ClassConstraintEventHandler<>).MakeGenericType(typeof(List<>));

            container.RegisterOpenGeneric(typeof(IEventHandler<>), partialOpenGenericType);

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), partialOpenGenericType);

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), partialOpenGenericType);

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), partialOpenGenericType);

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), partialOpenGenericType);

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), partialOpenGenericType);

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), partialOpenGenericType);

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

            container.RegisterOpenGeneric(typeof(IEventHandler<>), partialOpenGenericType);

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
            Action action = () => container.RegisterOpenGeneric(
                typeof(IValidate<>), 
                typeof(ValidatorWithUnusedTypeArgument<,>));

            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "contains unresolvable type arguments",
                action,
                "Registration should fail, because the framework should detect that the implementation " +
                "contains a generic type argument that can never be resolved.");
        }

        [TestMethod]
        public void RegisterOpenGeneric_TwoEquivalentImplementationsOfTheSameInterface_Verifies()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>), 
                Lifestyle.Transient, c => c.ServiceType.GetGenericArguments().Single().GetType() == typeof(int));
            
            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate2<>),
                Lifestyle.Transient, c => c.ServiceType.GetGenericArguments().Single().GetType() == typeof(long));

            // Act
            container.Verify();

            // Assert
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

        [TestMethod]
        public void RegisterOpenGeneric_PredicateContext_ServiceTypeIsClosedImplentation()
        {
            bool called = false;

            // Arrange
            var container = ContainerFactory.New();
            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c =>
                    {
                        if (c.ServiceType.ContainsGenericParameter())
                        {
                            throw new InvalidOperationException("ServiceType should be a closed type");
                        }

                        called = true;
                        return true;
                    });

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.IsTrue(called, "Predicate was not called");
        }

        [TestMethod]
        public void RegisterOpenGeneric_PredicateContext_ImplementationTypeIsClosedImplentation()
        {
            bool called = false;

            // Arrange
            var container = ContainerFactory.New();
            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c =>
                    {
                        if (c.ImplementationType.ContainsGenericParameter())
                        {
                            throw new InvalidOperationException("ImplementationType should be a closed type");
                        }

                        called = true;
                        return true;
                    });

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.IsTrue(called, "Predicate was not called");
        }

        [TestMethod]
        public void RegisterOpenGeneric_TwoEquivalentImplementationsOfTheSameInterfaceWithValidPredicate_AppliesPredicate1()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c => c.ImplementationType.GetGenericArguments().Single() == typeof(int));

            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate2<>),
                Lifestyle.Transient, c => c.ImplementationType.GetGenericArguments().Single() == typeof(long));

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.IsNotNull(result);
            AssertThat.IsInstanceOfType(typeof(OpenGenericWithPredicate1<int>), result);
        }

        [TestMethod]
        public void RegisterOpenGeneric_TwoEquivalentImplementationsOfTheSameInterfaceWithValidPredicate_AppliesPredicate2()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c => c.ImplementationType.GetGenericArguments().Single() == typeof(int));

            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate2<>),
                Lifestyle.Transient, c => c.ImplementationType.GetGenericArguments().Single() == typeof(long));

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<long>>();

            // Assert
            Assert.IsNotNull(result);
            AssertThat.IsInstanceOfType(typeof(OpenGenericWithPredicate2<long>), result);
        }

        [TestMethod]
        public void RegisterOpenGeneric_TwoEquivalentImplementationsOfTheSameInterfaceWithOverlappingPredicate_ThrowsException1()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c => c.ImplementationType.GetGenericArguments().Single().FullName.StartsWith("System"));

            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate2<>),
                Lifestyle.Transient, c => c.ImplementationType.GetGenericArguments().Single().Namespace.StartsWith("System"));

            // Act
            Action action = () =>
                container.GetInstance<IOpenGenericWithPredicate<long>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Multiple observers of the ResolveUnregisteredType event",
                action,
                "GetInstance should fail because the framework should detect that more than one " +
                "implemention of the requested service.");
        }

        [TestMethod]
        public void RegisterOpenGeneric_TwoEquivalentImplementationsOfTheSameInterfaceWithOverlappingPredicate_ThrowsException2()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c => c.ImplementationType.GetGenericArguments().Single() == typeof(int));

            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate2<>),
                Lifestyle.Transient, c => c.ImplementationType.GetGenericArguments().Single().Namespace.StartsWith("System"));

            // Act
            var result1 = container.GetInstance<IOpenGenericWithPredicate<long>>();
            Action action = () => 
                container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Multiple observers of the ResolveUnregisteredType event",
                action,
                "GetInstance should fail because the framework should detect that more than one " +
                "implementation of the requested service.");
        }

        [TestMethod]
        public void RegisterOpenGeneric_TwoEquivalentImplementationsWithValidPredicate_UpdateHandledProperty()
        {
            bool handled = false;

            // Arrange
            var container = ContainerFactory.New();
            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c =>
                {
                    if (c.Handled)
                    {
                        throw new InvalidOperationException("The test assumes handled is false at this time.");
                    }

                    return c.ImplementationType.GetGenericArguments().Single() == typeof(int);
                });

            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate2<>),
                Lifestyle.Transient, c =>
                {
                    // this is the test - we are checking that c.handled changed between
                    // the registered Predicates for OpenGenericWithPredicate1<> and OpenGenericWithPredicate2<>
                    handled = c.Handled;
                    return c.ImplementationType.GetGenericArguments().Single() == typeof(long);
                });

            // Act
            handled = false;
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.IsTrue(handled);
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
            container.RegisterOpenGeneric(typeof(ICommandHandler<>), typeof(UpdateCommandHandler<,>));

            // Act
            var actualInstance = container.GetInstance<ICommandHandler<UpdateCommand<SpecialEntity>>>();

            // Assert
            AssertThat.IsInstanceOfType(expectedType, actualInstance);
        }
        
        [TestMethod]
        public void RegisterOpenGeneric_WithPartialOpenGenericServiceType_ThrowsExpectedMessage()
        {
            // Arrange
            string expectedMessage = @"
                The supplied type 'IService<Int32, TB>' is a partially closed generic type, which is not 
                supported as value of the openGenericServiceType parameter. 
                Instead, please supply the open-generic type 'IService<,>' and make the type supplied to 
                the openGenericImplementation parameter partially closed instead."
                .TrimInside();            

            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterOpenGeneric(
                typeof(IService<,>).MakePartialOpenGenericType(firstArgument: typeof(int)),
                typeof(ServiceImpl<,>));

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentException>("openGenericServiceType", action);
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                expectedMessage,
                action);
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