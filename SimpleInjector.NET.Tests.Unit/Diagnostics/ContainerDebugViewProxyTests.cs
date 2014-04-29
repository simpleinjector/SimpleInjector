namespace SimpleInjector.Tests.Unit.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;

    [TestClass]
    public class ContainerDebugViewProxyTests
    {
        [TestMethod]
        public void Ctor_Always_Succeeds()
        {
            new ContainerDebugViewProxy(new Container());
        }

        [TestMethod]
        public void Cctor_LoadingDiagnosticsAssembly_Succeeds()
        {
            // When the static exception field is set, the cctor failed to initialize.
            if (ContainerDebugViewProxy.Exception != null)
            {
                throw ContainerDebugViewProxy.Exception;
            }
        }

        [TestMethod]
        public void RetrievingItems_Always_Succeeds()
        {
            // Arrange
            var proxy = new ContainerDebugViewProxy(new Container());

            // Act
            var items = proxy.Items;
        }

        [TestMethod]
        public void Options_Always_ReturnsTheOptionsFromTheContainer()
        {
            // Arrange
            var container = new Container();

            var proxy = new ContainerDebugViewProxy(container);

            // Act
            var options = proxy.Options;

            // Assert
            Assert.AreSame(container.Options, options);
        }
    }
}