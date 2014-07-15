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
            Assert.IsInstanceOfType(impl, typeof(Concrete4),
                "Internal type Concrete4 should be found.");
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
    }
}