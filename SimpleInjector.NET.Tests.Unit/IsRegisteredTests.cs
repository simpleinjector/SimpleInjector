namespace SimpleInjector.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IsRegisteredTests
    {
        private interface ITest { }

        private class TestImplementation : ITest
        {

        }

        [TestMethod]
        public void IsRegisteredTest()
        {
            var container = new Container();

            container.Register<ITest, TestImplementation>();
            Assert.IsTrue(container.IsRegistered<ITest>());
        }
    }
}