namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    public partial class PropertyInjectionTests
    {
        [TestMethod]
        public void InjectingAllProperties_OnPrivateTypeWithPrivateSetterPropertyInSilverlight_FailsWithDescriptiveMessage()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.RegisterSingle<ITimeProvider, RealTimeProvider>();

            try
            {
                // Act
                container.GetInstance<PrivateServiceWithPrivateSetPropertyDependency<ITimeProvider>>();
            }
            catch (Exception ex)
            {
                AssertThat.ExceptionMessageContains(@"
                    The security restrictions of your application's sandbox do not permit the injection of 
                    one of its properties.".TrimInside(), ex);
            }
        }
    }
}