namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

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
            AssertThat.IsInstanceOfType(typeof(NullLogger), instance);
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

            container.Register<ILogger, NullLogger>(Lifestyle.Singleton);

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
            AssertThat.IsInstanceOfType(typeof(NullLogger), instance);
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

            container.Register<ILogger, NullLogger>(Lifestyle.Singleton);

            var factory = container.GetInstance<Func<ILogger>>();

            // Act
            var instance1 = factory();
            var instance2 = factory();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(instance1, instance2), "Singleton object was expected.");
        }

        [TestMethod]
        public void AllowResolvingParameterizedFuncFactories_ResolvingTypeWithoutDependencies_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingParameterizedFuncFactories();

            // Act
            var tupleFactory = container.GetInstance<Func<int, string, Tuple<int, string>>>();

            var tuple = tupleFactory(1, "foo");

            // Assert
            Assert.AreEqual(1, tuple.Item1);
            Assert.AreEqual("foo", tuple.Item2);
        }

        [TestMethod]
        public void AllowResolvingParameterizedFuncFactories_ResolvingTypeWithOneDependencyAsLastCtorParameter_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingParameterizedFuncFactories();

            container.Register<ILogger, NullLogger>();

            // Act
            var tupleFactory = container.GetInstance<Func<int, string, Tuple<int, string, ILogger>>>();

            var tuple = tupleFactory(0, null);

            // Assert
            AssertThat.IsInstanceOfType(typeof(ILogger), tuple.Item3);
        }

        [TestMethod]
        public void AllowResolvingParameterizedFuncFactories_ResolvingTypeWithOneDependencyAsFirstCtorParameter_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingParameterizedFuncFactories();

            container.Register<ILogger, NullLogger>();

            // Act
            var tupleFactory = container.GetInstance<Func<int, string, Tuple<ILogger, int, string>>>();

            var tuple = tupleFactory(0, null);

            // Assert
            AssertThat.IsInstanceOfType(typeof(NullLogger), tuple.Item1);
        }

        [TestMethod]
        public void AllowResolvingParameterizedFuncFactories_ResolvingTypeWithOneDependencyAtBothSidesOfCtorParameterList_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingParameterizedFuncFactories();

            container.Register<ILogger, NullLogger>();
            container.Register<ICommand, ConcreteCommand>();

            // Act
            var tupleFactory = container.GetInstance<Func<int, string, Tuple<ILogger, int, string, ICommand>>>();

            var tuple = tupleFactory(0, null);

            // Assert
            AssertThat.IsInstanceOfType(typeof(NullLogger), tuple.Item1);
            AssertThat.IsInstanceOfType(typeof(ConcreteCommand), tuple.Item4);
        }

        [TestMethod]
        public void AllowResolvingParameterizedFuncFactories_WithDependencies_PrevervesLifestyles()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingParameterizedFuncFactories();

            container.Register<ILogger, NullLogger>(Lifestyle.Singleton);
            container.Register<ICommand, ConcreteCommand>();

            // Act
            var tupleFactory = container.GetInstance<Func<string, Tuple<ILogger, string, ICommand>>>();

            var tuple1 = tupleFactory(null);
            var tuple2 = tupleFactory(null);

            // Assert
            Assert.AreSame(tuple1.Item1, tuple2.Item1, "Logger is expected to be singleton");
            Assert.AreNotSame(tuple1.Item3, tuple2.Item3, "Command is expected to be transient");
        }

        [TestMethod]
        public void AllowResolvingParameterizedFuncFactories_WithParametersInIncorrectOrder_ThrowExpectedException()
        {
            // Arrange
            var container = new Container();

            container.AllowResolvingParameterizedFuncFactories();

            container.Register<ILogger, NullLogger>(Lifestyle.Singleton);
            container.Register<ICommand, ConcreteCommand>();

            // Act
            Action action = () => container.GetInstance<Func<int, string, Tuple<ILogger, string, int>>>();

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }
    }
}