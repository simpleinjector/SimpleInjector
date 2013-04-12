namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class KeyedFactoryTests
    {
        public interface IPlugin
        {
        }

        [TestMethod]
        public void DefaultUseCase()
        {
            // Arrange
            var container = new Container();

            var factory = new KeyedFactory<string, IPlugin>(container, 
                StringComparer.OrdinalIgnoreCase);

            // Act
            using (var keyedRegistar = factory.BeginRegistrations())
            {
                keyedRegistar.Register<Plugin1>("foo");
                keyedRegistar.Register<Plugin2>("bar", Lifestyle.Singleton);
            }

            IPlugin foo1 = factory["foo"];
            IPlugin foo2 = factory["foo"];

            IPlugin bar1 = factory["bar"];
            IPlugin bar2 = factory["BAR"];

            // Assert
            Assert.IsInstanceOfType(foo1, typeof(Plugin1));
            Assert.IsFalse(object.ReferenceEquals(foo1, foo2), "foo should be transient.");
            
            Assert.IsInstanceOfType(bar1, typeof(Plugin2));
            Assert.IsTrue(object.ReferenceEquals(bar1, bar2), "bar should be singleton.");
        }

        public class Plugin1 : IPlugin
        {
        }

        public class Plugin2 : IPlugin
        {
        }
    }
}