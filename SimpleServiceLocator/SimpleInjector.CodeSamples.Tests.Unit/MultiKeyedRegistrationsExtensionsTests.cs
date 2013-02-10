namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Extensions;

    [TestClass]
    public class MultiKeyedRegistrationsExtensionsTests
    {
        public interface IService1 
        {
        }

        public interface IService2 
        {
        }

        [TestMethod]
        public void Register_WithValidArguments_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Register<IService1, IService2, ImplementsBothInterfaces>(Lifestyle.Transient);
        }

        [TestMethod]
        public void GetInstance_OnFirstServiceType_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<IService1, IService2, ImplementsBothInterfaces>(Lifestyle.Transient);

            // Act
            container.GetInstance<IService1>();
        }


        [TestMethod]
        public void GetInstance_OnSecondServiceType_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<IService1, IService2, ImplementsBothInterfaces>(Lifestyle.Transient);

            // Act
            container.GetInstance<IService2>();
        }


        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior4()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Register<IService1, IService2, ImplementsBothInterfaces>(Lifestyle.Transient);

            // Assert
            Assert.IsFalse(container.GetCurrentRegistrations().Any(r => r.ServiceType == typeof(ImplementsBothInterfaces)));
        }



        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior5()
        {
            // Arrange
            var container = new Container();

            container.Register<IService1, IService2, ImplementsBothInterfaces>(Lifestyle.Singleton);

            // Act
            var service1 = container.GetInstance<IService1>();
            var service2 = container.GetInstance<IService2>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(service1, service2));
        }

        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior6()
        {
            // Arrange
            const int ExpectedCallCount = 1;
            int actualCallCount = 0;
            
            var container = new Container();

            container.Register<IService1, IService2, ImplementsBothInterfaces>(Lifestyle.Singleton);

            container.RegisterInitializer<IService2>(service => { actualCallCount++; });

            // Act
            container.GetInstance<IService2>();
            container.GetInstance<IService2>();
            container.GetInstance<IService2>();

            // Assert
            Assert.AreEqual(ExpectedCallCount, actualCallCount);
        }

        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior7()
        {
            // Arrange
            const int ExpectedCallCount = 1;
            int actualCallCount = 0;

            var container = new Container();

            container.Register<IService1, IService2, ImplementsBothInterfaces>(Lifestyle.Singleton);

            container.RegisterInitializer<IService1>(service => { actualCallCount++; });

            // Act
            container.GetInstance<IService2>();
            container.GetInstance<IService2>();

            // Assert
            Assert.AreEqual(ExpectedCallCount, actualCallCount);
        }

        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior8()
        {
            // Arrange
            var container = new Container();

            container.Register<IService1, IService2, ImplementsBothInterfaces>(Lifestyle.Singleton);

            container.RegisterDecorator(typeof(IService2), typeof(Service2Decorator));

            // Act
            var service = container.GetInstance<IService2>();
            
            // Assert
            Assert.IsInstanceOfType(service, typeof(Service2Decorator));
        }


        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior9()
        {
            // Arrange
            var container = new Container();

            container.Register<IService1, IService2, ImplementsBothInterfaces>(Lifestyle.Singleton);

            container.RegisterDecorator(typeof(IService2), typeof(Service2Decorator));

            // Act
            var service1 = container.GetInstance<IService1>();
            var decorator = (Service2Decorator)container.GetInstance<IService2>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(service1, decorator.DecoratedService));
        }

        public class ImplementsBothInterfaces : IService1, IService2 
        {
        }

        public class Service2Decorator : IService2
        {
            public Service2Decorator(IService2 service)
            {
                this.DecoratedService = service;
            }

            public IService2 DecoratedService { get; private set; }
        }
    }
}