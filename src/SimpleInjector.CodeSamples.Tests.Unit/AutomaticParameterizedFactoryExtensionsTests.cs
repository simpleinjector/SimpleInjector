namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AutomaticParameterizedFactoryExtensionsTests
    {
        public interface IService
        {
        }
        
        public interface IParameterizedServiceFactory
        {
            IService CreateService(int a, string b, ICommand input);
        }
        
        [TestMethod]
        public void Test()
        {
            // Arrange
            var container = new Container();

            container.Options.EnableAutomaticParameterizedFactories();

            container.Register<ILogger, NullLogger>();

            container.RegisterParameterizedFactory<IParameterizedServiceFactory>();

            container.RegisterFactoryProduct<IService, ParameterizedService>();

            // Act
            var factory = container.GetInstance<IParameterizedServiceFactory>();

            var service = (ParameterizedService)factory.CreateService(3, "foo", new ConcreteCommand());

            // Assert
            Assert.AreEqual(3, service.X);
            Assert.AreEqual("foo", service.Y);
            Assert.IsNotNull(service.Input);
            Assert.IsNotNull(service.Logger);
        }

        public class ParameterizedService : IService
        {
            public readonly int X;
            public readonly string Y;
            public readonly ICommand Input;
            public readonly ILogger Logger;

            public ParameterizedService(int x, string y, ILogger logger, ICommand input)
            {
                this.X = x;
                this.Y = y;
                this.Logger = logger;
                this.Input = input;
            }
        }
    }
}