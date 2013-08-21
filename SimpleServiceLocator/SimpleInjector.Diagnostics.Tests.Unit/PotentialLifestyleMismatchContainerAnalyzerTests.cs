namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Diagnostics.Analyzers;

    public static class Helpers
    {
        public static DiagnosticGroup Root(this DiagnosticGroup group)
        {
            while (true)
            {
                if (group.Parent == null)
                {
                    return group;
                }

                group = group.Parent;
            }
        }
    }

#if DEBUG
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
    }
#endif
}