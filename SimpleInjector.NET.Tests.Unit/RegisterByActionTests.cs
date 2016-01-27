﻿using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    /// <summary>
    /// Tests for the <see cref="SimpleServiceLocator.Register{T}(Action{T})"/> overload.
    /// This overload allows registering concrete transient instances to be returned and initialized using the
    /// supplied Action{T} delegate.
    /// </summary>
    [TestClass]
    public class RegisterByActionTests
    {
        [TestMethod]
        public void RegisterByAction_WithValidAction_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.Register<Samurai>(samurai => { });
        }

        [TestMethod]
        public void GetInstance_RegisterByAction_CallsActionOnce()
        {
            // Arrange
            const int ExpectedNumberOfCalls = 3;
            int actualNumberOfCalls = 0;

            var container = new SimpleServiceLocator();

            // Samurai takes an IWeapon as constructor argument.
            container.RegisterSingle<IWeapon>(new Katana());

            Action<Samurai> instanceInitializer = _ => { actualNumberOfCalls++; };

            container.Register<Samurai>(instanceInitializer);

            // Act
            container.GetInstance<Samurai>();
            container.GetInstance<Samurai>();
            container.GetInstance<Samurai>();
            
            // Assert
            Assert.AreEqual(ExpectedNumberOfCalls, actualNumberOfCalls,
                "The Action<T> was expected to be called once.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterByAction_WithNullArgument_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            Action<Samurai> invalidInstanceInitializer = null;

            // Act
            container.Register<Samurai>(invalidInstanceInitializer);
        }

        [TestMethod]
        public void RegisterByAction_WithAbstractType_ThrowsException()
        {
            // Arrange
            string expectedParameterName = "TConcrete";
            string expectedMessage = "IWeapon is not a concrete type.";

            var container = new SimpleServiceLocator();

            Action<IWeapon> validInstanceInitializer = _ => { };

            try
            {
                // Act
                // IWeapon is not an concrete type.
                container.Register<IWeapon>(validInstanceInitializer);

                Assert.Fail("IWeapon is not an concrete type and the registration is expected to fail.");
            }
            catch (ArgumentException ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException), "No sub type is expected.");
                
                Assert.AreEqual(expectedParameterName, ex.ParamName);

                Assert.IsTrue(ex.Message.Contains(expectedMessage), "Actual message: " + ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterByAction_CalledAfterCallToGetInstance_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<IWeapon>(() => new Katana());
            container.GetInstance<IWeapon>();

            Action<Samurai> validInstanceInitializer = _ => { };

            // Act
            container.Register<Samurai>(validInstanceInitializer);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterByAction_CalledAfterTypeAlreadyRegisteredUsingRegisterByFunc_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.Register<Samurai>(() => new Samurai(null));

            // Act
            container.Register<Samurai>(_ => { });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterByAction_CalledAfterTypeAlreadyRegisteredUsingRegisterByAction_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.Register<Samurai>(_ => { });

            // Act
            container.Register<Samurai>(_ => { });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterByAction_CalledAfterTypeAlreadyRegisteredUsingRegisterSigle_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingle<Samurai>();

            // Act
            container.Register<Samurai>(_ => { });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterByAction_CalledAfterTypeAlreadyRegisteredUsingRegisterSingleByFunc_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingle<Samurai>(() => new Samurai(null));

            // Act
            container.Register<Samurai>(_ => { });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterByAction_CalledAfterTypeAlreadyRegisteredUsingRegisterSingleByAction_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingle<Samurai>(_ => { });

            // Act
            container.Register<Samurai>(_ => { });
        }

        [TestMethod]
        public void RegisterByAction_CalledAfterTypeAlreadyRegisteredUsingRegisterFuncByKey_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterByKey<Samurai>("Sammy the Samurai", () => new Samurai(null));

            // Act
            container.Register<Samurai>(_ => { });
        }

        [TestMethod]
        public void RegisterByAction_CalledAfterTypeAlreadyRegisteredUsingRegisterByKeyedFunc_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterByKey<Samurai>(key => new Samurai(null));

            // Act
            container.Register<Samurai>(_ => { });
        }

        [TestMethod]
        public void RegisterByAction_CalledAfterTypeAlreadyRegisteredUsingRegisterSingleInstanceByKey_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingleByKey<Samurai>("Sammy the Samurai", new Samurai(null));

            // Act
            container.Register<Samurai>(_ => { });
        }

        [TestMethod]
        public void RegisterByAction_CalledAfterTypeAlreadyRegisteredUsingRegisterSingleFuncByKey_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingleByKey<Samurai>("Sammy the Samurai", () => new Samurai(null));

            // Act
            container.Register<Samurai>(_ => { });
        }

        [TestMethod]
        public void RegisterByAction_CalledAfterTypeAlreadyRegisteredUsingRegisterSingleByKeyedFunc_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingleByKey<Samurai>(key => new Samurai(null));

            // Act
            container.Register<Samurai>(_ => { });
        }

        [TestMethod]
        public void RegisterByAction_CalledAfterTypeAlreadyRegisteredUsingRegisterAll_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterAll<Samurai>(new[] { new Samurai(null) });

            // Act
            container.Register<Samurai>(_ => { });
        }

        [TestMethod]
        public void RegisterByAction_WithIncompleteSingletonRegistration_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Samurai is dependant on IWeapon.
            container.RegisterSingle<Warrior>(() => container.GetInstance<Samurai>());

            // Act
            // Kingdom is dependant on Warrior. Registration should succeed even though IWeapon is not 
            // registered yet.
            container.Register<Kingdom>(k =>
            {
                k.Karma = 5;
            });
        }

        [TestMethod]
        public void GetInstance_ForTypeDependingOnTransientType_ContainerWillRunInitializerOnType()
        {
            // Arrange
            int expectedValue = 10;

            var container = new SimpleServiceLocator();

            container.Register<Service>(createdService =>
            {
                createdService.Value = expectedValue;
            });

            // Act
            var consumer = container.GetInstance<Consumer>();

            // Assert
            Assert.AreEqual(expectedValue, consumer.Service.Value, "The Service initializer was not called.");
        }

        private sealed class Service
        {
            public int Value { get; set; }
        }

        private sealed class Consumer
        {
            public Consumer(Service service)
            {
                this.Service = service;
            }

            public Service Service { get; private set; }
        }
    }
}