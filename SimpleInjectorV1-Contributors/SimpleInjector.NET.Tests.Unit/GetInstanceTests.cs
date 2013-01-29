namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class GetInstanceTests
    {
        [Test]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceByType_CalledOnUnregisteredInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.GetInstance(typeof(ServiceWithUnregisteredDependencies));
        }

        [Test]
        public void GetInstanceByType_CalledOnRegisteredButInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = new Container();

            container.Register<ServiceWithUnregisteredDependencies>();

            try
            {
                // Act
                container.GetInstance(typeof(ServiceWithUnregisteredDependencies));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(@"
                    The constructor of the type ServiceWithUnregisteredDependencies contains the parameter 
                    of type IDisposable with name 'a' that is not registered."
                    .TrimInside(),
                    ex.Message);
            }
        }

        [Test]
        public void GetInstanceByType_CalledOnUnregisteredConcreteButInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.GetInstance(typeof(ServiceWithUnregisteredDependencies));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(@"
                    The constructor of the type ServiceWithUnregisteredDependencies contains the parameter 
                    of type IDisposable with name 'a' that is not registered."
                    .TrimInside(),
                    ex.Message);
            }
        }

        [Test]
        public void GetInstanceGeneric_CalledOnUnregisteredInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.GetInstance<ServiceWithUnregisteredDependencies>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(@"
                    The constructor of the type ServiceWithUnregisteredDependencies contains the parameter 
                    of type IDisposable with name 'a' that is not registered."
                    .TrimInside(),
                    ex.Message);
            }
        }

        [Test]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstanceGeneric_CalledOnRegisteredInvalidServiceType_ThrowsActivationException()
        {
            // Arrange
            var container = new Container();

            container.Register<ServiceWithUnregisteredDependencies>();

            // Act
            container.GetInstance<ServiceWithUnregisteredDependencies>();
        }

        [Test]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstance_OnObjectWhileUnregistered_ThrowsActivationException()
        {
            // Arrange
            var container = new Container();

            // Act
            container.GetInstance<object>();
        }

        [Test]
        public void GetInstanceType_DeeplyNestedGenericTypeWithInternalConstructor_ThrowsExceptionWithProperFriendlyTypeName()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.GetInstance(typeof(SomeGenericNastyness<>.ReadOnlyDictionary<,>.KeyCollection));

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(
                    "GetInstanceTests+SomeGenericNastyness<TBla>+ReadOnlyDictionary<TKey, TValue>+KeyCollection",
                    ex.Message);
            }
        }

        [Test]
        public void GetInstance_WithNastyOpenGenericType_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            // Lazy<Func<TResult>>
            var nastyOpenGenericType = typeof(Lazy<>).MakeGenericType(typeof(Func<>));

            try
            {
                // Act
                container.GetInstance(nastyOpenGenericType);
            }
            catch (ActivationException ex)
            {
                AssertThat.ExceptionMessageContains(
                    "No registration for type Lazy<Func<TResult>> could be found.", ex);
            }
        }

        [Test]
        public void GetAllInstances_WithOpenGenericEnumerableType_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.GetAllInstances(typeof(IEnumerable<>));
            }
            catch (ActivationException ex)
            {
                AssertThat.ExceptionMessageContains(
                    "No registration for type IEnumerable<IEnumerable<T>> could be found.", ex);
            }
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