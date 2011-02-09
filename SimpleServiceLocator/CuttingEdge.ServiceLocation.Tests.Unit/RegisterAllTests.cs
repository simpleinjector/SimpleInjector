using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class RegisterAllTests
    {
        [TestMethod]
        public void GetInstance_TypeWithEnumerableAsConstructorArguments_InjectsExpectedTypes()
        {
            // Arrange
            var container = new SimpleServiceLocator();

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
            var container = new SimpleServiceLocator();

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
            var container = new SimpleServiceLocator();

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
            var container = new SimpleServiceLocator();

            container.RegisterAll<IPlugin>(new PluginImpl());

            // Act
            container.RegisterSingle<IEnumerable<IPlugin>>(new IPlugin[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Register_WithEnumerableCalledAfterRegisterAllWithSameType_Fails()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterAll<IPlugin>(new PluginImpl());

            // Act
            container.Register<IEnumerable<IPlugin>>(() => new IPlugin[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterAll_WithEnumerableCalledAfterRegisterSingleWithSameType_Fails()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingle<IEnumerable<IPlugin>>(new IPlugin[0]);
            
            // Act
            container.RegisterAll<IPlugin>(new PluginImpl());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterAll_WithEnumerableCalledAfterRegisterWithSameType_Fails()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.Register<IEnumerable<IPlugin>>(() => new IPlugin[0]);

            // Act
            container.RegisterAll<IPlugin>(new PluginImpl());
        }

        [TestMethod]
        public void GetAllInstances_ListRegisteredUsingEnumerable_ReturnsExpectedList()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            IEnumerable<IWeapon> weaponsToRegister = new IWeapon[] { new Tanto(), new Katana() };
            container.RegisterAll<IWeapon>(weaponsToRegister);

            // Act
            var weapons = container.GetAllInstances<IWeapon>();

            // Assert
            Assert.IsNotNull(weapons, "This method MUST NOT return null.");
            Assert.AreEqual(2, weapons.Count(), "Collection is expected to contain two values.");
        }

        [TestMethod]
        public void GetAllInstances_ListRegisteredUsingParams_ReturnsExpectedList()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            
            container.RegisterAll<IWeapon>(new Tanto(), new Katana());

            // Act
            var weapons = container.GetAllInstances<IWeapon>();

            // Assert
            Assert.IsNotNull(weapons, "This method MUST NOT return null.");
            Assert.AreEqual(2, weapons.Count(), "Collection is expected to contain two values.");
        }

        [TestMethod]
        public void GetAllInstances_NoInstancesRegistered_ReturnsEmptyCollection()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            var weapons = container.GetAllInstances<IWeapon>();

            // Assert
            Assert.IsNotNull(weapons, "This method MUST NOT return null.");
            Assert.AreEqual(0, weapons.Count(),
                "If no instances of the requested type are available, this method MUST return an " +
                "enumerator of length 0 instead of throwing an exception.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterAll_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Tanto());
            container.GetInstance<IWeapon>();

            // Act
            container.RegisterAll<IWeapon>(new IWeapon[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetAllInstances.")]
        public void RegisterAll_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();
            var count = weapons.Count();

            // Act
            container.RegisterAll<IWeapon>(new IWeapon[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterAll_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterAll<IWeapon>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterAll_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = new IWeapon[] { new Tanto(), new Katana() };
            container.RegisterAll<IWeapon>(weapons);

            // Act
            container.RegisterAll<IWeapon>(weapons);
        }

        [TestMethod]
        public void GetAllInstancesNonGeneric_WithoutAnRegistration_ReturnsAnEmptyCollection()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            var weapons = container.GetAllInstances(typeof(IWeapon));

            // Assert
            Assert.AreEqual(0, weapons.Count());
        }

        [TestMethod]
        public void GetAllInstancesNonGeneric_WithValidRegistration_ReturnsCollectionWithExpectedElements()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterAll<IWeapon>(new Tanto(), new Katana());

            // Act
            var weapons = container.GetAllInstances(typeof(IWeapon)).ToArray();

            // Assert
            Assert.AreEqual(2, weapons.Length);
            Assert.IsInstanceOfType(weapons[0], typeof(Tanto));
            Assert.IsInstanceOfType(weapons[1], typeof(Katana));
        }

        [TestMethod]
        public void GetAllInstances_InvalidDelegateRegistered_ThrowsExceptionWithExpectedMessage()
        {
            // Arrange
            string expectedMessage =
                "The registered delegate for type System.Collections.Generic.IEnumerable`1[" +
                    "CuttingEdge.ServiceLocation.Tests.Unit.IWeapon] returned null.";
            
            var container = new SimpleServiceLocator();

            Func<IEnumerable<IWeapon>> invalidDelegate = () => null;

            container.Register<IEnumerable<IWeapon>>(invalidDelegate);

            try
            {
                // Act
                container.GetAllInstances<IWeapon>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(expectedMessage), "Actual message: " + ex.Message);                
            }
        }

        [TestMethod]
        public void GetAllInstances_WithArrayRegistered_DoesNotReturnAnArray()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterAll<IWeapon>(new IWeapon[] { new Katana(), new Tanto() });

            // Act
            var collection = container.GetAllInstances<IWeapon>();

            // Assert
            Assert_IsNotAMutableCollection(collection);
        }

        [TestMethod]
        public void GetAllInstances_WithListRegistered_DoesNotReturnAnArray()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterAll<IWeapon>(new List<IWeapon> { new Katana(), new Tanto() });

            // Act
            var collection = container.GetAllInstances<IWeapon>();

            // Assert
            Assert_IsNotAMutableCollection(collection);
        }

        [TestMethod]
        public void GetAllInstances_WithCollectionRegistered_DoesNotReturnAnArray()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterAll<IWeapon>(new Collection<IWeapon> { new Katana(), new Tanto() });

            // Act
            var collection = container.GetAllInstances<IWeapon>();

            // Assert
            Assert_IsNotAMutableCollection(collection);
        }

        [TestMethod]
        public void GetAllInstances_WithArray_ReturnsSameInstanceOnEachCall()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterAll<IWeapon>(new IWeapon[] { new Katana(), new Tanto() });

            // Act
            var collection1 = container.GetAllInstances<IWeapon>();
            var collection2 = container.GetAllInstances<IWeapon>();

            // Assert
            Assert.AreEqual(collection1, collection2,
                "For performance reasons, GetAllInstances<T> should always return the same instance.");
        }

        [TestMethod]
        public void GetInstance_OnATypeThatDependsOnACollectionThatIsRegisteredWithRegisterByFunc_CollectionShouldNotBeTreatedAsASingleton()
        {
            // Arrange
            var container = new SimpleServiceLocator();

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
            var container = new SimpleServiceLocator();

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