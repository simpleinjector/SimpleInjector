namespace SimpleInjector.Tests.Unit.Extensions
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Extensions;

    public partial class OpenGenericRegistrationExtensionsTests
    {
        [TestMethod]
        public void GetInstance_OnInternalTypeRegisteredAsOpenGeneric_ThrowsDescriptiveExceptionMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterOpenGeneric(typeof(IEventHandler<>), typeof(InternalEventHandler<>));

            try
            {
                // Act
                container.GetInstance<IEventHandler<int>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.ExceptionMessageContains("InternalEventHandler<Int32>", ex);
                AssertThat.ExceptionMessageContains("The security restrictions of your application's " +
                    "sandbox do not permit the creation of this type.", ex);
            }
        }
    }
}