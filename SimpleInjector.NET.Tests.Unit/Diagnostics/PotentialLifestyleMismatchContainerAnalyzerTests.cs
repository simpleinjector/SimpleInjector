namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Diagnostics.Analyzers;

    [TestClass]
    public class PotentialLifestyleMismatchContainerAnalyzerTests
    {
        [TestMethod]
        public void Analyze_ContainerWithOneMismatch_ReturnsItemWithExpectedName()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>();

            // RealUserService depends on IUserRepository
            container.RegisterSingle<RealUserService>();

            container.Verify();

            // Act
            var item = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container);

            // Assert
            Assert.AreEqual("Potential Lifestyle Mismatches", item.Name);
        }

        [TestMethod]
        public void Analyze_ContainerWithOneMismatch_ReturnsItemWithExpectedDescription()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>();

            // RealUserService depends on IUserRepository
            container.RegisterSingle<RealUserService>();

            container.Verify();

            // Act
            var item = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container);

            // Assert
            Assert.AreEqual("1 possible lifestyle mismatch for 1 service.", item.Description);
        }

        [TestMethod]
        public void Analyze_ContainerWithTwoMismatch_ReturnsItemWithExpectedDescription()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>();

            // RealUserService depends on IUserRepository
            container.RegisterSingle<RealUserService>();

            // FakeUserService depends on IUserRepository
            container.RegisterSingle<FakeUserService>();

            container.Verify();

            // Act
            var item = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container);

            // Assert
            Assert.AreEqual("2 possible lifestyle mismatches for 2 services.", item.Description);
        }

        [TestMethod]
        public void Analyze_MismatchWithSuppressDiagnosticWarning_NoWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>();

            var registration = 
                Lifestyle.Singleton.CreateRegistration<UserServiceBase, RealUserService>(container);

            container.AddRegistration(typeof(UserServiceBase), registration);

            container.Verify();

            registration.SuppressDiagnosticWarning(DiagnosticType.PotentialLifestyleMismatch);

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<PotentialLifestyleMismatchDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        private static string Actual(PotentialLifestyleMismatchDiagnosticResult[] results)
        {
            return "actual: " + string.Join(" - ", results.Select(r => r.Description));
        }
    }
}