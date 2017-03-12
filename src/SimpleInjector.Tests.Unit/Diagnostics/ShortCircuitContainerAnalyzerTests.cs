namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Diagnostics.Analyzers;
    using SimpleInjector.Diagnostics.Debugger;
    using SimpleInjector.Tests.Unit;

    public interface IUnitOfWork
    {
    }

    public interface IService1
    {
    }

    public interface IService2
    {
    }

    [TestClass]
    public class ShortCircuitContainerAnalyzerTests
    {
        [TestMethod]
        public void Analyze_OnConfigurationWithOneShortCircuitedRegistration_ReturnsThatWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IUnitOfWork, MyUnitOfWork>(Lifestyle.Singleton);

            // HomeController depends on MyUnitOfWork.
            container.Register<HomeController>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = GetShortCircuitedResults(DebuggerGeneralWarningsContainerAnalyzer.Analyze(container));

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("HomeController", results[0].Name);
            Assert.AreEqual(
                "HomeController might incorrectly depend on unregistered type MyUnitOfWork " +
                "(Transient) instead of IUnitOfWork (Singleton).",
                results[0].Description);
        }

        [TestMethod]
        public void Analyze_OnConfigurationWithOneShortCircuitedRegistration_ReturnsSeverityWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IUnitOfWork, MyUnitOfWork>(Lifestyle.Singleton);

            // HomeController depends on MyUnitOfWork.
            container.Register<HomeController>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var result = Analyzer.Analyze(container).OfType<ShortCircuitedDependencyDiagnosticResult>().First();

            // Assert
            Assert.AreEqual(DiagnosticSeverity.Warning, result.Severity);
        }

        [TestMethod]
        public void Analyze_OnConfigurationWithOneShortCircuitedRegistrationWithSuppressDiagnosticWarning_ReturnsNoWarning()
        {
            // Arrange
            var container = new Container();

            // HomeController depends on MyUnitOfWork.
            container.Register<IUnitOfWork, MyUnitOfWork>(Lifestyle.Singleton);

            container.Register<HomeController>();

            container.Verify(VerificationOption.VerifyOnly);

            var registration = container.GetRegistration(typeof(HomeController)).Registration;

            registration.SuppressDiagnosticWarning(DiagnosticType.ShortCircuitedDependency);

            // Act
            var results = Analyzer.Analyze(container).OfType<ShortCircuitedDependencyDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_OnConfigurationWithOneShortCircuitedRegistrationWithTwoPossibleSolutions_ReturnsThatWarning()
        {
            // Arrange
            var container = new Container();

            var registration = Lifestyle.Singleton.CreateRegistration<ImplementsBothInterfaces>(container);

            container.AddRegistration(typeof(IService1), registration);
            container.AddRegistration(typeof(IService2), registration);

            container.Register<Controller<int>>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = GetShortCircuitedResults(DebuggerGeneralWarningsContainerAnalyzer.Analyze(container));

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(typeof(Controller<int>).ToFriendlyName(), results[0].Name);
            Assert.AreEqual(
                "Controller<Int32> might incorrectly depend on unregistered type ImplementsBothInterfaces " +
                "(Transient) instead of IService1 (Singleton) or IService2 (Singleton).",
                results[0].Description);
        }

        [TestMethod]
        public void Analyze_ShortCircuitedRegistrationWithMultipleTypesInOneGroup_ReportsExpectedWarning()
        {
            // Arrange
            var container = new Container();

            var registration = Lifestyle.Singleton.CreateRegistration<ImplementsBothInterfaces>(container);

            container.AddRegistration(typeof(IService1), registration);
            container.AddRegistration(typeof(IService2), registration);

            // Two types in same group
            container.Register<Controller<int>>();
            container.Register<Controller<float>>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = GetShortCircuitedResults(DebuggerGeneralWarningsContainerAnalyzer.Analyze(container)).Single();

            // Assert
            Assert.AreEqual("Controller<T>", results.Name);
            Assert.AreEqual("2 short circuited components.", results.Description);
            AssertThat.IsInstanceOfType(typeof(DebuggerViewItem[]), results.Value);
            Assert.AreEqual(2, ((DebuggerViewItem[])results.Value).Length);
        }

        // Checks #248. 
        [TestMethod]
        public void Analyze_ShortCiruitedRegistrationWithSameLifestyle_ReportsExpectedWarning()
        {
            // Arrange
            var container = new Container();
            
            container.Register<ILogger, NullLogger>();
            container.Register<ServiceDependingOn<NullLogger>>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = GetShortCircuitedResults(DebuggerGeneralWarningsContainerAnalyzer.Analyze(container)).Single();

            // Assert
            Assert.AreEqual("ServiceDependingOn<NullLogger>", results.Name);
        }

        // Regression #248.
        [TestMethod]
        public void Analyze_ShortCiruitedRegistrationWithDecorator_ReportsExpectedWarning()
        {
            // Arrange
            var container = new Container();

            container.RegisterDecorator<ILogger, LoggerDecorator>();
            container.Register<ILogger, NullLogger>();
            container.Register<ServiceDependingOn<NullLogger>>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = GetShortCircuitedResults(DebuggerGeneralWarningsContainerAnalyzer.Analyze(container)).Single();

            // Assert
            Assert.AreEqual("ServiceDependingOn<NullLogger>", results.Name);
        }

        private static DebuggerViewItem[] GetShortCircuitedResults(DebuggerViewItem item)
        {
            var results = item.Value as DebuggerViewItem[];

            return results
                .Single(result => result.Name == "Possible Short Circuited Dependencies")
                .Value as DebuggerViewItem[];
        }

        private static string Actual(ShortCircuitedDependencyDiagnosticResult[] results) => 
            "actual: " + string.Join(" - ", results.Select(r => r.Description));
    }
        
    public class ImplementsBothInterfaces : IService1, IService2
    {
    }

    public class Controller<T>
    {
        public Controller(ImplementsBothInterfaces concrete)
        {
        }
    }

    public class MyUnitOfWork : IUnitOfWork 
    {
    }

    public class HomeController
    {
        private readonly MyUnitOfWork uow;

        public HomeController(MyUnitOfWork uow)
        {
            this.uow = uow;
        }
    }
}