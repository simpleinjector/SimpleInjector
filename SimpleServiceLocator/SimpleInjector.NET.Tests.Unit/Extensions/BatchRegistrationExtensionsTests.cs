namespace SimpleInjector.Tests.Unit.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Extensions;

    // Suppress the Obsolete warnings, since we want to test GetTypesToRegister overloads that are marked obsolete.
#pragma warning disable 0618
    [TestClass]
    public class BatchRegistrationExtensionsTests
    {
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

#if !SILVERLIGHT
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
#endif

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterManyForOpenGeneric_WithClosedGenericType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<int, int>), Assembly.GetExecutingAssembly());
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Assembly.GetExecutingAssembly());
        }

#if !SILVERLIGHT

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
#endif

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType1()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<string, object>>();

            // Assert
            Assert.IsInstanceOfType(impl, typeof(Concrete1),
                "Concrete1 implements IService<string, object> directly.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType2()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.IsInstanceOfType(impl, typeof(Concrete2),
                "Concrete2 implements OpenGenericBase<int> which implements IService<int, string>.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsTransientInstances()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Assembly.GetExecutingAssembly());

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
            container.RegisterManySinglesForOpenGeneric(typeof(IService<,>), Assembly.GetExecutingAssembly());

            // Act
            var impl1 = container.GetInstance<IService<int, string>>();
            var impl2 = container.GetInstance<IService<int, string>>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(impl1, impl2), "The types should be registered as singleton.");
        }

#if !SILVERLIGHT
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
#endif

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_RespectsTheSuppliedLifestyle3()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Lifestyle.Transient,
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
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Lifestyle.Singleton,
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
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Lifestyle.Singleton, assemblies);

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
            container.RegisterManySinglesForOpenGeneric(typeof(IService<,>), assemblies);
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

#if !SILVERLIGHT

        [TestMethod]
        public void RegisterManySinglesForOpenGenericAccessibilityOptionEnumerable_WithValidTypeDefinitions_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };

            // Act
            container.RegisterManySinglesForOpenGeneric(typeof(IService<,>), AccessibilityOption.AllTypes,
                assemblies);
        }

        [TestMethod]
        public void RegisterManySinglesForOpenGenericAccessibilityOptionParams_WithValidTypeDefinitions_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            Assembly[] assemblies = new[] { Assembly.GetExecutingAssembly() };

            // Act
            container.RegisterManySinglesForOpenGeneric(typeof(IService<,>), AccessibilityOption.AllTypes,
                assemblies);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_IncludingInternalTypes_ReturnsExpectedType()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), AccessibilityOption.AllTypes,
                Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<decimal, decimal>>();

            // Assert
            Assert.IsInstanceOfType(impl, typeof(Concrete4),
                "Internal type Concrete4 should be found.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterManyForOpenGeneric_WithInvalidAccessibilityOption_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), (AccessibilityOption)5,
                Assembly.GetExecutingAssembly());
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void RegisterManyForOpenGeneric_ExcludingInternalTypes_DoesNotRegisterInternalTypes()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), AccessibilityOption.PublicTypesOnly,
                Assembly.GetExecutingAssembly());

            // Act
            container.GetInstance<IService<decimal, decimal>>();
        }
#endif

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType3()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<float, double>>();

            // Assert
            Assert.IsInstanceOfType(impl, typeof(Concrete3),
                "Concrete3 implements INonGeneric which implements IService<float, double>.");
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithValidTypeDefinitions_ReturnsExpectedType4()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<Type, Type>>();

            // Assert
            Assert.IsInstanceOfType(impl, typeof(Concrete3),
                "Concrete3 implements IService<Type, Type>.");
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

            Assert.IsInstanceOfType(impl, typeof(Concrete1),
                "Concrete1 implements IService<string, object>.");
            Assert.IsInstanceOfType(impl, typeof(Concrete1),
                "Concrete2 implements IService<int, string>.");
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterManyForOpenGeneric_WithNullAssemblyParamsArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Assembly[] invalidArgument = null;

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), invalidArgument);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterManyForOpenGeneric_WithNullAssemblyIEnumerableArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Assembly> invalidArgument = null;

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), invalidArgument);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterManyForOpenGeneric_WithNullContainer_ThrowsException()
        {
            // Arrange
            Container invalidContainer = null;
            var validServiceType = typeof(IService<,>);
            var validAssembly = Assembly.GetExecutingAssembly();

            // Act
            invalidContainer.RegisterManyForOpenGeneric(validServiceType, validAssembly);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterManyForOpenGeneric_WithNullTypesToRegister_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var validServiceType = typeof(IService<,>);
            IEnumerable<Type> invalidTypesToRegister = null;

            // Act
            container.RegisterManyForOpenGeneric(validServiceType, invalidTypesToRegister);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterManyForOpenGeneric_WithNullOpenGenericServiceType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type invalidServiceType = null;
            IEnumerable<Type> validTypesToRegister = new Type[] { typeof(object) };

            // Act
            container.RegisterManyForOpenGeneric(invalidServiceType, validTypesToRegister);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterManyForOpenGeneric_WithNullElementInTypesToRegister_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type validServiceType = typeof(IService<,>);
            IEnumerable<Type> invalidTypesToRegister = new Type[] { null };

            // Act
            container.RegisterManyForOpenGeneric(validServiceType, invalidTypesToRegister);
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

#if !SILVERLIGHT
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
#endif

        [TestMethod]
        public void RegisterManyForOpenGenericEnumerable_WithValidArguments_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            BatchRegistrationCallback callback = (closedServiceType, implementations) => { };

            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };

            // Act
            container.RegisterManyForOpenGeneric(typeof(IService<,>), assemblies);
        }

#if !SILVERLIGHT
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
#endif

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

        public class ServiceDecorator : IService<int, object>
        {
            public ServiceDecorator(IService<int, object> decorated)
            {
            }
        }

        // Internal type.
        private class Concrete4 : IService<decimal, decimal>
        {
        }

        #endregion
    }
    #pragma warning restore 0618
}