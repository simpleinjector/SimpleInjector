namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Extensions;

    /// <summary>
    /// Variance Scenario 1: Single registration, single resolve.
    /// Per service, a single type is registered and a single instance is always requested.
    /// If that requested instance is missing, the nearest compatible registered type is returned.
    /// </summary>
    [TestClass]
    public class VarianceExtensions_AllowToResolveVariantTypesTests
    {
        private static Container CreateContainer()
        {
            var container = new Container();
            container.Options.AllowToResolveVariantTypes();
            return container;
        }

        public interface IInvariantInterface<T>
        {
        }

        public interface IEventHandler<in TEvent>
        {
            void Handle(TEvent e);
        }

        [TestMethod]
        public void GetInstance_RequestingAnUnregisteredTypeThatCanBeResolvedUsingContravariance_ReturnsExpectedType()
        {
            // Arrange
            var container = CreateContainer();

            container.Register<IEventHandler<CustomerMovedEvent>, CustomerMovedEventHandler>();

            // Act
            var handler = container.GetInstance<IEventHandler<CustomerMovedAbroadEvent>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(CustomerMovedEventHandler));
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstance_RequestingAnInvariantInterface_WillNotReturnTheRegisteredInstance()
        {
            // Arrange
            var container = CreateContainer();

            container.RegisterSingle<IInvariantInterface<CustomerMovedEvent>>(
                new InvariantClass<CustomerMovedEvent>());

            // Act
            container.GetInstance(typeof(IInvariantInterface<CustomerMovedAbroadEvent>));
        }

        [TestMethod]
        public void GetInstance_RequesingACovariantType_ReturnsTheExpectedFunc()
        {
            // Arrange
            var container = CreateContainer();

            // Note: Func<out T> contains an out parameter. We are testing covariance here.
            string expectedValue = "Hello covariance.";

            Func<string> expectedFunc = () => expectedValue;

            container.RegisterSingle<Func<string>>(expectedFunc);

            // Act
            var actualFunc = container.GetInstance<Func<object>>();

            object actualValue = actualFunc();

            // Assert
            Assert.AreEqual(expectedFunc, actualFunc);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void GetInstance_RequestingAnActionOfStringWhileActionOfObjectIsRegistered_ReturnsExpectedDelegate()
        {
            // Arrange
            string expectedValue = "some string";
            object actualValue = null;

            var container = CreateContainer();

            container.RegisterSingle<Action<object>>(s => { actualValue = s; });

            // Act
            var action = container.GetInstance<Action<string>>();

            action(expectedValue);

            // Assert
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void GetRegistration_RequestingAnActionOfObjectWhileActionOfStringIsRegistered_ReturnNull()
        {
            // Arrange
            var container = CreateContainer();

            container.RegisterSingle<Action<string>>(s => { });

            // Act
            var registration = container.GetRegistration(typeof(Action<object>));

            // Assert
            Assert.IsNull(registration, "No registration should have been found");
        }

        [TestMethod]
        public void GetInstance_RequestingATypeWithToCompatibleVariantTypes_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage =
                "There is an error in the container's configuration. " +
                "It is impossible to resolve type System.Action`1[System.ArgumentException], " +
                "because there are 2 registrations that are applicable. " +
                "Ambiguous registrations: ";

            var container = CreateContainer();

            container.RegisterSingle<Action<object>>(s => { });
            container.RegisterSingle<Action<Exception>>(s => { });

            try
            {
                // Act
                container.GetInstance<Action<ArgumentException>>();

                Assert.Fail("Call was expected to fail.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(expectedMessage), 
                    "Expected: " + expectedMessage + " Actual: " + ex.Message);
                Assert.IsTrue(ex.Message.Contains(typeof(Action<object>).Name));
                Assert.IsTrue(ex.Message.Contains(typeof(Action<Exception>).Name));
            }
        }

        public class InvariantClass<T> : IInvariantInterface<T>
        {
        }

        public class CustomerMovedEvent
        {
        }

        public class CustomerMovedAbroadEvent : CustomerMovedEvent
        {
        }

        public class CustomerMovedEventHandler : IEventHandler<CustomerMovedEvent>
        {
            public void Handle(CustomerMovedEvent e)
            {
            }
        }
    }
}