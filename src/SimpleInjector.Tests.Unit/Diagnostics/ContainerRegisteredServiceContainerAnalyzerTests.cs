#pragma warning disable 0618
namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
#pragma warning restore 0618