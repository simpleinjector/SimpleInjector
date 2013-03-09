namespace SimpleInjector.Tests.Unit.Lifestyles
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Lifestyles;

    [TestClass]
    public class HybridLifestyleTests
    {
        [TestMethod]
        public void BuildExpression_WithHybridLifestyle_BuildsExpectedExpression()
        {
            // Arrange
            var hybrid = Lifestyle.CreateHybrid(() => false, Lifestyle.Transient, Lifestyle.Singleton);

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>(hybrid);

            var registration = container.GetRegistration(typeof(IUserRepository)); 

            // Act
            var expression = registration.BuildExpression().ToString();

            // Assert
            Assert.AreEqual(@"
                IIF(Invoke(value(System.Func`1[System.Boolean])), 
                    Convert(new SqlUserRepository()), 
                    Convert(value(SimpleInjector.Tests.Unit.SqlUserRepository)))".TrimInside(),
                expression);
        }

        [TestMethod]
        public void BuildExpression_WithHybridLifestyleAndRegisteredDelegate_BuildsExpectedExpression()
        {
            // Arrange
            var hybrid = Lifestyle.CreateHybrid(() => false, Lifestyle.Transient, Lifestyle.Singleton);

            var container = ContainerFactory.New();

            container.Register<IUserRepository>(() => new SqlUserRepository(), hybrid);

            var registration = container.GetRegistration(typeof(IUserRepository));

            // Act
            var expression = registration.BuildExpression().ToString();

            // Assert
            Assert.IsTrue(expression.ToString().StartsWith(
                "IIF(Invoke(value(System.Func`1[System.Boolean])), Convert("),
                "Actual: " + expression.ToString());

            Assert.IsTrue(expression.ToString().EndsWith(
                "Convert(value(SimpleInjector.Tests.Unit.SqlUserRepository)))"),
                "Actual: " + expression.ToString());
        }

        [TestMethod]
        public void RegisterHybridLifestyle_SelectorPicksLeft_ReturnsInstanceWithTheExpectedLifestyle()
        {
            // Arrange
            Func<bool> selectTransient = () => true;

            var hybrid = Lifestyle.CreateHybrid(selectTransient, Lifestyle.Transient, Lifestyle.Singleton);

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>(hybrid);

            // Act
            var provider1 = container.GetInstance<IUserRepository>();
            var provider2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(provider1, provider2));
        }

        [TestMethod]
        public void RegisterHybridLifestyle_SelectorPicksRight_ReturnsInstanceWithTheExpectedLifestyle()
        {
            // Arrange
            Func<bool> selectSingleton = () => false;

            var hybrid = Lifestyle.CreateHybrid(selectSingleton, Lifestyle.Transient, Lifestyle.Singleton);

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>(hybrid);

            // Act
            var provider1 = container.GetInstance<IUserRepository>();
            var provider2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(provider1, provider2));
        }
        
        [TestMethod]
        public void GetInstance_Always_CallsTheDelegate()
        {
            // Arrange
            int callCount = 0;
            bool pickLeft = true;

            Func<bool> predicate = () =>
            {
                // This predicate should be called on each resolve.
                // In a sense the predicate itself is transient.
                callCount++;
                return pickLeft;
            };

            var hybrid = Lifestyle.CreateHybrid(predicate, Lifestyle.Transient, Lifestyle.Singleton);

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>(hybrid);

            // Act
            var provider1 = container.GetInstance<IUserRepository>();
            var provider2 = container.GetInstance<IUserRepository>();

            pickLeft = false;

            var provider3 = container.GetInstance<IUserRepository>();
            var provider4 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(4, callCount);
            Assert.IsFalse(object.ReferenceEquals(provider1, provider2),
                "When the predicate returns true, transient instances should be returned.");
            Assert.IsFalse(object.ReferenceEquals(provider2, provider3), "This is really weird.");
            Assert.IsTrue(object.ReferenceEquals(provider3, provider4),
                "When the predicate returns false, a singleton instance should be returned.");
        }

        [TestMethod]
        public void GetInstance_TwoSingletonLifestyles_EachLifestyleGetsItsOwnInstance()
        {
            // Arrange
            int callCount = 0;
            bool? pickLeft = null;

            Func<bool> predicate = () =>
            {
                callCount++;
                return pickLeft.Value;
            };
            
            var hybrid = Lifestyle.CreateHybrid(predicate, Lifestyle.Singleton, Lifestyle.Singleton);

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>(hybrid);

            // Act
            pickLeft = true;

            var provider1 = container.GetInstance<IUserRepository>();

            pickLeft = false;

            var provider2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(provider1, provider2),
                "Each wrapped lifestyle should get its own instance, even though the hybrid lifestyle " +
                "wraps two singleton lifestyles.");
        }

        [TestMethod]
        public void ExpressionBuilding_Always_GetsCalledOnceForEachSuppliedLifestyle()
        {
            // Arrange
            int expectedNumberOfCalls = 2;
            int actualNumberOfCalls = 0;

            var hybrid = Lifestyle.CreateHybrid(() => true, Lifestyle.Singleton, Lifestyle.Singleton);

            var container = ContainerFactory.New();

            container.ExpressionBuilding += (s, e) =>
            {
                Assert.IsInstanceOfType(e.Expression, typeof(NewExpression));

                actualNumberOfCalls++;
            };

            container.Register<IUserRepository, SqlUserRepository>(hybrid);

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(expectedNumberOfCalls, actualNumberOfCalls,
                "The ExpressionBuilding event is expected to be called once for each supplied lifestyle, " +
                "since it must be possible to change the supplied NewExpression. " +
                "The event is not expected to be called on the HybridLifestyle itself, since this is not a " +
                "NewExpression but an IFF wrapper.");
        }
        
        [TestMethod]
        public void ExpressionBuilt_Always_GetsCalledOnceOnlyForTheHybridLifestyle()
        {
            // Arrange
            int expectedNumberOfCalls = 1;
            int actualNumberOfCalls = 0;
            Expression expression = null;

            var hybrid = Lifestyle.CreateHybrid(() => true, Lifestyle.Transient, Lifestyle.Transient);

            var container = ContainerFactory.New();

            container.ExpressionBuilt += (s, e) =>
            {
                expression = e.Expression;

                actualNumberOfCalls++;
            };

            container.Register<IUserRepository, SqlUserRepository>(hybrid);

            // Act
            container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreEqual(expectedNumberOfCalls, actualNumberOfCalls,
                "The ExpressionBuilt event is expected to be called once when resolving a Hybrid lifestyled " +
                "instance, since this is the time that decorators would be applied and they should be " + 
                "to the whole expression.");

            Assert.AreEqual(@"
                IIF(Invoke(value(System.Func`1[System.Boolean])), 
                    Convert(new SqlUserRepository()), 
                    Convert(new SqlUserRepository()))".TrimInside(),
                expression.ToString());
        }

        [TestMethod]
        public void CreateRegistration_Always_ReturnsARegistrationThatWrapsTheOriginalLifestyle()
        {
            // Arrange
            var expectedLifestyle = Lifestyle.CreateHybrid(() => true, Lifestyle.Transient, Lifestyle.Transient);

            var container = ContainerFactory.New();

            // Act
            var registration =
                expectedLifestyle.CreateRegistration<IUserRepository, SqlUserRepository>(container);

            // Assert
            Assert.AreEqual(expectedLifestyle, registration.Lifestyle);
        }

#if DEBUG
        [TestMethod]
        public void Relationships_HybridRegistrationWithOneDependency_ReturnsThatDependencyWithExpectedLifestyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            var hybrid = Lifestyle.CreateHybrid(() => true, Lifestyle.Transient, Lifestyle.Singleton);

            // RealUserService depends on IUserRepository
            container.Register<UserServiceBase, RealUserService>(hybrid);

            var serviceRegistration = container.GetRegistration(typeof(UserServiceBase));

            // Verify triggers the building of the relationship list.
            container.Verify();

            // Act
            var repositoryRelationship = serviceRegistration.GetRelationships().Single();

            // Assert
            Assert.AreEqual(hybrid, repositoryRelationship.Lifestyle,
                "Even though the transient and singleton lifestyles build the list, the hybrid lifestyle" +
                "must merge the two lists two one and use it's own lifestyle since the the real lifestyle " +
                "is not singleton or transient, but hybrid");
        }
#endif
    }
}