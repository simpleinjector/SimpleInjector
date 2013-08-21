namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AnalyzerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Analyze_WithNullArgument_ThrowsExpectedException()
        {
            Analyzer.Analyze(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Analyze_SuppliedWithUnverifiedContainer_ThrowsExpectedException()
        {
            // Arrange
            var unverfiedContainer = new Container();

            // Act
            Analyzer.Analyze(unverfiedContainer);
        }

        [TestMethod]
        public void Analyze_WithVerfiedContainer_Succeeds()
        {
            // Arrange
            var verfiedContainer = new Container();

            verfiedContainer.Verify();

            // Act
            Analyzer.Analyze(verfiedContainer);
        }
    }
}
