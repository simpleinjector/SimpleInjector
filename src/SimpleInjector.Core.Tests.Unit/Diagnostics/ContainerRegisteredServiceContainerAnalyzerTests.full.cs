#pragma warning disable 0618
namespace SimpleInjector.Tests.Unit.Diagnostics
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Diagnostics.Tests.Unit;
    using SimpleInjector.Tests.Unit;

    /// <summary>
    /// Tests for the full .NET framework.
    /// </summary>
    [TestClass]
    public partial class ContainerRegisteredServiceContainerAnalyzerTests
    {
        [TestMethod]
        public void Analyze_ComponentDependingOnUnregisteredReadOnlyCollection_ReturnsWarning()
        {
            // Arrange
            var container = new Container();

            container.Options.ResolveUnregisteredCollections = true;

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

            container.Options.ResolveUnregisteredCollections = true;

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
#pragma warning restore 0618