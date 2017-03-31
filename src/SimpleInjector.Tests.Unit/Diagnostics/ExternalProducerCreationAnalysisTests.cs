namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExternalProducerCreationAnalysisTests
    {
        [TestMethod]
        public void Analyze_ProducerGenericWithLifestyleMismatch_ProducesALifestyleMismatchWarning()
        {
            // Arrange
            Func<Container, InstanceProducer> createLifestyleMismatch = container =>
                Lifestyle.Singleton.CreateProducer<RealUserService, RealUserService>(container);

            // Assert
            Assert_CreatingARegistrationWithMismatchTriggersAWarning(createLifestyleMismatch);
        }

        [TestMethod]
        public void Analyze_ProducerNonGenericWithLifestyleMismatch_ProducesALifestyleMismatchWarning()
        {
            // Arrange
            Func<Container, InstanceProducer> createLifestyleMismatch = container =>
                Lifestyle.Singleton.CreateProducer(typeof(RealUserService), typeof(RealUserService), container);

            // Assert
            Assert_CreatingARegistrationWithMismatchTriggersAWarning(createLifestyleMismatch);
        }

        [TestMethod]
        public void Analyze_UnusedProducerWithGarbageCollected_DoesNotProduceAnyWarnings()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>(Lifestyle.Transient);

            Lifestyle.Singleton.CreateProducer(typeof(RealUserService), typeof(RealUserService), container);

            GC.Collect();

            container.Verify();

            // Act
            var results =
                Analyzer.Analyze(container).OfType<LifestyleMismatchDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(0, results.Length,
                "The producer should have been disposed and therefor not visible in the results. " +
                "There is a warning and this means that the container keeps a reference to the producer, " +
                "which means there is a memory leak in the container.");
        }

        [TestMethod]
        public void Analyze_ConfigurationWithCollectionRegistration_DoesNotProduceAnyWarnings()
        {
            // Arrange
            var container = new Container();

            container.RegisterCollection<IPlugin>(new[] { typeof(SomePluginImpl) });

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container);

            // Assert
            Assert.AreEqual(0, results.Length,
                "Since the SomePluginImpl is registered explicitly using the RegisterAll method, this " +
                "registration should contain no warnings, but the following warnings are present: \"" +
                string.Join(" ", results.Select(result => result.Description)) + "\"");
        }

        [TestMethod]
        public void Analyze_ConfigurationWithCollectionRegistrationWithElementWithSrpViolation_ProducesASingleResponsibilityViolationWarning()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(IGeneric<>), typeof(GenericType<>));

            container.RegisterCollection<IPlugin>(new[] { typeof(PluginWith8Dependencies) });

            container.Verify();
            
            // Act
            var results = Analyzer.Analyze(container);

            // Assert
            Assert.AreEqual(1, results.Length, "An SRP violation is expected.");

            Assert.AreEqual(1, results.OfType<SingleResponsibilityViolationDiagnosticResult>().Count(),
                "An SRP violation is expected.");
        }
        
        private static void Assert_CreatingARegistrationWithMismatchTriggersAWarning(
            Func<Container, InstanceProducer> createLifestyleMismatch)
        {
            // Arrange
            var container = new Container();
            container.Options.SuppressLifestyleMismatchVerification = true;

            container.Register<IUserRepository, InMemoryUserRepository>(Lifestyle.Transient);

            // RealUserService (Singleton) depends on InMemoryUserRepository (Transient).
            var producer = createLifestyleMismatch(container);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<LifestyleMismatchDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(1, results.Length,
                "Even though RealUserService isn't registered in the container directly, when creating a " +
                "Registration or InstanceProducer, such instance should still be verified by the container. " +
                "Warnings: " +
                string.Join(" ", results.Select(result => result.Description)) +
                (results.Any() ? string.Empty : "[No warnings]"));

            GC.KeepAlive(producer);
        }
   }
}