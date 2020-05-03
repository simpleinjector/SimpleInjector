namespace SimpleInjector.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ActiveXTests
    {
        // #589
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

        // #589
        [TestMethod]
        public void RegisterInstance_RegisteringVerifyingAndResolvingAnActiveXObject_Succeeds()
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
    }
}