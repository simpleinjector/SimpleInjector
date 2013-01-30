namespace SimpleInjector.Tests.Unit
{
    using NUnit.Framework;

    [TestFixture]
    public class ActivationExceptionTests
    {
        [Test]
        public void Ctor_Always_Succeeds()
        {
            // Act
            new ActivationException();
        }
    }
}