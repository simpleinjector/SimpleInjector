namespace SimpleInjector.Tests.Unit.Extensions
{
    // Suppress the Obsolete warnings, since we want to test GetTypesToRegister overloads that are marked obsolete.
#pragma warning disable 0618
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Extensions;

    /// <summary>Tests for full .NET framework version.</summary>
    public partial class BatchRegistrationExtensionsTests
    {
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
    }
}