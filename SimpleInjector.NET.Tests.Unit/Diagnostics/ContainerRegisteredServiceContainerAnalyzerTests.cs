namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class ContainerRegisteredServiceContainerAnalyzerTests
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
        public void Analyze_ComponentDependingOnUnregisteredEnumerable_ReturnsWarning()
        {
            // Arrange
            var container = new Container();

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
        public void Analyze_ComponentDependingOnUnregisteredReadOnlyCollection_ReturnsWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<Consumer<IReadOnlyCollection<ILogger>>>();

            container.Verify();

            // Act
            var results = 
                Analyzer.Analyze(container).OfType<ContainerRegisteredServiceDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(
                "Consumer<IReadOnlyCollection<ILogger>> depends on container-registered type IReadOnlyCollection<ILogger>.",
                results.Single().Description);
        }

        [TestMethod]
        public void Analyze_ComponentDependingOnUnregisteredReadOnlyList_ReturnsWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<Consumer<IReadOnlyList<ILogger>>>();

            container.Verify();

            // Act
            var results = 
                Analyzer.Analyze(container).OfType<ContainerRegisteredServiceDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(
                "Consumer<IReadOnlyList<ILogger>> depends on container-registered type IReadOnlyList<ILogger>.",
                results.Single().Description);
        }

        [TestMethod]
        public void Analyze_ComponentDependingOnUnregisteredICollection_ReturnsWarning()
        {
            // Arrange
            var container = new Container();

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

        [TestMethod]
        public void Analyze_ComponentDependingOnReadOnlyCollectionForRegisteredCollection_DoesNotReturnsAWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<Consumer<IReadOnlyCollection<ILogger>>>();

            // Since this collection is registered, the previous registration should not yield a warning.
            container.RegisterCollection<ILogger>(new[] { typeof(NullLogger) });
            
            container.Verify();

            // Act
            var results = 
                Analyzer.Analyze(container).OfType<ContainerRegisteredServiceDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, "actual: " + string.Join(" - ", results.Select(r => r.Description)));
        }

        [TestMethod]
        public void Analyze_ComponentDependingOnReadOnlyListForRegisteredCollection_DoesNotReturnsAWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<Consumer<IReadOnlyList<ILogger>>>();

            // Since this collection is registered, the previous registration should not yield a warning.
            container.RegisterCollection<ILogger>(new[] { typeof(NullLogger) });

            container.Verify();

            // Act
            var results =
                Analyzer.Analyze(container).OfType<ContainerRegisteredServiceDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, "actual: " + string.Join(" - ", results.Select(r => r.Description)));
        }
    }
}