namespace SimpleInjector.Tests.Unit.Extensions
{
    // Suppress the Obsolete warnings, since we want to test GetTypesToRegister overloads that are marked obsolete.
#pragma warning disable 0618
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Extensions;

    /// <summary>Tests for full .NET framework version.</summary>
    public partial class BatchRegistrationExtensionsTests
    {
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
            AssertThat.IsInstanceOfType(typeof(InternalConcrete4), impl);
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
        public void RegisterManyForOpenGenericOverload1_AccessibilityOption_RegistersInternalTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterManyForOpenGeneric(typeof(IService<,>), Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<decimal, decimal>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InternalConcrete4), impl);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericOverload2_AccessibilityOption_RegistersInternalTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };
            container.RegisterManyForOpenGeneric(typeof(IService<,>), assemblies);

            // Act
            var impl = container.GetInstance<IService<decimal, decimal>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InternalConcrete4), impl);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericOverload3_AccessibilityOption_RegistersInternalTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterManyForOpenGeneric(typeof(IService<,>), 
                (s, i) => container.Register(s, i[0]),
                Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<decimal, decimal>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InternalConcrete4), impl);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericOverload4_AccessibilityOption_RegistersInternalTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };
            container.RegisterManyForOpenGeneric(typeof(IService<,>), 
                (s, i) => container.Register(s, i[0]),
                assemblies);

            // Act
            var impl = container.GetInstance<IService<decimal, decimal>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InternalConcrete4), impl);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericOverload5_AccessibilityOption_RegistersInternalTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterManyForOpenGeneric(typeof(IService<,>), Lifestyle.Transient, 
                Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<decimal, decimal>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InternalConcrete4), impl);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericOverload6_AccessibilityOption_RegistersInternalTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };
            container.RegisterManyForOpenGeneric(typeof(IService<,>), Lifestyle.Transient, assemblies);

            // Act
            var impl = container.GetInstance<IService<decimal, decimal>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InternalConcrete4), impl);
        }
        
        [TestMethod]
        public void RegisterManySinglesForOpenGenericOverload1_AccessibilityOption_RegistersInternalTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterManySinglesForOpenGeneric(typeof(IService<,>), Assembly.GetExecutingAssembly());

            // Act
            var impl = container.GetInstance<IService<decimal, decimal>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InternalConcrete4), impl);
        }

        [TestMethod]
        public void RegisterManySinglesForOpenGenericOverload2_AccessibilityOption_RegistersInternalTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };
            container.RegisterManySinglesForOpenGeneric(typeof(IService<,>), assemblies);

            // Act
            var impl = container.GetInstance<IService<decimal, decimal>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InternalConcrete4), impl);
        }
    }
}