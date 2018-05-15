namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ContainerCollectionsCreateRegistrationTests
    {
        [TestMethod]
        public void CreateRegistration_CalledTwiceForSameCollectionType_ReturnsSeparateInstances()
        {
            // Arrange
            var expectedTypes = new[] { typeof(NullLogger), typeof(ConsoleLogger) };

            var container = new Container();

            // Act
            var reg1 = container.Collection.CreateRegistration<ILogger>(typeof(NullLogger));
            var reg2 = container.Collection.CreateRegistration<ILogger>(typeof(ConsoleLogger));

            // Assert
            Assert.AreNotSame(reg1, reg2,
                "Registration caching should not have been applied here, since that would lead to incorrect results");
        }

        [TestMethod]
        public void CreateRegistration_CalledTwiceForSameCollectionType_ProducesStreamsThatProduceInstanceOfTheExpectedRegistrations()
        {
            // Arrange
            var expectedTypes = new[] { typeof(NullLogger), typeof(ConsoleLogger) };

            var container = new Container();

            // Act
            var reg1 = container.Collection.CreateRegistration<ILogger>(typeof(NullLogger));
            var reg2 = container.Collection.CreateRegistration<ILogger>(typeof(ConsoleLogger));

            var prod1 = new InstanceProducer<IEnumerable<ILogger>>(reg1);
            var prod2 = new InstanceProducer<IEnumerable<ILogger>>(reg2);

            var stream1 = GetStreamFromRegistration<ILogger>(container, typeof(NullLogger));
            var stream2 = GetStreamFromRegistration<ILogger>(container, typeof(ConsoleLogger));

            // Assert
            Assert.IsInstanceOfType(stream1.First(), typeof(NullLogger));
            Assert.IsInstanceOfType(stream2.First(), typeof(ConsoleLogger));
        }

        // DESIGN: This behavior is questionable, because Registration objects themselves actually never cause
        // verification, only InstanceProducers do. So this is actually a design quirk, but I'll leave it in
        // for now, because it made implementing this feature so much easier.
        [TestMethod]
        public void Verify_WhenRegistrationIsCreatedForTypeThatFailsDuringCreation_VerifyTestsTheCollection()
        {
            // Arrange
            var container = new Container();

            var registration = container.Collection.CreateRegistration<ILogger>(typeof(FailingConstructorLogger));

            // Notice the explicit call to GC.Collect(). Simple Injector holds on to 'stuff' using WeakReferences
            // to ensure that to memory is leaked, but as long as stream is referenced, should it as well be
            // verified
            GC.Collect();

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                expectedMessage: nameof(FailingConstructorLogger),
                action: action);

            GC.KeepAlive(registration);
        }

        private static IEnumerable<T> GetStreamFromRegistration<T>(
            Container container, params Type[] serviceTypes) where T : class
        {
            var reg = container.Collection.CreateRegistration<T>(serviceTypes);
            var prod = new InstanceProducer<IEnumerable<T>>(reg);
            return prod.GetInstance();
        }
    }
}