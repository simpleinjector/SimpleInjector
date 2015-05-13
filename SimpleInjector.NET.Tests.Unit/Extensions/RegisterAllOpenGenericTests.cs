namespace SimpleInjector.Tests.Unit.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;
    using SimpleInjector.Extensions;

    [TestClass]
    public class RegisterAllOpenGenericTests
    {
        [TestMethod]
        public void GetAllInstances_RegisterCollectionWithAllOpenGenericWithTypeConstraints_ResolvesExpectedTypes1()
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
            Assert_RegisterCollectionWithAllOpenGenericResultsInExpectedListOfTypes<IEventHandler<ClassEvent>>(
                registeredTypes, expectedTypes);
        }

        [TestMethod]
        public void GetAllInstancesRegisterCollectionWithAllOpenGenericWithTypeConstraints_ResolvesExpectedTypes2()
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
            Assert_RegisterCollectionWithAllOpenGenericResultsInExpectedListOfTypes<IEventHandler<StructEvent>>(
                registeredTypes, expectedTypes);
        }

        [TestMethod]
        public void GetAllInstances_RegisterCollectionWithAllOpenGenericWithTypeConstraints_ResolvesExpectedTypes3()
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
            Assert_RegisterCollectionWithAllOpenGenericResultsInExpectedListOfTypes<IEventHandler<NoDefaultConstructorEvent>>(
                registeredTypes, expectedTypes);
        }

        [TestMethod]
        public void GetAllInstances_RegisterCollectionWithAllOpenGenericWithoutLifestyleParameter_RegistersAsTransient()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(ClassConstraintEventHandler<>) });

            // Act
            var instance1 = container.GetAllInstances<IEventHandler<ClassEvent>>().Single();
            var instance2 = container.GetAllInstances<IEventHandler<ClassEvent>>().Single();

            // Assert
            Assert.AreNotSame(instance1, instance2, "Transient was expected.");
        }

        [TestMethod]
        public void RegisterCollectionWithAllOpenGeneric_SuppliedWithIncompatible_ThrowsExpectedException()
        {
            // Arrange
            Type invalidType = typeof(NullValidator<>);

            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterCollection(typeof(IEventHandler<>), new[] { invalidType });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "NullValidator<T> does not implement IEventHandler<TEvent>.", action);
        }

        [TestMethod]
        public void RegisterCollectionWithAllOpenGeneric_SuppliedWithCompatibleNonGenericType_Succeeds()
        {
            // Arrange
            Type compatibleNonGenericType = typeof(NonGenericEventHandler);

            var container = ContainerFactory.New();

            // Act
            container.RegisterCollection(typeof(IEventHandler<>), new[] { compatibleNonGenericType });
        }

        [TestMethod]
        public void RegisterCollectionWithAllOpenGeneric_SuppliedWithCompatibleClosedGenericType_Succeeds()
        {
            // Arrange
            Type compatibleClosedGenericType = typeof(ClassConstraintEventHandler<object>);

            var container = ContainerFactory.New();

            // Act
            container.RegisterCollection(typeof(IEventHandler<>), new[] { compatibleClosedGenericType });
        }

        [TestMethod]
        public void RegisterCollectionWithAllOpenGeneric_WithNullOpenGenericServiceTypeParameter_ThrowsArgumentNullException()
        {
            // Arrange
            Type invalidOpenGenericServiceType = null;

            var container = new Container();

            // Act
            Action action = () => container.RegisterCollection(invalidOpenGenericServiceType, new[] { typeof(int) });

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("serviceType", action);
        }

        [TestMethod]
        public void RegisterCollectionWithAllOpenGeneric_WithNullOpenGenericImplementationsParameter_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<Type> invalidOpenGenericImplementations = null;

            // Act
            Action action = () =>
                (new Container()).RegisterCollection(typeof(int), invalidOpenGenericImplementations);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("serviceTypes", action);
        }

        [TestMethod]
        public void RegisterCollectionWithAllOpenGeneric_WitEmptyOpenGenericImplementationsWithNullValues_ThrowsArgumentException()
        {
            // Arrange
            IEnumerable<Type> invalidOpenGenericImplementations = new Type[] { null };

            // Act
            Action action = () =>
                (new Container()).RegisterCollection(typeof(IEventHandler<>), invalidOpenGenericImplementations);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentException>("serviceTypes", action);
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The collection contains null elements.", action);
        }

        [TestMethod]
        public void GetRelationship_OnRegistrationBuiltByRegisterCollectionWithAllOpenGeneric_ReturnsTheExpectedRelationships()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ILogger, FakeLogger>(Lifestyle.Singleton);

            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(EventHandlerWithLoggerDependency<>) });

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

        [TestMethod]
        public void RegisterCollection_RegisteringOneInstanceThatImplementsTwoClosedVersionsOfTheGivenOpenGenericAbstraction_ResolvesBothClosedVersionsCorrectly()
        {
            // Arrange
            var container = new Container();

            container.RegisterCollection(typeof(IEventHandler<>), new Type[]
            {
                typeof(HandlerWithTwoImplementations) // : IEventHandler<int>, IEventHandler<double>
            });

            // Act
            var intHandlers = container.GetAllInstances<IEventHandler<int>>().ToArray();
            var doubleHandlers = container.GetAllInstances<IEventHandler<double>>().ToArray();

            // Assert
            Assert.AreEqual(1, intHandlers.Length, intHandlers.Select(h => h.GetType()).ToFriendlyNamesText());
            Assert.AreEqual(1, doubleHandlers.Length, doubleHandlers.Select(h => h.GetType()).ToFriendlyNamesText());
        }

        private static void Assert_RegisterCollectionWithAllOpenGenericResultsInExpectedListOfTypes<TService>(
            Type[] openGenericTypesToRegister, Type[] expectedTypes)
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(
                typeof(TService).GetGenericTypeDefinition(),
                openGenericTypesToRegister);

            // Act
            var instances = container.GetAllInstances<TService>().ToArray();

            // Assert
            var actualTypes = instances.Select(instance => instance.GetType()).ToArray();

            Assert.IsTrue(expectedTypes.SequenceEqual(actualTypes),
                "Actual: " + actualTypes.ToFriendlyNamesText());
        }
    }

    public class HandlerWithTwoImplementations : IEventHandler<int>, IEventHandler<double>
    { 
    }
}