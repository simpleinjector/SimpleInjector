namespace SimpleInjector.Integration.Web.Tests.Unit
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WebRequestLifestyleTests
    {
        [TestMethod]
        public void Instance_Always_NotNull()
        {
            Assert.IsNotNull(WebRequestLifestyle.Instance);
        }

        [TestMethod]
        public void Instance_Always_ReturnsTheSameInstance()
        {
            Assert.IsTrue(
                object.ReferenceEquals(WebRequestLifestyle.Instance, WebRequestLifestyle.Instance));
        }
    }
}