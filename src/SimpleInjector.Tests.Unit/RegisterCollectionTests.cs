namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector;
    using SimpleInjector.Advanced;

    /// <summary>Tests for RegisterCollection.</summary>
    [TestClass]
    public partial class RegisterCollectionTests
    {
        public interface ILogStuf
        {
        }

        public interface ITypeConverter<out TBase>
        {
            // TBase ConvertFromString(string value);
        }

        private interface IGenericDictionary<T> : IDictionary
        {
        }

        private static readonly Assembly CurrentAssembly = typeof(RegisterCollectionTests).GetTypeInfo().Assembly;

        [TestMethod]
        public void RegisterCollectionTServiceAssemblyArray_RegisteringNonGenericServiceAndAssemblyWithMultipleImplementations_RegistersThoseImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<ILogStuf>(new[] { CurrentAssembly });

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

            container.Collection.Register<ILogStuf>(assemblies);

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

            container.Collection.Register(typeof(ILogStuf), new[] { CurrentAssembly });

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

            container.Collection.Register(typeof(ILogStuf), Enumerable.Repeat(CurrentAssembly, 1));

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
            Action action = () => container.Collection.Register(typeof(ILogger), typeof(NullLogger));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The most likely cause of this happening is because the C# overload resolution picked " +
                "a different method for you than you expected to call. The method C# selected for you is: " +
                "Container.Collection.Register<Type>",
                action);
        }

        [TestMethod]
        public void RegisterCollection_WithOpenGenericTypeThatIsRegisteredAsSingleton_RespectsTheRegisteredLifestyle()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(GenericEventHandler<>), typeof(GenericEventHandler<>), Lifestyle.Singleton);

            container.Collection.Register(typeof(IEventHandler<>), new[] { typeof(GenericEventHandler<>) });

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

            container.Collection.Register(typeof(IEventHandler<>), new[] { typeof(ClassConstraintEventHandler<>) });

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
            Action action = () => container.Collection.Register(typeof(IEventHandler<>), new[] { invalidType });

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
            container.Collection.Register(typeof(IEventHandler<>), new[] { compatibleNonGenericType });
        }

        [TestMethod]
        public void RegisterCollectionWithAllOpenGeneric_SuppliedWithCompatibleClosedGenericType_Succeeds()
        {
            // Arrange
            Type compatibleClosedGenericType = typeof(ClassConstraintEventHandler<object>);

            var container = ContainerFactory.New();

            // Act
            container.Collection.Register(typeof(IEventHandler<>), new[] { compatibleClosedGenericType });
        }

        [TestMethod]
        public void RegisterCollectionWithAllOpenGeneric_WithNullOpenGenericServiceTypeParameter_ThrowsArgumentNullException()
        {
            // Arrange
            Type invalidOpenGenericServiceType = null;

            var container = new Container();

            // Act
            Action action = () => container.Collection.Register(invalidOpenGenericServiceType, new[] { typeof(int) });

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
                (new Container()).Collection.Register(typeof(int), invalidOpenGenericImplementations);

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
                (new Container()).Collection.Register(typeof(IEventHandler<>), invalidOpenGenericImplementations);

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

            container.Collection.Register(typeof(IEventHandler<>), new[] { typeof(EventHandlerWithLoggerDependency<>) });

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

            container.Collection.Register(typeof(IEventHandler<>), new Type[]
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

            container.Collection.Register<ITypeConverter<DerivedA>>(Enumerable.Repeat(new DerivedAConverter(), 1));
            container.Collection.Register<ITypeConverter<DerivedB>>(Enumerable.Repeat(new DerivedBConverter(), 1));

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

            container.Collection.Register(typeof(ITypeConverter<DerivedA>), new[] { new DerivedAConverter() });
            container.Collection.Register(typeof(ITypeConverter<DerivedB>), new[] { new DerivedBConverter() });

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

            container.Collection.Register<ITypeConverter<DerivedA>>(new[] { new DerivedAConverter() });
            container.Collection.Register<ITypeConverter<DerivedB>>(new[] { new DerivedBConverter() });

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

            container.Collection.Register(typeof(IEventHandler<>), Type.EmptyTypes);

            // Act
            container.GetAllInstances<IEventHandler<UserServiceBase>>();
        }

        [TestMethod]
        public void GetAllInstances_RequestingGenericTypeOnlyRegisteredForADifferentClosedVersion_StillSucceeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register(typeof(IEventHandler<int>), Type.EmptyTypes);

            // Act
            container.GetAllInstances<IEventHandler<double>>();
        }

        [TestMethod]
        public void RegisterCollectionTService_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IPlugin> plugins = null;

            // Act
            Action action = () => container.Collection.Register<IPlugin>(plugins);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetInstance_TypeWithEnumerableAsConstructorArguments_InjectsExpectedTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new PluginImpl(), new PluginImpl(), new PluginImpl());

            // Act
            // PluginManager has a constructor with an IEnumerable<IPlugin> argument.
            var manager = container.GetInstance<PluginManager>();

            // Assert
            Assert.AreEqual(3, manager.Plugins.Length);
        }

        [TestMethod]
        public void GetInstance_EnumerableTypeRegisteredWithRegisterSingle_InjectsExpectedTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            IPlugin[] plugins = new IPlugin[] { new PluginImpl(), new PluginImpl(), new PluginImpl() };

            // RegisterInstance<IEnumerable<T>> should have the same effect as RegisterCollection<T>
            container.RegisterInstance<IEnumerable<IPlugin>>(plugins);

            // Act
            // PluginManager has a constructor with an IEnumerable<IPlugin> argument.
            var manager = container.GetInstance<PluginManager>();

            // Assert
            Assert.AreEqual(3, manager.Plugins.Length);
        }

        [TestMethod]
        public void GetInstance_ConcreteTypeWithEnumerableArgumentOfUnregisteredType_InjectsZeroInstances()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            // PluginManager has a constructor with an IEnumerable<IPlugin> argument.
            // We expect this call to succeed, even while no IPlugin implementations are registered.
            Action action = () => container.GetInstance<PluginManager>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "IEnumerable<IPlugin>",
                action);
        }

        [TestMethod]
        public void GetInstance_ConcreteTypeWithEnumerableArgumentForRegisteredCollectionWithoutElements_InjectsZeroInstances()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(Type.EmptyTypes);

            // Act
            // PluginManager has a constructor with an IEnumerable<IPlugin> argument.
            // We expect this call to succeed, even while no IPlugin implementations are registered.
            var manager = container.GetInstance<PluginManager>();

            // Assert
            Assert.AreEqual(0, manager.Plugins.Length);
        }

        [TestMethod]
        public void RegisterSingle_WithEnumerableCalledAfterRegisterCollectionWithSameType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new PluginImpl());

            // Act
            Action action = () => container.RegisterInstance<IEnumerable<IPlugin>>(new IPlugin[0]);

            // Assert
            AssertThat.Throws<NotSupportedException>(action);
        }

        [TestMethod]
        public void Register_WithEnumerableCalledAfterRegisterCollectionWithSameType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new PluginImpl());

            // Act
            Action action = () => container.Register<IEnumerable<IPlugin>>(() => new IPlugin[0]);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "Mixing calls is not supported.",
                action);
        }

        [TestMethod]
        public void RegisterCollection_WithEnumerableCalledAfterRegisterSingleWithSameType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterInstance<IEnumerable<IPlugin>>(new IPlugin[0]);

            // Act
            Action action = () => container.Collection.Register<IPlugin>(new PluginImpl());

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "Mixing calls is not supported.",
                action);
        }

        [TestMethod]
        public void RegisterCollection_WithEnumerableCalledAfterRegisterWithSameType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IEnumerable<IPlugin>>(() => new IPlugin[0]);

            // Act
            Action action = () => container.Collection.Register<IPlugin>(new PluginImpl());

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "Mixing calls is not supported.",
                action);
        }

        [TestMethod]
        public void GetAllInstances_ListRegisteredUsingEnumerable_ReturnsExpectedList()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IUserRepository> repositoryToRegister = new IUserRepository[]
            {
                new InMemoryUserRepository(),
                new SqlUserRepository()
            };

            container.Collection.Register<IUserRepository>(repositoryToRegister);

            // Act
            var repositories = container.GetAllInstances<IUserRepository>();

            // Assert
            Assert.IsNotNull(repositories, "This method MUST NOT return null.");
            Assert.AreEqual(2, repositories.Count(), "Collection is expected to contain two values.");
        }

        [TestMethod]
        public void GetAllInstances_ListRegisteredUsingParams_ReturnsExpectedList()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IUserRepository>(new InMemoryUserRepository(), new SqlUserRepository());

            // Act
            var repositories = container.GetAllInstances<IUserRepository>();

            // Assert
            Assert.IsNotNull(repositories, "This method MUST NOT return null.");
            Assert.AreEqual(2, repositories.Count(), "Collection is expected to contain two values.");
        }

        [TestMethod]
        public void GetAllInstances_NoInstancesRegistered_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetAllInstances<IUserRepository>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "No registration for type IEnumerable<IUserRepository> could be found",
                action);
        }

        [TestMethod]
        public void RegisterCollection_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterInstance<IUserRepository>(new InMemoryUserRepository());
            container.GetInstance<IUserRepository>();

            // Act
            Action action = () => container.Collection.Register<IUserRepository>(new IUserRepository[0]);

            // Assert
            AssertThat.Throws<InvalidOperationException>(action,
                "The container should get locked after a call to GetInstance.");
        }

        [TestMethod]
        public void RegisterCollection_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Collection.Register<IUserRepository>(Type.EmptyTypes);
            var repositories = container.GetAllInstances<IUserRepository>();
            var count = repositories.Count();

            // Act
            Action action = () => container.Collection.Register<IUserRepository>(new IUserRepository[0]);

            // Assert
            AssertThat.Throws<InvalidOperationException>(action,
                "The container should get locked after a call to GetAllInstances.");
        }

        [TestMethod]
        public void RegisterCollectionParamsT_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IUserRepository[] repositories = null;

            // Act
            Action action = () => container.Collection.Register<IUserRepository>(repositories);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterCollectionParamsType_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type[] repositoryTypes = null;

            // Act
            Action action = () => container.Collection.Register<IUserRepository>(repositoryTypes);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterCollectionIEnumerableT_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IUserRepository> repositories = null;

            // Act
            Action action = () => container.Collection.Register<IUserRepository>(repositories);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterCollectionIEnumerableType_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Type> repositoryTypes = null;

            // Act
            Action action = () => container.Collection.Register<IUserRepository>(repositoryTypes);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterCollection_WithListOfTypes_ThrowsExpressiveExceptionExplainingAboutAmbiguity()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                // Act
                container.Collection.Register(new[] { typeof(IUserRepository) });

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.ExceptionMessageContains(
                    @"You are trying to register Type as a service type,
                      but registering this type is not allowed to be registered"
                    .TrimInside(),
                    ex,
                    "This call is expected to fail, since C# overload resolution will select the " +
                    "RegisterCollection<TService> overload where TService is Type, which is unlikely what the " +
                    "use intended. We should throw an exception instead.");
            }
        }

        [TestMethod]
        public void RegisterCollection_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            var repositories = new IUserRepository[] { new InMemoryUserRepository(), new SqlUserRepository() };
            container.Collection.Register<IUserRepository>(repositories);

            // Act
            Action action = () => container.Collection.Register<IUserRepository>(repositories);

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void GetAllInstancesNonGeneric_WithoutEmptyRegistration_ReturnsAnEmptyCollection()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IUserRepository>(new Assembly[0]);

            // Act
            var repositories = container.GetAllInstances(typeof(IUserRepository));

            // Assert
            Assert.AreEqual(0, repositories.Count());
        }

        [TestMethod]
        public void GetAllInstancesNonGeneric_WithValidRegistration_ReturnsCollectionWithExpectedElements()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IUserRepository>(new InMemoryUserRepository(), new SqlUserRepository());

            // Act
            var repositories = container.GetAllInstances(typeof(IUserRepository)).ToArray();

            // Assert
            Assert.AreEqual(2, repositories.Length);
            AssertThat.IsInstanceOfType(typeof(InMemoryUserRepository), repositories[0]);
            AssertThat.IsInstanceOfType(typeof(SqlUserRepository), repositories[1]);
        }

        [TestMethod]
        public void GetAllInstances_InvalidDelegateRegistered_ThrowsExceptionWithExpectedMessage()
        {
            // Arrange
            string expectedMessage =
                "The registered delegate for type IEnumerable<IUserRepository> returned null.";

            var container = ContainerFactory.New();

            Func<IEnumerable<IUserRepository>> invalidDelegate = () => null;

            container.Register<IEnumerable<IUserRepository>>(invalidDelegate);

            try
            {
                // Act
                container.GetAllInstances<IUserRepository>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void GetAllInstances_WithArrayRegistered_DoesNotReturnAnArray()
        {
            // Arrange
            var container = ContainerFactory.New();

            var repositories = new IUserRepository[] { new SqlUserRepository(), new InMemoryUserRepository() };

            container.Collection.Register<IUserRepository>(repositories);

            // Act
            var collection = container.GetAllInstances<IUserRepository>();

            // Assert
            Assert_IsNotAMutableCollection(collection);
        }

        [TestMethod]
        public void GetAllInstances_WithArrayRegistered_DoesNotAllowChangesToTheOriginalArray()
        {
            // Arrange
            var container = ContainerFactory.New();

            var repositories = new IUserRepository[] { new SqlUserRepository(), new InMemoryUserRepository() };

            container.Collection.Register<IUserRepository>(repositories);

            repositories[0] = null;

            // Act
            var collection = container.GetAllInstances<IUserRepository>().ToArray();

            // Assert
            Assert.IsNotNull(collection[0], "RegisterCollection<T>(T[]) did not make a copy of the supplied array.");
        }

        [TestMethod]
        public void GetAllInstances_WithListRegistered_DoesNotReturnAnArray()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IUserRepository>(new List<IUserRepository> { new SqlUserRepository(), new InMemoryUserRepository() });

            // Act
            var collection = container.GetAllInstances<IUserRepository>();

            // Assert
            Assert_IsNotAMutableCollection(collection);
        }

        [TestMethod]
        public void GetAllInstances_WithCollectionRegistered_DoesNotReturnAnArray()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IUserRepository>(new Collection<IUserRepository>
            {
                new SqlUserRepository(),
                new InMemoryUserRepository()
            });

            // Act
            var collection = container.GetAllInstances<IUserRepository>();

            // Assert
            Assert_IsNotAMutableCollection(collection);
        }

        [TestMethod]
        public void GetAllInstances_WithArray_ReturnsSameInstanceOnEachCall()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IUserRepository>(new IUserRepository[] { new SqlUserRepository(), new InMemoryUserRepository() });

            // Act
            var collection1 = container.GetAllInstances<IUserRepository>();
            var collection2 = container.GetAllInstances<IUserRepository>();

            // Assert
            Assert.AreEqual(collection1, collection2,
                "For performance reasons, GetAllInstances<T> should always return the same instance.");
        }

        [TestMethod]
        public void GetInstance_OnATypeThatDependsOnACollectionThatIsRegisteredWithRegisterByFunc_CollectionShouldNotBeTreatedAsASingleton()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IPlugin> left = new LeftEnumerable<IPlugin>();
            IEnumerable<IPlugin> right = new RightEnumerable<IPlugin>();
            bool returnLeft = true;

            container.Register<IEnumerable<IPlugin>>(() => returnLeft ? left : right);

            // This call will compile the delegate for the PluginContainer
            var firstContainer = container.GetInstance<PluginContainer>();

            Assert.AreEqual(firstContainer.Plugins, left, "Test setup failed.");

            returnLeft = false;

            // Act
            var secondContainer = container.GetInstance<PluginContainer>();

            // Assert
            Assert.AreEqual(secondContainer.Plugins, right,
                "When using Register<T> to register collections, the collection should not be treated as a " +
                "singleton.");
        }

        [TestMethod]
        public void RegisterCollectionTService_RegisteredCollectionWithNullElements_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Collection.Register<IUserRepository>(new IUserRepository[] { null });

            // Assert
            AssertThat.Throws<ArgumentException>(action);
        }

        [TestMethod]
        public void RegisterCollection_WithOpenGenericType_FailsWithExpectedExceptionMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                // Act
                container.Collection.Register<IDictionary>(new[] { typeof(IGenericDictionary<>) });

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains("open-generic type"), "Actual: " + ex.Message);
            }
        }

        [TestMethod]
        public void RegisterCollection_WithValidCollectionOfServices_Succeeds()
        {
            // Arrange
            var instance = new SqlUserRepository();

            var container = ContainerFactory.New();

            // Act
            container.Collection.Register(typeof(IUserRepository), new IUserRepository[] { instance });

            // Assert
            var instances = container.GetAllInstances<IUserRepository>();

            Assert.AreEqual(1, instances.Count());
            Assert.AreEqual(instance, instances.First());
        }

        [TestMethod]
        public void RegisterCollection_WithValidObjectCollectionOfServices_Succeeds()
        {
            // Arrange
            var instance = new SqlUserRepository();

            var container = ContainerFactory.New();

            // Act
            container.Collection.Register(typeof(IUserRepository), new object[] { instance });

            // Assert
            var instances = container.GetAllInstances<IUserRepository>();

            Assert.AreEqual(1, instances.Count());
            Assert.AreEqual(instance, instances.First());
        }

        [TestMethod]
        public void RegisterCollection_WithValidCollectionOfImplementations_Succeeds()
        {
            // Arrange
            var instance = new SqlUserRepository();

            var container = ContainerFactory.New();

            // Act
            container.Collection.Register(typeof(IUserRepository), new SqlUserRepository[] { instance });

            // Assert
            var instances = container.GetAllInstances<IUserRepository>();

            Assert.AreEqual(1, instances.Count());
            Assert.AreEqual(instance, instances.First());
        }

        [TestMethod]
        public void RegisterCollection_WithValidListOfTypes_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            // IServiceEx is a valid registration, because it could be registered.
            container.Collection.Register<IUserRepository>(new[] { typeof(SqlUserRepository), typeof(IUserRepository) });
        }

        [TestMethod]
        public void RegisterCollection_WithValidEnumeableOfTypes_Succeeds()
        {
            // Arrange
            IEnumerable<Type> types = new[] { typeof(SqlUserRepository) };

            var container = ContainerFactory.New();

            // Act
            container.Collection.Register<IUserRepository>(types);
        }

        [TestMethod]
        public void RegisterCollection_WithValidParamListOfTypes_Succeeds()
        {
            // Arrange
            Type[] types = new[] { typeof(SqlUserRepository) };

            var container = ContainerFactory.New();

            // Act
            container.Collection.Register<IUserRepository>(types);
        }

        [TestMethod]
        public void GetAllInstances_RegisteringValidListOfTypesWithRegisterCollection_ReturnsExpectedList()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IUserRepository>(new[] { typeof(SqlUserRepository) });

            // Act
            container.Verify();

            var list = container.GetAllInstances<IUserRepository>().ToArray();

            // Assert
            Assert.AreEqual(1, list.Length);
            AssertThat.IsInstanceOfType(typeof(SqlUserRepository), list[0]);
        }

        [TestMethod]
        public void RegisterCollection_WithInvalidListOfTypes_ThrowsExceptionWithExpectedMessage()
        {
            // Arrange
            string expectedMessage = "The supplied type IDisposable does not implement IUserRepository.";

            var container = ContainerFactory.New();

            try
            {
                // Act
                container.Collection.Register<IUserRepository>(new[]
                {
                    typeof(SqlUserRepository),
                    typeof(IDisposable)
                });

                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void RegisterCollection_RegisteringATypeThatEqualsTheRegisteredServiceType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            // Act
            // Registers a type that references the registration above.
            container.Collection.Register<IUserRepository>(new[] { typeof(SqlUserRepository), typeof(IUserRepository) });
        }

        [TestMethod]
        public void RegisterCollection_RegisteringAnInterfaceOnACollectionOfObjects_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.Collection.Register<object>(typeof(IDisposable));
        }

        [TestMethod]
        public void GetAllInstances_RegisterCollectionWithRegistration_ResolvesTheExpectedInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Registration> registrations = new[]
            {
                Lifestyle.Transient.CreateRegistration<SqlUserRepository>(container)
            };

            container.Collection.Register(typeof(IUserRepository), registrations);

            // Act
            var repository = container.GetAllInstances<IUserRepository>().Single();

            // Assert
            AssertThat.IsInstanceOfType(typeof(SqlUserRepository), repository);
        }

        [TestMethod]
        public void GetAllInstances_RegistrationUsedInMultipleCollections_ResolvesTheExpectedInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration =
                Lifestyle.Singleton.CreateRegistration<SqlUserRepository>(container);

            container.Collection.Register(typeof(IUserRepository), new[] { registration });
            container.Collection.Register(typeof(object), new[] { registration });

            // Act
            var instance1 = container.GetAllInstances<IUserRepository>().Single();
            var instance2 = container.GetAllInstances<object>().Single();

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        [TestMethod]
        public void GetAllInstances_RegisterCollectionWithRegistrationsAndDecorator_WrapsTheDecorator()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[]
            {
                Lifestyle.Transient.CreateRegistration<PluginImpl>(container),
                Lifestyle.Transient.CreateRegistration<PluginImpl2>(container)
            });

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            // Act
            var plugins = container.GetAllInstances<IPlugin>().ToArray();

            // Assert
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), plugins[0]);
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), plugins[1]);
        }

        [TestMethod]
        public void RegisterCollectionGeneric_RegisteringCovarientTypes_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<ICovariant<object>>(new[] { typeof(CovariantImplementation<string>) });

            // Act
            var instances = container.GetAllInstances<ICovariant<object>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(CovariantImplementation<string>), instances.Single());
        }

        [TestMethod]
        public void RegisterCollectionNonGeneric_RegisteringCovarientTypes_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register(typeof(ICovariant<object>), new[] { typeof(CovariantImplementation<string>) });

            // Act
            var instances = container.GetAllInstances<ICovariant<object>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(CovariantImplementation<string>), instances.Single());
        }

        [TestMethod]
        public void RegisterCollection_MixOfOpenClosedAndNonGenericTypes_ResolvesExpectedTypesInExpectedOrder1()
        {
            // Arrange
            Type[] registeredTypes = new[]
            {
                // This is a mix of open, closed and non-generic types.
                typeof(NewConstraintEventHandler<>), // IEventHandler<T> where T : new()
                typeof(StructConstraintEventHandler<>), // IEventHandler<T> where T : struct
                typeof(StructEventHandler), // IEventHandler<StructEvent>
                typeof(AuditableEventEventHandler), // IEventHandler<AuditableEvent>
                typeof(ClassConstraintEventHandler<AuditableEvent>), // IEventHandler<AuditableEvent>
                typeof(ClassConstraintEventHandler<ClassEvent>), // IEventHandler<ClassEvent>
                typeof(AuditableEventEventHandler<>), // IEventHandler<T> where T : IAuditableEvent
            };

            Type resolvedHandlerType = typeof(IEventHandler<StructEvent>);

            Type[] expectedHandlerTypes = new[]
            {
                typeof(NewConstraintEventHandler<StructEvent>),
                typeof(StructConstraintEventHandler<StructEvent>),
                typeof(StructEventHandler),
                typeof(AuditableEventEventHandler<StructEvent>),
            };

            var container = ContainerFactory.New();

            container.Collection.Register(typeof(IEventHandler<>), registeredTypes);

            // Act
            Type[] actualHandlerTypes = container.GetAllInstances(resolvedHandlerType)
                .Select(h => h.GetType()).ToArray();

            // Assert
            Assert.AreEqual(
                expected: string.Join(", ", expectedHandlerTypes.Select(TypesExtensions.ToFriendlyName)),
                actual: string.Join(", ", actualHandlerTypes.Select(TypesExtensions.ToFriendlyName)));
        }

        [TestMethod]
        public void RegisterCollection_MixOfOpenClosedAndNonGenericTypes_ResolvesExpectedTypesInExpectedOrder2()
        {
            // Arrange
            Type[] registeredTypes = new[]
            {
                // This is a mix of open, closed and non-generic types.
                typeof(AuditableEventEventHandler<>),
                typeof(NewConstraintEventHandler<>),
                typeof(StructConstraintEventHandler<>),
                typeof(StructEventHandler),
                typeof(AuditableEventEventHandler),
                typeof(ClassConstraintEventHandler<AuditableEvent>),
                typeof(ClassConstraintEventHandler<ClassEvent>),
            };

            Type resolvedHandlerType = typeof(IEventHandler<AuditableEvent>);

            Type[] expectedHandlerTypes = new[]
            {
                typeof(AuditableEventEventHandler<AuditableEvent>),
                typeof(NewConstraintEventHandler<AuditableEvent>),
                typeof(AuditableEventEventHandler),
                typeof(ClassConstraintEventHandler<AuditableEvent>),
            };

            var container = ContainerFactory.New();

            container.Collection.Register(typeof(IEventHandler<>), registeredTypes);

            // Act
            Type[] actualHandlerTypes = container.GetAllInstances(resolvedHandlerType)
                .Select(h => h.GetType()).ToArray();

            // Assert
            Assert.AreEqual(
                expected: expectedHandlerTypes.ToFriendlyNamesText(),
                actual: actualHandlerTypes.ToFriendlyNamesText());
        }

        [TestMethod]
        public void RegisterCollection_MixOfOpenClosedAndNonGenericTypes_ResolvesExpectedTypesInExpectedOrder3()
        {
            // Arrange
            Type[] registeredTypes = new[]
            {
                // This is a mix of open, closed and non-generic types.
                typeof(AuditableEventEventHandler<>),
                typeof(NewConstraintEventHandler<>),
                typeof(StructConstraintEventHandler<>),
                typeof(StructEventHandler),
                typeof(AuditableEventEventHandler),
                typeof(ClassConstraintEventHandler<AuditableEvent>),
                typeof(ClassConstraintEventHandler<ClassEvent>),
            };

            Type resolvedHandlerType = typeof(IEventHandler<NoDefaultConstructorEvent>);

            Type[] expectedHandlerTypes = new Type[]
            {
                // Empty
            };

            var container = ContainerFactory.New();

            container.Collection.Register(typeof(IEventHandler<>), registeredTypes);

            // Act
            Type[] actualHandlerTypes = container.GetAllInstances(resolvedHandlerType)
                .Select(h => h.GetType()).ToArray();

            // Assert
            Assert.AreEqual(
                expected: expectedHandlerTypes.ToFriendlyNamesText(),
                actual: actualHandlerTypes.ToFriendlyNamesText());
        }

        [TestMethod]
        public void GetAllInstances_ResolvingAnTypeThatHasNoConcreteImplementations_ResolvesApplicableOpenGenericTypes()
        {
            // Arrange
            Type[] registeredTypes = new[]
            {
                // This is a mix of open, closed and non-generic types.
                typeof(AuditableEventEventHandler<>),
                typeof(NewConstraintEventHandler<>),
                typeof(StructConstraintEventHandler<>),
                typeof(StructEventHandler),
                typeof(AuditableEventEventHandler),
                typeof(ClassConstraintEventHandler<AuditableEvent>),
                typeof(ClassConstraintEventHandler<ClassEvent>),
            };

            Type resolvedHandlerType = typeof(IEventHandler<AuditableStructEvent>);

            Type[] expectedHandlerTypes = new Type[]
            {
                typeof(AuditableEventEventHandler<AuditableStructEvent>),
                typeof(NewConstraintEventHandler<AuditableStructEvent>),
                typeof(StructConstraintEventHandler<AuditableStructEvent>),
            };

            var container = ContainerFactory.New();

            container.Collection.Register(typeof(IEventHandler<>), registeredTypes);

            // Act
            Type[] actualHandlerTypes = container.GetAllInstances(resolvedHandlerType)
                .Select(h => h.GetType()).ToArray();

            // Assert
            Assert.AreEqual(
                expected: expectedHandlerTypes.ToFriendlyNamesText(),
                actual: actualHandlerTypes.ToFriendlyNamesText());
        }

        [TestMethod]
        public void RegisterCollection_SuppliedWithATypeThatContainsUnresolvableTypeArguments_ThrowsDescriptiveException()
        {
            // Arrange
            Type[] registeredTypes = new[]
            {
                typeof(AuditableEventEventHandler<>),
                typeof(NewConstraintEventHandler<>),
                typeof(StructConstraintEventHandler<>),

                // This one is the ugly bugger!
                typeof(AuditableEventEventHandlerWithUnknown<>),
            };

            Type resolvedHandlerType = typeof(IEventHandler<AuditableEvent>);

            var container = ContainerFactory.New();

            // Act
            Action action = () => container.Collection.Register(typeof(IEventHandler<>), registeredTypes);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied type AuditableEventEventHandlerWithUnknown<TUnknown> contains unresolvable " +
                "type arguments. The type would never be resolved and is therefore not suited to be used.",
                action);
        }

        // This is a regression test. This bug existed since the introduction of the RegisterAllOpenGeneric.
        [TestMethod]
        public void GetAllInstances_MultipleTypesWithConstructorContainingPrimivive_ThrowsExpectedException()
        {
            // Arrange
            Type[] registeredTypes = new[]
            {
                // Multiple types that can't be built caused by them having multiple constructors.
                typeof(EventHandlerWithConstructorContainingPrimitive<StructEvent>),
                typeof(EventHandlerWithConstructorContainingPrimitive<StructEvent>),
            };

            var container = new Container();

            // RegisterCollection will not throw an exception, because registration is forwarded back into the
            // container, and it could be possible that someone does a registration like:
            // container.Register(typeof(EventHandlerWithConstructorContainingPrimitive<>), typeof(X))
            // where X is a type with one constructor.
            container.Collection.Register(typeof(IEventHandler<>), registeredTypes);

            // Act
            Action action = () => container.GetAllInstances(typeof(IEventHandler<StructEvent>)).ToArray();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "The constructor of type EventHandlerWithConstructorContainingPrimitive<StructEvent>",
                action);
        }

        [TestMethod]
        public void RegisterCollection_WithNonGenericType_DelegatesBackIntoTheContainer()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Register<StructEventHandler>(Lifestyle.Singleton);
            container.Collection.Register(typeof(IEventHandler<>), new[] { typeof(StructEventHandler) });

            // Assert
            var handlers = container.GetAllInstances<IEventHandler<StructEvent>>();

            Assert.AreSame(handlers.Single(), handlers.Single(), "The container didn't delegate back.");
        }

        [TestMethod]
        public void RegisterCollection_WithClosedGenericType_DelegatesBackIntoTheContainer()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Register<ClassConstraintEventHandler<AuditableEvent>>(Lifestyle.Singleton);
            container.Collection.Register(typeof(IEventHandler<>), new[]
            {
                typeof(ClassConstraintEventHandler<AuditableEvent>)
            });

            // Assert
            var handlers = container.GetAllInstances<IEventHandler<AuditableEvent>>();

            Assert.AreSame(handlers.Single(), handlers.Single(), "The container didn't delegate back.");
        }

        [TestMethod]
        public void RegisterCollection_WithOpenGenericType_DelegatesBackIntoTheContainer()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Register(typeof(NewConstraintEventHandler<>),
                typeof(NewConstraintEventHandler<>), Lifestyle.Singleton);

            container.Collection.Register(typeof(IEventHandler<>), new[]
            {
                typeof(NewConstraintEventHandler<>)
            });

            // Assert
            var handlers = container.GetAllInstances<IEventHandler<DefaultConstructorEvent>>();

            Assert.AreSame(handlers.Single(), handlers.Single(), "The container didn't delegate back.");
        }

        [TestMethod]
        public void RegisterCollection_WithClosedGenericType_DelegatesBackIntoTheContainerToOpenGenericRegistration()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Register(
                typeof(NewConstraintEventHandler<>),
                typeof(NewConstraintEventHandler<>),
                Lifestyle.Singleton);

            container.Collection.Register(typeof(IEventHandler<>), new[]
            {
                typeof(NewConstraintEventHandler<DefaultConstructorEvent>)
            });

            // Assert
            var handlers = container.GetAllInstances<IEventHandler<DefaultConstructorEvent>>();

            Assert.AreSame(handlers.Single(), handlers.Single(), "The container didn't delegate back.");
        }

        [TestMethod]
        public void RegisterCollection_WithOpenGenericType_DelegatesBackIntoTheContainerToCloseRegistration()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Register<ClassConstraintEventHandler<AuditableEvent>>(Lifestyle.Singleton);

            container.Collection.Register(typeof(IEventHandler<>), new[]
            {
                typeof(ClassConstraintEventHandler<>)
            });

            // Assert
            var handlers = container.GetAllInstances<IEventHandler<AuditableEvent>>();

            Assert.AreSame(handlers.Single(), handlers.Single(), "The container didn't delegate back.");
        }

        [TestMethod]
        public void RegisterCollection_WithAbstractType_DelegatesBackIntoTheContainerToCloseRegistration()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Register<IEventHandler<AuditableEvent>, ClassConstraintEventHandler<AuditableEvent>>();

            container.Collection.Register(typeof(IEventHandler<>), new[]
            {
                typeof(IEventHandler<>)
            });

            // Assert
            var handler = container.GetAllInstances<IEventHandler<AuditableEvent>>().Single();

            AssertThat.IsInstanceOfType(typeof(ClassConstraintEventHandler<AuditableEvent>), handler);
        }

        [TestMethod]
        public void Verify_ClosedTypeWithUnregisteredDependency_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register(typeof(IEventHandler<>), new[]
            {
                // This closed generic type has an ILogger constructor dependency, but ILogger is not
                // registered, and Verify() should catch this.
                typeof(EventHandlerWithDependency<AuditableEvent, ILogger>)
            });

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "For ILogger to be resolved, it must be registered in the container.",
                action);
        }

        [TestMethod]
        public void Verify_OpenTypeWithUnregisteredDependencyThatIsPartOfCollectionWithNonGenericType_ThrowsExpectedException()
        {
            // Arrange
            Type unregisteredDependencyType = typeof(ILogger);

            var container = ContainerFactory.New();

            container.Collection.Register(typeof(IEventHandler<>), new[]
            {
                // This open generic type has an ILogger constructor dependency, but ILogger is not registered,
                // and since it will be part of collection that contains a non-generic type (the collection
                // IEnumerable<IEventHandler<StructEvent>>) Verify() should be able to catch this.
                typeof(EventHandlerWithDependency<,>)
                    .MakePartialOpenGenericType(secondArgument: typeof(ILogger)),
                typeof(StructEventHandler),
            });

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "For ILogger to be resolved, it must be registered in the container",
                action);
        }

        [TestMethod]
        public void Verify_ClosedTypeWithUnregisteredDependencyResolvedBeforeCallingVerify_StillThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.EnableAutoVerification = false;

            container.Collection.Register(typeof(IEventHandler<>), new[]
            {
                typeof(EventHandlerWithDependency<AuditableEvent, ILogger>)
            });

            // There was a bug in the library that caused collections to be unverified if the collection
            // was resolved before Verify was called.
            container.GetAllInstances<IEventHandler<AuditableEvent>>();

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "For ILogger to be resolved, it must be registered in the container.",
                action);
        }

        [TestMethod]
        public void Verify_NonGenericTypeWithUnregisteredDependencyResolvedBeforeCallingVerify_StillThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register(typeof(IEventHandler<>), Type.EmptyTypes);

            container.Collection.Register(typeof(UserServiceBase), new[]
            {
                // Depends on unregistered type IUserRepository
                typeof(RealUserService)
            });

            // There was a bug in the library that caused collections to be unverified if the collection
            // was resolved before Verify was called.
            container.GetAllInstances<IEventHandler<UserServiceBase>>();

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "For IUserRepository to be resolved, it must be registered in the container",
                action);
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnICollection_InjectsTheRegisteredCollection()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            // Act
            ICollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<ICollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, collection.Count);
            AssertThat.IsInstanceOfType(typeof(PluginImpl), collection.First());
            AssertThat.IsInstanceOfType(typeof(PluginImpl2), collection.Second());
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnCollection_InjectsTheRegisteredCollection()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            // Act
            Collection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<Collection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, collection.Count);
            AssertThat.IsInstanceOfType(typeof(PluginImpl), collection.First());
            AssertThat.IsInstanceOfType(typeof(PluginImpl2), collection.Second());
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnICollection_InjectsTheRegisteredCollectionOfDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            // Act
            ICollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<ICollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, collection.Count);
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), collection.First());
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), collection.Second());
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnICollection_InjectsEmptyCollectionWhenNoInstancesRegistered()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(Type.EmptyTypes);

            // Act
            ICollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<ICollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnIList_InjectsTheRegisteredList()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            // Act
            IList<IPlugin> list = container.GetInstance<ClassDependingOn<IList<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, list.Count);
            AssertThat.IsInstanceOfType(typeof(PluginImpl), list[0]);
            AssertThat.IsInstanceOfType(typeof(PluginImpl2), list[1]);
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnList_InjectsTheRegisteredList()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            // Act
            List<IPlugin> list = container.GetInstance<ClassDependingOn<List<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, list.Count);
            AssertThat.IsInstanceOfType(typeof(PluginImpl), list[0]);
            AssertThat.IsInstanceOfType(typeof(PluginImpl2), list[1]);
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnIList_InjectsTheRegisteredListOfDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            // Act
            IList<IPlugin> list =
                container.GetInstance<ClassDependingOn<IList<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, list.Count);
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), list[0]);
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), list[0]);
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnIList_InjectsEmptyListWhenNoInstancesRegistered()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(Type.EmptyTypes);

            // Act
            IList<IPlugin> list =
                container.GetInstance<ClassDependingOn<IList<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void GetInstance_OnTypeWithArrayArgumentAfterCallingRegisterArrays_InjectsExpectedNumberOfArguments()
        {
            // Arrange
            var container = ContainerFactory.New();

            ICommand singletonCommand = new ConcreteCommand();

            container.Collection.Register<ICommand>(singletonCommand);

            // Act
            var composite = container.GetInstance<CompositeCommand>();

            // Assert
            Assert.IsNotNull(composite.Commands);
            Assert.AreEqual(1, composite.Commands.Length);
        }

        [TestMethod]
        public void GetInstance_OnTypeWithArrayArgumentAfterCallingRegisterArrays_InjectsExpectedElement()
        {
            // Arrange
            var expectedCommand = new ConcreteCommand();

            var container = ContainerFactory.New();

            container.Collection.Register<ICommand>(expectedCommand);

            // Act
            var composite = container.GetInstance<CompositeCommand>();

            // Assert
            Assert.AreEqual(expectedCommand, composite.Commands[0]);
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnContainerControlledCollection_InjectsANewArrayOnEachRequest()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<ICommand>(new[] { typeof(ConcreteCommand) });

            // Act
            var composite = container.GetInstance<CompositeCommand>();

            composite.Commands[0] = null;

            composite = container.GetInstance<CompositeCommand>();

            // Assert
            Assert.IsNotNull(composite.Commands[0],
                "The element in the array is expected NOT to be null. When it is null, it means that the " +
                "array has been cached.");
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnContainerControlledCollection_InjectsANewListOnEachRequest()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<ConcreteCommand>();

            container.Collection.Register<ICommand>(new[] { typeof(ConcreteCommand) });

            // Act
            var injectedList = container.GetInstance<ServiceDependingOn<List<ICommand>>>().Dependency;

            injectedList[0] = null;

            injectedList = container.GetInstance<ServiceDependingOn<List<ICommand>>>().Dependency;

            // Assert
            Assert.IsNotNull(injectedList[0],
                "The element in the array is expected NOT to be null. When it is null, it means that the " +
                "array has been cached.");
        }

        [TestMethod]
        public void GetInstance_AResolvedCollectionOfT_CanNotBeChanged()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<ConcreteCommand>();

            container.Collection.Register<ICommand>(new[] { typeof(ConcreteCommand) });

            var collection = container.GetInstance<Collection<ICommand>>();

            // Act
            Action action = () => collection[0] = new ConcreteCommand();

            // Assert
            Assert.ThrowsException<NotSupportedException>(action,
                "Changing the collection should be blocked by Simple Injector.");
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnContainerControlledSingletons_StillInjectsANewArrayOnEachRequest()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<ICommand>(new ConcreteCommand());

            // Act
            ICommand[] commands = container.GetInstance<ServiceDependingOn<ICommand[]>>().Dependency;

            commands[0] = null;

            commands = container.GetInstance<ServiceDependingOn<ICommand[]>>().Dependency;

            // Assert
            Assert.IsNotNull(commands[0],
                "The element in the array is expected NOT to be null. When it is null, it means that the " +
                "array has been cached.");
        }

        [TestMethod]
        public void GetInstance_ResolvingACollectionOfT_IsSingleton()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<ICommand>(new ConcreteCommand());

            // Act
            Collection<ICommand> commands1 = container.GetInstance<Collection<ICommand>>();
            Collection<ICommand> commands2 = container.GetInstance<Collection<ICommand>>();

            // Assert
            Assert.AreSame(commands1, commands2,
                "Collection<T> is just a wrapper for a container controlled collection IList<T>. And should therefore be a singleton.");

            Assert.AreSame(Lifestyle.Singleton, container.GetRegistration(typeof(Collection<ICommand>)).Lifestyle);
        }

        public class MyList<T> : IList<T>
        {
            public T this[int index]
            {
                get => default(T);
                set { }
            }

            public int Count => throw new NotImplementedException();

            public bool IsReadOnly => throw new NotImplementedException();

            public void Add(T item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(T item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<T> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public int IndexOf(T item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, T item)
            {
                throw new NotImplementedException();
            }

            public bool Remove(T item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void GetInstance_CollectionOfT_FunctionsAsAStream()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<ICommand>(typeof(ConcreteCommand));

            // Act
            Collection<ICommand> commands = container.GetInstance<Collection<ICommand>>();

            // Assert
            Assert.AreNotSame(commands[0], commands[0],
                "Requesting an instance from the collection thould cause a callback into the Container " +
                "causing the type to be resolved again.");

            Assert.AreSame(Lifestyle.Singleton, container.GetRegistration(typeof(Collection<ICommand>)).Lifestyle);
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesOnContainerUncontrolledCollection_StillInjectsANewArrayOnEachRequest()
        {
            // Arrange
            var container = ContainerFactory.New();

            List<ICommand> commands = new List<ICommand>();

            // Add a first command
            commands.Add(new ConcreteCommand());

            container.Collection.Register<ICommand>(commands);

            container.GetInstance<CompositeCommand>();

            // Add yet another command
            commands.Add(new ConcreteCommand());

            // Act
            var composite = container.GetInstance<CompositeCommand>();

            // Assert
            Assert.AreEqual(2, composite.Commands.Length, "The IEnumerable<ICommand> collection should be " +
                "cached by its reference, and not by its current content, because that content is allowed " +
                "to change.");
        }

        [TestMethod]
        public void GetRegistration_RequestingArrayRegistrationContainerControlledCollection_HasTheTransientLifestyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<ICommand>(new[] { typeof(ConcreteCommand) });

            // Act
            var registration = container.GetRegistration(typeof(ICommand[]));

            // Assert
            Assert.AreEqual(Lifestyle.Transient, registration.Lifestyle,
                "Array must be resolved as transient, because it is a mutable type.");
        }

        [TestMethod]
        public void GetRegistration_RequestingListRegistrationContainerControlledCollection_HasTheTransientLifestyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<ICommand>(new[] { typeof(ConcreteCommand) });

            // Act
            var registration = container.GetRegistration(typeof(List<ICommand>));

            // Assert
            Assert.AreEqual(Lifestyle.Transient, registration.Lifestyle,
                "List must be resolved as transient, because it is a mutable type.");
        }

        [TestMethod]
        public void GetRegistration_RequestingCollectionRegistrationContainerControlledCollection_HasTheTransientLifestyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<ICommand>(new[] { typeof(ConcreteCommand) });

            // Act
            var registration = container.GetRegistration(typeof(List<ICommand>));

            // Assert
            Assert.AreEqual(Lifestyle.Transient, registration.Lifestyle,
                "Although Collection technically doesn't have to a a transient, we chose to keep it that way. " +
                "See #545 for more details.");
        }

        [TestMethod]
        public void GetRegistration_RequestingArrayRegistrationUncontainerControlledCollection_HasTheTransientLifestyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<ICommand> commands = new List<ICommand> { new ConcreteCommand() };

            container.Collection.Register<ICommand>(commands);

            // Act
            var registration = container.GetRegistration(typeof(ICommand[]));

            // Assert
            Assert.AreEqual(Lifestyle.Transient, registration.Lifestyle);
        }

        [TestMethod]
        public void GetRegistration_RequestingArrayRegistrationContainerControlledCollectionThatOnlyContainsSingletons_StillHasTheTransientLifestyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ConcreteCommand>(Lifestyle.Singleton);

            container.Collection.Register<ICommand>(new[] { typeof(ConcreteCommand) });

            // Act
            var registration = container.GetRegistration(typeof(ICommand[]));

            // Assert
            Assert.AreEqual(Lifestyle.Transient, registration.Lifestyle,
                "We still expect the transient lifestyle here, since a new array is created every time. " +
                "We might change this in the future or make the diagnostic services smarter, but this is " +
                "quite hard and probably not useful at all. Instead of injecting arrays, users should be " +
                "injecting streams anyway.");
        }

        [TestMethod]
        public void GetAllInstances_RequestingAContravariantInterface_ResolvesAllAssignableImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            // NOTE: CustomerMovedAbroadEvent inherits from CustomerMovedEvent.
            var expectedHandlerTypes = new[]
            {
                typeof(CustomerMovedEventHandler), // IEventHandler<CustomerMovedEvent>
                typeof(CustomerMovedAbroadEventHandler) // IEventHandler<CustomerMovedAbroadEvent>
            };

            // IEventHandler<in TEvent> is contravariant.
            container.Collection.Register(typeof(IEventHandler<>), expectedHandlerTypes);

            // Act
            var handlers = container.GetAllInstances<IEventHandler<CustomerMovedAbroadEvent>>();
            Type[] actualHandlerTypes = handlers.Select(handler => handler.GetType()).ToArray();

            // Assert
            Assert.IsTrue(expectedHandlerTypes.SequenceEqual(actualHandlerTypes),
                "Actual: " + actualHandlerTypes.Select(t => t.ToFriendlyName()).ToCommaSeparatedText());
        }

        [TestMethod]
        public void GetAllInstances_RequestingAContravariantInterfaceWhenARegistrationForAClosedServiceTypeIsMade_ResolvesTheAssignableImplementation()
        {
            // NOTE: CustomerMovedAbroadEvent inherits from CustomerMovedEvent
            CustomerMovedEvent e = new CustomerMovedAbroadEvent();

            // NOTE: IEventHandler is contravariant.
            IEventHandler<CustomerMovedEvent> h1 = new CustomerMovedEventHandler();
            IEventHandler<CustomerMovedAbroadEvent> h2 = h1;

            // Arrange
            var container = ContainerFactory.New();

            var expectedHandlerTypes = new[]
            {
                typeof(CustomerMovedEventHandler), // IEventHandler<CustomerMovedEvent>
            };

            // IEventHandler<in TEvent> is contravariant.
            container.Collection.Register<IEventHandler<CustomerMovedEvent>>(expectedHandlerTypes);

            // Act
            var handlers = container.GetAllInstances<IEventHandler<CustomerMovedAbroadEvent>>();

            // Assert
            Assert.IsTrue(handlers.Any(), "Since IEventHandler<CustomerMovedEvent> is assignable from " +
                "IEventHandler<CustomerMovedAbroadEvent> (because of the in-keyword) the " +
                "CustomerMovedEventHandler should have been resolved here.");
        }

        [TestMethod]
        public void RegisterCollectionEnumerableRegistration_NullArgument_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Registration> registrations = null;

            // Act
            Action action = () => container.Collection.Register<IPlugin>(registrations);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("registrations", action);
        }

        [TestMethod]
        public void RegisterCollection_MultipleRegistrationsForDifferentClosedVersions_InfluenceOtherRegistrations()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type[] expectedHandlerTypes = new[]
            {
                typeof(CustomerMovedAbroadEventHandler),
                typeof(CustomerMovedEventHandler)
            };

            // IEventHandler<in TEvent> is contravariant.
            container.Collection.Register(typeof(IEventHandler<CustomerMovedAbroadEvent>), new[]
            {
                typeof(CustomerMovedAbroadEventHandler)
            });

            container.Collection.Register(typeof(IEventHandler<CustomerMovedEvent>), new Type[]
            {
                typeof(CustomerMovedEventHandler)
            });

            // Act
            var handlers = container.GetAllInstances<IEventHandler<CustomerMovedAbroadEvent>>();
            var actualHandlerTypes = handlers.Select(handler => handler.GetType()).ToArray();

            // Assert
            Assert.IsTrue(expectedHandlerTypes.SequenceEqual(actualHandlerTypes),
                @"The registrations for IEventHandler<CustomerMovedEvent> that are assignable to
                IEventHandler<CustomerMovedAbroadEvent> are expected to 'flow' to the
                IEventHandler<CustomerMovedAbroadEvent> collection, because the expected way for users to
                register generic types by supplying the RegisterCollection(Type, Type[]) overload as follows:
                container.RegisterManyForOpenGeneric(type, container.Collection.Register, assemblies)."
                .TrimInside() +
                "Actual: " + actualHandlerTypes.ToFriendlyNamesText());
        }

        // This is a regression test for bug: 21000.
        [TestMethod]
        public void GetInstance_OnAnUnregisteredConcreteInstanceRegisteredAsPartOfACollection_ShouldSucceed()
        {
            // Arrange
            var container = new Container();

            container.Collection.Register<ITimeProvider>(new[] { typeof(RealTimeProvider) });

            container.Verify();

            // Act
            // This fails in v2.6 and v2.7 when the call is preceded with a call to Verify().
            var instance = container.GetInstance<RealTimeProvider>();
        }

        [TestMethod]
        public void RegisterCollection_CalledAfterRegisterForSameCollection_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            container.Register<IEnumerable<IEventHandler<AuditableEvent>>>(() => null);

            // Act
            Action action = () => container.Collection.Register<IEventHandler<AuditableEvent>>(new[]
            {
                typeof(AuditableEventEventHandler)
            });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "Mixing calls is not supported.",
                action);
        }

        [TestMethod]
        public void RegisterCollectionClosedGenericControlled_CalledAfterRegisterCollectionUncontrolledForSameType_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            var uncontrolledCollection = Enumerable.Empty<IEventHandler<AuditableEvent>>();

            container.Collection.Register<IEventHandler<AuditableEvent>>(uncontrolledCollection);

            // Act
            Action action = () => container.Collection.Register<IEventHandler<AuditableEvent>>(new[]
                {
                    typeof(AuditableEventEventHandler)
                });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(@"
                You already made a registration for IEventHandler<TEvent> using one of the
                Container.Collection.Register overloads that registers container-uncontrolled collections, while this
                method registers container-controlled collections. Mixing calls is not supported."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterCollectionClosedGenericUncontrolled_CalledAfterRegisterCollectionControlledForSameType_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            var uncontrolledCollection = Enumerable.Empty<IEventHandler<AuditableEvent>>();

            container.Collection.Register<IEventHandler<AuditableEvent>>(new[] { typeof(AuditableEventEventHandler) });

            // Act
            Action action = () => container.Collection.Register<IEventHandler<AuditableEvent>>(uncontrolledCollection);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(@"
                You already made a registration for IEventHandler<TEvent> using one of the
                Container.Collection.Register overloads that registers container-controlled collections, while this
                method registers container-uncontrolled collections. Mixing calls is not supported."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterCollectionClosedGeneric_CalledAfterRegisterSingleOnSameCollectionType_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            var collection = new IEventHandler<AuditableEvent>[0];

            container.RegisterInstance<IEnumerable<IEventHandler<AuditableEvent>>>(collection);

            // Act
            Action action =
                () => container.Collection.Register<IEventHandler<AuditableEvent>>(new[] { typeof(AuditableEventEventHandler) });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "Mixing calls is not supported.",
                action);
        }

        [TestMethod]
        public void RegisterWithGenericCollection_CalledAfterRegisterCollectionForSameClosedCollection_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            container.Collection.Register<IEventHandler<AuditableEvent>>(new[] { typeof(AuditableEventEventHandler) });

            // Act
            Action action = () => container.Register<IEnumerable<IEventHandler<AuditableEvent>>>(() => null);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "Mixing calls is not supported.",
                action);
        }

        [TestMethod]
        public void RegisterWithGenericCollection_CalledAfterRegisterCollectionSingletonForSameCollection_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            container.Collection.Register<IEventHandler<AuditableEvent>>(new AuditableEventEventHandler());

            // Act
            Action action = () => container.Register<IEnumerable<IEventHandler<AuditableEvent>>>(() => null);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "Mixing calls is not supported.",
                action);
        }

        [TestMethod]
        public void RegisterCollectionSingleton_CalledAfterRegisterForSameCollection_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            container.Register<IEnumerable<IEventHandler<AuditableEvent>>>(() => null);

            // Act
            Action action = () => container.Collection.Register<IEventHandler<AuditableEvent>>(new AuditableEventEventHandler());

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "Mixing calls is not supported.",
                action);
        }

        [TestMethod]
        public void RegisterCollectionClosed_CalledAfterRegisterCollectionSingletonForSameCollection_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            container.Collection.Register<IEventHandler<AuditableEvent>>(new AuditableEventEventHandler());

            // Act
            Action action = () => container.Collection.Register<IEventHandler<AuditableEvent>>(new[]
            {
                typeof(AuditableEventEventHandler)
            });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "Collection of items for type IEventHandler<AuditableEvent> has already been registered",
                action);

            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "In case it is your goal to append items to an already registered collection, please use " +
                "the Container.Collection.Append method overloads.",
                action);
        }

        [TestMethod]
        public void GetAllInstances_CollectionRegisteredForOpenGenericTypeMadeUsingRegistrationInstances_ReturnsTheExpectedTypes()
        {
            // Arrange
            var expectedTypes = new Type[]
            {
                typeof(AuditableEventEventHandler),
                typeof(EventHandlerImplementationTwoInterface)
            };

            var registeredTypes = new Type[]
            {
                typeof(AuditableEventEventHandler), // IEventHandler<AuditableEvent>
                typeof(EventHandlerImplementationTwoInterface), // IEventHandler<AuditableEvent>, IEventHandler<ClassEvent>
                typeof(StructEventHandler), // IEventHandler<StructEvent>
            };

            var container = ContainerFactory.New();

            var registrations =
                from type in registeredTypes
                select Lifestyle.Transient.CreateRegistration(type, container);

            container.Collection.Register(typeof(IEventHandler<>), registrations);

            // Act
            var auditableHandlers = container.GetAllInstances<IEventHandler<AuditableEvent>>().ToArray();

            // Assert
            AssertThat.SequenceEquals(expectedTypes, auditableHandlers.Select(h => h.GetType()));
        }

        [TestMethod]
        public void RegisterCollectionClosed_CalledAfterRegisterCollectionSingletonWithAllowOverridingRegistrations_CompletelyReplacesPrevious()
        {
            // Arrange
            Type[] expectedHandlerTypes = new[] { typeof(AuditableEventEventHandler) };

            var container = new Container();

            container.Options.AllowOverridingRegistrations = true;

            container.Collection.Register<IEventHandler<AuditableEvent>>(new AuditableEventEventHandler<AuditableEvent>());

            // Act
            // Should Completely override the previous registration
            container.Collection.Register<IEventHandler<AuditableEvent>>(expectedHandlerTypes);

            // Assert
            var handlers = container.GetAllInstances<IEventHandler<AuditableEvent>>();

            Type[] actualHandlerTypes = handlers.Select(handler => handler.GetType()).ToArray();

            Assert.IsTrue(expectedHandlerTypes.SequenceEqual(actualHandlerTypes),
                "The original registration was expected to be replaced completely. Actual: " +
                actualHandlerTypes.ToFriendlyNamesText());
        }

        // Regression: #627 In case Registration classes where added where the ImplementationType was the abstraction,
        // the check whether service is assignable from the implementation type would fail, due to a bug in the Types class.
        [TestMethod]
        public void CollectionRegister_SupplyingRegistrationsForVariantAbstractionWithOnlyAbstractionKnown_SuccessfullyRegistersAndResolvesThem()
        {
            // Arrange
            var container = new Container();

            IEventHandler<BaseClass> impl = new GenericEventHandler<BaseClass>();

            // Act
            // This call failed.
            container.Collection.Register<IEventHandler<DerivedA>>(new[]
            {
                Lifestyle.Transient.CreateRegistration<IEventHandler<BaseClass>>(() => impl, container),
                Lifestyle.Transient.CreateRegistration<IEventHandler<DerivedA>>(() => impl, container)
            });

            var deriveds = container.GetAllInstances<IEventHandler<DerivedA>>();
            var bases = container.GetAllInstances<IEventHandler<BaseClass>>();

            // Assert
            Assert.AreEqual(2, deriveds.Count(), "Two registrations were made for IContra<Derived>.");
            Assert.AreEqual(0, bases.Count(), "No registrations were made for IContra<Base>.");
        }

        // Test for #638.
        [TestMethod]
        public void GetAllInstances_TwoOpenGenericCovariantsWithTypeConstraintsRegistered_ResolvesExpectedInstances()
        {
            // Arrange
            var container = new Container();

            container.Collection.Register(
                serviceType: typeof(ICovariant<>),
                serviceTypes: new[] { typeof(BaseClassCovariant<>), typeof(DerivedACovariant<>) });

            // Act
            var deriveds = container.GetAllInstances<ICovariant<DerivedA>>();
            var bases = container.GetAllInstances<ICovariant<BaseClass>>();

            // Assert
            AssertThat.SequenceEquals(
                expectedTypes: new[] { typeof(BaseClassCovariant<DerivedA>), typeof(DerivedACovariant<DerivedA>) },
                actualTypes: deriveds.Select(d => d.GetType()).ToArray());

            AssertThat.SequenceEquals(
                expectedTypes: new[] { typeof(BaseClassCovariant<BaseClass>) },
                actualTypes: bases.Select(d => d.GetType()).ToArray());
        }

        // Test for #441
        [TestMethod]
        public void GetAllInstances_OnUnregisteredCollection_ThrowsExceptionThatDescribesHowCollectionsMustBeRegistered()
        {
            // Arrange
            var container = new Container();

            // to ensure the container isn't empty
            container.RegisterInstance(new object());

            // Act
            // resolve an unregistered collection
            Action action = () => container.GetAllInstances<IPlugin>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                No registration for type IEnumerable<IPlugin> could be found. You can use one of the
                Container.Collection.Register overloads to register a collection of IPlugin types, or one of
                the Container.Collection.Append overloads to append a single registration to a collection.
                In case you intend to resolve an empty collection of IPlugin elements, make sure you register
                an empty collection; Simple Injector requires a call to Container.Collection.Register to be
                made, even in the absence of any instances.
                Please see https://simpleinjector.org/collections for more information about registering and
                resolving collections."
                .TrimInside(),
                action);
        }

        // Test for #441
        [TestMethod]
        public void GetInstance_OnTypeDependingOnUnregisteredCollection_ThrowsExceptionThatDescribesHowCollectionsMustBeRegistered()
        {
            // Arrange
            var container = new Container();

            container.Register<ServiceDependingOn<ICollection<IPlugin>>>();

            // Act
            Action action = () => container.GetInstance<ServiceDependingOn<ICollection<IPlugin>>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                In case you intend to resolve an empty collection of IPlugin elements, make sure you register
                an empty collection"
                .TrimInside(),
                action);
        }

        private static void Assert_IsNotAMutableCollection<T>(IEnumerable<T> collection)
        {
            string assertMessage = "The container should wrap mutable types to make it impossible for " +
                "users to change the collection.";

            if (collection is ReadOnlyCollection<T> ||
                collection.GetType().Name.Contains("ContainerControlled"))
            {
                return;
            }

            AssertThat.IsNotInstanceOfType(typeof(T[]), collection, assertMessage);
            AssertThat.IsNotInstanceOfType(typeof(IList), collection, assertMessage);
            AssertThat.IsNotInstanceOfType(typeof(ICollection<T>), collection, assertMessage);
        }

        private static void Assert_RegisterCollectionWithAllOpenGenericResultsInExpectedListOfTypes<TService>(
            Type[] openGenericTypesToRegister, Type[] expectedTypes)
            where TService : class
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register(
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

        // Events
        public class CustomerMovedEvent
        {
        }

        public class CustomerMovedAbroadEvent : CustomerMovedEvent
        {
        }

        public class SpecialCustomerMovedEvent : CustomerMovedEvent
        {
        }

        // Handler implementations
        public class CustomerMovedEventHandler : IEventHandler<CustomerMovedEvent>
        {
        }

        public class CustomerMovedAbroadEventHandler : IEventHandler<CustomerMovedAbroadEvent>
        {
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

        public class BaseClassCovariant<T> : ICovariant<T> where T : BaseClass
        {
        }

        public class DerivedACovariant<T> : ICovariant<T> where T : DerivedA
        {
        }

        private sealed class LeftEnumerable<T> : IEnumerable<T>
        {
            public IEnumerator<T> GetEnumerator()
            {
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }

        private sealed class RightEnumerable<T> : IEnumerable<T>
        {
            public IEnumerator<T> GetEnumerator()
            {
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }

        private sealed class PluginContainer
        {
            public PluginContainer(IEnumerable<IPlugin> plugins)
            {
                this.Plugins = plugins;
            }

            public IEnumerable<IPlugin> Plugins { get; }
        }

        private class ClassDependingOn<TDependency>
        {
            public ClassDependingOn(TDependency dependency)
            {
                this.Dependency = dependency;
            }

            public TDependency Dependency { get; }
        }

        private sealed class CompositeCommand
        {
            public CompositeCommand(ICommand[] commands)
            {
                this.Commands = commands;
            }

            public ICommand[] Commands { get; }
        }
    }
}