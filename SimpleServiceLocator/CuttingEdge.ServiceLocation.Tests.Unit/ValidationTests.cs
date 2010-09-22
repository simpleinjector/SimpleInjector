using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.ServiceLocation.Tests.Unit
{
    /// <summary>
    /// Tests for validating the <see cref="SimpleServiceLocator"/>.
    /// </summary>
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
        [ExpectedException(typeof(InvalidOperationException), "An exception was expected because the configuration is unvalid without registering an IWeapon.")]
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
    }
}