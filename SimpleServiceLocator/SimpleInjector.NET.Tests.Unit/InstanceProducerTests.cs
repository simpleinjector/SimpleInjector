namespace SimpleInjector.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Advanced;

    [TestClass]
    public class InstanceProducerTests
    {
        [TestMethod]
        public void GetInstance_Always_LocksTheContainer()
        {
            // Arrange
            var container = new Container();

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
            var container = new Container();

            var registration = container.GetRegistration(typeof(Container));

            // Act
            registration.BuildExpression();

            // Assert
            Assert.IsTrue(container.IsLocked());
        }

        [TestMethod]
        public void GetRelationships_AfterVerification_ReturnsTheExpectedRelationships()
        {
            // Arrange
            var container = new Container();

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
    }
}