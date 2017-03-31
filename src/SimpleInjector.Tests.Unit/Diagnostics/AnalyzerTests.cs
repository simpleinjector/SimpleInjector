namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class AnalyzerTests
    {
        [TestMethod]
        public void Analyze_WithNullArgument_ThrowsExpectedException()
        {
            // Act
            Action action = () => Analyzer.Analyze(null);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void Analyze_SuppliedWithUnverifiedContainer_ThrowsExpectedException()
        {
            // Arrange
            var unverfiedContainer = new Container();

            // Act
            Action action = () => Analyzer.Analyze(unverfiedContainer);

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
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