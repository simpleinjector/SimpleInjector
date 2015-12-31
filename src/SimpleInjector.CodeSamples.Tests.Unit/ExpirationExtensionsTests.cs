namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExpirationExtensionsTests
    {
        // DateTime.UtcNow and DateTime.Now only have a precision up to about 15 ms. But since it
        // depends on the machine, we take 30 ms. Just to make the tests trustworthy.
        private static readonly TimeSpan OneTimeUnit = TimeSpan.FromMilliseconds(30);
        private static readonly TimeSpan TwoTimeUnits = new TimeSpan(OneTimeUnit.Ticks * 2);
        private static readonly TimeSpan ThreeTimeUnits = new TimeSpan(OneTimeUnit.Ticks * 3);

        [TestMethod]
        public void RegisterWithAbsoluteExpiration_WithValidArguments_Succeeds()
        {
            // Arrange
            var container = new Container();

            var validTimeout = TimeSpan.FromMinutes(1);

            // Act
            container.RegisterWithAbsoluteExpiration<ICommand, ConcreteCommand>(validTimeout);
        }

        [TestMethod]
        public void GetInstance_OnInstanceRegisteredWithRegisterWithAbsoluteExpiration_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithAbsoluteExpiration<ICommand, ConcreteCommand>(TimeSpan.FromMinutes(1));

            // Act
            container.GetInstance<ICommand>();
        }

        [TestMethod]
        public void GetInstance_CalledSecondTimeBeforeAbsoluteExpirationTimesOut_ReturnsSameInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithAbsoluteExpiration<ICommand, ConcreteCommand>(TimeSpan.FromMinutes(1));

            // Act
            var instance1 = container.GetInstance<ICommand>();
            var instance2 = container.GetInstance<ICommand>();

            // Assert
            Assert.AreEqual(instance1, instance2, "The same instance was expected to be returned.");
        }

        [TestMethod]
        public void GetInstance_CalledSecondTimeBeforeAbsoluteExpirationTimesOut2_ReturnsSameInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithAbsoluteExpiration<ICommand, ConcreteCommand>(ThreeTimeUnits);

            // Act
            WaitFor(TwoTimeUnits);

            // Timing only begins during first call to GetInstance.
            var instance1 = container.GetInstance<ICommand>();

            WaitFor(TwoTimeUnits);

            var instance2 = container.GetInstance<ICommand>();

            // Assert
            Assert.AreEqual(instance1, instance2, string.Format("Timing only begins during first call to " + 
                "GetInstance. The timeout is {0} ms. and {1} ms. is within this timeout. We should get the " +
                "same instance.", ThreeTimeUnits, TwoTimeUnits));
        }

        [TestMethod]
        public void GetInstance_CalledSecondTimeAfterAbsoluteExpirationTimedOut_ReturnsANewInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithAbsoluteExpiration<ICommand, ConcreteCommand>(OneTimeUnit);

            // Act
            var instance1 = container.GetInstance<ICommand>();

            WaitFor(TwoTimeUnits);

            var instance2 = container.GetInstance<ICommand>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(instance1, instance2), 
                "A new instance was expected to be returned when it is requested after it timed out.");
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesWithinTimeoutPeriod_WillReturnANewInstanceAfterTheTimeoutPeriod()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithAbsoluteExpiration<ICommand, ConcreteCommand>(ThreeTimeUnits);

            // Act
            var instance1 = container.GetInstance<ICommand>();

            for (int i = 0; i < 5; i++)
            {
                WaitFor(OneTimeUnit);
                
                container.GetInstance<ICommand>();
            }

            var instance2 = container.GetInstance<ICommand>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(instance1, instance2),
                "The new instance is expected to be returned, because of the absolute timeout period. ");
        }

        [TestMethod]
        public void RegisterWithSlidingExpiration_WithValidArguments_Succeeds()
        {
            // Arrange
            var container = new Container();

            var validTimeout = TimeSpan.FromMinutes(1);

            // Act
            container.RegisterWithSlidingExpiration<ICommand, ConcreteCommand>(validTimeout);
        }

        [TestMethod]
        public void GetInstance_OnInstanceRegisteredWithRegisterWithRegisterWithSlidingExpiration_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithSlidingExpiration<ICommand, ConcreteCommand>(TimeSpan.FromMinutes(1));

            // Act
            container.GetInstance<ICommand>();
        }

        [TestMethod]
        public void GetInstance_CalledSecondTimeBeforeSlidingExpirationTimesOut_ReturnsSameInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithSlidingExpiration<ICommand, ConcreteCommand>(TimeSpan.FromMinutes(1));

            // Act
            var instance1 = container.GetInstance<ICommand>();
            var instance2 = container.GetInstance<ICommand>();

            // Assert
            Assert.AreEqual(instance1, instance2, "The same instance was expected to be returned.");
        }

        [TestMethod]
        public void GetInstance_CalledSecondTimeBeforeSlidingExpirationTimesOut2_ReturnsSameInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithSlidingExpiration<ICommand, ConcreteCommand>(ThreeTimeUnits);

            // Act
            WaitFor(TwoTimeUnits);

            // Timing only begins during first call to GetInstance.
            var instance1 = container.GetInstance<ICommand>();

            WaitFor(TwoTimeUnits);

            var instance2 = container.GetInstance<ICommand>();

            // Assert
            Assert.AreEqual(instance1, instance2, string.Format("Timing only begins during first call to " +
                "GetInstance. The timeout is {0} ms. and {1} ms. is within this timeout. We should get the " +
                "same instance.", ThreeTimeUnits, TwoTimeUnits));
        }

        [TestMethod]
        public void GetInstance_CalledSecondTimeAfterSlidingExpirationTimedOut_ReturnsANewInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithSlidingExpiration<ICommand, ConcreteCommand>(OneTimeUnit);

            // Act
            var instance1 = container.GetInstance<ICommand>();

            WaitFor(TwoTimeUnits);

            var instance2 = container.GetInstance<ICommand>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(instance1, instance2),
                "A new instance was expected to be returned when it is requested after it timed out.");
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesWithinTimeoutPeriod_AlwaysReturnsSameInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithSlidingExpiration<ICommand, ConcreteCommand>(ThreeTimeUnits);

            for (int i = 0; i < 10; i++)
            {
                // Act
                WaitFor(OneTimeUnit);

                var instance1 = container.GetInstance<ICommand>();

                WaitFor(OneTimeUnit);

                var instance2 = container.GetInstance<ICommand>();

                // Assert
                Assert.IsTrue(object.ReferenceEquals(instance1, instance2),
                    "The same instance is expected to be returned, because of the sliding period. " + i);
            }
        }

        private static void WaitFor(TimeSpan timespan)
        {
            Thread.Sleep((int)timespan.TotalMilliseconds);
        }
    }
}