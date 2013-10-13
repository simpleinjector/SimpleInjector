namespace SimpleInjector.Tests.Unit.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;
    using SimpleInjector.Extensions;

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

            // NullableProducer<T> : IProducer<Nullable<T>>, IProducer<IValidate<T>>, IProducer<double> where T : struct
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
        public void RegisterOpenGeneric_RegisterOpenGenericWithImplementationWithTypeArgumentThatHasNoMapping_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Atca
            Action action = () => container.RegisterOpenGeneric(typeof(IDictionary<,>), typeof(SneakyMonoDictionary<,>));
            
            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "SneakyMonoDictionary<T, TUnused> contains unresolvable type arguments",
                action);
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
            Assert.IsInstanceOfType(instance, typeof(SneakyMonoDictionary<int, object>),
                "SneakyMonoDictionary implements Dictionary<T, T>, so requesting an IDictionary<int, int> " +
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
            Assert.IsInstanceOfType(instance, typeof(Implementation<int, object, string, double>));
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
            Assert.IsInstanceOfType(instance, typeof(Implementation<int, object, string, object>));
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

        [TestMethod]
        public void GetAllInstances_RegisterAllOpenGenericWithTypeConstraints_ResolvesExpectedTypes1()
        {
            // Arrange
            var registeredTypes = new[]
            {
                typeof(NewConstraintEventHandler<>),
                typeof(ClassConstraintEventHandler<>), 
                typeof(StructConstraintEventHandler<>)
            };

            var expectedTypes = new[] 
            {
                typeof(NewConstraintEventHandler<ClassEvent>),
                typeof(ClassConstraintEventHandler<ClassEvent>)
            };

            // Assert
            Assert_RegisterAllOpenGenericResultsInExpectedListOfTypes<IEventHandler<ClassEvent>>(
                registeredTypes, expectedTypes);
        }

        [TestMethod]
        public void GetAllInstances_RegisterAllOpenGenericWithTypeConstraints_ResolvesExpectedTypes2()
        {
            // Arrange
            var registeredTypes = new[]
            {
                typeof(NewConstraintEventHandler<>),
                typeof(ClassConstraintEventHandler<>), 
                typeof(StructConstraintEventHandler<>)
            };

            var expectedTypes = new[] 
            {
                typeof(NewConstraintEventHandler<StructEvent>),
                typeof(StructConstraintEventHandler<StructEvent>)
            };

            // Assert
            Assert_RegisterAllOpenGenericResultsInExpectedListOfTypes<IEventHandler<StructEvent>>(
                registeredTypes, expectedTypes);
        }

        [TestMethod]
        public void GetAllInstances_RegisterAllOpenGenericWithTypeConstraints_ResolvesExpectedTypes3()
        {
            // Arrange
            var registeredTypes = new[]
            {
                typeof(NewConstraintEventHandler<>),
                typeof(ClassConstraintEventHandler<>), 
                typeof(StructConstraintEventHandler<>)
            };

            var expectedTypes = new[] 
            {
                typeof(ClassConstraintEventHandler<NoDefaultConstructorEvent>)
            };

            // Assert
            Assert_RegisterAllOpenGenericResultsInExpectedListOfTypes<IEventHandler<NoDefaultConstructorEvent>>(
                registeredTypes, expectedTypes);
        }

        [TestMethod]
        public void GetAllInstances_RegisterAllOpenGenericWithoutLifestyleParameter_RegistersAsTransient()
        {            
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAllOpenGeneric(typeof(IEventHandler<>), typeof(ClassConstraintEventHandler<>));

            // Act
            var instance1 = container.GetAllInstances<IEventHandler<ClassEvent>>().Single();
            var instance2 = container.GetAllInstances<IEventHandler<ClassEvent>>().Single();

            // Assert
            Assert.AreNotSame(instance1, instance2, "Transient was expected.");
        }

        [TestMethod]
        public void GetAllInstances_RegisterAllOpenGenericWithLifestyleParameter_RegistersAccordingToLifestyle1()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAllOpenGeneric(typeof(IEventHandler<>), Lifestyle.Transient, 
                typeof(ClassConstraintEventHandler<>));

            // Act
            var instance1 = container.GetAllInstances<IEventHandler<ClassEvent>>().Single();
            var instance2 = container.GetAllInstances<IEventHandler<ClassEvent>>().Single();

            // Assert
            Assert.AreNotSame(instance1, instance2, "Transient was expected.");
        }

        [TestMethod]
        public void GetAllInstances_RegisterAllOpenGenericWithLifestyleParameter_RegistersAccordingToLifestyle2()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAllOpenGeneric(typeof(IEventHandler<>), Lifestyle.Singleton,
                typeof(ClassConstraintEventHandler<>));

            // Act
            var instance1 = container.GetAllInstances<IEventHandler<ClassEvent>>().Single();
            var instance2 = container.GetAllInstances<IEventHandler<ClassEvent>>().Single();

            // Assert
            Assert.AreSame(instance1, instance2, "Singleton was expected.");
        }
        
        [TestMethod]
        public void RegisterAllOpenGeneric_SuppliedWithIncompatible_ThrowsExpectedException()
        {
            // Arrange
            Type invalidType = typeof(NullValidator<>);

            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterAllOpenGeneric(typeof(IEventHandler<>), invalidType);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "NullValidator<T> does not implement IEventHandler<TEvent>.", action);
        }

        [TestMethod]
        public void RegisterAllOpenGeneric_SuppliedWithNonGenericType_ThrowsExpectedException()
        {
            // Arrange
            Type invalidType = typeof(NonGenericEventHandler);

            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterAllOpenGeneric(typeof(IEventHandler<>), invalidType);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "NonGenericEventHandler is not an open generic type.", action);
        }

        [TestMethod]
        public void RegisterAllOpenGeneric_SuppliedWithAClosedGenericType_ThrowsExpectedException()
        {
            // Arrange
            Type invalidType = typeof(ClassConstraintEventHandler<object>);

            var container = ContainerFactory.New();
            
            // Act
            Action action = () => container.RegisterAllOpenGeneric(typeof(IEventHandler<>), invalidType);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "ClassConstraintEventHandler<Object> is not an open generic type.", action);
        }

        [TestMethod]
        public void RegisterAllOpenGeneric_CalledWithAbstractType_ThrowsExpectedException()
        {
            // Arrange
            Type invalidType = typeof(AbstractEventHandler<>);

            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterAllOpenGeneric(typeof(IEventHandler<>), invalidType);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "AbstractEventHandler<TEvent> is not a concrete type.", action);
        }

        [TestMethod]
        public void RegisterAllOpenGeneric_WithNullContainerParameter_ThrowsArgumentNullException()
        {
            // Arrange
            Container invalidContainer = null;

            // Act
            Action action = () => invalidContainer.RegisterAllOpenGeneric(typeof(int), typeof(int));

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("container", action);
        }

        [TestMethod]
        public void RegisterAllOpenGeneric_WithNullOpenGenericServiceTypeParameter_ThrowsArgumentNullException()
        {
            // Arrange
            Type invalidOpenGenericServiceType = null;

            // Act
            Action action = () => 
                (new Container()).RegisterAllOpenGeneric(invalidOpenGenericServiceType, typeof(int));

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("openGenericServiceType", action);
        }

        [TestMethod]
        public void RegisterAllOpenGeneric_WithNullOpenGenericImplementationsParameter_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<Type> invalidOpenGenericImplementations = null;

            // Act
            Action action = () =>
                (new Container()).RegisterAllOpenGeneric(typeof(int), invalidOpenGenericImplementations);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("openGenericImplementations", action);
        }

        [TestMethod]
        public void RegisterAllOpenGeneric_WithNullLifestyleParameter_ThrowsArgumentNullException()
        {
            // Arrange
            Lifestyle invalidLifestyle = null;

            // Act
            Action action = () =>
                (new Container()).RegisterAllOpenGeneric(typeof(int), invalidLifestyle, typeof(int));

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("lifestyle", action);
        }

        [TestMethod]
        public void RegisterAllOpenGeneric_WithEmptyOpenGenericImplementationsParameter_ThrowsArgumentException()
        {
            // Arrange
            IEnumerable<Type> invalidOpenGenericImplementations = Enumerable.Empty<Type>();

            // Act
            Action action = () =>
                (new Container()).RegisterAllOpenGeneric(typeof(int), invalidOpenGenericImplementations);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentException>("openGenericImplementations", action);
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied collection should contain atleast one element.", action);
        }
        
        [TestMethod]
        public void RegisterAllOpenGeneric_WitEmptyOpenGenericImplementationsWithNullValues_ThrowsArgumentException()
        {
            // Arrange
            IEnumerable<Type> invalidOpenGenericImplementations = new Type[] { null };

            // Act
            Action action = () =>
                (new Container()).RegisterAllOpenGeneric(typeof(IEventHandler<>), invalidOpenGenericImplementations);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentException>("openGenericImplementations", action);
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The collection contains null elements.", action);
        }

#if DEBUG && !SILVERLIGHT
        [TestMethod]
        public void GetRelationship_OnRegistrationBuiltByRegisterAllOpenGeneric_ReturnsTheExpectedRelationships()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<ILogger, FakeLogger>();

            container.RegisterAllOpenGeneric(typeof(IEventHandler<>), typeof(EventHandlerWithLoggerDependency<>));

            container.Register<ServiceWithDependency<IEnumerable<IEventHandler<ClassEvent>>>>();

            container.Verify();

            var expectedRelationship = new KnownRelationship(
                implementationType: typeof(EventHandlerWithLoggerDependency<ClassEvent>),
                lifestyle: Lifestyle.Transient,
                dependency: container.GetRegistration(typeof(ILogger)));

            // Act
            var actualRelationship =
                container.GetRegistration(typeof(IEnumerable<IEventHandler<ClassEvent>>)).GetRelationships()
                .Single();
            
            // Assert
            Assert.AreEqual(expectedRelationship.ImplementationType, actualRelationship.ImplementationType);
            Assert.AreEqual(expectedRelationship.Lifestyle, actualRelationship.Lifestyle);
            Assert.AreEqual(expectedRelationship.Dependency, actualRelationship.Dependency);
        }
#endif

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
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c =>
                    {
                        if (c.ServiceType.ContainsGenericParameter())
                        {
                            throw new InvalidOperationException("ServiceType should be a closed type");
                        }

                        return true;
                    });

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();
        }

        [TestMethod]
        public void RegisterOpenGeneric_PredicateContext_ImplementationTypeIsClosedImplentation()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterOpenGeneric(typeof(IOpenGenericWithPredicate<>), typeof(OpenGenericWithPredicate1<>),
                Lifestyle.Transient, c =>
                    {
                        if (c.ImplementationType.ContainsGenericParameter())
                        {
                            throw new InvalidOperationException("ImplementationType should be a closed type");
                        }

                        return true;
                    });

            // Act
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();
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
            Assert.IsInstanceOfType(result, typeof(OpenGenericWithPredicate1<int>));
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
            Assert.IsInstanceOfType(result, typeof(OpenGenericWithPredicate2<long>));
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
                "implemention of the requested service.");
        }

        private bool handled = false;
        [TestMethod]
        public void RegisterOpenGeneric_TwoEquivalentImplementationsWithValidPredicate_UpdateHandledProperty()
        {
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
                        this.handled = c.Handled;
                        return c.ImplementationType.GetGenericArguments().Single() == typeof(long);
                    });

            // Act
            this.handled = false;
            var result = container.GetInstance<IOpenGenericWithPredicate<int>>();

            // Assert
            Assert.IsTrue(this.handled);
        }

        private static void Assert_RegisterAllOpenGenericResultsInExpectedListOfTypes<TService>(
            Type[] openGenericTypesToRegister, Type[] expectedTypes)
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAllOpenGeneric(
                typeof(TService).GetGenericTypeDefinition(), 
                openGenericTypesToRegister);

            // Act
            var instances = container.GetAllInstances<TService>().ToArray();

            // Assert
            var actualTypes = instances.Select(instance => instance.GetType()).ToArray();

            Assert.IsTrue(expectedTypes.SequenceEqual(actualTypes), 
                "Actual: " + actualTypes.ToFriendlyNamesText());
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
        }

        public class NewConstraintEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : new()
        {
        }

        public class StructConstraintEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : struct
        {
        }

        public class ClassConstraintEventHandler<TClassEvent> : IEventHandler<TClassEvent>
            where TClassEvent : class
        {
        }

        public abstract class AbstractEventHandler<TEvent> : IEventHandler<TEvent>
        {
        }

        public class EventHandlerWithLoggerDependency<TEvent> : IEventHandler<TEvent>
        {
            public EventHandlerWithLoggerDependency(ILogger logger)
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

        public class NonGenericEventHandler : IEventHandler<ClassEvent>
        {
        }

        public class ServiceWithDependency<TDependency>
        {
            public ServiceWithDependency(TDependency dependency)
            {
                this.Dependency = dependency;
            }

            public TDependency Dependency { get; private set; }
        }
        
        public class Implementation<X, TUnused1, TUnused2, Y> : IInterface<X, X, Y> 
        {
        }

        public interface IOpenGenericWithPredicate<T>
        {
        }

        public class OpenGenericWithPredicate1<T> : IOpenGenericWithPredicate<T> 
        {
        }

        public class OpenGenericWithPredicate2<T> : IOpenGenericWithPredicate<T> 
        {
        }

        internal class InternalEventHandler<TEvent> : IEventHandler<TEvent>
        {
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