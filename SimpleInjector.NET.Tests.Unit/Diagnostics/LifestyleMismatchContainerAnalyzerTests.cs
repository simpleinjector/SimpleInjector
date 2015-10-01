namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Diagnostics.Analyzers;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class LifestyleMismatchContainerAnalyzerTests
    {
        [TestMethod]
        public void Analyze_ContainerWithOneMismatch_ReturnsItemWithExpectedName()
        {
            // Arrange
            var container = new Container();
            container.Options.SuppressLifestyleMismatchVerification = true;

            container.Register<IUserRepository, InMemoryUserRepository>();

            // RealUserService depends on IUserRepository
            container.Register<RealUserService>(Lifestyle.Singleton);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var item = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container);

            // Assert
            Assert.AreEqual("Lifestyle Mismatches", item.Name);
        }

        [TestMethod]
        public void Analyze_ContainerWithOneMismatchCausedByDecorator_ReturnsExpectedWarning()
        {
            // Arrange
            var container = new Container();
            container.Options.SuppressLifestyleMismatchVerification = true;

            container.Register<ILogger, ConsoleLogger>(Lifestyle.Singleton);
            container.RegisterDecorator<ILogger, LoggerDecorator>(Lifestyle.Transient);

            // RealUserService depends on IUserRepository
            container.Register<ServiceWithDependency<ILogger>>(Lifestyle.Singleton);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var result = Analyzer.Analyze(container).OfType<LifestyleMismatchDiagnosticResult>().Single();

            // Assert
            Assert.AreEqual(
                "ServiceWithDependency<ILogger> (Singleton) depends on ILogger implemented by " +
                "LoggerDecorator (Transient).",
                result.Description);
        }

        [TestMethod]
        public void Analyze_ContainerWithOneMismatch_ReturnsSeverityWarning()
        {
            // Arrange
            var container = new Container();
            container.Options.SuppressLifestyleMismatchVerification = true;

            container.Register<IUserRepository, InMemoryUserRepository>();

            // RealUserService depends on IUserRepository
            container.Register<RealUserService>(Lifestyle.Singleton);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var result = Analyzer.Analyze(container).OfType<LifestyleMismatchDiagnosticResult>().First();

            // Assert
            Assert.AreEqual(DiagnosticSeverity.Warning, result.Severity);
        }

        [TestMethod]
        public void Analyze_ContainerWithOneMismatch_ReturnsItemWithExpectedDescription()
        {
            // Arrange
            var container = new Container();
            container.Options.SuppressLifestyleMismatchVerification = true;

            container.Register<IUserRepository, InMemoryUserRepository>();

            // RealUserService depends on IUserRepository
            container.Register<RealUserService>(Lifestyle.Singleton);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var item = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container);

            // Assert
            Assert.AreEqual("1 lifestyle mismatch for 1 service.", item.Description);
        }

        [TestMethod]
        public void Analyze_ContainerWithTwoMismatch_ReturnsItemWithExpectedDescription()
        {
            // Arrange
            var container = new Container();
            container.Options.SuppressLifestyleMismatchVerification = true;

            container.Register<IUserRepository, InMemoryUserRepository>();

            // RealUserService depends on IUserRepository
            container.Register<RealUserService>(Lifestyle.Singleton);

            // FakeUserService depends on IUserRepository
            container.Register<FakeUserService>(Lifestyle.Singleton);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var item = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container);

            // Assert
            Assert.AreEqual("2 lifestyle mismatches for 2 services.", item.Description);
        }

        [TestMethod]
        public void Analyze_MismatchWithSuppressDiagnosticWarning_NoWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>(Lifestyle.Transient);
            container.Register<UserServiceBase, RealUserService>(Lifestyle.Singleton);

            var registration = container.GetRegistration(typeof(IUserRepository)).Registration;

            container.Options.SuppressLifestyleMismatchVerification = true;

            container.Verify(VerificationOption.VerifyOnly);

            registration.SuppressDiagnosticWarning(DiagnosticType.LifestyleMismatch);

            container.Options.SuppressLifestyleMismatchVerification = false;

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<LifestyleMismatchDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        // See issue #128.
        [TestMethod]
        public void Analyze_MismatchBetweenDecoratorAndDecorateeWithThreeLayersOfDecorators_ReturnsExpectedWarning()
        {
            // Arrange
            var container = new Container();

            container.Options.SuppressLifestyleMismatchVerification = true;

            container.Register(typeof(ICommandHandler<int>), typeof(NullCommandHandler<int>), Lifestyle.Transient);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Singleton);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Singleton);

            // Adding this third decorator caused the warning to not be shown in v3.0
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Singleton);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<LifestyleMismatchDiagnosticResult>().ToArray();

            // Arrange
            Assert.AreEqual(1, results.Length, Actual(results));

            Assert.AreEqual(
                "CommandHandlerDecorator<Int32> (Singleton) depends on ICommandHandler<Int32> implemented " +
                "by NullCommandHandler<Int32> (Transient).",
                results.Single().Description);
        }

        private static string Actual(LifestyleMismatchDiagnosticResult[] results)
        {
            return "actual: " + string.Join(" - ", results.Select(r => r.Description));
        }
    }
}