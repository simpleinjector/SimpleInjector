namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Extensions;

    [TestClass]
    public class IndexableCollectionsTests
    {
        private const string ReadOnlyMessage = "Collection is read-only.";

        private static string NotSupportedMessage
        {
            get { return new NotSupportedException().Message; }
        }        
       
        [TestMethod]
        public void GetAllInstances_OnContainerControlledCollection_ReturnsAGenericIList()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            // Act
            var plugins = container.GetAllInstances<IPlugin>();

            // Assert
            Assert.IsInstanceOfType(plugins, typeof(IList<IPlugin>));
        }

        [TestMethod]
        public void GetAllInstances_OnContainerControlledCollection_CanGetTheInstancesByIndex()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            // Act
            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Assert
            Assert.IsInstanceOfType(plugins[0], typeof(Plugin0));
            Assert.IsInstanceOfType(plugins[1], typeof(Plugin1));
            Assert.IsInstanceOfType(plugins[2], typeof(Plugin2));
        }

        [TestMethod]
        public void Count_OnContainerControlledCollection_ReturnsTheExpectedNumberOfElements1()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1));

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            var count = plugins.Count;

            // Assert
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void Count_OnContainerControlledCollection_ReturnsTheExpectedNumberOfElements2()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            var count = plugins.Count;

            // Assert
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void GetAllInstances_OnContainerControlledCollection_IsReadOnlyReturnsTrue()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            // Act
            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Assert
            Assert.IsTrue(plugins.IsReadOnly);
        }

        [TestMethod]
        public void SetIndexer_OnContainerControlledCollection_ThrowsNotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            Action action = () => plugins[0] = new Plugin0();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(ReadOnlyMessage, action);
        }

        [TestMethod]
        public void RemoveAt_OnContainerControlledCollection_ThrowsNotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            Action action = () => plugins.RemoveAt(0);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(ReadOnlyMessage, action);
        }

        [TestMethod]
        public void Insert_OnContainerControlledCollection_ThrowsNotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            Action action = () => plugins.Insert(0, new Plugin0());

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(ReadOnlyMessage, action);
        }

        [TestMethod]
        public void Add_OnContainerControlledCollection_ThrowsNotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            Action action = () => plugins.Add(new Plugin0());

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(ReadOnlyMessage, action);
        }

        [TestMethod]
        public void Clear_OnContainerControlledCollection_ThrowsNotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            Action action = () => plugins.Clear();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(ReadOnlyMessage, action);
        }

        [TestMethod]
        public void Remove_OnContainerControlledCollection_ThrowsNotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            Action action = () => plugins.Remove(null);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(ReadOnlyMessage, action);
        }

        [TestMethod]
        public void IndexOf_OnContainerControlledCollection_ThrowsNotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            Action action = () => plugins.IndexOf(null);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(NotSupportedMessage, action);
        }

        [TestMethod]
        public void Contains_OnContainerControlledCollection_ThrowsNotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            var plugins = container.GetAllInstances<IPlugin>() as ICollection<IPlugin>;

            // Act
            Action action = () => plugins.Contains(null);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(NotSupportedMessage, action);
        }

        [TestMethod]
        public void ToArray_OnContainerControlledCollection_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            // Act
            var plugins = container.GetAllInstances<IPlugin>().ToArray();

            // Assert
            Assert.IsInstanceOfType(plugins[0], typeof(Plugin0));
            Assert.IsInstanceOfType(plugins[1], typeof(Plugin1));
            Assert.IsInstanceOfType(plugins[2], typeof(Plugin2));
        }

        [TestMethod]
        public void CopyTo_OnContainerControlledCollection_Succeeds()
        {
            // Arrange
            IPlugin[] pluginCopies = new IPlugin[3];

            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            var plugins = container.GetAllInstances<IPlugin>() as ICollection<IPlugin>;

            // Act
            plugins.CopyTo(pluginCopies, 0);

            // Assert
            Assert.IsInstanceOfType(pluginCopies[0], typeof(Plugin0));
            Assert.IsInstanceOfType(pluginCopies[1], typeof(Plugin1));
            Assert.IsInstanceOfType(pluginCopies[2], typeof(Plugin2));
        }

        [TestMethod]
        public void GetAllInstances_OnContainerControlledCollectionByNonGenericRegistration_CanGetTheInstancesByIndex()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll(typeof(IPlugin), new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            // Act
            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Assert
            Assert.IsInstanceOfType(plugins[0], typeof(Plugin0));
            Assert.IsInstanceOfType(plugins[1], typeof(Plugin1));
            Assert.IsInstanceOfType(plugins[2], typeof(Plugin2));
        }

        [TestMethod]
        public void GetAllInstances_OnDecoratedContainerControlledCollection_CanGetTheInstancesByIndex()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator),
                context => context.ImplementationType != typeof(Plugin2));

            // Act
            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Assert
            Assert.IsInstanceOfType(plugins[0], typeof(PluginDecorator));
            Assert.IsInstanceOfType(plugins[1], typeof(PluginDecorator));
            Assert.IsInstanceOfType(plugins[2], typeof(Plugin2));
        }

        [TestMethod]
        public void Count_OnDecoratedContainerControlledCollection_ReturnsTheExpectedNumberOfElements1()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator),
                context => context.ImplementationType != typeof(Plugin2));

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            int count = plugins.Count;

            // Assert
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void IEnumerableGetEnumerator_OnContainerControlledCollection_ReturnsACorrectEnumerator()
        {
            // Arrange
            List<object> pluginsCopy = new List<object>();

            var container = ContainerFactory.New();

            container.RegisterAll<IPlugin>(typeof(Plugin0), typeof(Plugin1), typeof(Plugin2));

            var plugins = (IEnumerable)container.GetAllInstances<IPlugin>();

            // Act
            foreach (var plugin in plugins)
            {
                pluginsCopy.Add(plugin);
            }

            // Assert
            Assert.IsInstanceOfType(pluginsCopy[0], typeof(Plugin0));
            Assert.IsInstanceOfType(pluginsCopy[1], typeof(Plugin1));
            Assert.IsInstanceOfType(pluginsCopy[2], typeof(Plugin2));
        }

        public class Plugin0 : IPlugin
        {
        }

        public class Plugin1 : IPlugin
        {
        }

        public class Plugin2 : IPlugin
        {
        }
    }
}