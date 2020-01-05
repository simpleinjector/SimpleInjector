namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests for RegisterCollection for the full .NET version.</summary>
    public partial class RegisterCollectionTests
    {
        [TestMethod]
        public void GetInstance_TypeDependingOnIReadOnlyCollection_InjectsTheRegisteredCollection()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            // Act
            IReadOnlyCollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<IReadOnlyCollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, collection.Count);
            AssertThat.IsInstanceOfType(typeof(PluginImpl), collection.First());
            AssertThat.IsInstanceOfType(typeof(PluginImpl2), collection.Second());
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnIReadOnlyCollection_InjectsTheRegisteredCollectionOfDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            // Act
            IReadOnlyCollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<IReadOnlyCollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, collection.Count);
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), collection.First());
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), collection.Second());
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnIReadOnlyCollection_InjectsEmptyCollectionWhenNoInstancesRegistered()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(Type.EmptyTypes);

            // Act
            IReadOnlyCollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<IReadOnlyCollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnIReadOnlyList_InjectsTheRegisteredList()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            // Act
            IReadOnlyList<IPlugin> list =
                container.GetInstance<ClassDependingOn<IReadOnlyList<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, list.Count);
            AssertThat.IsInstanceOfType(typeof(PluginImpl), list[0]);
            AssertThat.IsInstanceOfType(typeof(PluginImpl2), list[1]);
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnIReadOnlyList_InjectsTheRegisteredListOfDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            // Act
            IReadOnlyList<IPlugin> list =
                container.GetInstance<ClassDependingOn<IReadOnlyList<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, list.Count);
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), list[0]);
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), list[0]);
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnIReadOnlyList_InjectsEmptyListWhenNoInstancesRegistered()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(Type.EmptyTypes);

            // Act
            IReadOnlyList<IPlugin> list =
                container.GetInstance<ClassDependingOn<IReadOnlyList<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(0, list.Count);
        }
        
        [TestMethod]
        public void GetInstance_TypeDependingOnReadOnlyCollection_InjectsTheRegisteredCollection()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            // Act
            ReadOnlyCollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<ReadOnlyCollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, collection.Count);
            AssertThat.IsInstanceOfType(typeof(PluginImpl), collection.First());
            AssertThat.IsInstanceOfType(typeof(PluginImpl2), collection.Second());
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnReadOnlyCollection_InjectsTheRegisteredCollectionOfDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            // Act
            ReadOnlyCollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<ReadOnlyCollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, collection.Count);
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), collection.First());
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), collection.Second());
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnReadOnlyCollection_InjectsEmptyCollectionWhenNoInstancesRegistered()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(Type.EmptyTypes);

            // Act
            ReadOnlyCollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<ReadOnlyCollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void ReadOnlyCollection_WhenInjected_IsAlwaysASingleton()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl) });

            // Act
            ReadOnlyCollection<IPlugin> collection1 =
                container.GetInstance<ClassDependingOn<ReadOnlyCollection<IPlugin>>>().Dependency;

            ReadOnlyCollection<IPlugin> collection2 =
                container.GetInstance<ClassDependingOn<ReadOnlyCollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreSame(collection1, collection2, "ReadOnlyCollection<T> should be a singleton.");
        }

        [TestMethod]
        public void ReadOnlyCollection_WhenIterated_ProducesInstancesAccordingToTheirLifestyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append<IPlugin, PluginImpl>(Lifestyle.Transient);
            container.Collection.Append<IPlugin, PluginImpl2>(Lifestyle.Singleton);

            // Act
            ReadOnlyCollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<ReadOnlyCollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreNotSame(collection.First(), collection.First(), "PluginImpl should be transient.");
            Assert.AreSame(collection.Last(), collection.Last(), "PluginImpl2 should be singleton.");
        }
    }
}