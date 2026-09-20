namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    // #987
    [TestClass]
    public sealed class ActiveXTests : IDisposable
    {
        private SHDocVw.InternetExplorer comObject;

        public ActiveXTests()
        {
             this.comObject = new SHDocVw.InternetExplorer();
        }

        public void Dispose()
        {
            if (this.comObject != null)
            {
                this.comObject.Quit();
                this.comObject = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [TestMethod]
        public void RegisterInstance_RegisteringAnActiveXObject_Fails()
        {
            // Arrange
            var container = new Container();

            // Act
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "You are trying to register COM object __ComObject, which not supported.",
                () => container.RegisterInstance(this.comObject));
        }

        // Registering COM objects is no longer supported in v6.
        [TestMethod]
        public void RegisterInstance_RegisteringAnActiveXObjectViaAnInterface_Fails()
        {
            // Arrange
            var container = new Container();

            // Act
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "You are trying to register COM object __ComObject, which not supported.",
                () => container.RegisterInstance(typeof(SHDocVw.InternetExplorer), this.comObject));
        }
    }
}