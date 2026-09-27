#pragma warning disable CS9113 // Parameter is unread.
namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Diagnostics.Analyzers;
    using SimpleInjector.Diagnostics.Debugger;

    [TestClass]
    public class UnusedConditionalRegistrationAnalyzerTests
    {
        [TestMethod]
        public void Analyze_OnConfigurationWithOneUnusedConditionalRegistration_ReturnsThatWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<UserServiceBase, RealUserService>();

            container.RegisterConditional<IUserRepository, SqlUserRepository>(c => true);
            container.RegisterConditional<IUserRepository, InMemoryUserRepository>(c => false);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            DebuggerViewItem item = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container);
            var results = GetUnusedConditionalRegistrationsResults(item);

            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("IUserRepository", results[0].Name);
            Assert.AreEqual(
                "The conditional registration for IUserRepository (Transient) with implementation " +
                "InMemoryUserRepository is never used as a dependency.",
                results[0].Description);
        }

        private static DebuggerViewItem[] GetUnusedConditionalRegistrationsResults(DebuggerViewItem item)
        {
            if (item.Name == "Unused Conditional Registration") return item.Value as DebuggerViewItem[];

            var results = item.Value as DebuggerViewItem[];

            return results
                .Single(result => result.Name == "Unused Conditional Registration")
                .Value as DebuggerViewItem[];
        }
    }
}
#pragma warning restore CS9113 // Parameter is unread.