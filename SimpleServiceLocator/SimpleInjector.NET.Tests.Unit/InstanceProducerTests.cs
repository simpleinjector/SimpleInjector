namespace SimpleInjector.Tests.Unit
{
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Advanced;
    using SimpleInjector.Extensions;

    [TestClass]
    public class InstanceProducerTests
    {
        public interface IOne
        {
        }

        public interface ITwo
        {
        }

        [TestMethod]
        public void GetInstance_Always_LocksTheContainer()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration = container.GetRegistration(typeof(Container));

            // Act
            registration.GetInstance();

            // Assert
            Assert.IsTrue(container.IsLocked());
        }

        [TestMethod]
        public void BuildExpression_Always_LocksTheContainer()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration = container.GetRegistration(typeof(Container));

            // Act
            registration.BuildExpression();

            // Assert
            Assert.IsTrue(container.IsLocked());
        }

#if DEBUG
        [TestMethod]
        public void GetRelationships_AfterVerification_ReturnsTheExpectedRelationships()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            var registration = container.GetRegistration(typeof(RealUserService));

            container.Verify();

            // Act
            var relationships = registration.GetRelationships();

            // Assert
            Assert.AreEqual(1, relationships.Length);
            Assert.AreEqual(typeof(RealUserService), relationships[0].ImplementationType);
            Assert.AreEqual(Lifestyle.Transient, relationships[0].Lifestyle);
            Assert.AreEqual(typeof(IUserRepository), relationships[0].Dependency.ServiceType);
        }

        // This test proves a bug in v2.2.3.
        [TestMethod]
        public void GetRelationships_ForInstanceProducerThatSharesThRegistrationWithAnOtherProducer_HasItsOwnSetOfRegistrations()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration = Lifestyle.Transient.CreateRegistration<OneAndTwo, OneAndTwo>(container);

            container.AddRegistration(typeof(IOne), registration);
            container.AddRegistration(typeof(ITwo), registration);

            // Decorator to wrap around IOne (and not ITwo).
            container.RegisterDecorator(typeof(IOne), typeof(OneDecorator));

            InstanceProducer twoProducer = container.GetRegistration(typeof(ITwo));

            container.Verify();

            // Act
            KnownRelationship[] relationships = twoProducer.GetRelationships();
                        
            // Assert
            Assert.IsFalse(relationships.Any(), 
                "The InstanceProducer for ITwo was expected to have no relationships. Current: " +
                relationships.Select(r => r.ImplementationType).ToFriendlyNamesText());
        }
#endif

        public class OneAndTwo : IOne, ITwo 
        {
        }

        public class OneDecorator : IOne
        {
            public OneDecorator(IOne one)
            {
            }
        }
    }
}