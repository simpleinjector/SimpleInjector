namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    using SimpleInjector.Extensions;
    
    [TestClass]
    public class KeyedRegistrationsTests
    {
        public interface IPlugin 
        {
        }

        [TestMethod]
        public void Scenario1()
        {
            // Arrange
            var container = new Container();

            var plugins = new KeyedRegistrations<string, IPlugin>(container);

            plugins.Register(typeof(Plugin1), "1");
            plugins.Register<Plugin2>("2");
            plugins.Register(typeof(Plugin3), "3", Lifestyle.Singleton);
            plugins.Register(() => new Plugin("4"), "4");
            plugins.Register(() => new Plugin("5"), "5", Lifestyle.Singleton);

            container.RegisterAll<IPlugin>(plugins);

            container.RegisterSingle<Func<string, IPlugin>>(key => plugins.GetInstance(key));

            container.Verify();

            // Act
            var actualPlugins1 = container.GetAllInstances<IPlugin>().ToArray();
            var actualPlugins2 = container.GetAllInstances<IPlugin>().ToArray();
            var factory = container.GetInstance<Func<string, IPlugin>>();

            // Assert
            Assert.IsInstanceOfType(actualPlugins1[0], typeof(Plugin1));
            Assert.IsInstanceOfType(actualPlugins1[1], typeof(Plugin2));
            Assert.IsInstanceOfType(actualPlugins1[2], typeof(Plugin3));
            Assert.IsInstanceOfType(actualPlugins1[3], typeof(Plugin));
            Assert.IsInstanceOfType(actualPlugins1[4], typeof(Plugin));

            Assert.IsInstanceOfType(factory("1"), typeof(Plugin1));
            Assert.IsInstanceOfType(factory("2"), typeof(Plugin2));
            Assert.IsInstanceOfType(factory("3"), typeof(Plugin3));
            Assert.IsInstanceOfType(factory("4"), typeof(Plugin));
            Assert.IsInstanceOfType(factory("5"), typeof(Plugin));

            Assert.IsFalse(object.ReferenceEquals(actualPlugins1[0], actualPlugins2[0]));
            Assert.IsFalse(object.ReferenceEquals(actualPlugins1[1], actualPlugins2[1]));
            Assert.IsTrue(object.ReferenceEquals(actualPlugins1[2], actualPlugins2[2]));
            Assert.IsFalse(object.ReferenceEquals(actualPlugins1[3], actualPlugins2[3]));
            Assert.IsTrue(object.ReferenceEquals(actualPlugins1[4], actualPlugins2[4]));
        }

        public class Plugin1 : IPlugin 
        {
        }
        
        public class Plugin2 : IPlugin 
        {
        }
        
        public class Plugin3 : IPlugin 
        {
        }
        
        public class Plugin : IPlugin 
        {
            public Plugin(string a)
            {
            }
        }
        
        public class PluginDecorator : IPlugin
        {
            public PluginDecorator(IPlugin plugin)
            {
            }
        }
    }
}
