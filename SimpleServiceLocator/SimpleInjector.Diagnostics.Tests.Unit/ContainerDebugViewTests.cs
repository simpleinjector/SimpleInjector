#if !SILVERLIGHT
#if DEBUG
namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Diagnostics;
    using SimpleInjector.Diagnostics.Debugger;

    public interface ILogger
    {
    }

    public interface IConcreteThing
    {
    }

    public class ConcreteShizzle
    {
    }

    public class FakeLogger : ILogger
    {
        public FakeLogger(ConcreteShizzle shizzle, ConcreteThing thing)
        {
        }
    }

    public class ConcreteThing : IConcreteThing
    {
    }

    [TestClass]
    public class ContainerDebugViewTests
    {
        [TestMethod]
        public void Ctor_WithValidArgument_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            new ContainerDebugView(container);
        }

        [TestMethod]
        public void Options_Always_ReturnsSameInstanceAsThatOfContainer()
        {
            // Arrange
            var container = new Container();

            // Act
            var debugView = new ContainerDebugView(container);

            // Assert
            Assert.IsTrue(object.ReferenceEquals(container.Options, debugView.Options));
        }

        [TestMethod]
        public void Ctor_WithUnlockedContainer_LeavesContainerUnlocked()
        {
            var container = new Container();

            // Act
            new ContainerDebugView(container);

            // Assert
            Assert.IsFalse(container.IsLocked);
        }

        [TestMethod]
        public void Ctor_WithLockedContainer_LeavesContainerLocked()
        {
            var container = new Container();

            // This locks the container
            container.Verify();

            Assert.IsTrue(container.IsLocked, "Test setup failed.");

            // Act
            new ContainerDebugView(container);

            // Assert
            Assert.IsTrue(container.IsLocked);
        }

        [TestMethod]
        public void Ctor_WithLockedContainer_ReturnsAnItemWithTheRegistrations()
        {
            var container = new Container();

            // This locks the container
            container.Verify();

            var debugView = new ContainerDebugView(container);

            // Act
            var registrationsItem = debugView.Items.Single(item => item.Name == "Registrations");

            // Assert
            Assert.IsInstanceOfType(registrationsItem.Value, typeof(InstanceProducer[]));
        }

        [TestMethod]
        public void Ctor_UnverifiedContainer_ReturnsOneItemWithInfoAboutHowToGetAnalysisInformation()
        {
            // Arrange
            var container = new Container();

            // Act
            var debugView = new ContainerDebugView(container);

            // Assert
            Assert.AreEqual(1, debugView.Items.Length);
            Assert.AreEqual("How To View Diagnostic Info", debugView.Items.Single().Name);
            Assert.AreEqual(
                "Analysis info is available in this debug view after Verify() is " +
                "called on this container instance.", 
                debugView.Items.Single().Description);
        }

        [TestMethod]
        public void Ctor_UnsuccesfullyVerifiedContainer_ReturnsOneItemWithInfoAboutHowToGetAnalysisInformation()
        {
            // Arrange
            var container = new Container();

            // Invalid registration
            container.Register<ILogger>(() => null);

            try
            {
                container.Verify();
            }
            catch (InvalidOperationException) 
            {
            }

            // Act
            var debugView = new ContainerDebugView(container);

            // Assert
            Assert.AreEqual(1, debugView.Items.Length);
            Assert.AreEqual("How To View Diagnostic Info", debugView.Items.Single().Name);
        }

        [TestMethod]
        public void Ctor_VerifiedContainerWithoutConfigurationErrors_ContainsAPotentialLifestyleMismatchesSection()
        {
            // Arrange
            var container = new Container();

            // Forces a lifestyle mismatch
            container.RegisterSingle<ILogger, FakeLogger>();

            container.Verify();

            // Act
            var debugView = new ContainerDebugView(container);

            var warningsItem = debugView.Items.Single(item => item.Name == "Configuration Warnings");

            var items = warningsItem.Value as DebuggerViewItem[];

            // Assert
            Assert.IsTrue(items.Any(item => item.Name == "Potential Lifestyle Mismatches"));
        }
    }
}
#endif
#endif