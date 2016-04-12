namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
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

        // See issue #129.
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

            // To make sure the warning is never lost, let's add a few extra decorators.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Singleton);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Singleton);
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

        // See issue #128.
        [TestMethod]
        public void Analyze_MismatchBetweenDecoratorAndDecorateeWrappedWithDecoratorWithDecorateeFactory_ReturnsExpectedWarning()
        {
            // Arrange
            var container = new Container();

            container.Options.SuppressLifestyleMismatchVerification = true;

            container.Register(typeof(ICommandHandler<int>), typeof(NullCommandHandler<int>), Lifestyle.Transient);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Singleton);

            // AsyncCommandHandlerProxy<T> depends on Func<ICommandHandler<T>>. Due to the bug reported in
            // #128, this suppressed the mismatch warning.
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>), Lifestyle.Singleton);

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

        // See issue #128.
        [TestMethod]
        public void Verify_WithDecorateeGraphWithLifestyleMismatch_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(ICommandHandler<int>), typeof(NullCommandHandler<int>), Lifestyle.Transient);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Singleton);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>), Lifestyle.Singleton);

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(
                "CommandHandlerDecorator<Int32> (Singleton) depends on ICommandHandler<Int32>",
                action);
        }

        // See issue #128.
        [TestMethod]
        public void GetInstance_WithDecorateeGraphWithLifestyleMismatch_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(ICommandHandler<int>), typeof(NullCommandHandler<int>), Lifestyle.Transient);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Singleton);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>), Lifestyle.Singleton);

            // Act
            Action action = () => container.GetInstance<ICommandHandler<int>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "CommandHandlerDecorator<Int32> (Singleton) depends on ICommandHandler<Int32>",
                action);
        }

        [TestMethod]
        public void InvokeDecorateeFactory_WithDecorateeGraphWithLifestyleMismatchContainerSuppressingLifestyleMismatchVerification_Succeeeds()
        {
            // Arrange
            var container = new Container();

            container.Options.SuppressLifestyleMismatchVerification = true;

            container.Register(typeof(ICommandHandler<int>), typeof(NullCommandHandler<int>), Lifestyle.Transient);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>), Lifestyle.Singleton);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>), Lifestyle.Singleton);

            var proxy = (AsyncCommandHandlerProxy<int>)container.GetInstance<ICommandHandler<int>>();

            // Act
            proxy.DecorateeFactory();
        }

        [TestMethod]
        public void GetInstance_ResolvingcTypeWithLifestyleMismatchInGraph_ThrowsException()
        {
            // Arrange
            var container = new Container();

            container.Register<ServiceWithDependency<ServiceDependingOn<ILogger>>>(Lifestyle.Transient);
            container.Register<ServiceDependingOn<ILogger>>(Lifestyle.Singleton);
            container.Register<ILogger, NullLogger>(Lifestyle.Transient);

            // Act
            Action action = () => container.GetInstance(typeof(ServiceWithDependency<ServiceDependingOn<ILogger>>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "lifestyle mismatch is encountered",
                action);
        }

        [TestMethod]
        public void GetInstance_ResolvingGenericTypeWithLifestyleMismatchInGraph_ThrowsException()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(ServiceWithDependency<>), typeof(ServiceWithDependency<>), Lifestyle.Transient);
            container.Register(typeof(ServiceDependingOn<>), typeof(ServiceDependingOn<>), Lifestyle.Singleton);
            container.Register<ILogger, NullLogger>(Lifestyle.Transient);

            // Act
            // Resolve graph: 
            // ServiceWithDependency<T> (Transient) -> ServiceDependingOn<T> (Singleton) -> NullLogger (Transient)
            Action action = () => container.GetInstance(typeof(ServiceWithDependency<ServiceDependingOn<ILogger>>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "lifestyle mismatch is encountered",
                action);
        }

        private static string Actual(LifestyleMismatchDiagnosticResult[] results) => 
            "actual: " + string.Join(" - ", results.Select(r => r.Description));
    }
}