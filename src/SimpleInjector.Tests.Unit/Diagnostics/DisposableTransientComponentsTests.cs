namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using System.Linq;
    using Lifestyles;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class RegistrationExtensions
    {
        public static void SuppressDiagnosticWarning(this Registration registration, DiagnosticType type)
        {
            registration.SuppressDiagnosticWarning(type, "Some random justification that we don't care about.");
        }
    }

    [TestClass]
    public class DisposableTransientComponentsTests
    {
        [TestMethod]
        public void Analyze_TransientRegistrationForDisposableComponent_Warning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, DisposablePlugin>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(1, results.Length, Actual(results));
            Assert.IsTrue(
                results.Single().Description.StartsWith(
                    "DisposablePlugin is registered as transient, but implements IDisposable."),
                Actual(results));
        }

        [TestMethod]
        public void Analyze_TransientRegistrationForAsyncDisposableComponent_Warning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, AsyncDisposablePlugin>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(1, results.Length, Actual(results));
            Assert.IsTrue(
                results.Single().Description.StartsWith(
                    "AsyncDisposablePlugin is registered as transient, but implements IAsyncDisposable."),
                Actual(results));
        }

        [TestMethod]
        public void Analyze_TransientRegistrationForComponentImplementingBothIDisposableAndIAsyncDisposable_Warning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, DisposableAsyncDisposablePlugin>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(1, results.Length, Actual(results));
            Assert.IsTrue(
                results.Single().Description.StartsWith(
                    "DisposableAsyncDisposablePlugin is registered as transient, but implements IDisposable."),
                Actual(results));
        }

        [TestMethod]
        public void Analyze_TransientRegistrationForDisposableComponent_ReturnsSeverityWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, DisposablePlugin>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var result = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>().First();

            // Assert
            Assert.AreEqual(DiagnosticSeverity.Warning, result.Severity);
        }

        [TestMethod]
        public void Analyze_TransientRegistrationForDisposableComponentWithSuppressDiagnosticWarning_NoWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, DisposablePlugin>();

            Registration registration = container.GetRegistration(typeof(IPlugin)).Registration;

            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent);

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_SingletonRegistrationForDisposableComponent_NoWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, DisposablePlugin>(Lifestyle.Singleton);

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_ScopedRegistrationForDisposableComponent_NoWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, DisposablePlugin>(new ThreadScopedLifestyle());

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_TransientRegistrationForComponentThatsNotDisposable_NoWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, SomePluginImpl>();

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        // #864
        [TestMethod]
        public void Analyze_TransientSelfRegisteredDisposableComponent_DoesNotReportItsRegisteredServices()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposablePlugin, DisposablePlugin>();

            // Act
            string description = GetSingleDisposableTransientComponentDiagnosticDescription(container);

            // Assert
            Assert.IsTrue(
                description.Equals("DisposablePlugin is registered as transient, but implements IDisposable."),
                message: "Actual: " + description);
        }

        // #864
        [TestMethod]
        public void Analyze_TransientDisposableComponentRegisteredByService_ReportsTheUsedServiceType()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, DisposablePlugin>();

            // Act
            string description = GetSingleDisposableTransientComponentDiagnosticDescription(container);

            // Assert
            Assert.IsTrue(
                description.Contains(" DisposablePlugin was registered for service IPlugin."),
                message: "Actual: " + description);
        }

        // #864
        [TestMethod]
        public void Analyze_TransientDisposableComponentRegisteredByMultipleServices_ReportsAllServices()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, AsyncDisposablePlugin>();
            container.Register<IAsyncDisposable, AsyncDisposablePlugin>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            string description =
                Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .First()
                .Description;

            // Assert
            Assert.IsTrue(
                description.Contains(
                    " AsyncDisposablePlugin was registered for services IPlugin and IAsyncDisposable."),
                message: "Actual: " + description);
        }

        private static string Actual(DisposableTransientComponentDiagnosticResult[] results) =>
            "actual: " + string.Join(" - ", results.Select(r => r.Description));

        private static string GetSingleDisposableTransientComponentDiagnosticDescription(Container c)
        {
            c.Verify(VerificationOption.VerifyOnly);
            return Analyzer.Analyze(c).OfType<DisposableTransientComponentDiagnosticResult>()
                .Single()
                .Description;
        }
    }
}