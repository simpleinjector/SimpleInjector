namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector;
    using SimpleInjector.Advanced;

    [TestClass]
    public class RegisterCollectionTests
    {
        public interface ILogStuf
        {
        }

        private static readonly Assembly CurrentAssembly = typeof(RegisterCollectionTests).GetTypeInfo().Assembly;

        [TestMethod]
        public void RegisterCollectionTServiceAssemblyArray_RegisteringNonGenericServiceAndAssemblyWithMultipleImplementations_RegistersThoseImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<ILogStuf>(new[] { CurrentAssembly });

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllLoggers(loggers);
        }

        [TestMethod]
        public void RegisterCollectionTServiceAssemblyEnumerable_AccidentallyUsingTheSameAssemblyTwice_RegistersThoseImplementationsOnce()
        {
            // Arrange
            var container = ContainerFactory.New();

            var assemblies = Enumerable.Repeat(CurrentAssembly, 2);

            container.RegisterCollection<ILogStuf>(assemblies);

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllLoggers(loggers);
        }

        [TestMethod]
        public void RegisterCollectionServiceAssemblyArray_RegisteringNonGenericServiceAndAssemblyWithMultipleImplementations_RegistersThoseImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ILogStuf), new[] { CurrentAssembly });

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllLoggers(loggers);
        }

        [TestMethod]
        public void RegisterCollectionServiceAssemblyEnumerable_RegisteringNonGenericServiceAndAssemblyWithMultipleImplementations_RegistersThoseImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ILogStuf), Enumerable.Repeat(CurrentAssembly, 1));

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllLoggers(loggers);
        }

        [TestMethod]
        public void RegisterCollection_UnexpectedCSharpOverloadResolution_ThrowsDescriptiveException()
        {
            // Arrange
            var container = new Container();

            // Act
            // Here the user might think he calls RegisterCollection(Type, params Type[]), but instead
            // RegisterCollection<Type>(new[] { typeof(ILogger), typeof(NullLogger) }) is called. 
            Action action = () => container.RegisterCollection(typeof(ILogger), typeof(NullLogger));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The most likely cause of this happening is because the C# overload resolution picked " +
                "a different method for you than you expected to call. The method C# selected for you is: " +
                "RegisterCollection<Type>",
                action);
        }

        [TestMethod]
        public void RegisterCollection_WithOpenGenericTypeThatIsRegisteredAsSingleton_RespectsTheRegisteredLifestyle()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(GenericEventHandler<>), typeof(GenericEventHandler<>), Lifestyle.Singleton);

            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(GenericEventHandler<>) });

            // Act
            var handler1 = container.GetAllInstances<IEventHandler<int>>().Single();
            var handler2 = container.GetAllInstances<IEventHandler<int>>().Single();

            // Assert
            Assert.AreSame(handler1, handler2, "Singleton was expected.");
        }

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

        [TestMethod]
        public void GetAllInstances_TwoUncontrolledVariantCollectionsRegisteredWithTService_ResolvesInstancesThroughBaseType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<ITypeConverter<DerivedA>>(Enumerable.Repeat(new DerivedAConverter(), 1));
            container.RegisterCollection<ITypeConverter<DerivedB>>(Enumerable.Repeat(new DerivedBConverter(), 1));

            // Act
            var baseConverters = container.GetAllInstances<ITypeConverter<BaseClass>>();
            var types = baseConverters.Select(b => b.GetType()).ToArray();

            // Assert
            AssertThat.SequenceEquals(new[] { typeof(DerivedAConverter), typeof(DerivedBConverter) }, types);
        }

        [TestMethod]
        public void GetAllInstances_TwoUncontrolledVariantCollections_ResolvesInstancesThroughBaseType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ITypeConverter<DerivedA>), new[] { new DerivedAConverter() });
            container.RegisterCollection(typeof(ITypeConverter<DerivedB>), new[] { new DerivedBConverter() });

            // Act
            var baseConverters = container.GetAllInstances<ITypeConverter<BaseClass>>();
            var types = baseConverters.Select(b => b.GetType()).ToArray();

            // Assert
            AssertThat.SequenceEquals(new[] { typeof(DerivedAConverter), typeof(DerivedBConverter) }, types);
        }

        [TestMethod]
        public void GetAllInstances_TwoUncontrolledVariantCollections2_ResolvesInstancesThroughBaseType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<ITypeConverter<DerivedA>>(new[] { new DerivedAConverter() });
            container.RegisterCollection<ITypeConverter<DerivedB>>(new[] { new DerivedBConverter() });

            // Act
            var baseConverters = container.GetAllInstances<ITypeConverter<BaseClass>>();
            var types = baseConverters.Select(b => b.GetType()).ToArray();

            // Assert
            AssertThat.SequenceEquals(new[] { typeof(DerivedAConverter), typeof(DerivedBConverter) }, types);
        }

        [TestMethod]
        public void GetAllInstances_RequestingGenericTypeRegisteredAsOpenGenericEmptyCollection_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(IEventHandler<>), Type.EmptyTypes);

            // Act
            container.GetAllInstances<IEventHandler<UserServiceBase>>();
        }

        [TestMethod]
        public void GetAllInstances_RequestingGenericTypeOnlyRegisteredForADifferentClosedVersion_StillSucceeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(IEventHandler<int>), Type.EmptyTypes);

            // Act
            container.GetAllInstances<IEventHandler<double>>();
        }

        private static void Assert_RegisterCollectionWithAllOpenGenericResultsInExpectedListOfTypes<TService>(
            Type[] openGenericTypesToRegister, Type[] expectedTypes)
            where TService : class
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

        private static void Assert_ContainsAllLoggers(IEnumerable loggers)
        {
            var instances = loggers.Cast<ILogStuf>().ToArray();

            string types = string.Join(", ", instances.Select(instance => instance.GetType().Name));

            Assert.AreEqual(3, instances.Length, "Actual: " + types);
            Assert.IsTrue(instances.OfType<LogStuff1>().Any(), "Actual: " + types);
            Assert.IsTrue(instances.OfType<LogStuff2>().Any(), "Actual: " + types);
            Assert.IsTrue(instances.OfType<LogStuff3>().Any(), "Actual: " + types);
        }

        public class HandlerWithTwoImplementations : IEventHandler<int>, IEventHandler<double>
        {
        }

        public class LogStuff1 : ILogStuf
        {
        }

        public class LogStuff2 : ILogStuf
        {
        }

        public class LogStuff3 : ILogStuf
        {
        }

        public interface ITypeConverter<out TBase>
        {
            // TBase ConvertFromString(string value);
        }

        public class BaseClass
        {
        }

        public class DerivedA : BaseClass
        {
        }

        public class DerivedB : BaseClass
        {
        }

        public class DerivedAConverter : ITypeConverter<DerivedA>
        {
        }

        public class DerivedBConverter : ITypeConverter<DerivedB>
        {
        }
    }
}