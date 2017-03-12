#pragma warning disable 0618
namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests for RegisterAll.</summary>
    [TestClass]
    public partial class RegisterAllTests
    {
        private interface IGenericDictionary<T> : IDictionary
        {
        }

        [TestMethod]
        public void RegisterAllTService_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IPlugin> plugins = null;

            // Act
            Action action = () => container.RegisterCollection<IPlugin>(plugins);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetInstance_TypeWithEnumerableAsConstructorArguments_InjectsExpectedTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<IPlugin>(new PluginImpl(), new PluginImpl(), new PluginImpl());

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

            // RegisterSingleton<IEnumerable<T>> should have the same effect as RegisterAll<T>
            container.RegisterSingleton<IEnumerable<IPlugin>>(plugins);

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

            container.RegisterCollection<IPlugin>();

            // Act
            // PluginManager has a constructor with an IEnumerable<IPlugin> argument.
            // We expect this call to succeed, even while no IPlugin implementations are registered.
            var manager = container.GetInstance<PluginManager>();

            // Assert
            Assert.AreEqual(0, manager.Plugins.Length);
        }

        [TestMethod]
        public void RegisterSingle_WithEnumerableCalledAfterRegisterAllWithSameType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<IPlugin>(new PluginImpl());

            // Act
            Action action = () => container.RegisterSingleton<IEnumerable<IPlugin>>(new IPlugin[0]);

            // Assert
            AssertThat.Throws<NotSupportedException>(action);
        }

        [TestMethod]
        public void Register_WithEnumerableCalledAfterRegisterAllWithSameType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<IPlugin>(new PluginImpl());

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

            container.RegisterSingleton<IEnumerable<IPlugin>>(new IPlugin[0]);

            // Act
            Action action = () => container.RegisterCollection<IPlugin>(new PluginImpl());

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
            Action action = () => container.RegisterCollection<IPlugin>(new PluginImpl());

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

            container.RegisterCollection<IUserRepository>(repositoryToRegister);

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

            container.RegisterCollection<IUserRepository>(new InMemoryUserRepository(), new SqlUserRepository());

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
            container.RegisterSingleton<IUserRepository>(new InMemoryUserRepository());
            container.GetInstance<IUserRepository>();

            // Act
            Action action = () => container.RegisterCollection<IUserRepository>(new IUserRepository[0]);

            // Assert
            AssertThat.Throws<InvalidOperationException>(action,
                "The container should get locked after a call to GetInstance.");
        }

        [TestMethod]
        public void RegisterCollection_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterCollection<IUserRepository>();
            var repositories = container.GetAllInstances<IUserRepository>();
            var count = repositories.Count();

            // Act
            Action action = () => container.RegisterCollection<IUserRepository>(new IUserRepository[0]);

            // Assert
            AssertThat.Throws<InvalidOperationException>(action,
                "The container should get locked after a call to GetAllInstances.");
        }

        [TestMethod]
        public void RegisterAllParamsT_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IUserRepository[] repositories = null;

            // Act
            Action action = () => container.RegisterCollection<IUserRepository>(repositories);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterAllParamsType_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type[] repositoryTypes = null;

            // Act
            Action action = () => container.RegisterCollection<IUserRepository>(repositoryTypes);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterAllIEnumerableT_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IUserRepository> repositories = null;

            // Act
            Action action = () => container.RegisterCollection<IUserRepository>(repositories);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void RegisterAllIEnumerableType_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Type> repositoryTypes = null;

            // Act
            Action action = () => container.RegisterCollection<IUserRepository>(repositoryTypes);

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
                container.RegisterCollection(new[] { typeof(IUserRepository) });

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
                    "RegisterAll<TService> overload where TService is Type, which is unlikely what the " +
                    "use intended. We should throw an exception instead.");
            }
        }

        [TestMethod]
        public void RegisterCollection_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            var repositories = new IUserRepository[] { new InMemoryUserRepository(), new SqlUserRepository() };
            container.RegisterCollection<IUserRepository>(repositories);

            // Act
            Action action = () => container.RegisterCollection<IUserRepository>(repositories);

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void GetAllInstancesNonGeneric_WithoutEmptyRegistration_ReturnsAnEmptyCollection()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<IUserRepository>();

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

            container.RegisterCollection<IUserRepository>(new InMemoryUserRepository(), new SqlUserRepository());

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

            container.RegisterCollection<IUserRepository>(repositories);

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

            container.RegisterCollection<IUserRepository>(repositories);

            repositories[0] = null;

            // Act
            var collection = container.GetAllInstances<IUserRepository>().ToArray();

            // Assert
            Assert.IsNotNull(collection[0], "RegisterAll<T>(T[]) did not make a copy of the supplied array.");
        }

        [TestMethod]
        public void GetAllInstances_WithListRegistered_DoesNotReturnAnArray()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<IUserRepository>(new List<IUserRepository> { new SqlUserRepository(), new InMemoryUserRepository() });

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

            container.RegisterCollection<IUserRepository>(new Collection<IUserRepository> 
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

            container.RegisterCollection<IUserRepository>(new IUserRepository[] { new SqlUserRepository(), new InMemoryUserRepository() });

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
        public void RegisterAllTService_RegisteredCollectionWithNullElements_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.RegisterCollection<IUserRepository>(new IUserRepository[] { null });

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
                container.RegisterCollection<IDictionary>(new[] { typeof(IGenericDictionary<>) });

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains("open generic type"), "Actual: " + ex.Message);
            }
        }

        [TestMethod]
        public void RegisterCollection_WithValidCollectionOfServices_Succeeds()
        {
            // Arrange
            var instance = new SqlUserRepository();

            var container = ContainerFactory.New();

            // Act
            container.RegisterCollection(typeof(IUserRepository), new IUserRepository[] { instance });

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
            container.RegisterCollection(typeof(IUserRepository), new object[] { instance });

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
            container.RegisterCollection(typeof(IUserRepository), new SqlUserRepository[] { instance });

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
            container.RegisterCollection<IUserRepository>(new[] { typeof(SqlUserRepository), typeof(IUserRepository) });
        }

        [TestMethod]
        public void RegisterCollection_WithValidEnumeableOfTypes_Succeeds()
        {
            // Arrange
            IEnumerable<Type> types = new[] { typeof(SqlUserRepository) };

            var container = ContainerFactory.New();

            // Act
            container.RegisterCollection<IUserRepository>(types);
        }

        [TestMethod]
        public void RegisterCollection_WithValidParamListOfTypes_Succeeds()
        {
            // Arrange
            Type[] types = new[] { typeof(SqlUserRepository) };

            var container = ContainerFactory.New();

            // Act
            container.RegisterCollection<IUserRepository>(types);
        }

        [TestMethod]
        public void GetAllInstances_RegisteringValidListOfTypesWithRegisterCollection_ReturnsExpectedList()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<IUserRepository>(new[] { typeof(SqlUserRepository) });

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
                container.RegisterCollection<IUserRepository>(new[]
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
            container.RegisterCollection<IUserRepository>(new[] { typeof(SqlUserRepository), typeof(IUserRepository) });
        }

        [TestMethod]
        public void RegisterCollection_RegisteringAnInterfaceOnACollectionOfObjects_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterCollection<object>(typeof(IDisposable));
        }

        [TestMethod]
        public void GetAllInstances_RegisterAllWithRegistration_ResolvesTheExpectedInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Registration> registrations = new[] 
            { 
                Lifestyle.Transient.CreateRegistration<SqlUserRepository>(container)
            };

            container.RegisterCollection(typeof(IUserRepository), registrations);

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

            container.RegisterCollection(typeof(IUserRepository), new[] { registration });
            container.RegisterCollection(typeof(object), new[] { registration });

            // Act
            var instance1 = container.GetAllInstances<IUserRepository>().Single();
            var instance2 = container.GetAllInstances<object>().Single();

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        [TestMethod]
        public void GetAllInstances_RegisterAllWithRegistrationsAndDecorator_WrapsTheDecorator()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<IPlugin>(new[] 
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
        public void RegisterAllGeneric_RegisteringCovarientTypes_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<ICovariant<object>>(new[] { typeof(CovariantImplementation<string>) });

            // Act
            var instances = container.GetAllInstances<ICovariant<object>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(CovariantImplementation<string>), instances.Single());
        }

        [TestMethod]
        public void RegisterAllNonGeneric_RegisteringCovarientTypes_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ICovariant<object>), new[] { typeof(CovariantImplementation<string>) });

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

            container.RegisterCollection(typeof(IEventHandler<>), registeredTypes);

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

            container.RegisterCollection(typeof(IEventHandler<>), registeredTypes);

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

            container.RegisterCollection(typeof(IEventHandler<>), registeredTypes);

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

            container.RegisterCollection(typeof(IEventHandler<>), registeredTypes);

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
            Action action = () => container.RegisterCollection(typeof(IEventHandler<>), registeredTypes);

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

            // RegisterAll will not throw an exception, because registration is forwarded back into the
            // container, and it could be possible that someone does a registration like:
            // container.Register(typeof(EventHandlerWithConstructorContainingPrimitive<>), typeof(X))
            // where X is a type with one constructor.
            container.RegisterCollection(typeof(IEventHandler<>), registeredTypes);

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
            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(StructEventHandler) });

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
            container.RegisterCollection(typeof(IEventHandler<>), new[]
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

            container.RegisterCollection(typeof(IEventHandler<>), new[]
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

            container.RegisterCollection(typeof(IEventHandler<>), new[]
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

            container.RegisterCollection(typeof(IEventHandler<>), new[]
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

            container.RegisterCollection(typeof(IEventHandler<>), new[]
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

            container.RegisterCollection(typeof(IEventHandler<>), new[]
            {
                // This closed generic type has an ILogger constructor dependency, but ILogger is not 
                // registered, and Verify() should catch this.
                typeof(EventHandlerWithDependency<AuditableEvent, ILogger>)
            });

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "Please ensure ILogger is registered",
                action);
        }

        [TestMethod]
        public void Verify_OpenTypeWithUnregisteredDependencyThatIsPartOfCollectionWithNonGenericType_ThrowsExpectedException()
        {
            // Arrange
            Type unregisteredDependencyType = typeof(ILogger);

            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(IEventHandler<>), new[]
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
                "Please ensure ILogger is registered",
                action);
        }

        [TestMethod]
        public void Verify_ClosedTypeWithUnregisteredDependencyResolvedBeforeCallingVerift_StillThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(IEventHandler<>), new[]
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
                "Please ensure ILogger is registered",
                action);
        }

        [TestMethod]
        public void Verify_NonGenericTypeWithUnregisteredDependencyResolvedBeforeCallingVerify_StillThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            
            container.RegisterCollection(typeof(IEventHandler<>), Type.EmptyTypes);

            container.RegisterCollection(typeof(UserServiceBase), new[]
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
                "Please ensure IUserRepository is registered",
                action);
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnICollection_InjectsTheRegisteredCollection()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            // Act
            ICollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<ICollection<IPlugin>>>().Dependency;

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

            container.RegisterCollection<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

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

            container.RegisterCollection<IPlugin>();

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

            container.RegisterCollection<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            // Act
            IList<IPlugin> list = container.GetInstance<ClassDependingOn<IList<IPlugin>>>().Dependency;

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

            container.RegisterCollection<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

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

            container.RegisterCollection<IPlugin>();

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

            container.RegisterCollection<ICommand>(singletonCommand);

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

            container.RegisterCollection<ICommand>(expectedCommand);

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

            container.RegisterCollection<ICommand>(new[] { typeof(ConcreteCommand) });

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
        public void GetInstance_CalledMultipleTimesOnContainerControlledSingletons_StillInjectsANewArrayOnEachRequest()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<ICommand>(new ConcreteCommand());

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
        public void GetInstance_CalledMultipleTimesOnContainerUncontrolledCollection_StillInjectsANewArrayOnEachRequest()
        {
            // Arrange
            var container = ContainerFactory.New();

            List<ICommand> commands = new List<ICommand>();

            // Add a first command
            commands.Add(new ConcreteCommand());

            container.RegisterCollection<ICommand>(commands);

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

            container.RegisterCollection<ICommand>(new[] { typeof(ConcreteCommand) });

            // Act
            var registration = container.GetRegistration(typeof(ICommand[]));

            // Assert
            Assert.AreEqual(Lifestyle.Transient, registration.Lifestyle);
        }

        [TestMethod]
        public void GetRegistration_RequestingArrayRegistrationUncontainerControlledCollection_HasTheTransientLifestyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<ICommand> commands = new List<ICommand> { new ConcreteCommand() };

            container.RegisterCollection<ICommand>(commands);

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

            container.RegisterCollection<ICommand>(new[] { typeof(ConcreteCommand) });

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
            container.RegisterCollection(typeof(IEventHandler<>), expectedHandlerTypes);

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
            // Arrange
            var container = ContainerFactory.New();

            // NOTE: CustomerMovedAbroadEvent inherits from CustomerMovedEvent.
            var expectedHandlerTypes = new[]
            {
                typeof(CustomerMovedEventHandler), // IEventHandler<CustomerMovedEvent>
            };

            // IEventHandler<in TEvent> is contravariant.
            container.RegisterCollection<IEventHandler<CustomerMovedEvent>>(expectedHandlerTypes);

            // Act
            var handlers = container.GetAllInstances<IEventHandler<CustomerMovedAbroadEvent>>();

            // Assert
            Assert.IsTrue(handlers.Any(), "Since IEventHandler<CustomerMovedEvent> is assignable from " +
                "IEventHandler<CustomerMovedAbroadEvent> (because of the in-keyword) the " +
                "CustomerMovedEventHandler should have been resolved here.");
        }

        [TestMethod]
        public void RegisterAllEnumerableRegistration_NullArgument_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Registration> registrations = null;

            // Act
            Action action = () => container.RegisterCollection<IPlugin>(registrations);

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
            container.RegisterCollection(typeof(IEventHandler<CustomerMovedAbroadEvent>), new[] 
            {
                typeof(CustomerMovedAbroadEventHandler) 
            });

            container.RegisterCollection(typeof(IEventHandler<CustomerMovedEvent>), new Type[] 
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
                register generic types by supplying the RegisterAll(Type, Type[]) overload as follows:
                container.RegisterManyForOpenGeneric(type, container.RegisterAll, assemblies)."
                .TrimInside() +
                "Actual: " + actualHandlerTypes.ToFriendlyNamesText());
        }

        // This is a regression test for bug: 21000.
        [TestMethod]
        public void GetInstance_OnAnUnregisteredConcreteInstanceRegisteredAsPartOfACollection_ShouldSucceed()
        {
            // Arrange
            var container = new Container();

            container.RegisterCollection<ITimeProvider>(new[] { typeof(RealTimeProvider) });

            container.Verify();

            // Act
            // This fails in v2.6 and v2.7 when the call is preceded with a call to Verify().
            var instance = container.GetInstance<RealTimeProvider>();
        }

        [TestMethod]
        public void RegisterAllClosedGeneric_CalledAfterRegisterForSameCollection_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            container.Register<IEnumerable<IEventHandler<AuditableEvent>>>(() => null);

            // Act
            Action action = () => container.RegisterCollection<IEventHandler<AuditableEvent>>(new[] 
            {
                typeof(AuditableEventEventHandler) 
            });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "Mixing calls is not supported.",
                action);
        }

        [TestMethod]
        public void RegisterAllClosedGenericControlled_CalledAfterRegisterAllUncontrolledForSameType_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            var uncontrolledCollection = Enumerable.Empty<IEventHandler<AuditableEvent>>();

            container.RegisterCollection<IEventHandler<AuditableEvent>>(uncontrolledCollection);

            // Act
            Action action = () => container.RegisterCollection<IEventHandler<AuditableEvent>>(new[] 
                {
                    typeof(AuditableEventEventHandler) 
                });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(@"
                You already made a registration for the IEventHandler<TEvent> type using one of the 
                RegisterCollection overloads that registers container-uncontrolled collections, while this 
                method registers container-controlled collections. Mixing calls is not supported."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterAllClosedGenericUncontrolled_CalledAfterRegisterAllControlledForSameType_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            var uncontrolledCollection = Enumerable.Empty<IEventHandler<AuditableEvent>>();
                        
            container.RegisterCollection<IEventHandler<AuditableEvent>>(new[] { typeof(AuditableEventEventHandler) });

            // Act
            Action action = () => container.RegisterCollection<IEventHandler<AuditableEvent>>(uncontrolledCollection);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(@"
                You already made a registration for the IEventHandler<TEvent> type using one of the 
                RegisterCollection overloads that registers container-controlled collections, while this 
                method registers container-uncontrolled collections. Mixing calls is not supported."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void RegisterAllClosedGeneric_CalledAfterRegisterSingleOnSameCollectionType_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            var collection = new IEventHandler<AuditableEvent>[0];

            container.RegisterSingleton<IEnumerable<IEventHandler<AuditableEvent>>>(collection);

            // Act
            Action action =
                () => container.RegisterCollection<IEventHandler<AuditableEvent>>(new[] { typeof(AuditableEventEventHandler) });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "Mixing calls is not supported.",
                action);
        }

        [TestMethod]
        public void RegisterWithGenericCollection_CalledAfterRegisterAllForSameClosedCollection_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            container.RegisterCollection<IEventHandler<AuditableEvent>>(new[] { typeof(AuditableEventEventHandler) });

            // Act
            Action action = () => container.Register<IEnumerable<IEventHandler<AuditableEvent>>>(() => null);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "Mixing calls is not supported.",
                action);
        }

        [TestMethod]
        public void RegisterWithGenericCollection_CalledAfterRegisterAllSingletonForSameCollection_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            container.RegisterCollection<IEventHandler<AuditableEvent>>(new AuditableEventEventHandler());

            // Act
            Action action = () => container.Register<IEnumerable<IEventHandler<AuditableEvent>>>(() => null);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "Mixing calls is not supported.",
                action);
        }

        [TestMethod]
        public void RegisterAllSingleton_CalledAfterRegisterForSameCollection_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            container.Register<IEnumerable<IEventHandler<AuditableEvent>>>(() => null);

            // Act
            Action action = () => container.RegisterCollection<IEventHandler<AuditableEvent>>(new AuditableEventEventHandler());

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "Mixing calls is not supported.",
                action);
        }

        [TestMethod]
        public void RegisterAllClosed_CalledAfterRegisterAllSingletonForSameCollection_ThrowsAlreadyRegisteredException()
        {
            // Arrange
            var container = new Container();

            container.RegisterCollection<IEventHandler<AuditableEvent>>(new AuditableEventEventHandler());

            // Act
            Action action = () => container.RegisterCollection<IEventHandler<AuditableEvent>>(new[]
            {
                typeof(AuditableEventEventHandler)
            });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "Collection of items for type IEventHandler<AuditableEvent> has already been registered",
                action);

            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "In case it is your goal to append items to an already registered collection, please use " +
                "the AppendToCollection extension method. This method is located in the " +
                "SimpleInjector.Advanced namespace.",
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

            container.RegisterCollection(typeof(IEventHandler<>), registrations);

            // Act
            var auditableHandlers = container.GetAllInstances<IEventHandler<AuditableEvent>>().ToArray();

            // Assert
            AssertThat.SequenceEquals(expectedTypes, auditableHandlers.Select(h => h.GetType()));
        }

        [TestMethod]
        public void RegisterAllClosed_CalledAfterRegisterAllSingletonWithAllowOverridingRegistrations_CompletelyReplacesPrevious()
        {
            // Arrange
            Type[] expectedHandlerTypes = new[] { typeof(AuditableEventEventHandler) };

            var container = new Container();

            container.Options.AllowOverridingRegistrations = true;

            container.RegisterCollection<IEventHandler<AuditableEvent>>(new AuditableEventEventHandler<AuditableEvent>());

            // Act
            // Should Completely override the previous registration
            container.RegisterCollection<IEventHandler<AuditableEvent>>(expectedHandlerTypes);

            // Assert
            var handlers = container.GetAllInstances<IEventHandler<AuditableEvent>>();

            Type[] actualHandlerTypes = handlers.Select(handler => handler.GetType()).ToArray();

            Assert.IsTrue(expectedHandlerTypes.SequenceEqual(actualHandlerTypes),
                "The original registration was expected to be replaced completely. Actual: " +
                actualHandlerTypes.ToFriendlyNamesText());
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
#pragma warning restore 0618