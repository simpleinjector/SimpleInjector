namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetRegistrationTests
    {
        [TestMethod]
        public void GetRegistration_Always_LocksTheContainer1()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.GetRegistration(typeof(ITimeProvider));

            try
            {
                // Act
                container.Register<ITimeProvider, RealTimeProvider>();

                // Assert
                Assert.Fail("The container should get locked during the call to GetRegistration, because a " +
                    "user can call the GetInstance() and BuildExpression() methods on the returned instance. " +
                    "BuildExpression can internally call GetInstance and the first call to GetInstance should " +
                    "always lock the container for reasons of correctness.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains("container can't be changed", ex);
            }
        }

        [TestMethod]
        public void GetRegistration_Always_LocksTheContainer2()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.GetRegistration(typeof(ITimeProvider), throwOnFailure: false);

            try
            {
                // Act
                container.Register<ITimeProvider, RealTimeProvider>();

                // Assert
                Assert.Fail("The container should get locked during the call to GetRegistration, because a " +
                    "user can call the GetInstance() and BuildExpression() methods on the returned instance. " +
                    "BuildExpression can internally call GetInstance and the first call to GetInstance should " +
                    "always lock the container for reasons of correctness.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains("container can't be changed", ex);
            }
        }

        [TestMethod]
        public void GetRegistration_Always_LocksTheContainer3()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                container.GetRegistration(typeof(ITimeProvider), throwOnFailure: true);
            }
            catch
            {
                // Exception expected.
            }

            try
            {
                // Act
                container.Register<ITimeProvider, RealTimeProvider>();

                // Assert
                Assert.Fail("The container should get locked during the call to GetRegistration, because a " +
                    "user can call the GetInstance() and BuildExpression() methods on the returned instance. " +
                    "BuildExpression can internally call GetInstance and the first call to GetInstance should " +
                    "always lock the container for reasons of correctness.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains("container can't be changed", ex);
            }
        }

        [TestMethod]
        public void GetRegistration_TransientInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ITimeProvider, RealTimeProvider>();

            // Act
            var provider = container.GetRegistration(typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_TransientConcreteInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<RealTimeProvider>();

            // Act
            var provider = container.GetRegistration(typeof(RealTimeProvider));

            // Assert
            Assert.AreEqual(typeof(RealTimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_FuncTransientInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ITimeProvider>(() => new RealTimeProvider());

            // Act
            var provider = container.GetRegistration(typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_ImplicitlyRegisteredTransientInstance_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.GetInstance<RealTimeProvider>();

            // Act
            var provider = container.GetRegistration(typeof(RealTimeProvider));

            // Assert
            Assert.AreEqual(typeof(RealTimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_SingleInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            // Act
            var provider = container.GetRegistration(typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_SingleConcreteInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<RealTimeProvider>(Lifestyle.Singleton);

            // Act
            var provider = container.GetRegistration(typeof(RealTimeProvider));

            // Assert
            Assert.AreEqual(typeof(RealTimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_FuncSingleInstanceRegistered_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ITimeProvider>(() => new RealTimeProvider(), Lifestyle.Singleton);

            // Act
            var provider = container.GetRegistration(typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_SingleInstanceRegisteredByObject_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingleton<ITimeProvider>(new RealTimeProvider());

            // Act
            var provider = container.GetRegistration(typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_InstanceResolvedUsingUnregisteredTypeResolution_ReturnsExpectedRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.ResolveUnregisteredType += (sender, e) =>
            {
                e.Register(() => new RealTimeProvider());
            };

            container.GetInstance<ITimeProvider>();

            // Act
            var provider = container.GetRegistration(typeof(ITimeProvider));

            // Assert
            Assert.AreEqual(typeof(ITimeProvider), provider.ServiceType);
        }

        [TestMethod]
        public void GetRegistration_OnUnregisteredUnconstructableType_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var registration = container.GetRegistration(typeof(IDisposable));

            // Assert
            Assert.IsNull(registration);
        }

        [TestMethod]
        public void GetRegistrationDontThrowOnFailure_OnUnregisteredUnconstructableType_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var registration = container.GetRegistration(typeof(IDisposable), throwOnFailure: false);

            // Assert
            Assert.IsNull(registration);
        }

        [TestMethod]
        public void GetRegistrationDoThrowOnFailure_OnUnregisteredUnconstructableType_Throws()
        {
            // Arrange
            var container = ContainerFactory.New();

            try
            {
                // Act
                var registration = container.GetRegistration(typeof(IDisposable), throwOnFailure: true);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains("No registration for type IDisposable could be found.", ex.Message);
            }
        }

        [TestMethod]
        public void GetRegistration_OnOnregisteredString_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var registration = container.GetRegistration(typeof(string));

            // Assert
            Assert.IsNull(registration);
        }

        [TestMethod]
        public void GetRegistration_OnValueType_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var registration = container.GetRegistration(typeof(int));

            // Assert
            Assert.IsNull(registration);
        }

        [TestMethod]
        public void GetRegistration_OnInvalidUnregisteredType_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var registration = container.GetRegistration(typeof(ServiceWithUnregisteredDependencies));

            // Assert
            Assert.IsNull(registration, "IInstanceProducer returned.");
        }

        [TestMethod]
        public void GetRegistration_OnInvalidButRegisteredType_ReturnsThatRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ServiceWithUnregisteredDependencies>();

            // Act
            var registration = container.GetRegistration(typeof(ServiceWithUnregisteredDependencies));

            // Assert
            Assert.IsNotNull(registration, "The GetRegistration method is expected to return an " +
                "InstanceProducer since it is explicitly registered by the user.");
        }

        [TestMethod]
        public void GetRegistration_OnOpenGenericType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetRegistration(typeof(IEnumerable<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "IEnumerable<T> is invalid because it is an open generic type",
                action);
        }

        [TestMethod]
        public void GetRegistration_DeeplyNestedGenericTypeWithInternalConstructor_ReturnsNull()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var registration = container.GetRegistration(
                typeof(SomeGenericNastyness<int>.ReadOnlyDictionary<object, string>.KeyCollection));

            // Assert
            Assert.IsNull(registration);
        }

        public class SomeGenericNastyness<TBla>
        {
            public class ReadOnlyDictionary<TKey, TValue>
            {
                public sealed class KeyCollection
                {
                    internal KeyCollection(ICollection<TKey> collection)
                    {
                    }
                }
            }
        }
    }
}