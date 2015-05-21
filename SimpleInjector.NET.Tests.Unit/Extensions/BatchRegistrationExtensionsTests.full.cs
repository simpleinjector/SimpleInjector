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
        // This is the open generic interface that will be used as service type.
        public interface IServiceFull<TA, TB>
        {
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_IncludingInternalTypes_ReturnsExpectedType()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register(typeof(IServiceFull<,>), new[] { Assembly.GetExecutingAssembly() });

            // Act
            var impl = container.GetInstance<IServiceFull<decimal, decimal>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InternalConcrete4Full), impl);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericOverload1_AccessibilityOption_RegistersInternalTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IServiceFull<,>), new[] { Assembly.GetExecutingAssembly() });

            // Act
            var impl = container.GetInstance<IServiceFull<decimal, decimal>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InternalConcrete4Full), impl);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericOverload2_AccessibilityOption_RegistersInternalTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };
            container.Register(typeof(IServiceFull<,>), assemblies);

            // Act
            var impl = container.GetInstance<IServiceFull<decimal, decimal>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InternalConcrete4Full), impl);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericOverload5_AccessibilityOption_RegistersInternalTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(typeof(IServiceFull<,>), new[] { Assembly.GetExecutingAssembly() }, Lifestyle.Transient);

            // Act
            var impl = container.GetInstance<IServiceFull<decimal, decimal>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InternalConcrete4Full), impl);
        }

        [TestMethod]
        public void RegisterManyForOpenGenericOverload6_AccessibilityOption_RegistersInternalTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };

            container.Register(typeof(IServiceFull<,>), assemblies, Lifestyle.Transient);

            // Act
            var impl = container.GetInstance<IServiceFull<decimal, decimal>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InternalConcrete4Full), impl);
        }
        
        [TestMethod]
        public void GetTypesToRegisterOverload1_WithoutAccessibilityOption_ReturnsInternalTypes()
        {
            // Arrange
            var container = new Container();

            // Act
            var result = container.GetTypesToRegister(typeof(IServiceFull<,>), new[] { typeof(IServiceFull<,>).Assembly });

            // Assert
            Assert.IsTrue(result.Contains(typeof(InternalConcrete4Full)));
        }

        [TestMethod]
        public void GetTypesToRegisterOverload2_WithoutAccessibilityOption_ReturnsInternalTypes()
        {
            // Arrange
            var container = new Container();

            // Act
            IEnumerable<Assembly> assemblies = new[] { typeof(IServiceFull<,>).Assembly };

            var result = container.GetTypesToRegister(typeof(IServiceFull<,>), assemblies);

            // Assert
            Assert.IsTrue(result.Contains(typeof(InternalConcrete4Full)));
        }

        #region IServiceFull

        public class ServiceImplFull<TA, TB> : IServiceFull<TA, TB>
        {
        }

        // An generic abstract class. Should not be used by the registration.
        public abstract class OpenGenericBaseFull<T> : IServiceFull<T, string>
        {
        }

        // A non-generic abstract class. Should not be used by the registration.
        public abstract class ClosedGenericBaseFull : IServiceFull<int, object>
        {
        }

        // A non-abstract generic type. Should not be used by the registration.
        public class OpenGenericFull<T> : OpenGenericBaseFull<T>
        {
        }

        // Instance of this type should be returned on container.GetInstance<IService<int, string>>()
        public class Concrete1Full : IServiceFull<string, object>
        {
        }

        // Instance of this type should be returned on container.GetInstance<IService<string, object>>()
        public class Concrete2Full : OpenGenericBaseFull<int>
        {
        }

        // Instance of this type should be returned on container.GetInstance<IService<float, double>>() and
        // on container.GetInstance<IService<Type, Type>>()
        public class Concrete3Full : IServiceFull<Type, Type>
        {
        }

        // Internal type.
        private class InternalConcrete4Full : IServiceFull<decimal, decimal>
        {
        }

        #endregion
    }
}