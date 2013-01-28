namespace SimpleInjector.Tests.Unit
{
    using Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Tests.Unit;

    /// <content>Silverlight specific tests for injecting properties.</content>
    public partial class InjectPropertiesTests
    {
        [TestMethod]
        public void InjectProperties_InjectingPropertyInInternalClass_ThrowsExpectedExceptionMessage()
        {
            // Arrange
            string expectedMessage = "Unable to inject properties into type ";

            var container = new Container();

            container.Register<ITimeProvider, RealTimeProvider>();

            var instance = new InternalService();

            try
            {
                // Act
                container.InjectProperties(instance);

                Assert.Fail("Injection was expected to fail due to running in the Silverlight sandbox.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(expectedMessage, ex.Message);
            }
        }

        internal class InternalService
        {
            public ITimeProvider TimeProvider { get; set; }
        }
    }
}