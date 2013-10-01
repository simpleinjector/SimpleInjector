namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;
    using SimpleInjector.Extensions;

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

            container.Options.ConstructorInjectionBehavior = new NamedConstructorInjectionBehavior(
                container.Options.ConstructorInjectionBehavior,
                (serviceType, name) => plugins.GetRegistration(name));

            plugins.Register(typeof(Plugin1), "1");
            plugins.Register<Plugin2>("2");
            plugins.Register(typeof(Plugin3), "3", Lifestyle.Singleton);
            plugins.Register(() => new Plugin("4"), "4");
            plugins.Register(() => new Plugin("5"), "5", Lifestyle.Singleton);

            container.RegisterAll<IPlugin>(plugins);

            container.RegisterSingleDecorator(typeof(IPlugin), typeof(PluginDecorator),
                context => context.ImplementationType == typeof(Plugin3));

            container.RegisterSingle<Func<string, IPlugin>>(key => plugins.GetInstance(key));

            container.Verify();

            // Act
            var actualPlugins1 = container.GetAllInstances<IPlugin>().ToArray();
            var actualPlugins2 = container.GetAllInstances<IPlugin>().ToArray();
            var factory = container.GetInstance<Func<string, IPlugin>>();
            var consumer = container.GetInstance<NamedPluginConsumer>();

            // Assert
            Assert.IsInstanceOfType(actualPlugins1[0], typeof(Plugin1));
            Assert.IsInstanceOfType(actualPlugins1[1], typeof(Plugin2));
            Assert.IsInstanceOfType(actualPlugins1[2], typeof(PluginDecorator));
            Assert.IsInstanceOfType(actualPlugins1[3], typeof(Plugin));
            Assert.IsInstanceOfType(actualPlugins1[4], typeof(Plugin));

            Assert.IsInstanceOfType(factory("1"), typeof(Plugin1));
            Assert.IsInstanceOfType(factory("2"), typeof(Plugin2));
            Assert.IsInstanceOfType(factory("3"), typeof(PluginDecorator));
            Assert.IsInstanceOfType(factory("4"), typeof(Plugin));
            Assert.IsInstanceOfType(factory("5"), typeof(Plugin));

            Assert.AreNotSame(actualPlugins1[0], actualPlugins2[0]);
            Assert.AreNotSame(actualPlugins1[1], actualPlugins2[1]);
            Assert.AreSame(actualPlugins1[2], actualPlugins2[2]);
            Assert.AreNotSame(actualPlugins1[3], actualPlugins2[3]);
            Assert.AreSame(actualPlugins1[4], actualPlugins2[4]);

            Assert.IsInstanceOfType(consumer.Plugin1, typeof(Plugin1));
            Assert.IsInstanceOfType(consumer.Plugin2, typeof(Plugin2));
            Assert.IsInstanceOfType(consumer.Plugin3, typeof(PluginDecorator));
            Assert.IsInstanceOfType(consumer.Plugin4, typeof(Plugin));
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

            public string Name { get; private set; }
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

            public IPlugin Plugin1 { get; private set; }

            public IPlugin Plugin2 { get; private set; }

            public IPlugin Plugin3 { get; private set; }

            public IPlugin Plugin4 { get; private set; }
        }

        public class NamedConstructorInjectionBehavior : IConstructorInjectionBehavior
        {
            private readonly IConstructorInjectionBehavior defaultBehavior;
            private readonly Func<Type, string, InstanceProducer> keyedProducerRetriever;

            public NamedConstructorInjectionBehavior(IConstructorInjectionBehavior defaultBehavior,
                Func<Type, string, InstanceProducer> keyedProducerRetriever)
            {
                this.defaultBehavior = defaultBehavior;
                this.keyedProducerRetriever = keyedProducerRetriever;
            }

            public Expression BuildParameterExpression(ParameterInfo parameter)
            {
                var attribute = parameter.GetCustomAttribute<NamedAttribute>();

                if (attribute != null)
                {
                    return this.keyedProducerRetriever(parameter.ParameterType, attribute.Name).BuildExpression();
                }

                return this.defaultBehavior.BuildParameterExpression(parameter);
            }
        }
    }
}