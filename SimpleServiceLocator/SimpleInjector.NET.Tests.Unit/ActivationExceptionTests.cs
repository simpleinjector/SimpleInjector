using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.Tests.Unit
{
    [TestClass]
    public class ActivationExceptionTests
    {
        [TestMethod]
        public void Ctor_Always_Succeeds()
        {
            // Act
            new ActivationException();
        }
    }
}