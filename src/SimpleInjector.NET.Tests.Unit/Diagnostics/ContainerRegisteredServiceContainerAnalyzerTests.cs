namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    /// <summary>
    /// Normal tests.
    /// </summary>
    [TestClass]
    public partial class ContainerRegisteredServiceContainerAnalyzerTests
    {
        [TestMethod]
        public void Analyze_ComponentDependingOnUnregisteredConcreteType_ReturnsWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<Consumer<ConcreteShizzle>>();

            container.Verify();
            
            // Act
            var results = 
                Analyzer.Analyze(container).OfType<ContainerRegisteredServiceDiagnosticResult>().ToArray();
            
            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(
                "Consumer<ConcreteShizzle> depends on container-registered type ConcreteShizzle.",
                results.Single().Description);
        }

        [TestMethod]
        public void Analyze_ComponentDependingOnUnregisteredConcreteType_ReturnsSeverityInformation()
        {
            // Arrange
            var container = new Container();

            container.Register<Consumer<ConcreteShizzle>>();

            container.Verify();

            // Act
            var result = Analyzer.Analyze(container).OfType<ContainerRegisteredServiceDiagnosticResult>().First();

            // Assert
            Assert.AreEqual(DiagnosticSeverity.Information, result.Severity);
        }

        [TestMethod]
        public void Analyze_ComponentDependingOnUnregisteredEnumerable_ReturnsWarning()
        {
            // Arrange
            var container = new Container();

            container.Options.ResolveUnregisteredCollections = true;

            container.Register<Consumer<IEnumerable<ILogger>>>();

            container.Verify();

            // Act
            var results = 
                Analyzer.Analyze(container).OfType<ContainerRegisteredServiceDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(
                "Consumer<IEnumerable<ILogger>> depends on container-registered type IEnumerable<ILogger>.",
                results.Single().Description,
                "It's important for this warning to be signaled, because developers sometimes misconfigure " +
                "the container in a way that their registered collection doesn't get injected, because " +
                "they accidentally depend on an other type. Simple Injector will in that case silently " +
                "inject an empty collection.");
        }

        [TestMethod]
        public void Analyze_ComponentDependingOnUnregisteredICollection_ReturnsWarning()
        {
            // Arrange
            var container = new Container();

            container.Options.ResolveUnregisteredCollections = true;

            container.Register<Consumer<ICollection<ILogger>>>();

            container.Verify();

            // Act
            var results =
                Analyzer.Analyze(container).OfType<ContainerRegisteredServiceDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(
                "Consumer<ICollection<ILogger>> depends on container-registered type ICollection<ILogger>.",
                results.Single().Description);
        }

        [TestMethod]
        public void Analyze_ComponentDependingOnUnregisteredIList_ReturnsWarning()
        {
            // Arrange
            var container = new Container();

            container.Options.ResolveUnregisteredCollections = true;

            container.Register<Consumer<IList<ILogger>>>();

            container.Verify();

            // Act
            var results =
                Analyzer.Analyze(container).OfType<ContainerRegisteredServiceDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(
                "Consumer<IList<ILogger>> depends on container-registered type IList<ILogger>.",
                results.Single().Description);
        }
    }
}