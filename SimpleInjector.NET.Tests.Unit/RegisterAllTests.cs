namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Extensions;
    using SimpleInjector.Tests.Unit.Extensions;

    /// <summary>Tests for RegisterAll.</summary>
    [TestClass]
    public partial class RegisterAllTests
    {
        private interface IGenericDictionary<T> : IDictionary
        {
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterAllTService_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IPlugin> plugins = null;

            // Act
            container.RegisterAll<IPlugin>(plugins);
        }

        [TestMethod]
        public void GetInstance_TypeWithEnumerableAsConstructorArguments_InjectsExpectedTypes()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(new PluginImpl(), new PluginImpl(), new PluginImpl());

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

            // RegisterSingle<IEnumerable<T>> should have the same effect as RegisterAll<T>
            container.RegisterSingle<IEnumerable<IPlugin>>(plugins);

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
            var manager = container.GetInstance<PluginManager>();

            // Assert
            Assert.AreEqual(0, manager.Plugins.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterSingle_WithEnumerableCalledAfterRegisterAllWithSameType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(new PluginImpl());

            // Act
            container.RegisterSingle<IEnumerable<IPlugin>>(new IPlugin[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Register_WithEnumerableCalledAfterRegisterAllWithSameType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(new PluginImpl());

            // Act
            container.Register<IEnumerable<IPlugin>>(() => new IPlugin[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterAll_WithEnumerableCalledAfterRegisterSingleWithSameType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<IEnumerable<IPlugin>>(new IPlugin[0]);

            // Act
            container.RegisterAll<IPlugin>(new PluginImpl());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterAll_WithEnumerableCalledAfterRegisterWithSameType_Fails()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IEnumerable<IPlugin>>(() => new IPlugin[0]);

            // Act
            container.RegisterAll<IPlugin>(new PluginImpl());
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

            container.RegisterAll<IUserRepository>(repositoryToRegister);

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

            container.RegisterAll<IUserRepository>(new InMemoryUserRepository(), new SqlUserRepository());

            // Act
            var repositories = container.GetAllInstances<IUserRepository>();

            // Assert
            Assert.IsNotNull(repositories, "This method MUST NOT return null.");
            Assert.AreEqual(2, repositories.Count(), "Collection is expected to contain two values.");
        }

        [TestMethod]
        public void GetAllInstances_NoInstancesRegistered_ReturnsEmptyCollection()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var repositories = container.GetAllInstances<IUserRepository>();

            // Assert
            Assert.IsNotNull(repositories, "This method MUST NOT return null.");
            Assert.AreEqual(0, repositories.Count(),
                "If no instances of the requested type are available, this method MUST return an " +
                "enumerator of length 0 instead of throwing an exception.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterAll_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.RegisterSingle<IUserRepository>(new InMemoryUserRepository());
            container.GetInstance<IUserRepository>();

            // Act
            container.RegisterAll<IUserRepository>(new IUserRepository[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetAllInstances.")]
        public void RegisterAll_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            var repositories = container.GetAllInstances<IUserRepository>();
            var count = repositories.Count();

            // Act
            container.RegisterAll<IUserRepository>(new IUserRepository[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterAllParamsT_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IUserRepository[] repositories = null;

            // Act
            container.RegisterAll<IUserRepository>(repositories);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterAllParamsType_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type[] repositoryTypes = null;

            // Act
            container.RegisterAll<IUserRepository>(repositoryTypes);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterAllIEnumerableT_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IUserRepository> repositories = null;

            // Act
            container.RegisterAll<IUserRepository>(repositories);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterAllIEnumerableType_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Type> repositoryTypes = null;

            // Act
            container.RegisterAll<IUserRepository>(repositoryTypes);
        }

        [TestMethod]
        public void RegisterAll_WithListOfTypes_ThrowsExpressiveExceptionExplainingAboutAmbiguity()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                // Act
                container.RegisterAll(new[] { typeof(IUserRepository) });

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
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterAll_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            var repositories = new IUserRepository[] { new InMemoryUserRepository(), new SqlUserRepository() };
            container.RegisterAll<IUserRepository>(repositories);

            // Act
            container.RegisterAll<IUserRepository>(repositories);
        }

        [TestMethod]
        public void GetAllInstancesNonGeneric_WithoutAnRegistration_ReturnsAnEmptyCollection()
        {
            // Arrange
            var container = ContainerFactory.New();

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

            container.RegisterAll<IUserRepository>(new InMemoryUserRepository(), new SqlUserRepository());

            // Act
            var repositories = container.GetAllInstances(typeof(IUserRepository)).ToArray();

            // Assert
            Assert.AreEqual(2, repositories.Length);
            Assert.IsInstanceOfType(repositories[0], typeof(InMemoryUserRepository));
            Assert.IsInstanceOfType(repositories[1], typeof(SqlUserRepository));
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

            container.RegisterAll<IUserRepository>(repositories);

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

            container.RegisterAll<IUserRepository>(repositories);

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

            container.RegisterAll<IUserRepository>(new List<IUserRepository> { new SqlUserRepository(), new InMemoryUserRepository() });

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

            container.RegisterAll<IUserRepository>(new Collection<IUserRepository> { new SqlUserRepository(), new InMemoryUserRepository() });

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

            container.RegisterAll<IUserRepository>(new IUserRepository[] { new SqlUserRepository(), new InMemoryUserRepository() });

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
        public void GetInstance_OnATypeThatDependsOnACollectionThatIsNotRegistered_SameInstanceInjectedEachTime()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            // PluginContainer depends on IEnumerable<IPlugin>
            var firstContainer = container.GetInstance<PluginContainer>();
            var secondContainer = container.GetInstance<PluginContainer>();

            // Assert
            Assert.AreEqual(firstContainer.Plugins, secondContainer.Plugins, "When a collection is not " +
                "registered, the container should register a single empty instance that can be returned " +
                "every time. This saves performance.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterAllTService_RegisteredCollectionWithNullElements_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterAll<IUserRepository>(new IUserRepository[] { null });
        }

        [TestMethod]
        public void RegisterAll_WithOpenGenericType_FailsWithExpectedExceptionMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                // Act
                container.RegisterAll<IDictionary>(new[] { typeof(IGenericDictionary<>) });

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains("open generic type"), "Actual: " + ex.Message);
            }
        }

        [TestMethod]
        public void RegisterAll_WithValidCollectionOfServices_Succeeds()
        {
            // Arrange
            var instance = new SqlUserRepository();

            var container = ContainerFactory.New();

            // Act
            container.RegisterAll(typeof(IUserRepository), new IUserRepository[] { instance });

            // Assert
            var instances = container.GetAllInstances<IUserRepository>();

            Assert.AreEqual(1, instances.Count());
            Assert.AreEqual(instance, instances.First());
        }

        [TestMethod]
        public void RegisterAll_WithValidObjectCollectionOfServices_Succeeds()
        {
            // Arrange
            var instance = new SqlUserRepository();

            var container = ContainerFactory.New();

            // Act
            container.RegisterAll(typeof(IUserRepository), new object[] { instance });

            // Assert
            var instances = container.GetAllInstances<IUserRepository>();

            Assert.AreEqual(1, instances.Count());
            Assert.AreEqual(instance, instances.First());
        }

        [TestMethod]
        public void RegisterAll_WithValidCollectionOfImplementations_Succeeds()
        {
            // Arrange
            var instance = new SqlUserRepository();

            var container = ContainerFactory.New();

            // Act
            container.RegisterAll(typeof(IUserRepository), new SqlUserRepository[] { instance });

            // Assert
            var instances = container.GetAllInstances<IUserRepository>();

            Assert.AreEqual(1, instances.Count());
            Assert.AreEqual(instance, instances.First());
        }

        [TestMethod]
        public void RegisterAll_WithValidListOfTypes_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            // IServiceEx is a valid registration, because it could be registered.
            container.RegisterAll<IUserRepository>(new[] { typeof(SqlUserRepository), typeof(IUserRepository) });
        }

        [TestMethod]
        public void RegisterAll_WithValidEnumeableOfTypes_Succeeds()
        {
            // Arrange
            IEnumerable<Type> types = new[] { typeof(SqlUserRepository) };

            var container = ContainerFactory.New();

            // Act
            container.RegisterAll<IUserRepository>(types);
        }

        [TestMethod]
        public void RegisterAll_WithValidParamListOfTypes_Succeeds()
        {
            // Arrange
            Type[] types = new[] { typeof(SqlUserRepository) };

            var container = ContainerFactory.New();

            // Act
            container.RegisterAll<IUserRepository>(types);
        }

        [TestMethod]
        public void GetAllInstances_RegisteringValidListOfTypesWithRegisterAll_ReturnsExpectedList()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IUserRepository>(new[] { typeof(SqlUserRepository) });

            // Act
            container.Verify();

            var list = container.GetAllInstances<IUserRepository>().ToArray();

            // Assert
            Assert.AreEqual(1, list.Length);
            Assert.IsInstanceOfType(list[0], typeof(SqlUserRepository));
        }

        [TestMethod]
        public void RegisterAll_WithInvalidListOfTypes_ThrowsExceptionWithExpectedMessage()
        {
            // Arrange
            string expectedMessage = "The supplied type IDisposable does not implement IUserRepository.";

            var container = ContainerFactory.New();

            try
            {
                // Act
                container.RegisterAll<IUserRepository>(new[]
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
        public void RegisterAll_RegisteringATypeThatEqualsTheRegisteredServiceType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            // Act
            // Registers a type that references the registration above.
            container.RegisterAll<IUserRepository>(typeof(SqlUserRepository), typeof(IUserRepository));
        }

        [TestMethod]
        public void RegisterAll_RegisteringAnInterfaceOnACollectionOfObjects_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.RegisterAll<object>(typeof(IDisposable));
        }

        [TestMethod]
        public void GetAllInstances_RegisterAllWithRegistration_ResolvesTheExpectedInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<Registration> registrations = new[] 
            { 
                Lifestyle.Transient.CreateRegistration<IUserRepository, SqlUserRepository>(container)
            };

            container.RegisterAll(typeof(IUserRepository), registrations);

            // Act
            var repository = container.GetAllInstances<IUserRepository>().Single();

            // Assert
            Assert.IsInstanceOfType(repository, typeof(SqlUserRepository));
        }

        [TestMethod]
        public void GetAllInstances_RegistrationUsedInMultipleCollections_ResolvesTheExpectedInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration =
                Lifestyle.Singleton.CreateRegistration<IUserRepository, SqlUserRepository>(container);

            container.RegisterAll(typeof(IUserRepository), new[] { registration });
            container.RegisterAll(typeof(object), new[] { registration });

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

            container.RegisterAll(typeof(IPlugin), new[] 
            { 
                Lifestyle.Transient.CreateRegistration<PluginImpl, PluginImpl>(container),
                Lifestyle.Transient.CreateRegistration<IPlugin, PluginImpl2>(container)
            });

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            // Act
            var plugins = container.GetAllInstances<IPlugin>().ToArray();

            // Assert
            Assert.IsInstanceOfType(plugins[0], typeof(PluginDecorator));
            Assert.IsInstanceOfType(plugins[1], typeof(PluginDecorator));
        }

        [TestMethod]
        public void RegisterAllGeneric_RegisteringCovarientTypes_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<ICovariant<object>>(typeof(CovariantImplementation<string>));

            // Act
            var instances = container.GetAllInstances<ICovariant<object>>();

            // Assert
            Assert.IsInstanceOfType(instances.Single(), typeof(CovariantImplementation<string>));
        }

        [TestMethod]
        public void RegisterAllNonGeneric_RegisteringCovarientTypes_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll(typeof(ICovariant<object>), new[] { typeof(CovariantImplementation<string>) });

            // Act
            var instances = container.GetAllInstances<ICovariant<object>>();

            // Assert
            Assert.IsInstanceOfType(instances.Single(), typeof(CovariantImplementation<string>));
        }

        [TestMethod]
        public void RegisterAll_MixOfOpenClosedAndNonGenericTypes_ResolvesExpectedTypesInExpectedOrder1()
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

            container.RegisterAll(typeof(IEventHandler<>), registeredTypes);

            // Act
            Type[] actualHandlerTypes = container.GetAllInstances(resolvedHandlerType)
                .Select(h => h.GetType()).ToArray();

            // Assert
            Assert.AreEqual(
                expected: string.Join(", ", expectedHandlerTypes.Select(TestHelpers.ToFriendlyName)),
                actual: string.Join(", ", actualHandlerTypes.Select(TestHelpers.ToFriendlyName)));
        }

        [TestMethod]
        public void RegisterAll_MixOfOpenClosedAndNonGenericTypes_ResolvesExpectedTypesInExpectedOrder2()
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

            container.RegisterAll(typeof(IEventHandler<>), registeredTypes);

            // Act
            Type[] actualHandlerTypes = container.GetAllInstances(resolvedHandlerType)
                .Select(h => h.GetType()).ToArray();

            // Assert
            Assert.AreEqual(
                expected: string.Join(", ", expectedHandlerTypes.Select(TestHelpers.ToFriendlyName)),
                actual: string.Join(", ", actualHandlerTypes.Select(TestHelpers.ToFriendlyName)));
        }

        [TestMethod]
        public void RegisterAll_MixOfOpenClosedAndNonGenericTypes_ResolvesExpectedTypesInExpectedOrder3()
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

            container.RegisterAll(typeof(IEventHandler<>), registeredTypes);

            // Act
            Type[] actualHandlerTypes = container.GetAllInstances(resolvedHandlerType)
                .Select(h => h.GetType()).ToArray();

            // Assert
            Assert.AreEqual(
                expected: string.Join(", ", expectedHandlerTypes.Select(TestHelpers.ToFriendlyName)),
                actual: string.Join(", ", actualHandlerTypes.Select(TestHelpers.ToFriendlyName)));
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

            container.RegisterAll(typeof(IEventHandler<>), registeredTypes);

            // Act
            Type[] actualHandlerTypes = container.GetAllInstances(resolvedHandlerType)
                .Select(h => h.GetType()).ToArray();

            // Assert
            Assert.AreEqual(
                expected: string.Join(", ", expectedHandlerTypes.Select(TestHelpers.ToFriendlyName)),
                actual: string.Join(", ", actualHandlerTypes.Select(TestHelpers.ToFriendlyName)));
        }
        
        [TestMethod]
        public void RegisterAll_SuppliedWithATypeThatContainsUnresolvableTypeArguments_ThrowsDescriptiveException()
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
            Action action = () => container.RegisterAll(typeof(IEventHandler<>), registeredTypes);

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
            // container.RegisterOpeNGeneric(typeof(EventHandlerWithConstructorContainingPrimitive<>), typeof(X))
            // where X is a type with one constructor.
            container.RegisterAll(typeof(IEventHandler<>), registeredTypes);

            // Act
            Action action = () => container.GetAllInstances(typeof(IEventHandler<StructEvent>)).ToArray();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "The constructor of type EventHandlerWithConstructorContainingPrimitive<StructEvent>",
                action);
        }

        [TestMethod]
        public void RegisterAll_WithNonGenericType_DelegatesBackIntoTheContainer()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Register<StructEventHandler>(Lifestyle.Singleton);
            container.RegisterAll(typeof(IEventHandler<>), new[] { typeof(StructEventHandler) });

            // Assert
            var handlers = container.GetAllInstances<IEventHandler<StructEvent>>();

            Assert.AreSame(handlers.Single(), handlers.Single(), "The container didn't delegate back.");
        }

        [TestMethod]
        public void RegisterAll_WithClosedGenericType_DelegatesBackIntoTheContainer()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Register<ClassConstraintEventHandler<AuditableEvent>>(Lifestyle.Singleton);
            container.RegisterAll(typeof(IEventHandler<>), new[]
            {
                typeof(ClassConstraintEventHandler<AuditableEvent>)
            });

            // Assert
            var handlers = container.GetAllInstances<IEventHandler<AuditableEvent>>();

            Assert.AreSame(handlers.Single(), handlers.Single(), "The container didn't delegate back.");
        }

        [TestMethod]
        public void RegisterAll_WithOpenGenericType_DelegatesBackIntoTheContainer()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterOpenGeneric(typeof(NewConstraintEventHandler<>),
                typeof(NewConstraintEventHandler<>), Lifestyle.Singleton);

            container.RegisterAll(typeof(IEventHandler<>), new[]
            {
                typeof(NewConstraintEventHandler<>)
            });

            // Assert
            var handlers = container.GetAllInstances<IEventHandler<DefaultConstructorEvent>>();

            Assert.AreSame(handlers.Single(), handlers.Single(), "The container didn't delegate back.");
        }

        [TestMethod]
        public void RegisterAll_WithClosedGenericType_DelegatesBackIntoTheContainerToOpenGenericRegistration()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterOpenGeneric(
                typeof(NewConstraintEventHandler<>),
                typeof(NewConstraintEventHandler<>), 
                Lifestyle.Singleton);

            container.RegisterAll(typeof(IEventHandler<>), new[]
            {
                typeof(NewConstraintEventHandler<DefaultConstructorEvent>)
            });

            // Assert
            var handlers = container.GetAllInstances<IEventHandler<DefaultConstructorEvent>>();

            Assert.AreSame(handlers.Single(), handlers.Single(), "The container didn't delegate back.");
        }

        [TestMethod]
        public void RegisterAll_WithOpenGenericType_DelegatesBackIntoTheContainerToCloseRegistration()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Register<ClassConstraintEventHandler<AuditableEvent>>(Lifestyle.Singleton);

            container.RegisterAll(typeof(IEventHandler<>), new[]
            {
                typeof(ClassConstraintEventHandler<>)
            });

            // Assert
            var handlers = container.GetAllInstances<IEventHandler<AuditableEvent>>();

            Assert.AreSame(handlers.Single(), handlers.Single(), "The container didn't delegate back.");
        }

        [TestMethod]
        public void RegisterAll_WithAbstractType_DelegatesBackIntoTheContainerToCloseRegistration()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Register<IEventHandler<AuditableEvent>, ClassConstraintEventHandler<AuditableEvent>>();

            container.RegisterAll(typeof(IEventHandler<>), new[]
            {
                typeof(IEventHandler<>)
            });

            // Assert
            var handler = container.GetAllInstances<IEventHandler<AuditableEvent>>().Single();

            Assert.IsInstanceOfType(handler, typeof(ClassConstraintEventHandler<AuditableEvent>));
        }

        [TestMethod]
        public void Verify_ClosedTypeWithUnregisteredDependency_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll(typeof(IEventHandler<>), new[]
            {
                // This closed generic type has an ILogger constructor dependency, but ILogger is not 
                // registered, and Verify() should catch this.
                typeof(EventHandlerWithDependency<AuditableEvent, ILogger>)
            });

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "Please ensure ILogger is registered in the container",
                action);
        }

        [TestMethod]
        public void Verify_OpenTypeWithUnregisteredDependencyThatIsPartOfCollectionWithNonGenericType_ThrowsExpectedException()
        {
            // Arrange
            Type unregisteredDependencyType = typeof(ILogger);

            var container = ContainerFactory.New();

            container.RegisterAll(typeof(IEventHandler<>), new[]
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
                "Please ensure ILogger is registered in the container",
                action);
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnICollection_InjectsTheRegisteredCollection()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(PluginImpl), typeof(PluginImpl2));

            // Act
            ICollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<ICollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, collection.Count);
            Assert.IsInstanceOfType(collection.First(), typeof(PluginImpl));
            Assert.IsInstanceOfType(collection.Second(), typeof(PluginImpl2));
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnICollection_InjectsTheRegisteredCollectionOfDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(PluginImpl), typeof(PluginImpl2));

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            // Act
            ICollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<ICollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, collection.Count);
            Assert.IsInstanceOfType(collection.First(), typeof(PluginDecorator));
            Assert.IsInstanceOfType(collection.Second(), typeof(PluginDecorator));
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnICollection_InjectsEmptyCollectionWhenNoInstancesRegistered()
        {
            // Arrange
            var container = ContainerFactory.New();

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

            container.RegisterAll<IPlugin>(typeof(PluginImpl), typeof(PluginImpl2));

            // Act
            IList<IPlugin> list = container.GetInstance<ClassDependingOn<IList<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, list.Count);
            Assert.IsInstanceOfType(list[0], typeof(PluginImpl));
            Assert.IsInstanceOfType(list[1], typeof(PluginImpl2));
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnIList_InjectsTheRegisteredListOfDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(PluginImpl), typeof(PluginImpl2));

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            // Act
            IList<IPlugin> list =
                container.GetInstance<ClassDependingOn<IList<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, list.Count);
            Assert.IsInstanceOfType(list[0], typeof(PluginDecorator));
            Assert.IsInstanceOfType(list[0], typeof(PluginDecorator));
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnIList_InjectsEmptyListWhenNoInstancesRegistered()
        {
            // Arrange
            var container = ContainerFactory.New();

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

            container.RegisterAll<ICommand>(singletonCommand);

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

            container.RegisterAll<ICommand>(expectedCommand);

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

            container.RegisterAll<ICommand>(typeof(ConcreteCommand));

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

            container.RegisterAll<ICommand>(new ConcreteCommand());

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

            container.RegisterAll<ICommand>(commands);

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

            container.RegisterAll<ICommand>(typeof(ConcreteCommand));

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

            container.RegisterAll<ICommand>(commands);

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

            container.RegisterSingle<ConcreteCommand>();

            container.RegisterAll<ICommand>(typeof(ConcreteCommand));

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
            container.RegisterAll(typeof(IEventHandler<>), expectedHandlerTypes);

            // Act
            var handlers = container.GetAllInstances<IEventHandler<CustomerMovedAbroadEvent>>();
            Type[] actualHandlerTypes = handlers.Select(handler => handler.GetType()).ToArray();

            // Assert
            Assert.IsTrue(expectedHandlerTypes.SequenceEqual(actualHandlerTypes),
                "Actual: " + actualHandlerTypes.Select(Helpers.ToFriendlyName).ToCommaSeparatedText());
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
            container.RegisterAll<IEventHandler<CustomerMovedEvent>>(expectedHandlerTypes);

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
            Action action = () => container.RegisterAll(typeof(IPlugin), registrations);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("registrations", action);
        }

        [TestMethod]
        public void RegisterAll_MultipleRegistrationsForDifferentClosedVersions_InfluenceOtherRegistrations()
        {
            // Arrange
            var container = ContainerFactory.New();

            Type[] expectedHandlerTypes = new[] 
            {
                typeof(CustomerMovedAbroadEventHandler),
                typeof(CustomerMovedEventHandler)
            };

            // IEventHandler<in TEvent> is contravariant.
            container.RegisterAll(typeof(IEventHandler<CustomerMovedAbroadEvent>), new[] 
            {
                typeof(CustomerMovedAbroadEventHandler) 
            });

            container.RegisterAll(typeof(IEventHandler<CustomerMovedEvent>), new Type[] 
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
                "Actual: " + actualHandlerTypes.Select(Helpers.ToFriendlyName).ToCommaSeparatedText());
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

            Assert.IsNotInstanceOfType(collection, typeof(T[]), assertMessage);
            Assert.IsNotInstanceOfType(collection, typeof(IList), assertMessage);
            Assert.IsNotInstanceOfType(collection, typeof(ICollection<T>), assertMessage);
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

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private sealed class RightEnumerable<T> : IEnumerable<T>
        {
            public IEnumerator<T> GetEnumerator()
            {
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private sealed class PluginContainer
        {
            public PluginContainer(IEnumerable<IPlugin> plugins)
            {
                this.Plugins = plugins;
            }

            public IEnumerable<IPlugin> Plugins { get; private set; }
        }

        private class ClassDependingOn<TDependency>
        {
            public ClassDependingOn(TDependency dependency)
            {
                this.Dependency = dependency;
            }

            public TDependency Dependency { get; private set; }
        }

        private sealed class CompositeCommand
        {
            public CompositeCommand(ICommand[] commands)
            {
                this.Commands = commands;
            }

            public ICommand[] Commands { get; private set; }
        }
    }
}