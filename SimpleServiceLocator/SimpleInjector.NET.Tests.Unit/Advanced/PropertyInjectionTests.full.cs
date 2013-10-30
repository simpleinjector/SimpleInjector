namespace SimpleInjector.Tests.Unit.Advanced
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class PropertyInjectionTests
    {
        [TestMethod]
        public void InjectingAllProperties_OnPrivateTypeWithPrivateSetterProperty_Succeeds()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.RegisterSingle<ITimeProvider, RealTimeProvider>();

            // Act
            var service = container.GetInstance<PrivateServiceWithPrivateSetPropertyDependency<ITimeProvider>>();

            // Assert
            Assert.IsNotNull(service.Dependency);
        }
    }
}