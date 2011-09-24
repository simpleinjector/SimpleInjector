namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ResolvingFuncFactoriesExtensionsTests
    {
        [TestMethod]
        public void GetInstance_OnUnregisteredFuncDelegate_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingFuncFactories();

            container.Register<ILogger, NullLogger>();

            // Act
            container.GetInstance<Func<ILogger>>();
        }

        [TestMethod]
        public void InvokingTheFactory_ReturnedFromGetInstanceFun_CreatesAnInstanceOfTheExpectedType()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingFuncFactories();

            container.Register<ILogger, NullLogger>();

            var factory = container.GetInstance<Func<ILogger>>();

            // Act
            var instance = factory();

            // Assert
            Assert.IsInstanceOfType(instance, typeof(NullLogger));
        }

        [TestMethod]
        public void InvokingTheFactoryMultipleTimes_ReturnedFromGetInstanceFun_PreservesTheLifeTime1()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingFuncFactories();

            container.Register<ILogger, NullLogger>();

            var factory = container.GetInstance<Func<ILogger>>();

            // Act
            var instance1 = factory();
            var instance2 = factory();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(instance1, instance2), "Transient object was expected.");
        }

        [TestMethod]
        public void InvokingTheFactoryMultipleTimes_ReturnedFromGetInstanceFun_PreservesTheLifeTime2()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingFuncFactories();

            container.RegisterSingle<ILogger, NullLogger>();

            var factory = container.GetInstance<Func<ILogger>>();

            // Act
            var instance1 = factory();
            var instance2 = factory();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(instance1, instance2), "Singleton object was expected.");
        }
    }
}
