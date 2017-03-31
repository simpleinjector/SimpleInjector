namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    // Test test class contains the left over tests that don't fit into any of the other test classes.
    [TestClass]
    public class ContainerTests
    {
        public interface IService
        {
        }

        [TestMethod]
        public void Equals_OnSameInstance_ReturnsTrue()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var result = container.Equals(container);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_OnDifferentInstance_ReturnsFalse()
        {
            // Arrange
            var container1 = ContainerFactory.New();
            var container2 = ContainerFactory.New();

            // Act
            var result = container1.Equals(container2);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ToString_Always_ReturnsExpectedValue()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            string result = container.ToString();

            // Assert
            Assert.AreEqual("SimpleInjector.Container", result);
        }

        [TestMethod]
        public void GetHashCode_Always_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.GetHashCode();
        }

        [TestMethod]
        public void GetType_Always_ReturnsTheExpectedType()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Type type = container.GetType();

            // Assert
            Assert.AreEqual(typeof(Container), type);
        }

        [TestMethod]
        public void Dispose_RegisteredSingletonConcreteDisposable_DisposesThatInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<DisposableService>(Lifestyle.Singleton);

            var instance = container.GetInstance<DisposableService>();

            // Act
            container.Dispose();

            // Assert
            Assert.IsTrue(instance.Disposed);
        }

        [TestMethod]
        public void Dispose_RegisteredSingletonAbstractionForDisposableImplementation_DisposesThatInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IService, DisposableService>(Lifestyle.Singleton);

            var instance = container.GetInstance<IService>() as DisposableService;

            // Act
            container.Dispose();

            // Assert
            Assert.IsTrue(instance.Disposed);
        }

        [TestMethod]
        public void Dispose_RegistrationSingletonWithFactory_DisposesThatInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IService>(() => new DisposableService(), Lifestyle.Singleton);

            var instance = container.GetInstance<IService>() as DisposableService;

            // Act
            container.Dispose();

            // Assert
            Assert.IsTrue(instance.Disposed, 
                "When supplying a factory, the instances returned from that factory are considered to be " +
                "'container controlled' and the container should dispose them when its lifetime ends.");
        }

        [TestMethod]
        public void Dispose_RegistrationSingletonInstance_DoesNotDisposeThatInstance()
        {
            // Arrange
            DisposableService instance = new DisposableService();

            var container = ContainerFactory.New();

            container.RegisterSingleton<IService>(instance);

            container.GetInstance<IService>();

            // Act
            container.Dispose();

            // Assert
            Assert.IsFalse(instance.Disposed,
                "When supplying an already created instance to the container, such instance considered to be " +
                "'container uncontrolled' and the container should NOT dispose them, because the lifetime of " +
                "such instance exceeds that of the container.");
        }

        [TestMethod]
        public void Dispose_CalledMultipleTimes_DisposesInstancesJustOnce()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<DisposableService>(Lifestyle.Singleton);

            var instance = container.GetInstance<DisposableService>();

            // Act
            container.Dispose();
            container.Dispose();

            // Assert
            Assert.AreEqual(1, instance.DisposeCount);
        }

        [TestMethod]
        public void Dispose_MultipleDisposableSingletons_DisposesThemAll()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IService, DisposableService>(Lifestyle.Singleton);
            container.Register<DisposableService>(Lifestyle.Singleton);

            var instance1 = container.GetInstance<DisposableService>();
            var instance2 = container.GetInstance<IService>() as DisposableService;

            // Act
            container.Dispose();

            // Assert
            Assert.IsTrue(instance1.Disposed);
            Assert.IsTrue(instance2.Disposed);
        }
        
        [TestMethod]
        public void Dispose_MultipleDisposableSingletons_DisposesThemInOppositeOrderOfCreation()
        {
            // Arrange
            var instances = new List<DisposableService>();

            var container = ContainerFactory.New();

            container.Register<IService, DisposableService>(Lifestyle.Singleton);
            container.Register<DisposableService>(Lifestyle.Singleton);

            var instance1 = container.GetInstance<DisposableService>();
            var instance2 = container.GetInstance<IService>() as DisposableService;

            instance1.Disposing += instances.Add;
            instance2.Disposing += instances.Add;

            // Act
            container.Dispose();

            // Assert
            Assert.AreSame(instance2, instances[0], "Instances are expected to be disposed in opposite order.");
            Assert.AreSame(instance1, instances[1], "Instances are expected to be disposed in opposite order.");
        }
        
        [TestMethod]
        public void Dispose_ResolvedDisposableTransient_DoesNotDisposeInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IService, DisposableService>(Lifestyle.Transient);

            var instance1 = container.GetInstance<DisposableService>();

            // Act
            container.Dispose();

            // Assert
            Assert.IsFalse(instance1.Disposed, 
                "Transients should not get disposed. That would keep them alive for the lifetime of the container.");
        }

        [TestMethod]
        public void Dispose_ResolvedDisposableSingletonForExternalInstanceProducer_DisposesInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            var producer = Lifestyle.Singleton.CreateProducer<DisposableService, DisposableService>(container);

            var instance = producer.GetInstance();

            Assert.IsFalse(instance.Disposed, "Test setup failed.");

            // Act
            container.Dispose();

            // Assert
            Assert.IsTrue(instance.Disposed);
        }

        [TestMethod]
        public void GetInstanceOfType_OnDisposedContainer_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Dispose();

            // Act
            Action action = () => container.GetInstance<NullLogger>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ObjectDisposedException>(
                "Cannot access a disposed object.",
                action);
        }

        [TestMethod]
        public void GetInstance_OnDisposedContainer_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Dispose();

            // Act
            Action action = () => container.GetInstance(typeof(NullLogger));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ObjectDisposedException>(
                "Cannot access a disposed object.",
                action);
        }

        [TestMethod]
        public void Register_OnDisposedContainer_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Dispose();

            // Act
            Action action = () => container.Register<NullLogger>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ObjectDisposedException>(
                "Cannot access a disposed object.",
                action);
        }

        public class DisposableService : IDisposable, IService
        {
            public bool Disposed { get; private set; }

            public int DisposeCount { get; private set; }

            public event Action<DisposableService> Disposing = _ => { };

            public void Dispose()
            {
                this.Disposing(this);
                this.DisposeCount += 1;
                this.Disposed = true;
            }
        }
    }
}