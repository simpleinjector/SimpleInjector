#if DEBUG
namespace SimpleInjector.Tests.Unit.Diagnostics
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Extensions;

    [TestClass]
    public class SingleResponsibilityViolationsAnalyzerTests
    {
        [TestMethod]
        public void Analyze_OnEmptyConfiguration_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Verify();

            var analyzer = new SingleResponsibilityViolationsAnalyzer();

            // Act
            var results = analyzer.Analyze(container);

            // Assert
            Assert.IsNull(results);
        }

        [TestMethod]
        public void Analyze_OnValidConfiguration_ReturnsNull()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(typeof(PluginWith7Dependencies));

            container.Verify();

            var analyzer = new SingleResponsibilityViolationsAnalyzer();

            // Act
            var results = analyzer.Analyze(container);

            // Assert
            Assert.IsNull(results, 
                "7 dependencies is still considered valid (to prevent too many false positives).");
        }

        [TestMethod]
        public void Analyze_OnConfigurationWithOneViolation_ReturnsThatViolation()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(typeof(PluginWith8Dependencies));

            container.Verify();

            var analyzer = new SingleResponsibilityViolationsAnalyzer();

            // Act
            var results = analyzer.Analyze(container).Value as DebuggerViewItem[];

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("PluginWith8Dependencies", results[0].Name);
            Assert.AreEqual("PluginWith8Dependencies has 8 dependencies which might indicate a SRP violation.",
                results[0].Description);
        }

        [TestMethod]
        public void Analyze_Scenario_Behavior1()
        {
            // Arrange
            var expectedImplementationTypeInformation = new DebuggerViewItem(
                name: "ImplementationType",
                description: "PluginWith8Dependencies",
                value: typeof(PluginWith8Dependencies));

            var expectedDependenciesInformation = new DebuggerViewItem(
                name: "Dependencies",
                description: "8 dependencies.",
                value: null);

            Container container = CreateContainerWithRegistrations(typeof(PluginWith8Dependencies));

            container.Verify();

            var analyzer = new SingleResponsibilityViolationsAnalyzer();

            // Act
            var results = analyzer.Analyze(container).Value as DebuggerViewItem[];

            var result = results.Single();

            var violation = (result.Value as DebuggerViewItem[]).Single();

            var violationInformation = violation.Value as DebuggerViewItem[];

            // Assert
            Assert_AreEqual(expectedImplementationTypeInformation, violationInformation[0]);
            Assert_AreEqual(expectedDependenciesInformation, violationInformation[1], validateValue: false);
        }
        
        [TestMethod]
        public void Analyze_OnConfigurationWithMultipleViolations_ReturnsThoseTwoViolations()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(
                typeof(PluginWith8Dependencies),
                typeof(AnotherPluginWith8Dependencies));

            container.Verify();

            var analyzer = new SingleResponsibilityViolationsAnalyzer();

            // Act
            var results = analyzer.Analyze(container).Value as DebuggerViewItem[];

            // Assert
            Assert.AreEqual(2, results.Length);

            var plugin1 = results.Single(r => r.Name == "PluginWith8Dependencies");

            Assert.AreEqual(
                "PluginWith8Dependencies has 8 dependencies which might indicate a SRP violation.",
                plugin1.Description);

            var plugin2 = results.Single(r => r.Name == "AnotherPluginWith8Dependencies");
            
            Assert.AreEqual(
                "AnotherPluginWith8Dependencies has 8 dependencies which might indicate a SRP violation.",
                plugin2.Description);
        }

        [TestMethod]
        public void Analyze_ConfigurationWithTwoViolationsOnSingleService_ReturnsOneViewItemForBothViolations()
        {
            // Arrange
            Container container = CreateContainerWithRegistration<IPlugin, PluginWith8Dependencies>();

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith8Dependencies));
            
            container.Verify();

            var analyzer = new SingleResponsibilityViolationsAnalyzer();

            // Act
            var items = analyzer.Analyze(container).Value as DebuggerViewItem[];

            // Assert
            Assert.AreEqual(1, items.Length);

            Assert.AreEqual("IPlugin", items[0].Name);
            Assert.AreEqual("2 possible violations.", items[0].Description);
        }

        [TestMethod]
        public void Analyze_ConfigurationWithTwoViolationsOnSingleService_ReturnsOneViewItemThatWrapsFirstViolation()
        {
            // Arrange
            Container container = CreateContainerWithRegistration<IPlugin, PluginWith8Dependencies>();

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith8Dependencies));

            container.Verify();

            var analyzer = new SingleResponsibilityViolationsAnalyzer();

            // Act
            var results = analyzer.Analyze(container).Value as DebuggerViewItem[];

            var items = results.Single().Value as DebuggerViewItem[];

            var item = items.Single(i => i.Description.Contains("PluginWith8Dependencies"));

            // Assert
            Assert.AreEqual("Violation", item.Name);
            Assert.AreEqual("PluginWith8Dependencies has 8 dependencies which might indicate a SRP violation.",
                item.Description);
        }

        [TestMethod]
        public void Analyze_ConfigurationWithTwoViolationsOnSingleService_ReturnsOneViewItemThatWrapsSecondViolation()
        {
            // Arrange
            Container container = CreateContainerWithRegistration<IPlugin, PluginWith8Dependencies>();

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith8Dependencies));

            container.Verify();

            var analyzer = new SingleResponsibilityViolationsAnalyzer();

            // Act
            var results = analyzer.Analyze(container).Value as DebuggerViewItem[];

            var items = results.Single().Value as DebuggerViewItem[];

            var item = items.Single(i => i.Description.Contains("PluginDecoratorWith8Dependencies"));

            // Assert
            Assert.AreEqual("Violation", item.Name);
            Assert.AreEqual("PluginDecoratorWith8Dependencies has 8 dependencies which might indicate a SRP violation.",
                item.Description);
        }

        [TestMethod]
        public void Analyze_CollectionWithTypeWithTooManyDependencies_WarnsAboutThatViolation()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(Type.EmptyTypes);

            container.RegisterAll<IPlugin>(typeof(PluginWith8Dependencies));

            container.Verify();

            var analyzer = new SingleResponsibilityViolationsAnalyzer();

            // Act
            var results = analyzer.Analyze(container).Value as DebuggerViewItem[];

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("PluginWith8Dependencies", results[0].Name);
            Assert.AreEqual(
                "PluginWith8Dependencies has 8 dependencies which might indicate a SRP violation.",
                results[0].Description);
        }

        [TestMethod]
        public void Analyze_ConfigurationWithCollectionWithDecoratorWithTooManyDependencies_DoesNotWarnAboutThatDecorator()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(Type.EmptyTypes);

            // This decorator has too many dependencies
            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith8Dependencies));

            // Non of these types have too many dependencies.
            container.RegisterAll<IPlugin>(typeof(PluginImpl), typeof(SomePluginImpl));

            container.Verify();

            var analyzer = new SingleResponsibilityViolationsAnalyzer();

            // Act
            var results = analyzer.Analyze(container);

            // Assert
            Assert.IsNull(results, @"
                Although the decorator has too many dependencies, the system has not enough information to
                differentiate between a decorator with too many dependencies and a decorator that wraps many
                elements. The diagnostic system simply registers all dependencies that this decorator has,
                and all elements it decorates are a dependency and its hard to see the real number of
                dependencies it has. Because of this, we have to suppress violations on collections
                completely.");
        }

        private static Container CreateContainerWithRegistrations(params Type[] implementationTypes)
        {
            var container = ContainerFactory.New();

            container.RegisterOpenGeneric(typeof(IGeneric<>), typeof(GenericType<>));

            foreach (var type in implementationTypes)
            {
                container.Register(type);
            }

            return container;
        }

        private static Container CreateContainerWithRegistration<TService, TImplementation>()
            where TService : class, IPlugin
            where TImplementation : class, TService
        {
            var container = ContainerFactory.New();

            container.RegisterOpenGeneric(typeof(IGeneric<>), typeof(GenericType<>));

            container.Register<TService, TImplementation>();

            return container;
        }
        
        private static void Assert_AreEqual(DebuggerViewItem expected, DebuggerViewItem actual,
            bool validateValue = true)
        {
            Assert.AreEqual(expected.Name, actual.Name, "Names do not match");
            Assert.AreEqual(expected.Description, actual.Description, "Descriptions do not match");

            if (validateValue)
            {
                Assert.AreEqual(expected.Value, actual.Value, "Values do not match");
            }
        }
    }
    
    public class SomePluginImpl : IPlugin
    {
    }

    public class PluginWith7Dependencies : IPlugin
    {
        public PluginWith7Dependencies(
            IGeneric<int> dependency1,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5,
            IGeneric<decimal> dependency6,
            IGeneric<int?> dependency7)
        {
        }
    }

    public class PluginWith8Dependencies : IPlugin
    {
        public PluginWith8Dependencies(
            IGeneric<int> dependency1,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5,
            IGeneric<decimal> dependency6,
            IGeneric<int?> dependency7,
            IGeneric<byte?> dependency8)
        {
        }
    }

    public class AnotherPluginWith8Dependencies : IPlugin
    {
        public AnotherPluginWith8Dependencies(
            IGeneric<int> dependency1,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5,
            IGeneric<decimal> dependency6,
            IGeneric<int?> dependency7,
            IGeneric<byte?> dependency8)
        {
        }
    }

    public class PluginDecoratorWith6Dependencies : IPlugin
    {
        public PluginDecoratorWith6Dependencies(
            IPlugin decoratee,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5,
            IGeneric<decimal> dependency6)
        {
        }
    }

    public class PluginDecoratorWith8Dependencies : IPlugin
    {
        public PluginDecoratorWith8Dependencies(
            IPlugin decoratee,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5,
            IGeneric<decimal> dependency6,
            IGeneric<int?> dependency7,
            IGeneric<byte?> dependency8)
        {
        }
    }
}
#endif