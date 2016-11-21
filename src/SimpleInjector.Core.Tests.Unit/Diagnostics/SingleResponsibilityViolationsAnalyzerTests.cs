namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics.Analyzers;
    using SimpleInjector.Diagnostics.Debugger;
    using SimpleInjector.Diagnostics.Tests.Unit.Helpers;
    using SimpleInjector.Tests.Unit;

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
        public void Analyze_OnConfigurationWithOneViolation_ReturnsSeverityWarning()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(typeof(PluginWith8Dependencies));

            container.Verify();

            // Act
            var result = Analyzer.Analyze(container).OfType<SingleResponsibilityViolationDiagnosticResult>().First();

            // Assert
            Assert.AreEqual(DiagnosticSeverity.Information, result.Severity);
        }

        [TestMethod]
        public void Analyze_OnValidConfiguration_ReturnsNull()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(typeof(PluginWith7Dependencies));

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
            container.Register(typeof(IGenericPlugin<>), typeof(GenericPluginWith6Dependencies<>));

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
            Container container = CreateContainerWithRegistrations(typeof(PluginWith8Dependencies));

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container).Value as DebuggerViewItem[];

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(typeof(PluginWith8Dependencies).Name, results[0].Name);
            Assert.AreEqual(typeof(PluginWith8Dependencies).Name + 
                " has 8 dependencies which might indicate a SRP violation.",
                results[0].Description);
        }

        [TestMethod]
        public void Analyze_OneViolationWithSuppressDiagnosticWarning_NoWarning()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(typeof(PluginWith8Dependencies));

            container.Verify();

            var registration = container.GetRegistration(typeof(PluginWith8Dependencies)).Registration;

            registration.SuppressDiagnosticWarning(DiagnosticType.SingleResponsibilityViolation);

            // Act
            var results = Analyzer.Analyze(container).OfType<SingleResponsibilityViolationDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_WithInvalidConfiguration_ReturnsResultsWithExpectedViolationInformation()
        {
            // Arrange
            var expectedImplementationTypeInformation = new DebuggerViewItem(
                name: "ImplementationType",
                description: typeof(PluginWith8Dependencies).Name,
                value: typeof(PluginWith8Dependencies));

            var expectedDependenciesInformation = new DebuggerViewItem(
                name: "Dependencies",
                description: "8 dependencies.",
                value: null);

            Container container = CreateContainerWithRegistrations(typeof(PluginWith8Dependencies));

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
                typeof(PluginWith8Dependencies),
                typeof(AnotherPluginWith8Dependencies));

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container).Value as DebuggerViewItem[];

            // Assert
            Assert.AreEqual(2, results.Length);

            var plugin1 = results.Single(r => r.Name == typeof(PluginWith8Dependencies).Name);

            Assert.AreEqual(
                typeof(PluginWith8Dependencies).Name + " has 8 dependencies which might indicate a SRP violation.",
                plugin1.Description);

            var plugin2 = results.Single(r => r.Name == typeof(AnotherPluginWith8Dependencies).Name);
            
            Assert.AreEqual(
                typeof(AnotherPluginWith8Dependencies).Name + 
                " has 8 dependencies which might indicate a SRP violation.",
                plugin2.Description);
        }

        [TestMethod]
        public void Analyze_ConfigurationWithTwoViolationsOnSingleService_ReturnsOneViewItemForBothViolations()
        {
            // Arrange
            Container container = CreateContainerWithRegistration<IPlugin, PluginWith8Dependencies>();

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith8Dependencies));
            
            container.Verify();

            var ip = container.GetRegistration(typeof(IPlugin));

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
            Container container = CreateContainerWithRegistration<IPlugin, PluginWith8Dependencies>();

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith8Dependencies));

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container).Value as DebuggerViewItem[];

            var items = results.Single().Value as DebuggerViewItem[];

            var item = items.Single(i => i.Description.Contains(typeof(PluginWith8Dependencies).Name));

            // Assert
            Assert.AreEqual("IPlugin", item.Name);
            Assert.AreEqual(typeof(PluginWith8Dependencies).Name + 
                " has 8 dependencies which might indicate a SRP violation.",
                item.Description);
        }

        [TestMethod]
        public void Analyze_ConfigurationWithTwoViolationsOnSingleService_ReturnsOneViewItemThatWrapsSecondViolation()
        {
            // Arrange
            Container container = CreateContainerWithRegistration<IPlugin, PluginWith8Dependencies>();

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith8Dependencies));

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container).Value as DebuggerViewItem[];

            var items = results.Single().Value as DebuggerViewItem[];

            var item = items.Single(i => i.Description.Contains(typeof(PluginDecoratorWith8Dependencies).Name));

            // Assert
            Assert.AreEqual("IPlugin", item.Name);
            Assert.AreEqual(typeof(PluginDecoratorWith8Dependencies).Name + 
                " has 8 dependencies which might indicate a SRP violation.",
                item.Description);
        }

        [TestMethod]
        public void Analyze_CollectionWithTypeWithTooManyDependencies_WarnsAboutThatViolation()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(Type.EmptyTypes);

            container.RegisterCollection<IPlugin>(new[] { typeof(PluginWith8Dependencies) });

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container).Value as DebuggerViewItem[];

            // Assert
            Assert.IsNotNull(results, "A warning should have been detected.");
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(
                typeof(PluginWith8Dependencies).Name + " has 8 dependencies which might indicate a SRP violation.",
                results[0].Description);
        }

        [TestMethod]
        public void Analyze_ConfigurationWithCollectionWithMultipleDecoratorsWithValidNumberOfDependencies_DoesNotWarnAboutThatDecorator()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(Type.EmptyTypes);

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith5Dependencies));
            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith5Dependencies));
            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith5Dependencies));

            // Non of these types have too many dependencies.
            container.RegisterCollection<IPlugin>(new[] { typeof(PluginImpl), typeof(SomePluginImpl) });

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

        [TestMethod]
        public void Analyze_ConfigurationWithCollectionADecoratorsWithTooManyDependencies_WarnsAboutThatDecorator()
        {
            // Arrange
            Container container = CreateContainerWithRegistrations(Type.EmptyTypes);

            // This decorator has too many dependencies
            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecoratorWith8Dependencies));

            // Non of these types have too many dependencies.
            container.RegisterCollection<IPlugin>(new[] { typeof(PluginImpl), typeof(SomePluginImpl) });

            container.Verify();

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container);

            // Assert
            // We expect two violations here, since the decorator is wrapping two different registrations.
            Assert.AreEqual("2 possible single responsibility violations.", results.Description);
        }

        private static Container CreateContainerWithRegistrations(params Type[] implementationTypes)
        {
            var container = new Container();

            container.Register(typeof(IGeneric<>), typeof(GenericType<>));

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

            container.Register(typeof(IGeneric<>), typeof(GenericType<>));

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

        private static string Actual(SingleResponsibilityViolationDiagnosticResult[] results) => 
            "actual: " + string.Join(" - ", results.Select(r => r.Description));
    }
}