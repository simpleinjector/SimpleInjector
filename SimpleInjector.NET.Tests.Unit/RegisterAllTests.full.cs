namespace SimpleInjector.Tests.Unit
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Extensions;

    /// <summary>Tests for RegisterAll for the full .NET version.</summary>
    public partial class RegisterAllTests
    {
        [TestMethod]
        public void GetInstance_TypeDependingOnIReadOnlyCollection_InjectsTheRegisteredCollection()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(PluginImpl), typeof(PluginImpl2));

            // Act
            IReadOnlyCollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<IReadOnlyCollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, collection.Count);
            Assert.IsInstanceOfType(collection.First(), typeof(PluginImpl));
            Assert.IsInstanceOfType(collection.Second(), typeof(PluginImpl2));
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnIReadOnlyCollection_InjectsTheRegisteredCollectionOfDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(PluginImpl), typeof(PluginImpl2));

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            // Act
            IReadOnlyCollection<IPlugin> collection =
                container.GetInstance<ClassDependingOn<IReadOnlyCollection<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, collection.Count);
            Assert.IsInstanceOfType(collection.First(), typeof(PluginDecorator));
            Assert.IsInstanceOfType(collection.Second(), typeof(PluginDecorator));
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnIReadOnlyCollection_InjectsEmptyCollectionWhenNoInstancesRegistered()
        {
            // Arrange
            var container = ContainerFactory.New();

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

            container.RegisterAll<IPlugin>(typeof(PluginImpl), typeof(PluginImpl2));

            // Act
            IReadOnlyList<IPlugin> list = 
                container.GetInstance<ClassDependingOn<IReadOnlyList<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, list.Count);
            Assert.IsInstanceOfType(list[0], typeof(PluginImpl));
            Assert.IsInstanceOfType(list[1], typeof(PluginImpl2));
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnIReadOnlyList_InjectsTheRegisteredListOfDecorators()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(PluginImpl), typeof(PluginImpl2));

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            // Act
            IReadOnlyList<IPlugin> list = 
                container.GetInstance<ClassDependingOn<IReadOnlyList<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(2, list.Count);
            Assert.IsInstanceOfType(list[0], typeof(PluginDecorator));
            Assert.IsInstanceOfType(list[0], typeof(PluginDecorator));
        }

        [TestMethod]
        public void GetInstance_TypeDependingOnIReadOnlyList_InjectsEmptyListWhenNoInstancesRegistered()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            IReadOnlyList<IPlugin> list = 
                container.GetInstance<ClassDependingOn<IReadOnlyList<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(0, list.Count);
        }
    }
}