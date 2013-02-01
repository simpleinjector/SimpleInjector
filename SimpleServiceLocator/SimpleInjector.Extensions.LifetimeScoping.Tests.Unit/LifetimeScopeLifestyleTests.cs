namespace SimpleInjector.Extensions.LifetimeScoping.Tests.Unit
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LifetimeScopeLifestyleTests
    {
        [TestMethod]
        public void Instance_Always_NotNull()
        {
            Assert.IsNotNull(LifetimeScopeLifestyle.Instance);
        }

        [TestMethod]
        public void Instance_Always_ReturnsTheSameInstance()
        {
            Assert.IsTrue(
                object.ReferenceEquals(LifetimeScopeLifestyle.Instance, LifetimeScopeLifestyle.Instance));
        }
    }
}
