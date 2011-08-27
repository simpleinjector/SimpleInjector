using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SimpleInjector.Extensions;

namespace SimpleInjector.CodeSamples.Tests.Unit
{
    /// <summary>
    /// Variance Scenario 1: Single registration, single resolve.
    /// Per service, a single type is registered and and a single instance is always requested.
    /// If that requested instance is missing, the nearest compatible registered type is returned.
    /// </summary>
    [TestClass]
    public class VarianceExtensions_AllowToResolveVariantTypesTests
    {
        // Each test gets its own test class instance and therefore its own new container and logger.
        private readonly Container container = new Container();
        
        public VarianceExtensions_AllowToResolveVariantTypesTests()
        {
            this.container.AllowToResolveVariantTypes();
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
            this.container.Register<IEventHandler<CustomerMovedEvent>, CustomerMovedEventHandler>();

            // Act
            var handler = this.container.GetInstance<IEventHandler<SpecialCustomerMovedEvent>>();

            // Assert
            Assert.IsInstanceOfType(handler, typeof(CustomerMovedEventHandler));
        }

        [TestMethod]
        [ExpectedException(typeof(ActivationException))]
        public void GetInstance_RequestingAnInvariantInterface_WillNotReturnTheRegisteredInstance()
        {
            // Arrange
            this.container.RegisterSingle<IInvariantInterface<CustomerMovedEvent>>(
                new InvariantClass<CustomerMovedEvent>());

            // Act
            this.container.GetInstance(typeof(IInvariantInterface<SpecialCustomerMovedEvent>));
        }

        [TestMethod]
        public void GetInstance_RequesingACovariantType_ReturnsTheExpectedFunc()
        {
            // Arrange
            // Note: Func<out T> contains an out parameter. We are testing covariance here.
            string expectedValue = "Hello covariance.";

            Func<string> expectedFunc = () => expectedValue;

            this.container.RegisterSingle<Func<string>>(expectedFunc);

            // Act
            var actualFunc = this.container.GetInstance<Func<object>>();

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

            this.container.RegisterSingle<Action<object>>(s => { actualValue = s; });

            // Act
            var action = this.container.GetInstance<Action<string>>();

            action(expectedValue);

            // Assert
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void GetRegistration_RequestingAnActionOfObjectWhileActionOfStringIsRegistered_ReturnNull()
        {
            // Arrange
            this.container.RegisterSingle<Action<string>>(s => { });

            // Act
            var registration = this.container.GetRegistration(typeof(Action<object>));

            // Assert
            Assert.IsNull(registration, "No registration should have been found");
        }

        [TestMethod]
        public void GetInstance_RequestingATypeWithToCompatibleVariantTypes_ThrowsExpectedException()
        {
            // Arrange
            string expectedMessage =
                "There is an error in the container's contiguration. " +
                "It is impossible to resolve type System.Action`1[System.ArgumentException], " +
                "because there are 2 registrations that are applicable. " +
                "Ambiguous registrations: System.Action`1[System.Object], System.Action`1[System.Exception].";

            this.container.RegisterSingle<Action<object>>(s => { });
            this.container.RegisterSingle<Action<Exception>>(s => { });

            try
            {
                // Act
                this.container.GetInstance<Action<ArgumentException>>();

                Assert.Fail("Call was expected to fail.");
            }
            catch (ActivationException ex)
            {
                Assert.AreEqual(expectedMessage, ex.Message);
            }
        }

        public class InvariantClass<T> : IInvariantInterface<T>
        {
        }

        public class CustomerMovedEvent
        {
        }

        public class SpecialCustomerMovedEvent : CustomerMovedEvent
        {
        }

        public class CustomerMovedEventHandler : IEventHandler<CustomerMovedEvent>
        {
            public void Handle(CustomerMovedEvent e)
            {
            }
        }

        public class NotifyStaffWhenCustomerMovedEventHandler : IEventHandler<CustomerMovedEvent>
        {
            public void Handle(CustomerMovedEvent e)
            {
            }
        }
    }
}