namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Diagnostics;

#if DEBUG
    [TestClass]
    public class PotentialLifestyleMismatchContainerAnalyzerTests
    {
        [TestMethod]
        public void Analyze_ValidConfiguration_ReturnsNull()
        {
            // Arrange
            var analyzer = new PotentialLifestyleMismatchContainerAnalyzer();

            // Act
            var item = analyzer.Analyze(new Container());

            // Assert
            Assert.IsNull(item, 
                "By returning null, the results can be hidden by the GeneralWarningsContainerAnalyzer.");
        }

        [TestMethod]
        public void Analyze_ContainerWithOneMismatch_ReturnsItemWithExpectedName()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>();

            // RealUserService depends on IUserRepository
            container.RegisterSingle<RealUserService>();

            var analyzer = new PotentialLifestyleMismatchContainerAnalyzer();

            container.Verify();

            // Act
            var item = analyzer.Analyze(container);

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

            var analyzer = new PotentialLifestyleMismatchContainerAnalyzer();

            container.Verify();

            // Act
            var item = analyzer.Analyze(container);

            // Assert
            Assert.AreEqual("1 possible mismatch for 1 service.", item.Description);
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

            var analyzer = new PotentialLifestyleMismatchContainerAnalyzer();

            container.Verify();

            // Act
            var item = analyzer.Analyze(container);

            // Assert
            Assert.AreEqual("2 possible mismatches for 2 services.", item.Description);
        }
    }
#endif
}