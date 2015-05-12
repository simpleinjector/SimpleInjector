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
        public void RegisterAllOpenGeneric_SuppliedWithCompatibleNonGenericType_Succeeds()
        {
            // Arrange
            Type compatibleNonGenericType = typeof(NonGenericEventHandler);

            var container = ContainerFactory.New();

            // Act
            container.RegisterAllOpenGeneric(typeof(IEventHandler<>), compatibleNonGenericType);
        }

        [TestMethod]
        public void RegisterAllOpenGeneric_SuppliedWithCompatibleClosedGenericType_Succeeds()
        {
            // Arrange
            Type compatibleClosedGenericType = typeof(ClassConstraintEventHandler<object>);

            var container = ContainerFactory.New();

            // Act
            container.RegisterAllOpenGeneric(typeof(IEventHandler<>), compatibleClosedGenericType);
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
                "The supplied collection should contain at least one element.", action);
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

        [TestMethod]
        public void GetRelationship_OnRegistrationBuiltByRegisterAllOpenGeneric_ReturnsTheExpectedRelationships()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ILogger, FakeLogger>(Lifestyle.Singleton);

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
        
        // This is a regression test. This bug existed since the introduction of the RegisterAllOpenGeneric.
        [TestMethod]
        public void GetAllInstances_MultipleTypesWithConstructorContainingPrimivive_ThrowsExpectedException()
        {
            // Arrange
            Type[] registeredTypes = new[]
            {
                // Multiple types that can't be built caused by them having multiple constructors.
                typeof(EventHandlerWithConstructorContainingPrimitive<>),
                typeof(EventHandlerWithConstructorContainingPrimitive<>),
            };

            var container = new Container();

            // Act
            Action action = () => container.RegisterAllOpenGeneric(typeof(IEventHandler<>), registeredTypes);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The constructor of type EventHandlerWithConstructorContainingPrimitive<T>",
                action);
        }

        [TestMethod]
        public void GetAllInstances_Scenario_Behavior()
        {
            // Arrange
            var container = new Container();

            container.RegisterManyForOpenGeneric(typeof(IEventHandler<>), container.RegisterAll, new Type[]
            {
                typeof(HandlerWithTwoImplementations)
            });

            // Act
            var handlers = container.GetAllInstances<IEventHandler<int>>().ToArray();

            // Assert
            Assert.AreEqual(1, handlers.Length, handlers.Select(h => h.GetType()).ToFriendlyNamesText());
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
    }

    public class HandlerWithTwoImplementations : IEventHandler<int>, IEventHandler<double>
    { 
    }
}