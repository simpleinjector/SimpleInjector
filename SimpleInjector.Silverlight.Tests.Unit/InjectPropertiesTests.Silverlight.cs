namespace SimpleInjector.Tests.Unit
{
    using Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
#pragma warning disable 618
                container.InjectProperties(instance);
#pragma warning restore 618

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