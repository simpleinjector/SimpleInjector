namespace SimpleInjector.Tests.Unit.Analysis
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Analysis;

    [TestClass]
    public class PotentialLifestyleMismatchContainerAnalyzerTests
    {
        [TestMethod]
        public void Analyse_Always_ReturnsAnItemWithThePotentialLifestyleMismatchesName()
        {
            // Arrange
            var analyzer = new PotentialLifestyleMismatchContainerAnalyzer();

            // Act
            var item = analyzer.Analyse(new Container());

            // Assert
            Assert.IsNotNull(item);
            Assert.AreEqual("Potential Lifestyle Mismatches", item.Name);
        }

        [TestMethod]
        public void Analyse_ContainerWithOneMismatch_ReturnsItemWithExpectedDescription()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>();

            // RealUserService depends on IUserRepository
            container.RegisterSingle<RealUserService>();

            var analyzer = new PotentialLifestyleMismatchContainerAnalyzer();

            container.Verify();

            // Act
            var item = analyzer.Analyse(container);

            // Assert
            Assert.AreEqual("1 potential mismatches for 1 services.", item.Description);
        }

        [TestMethod]
        public void Analyse_ContainerWithTwoMismatch_ReturnsItemWithExpectedDescription()
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
            var item = analyzer.Analyse(container);

            // Assert
            Assert.AreEqual("2 potential mismatches for 2 services.", item.Description);
        }
    }
}