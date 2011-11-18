namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ResolvingFactoriesExtensionsTests
    {
        [TestMethod]
        public void AllowResolvingLazyFactories_GettingALazyForARegisteredType_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingLazyFactories();

            container.Register<ILogger, NullLogger>();

            // Act
            container.GetInstance<Lazy<ILogger>>();
        }

        [TestMethod]
        public void AllowResolvingLazyFactories_InvokingTheValueReturnedFromGetInstance_CreatesAnInstanceOfTheExpectedType()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingLazyFactories();

            container.Register<ILogger, NullLogger>();

            var lazy = container.GetInstance<Lazy<ILogger>>();

            // Act
            var instance = lazy.Value;

            // Assert
            Assert.IsInstanceOfType(instance, typeof(NullLogger));
        }

        [TestMethod]
        public void AllowResolvingLazyFactories_InvokingTheFactoryMultipleTimesReturnedFromGetInstance_PreservesTheLifeTime1()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingLazyFactories();

            container.Register<ILogger, NullLogger>();

            var lazy1 = container.GetInstance<Lazy<ILogger>>();
            var lazy2 = container.GetInstance<Lazy<ILogger>>();

            // Act
            var instance1 = lazy1.Value;
            var instance2 = lazy2.Value;

            // Assert
            Assert.IsFalse(object.ReferenceEquals(instance1, instance2), "Transient object was expected.");
        }

        [TestMethod]
        public void AllowResolvingLazyFactories_InvokingTheFactoryMultipleTimesReturnedFromGetInstanceFun_PreservesTheLifeTime2()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingLazyFactories();

            container.RegisterSingle<ILogger, NullLogger>();

            var lazy1 = container.GetInstance<Lazy<ILogger>>();
            var lazy2 = container.GetInstance<Lazy<ILogger>>();

            // Act
            var instance1 = lazy1.Value;
            var instance2 = lazy2.Value;

            // Assert
            Assert.IsTrue(object.ReferenceEquals(instance1, instance2), "Singleton object was expected.");
        }
        
        [TestMethod]
        public void AllowResolvingFuncFactories_GetInstanceOnUnregisteredFuncDelegate_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingFuncFactories();

            container.Register<ILogger, NullLogger>();

            // Act
            container.GetInstance<Func<ILogger>>();
        }

        [TestMethod]
        public void AllowResolvingFuncFactories_InvokingTheFactoryReturnedFromGetInstanceFun_CreatesAnInstanceOfTheExpectedType()
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
        public void AllowResolvingFuncFactories_InvokingTheFactoryMultipleTimesReturnedFromGetInstanceFun_PreservesTheLifeTime1()
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
        public void AllowResolvingFuncFactories_InvokingTheFactoryMultipleTimesReturnedFromGetInstanceFun_PreservesTheLifeTime2()
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
