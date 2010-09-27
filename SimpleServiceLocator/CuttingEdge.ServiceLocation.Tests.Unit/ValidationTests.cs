using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    [TestClass]
    public class ValidationTests
    {
        [TestMethod]
        public void Validate_WithEmptyConfiguration_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Act
            container.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Validate_Always_LocksContainer()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.Validate();

            // Act
            container.RegisterSingle<IWeapon>(new Katana());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Registration of a type after validation should fail, because the container should be locked down.")]
        public void Validate_WithEmptyConfiguration_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterSingle<Samurai>();
            container.Validate();

            // Act
            container.Register<Warrior>(() => container.GetInstance<Samurai>());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "An exception was expected because the configuration is invalid without registering an IWeapon.")]
        public void Validate_WithDependantTypeNotRegistered_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            // Samurai has a constructor that takes an IWeapon.
            container.RegisterSingle<Samurai>();

            // Act
            container.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Validate_WithFailingFunc_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.Register<IWeapon>(() =>
            {
                throw new ArgumentNullException();
            });

            // Act
            container.Validate();
        }

        [TestMethod]
        public void Validate_RegisteredCollectionWithValidElements_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterAll<IWeapon>(new IWeapon[] { new Katana(), new Tanto() });

            // Act
            container.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Validate_RegisteredCollectionWithNullElements_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();
            container.RegisterAll<IWeapon>(new IWeapon[] { null });

            // Act
            container.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Validate_FailingCollection_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            IEnumerable<IWeapon> weapons =
                from nullWeapon in Enumerable.Repeat<IWeapon>(null, 1)
                where nullWeapon.ToString() == "This line fails with an NullReferenceException"
                select nullWeapon;

            container.RegisterAll<IWeapon>(weapons);

            // Act
            container.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Validate_FailingKeyedSingleton_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingleByKey<IWeapon>("katana", () =>
            {
                throw new NullReferenceException();
            });

            // Act
            container.Validate();
        }

        [TestMethod]
        public void Validate_FailingKeyedFuncSingleton_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterSingleByKey<IWeapon>(key =>
            {
                throw new NullReferenceException();
            });

            // Act
            // This call will succeed, because there is no way for the container to know by which keys
            // the delegate can be called for testing.
            container.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Validate_RegisterCalledWithFuncReturningNullInstances_ThrowsExpectedException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.Register<IWeapon>(() => null);

            // Act
            container.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Validate_RegisterByKeyCalledWithFuncReturningNullInstances_ThrowsExpectedException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterByKey<IWeapon>("katana", () => null);

            // Act
            container.Validate();
        }

        [TestMethod]
        public void Validate_RegisterByKeyCalledWithValidFunc_Succeeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            container.RegisterByKey<IWeapon>("katana", () => new Katana());

            // Act
            container.Validate();
        }

        [TestMethod]
        public void Validate_RegisterByKeyKeyedFuncWithInvalidFunc_StillSucceeds()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            Func<string, IWeapon> throwingFunc = key =>
            {
                switch (key)
                {
                    default: throw new InvalidOperationException(); 
                }
            };

            container.RegisterByKey<IWeapon>(throwingFunc);

            // Act
            // This call will still succeed, because there is no way to validate that delegate.
            container.Validate();
        }
    }
}