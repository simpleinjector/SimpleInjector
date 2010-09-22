using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    /// <summary>
    /// Tests for using transient objects with the <see cref="SimpleServiceLocator"/>.
    /// </summary>
    [TestClass]
    public class TransientTests
    {
        [TestMethod]
        public void GetInstance_UnregisteredConcreteType_CanStillBeCreated()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            // Samurai is a concrete class with a constructor with a single argument of type IWeapon.
            var instance = container.GetInstance<Samurai>();

            // Arrange
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void GetInstance_UnregisteredType_AlwaysReturnsANewInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            object instance1 = container.GetInstance<Samurai>();
            object instance2 = container.GetInstance<Samurai>();

            // Assert
            Assert.AreNotEqual(instance1, instance2, "Values should reference different instances.");
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithConcreteConstructorArguments_CanStillBeCreated()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            // SamuraiWrapper is a concrete class with a constructor with a single argument of concrete type 
            // Samurai.
            var instance = container.GetInstance<ConcreteTypeWithConcreteTypeConstructorArgument>();

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithMultiplePublicConstructors_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            try
            {
                // Act
                container.GetInstance<ConcreteTypeWithMultiplePublicConstructors>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ActivationException ex)
            {
                string message = ex.Message;

                Assert.IsTrue(message.Contains(typeof(ConcreteTypeWithMultiplePublicConstructors).FullName),
                    "The exception message should contain the name of the type. Actual message: " + message);

                Assert.IsTrue(message.Contains("type should contain exactly one public constructor"),
                    "The exception message should describe the actual problem. Actual message: " + message);
            }
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithConstructorWithInvalidArguments_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            try
            {
                // Act
                // Because we did not register the IWeapon interface, GetInstance<Samurai> should fail.
                container.GetInstance<Samurai>();

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ActivationException ex)
            {
                string message = ex.Message;

                Assert.IsTrue(message.Contains(typeof(Samurai).FullName),
                    "The exception message should contain the name of the type. Actual message: " + message);

                Assert.IsTrue(message.Contains(typeof(IWeapon).FullName),
                    "The exception message should contain the missing constructor argument. " +
                    "Actual message: " + message);

                Assert.IsTrue(message.Contains("Please ensure IWeapon is registered in the container"),
                    "(1) The exception message should give a solution to solve the problem. " +
                    "Actual message: " + message);

                Assert.IsTrue(message.Contains("register the type Samurai directly"),
                    "(2) The exception message should give a solution to solve the problem. " +
                    "Actual message: " + message);
            }
        }

        [TestMethod]
        public void GetInstance_WithUnregisteredGenericTypeDefinition_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            try
            {
                // Act
                container.GetInstance(typeof(GenericType<>));

                // Assert
                Assert.Fail("Exception was expected.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(typeof(GenericType<>).Name));              
            }
        }

        [TestMethod]
        public void GetInstanceByKey_InstanceSetWithRegisterSingleByKey_AlwaysReturnsSameInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var key = "Sword";
            container.RegisterSingleByKey<IWeapon>(key, new Katana());

            // Act
            var instance1 = container.GetInstance<IWeapon>(key);
            var instance2 = container.GetInstance<IWeapon>(key);

            // Assert
            Assert.IsNotNull(instance1, "GetInstance should never return null.");
            Assert.AreEqual(instance1, instance2, "Values should reference the same instance.");
        }

        [TestMethod]
        public void GetInstanceByKey_InstanceSetWithRegisterByKey_AlwaysReturnsANewInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>("Ninja", new Tanto());
            container.RegisterByKey<Warrior>(name => new Ninja(container.GetInstance<IWeapon>(name)));

            // Act
            var weapon = container.GetInstance<Warrior>("Ninja").Weapon;

            // Assert
            Assert.IsInstanceOfType(weapon, typeof(Tanto));
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstance_NoRegisteredInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.GetInstance<IWeapon>();
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByKey_NoRegisteredInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.GetInstance<IWeapon>("Tanto");
        }

        [TestMethod]
        public void GetInstanceByKey_WithNullKey_ReturnsDefaultInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var defaultInstance = new Katana();
            container.RegisterSingle<IWeapon>(defaultInstance);
            container.RegisterSingleByKey<IWeapon>("Ninja", new Tanto());

            // Act
            var weapon = container.GetInstance<IWeapon>(null);

            // Assert
            Assert.IsInstanceOfType(weapon, defaultInstance.GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByKey_NoInstanceByThatKeyRegisteredWithRegisterByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterByKey<IWeapon>(key =>
            {
                switch (key)
                {
                    case "Tanto":
                        return new Tanto();
                    default:
                        // When name unknown, return null.
                        return null;
                }
            });

            // Act
            container.GetInstance<IWeapon>("Katana");
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByKey_NoInstanceByThatKeyRegisteredWithRegisterSingleByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<IWeapon>("Tanto", new Tanto());

            // Act
            container.GetInstance<IWeapon>("Katana");
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByKey_NullRegisteredWithRegister_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<IWeapon>(() => null);

            // Act
            container.GetInstance<IWeapon>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Register_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.Register<IWeapon>(null);
        }

        [TestMethod]
        public void Register_WithNullKeyCalledAfterRegisterByKey_ReturnDefaultInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Note that the key is not used: a new Tanto will be returned on every call.
            container.RegisterByKey<IWeapon>(key => new Tanto());
            Func<IWeapon> defaultInstance = () => new Katana();
            container.Register<IWeapon>(defaultInstance);

            // Act
            var instance = container.GetInstance<IWeapon>(null);

            // Assert
            // GetInstance(null) will never use the Func<string, T> registered with RegisterByKey.
            Assert.IsInstanceOfType(instance, typeof(Katana));
        }

        [TestMethod]
        public void Register_WithNullKeyCalledAfterRegisterSingleByKey_ReturnsDefaultInstance()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Note that the key is not used: a new Tanto will be returned on every call.
            container.RegisterSingleByKey<IWeapon>("Tanto", new Tanto());
            container.Register<IWeapon>(() => new Katana());

            // Act
            var instance = container.GetInstance<IWeapon>(null);

            // Assert
            // GetInstance(null) will never use the instance register with RegisterSingleByKey.
            Assert.IsInstanceOfType(instance, typeof(Katana));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterByKey_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.RegisterByKey<IWeapon>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterByKey_WithNullString_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            string invalidKey = null;

            // Act
            container.RegisterSingleByKey<IWeapon>(invalidKey, new Katana());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterByKey_WithEmptyString_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var invalidKey = string.Empty;

            // Act
            container.RegisterSingleByKey<IWeapon>(invalidKey, new Katana());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void Register_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<Warrior>(() => new Samurai(null));

            // Act
            container.Register<Warrior>(() => new Ninja(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once.")]
        public void Register_CalledAfterRegisterSingleOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Katana());

            // Act
            container.Register<IWeapon>(() => new Tanto());
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException), "RegisterByKey will only get called with a key that's not null.")]
        public void GetInstance_WithNullKeyCalledAfterRegisterByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Note that the key is not used: a new Tanto will be returned on every call.
            container.RegisterByKey<IWeapon>(key => new Tanto());

            // Act
            var instance = container.GetInstance<IWeapon>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once by key.")]
        public void RegisterByKey_CalledTwiceOnSameType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterByKey<Warrior>(key => new Samurai(null));

            // Act
            container.RegisterByKey<Warrior>(key => new Ninja(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "A certain type can only be registered once by key.")]
        public void RegisterByKey_CalledAfterRegisterSingleByKey_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingleByKey<Warrior>("Samurai", new Samurai(null));

            // Act
            container.RegisterByKey<Warrior>(key => new Ninja(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void Register_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Tanto());
            container.GetInstance<IWeapon>();

            // Act
            container.Register<Warrior>(() => new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetAllInstances.")]
        public void Register_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();

            // Only during iterating the collection, will the underlying container be called.
            var count = weapons.Count();

            // Act
            container.Register<Warrior>(() => new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetInstance.")]
        public void RegisterByKey_AfterCallingGetInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<IWeapon>(new Tanto());
            container.GetInstance<IWeapon>();

            // Act
            container.RegisterByKey<Warrior>(key => new Samurai(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "The container should get locked after a call to GetAllInstances.")]
        public void RegisterByKey_AfterCallingGetAllInstances_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            var weapons = container.GetAllInstances<IWeapon>();
            var count = weapons.Count();

            // Act
            container.RegisterByKey<Warrior>(key => new Samurai(null));
        }

        [TestMethod]
        public void FormatActivationExceptionMessage_NullException_ReturnsDefaultMessage()
        {
            // Arrange
            var simpleServiceLocator = new FakeSimpleServiceLocator();
            var key = "key";
            var expectedMessage = 
                "Activation error occured while trying to get instance of type Int32, key \"key\".";

            // Act
            var actualMessage = simpleServiceLocator.FormatActivationExceptionMessage(null, typeof(int), key);

            // Assert
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public void FormatActivationExceptionMessage_ExceptionWithMessage_ReturnsDefaultMessage()
        {
            // Arrange
            var simpleServiceLocator = new FakeSimpleServiceLocator();
            var exception = new InvalidOperationException("Some message.");
            var key = "key";
            var expectedMessage = "Activation error occured while trying to get instance of type Int32, " +
                "key \"key\". Some message.";

            // Act
            var actualMessage = 
                simpleServiceLocator.FormatActivationExceptionMessage(exception, typeof(int), key);

            // Assert
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public void FormatActivateAllExceptionMessage_NullException_ReturnsDefaultMessage()
        {
            // Arrange
            var simpleServiceLocator = new FakeSimpleServiceLocator();
            var expectedMessage =
                "Activation error occured while trying to get all instances of type Int32.";

            // Act
            var actualMessage = simpleServiceLocator.FormatActivateAllExceptionMessage(null, typeof(int));

            // Assert
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public void FormatActivateAllExceptionMessage_ExceptionWithMessage_ReturnsDefaultMessage()
        {
            // Arrange
            var simpleServiceLocator = new FakeSimpleServiceLocator();
            var exception = new InvalidOperationException("Some message.");
            var expectedMessage =
                "Activation error occured while trying to get all instances of type Int32. Some message.";

            // Act
            var actualMessage = simpleServiceLocator.FormatActivateAllExceptionMessage(exception, typeof(int));

            // Assert
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        private sealed class FakeSimpleServiceLocator : SimpleServiceLocator
        {
            public new string FormatActivateAllExceptionMessage(Exception actualException, Type serviceType)
            {
                return base.FormatActivateAllExceptionMessage(actualException, serviceType);
            }

            public new string FormatActivationExceptionMessage(Exception actualException, Type serviceType, string key)
            {
                return base.FormatActivationExceptionMessage(actualException, serviceType, key);
            }
        }
    }
}