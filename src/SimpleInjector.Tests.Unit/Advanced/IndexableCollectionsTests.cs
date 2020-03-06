namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IndexableCollectionsTests
    {
        private const string ReadOnlyMessage = "Collection is read-only.";

        private static string NotSupportedMessage => new NotSupportedException().Message;

        [TestMethod]
        public void GetAllInstances_Always_ReturnsAGenericIList()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            // Act
            var plugins = container.GetAllInstances<IPlugin>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(IList<IPlugin>), plugins);
        }

        [TestMethod]
        public void GetAllInstances_WithValidRegistrations_CanGetTheInstancesByIndex()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            // Act
            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Assert
            AssertThat.IsInstanceOfType(typeof(Plugin0), plugins[0]);
            AssertThat.IsInstanceOfType(typeof(Plugin1), plugins[1]);
            AssertThat.IsInstanceOfType(typeof(Plugin2), plugins[2]);
        }

        [TestMethod]
        public void Count_CollectionWithTwoElements_Returns2()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1) });

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            var count = plugins.Count;

            // Assert
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void Count_CollectionWithThreeElements_Returns3()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            var count = plugins.Count;

            // Assert
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void GetAllInstances_Always_IsReadOnlyReturnsTrue()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            // Act
            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Assert
            Assert.IsTrue(plugins.IsReadOnly);
        }

        [TestMethod]
        public void SetIndexer_Always_ThrowsNotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            Action action = () => plugins[0] = new Plugin0();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(ReadOnlyMessage, action);
        }

        [TestMethod]
        public void RemoveAt_Always_ThrowsNotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            Action action = () => plugins.RemoveAt(0);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(ReadOnlyMessage, action);
        }

        [TestMethod]
        public void Insert_Always_ThrowsNotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            Action action = () => plugins.Insert(0, new Plugin0());

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(ReadOnlyMessage, action);
        }

        [TestMethod]
        public void Add_Always_ThrowsNotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ILogger, NullLogger>(Lifestyle.Transient);
            container.Collection.Register<IPlugin>(new[] { typeof(PluginX<ILogger>), typeof(Plugin2) });

            container.Verify();

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            Action action = () => plugins.Add(new Plugin0());

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(ReadOnlyMessage, action);
        }

        public class PluginX<TDependency> : IPlugin
        {
            public PluginX(TDependency dependency)
            {

            }
        }

        [TestMethod]
        public void Clear_Always_ThrowsNotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            Action action = () => plugins.Clear();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(ReadOnlyMessage, action);
        }

        [TestMethod]
        public void Remove_Always_ThrowsNotSupported()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            Action action = () => plugins.Remove(null);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(ReadOnlyMessage, action);
        }

        [TestMethod]
        public void IndexOf_Null_ReturnsNegative1()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            int actualIndex = plugins.IndexOf(null);

            // Assert
            Assert.AreEqual(-1, actualIndex);
        }

        [TestMethod]
        public void GetAllInstances_NonGenericAppendTypeWithLifestyle_RegistersTypeAccordingToExpectedLifestyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append(typeof(IPlugin), typeof(Plugin0), Lifestyle.Singleton);
            container.Collection.Append(typeof(IPlugin), typeof(Plugin1), Lifestyle.Transient);

            // Act
            var plugins = container.GetAllInstances<IPlugin>();

            // Assert
            Assert.AreSame(plugins.First(), plugins.First(), "Plugin0 should be Singleton.");
            Assert.AreNotSame(plugins.Last(), plugins.Last(), "Plugin1 should be Transient.");
        }
        
        [TestMethod]
        public void Index_ValuePartOfTheCollection1_ReturnsCorrectIndex()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append<IPlugin, Plugin0>(Lifestyle.Singleton);
            container.Collection.Append<IPlugin, Plugin1>(Lifestyle.Singleton);
            container.Collection.Append<IPlugin, Plugin2>(Lifestyle.Singleton);

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            int actualIndex = plugins.IndexOf(plugins.First());

            // Assert
            Assert.AreEqual(0, actualIndex);
        }

        [TestMethod]
        public void Index_ValuePartOfTheCollection2_ReturnsCorrectIndex()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append<IPlugin, Plugin0>(Lifestyle.Singleton);
            container.Collection.Append<IPlugin, Plugin1>(Lifestyle.Singleton);
            container.Collection.Append<IPlugin, Plugin2>(Lifestyle.Singleton);

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            int actualIndex = plugins.IndexOf(plugins.Last());

            // Assert
            Assert.AreEqual(2, actualIndex);
        }

        [TestMethod]
        public void Contains_ValuePartOfTheCollection_ReturnsTrue()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append<IPlugin, Plugin0>(Lifestyle.Singleton);
            container.Collection.Append<IPlugin, Plugin1>(Lifestyle.Singleton);
            container.Collection.Append<IPlugin, Plugin2>(Lifestyle.Singleton);

            var plugins = container.GetAllInstances<IPlugin>() as ICollection<IPlugin>;

            // Act
            var result = plugins.Contains(plugins.Last());

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Contains_ValueNotPartOfTheCollection_ReturnsFalse()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append<IPlugin, Plugin0>(Lifestyle.Singleton);
            container.Collection.Append<IPlugin, Plugin1>(Lifestyle.Singleton);
            container.Collection.Append<IPlugin, Plugin2>(Lifestyle.Singleton);

            var plugins = container.GetAllInstances<IPlugin>() as ICollection<IPlugin>;

            var differentInstance = new Plugin2();

            // Act
            var result = plugins.Contains(differentInstance);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ToArray_WithValidRegistrations_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            // Act
            var plugins = container.GetAllInstances<IPlugin>().ToArray();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Plugin0), plugins[0]);
            AssertThat.IsInstanceOfType(typeof(Plugin1), plugins[1]);
            AssertThat.IsInstanceOfType(typeof(Plugin2), plugins[2]);
        }

        [TestMethod]
        public void CopyTo_WithValidRegistrations_Succeeds()
        {
            // Arrange
            IPlugin[] pluginCopies = new IPlugin[3];

            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            var plugins = container.GetAllInstances<IPlugin>() as ICollection<IPlugin>;

            // Act
            plugins.CopyTo(pluginCopies, 0);

            // Assert
            AssertThat.IsInstanceOfType(typeof(Plugin0), pluginCopies[0]);
            AssertThat.IsInstanceOfType(typeof(Plugin1), pluginCopies[1]);
            AssertThat.IsInstanceOfType(typeof(Plugin2), pluginCopies[2]);
        }

        [TestMethod]
        public void GetAllInstances_ByNonGenericRegistration_CanGetTheInstancesByIndex()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register(typeof(IPlugin), new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            // Act
            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Assert
            AssertThat.IsInstanceOfType(typeof(Plugin0), plugins[0]);
            AssertThat.IsInstanceOfType(typeof(Plugin1), plugins[1]);
            AssertThat.IsInstanceOfType(typeof(Plugin2), plugins[2]);
        }

        [TestMethod]
        public void GetAllInstances_Always_CanGetTheInstancesByIndex()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator),
                context => context.ImplementationType != typeof(Plugin2));

            // Act
            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Assert
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), plugins[0]);
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), plugins[1]);
            AssertThat.IsInstanceOfType(typeof(Plugin2), plugins[2]);
        }

        [TestMethod]
        public void Count_Always_ReturnsTheExpectedNumberOfElements1()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator),
                context => context.ImplementationType != typeof(Plugin2));

            var plugins = container.GetAllInstances<IPlugin>() as IList<IPlugin>;

            // Act
            int count = plugins.Count;

            // Assert
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void IEnumerableGetEnumerator_Always_ReturnsACorrectEnumerator()
        {
            // Arrange
            List<object> pluginsCopy = new List<object>();

            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(Plugin0), typeof(Plugin1), typeof(Plugin2) });

            var plugins = (IEnumerable)container.GetAllInstances<IPlugin>();

            // Act
            foreach (var plugin in plugins)
            {
                pluginsCopy.Add(plugin);
            }

            // Assert
            AssertThat.IsInstanceOfType(typeof(Plugin0), pluginsCopy[0]);
            AssertThat.IsInstanceOfType(typeof(Plugin1), pluginsCopy[1]);
            AssertThat.IsInstanceOfType(typeof(Plugin2), pluginsCopy[2]);
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