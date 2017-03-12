#pragma warning disable 0618
namespace SimpleInjector.Tests.Unit
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests for RegisterAll for the full .NET version.</summary>
    public partial class RegisterAllTests
    {
        [TestMethod]
        public void GetInstance_TypeDependingOnIReadOnlyCollection_InjectsTheRegisteredCollection()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

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

            container.RegisterCollection<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

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

            container.RegisterCollection<IPlugin>();

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

            container.RegisterCollection<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

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

            container.RegisterCollection<IPlugin>(new[] { typeof(PluginImpl), typeof(PluginImpl2) });

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

            container.RegisterCollection<IPlugin>();

            // Act
            IReadOnlyList<IPlugin> list = 
                container.GetInstance<ClassDependingOn<IReadOnlyList<IPlugin>>>().Dependency;

            // Assert
            Assert.AreEqual(0, list.Count);
        }
    }
}