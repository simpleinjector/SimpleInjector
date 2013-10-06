namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetInstanceTests
    {
        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByType_CalledOnUnregisteredInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.GetInstance(typeof(ServiceWithUnregisteredDependencies));
        }

        [TestMethod]
        public void GetInstanceByType_CalledOnRegisteredButInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ServiceWithUnregisteredDependencies>();

            // Act
            Action action = () => container.GetInstance(typeof(ServiceWithUnregisteredDependencies));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The constructor of the type ServiceWithUnregisteredDependencies contains the parameter 
                    of type IDisposable with name 'a' that is not registered."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstanceByType_CalledOnUnregisteredConcreteButInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetInstance(typeof(ServiceWithUnregisteredDependencies));
            
            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The constructor of the type ServiceWithUnregisteredDependencies contains the parameter 
                of type IDisposable with name 'a' that is not registered."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstanceGeneric_CalledOnUnregisteredInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetInstance<ServiceWithUnregisteredDependencies>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The constructor of the type ServiceWithUnregisteredDependencies contains the parameter 
                of type IDisposable with name 'a' that is not registered."
                .TrimInside(),
                action);
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceGeneric_CalledOnRegisteredInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ServiceWithUnregisteredDependencies>();

            // Act
            container.GetInstance<ServiceWithUnregisteredDependencies>();
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstance_OnObjectWhileUnregistered_ThrowsActivationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.GetInstance<object>();
        }

        [TestMethod]
        public void GetInstanceType_DeeplyNestedGenericTypeWithInternalConstructor_ThrowsExceptionWithProperFriendlyTypeName()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = 
                () => container.GetInstance(typeof(SomeGenericNastyness<>.ReadOnlyDictionary<,>.KeyCollection));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "GetInstanceTests+SomeGenericNastyness<TBla>+ReadOnlyDictionary<TKey, TValue>+KeyCollection", 
                action);
        }

        [TestMethod]
        public void GetInstance_WithNastyOpenGenericType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Lazy<Func<TResult>>
            var nastyOpenGenericType = typeof(Lazy<>).MakeGenericType(typeof(Func<>));

            // Act
            Action action = () => container.GetInstance(nastyOpenGenericType);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "No registration for type Lazy<Func<TResult>> could be found.", action);
        }

        [TestMethod]
        public void GetAllInstances_WithOpenGenericEnumerableType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            Action action = () => container.GetAllInstances(typeof(IEnumerable<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "No registration for type IEnumerable<IEnumerable<T>> could be found.", action);
        }

        [TestMethod]
        public void GetInstance_RegisteredConcreteTypeWithMissingDependency_DoesNotThrowMessageAboutMissingRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<FakeUserService>();

            // Act
            Action action = () => container.GetInstance<FakeUserService>();
            
            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "No registration for type FakeUserService could be found", action);
        }

        [TestMethod]
        public void GetInstance_RegisteredAbstractionWithImplementationWithMissingDependency_DoesNotThrowMessageAboutMissingRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<UserServiceBase, FakeUserService>();

            // Act
            Action action = () => container.GetInstance<UserServiceBase>();
            
            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "No registration for type FakeUserService could be found", action);
        }

        [TestMethod]
        public void GetInstance_NonRootTypeRegistrationWithMissingDependency_DoesNotThrowMessageAboutMissingRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            // FakeUserService depends on IUserRepository
            container.Register<UserServiceBase, FakeUserService>();

            // SomeUserRepository depends on IPlugin, but that isn't registered
            container.Register<IUserRepository, PluginDependantUserRepository>();

            // Act
            Action action = () => container.GetInstance<UserServiceBase>();
            
            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "No registration for type PluginDependantUserRepository could be found", action);
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithMissingDependency_ThrowsExceptionAboutMissingRegistration()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            // FakeUserService depends on IUserRepository but this abstraction is not registered.
            Action action = () => container.GetInstance<FakeUserService>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                No registration for type FakeUserService could be found and an implicit registration 
                could not be made."
                .TrimInside(),
                action);
        }

        //// Seems like there are tests missing, but all other cases are already covered by other test classes.

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