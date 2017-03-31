namespace SimpleInjector.Tests.Unit.Advanced
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests for property injection for the full .NET framework version.</summary>
    public partial class PropertyInjectionTests
    {
        [TestMethod]
        public void InjectingAllProperties_OnPrivateTypeWithPrivateSetterProperty_Succeeds()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            // Act
            var service = container.GetInstance<PrivateServiceWithPrivateSetPropertyDependency<ITimeProvider>>();

            // Assert
            Assert.IsNotNull(service.Dependency);
        }
    }
}