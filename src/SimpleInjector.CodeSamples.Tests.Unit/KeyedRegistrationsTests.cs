namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class KeyedRegistrationsTests
    {
        public interface IPlugin
        {
        }

        // Okay, I admit. I was too lazy to write a set of good unit tests. This is just one big test that
        // show cases the KeyedRegistrations.
        [TestMethod]
        public void Scenario1()
        {
            // Arrange
            var container = new Container();

            var plugins = new KeyedRegistrations<string, IPlugin>(container);

            container.Options.DependencyInjectionBehavior = new NamedDependencyInjectionBehavior(
                container.Options.DependencyInjectionBehavior,
                (serviceType, name) => plugins.GetRegistration(name));

            plugins.Register(typeof(Plugin1), "1");
            plugins.Register<Plugin2>("2");
            plugins.Register(typeof(Plugin3), "3", Lifestyle.Singleton);
            plugins.Register(() => new Plugin("4"), "4");
            plugins.Register(() => new Plugin("5"), "5", Lifestyle.Singleton);

            container.RegisterCollection<IPlugin>(plugins);

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator), Lifestyle.Singleton,
                context => context.ImplementationType == typeof(Plugin3));

            container.RegisterSingleton<Func<string, IPlugin>>(key => plugins.GetInstance(key));

            container.Verify();

            // Act
            var actualPlugins1 = container.GetAllInstances<IPlugin>().ToArray();
            var actualPlugins2 = container.GetAllInstances<IPlugin>().ToArray();
            var factory = container.GetInstance<Func<string, IPlugin>>();
            var consumer = container.GetInstance<NamedPluginConsumer>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Plugin1), actualPlugins1[0]);
            AssertThat.IsInstanceOfType(typeof(Plugin2), actualPlugins1[1]);
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), actualPlugins1[2]);
            AssertThat.IsInstanceOfType(typeof(Plugin), actualPlugins1[3]);
            AssertThat.IsInstanceOfType(typeof(Plugin), actualPlugins1[4]);

            AssertThat.IsInstanceOfType(typeof(Plugin1), factory("1"));
            AssertThat.IsInstanceOfType(typeof(Plugin2), factory("2"));
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), factory("3"));
            AssertThat.IsInstanceOfType(typeof(Plugin), factory("4"));
            AssertThat.IsInstanceOfType(typeof(Plugin), factory("5"));

            Assert.AreNotSame(actualPlugins1[0], actualPlugins2[0]);
            Assert.AreNotSame(actualPlugins1[1], actualPlugins2[1]);
            Assert.AreSame(actualPlugins1[2], actualPlugins2[2]);
            Assert.AreNotSame(actualPlugins1[3], actualPlugins2[3]);
            Assert.AreSame(actualPlugins1[4], actualPlugins2[4]);

            AssertThat.IsInstanceOfType(typeof(Plugin1), consumer.Plugin1);
            AssertThat.IsInstanceOfType(typeof(Plugin2), consumer.Plugin2);
            AssertThat.IsInstanceOfType(typeof(PluginDecorator), consumer.Plugin3);
            AssertThat.IsInstanceOfType(typeof(Plugin), consumer.Plugin4);
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

        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
        public class NamedAttribute : Attribute
        {
            public NamedAttribute(string name)
            {
                this.Name = name;
            }

            public string Name { get; }
        }

        public class NamedPluginConsumer
        {
            public NamedPluginConsumer(
                [Named("1")] IPlugin plugin1,
                [Named("2")] IPlugin plugin2,
                [Named("3")] IPlugin plugin3,
                [Named("4")] IPlugin plugin4)
            {
                this.Plugin1 = plugin1;
                this.Plugin2 = plugin2;
                this.Plugin3 = plugin3;
                this.Plugin4 = plugin4;
            }

            public IPlugin Plugin1 { get; }

            public IPlugin Plugin2 { get; }

            public IPlugin Plugin3 { get; }

            public IPlugin Plugin4 { get; }
        }

        public class NamedDependencyInjectionBehavior : IDependencyInjectionBehavior
        {
            private readonly IDependencyInjectionBehavior defaultBehavior;
            private readonly Func<Type, string, InstanceProducer> keyedProducerRetriever;

            public NamedDependencyInjectionBehavior(IDependencyInjectionBehavior defaultBehavior,
                Func<Type, string, InstanceProducer> keyedProducerRetriever)
            {
                this.defaultBehavior = defaultBehavior;
                this.keyedProducerRetriever = keyedProducerRetriever;
            }

            public InstanceProducer GetInstanceProducer(InjectionConsumerInfo consumer, bool throwOnFailure)
            {
                var attribute = consumer.Target.GetCustomAttribute<NamedAttribute>();

                if (attribute != null)
                {
                    return this.keyedProducerRetriever(consumer.Target.TargetType, attribute.Name);
                }

                return this.defaultBehavior.GetInstanceProducer(consumer, throwOnFailure);
            }

            public void Verify(InjectionConsumerInfo consumer)
            {
                this.defaultBehavior.Verify(consumer);
            }
        }
    }
}