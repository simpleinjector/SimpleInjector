namespace SimpleInjector.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    // #589
    [TestClass]
    public class ActiveXTests
    {
        [TestMethod]
        public void RegisterSingleton_RegisteringVerifyingAndResolvingAnActiveXObject_Succeeds()
        {
            // Arrange
            var container = new Container();
            SHDocVw.InternetExplorer comObject = new SHDocVw.InternetExplorer();

            // Act
            container.RegisterSingleton(() => comObject);
            container.Verify();
            var ie = container.GetInstance<SHDocVw.InternetExplorer>();
            ie.ToolBar = 0;
        }

        [TestMethod]
        public void RegisterInstanceGeneric_RegisteringVerifyingAndResolvingAnActiveXObject_Succeeds()
        {
            // Arrange
            var container = new Container();
            SHDocVw.InternetExplorer comObject = new SHDocVw.InternetExplorer();

            // Act
            container.RegisterInstance(comObject);
            container.Verify();
            var ie = container.GetInstance<SHDocVw.InternetExplorer>();
            ie.ToolBar = 0;
        }

        [TestMethod]
        public void RegisterInstance_RegisteringVerifyingAndResolvingAnActiveXObject_Succeeds()
        {
            // Arrange
            var container = new Container();
            SHDocVw.InternetExplorer comObject = new SHDocVw.InternetExplorer();

            // Act
            container.RegisterInstance(typeof(SHDocVw.InternetExplorer), comObject);
            container.Verify();
            var ie = container.GetInstance<SHDocVw.InternetExplorer>();
            ie.ToolBar = 0;
        }
    }
}