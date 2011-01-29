using System;

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
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterByAction_WithAbstractType_ThrowsException()
        {
            // Arrange
            var container = new SimpleServiceLocator();

            Action<IWeapon> validInstanceInitializer = _ => { };

            // Act
            // IWeapon is not an concrete type.
            container.Register<IWeapon>(validInstanceInitializer);
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
    }
}