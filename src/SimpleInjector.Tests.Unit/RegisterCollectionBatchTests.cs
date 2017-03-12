namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterCollectionBatchTests
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

        [TestMethod]
        public void RegisterCollectionTypes_ConcreteTypeImplementingMultipleClosedVersions_CanResolveBoth()
        {
            // Arrange
            var container = ContainerFactory.New();

            // class Concrete3 : IService<float, double>, IService<Type, Type>
            container.RegisterCollection(typeof(IService<,>), new[] { typeof(Concrete3) });

            // Act
            container.GetAllInstances<IService<float, double>>().Single();
            container.GetAllInstances<IService<Type, Type>>().Single();
        }
        
        [TestMethod]
        public void RegisterCollectionTypes_SuppliedWithOpenGenericType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            var types = new[] { typeof(GenericHandler<>) };

            container.RegisterCollection(typeof(ICommandHandler<>), types);

            // Act
            var handler = container.GetAllInstances<ICommandHandler<int>>().Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(GenericHandler<int>), handler);
        }

        [TestMethod]
        public void RegisterCollectionTypes_SuppliedWithOpenGenericType_ReturnsTheExpectedClosedGenericVersion()
        {
            // Arrange
            var registeredTypes = new[] { typeof(DecimalHandler), typeof(GenericHandler<>) };

            var expected = new[] { typeof(DecimalHandler), typeof(GenericHandler<decimal>) };

            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<>), registeredTypes);

            // Act
            var handlers = container.GetAllInstances<ICommandHandler<decimal>>();

            // Assert
            var actual = handlers.Select(handler => handler.GetType()).ToArray();
            Assert.AreEqual(expected.ToFriendlyNamesText(), actual.ToFriendlyNamesText());
        }

        [TestMethod]
        public void RegisterCollectionTypes_SuppliedWithOpenGenericTypeWithCompatibleTypeConstraint_ReturnsThatGenericType()
        {
            // Arrange
            var registeredTypes = new[] { typeof(FloatHandler), typeof(GenericStructHandler<>) };

            var expected = new[] { typeof(FloatHandler), typeof(GenericStructHandler<float>) };

            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<>), registeredTypes);

            // Assert
            var handlers = container.GetAllInstances<ICommandHandler<float>>();

            // Assert
            var actual = handlers.Select(handler => handler.GetType()).ToArray();
            Assert.AreEqual(expected.ToFriendlyNamesText(), actual.ToFriendlyNamesText());
        }

        [TestMethod]
        public void RegisterCollectionTypes_SuppliedWithOpenGenericTypeWithIncompatibleTypeConstraint_DoesNotReturnThatGenericType()
        {
            // Arrange
            var registeredTypes = new[] { typeof(ObjectHandler), typeof(GenericStructHandler<>) };

            var expected = new[] { typeof(ObjectHandler) };

            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICommandHandler<>), registeredTypes);

            // Assert
            var handlers = container.GetAllInstances<ICommandHandler<object>>();

            // Assert
            var actual = handlers.Select(handler => handler.GetType()).ToArray();
            Assert.AreEqual(expected.ToFriendlyNamesText(), actual.ToFriendlyNamesText());
        }

        #region IService

        // Instance of this type should be returned on container.GetInstance<IService<float, double>>() and
        // on container.GetInstance<IService<Type, Type>>()
        public class Concrete3 : IService<Type, Type>, INonGeneric
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
        
        #endregion
    }
}