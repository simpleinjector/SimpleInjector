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
            container.Collection.Register(typeof(IService<,>), new[] { typeof(Concrete3) });

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

            container.Collection.Register(typeof(ICommandHandler<>), types);

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

            container.Collection.Register(typeof(ICommandHandler<>), registeredTypes);

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

            container.Collection.Register(typeof(ICommandHandler<>), registeredTypes);

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

            container.Collection.Register(typeof(ICommandHandler<>), registeredTypes);

            // Assert
            var handlers = container.GetAllInstances<ICommandHandler<object>>();

            // Assert
            var actual = handlers.Select(handler => handler.GetType()).ToArray();
            Assert.AreEqual(expected.ToFriendlyNamesText(), actual.ToFriendlyNamesText());
        }

        // #857
        [TestMethod]
        public void GetAllInstances_OpenGenericsRegisteredWithSingletonLifestyle_ResolvesInstancesUsingExpectedLifestyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register(
                typeof(IEventHandler<>),
                new[] { typeof(NewConstraintEventHandler<>), typeof(StructConstraintEventHandler<>) },
                Lifestyle.Singleton);

            // Act
            var handlers1 = container.GetAllInstances<IEventHandler<StructEvent>>().ToArray();
            var handlers2 = container.GetAllInstances<IEventHandler<StructEvent>>().ToArray();

            // Assert
            Assert.AreEqual(2, handlers1.Count());
            Assert.AreSame(handlers1.First(), handlers2.First());
            Assert.AreSame(handlers1.Last(), handlers2.Last());
        }

        // #857
        [TestMethod]
        public void GetAllInstances_OpenGenericsRegisteredWithSingletonLifestyleAndMappingToAmbigousRegistration_ThrowsAnException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(
                typeof(NewConstraintEventHandler<>),
                typeof(NewConstraintEventHandler<>),
                Lifestyle.Transient);

            container.Collection.Register(
                typeof(IEventHandler<>),
                new[] { typeof(NewConstraintEventHandler<>), typeof(StructConstraintEventHandler<>) },
                Lifestyle.Singleton);

            // Act
            Action action = () => container.GetAllInstances<IEventHandler<StructEvent>>().ToArray();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The registration for the collection of IEventHandler<StructEvent> (i.e.
                IEnumerable<IEventHandler<StructEvent>>) is supplied with the type
                NewConstraintEventHandler<TEvent>, which was either registered explicitly, or was resolved
                using unregistered type resolution. It was, however, done so using a different lifestyle.
                The collection was made explicitly for the Singleton lifestyle, while the explicit
                registration was given the Transient lifestyle. For Simple Injector to be able to resolve this
                collection, these lifestyle must match."
                .TrimInside(),
                action);
        }

        // #857
        [TestMethod]
        public void GetAllInstances_OpenGenericsRegisteredWithSingletonLifestyleAndMappingToAmbigousAbstraction_ThrowsAnException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register(
                typeof(IEventHandler<>),
                typeof(StructConstraintEventHandler<>),
                Lifestyle.Transient);

            container.Collection.Register(
                typeof(IEventHandler<>),
                new[] { typeof(NewConstraintEventHandler<>), typeof(IEventHandler<>) },
                Lifestyle.Singleton);

            // Act
            Action action = () => container.GetAllInstances<IEventHandler<StructEvent>>().ToArray();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The registration for the collection of IEventHandler<StructEvent> (i.e.
                IEnumerable<IEventHandler<StructEvent>>) is supplied with the type IEventHandler<TEvent>,
                which was either registered explicitly, or was resolved using unregistered type resolution.
                It was, however, done so using a different lifestyle. The collection was made explicitly for
                the Singleton lifestyle, while the explicit registration was given the Transient lifestyle.
                For Simple Injector to be able to resolve this collection, these lifestyle must match."
                .TrimInside(),
                action);
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