namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetRegistrationTests
    {
        [TestMethod]
        public void GetRegistration_RequestingAnExplicitlyRegisteredType_DoesNotLockTheContainer()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ITimeProvider, RealTimeProvider>();

            // Act
            // An explicitly made registration is returned from the internal root cache
            var prod = container.GetRegistration(typeof(ITimeProvider));

            Assert.IsNotNull(prod, "Test setup failed");

            // Act
            Assert.IsFalse(container.IsLocked,
                "No internal container state has been changed, and the container doesn't have to be locked.");
        }

        [TestMethod]
        public void GetRegistration_WhenTheRegistrationIsUnknownAndCanNotBeCreated_LocksTheContainer()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var prod = container.GetRegistration(typeof(ITimeProvider));

            Assert.IsNull(prod, "Test setup failed");

            Assert.IsTrue(container.IsLocked, @"
                Even if GetRegistration returns null, and no InstanceProducer is built, the container
                should be locked, because all kinds of things could have happened on the background, such as
                the invocation of unregistered type resolution event handlers.");
        }

        [TestMethod]
        public void GetRegistration_WhenTheRegistrationIsUnknownButCanBeCreated_LocksTheContainer()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var prod = container.GetRegistration(typeof(RealTimeProvider));

            Assert.IsNotNull(prod, "Test setup failed.");

            // Arrange
            Assert.IsTrue(container.IsLocked, @"
                Whenever a not explicitly made registration can be returned, the container needs to be locked,
                since in most cases, changing the container might invalidate the registration. For instance,
                building concrete type registrations will create a transient registration, while making the
                registration later might be done with a different lifestyle.");
        }

        [TestMethod]
        public void GetRegistration_WhenTheRegistrationIsUnknownButCanBeCreatedUsingUnregTypeRes_DoesNotLockTheContainer()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Not sure whether it would be wise to always lock the container when an unregistered event handler is fired.
            container.ResolveUnregisteredType += (s, e) =>
            {
                e.Register(Lifestyle.Singleton.CreateRegistration<RealTimeProvider>(container));
            };

            // Act
            var prod = container.GetRegistration(typeof(ITimeProvider));

            Assert.IsNotNull(prod, "Test setup failed.");

            // Arrange
            Assert.IsTrue(container.IsLocked, @"
            Whenever a not explicitly made registration can be returned using unregistered type resolution,
            the container needs to be locked, changing the container might invalidate the registration.
            For instance, adding unregistered type resolution events later, might cause a different registration to
            be returned when GetRegistration is called again.");
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

            container.RegisterInstance<ITimeProvider>(new RealTimeProvider());

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
                "IEnumerable<T> is invalid because it is an open-generic type",
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

        public static class SomeGenericNastyness<TBla>
        {
            public static class ReadOnlyDictionary<TKey, TValue>
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