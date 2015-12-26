namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class AutomaticFactoryExtensionsTests
    {
        public interface IService
        {
        }

        public interface IServiceFactory
        {
            IService CreateService();
        }

        [TestMethod]
        public void GetInstance_TypeRegisteredAsFactory_ResolvesAsExpected()
        {
            // Arrange
            var container = new Container();

            container.RegisterFactory<IServiceFactory>();

            // Act
            var factory = container.GetInstance<IServiceFactory>();

            // Assert
            Assert.IsNotNull(factory);
        }

        [TestMethod]
        public void CallingFactoryMethod_OnInstanceResolvedAsAutomaticFactory_ResolvesExpectedService()
        {
            // Arrange
            var container = new Container();

            container.Register<IService, Service>();

            container.RegisterFactory<IServiceFactory>();

            var factory = container.GetInstance<IServiceFactory>();

            // Act
            var instance = factory.CreateService();

            // Assert
            AssertThat.IsInstanceOfType(typeof(Service), instance);
        }
        
        [TestMethod]
        public void ToString_OnInstanceResolvedAsAutomaticFactory_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<IService, Service>();

            container.RegisterFactory<IServiceFactory>();

            var factory = container.GetInstance<IServiceFactory>();

            // Act
            factory.ToString();
        }

        public class Service : IService 
        {
        }
    }
}