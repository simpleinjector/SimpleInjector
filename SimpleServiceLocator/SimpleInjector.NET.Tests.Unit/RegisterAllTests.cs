namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterAllTests
    {
        [TestMethod]
        public void GetInstance_TypeWithEnumerableAsConstructorArguments_InjectsExpectedTypes()
        {
            // Arrange
            var container = new Container();

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
            var container = new Container();

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
            var container = new Container();

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
            var container = new Container();

            container.RegisterAll<IPlugin>(new PluginImpl());

            // Act
            container.RegisterSingle<IEnumerable<IPlugin>>(new IPlugin[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Register_WithEnumerableCalledAfterRegisterAllWithSameType_Fails()
        {
            // Arrange
            var container = new Container();

            container.RegisterAll<IPlugin>(new PluginImpl());

            // Act
            container.Register<IEnumerable<IPlugin>>(() => new IPlugin[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterAll_WithEnumerableCalledAfterRegisterSingleWithSameType_Fails()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<IEnumerable<IPlugin>>(new IPlugin[0]);
            
            // Act
            container.RegisterAll<IPlugin>(new PluginImpl());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterAll_WithEnumerableCalledAfterRegisterWithSameType_Fails()
        {
            // Arrange
            var container = new Container();

            container.Register<IEnumerable<IPlugin>>(() => new IPlugin[0]);

            // Act
            container.RegisterAll<IPlugin>(new PluginImpl());
        }

        [TestMethod]
        public void GetAllInstances_ListRegisteredUsingEnumerable_ReturnsExpectedList()
        {
            // Arrange
            var container = new Container();

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
            var container = new Container();
            
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
            var container = new Container();

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
            var container = new Container();
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
            var container = new Container();
            var repositories = container.GetAllInstances<IUserRepository>();
            var count = repositories.Count();

            // Act
            container.RegisterAll<IUserRepository>(new IUserRepository[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterAll_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterAll<IUserRepository>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterAll_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new Container();
            var repositories = new IUserRepository[] { new InMemoryUserRepository(), new SqlUserRepository() };
            container.RegisterAll<IUserRepository>(repositories);

            // Act
            container.RegisterAll<IUserRepository>(repositories);
        }

        [TestMethod]
        public void GetAllInstancesNonGeneric_WithoutAnRegistration_ReturnsAnEmptyCollection()
        {
            // Arrange
            var container = new Container();

            // Act
            var repositories = container.GetAllInstances(typeof(IUserRepository));

            // Assert
            Assert.AreEqual(0, repositories.Count());
        }

        [TestMethod]
        public void GetAllInstancesNonGeneric_WithValidRegistration_ReturnsCollectionWithExpectedElements()
        {
            // Arrange
            var container = new Container();

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
                "The registered delegate for type System.Collections.Generic.IEnumerable`1[" +
                    typeof(IUserRepository).FullName + "] returned null.";
            
            var container = new Container();

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
            var container = new Container();

            container.RegisterAll<IUserRepository>(new IUserRepository[] { new SqlUserRepository(), new InMemoryUserRepository() });

            // Act
            var collection = container.GetAllInstances<IUserRepository>();

            // Assert
            Assert_IsNotAMutableCollection(collection);
        }

        [TestMethod]
        public void GetAllInstances_WithListRegistered_DoesNotReturnAnArray()
        {
            // Arrange
            var container = new Container();

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
            var container = new Container();

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
            var container = new Container();

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
            var container = new Container();

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
            var container = new Container();

            // Act
            // PluginContainer depends on IEnumerable<IPlugin>
            var firstContainer = container.GetInstance<PluginContainer>();
            var secondContainer = container.GetInstance<PluginContainer>();

            // Assert
            Assert.AreEqual(firstContainer.Plugins, secondContainer.Plugins, "When a collection is not " +
                "registered, the container should register a single empty instance that can be returned " +
                "every time. This saves performance.");
        }

        private static void Assert_IsNotAMutableCollection<T>(IEnumerable<T> collection)
        {
            string assertMessage = "The container should wrap mutable types to make it impossible for " +
                "users to change the collection.";

            Assert.IsNotInstanceOfType(collection, typeof(T[]), assertMessage);
            Assert.IsNotInstanceOfType(collection, typeof(IList), assertMessage);
            Assert.IsNotInstanceOfType(collection, typeof(ICollection<T>), assertMessage);
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
    }
}