namespace SimpleInjector.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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