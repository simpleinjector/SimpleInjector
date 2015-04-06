namespace SimpleInjector.Tests.Unit.Extensions
{
    // Suppress the Obsolete warnings, since we want to test GetTypesToRegister overloads that are marked obsolete.
#pragma warning disable 0618
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Extensions;

    /// <summary>Normal tests.</summary>
    [TestClass]
    public partial class BatchRegistrationExtensionsTests
    {
        public interface ICommandHandler<T> 
        { 
        }

        // This is the open generic interface that will be used as service type.
        public interface IService<TA, TB>
        {
        }

        // An non-generic interface that inherits from the closed generic IGenericService.
        public interface INonGeneric : IService<float, double>
        {
        }

        public interface IInvalid<TA, TB>
        {
        }

        [TestMethod]
        public void GetTypesToRegister1_Always_ReturnsAValue()
        {
            // Arrange
            var result = OpenGenericBatchRegistrationExtensions.GetTypesToRegister(typeof(IService<,>),
                typeof(IService<,>).Assembly);

            // Act
            result.ToArray();
        }

        [TestMethod]
        public void GetTypesToRegister2_Always_ReturnsAValue()
        {
            // Arrange
            IEnumerable<Assembly> assemblies = new[] { typeof(IService<,>).Assembly };

            var result = OpenGenericBatchRegistrationExtensions.GetTypesToRegister(typeof(IService<,>),
                assemblies);

            // Act
            result.ToArray();
        }

        [TestMethod]
        public void GetTypesToRegister_WithoutContainerArgument_ReturnsDecorators()
        {
            // Arrange
            // Act
            var types = OpenGenericBatchRegistrationExtensions.GetTypesToRegister(
                typeof(IService<,>), typeof(IService<,>).Assembly);

            // Assert
            Assert.IsTrue(types.Any(type => type == typeof(ServiceDecorator)), "The decorator was not included.");
        }

        [TestMethod]
        public void GetTypesToRegister_WithContainerArgument_ReturnsNoDecorators()
        {
            // Arrange
            var container = new Container();

            // Act
            var types = OpenGenericBatchRegistrationExtensions.GetTypesToRegister(container,
                typeof(IService<,>), typeof(IService<,>).Assembly);

            // Assert
            Assert.IsFalse(types.Any(type => type == typeof(ServiceDecorator)), "The decorator was included.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithClosedGenericType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () =>
                container.RegisterManyForOpenGeneric(typeof(IService<int, int>), Assembly.GetExecutingAssembly());

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), 
                AccessibilityOption.PublicTypesOnly,
                Assembly.GetExecutingAssembly());
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType1()
        {
            // Arrange
            var container = ContainerFactory.New();

            // We've got these concrete public implementations: Concrete1, Concrete2, Concrete3
            container.RegisterManyForOpenGeneric(typeof(IService<,>), 
                AccessibilityOption.PublicTypesOnly,
                Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<string, object>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Concrete1), impl, "Concrete1 implements IService<string, object> directly.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType2()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterManyForOpenGeneric(typeof(IService<,>),
                AccessibilityOption.PublicTypesOnly, 
                Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<int, string>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Concrete2), impl, "Concrete2 implements OpenGenericBase<int> which implements IService<int, string>.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsTransientInstances()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterManyForOpenGeneric(typeof(IService<,>), 
                AccessibilityOption.PublicTypesOnly,
                Assembly.GetExecutingAssembly());

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreNotEqual(impl1, impl2, "The types should be registered as transient.");
        }

        [TestMethod]
        public void RegisterManySinglesForOpenGeneric_WithValidTypeDefinitions_ReturnsSingletonInstances()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterManySinglesForOpenGeneric(typeof(IService<,>), 
                AccessibilityOption.PublicTypesOnly, 
                Assembly.GetExecutingAssembly());

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(impl1, impl2), "The types should be registered as singleton.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle3()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterManyForOpenGeneric(typeof(IService<,>),
                AccessibilityOption.PublicTypesOnly,
                Lifestyle.Transient,
                Assembly.GetExecutingAssembly());

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreNotEqual(impl1, impl2, "The types should be registered as transient.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle4()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterManyForOpenGeneric(typeof(IService<,>), 
                AccessibilityOption.PublicTypesOnly, 
                Lifestyle.Singleton,
                Assembly.GetExecutingAssembly());

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreEqual(impl1, impl2, "The type should be returned as singleton.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle5()
        {
            // Arrange
            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };

            var container = ContainerFactory.New();

            container.RegisterManyForOpenGeneric(typeof(IService<,>),
                AccessibilityOption.PublicTypesOnly, 
                Lifestyle.Singleton, 
                assemblies);

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreEqual(impl1, impl2, "The type should be returned as singleton.");
        }

        [TestMethod]
        public void RegisterManySinglesForOpenGeneric_WithValidTypeDefinitions2_ReturnsSingletonInstances()
        {
            // Arrange
            // Concrete1 implements IService<string, object>
            IEnumerable<Type> typesToRegister = new[] { typeof(Concrete1) };

            var container = ContainerFactory.New();

            container.RegisterManySinglesForOpenGeneric(typeof(IService<,>), typesToRegister);

            // Act
            var impl1 = container.GetInstance<IService<string, object>>();
            var impl2 = container.GetInstance<IService<string, object>>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(impl1, impl2), "The types should be registered as singleton.");
        }

        [TestMethod]
        public void RegisterManySinglesForOpenGenericEnumerable_WithValidTypeDefinitions_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };

            // Act
            container.RegisterManySinglesForOpenGeneric(typeof(IService<,>), 
                AccessibilityOption.PublicTypesOnly, assemblies);
        }

        [TestMethod]
        public void RegisterManySinglesForOpenGenericTypeParams_WithValidTypeDefinitions_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type[] typesToRegister = new[] { typeof(Concrete1) };

            // Act
            container.RegisterManySinglesForOpenGeneric(typeof(IService<,>), typesToRegister);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType3()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), 
                AccessibilityOption.PublicTypesOnly,
                Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<float, double>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Concrete3), impl, "Concrete3 implements INonGeneric which implements IService<float, double>.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType4()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterManyForOpenGeneric(typeof(IService<,>), 
                AccessibilityOption.PublicTypesOnly,
                Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<Type, Type>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Concrete3), impl, "Concrete3 implements IService<Type, Type>.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithMultipleTypeDefinitionsReferencingTheSameInterface_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                // Act
                // This call should fail, because both Invalid1 and Invalid2 implement the same closed generic
                // interface IInvalid<int, double>
                container.RegisterManyForOpenGeneric(typeof(IInvalid<,>), Assembly.GetExecutingAssembly());

                Assert.Fail("RegisterManyForOpenGeneric is expected to fail.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("There are 2 types that represent the closed generic type"),
                    "Actual message: " + ex.Message);
            }
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithMultipleConcreteTypes_RegistersTheExpectedServiceTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), typeof(Concrete1), typeof(Concrete2));

            // Assert
            var impl = container.GetInstance<IService<string, object>>();
            var imp2 = container.GetInstance<IService<int, string>>();

            AssertThat.IsInstanceOfType(typeof(Concrete1), impl, "Concrete1 implements IService<string, object>.");
            AssertThat.IsInstanceOfType(typeof(Concrete1), impl, "Concrete2 implements IService<int, string>.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithNonInheritableType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var serviceType = typeof(IService<,>);
            var validType = typeof(ServiceImpl<object, string>);
            var invalidType = typeof(List<int>);

            try
            {
                // Act
                container.RegisterManyForOpenGeneric(serviceType, validType, invalidType);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (Exception ex)
            {
                AssertThat.StringContains("List<Int32>", ex.Message);
                AssertThat.StringContains("IService<TA, TB>", ex.Message);
            }
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithNullAssemblyParamsArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Assembly[] invalidArgument = null;

            // Act
            Action action = () => container.RegisterManyForOpenGeneric(typeof(IService<,>), invalidArgument);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithNullAssemblyIEnumerableArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Assembly> invalidArgument = null;

            // Act
            Action action = () => container.RegisterManyForOpenGeneric(typeof(IService<,>), invalidArgument);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithNullContainer_ThrowsException()
        {
            // Arrange
            Container invalidContainer = null;
            var validServiceType = typeof(IService<,>);
            var validAssembly = Assembly.GetExecutingAssembly();

            // Act
            Action action = () => invalidContainer.RegisterManyForOpenGeneric(validServiceType, validAssembly);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithNullTypesToRegister_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var validServiceType = typeof(IService<,>);
            IEnumerable<Type> invalidTypesToRegister = null;

            // Act
            Action action = () => container.RegisterManyForOpenGeneric(validServiceType, invalidTypesToRegister);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithNullOpenGenericServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type invalidServiceType = null;
            IEnumerable<Type> validTypesToRegister = new Type[] { typeof(object) };

            // Act
            Action action = () => container.RegisterManyForOpenGeneric(invalidServiceType, validTypesToRegister);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithNullElementInTypesToRegister_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type validServiceType = typeof(IService<,>);
            IEnumerable<Type> invalidTypesToRegister = new Type[] { null };

            // Act
            Action action = () => container.RegisterManyForOpenGeneric(validServiceType, invalidTypesToRegister);

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericAssemblyParams_WithCallbackThatDoesNothing_DoesNotRegisterAnything()
        {
            // Arrange
            var container = ContainerFactory.New();

            BatchRegistrationCallback callback = (closedServiceType, implementations) =>
            {
                // Do nothing.
            };

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), callback, Assembly.GetExecutingAssembly());

            // Assert
            var registration = container.GetRegistration(typeof(IService<string, object>));

            Assert.IsNull(registration, "GetRegistration should result in null, because by supplying a delegate, the " +
                "extension method does not do any registration itself.");
        }

        [TestMethod]
        public void RegisterManyForOpenGenericEnumerable_WithValidArguments_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            BatchRegistrationCallback callback = (closedServiceType, implementations) => { };

            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), 
                AccessibilityOption.PublicTypesOnly,
                assemblies);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithCallback_IsCalledTheExpectedAmountOfTimes()
        {
            // Arrange
            List<Type> expectedClosedServiceTypes = new List<Type>
            {
                typeof(IService<float, double>), 
                typeof(IService<Type, Type>) 
            };

            List<Type> actualClosedServiceTypes = new List<Type>();

            var container = ContainerFactory.New();

            BatchRegistrationCallback callback = (closedServiceType, implementations) =>
            {
                actualClosedServiceTypes.Add(closedServiceType);
            };

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), callback, new[] { typeof(Concrete3) });

            // Assert
            Assert_AreEqual(expectedClosedServiceTypes, actualClosedServiceTypes);
        }

        [TestMethod]
        public void GetInstance_ImplementationWithMultipleInterfaces_ReturnsThatImplementationForEachInterface()
        {
            // Arrange
            var container = ContainerFactory.New();

            // MultiInterfaceHandler implements two interfaces.
            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), Assembly.GetExecutingAssembly());

            // Act
            var instance1 = container.GetInstance<ICommandHandler<int>>();
            var instance2 = container.GetInstance<ICommandHandler<double>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(MultiInterfaceHandler), instance1);
            AssertThat.IsInstanceOfType(typeof(MultiInterfaceHandler), instance2);
        }

        [TestMethod]
        public void GetInstance_BatchRegistrationUsingSingletonLifestyle_AlwaysReturnsTheSameInstanceForItsInterfaces()
        {
            // Arrange
            var container = ContainerFactory.New();

            // MultiInterfaceHandler implements two interfaces.
            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), Lifestyle.Singleton,
                Assembly.GetExecutingAssembly());

            // Act
            var instance1 = container.GetInstance<ICommandHandler<int>>();
            var instance2 = container.GetInstance<ICommandHandler<double>>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(instance1, instance2));
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithoutCallback1_SuppliedWithOpenGenericType_FailsWithExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type[] types = new[] { typeof(GenericHandler<>) };

            // Act
            Action action = () => container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), types);

            // Assert
            AssertThat.ThrowsWithParamName("typesToRegister", action);
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(@"
                The supplied list of types contains an open generic type, but this overloaded method is unable
                to handle open generic types because this overload can only register closed generic services 
                types that have a single implementation. Please use the overload that takes in the 
                BatchRegistrationCallback instead.".TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithoutCallback2_SuppliedWithOpenGenericType_FailsWithExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Type> types = new[] { typeof(GenericHandler<>) };

            // Act
            Action action = () => container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), types);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                @"The supplied list of types contains an open generic type", action);
        }

        [TestMethod]
        public void RegisterManySinglesForOpenGenericWithoutCallback1_SuppliedWithOpenGenericType_FailsWithExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type[] types = new[] { typeof(GenericHandler<>) };

            // Act
            Action action = () => container.RegisterManySinglesForOpenGeneric(typeof(ICommandHandler<>), types);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                @"The supplied list of types contains an open generic type", action);
        }

        [TestMethod]
        public void RegisterManySinglesForOpenGenericWithoutCallback2_SuppliedWithOpenGenericType_FailsWithExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Type> types = new[] { typeof(GenericHandler<>) };

            // Act
            Action action = () => container.RegisterManySinglesForOpenGeneric(typeof(ICommandHandler<>), types);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                @"The supplied list of types contains an open generic type", action);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithCallback_SuppliedWithOpenGenericType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            var types = new[] { typeof(GenericHandler<>) };

            // Act
            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), 
                (service, implementations) => { },
                types);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithCallback_SuppliedWithOpenGenericType_ReturnsTheExpectedClosedGenericVersion()
        {
            // Arrange
            var types = new[] { typeof(DecimalHandler), typeof(GenericHandler<>) };
            var expected = new[] { typeof(DecimalHandler), typeof(GenericHandler<decimal>) };

            // Assert
            Assert_RegisterManyForOpenGenericWithCallback_ReturnsExpectedImplementations(types, expected);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithCallback_SuppliedWithOpenGenericTypeWithCompatibleTypeConstraint_ReturnsThatGenericType()
        {
            // Arrange
            var types = new[] { typeof(FloatHandler), typeof(GenericStructHandler<>) };
            var expected = new[] { typeof(FloatHandler), typeof(GenericStructHandler<float>) };

            // Assert
            Assert_RegisterManyForOpenGenericWithCallback_ReturnsExpectedImplementations(types, expected);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericWithCallback_SuppliedWithOpenGenericTypeWithIncompatibleTypeConstraint_DoesNotReturnThatGenericType()
        {
            // Arrange
            var types = new[] { typeof(ObjectHandler), typeof(GenericStructHandler<>) };
            var expected = new[] { typeof(ObjectHandler) };

            // Assert
            Assert_RegisterManyForOpenGenericWithCallback_ReturnsExpectedImplementations(types, expected);
        }

        [TestMethod]
        public void GetTypesToRegister3_Always_ReturnsAValue()
        {
            var result = OpenGenericBatchRegistrationExtensions.GetTypesToRegister(typeof(IService<,>),
                AccessibilityOption.AllTypes, typeof(IService<,>).Assembly);

            // Act
            result.ToArray();
        }

        [TestMethod]
        public void GetTypesToRegister4_Always_ReturnsAValue()
        {
            // Arrange
            IEnumerable<Assembly> assemblies = new[] { typeof(IService<,>).Assembly };

            var result = OpenGenericBatchRegistrationExtensions.GetTypesToRegister(typeof(IService<,>),
                AccessibilityOption.AllTypes, assemblies);

            // Act
            result.ToArray();
        }

        [TestMethod]
        public void GetTypesToRegisterOverload1_WithoutAccessibilityOption_ReturnsInternalTypes()
        {
            // Arrange
            var container = new Container();

            // Act
            var result = OpenGenericBatchRegistrationExtensions.GetTypesToRegister(
                container, 
                typeof(IService<,>), 
                typeof(IService<,>).Assembly);

            // Assert
            Assert.IsTrue(result.Contains(typeof(InternalConcrete4)));
        }

        [TestMethod]
        public void GetTypesToRegisterOverload2_WithoutAccessibilityOption_ReturnsInternalTypes()
        {
            // Arrange
            var container = new Container();

            // Act
            IEnumerable<Assembly> assemblies = new[] { typeof(IService<,>).Assembly };
            var result = OpenGenericBatchRegistrationExtensions.GetTypesToRegister(
                container,
                typeof(IService<,>),
                assemblies);

            // Assert
            Assert.IsTrue(result.Contains(typeof(InternalConcrete4)));
        }

        [TestMethod]
        public void GetTypesToRegisterOverload3_WithoutAccessibilityOption_ReturnsInternalTypes()
        {
            // Arrange
            var container = new Container();

            // Act
            var result = OpenGenericBatchRegistrationExtensions.GetTypesToRegister(
                typeof(IService<,>),
                typeof(IService<,>).Assembly);

            // Assert
            Assert.IsTrue(result.Contains(typeof(InternalConcrete4)));
        }

        [TestMethod]
        public void GetTypesToRegisterOverload4_WithoutAccessibilityOption_ReturnsInternalTypes()
        {
            // Arrange
            var container = new Container();

            // Act
            IEnumerable<Assembly> assemblies = new[] { typeof(IService<,>).Assembly };
            var result = OpenGenericBatchRegistrationExtensions.GetTypesToRegister(
                typeof(IService<,>),
                assemblies);

            // Assert
            Assert.IsTrue(result.Contains(typeof(InternalConcrete4)));
        }

        [TestMethod]
        public void RegisterManyForOpenGenericAccessibilityOptionAndParams_WithValidTypeDefinitions_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), AccessibilityOption.PublicTypesOnly,
                Assembly.GetExecutingAssembly());
        }

        [TestMethod]
        public void RegisterManyForOpenGenericAccessibilityOptionAndEnumerable_WithValidTypeDefinitions_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), AccessibilityOption.PublicTypesOnly,
                assemblies);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle1()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterManyForOpenGeneric(typeof(IService<,>),
                AccessibilityOption.PublicTypesOnly, Lifestyle.Transient,
                Assembly.GetExecutingAssembly());

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreNotEqual(impl1, impl2, "The types should be registered as transient.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle2()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterManyForOpenGeneric(typeof(IService<,>),
                AccessibilityOption.PublicTypesOnly, Lifestyle.Singleton,
                Assembly.GetExecutingAssembly());

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.AreEqual(impl1, impl2, "The type should be returned as singleton.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithInvalidAccessibilityOption_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => 
                container.RegisterManyForOpenGeneric(typeof(IService<,>), (AccessibilityOption)5,
                    Assembly.GetExecutingAssembly());

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_ExcludingInternalTypes_DoesNotRegisterInternalTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterManyForOpenGeneric(typeof(IService<,>), AccessibilityOption.PublicTypesOnly,
                Assembly.GetExecutingAssembly());

            // Act
            Action action = () => container.GetInstance<IService<decimal, decimal>>();

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericAssemblyIEnumerable_WithCallbackThatDoesNothing_DoesNotRegisterAnything()
        {
            // Arrange
            var container = ContainerFactory.New();

            BatchRegistrationCallback callback = (closedServiceType, implementations) =>
            {
                // Do nothing.
            };

            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), AccessibilityOption.PublicTypesOnly,
                callback, assemblies);

            // Assert
            var registration = container.GetRegistration(typeof(IService<string, object>));

            Assert.IsNull(registration, "GetRegistration should result in null, because by supplying a delegate, the " +
                "extension method does not do any registration itself.");
        }

        [TestMethod]
        public void RegisterManyForOpenGenericAccessibilityOptionCallbackEnum_WithValidArguments_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            BatchRegistrationCallback callback = (closedServiceType, implementations) => { };

            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), AccessibilityOption.AllTypes, callback,
                assemblies);
        }

        // This is a regression test for work item 21002
        [TestMethod]
        public void RegisterManyForOpenGenericAssembly_RegistersTypeWithTheeImplementations_ResolvesThatTypeAsExpected()
        {
            // Arrange
            var container = new Container();

            // Just registers RequestGroup in three groups.
            container.RegisterManyForOpenGeneric(typeof(IHandler<,>), Lifestyle.Singleton,
                typeof(RequestGroup).Assembly);

            container.RegisterDecorator(typeof(IHandler<,>), typeof(RequestDecorator<,>));

            // Act
            // RequestGroup implements all three these interfaces.
            var decorator1 = container.GetInstance<IHandler<Query1, int>>() as RequestDecorator<Query1, int>;

            // This call fails in v2.1.0 to v2.7.1
            var decorator2 = container.GetInstance<IHandler<Query2, double>>() as RequestDecorator<Query2, double>;
            var decorator3 = container.GetInstance<IHandler<Query3, double>>() as RequestDecorator<Query3, double>;

            // Assert
            Assert.AreSame(decorator1.Decoratee, decorator2.Decoratee);
            Assert.AreSame(decorator2.Decoratee, decorator3.Decoratee);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericTypes_RegistersTypeWithTheeImplementations_ResolvesThatTypeAsExpected()
        {
            // Arrange
            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(IHandler<,>), Lifestyle.Singleton, new Type[]
            {
                typeof(RequestGroup)
            });

            container.RegisterDecorator(typeof(IHandler<,>), typeof(RequestDecorator<,>));

            // Act
            // RequestGroup implements all three these interfaces.
            var decorator1 = container.GetInstance<IHandler<Query1, int>>() as RequestDecorator<Query1, int>;
            var decorator2 = container.GetInstance<IHandler<Query2, double>>() as RequestDecorator<Query2, double>;
            var decorator3 = container.GetInstance<IHandler<Query3, double>>() as RequestDecorator<Query3, double>;

            // Assert
            Assert.AreSame(decorator1.Decoratee, decorator2.Decoratee);
            Assert.AreSame(decorator2.Decoratee, decorator3.Decoratee);
        }

        private static void Assert_RegisterManyForOpenGenericWithCallback_ReturnsExpectedImplementations(
            Type[] inputTypes, Type[] expectedTypes)
        {
            // Arrange
            var actualTypes = new List<Type>();

            var container = ContainerFactory.New();

            // Act
            container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>),
                (service, implementations) => actualTypes.AddRange(implementations),
                inputTypes);

            // Assert
            bool collectionsContainTheSameElements = !Enumerable.Except(expectedTypes, actualTypes).Any();

            Assert.IsTrue(collectionsContainTheSameElements,
                "Actual list: " + actualTypes.ToFriendlyNamesText());
        }

        private static void Assert_AreEqual<T>(List<T> expectedList, List<T> actualList)
        {
            Assert.IsNotNull(actualList);

            Assert.AreEqual(expectedList.Count, actualList.Count);

            for (int i = 0; i < expectedList.Count; i++)
            {
                T expected = expectedList[i];
                T actual = actualList[i];

                Assert.AreEqual(expected, actual, "Items at index " + i + " of list were expected to be the same.");
            }
        }

        #region IInvalid

        // Both Invalid1 and Invalid2 implement the same closed generic type.
        public class Invalid1 : IInvalid<int, double>
        {
        }

        public class Invalid2 : IInvalid<int, double>
        {
        }

        #endregion

        #region IService

        public class ServiceImpl<TA, TB> : IService<TA, TB>
        {
        }

        // An generic abstract class. Should not be used by the registration.
        public abstract class OpenGenericBase<T> : IService<T, string>
        {
        }

        // A non-generic abstract class. Should not be used by the registration.
        public abstract class ClosedGenericBase : IService<int, object>
        {
        }

        // A non-abstract generic type. Should not be used by the registration.
        public class OpenGeneric<T> : OpenGenericBase<T>
        {
        }

        // Instance of this type should be returned on container.GetInstance<IService<int, string>>()
        public class Concrete1 : IService<string, object>
        {
        }

        // Instance of this type should be returned on container.GetInstance<IService<string, object>>()
        public class Concrete2 : OpenGenericBase<int>
        {
        }

        // Instance of this type should be returned on container.GetInstance<IService<float, double>>() and
        // on container.GetInstance<IService<Type, Type>>()
        public class Concrete3 : INonGeneric, IService<Type, Type>
        {
        }

        public class MultiInterfaceHandler : ICommandHandler<int>, ICommandHandler<double>
        {
        }

        public class DecimalHandler : ICommandHandler<decimal>
        {
        }

        public class FloatHandler : ICommandHandler<float>
        {
        }

        public class ObjectHandler : ICommandHandler<object>
        {
        }

        public class GenericHandler<T> : ICommandHandler<T>
        {
        }

        public class GenericStructHandler<T> : ICommandHandler<T> where T : struct
        {
        }

        public class ServiceDecorator : IService<int, object>
        {
            public ServiceDecorator(IService<int, object> decorated)
            {
            }
        }

        // Internal type.
        private class InternalConcrete4 : IService<decimal, decimal>
        {
        }

        #endregion
    }

    public interface IRequest<TResponse>
    {
    }

    public interface IHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
    }

    public class RequestGroup :
        IHandler<Query1, int>,
        IHandler<Query2, double>,
        IHandler<Query3, double>
    {
    }

    public class Query1 : IRequest<int>
    {
    }

    public class Query2 : IRequest<double>
    {
    }

    public class Query3 : IRequest<double>
    {
    }

    public class RequestDecorator<TRequest, TResponse> : IHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public RequestDecorator(IHandler<TRequest, TResponse> decoratee)
        {
            this.Decoratee = decoratee;
        }

        public IHandler<TRequest, TResponse> Decoratee { get; private set; }
    }
    #pragma warning restore 0618
}