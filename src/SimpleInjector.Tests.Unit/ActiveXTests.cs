namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

#if false
    // #589
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
        public void RegisterSingleton_RegisteringVerifyingAndResolvingAnActiveXObject_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterSingleton(() => this.comObject);
            container.Verify();
            var ie = container.GetInstance<SHDocVw.InternetExplorer>();
            ie.ToolBar = 0;
        }

        [TestMethod]
        public void RegisterInstanceGeneric_RegisteringVerifyingAndResolvingAnActiveXObject_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterInstance(this.comObject);
            container.Verify();
            var ie = container.GetInstance<SHDocVw.InternetExplorer>();
            ie.ToolBar = 0;
        }

        [TestMethod]
        public void RegisterInstance_RegisteringVerifyingAndResolvingAnActiveXObject_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.RegisterInstance(typeof(SHDocVw.InternetExplorer), this.comObject);
            container.Verify();
            var ie = container.GetInstance<SHDocVw.InternetExplorer>();
            ie.ToolBar = 0;
        }
    }
#endif
}