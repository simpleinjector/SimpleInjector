#if DEBUG
namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Diagnostics.Analyzers;
    using SimpleInjector.Diagnostics.Debugger;
    using SimpleInjector.Extensions;

    public interface IGenericPlugin<T> 
    {
    }

    [TestClass]
    public class SingleResponsibilityViolationsAnalyzerTests
    {
        [TestMethod]
        public void Analyze_OnEmptyConfiguration_ReturnsNull()
        {
            // Arrange
            var container = new Container();

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container);

            // Assert
            Assert.AreEqual("No warnings detected.", results.Description);
        }

        [TestMethod]
        public void Analyze_OnValidConfiguration_ReturnsNull()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(typeof(PluginWith6Dependencies));

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container);

            // Assert
            Assert.AreEqual("No warnings detected.", results.Description,
                "6 dependencies is still considered valid (to prevent too many false positives).");
        }

        [TestMethod]
        public void Analyze_OpenGenericRegistrationWithValidAmountOfDependencies_ReturnsNull()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations();

            // Consumer class contains a IGenericPlugin<IDisposable> dependency
            container.Register<Consumer<IGenericPlugin<IDisposable>>>();

            // Register open generic type with 6 dependencies.
            container.RegisterOpenGeneric(typeof(IGenericPlugin<>), typeof(GenericPluginWith6Dependencies<>));

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container);

            // Assert
            Assert.AreEqual("No warnings detected.", results.Description,
                "The registration is considered to be valid, since both the type and decorator do not " +
                "exceed the maximum number of dependencies. Message: {0}",
                results == null ? null : results.Items().FirstOrDefault());
        }

        [TestMethod]
        public void Analyze_OnConfigurationWithOneViolation_ReturnsThatViolation()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(typeof(PluginWith7Dependencies));

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container).Value as DebuggerViewItem[];

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(typeof(PluginWith7Dependencies).Name, results[0].Name);
            Assert.AreEqual(typeof(PluginWith7Dependencies).Name + 
                " has 7 dependencies which might indicate a SRP violation.",
                results[0].Description);
        }

        [TestMethod]
        public void Analyze_WithInvalidConfiguration_ReturnsResultsWithExpectedViolationInformation()
        {
            // Arrange
            var expectedImplementationTypeInformation = new DebuggerViewItem(
                name: "ImplementationType",
                description: typeof(PluginWith7Dependencies).Name,
                value: typeof(PluginWith7Dependencies));

            var expectedDependenciesInformation = new DebuggerViewItem(
                name: "Dependencies",
                description: "7 dependencies.",
                value: null);

            Container container = CreateContainerWithRegistrations(typeof(PluginWith7Dependencies));

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container).Value as DebuggerViewItem[];

            var result = results.Single();

            var violationInformation = result.Value as DebuggerViewItem[];

            // Assert
            Assert_AreEqual(expectedImplementationTypeInformation, violationInformation[0]);
            Assert_AreEqual(expectedDependenciesInformation, violationInformation[1], validateValue: false);
        }
        
        [TestMethod]
        public void Analyze_OnConfigurationWithMultipleViolations_ReturnsThoseTwoViolations()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(
                typeof(PluginWith7Dependencies),
                typeof(AnotherPluginWith7Dependencies));

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container).Value as DebuggerViewItem[];

            // Assert
            Assert.AreEqual(2, results.Length);

            var plugin1 = results.Single(r => r.Name == typeof(PluginWith7Dependencies).Name);

            Assert.AreEqual(
                typeof(PluginWith7Dependencies).Name + " has 7 dependencies which might indicate a SRP violation.",
                plugin1.Description);

            var plugin2 = results.Single(r => r.Name == typeof(AnotherPluginWith7Dependencies).Name);
            
            Assert.AreEqual(
                typeof(AnotherPluginWith7Dependencies).Name + 
                " has 7 dependencies which might indicate a SRP violation.",
                plugin2.Description);
        }

        [TestMethod]
        public void Analyze_ConfigurationWithTwoViolationsOnSingleService_ReturnsOneViewItemForBothViolations()
        {
            // Arrange
            Container container = CreateContainerWithRegistration<IPlugin, PluginWith7Dependencies>();

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith7Dependencies));
            
            container.Verify();

            // Act
            var items = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container).Value as DebuggerViewItem[];

            // Assert
            Assert.AreEqual(1, items.Length);

            Assert.AreEqual(typeof(IPlugin).Name, items[0].Name);
            Assert.AreEqual("2 possible violations.", items[0].Description);
        }

        [TestMethod]
        public void Analyze_ConfigurationWithTwoViolationsOnSingleService_ReturnsOneViewItemThatWrapsFirstViolation()
        {
            // Arrange
            Container container = CreateContainerWithRegistration<IPlugin, PluginWith7Dependencies>();

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith7Dependencies));

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container).Value as DebuggerViewItem[];

            var items = results.Single().Value as DebuggerViewItem[];

            var item = items.Single(i => i.Description.Contains(typeof(PluginWith7Dependencies).Name));

            // Assert
            Assert.AreEqual("IPlugin", item.Name);
            Assert.AreEqual(typeof(PluginWith7Dependencies).Name + 
                " has 7 dependencies which might indicate a SRP violation.",
                item.Description);
        }

        [TestMethod]
        public void Analyze_ConfigurationWithTwoViolationsOnSingleService_ReturnsOneViewItemThatWrapsSecondViolation()
        {
            // Arrange
            Container container = CreateContainerWithRegistration<IPlugin, PluginWith7Dependencies>();

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith7Dependencies));

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container).Value as DebuggerViewItem[];

            var items = results.Single().Value as DebuggerViewItem[];

            var item = items.Single(i => i.Description.Contains(typeof(PluginDecoratorWith7Dependencies).Name));

            // Assert
            Assert.AreEqual("IPlugin", item.Name);
            Assert.AreEqual(typeof(PluginDecoratorWith7Dependencies).Name + 
                " has 7 dependencies which might indicate a SRP violation.",
                item.Description);
        }

        [TestMethod]
        public void Analyze_CollectionWithTypeWithTooManyDependencies_WarnsAboutThatViolation()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(Type.EmptyTypes);

            container.RegisterAll<IPlugin>(typeof(PluginWith7Dependencies));

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container).Value as DebuggerViewItem[];

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(typeof(PluginWith7Dependencies).Name, results[0].Name);
            Assert.AreEqual(
                typeof(PluginWith7Dependencies).Name + " has 7 dependencies which might indicate a SRP violation.",
                results[0].Description);
        }

        [TestMethod]
        public void Analyze_ConfigurationWithCollectionWithDecoratorWithTooManyDependencies_DoesNotWarnAboutThatDecorator()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(Type.EmptyTypes);

            // This decorator has too many dependencies
            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith7Dependencies));

            // Non of these types have too many dependencies.
            container.RegisterAll<IPlugin>(typeof(PluginImpl), typeof(SomePluginImpl));

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container);

            // Assert
            Assert.AreEqual("No warnings detected.", results.Description, @"
                Although the decorator has too many dependencies, the system has not enough information to
                differentiate between a decorator with too many dependencies and a decorator that wraps many
                elements. The diagnostic system simply registers all dependencies that this decorator has,
                and all elements it decorates are a dependency and its hard to see the real number of
                dependencies it has. Because of this, we have to suppress violations on collections
                completely.");
        }

        private static Container CreateContainerWithRegistrations(params Type[] implementationTypes)
        {
            var container = new Container();

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
            var container = new Container();

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

    public class PluginWith6Dependencies : IPlugin
    {
        public PluginWith6Dependencies(
            IGeneric<int> dependency1,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5,
            IGeneric<decimal> dependency6)
        {
        }
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

    public class AnotherPluginWith7Dependencies : IPlugin
    {
        public AnotherPluginWith7Dependencies(
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

    public class PluginDecoratorWith5Dependencies : IPlugin
    {
        public PluginDecoratorWith5Dependencies(
            IPlugin decoratee,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5)
        {
        }
    }

    public class PluginDecoratorWith7Dependencies : IPlugin
    {
        public PluginDecoratorWith7Dependencies(
            IPlugin decoratee,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5,
            IGeneric<decimal> dependency6,
            IGeneric<int?> dependency7)
        {
        }
    }

    public class Consumer<TDependency>
    {
        public Consumer(TDependency dependency)
        {
        }
    }

    public class GenericPluginWith6Dependencies<T> : IGenericPlugin<T>
    {
        public GenericPluginWith6Dependencies(
            IGeneric<int> dependency1,
            IGeneric<byte> dependency2,
            IGeneric<double> dependency3,
            IGeneric<float> dependency4,
            IGeneric<char> dependency5,
            IGeneric<decimal> dependency6)
        {
        }
    }
}
#endif