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
    public class ContainerRegisteredServiceContainerAnalyzerTestsFull
    {
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