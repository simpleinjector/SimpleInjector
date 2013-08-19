#if DEBUG
namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Diagnostics;
    using SimpleInjector.Diagnostics.Analyzers;
    using SimpleInjector.Diagnostics.Debugger;

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

            container.Register<HomeController>();

            container.Verify();

            var analyzer = new DebuggerGeneralWarningsContainerAnalyzer();

            // Act
            var results = GetShortCircuitedResults(analyzer.Analyze(container));

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("HomeController", results[0].Name);
            Assert.AreEqual(
                "HomeController might incorrectly depend on unregistered type MyUnitOfWork " +
                "(Transient) instead of IUnitOfWork (Singleton).",
                results[0].Description);
        }

        [TestMethod]
        public void Analyze_OnConfigurationWithOneShortCircuitedRegistrationWithTwoPossibleSolutions_ReturnsThatWarning()
        {
            // Arrange
            var container = new Container();

            var registration = Lifestyle.Singleton
                .CreateRegistration<ImplementsBothInterfaces, ImplementsBothInterfaces>(container);

            container.AddRegistration(typeof(IService1), registration);
            container.AddRegistration(typeof(IService2), registration);

            container.Register<Controller<int>>();

            container.Verify();

            var analyzer = new DebuggerGeneralWarningsContainerAnalyzer();

            // Act
            var results = GetShortCircuitedResults(analyzer.Analyze(container));

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(typeof(Controller<int>).ToFriendlyName(), results[0].Name);
            Assert.AreEqual(
                "Controller<Int32> might incorrectly depend on unregistered type ImplementsBothInterfaces " +
                "(Transient) instead of IService1 (Singleton) or IService2 (Singleton).",
                results[0].Description);
        }

        [TestMethod]
        public void Analyze_ShortCircuitedRegistrationWithMultipleTypesInOneGroup_Behavior()
        {
            // Arrange
            var container = new Container();

            var registration = Lifestyle.Singleton
                .CreateRegistration<ImplementsBothInterfaces, ImplementsBothInterfaces>(container);

            container.AddRegistration(typeof(IService1), registration);
            container.AddRegistration(typeof(IService2), registration);

            // Two types in same group
            container.Register<Controller<int>>();
            container.Register<Controller<float>>();

            container.Verify();

            var analyzer = new DebuggerGeneralWarningsContainerAnalyzer();

            // Act
            var results = GetShortCircuitedResults(analyzer.Analyze(container)).Single();

            // Assert
            Assert.AreEqual("Controller<T>", results.Name);
            Assert.AreEqual("2 short circuited components.", results.Description);
            Assert.IsInstanceOfType(results.Value, typeof(DebuggerViewItem[]));
            Assert.AreEqual(2, ((DebuggerViewItem[])results.Value).Length);
        }

        private static DebuggerViewItem[] GetShortCircuitedResults(DebuggerViewItem item)
        {
            var results = item.Value as DebuggerViewItem[];

            return results
                .Single(result => result.Name == "Possible Short Circuited Dependencies")
                .Value as DebuggerViewItem[];
        }
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
#endif