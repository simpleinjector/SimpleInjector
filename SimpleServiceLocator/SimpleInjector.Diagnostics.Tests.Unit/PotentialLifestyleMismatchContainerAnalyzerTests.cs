namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Diagnostics;
    using SimpleInjector.Diagnostics.Analyzers;
    using SimpleInjector.Diagnostics.Debugger;
    using SimpleInjector.Extensions;

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
        public void Analyze_ValidConfiguration_ReturnsNull()
        {
            // Arrange
            var analyzer = new DebuggerPotentialLifestyleMismatchContainerAnalyzer();

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

            var analyzer = new DebuggerPotentialLifestyleMismatchContainerAnalyzer();

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

            var analyzer = new DebuggerPotentialLifestyleMismatchContainerAnalyzer();

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

            var analyzer = new DebuggerPotentialLifestyleMismatchContainerAnalyzer();

            container.Verify();

            // Act
            var item = analyzer.Analyze(container);

            // Assert
            Assert.AreEqual("2 possible mismatches for 2 services.", item.Description);

        }

        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior()
        {
            // Arrange
            var container = new Container();

            AddLifestyleMismatch(container);
            
            AddShortCircuit(container);

            AddSRPViolation(container);
            
            container.Verify();

            var analyzer = new Analyzer(container);

            var results = analyzer.Analyze();

            var rootGroups = results.Select(r => r.Group.Root()).Distinct().ToArray();

            System.Console.WriteLine();

            // Act

            // Assert
        }

        private static void AddLifestyleMismatch(Container container)
        {
            container.Register<IUserRepository, InMemoryUserRepository>();

            // RealUserService depends on IUserRepository
            container.RegisterSingle<RealUserService>();

            // FakeUserService depends on IUserRepository
            container.RegisterSingle<FakeUserService>();


            // Transient
            container.Register<ILogger, FakeLogger>();

            // Singletons depending on transient logger.
            container.RegisterSingle<ICommandHandler<int>, GenericHandler<int, ILogger>>();
            container.RegisterSingle<ICommandHandler<float>, GenericHandler<float, ILogger>>();
            container.RegisterSingle<ICommandHandler<byte>, GenericHandler<byte, ILogger>>();
            container.RegisterSingle<ICommandHandler<decimal>, GenericHandler<decimal, ILogger>>();
        }

        private static void AddShortCircuit(Container container)
        {
            var registration = Lifestyle.Singleton
                .CreateRegistration<ImplementsBothInterfaces, ImplementsBothInterfaces>(container);

            container.AddRegistration(typeof(IService1), registration);
            container.AddRegistration(typeof(IService2), registration);

            container.Register<Controller<int>>();

        }

        private static void AddSRPViolation(Container container)
        {
            container.RegisterOpenGeneric(typeof(IGeneric<>), typeof(GenericType<>));

            container.Register<PluginWith7Dependencies>();
        }
    }
#endif
}