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
            Assert.IsTrue(expression.StartsWith(
                "IIF(Invoke(value(System.Func`1[System.Boolean])), Convert("),
                "Actual: " + expression);

            Assert.IsTrue(expression.EndsWith(
                "Convert(value(SimpleInjector.Tests.Unit.SqlUserRepository)))"),
                "Actual: " + expression);
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
        public void CreateHybrid_WithFallbackLifestyleWithoutActiveScope_UsesFallbackLifestyle()
        {
            // Arrange
            var hybrid = Lifestyle.CreateHybrid(
                defaultLifestyle: new ThreadScopedLifestyle(), 
                fallbackLifestyle: Lifestyle.Singleton);

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>(hybrid);

            // Act
            var provider1 = container.GetInstance<IUserRepository>();
            var provider2 = container.GetInstance<IUserRepository>();

            // Assert
            Assert.AreSame(provider1, provider2);
        }

        [TestMethod]
        public void CreateHybrid_WithFallbackLifestyleWithActiveScope_UsesDefaultScopedLifestyle()
        {
            // Arrange
            var hybrid = Lifestyle.CreateHybrid(
                defaultLifestyle: new ThreadScopedLifestyle(), 
                fallbackLifestyle: Lifestyle.Singleton);

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>(hybrid);

            IUserRepository provider1;
            IUserRepository provider2;

            // Act
            using (ThreadScopedLifestyle.BeginScope(container))
            {
                provider1 = container.GetInstance<IUserRepository>();
            }

            using (ThreadScopedLifestyle.BeginScope(container))
            {
                provider2 = container.GetInstance<IUserRepository>();
            }

            // Assert
            Assert.AreNotSame(provider1, provider2);
        }

        [TestMethod]
        public void CreateHybrid_TwoScopedLifestyles_ResolvesFromBothLifestyles()
        {
            // Arrange
            var scope = new Scope();

            var container = new Container();

            container.Options.DefaultScopedLifestyle = Lifestyle.CreateHybrid(
                defaultLifestyle: new ThreadScopedLifestyle(),
                fallbackLifestyle: new CustomScopedLifestyle(scope));

            container.Register<IUserRepository, SqlUserRepository>(Lifestyle.Scoped);

            // Act
            IUserRepository repo1 = container.GetInstance<IUserRepository>();
            IUserRepository repo2 = null;

            using (ThreadScopedLifestyle.BeginScope(container))
            {
                repo2 = container.GetInstance<IUserRepository>();
            }

            // Assert
            Assert.AreNotSame(repo1, repo2);
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
        public void GetInstance_TwoSingletonLifestyles_ResultsInOneInstance()
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
            Assert.AreSame(provider1, provider2,
                "Each wrapped lifestyle should get its own instance, except when both sides point at the " +
                "same lifestyle, because the same Registration instance should be used due to internal " +
                "lifestyle caching.");
        }

        [TestMethod]
        public void ExpressionBuilding_Always_GetsCalledOnceForEachSuppliedLifestyle()
        {
            // Arrange
            int expectedNumberOfCalls = 2;
            int actualNumberOfCalls = 0;

            var hybrid = Lifestyle.CreateHybrid(() => true, Lifestyle.Singleton, Lifestyle.Transient);

            var container = ContainerFactory.New();

            container.ExpressionBuilding += (s, e) =>
            {
                AssertThat.IsInstanceOfType(typeof(NewExpression), e.Expression);

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
                expectedLifestyle.CreateRegistration<SqlUserRepository>(container);

            // Assert
            Assert.AreEqual(expectedLifestyle, registration.Lifestyle);
        }

        [TestMethod]
        public void CreateRegistration_WithScopedLifestylesAlways_ReturnsARegistrationThatWrapsTheOriginalLifestyle()
        {
            // Arrange
            ScopedLifestyle expectedLifestyle =
                Lifestyle.CreateHybrid(() => true, new CustomScopedLifestyle(), new CustomScopedLifestyle());

            var container = ContainerFactory.New();

            // Act
            var registration =
                expectedLifestyle.CreateRegistration<SqlUserRepository>(container);

            // Assert
            Assert.AreEqual(expectedLifestyle, registration.Lifestyle);
        }

        [TestMethod]
        public void CreateRegistrationService_WithScopedLifestylesAlways_ReturnsARegistrationThatWrapsTheOriginalLifestyle()
        {
            // Arrange
            ScopedLifestyle expectedLifestyle =
                Lifestyle.CreateHybrid(() => true, new CustomScopedLifestyle(), new CustomScopedLifestyle());

            var container = ContainerFactory.New();

            // Act
            var registration =
                expectedLifestyle.CreateRegistration<IUserRepository>(() => null, container);

            // Assert
            Assert.AreEqual(expectedLifestyle, registration.Lifestyle);
        }

        [TestMethod]
        public void Relationships_HybridRegistrationWithOneDependency_ReturnsThatDependencyWithExpectedLifestyle()
        {
            // Arrange
            var container = ContainerFactory.New();

            var hybridLifestyle = Lifestyle.CreateHybrid(() => true, Lifestyle.Transient, Lifestyle.Singleton);

            // class RealUserService(IUserRepository)
            container.Register<UserServiceBase, RealUserService>(hybridLifestyle);
            container.Register<IUserRepository, SqlUserRepository>(Lifestyle.Singleton);

            var serviceRegistration = container.GetRegistration(typeof(UserServiceBase));

            // Verify triggers the building of the relationship list.
            container.Verify();

            // Act
            var repositoryRelationship = serviceRegistration.GetRelationships().Single();

            // Assert
            Assert.AreEqual(hybridLifestyle, repositoryRelationship.Lifestyle,
                "Even though the transient and singleton lifestyles build the list, the hybrid lifestyle" +
                "must merge the two lists two one and use it's own lifestyle since the real lifestyle " +
                "is not singleton or transient, but hybrid");
        }

        [TestMethod]
        public void CreateHybrid_WithNullLifestyleSelector_ReturnsScopedLifestyle()
        {
            // Arrange
            Func<bool> invalidLifestyleSelector = null;

            // Act
            Action action = () =>
            {
                ScopedLifestyle hybrid = Lifestyle.CreateHybrid(
                    invalidLifestyleSelector,
                    new CustomScopedLifestyle(),
                    new CustomScopedLifestyle());
            };

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("lifestyleSelector", action);
        }

        [TestMethod]
        public void CreateHybrid_WithNullTrueLifestyle_ReturnsScopedLifestyle()
        {
            // Arrange
            ScopedLifestyle invalidTrueLifestyle = null;

            // Act
            Action action = () =>
            {
                ScopedLifestyle hybrid = Lifestyle.CreateHybrid(
                    () => true,
                    invalidTrueLifestyle,
                    new CustomScopedLifestyle());
            };

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("trueLifestyle", action);
        }

        [TestMethod]
        public void CreateHybrid_WithNullFalseLifestyle_ReturnsScopedLifestyle()
        {
            // Arrange
            ScopedLifestyle invalidFalseLifestyle = null;

            // Act
            Action action = () =>
            {
                ScopedLifestyle hybrid = Lifestyle.CreateHybrid(
                    () => true,
                    new CustomScopedLifestyle(),
                    invalidFalseLifestyle);
            };

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("falseLifestyle", action);
        }

        [TestMethod]
        public void CreateHybrid_WithValidScopedLifestyles_ReturnsScopedLifestyle()
        {
            // Arrange
            ScopedLifestyle trueLifestyle = new CustomScopedLifestyle();
            ScopedLifestyle falseLifestyle = new CustomScopedLifestyle();

            // Act
            ScopedLifestyle hybrid = Lifestyle.CreateHybrid(() => true, trueLifestyle, falseLifestyle);

            // Assert
            Assert.IsNotNull(hybrid);
        }

        [TestMethod]
        public void RegisterForDisposal_CalledOnCreatedScopedHybridWithFalseSelector_ForwardsCallToFalseLifestyleScope()
        {
            // Arrange
            var container = new Container();
            Scope trueScope = new Scope(container);
            Scope falseScope = new Scope(container);

            var trueLifestyle = new CustomScopedLifestyle(trueScope);
            var falseLifestyle = new CustomScopedLifestyle(falseScope);

            ScopedLifestyle hybrid = Lifestyle.CreateHybrid(() => false, trueLifestyle, falseLifestyle);

            // Act
            hybrid.RegisterForDisposal(container, new DisposableObject());

            // Assert
            Assert.IsFalse(trueScope.GetDisposables().Any(), "TrueLifestyle was NOT expected to be called.");
            Assert.AreEqual(1, falseScope.GetDisposables().Length, "FalseLifestyle was expected to be called.");
        }

        [TestMethod]
        public void RegisterForDisposal_CalledOnCreatedScopedHybridWithTrueSelector_ForwardsCallToTrueLifestyleScope()
        {
            // Arrange
            Container container = new Container();
            Scope trueScope = new Scope(container);
            Scope falseScope = new Scope(container);

            var trueLifestyle = new CustomScopedLifestyle(trueScope);
            var falseLifestyle = new CustomScopedLifestyle(falseScope);

            ScopedLifestyle hybrid = Lifestyle.CreateHybrid(() => true, trueLifestyle, falseLifestyle);

            // Act
            hybrid.RegisterForDisposal(container, new DisposableObject());

            // Assert
            Assert.AreEqual(1, trueScope.GetDisposables().Length, "TrueLifestyle was expected to be called.");
            Assert.IsFalse(falseScope.GetDisposables().Any(), "FalseLifestyle was NOT expected to be called.");
        }

        [TestMethod]
        public void GetInstance_CalledMultipleTimesForDifferentLifestyles_ResolvesInstanceForBothLifestyles()
        {
            // Arrange
            var container = new Container();

            bool selectTrueLifestyle = true;

            Scope trueScope = new Scope(container);
            Scope falseScope = new Scope(container);

            var trueLifestyle = new CustomScopedLifestyle(trueScope);
            var falseLifestyle = new CustomScopedLifestyle(falseScope);

            ScopedLifestyle hybrid = 
                Lifestyle.CreateHybrid(() => selectTrueLifestyle, trueLifestyle, falseLifestyle);

            container.Register<IDisposable, DisposableObject>(hybrid);

            // Act
            var instance1 = container.GetInstance<IDisposable>();

            // Assert
            Assert.AreEqual(1, trueScope.GetDisposables().Length, "TrueLifestyle was expected to be called.");
            Assert.IsFalse(falseScope.GetDisposables().Any(), "FalseLifestyle was NOT expected to be called.");

            // Act
            // Here we flip the switch. Resolving the instance now should get a new instance from the other
            // lifestyle
            selectTrueLifestyle = false;

            var instance2 = container.GetInstance<IDisposable>();

            // Assert
            Assert.AreEqual(1, falseScope.GetDisposables().Length, "FalseLifestyle was expected to be called this time.");
            Assert.IsFalse(object.ReferenceEquals(instance1, instance2));
        }

        [TestMethod]
        public void WhenScopeEnds_CalledOnCreatedScopedHybridWithFalseSelector_ForwardsCallToFalseLifestyle()
        {
            // Arrange
            CustomScopedLifestyle trueLifestyle = new CustomScopedLifestyle();
            CustomScopedLifestyle falseLifestyle = new CustomScopedLifestyle();

            ScopedLifestyle hybrid = Lifestyle.CreateHybrid(() => false, trueLifestyle, falseLifestyle);

            // Act
            hybrid.WhenScopeEnds(new Container(), () => { });

            // Assert
            Assert.IsFalse(trueLifestyle.ScopeUsed, "TrueLifestyle was NOT expected to be called.");
            Assert.IsTrue(falseLifestyle.ScopeUsed, "FalseLifestyle was expected to be called.");
        }

        [TestMethod]
        public void WhenScopeEnds_CalledOnCreatedScopedHybridWithTrueSelector_ForwardsCallToTrueLifestyle()
        {
            // Arrange
            CustomScopedLifestyle trueLifestyle = new CustomScopedLifestyle();
            CustomScopedLifestyle falseLifestyle = new CustomScopedLifestyle();

            ScopedLifestyle hybrid = Lifestyle.CreateHybrid(() => true, trueLifestyle, falseLifestyle);

            // Act
            hybrid.WhenScopeEnds(new Container(), () => { });

            // Assert
            Assert.IsTrue(trueLifestyle.ScopeUsed, "TrueLifestyle was expected to be called.");
            Assert.IsFalse(falseLifestyle.ScopeUsed, "FalseLifestyle was NOT expected to be called.");
        }

        [TestMethod]
        public void RegisterForDisposal_CalledOnCreatedScopedHybridWithFalseSelector_ForwardsCallToFalseLifestyle()
        {
            // Arrange
            CustomScopedLifestyle trueLifestyle = new CustomScopedLifestyle();
            CustomScopedLifestyle falseLifestyle = new CustomScopedLifestyle();

            ScopedLifestyle hybrid = Lifestyle.CreateHybrid(() => false, trueLifestyle, falseLifestyle);

            // Act
            hybrid.RegisterForDisposal(new Container(), new DisposableObject());

            // Assert
            Assert.IsFalse(trueLifestyle.ScopeUsed, "TrueLifestyle was NOT expected to be called.");
            Assert.IsTrue(falseLifestyle.ScopeUsed, "FalseLifestyle was expected to be called.");
        }

        [TestMethod]
        public void RegisterForDisposal_CalledOnCreatedScopedHybridWithTrueSelector_ForwardsCallToTrueLifestyle()
        {
            // Arrange
            CustomScopedLifestyle trueLifestyle = new CustomScopedLifestyle();
            CustomScopedLifestyle falseLifestyle = new CustomScopedLifestyle();

            ScopedLifestyle hybrid = Lifestyle.CreateHybrid(() => true, trueLifestyle, falseLifestyle);

            // Act
            hybrid.RegisterForDisposal(new Container(), new DisposableObject());

            // Assert
            Assert.IsTrue(trueLifestyle.ScopeUsed, "TrueLifestyle was expected to be called.");
            Assert.IsFalse(falseLifestyle.ScopeUsed, "FalseLifestyle was NOT expected to be called.");
        }

        [TestMethod]
        public void CreateHybrid_WithMixedHybridAndScopedHybrid1_CreatesExpectedLifestyleName()
        {
            // Act
            var lifestyle = Lifestyle.CreateHybrid(() => true,
                Lifestyle.CreateHybrid(() => true,
                    new CustomScopedLifestyle("Custom1"),
                    new CustomScopedLifestyle("Custom2")),
                Lifestyle.Transient);

            // Assert
            Assert.AreEqual("Hybrid Custom1 / Custom2 / Transient", lifestyle.Name);
        }

        [TestMethod]
        public void CreateHybrid_WithMixedHybridAndScopedHybrid2_CreatesExpectedLifestyleName()
        {
            // Act
            var lifestyle = Lifestyle.CreateHybrid(() => true,
                Lifestyle.CreateHybrid(() => true,
                    Lifestyle.Transient,
                    Lifestyle.Singleton),
                new CustomScopedLifestyle("Custom1"));

            // Assert
            Assert.AreEqual("Hybrid Transient / Singleton / Custom1", lifestyle.Name);
        }

        private class DisposableObject : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private class CustomScopedLifestyle : ScopedLifestyle
        {
            private readonly Lifestyle realLifestyle;

            public CustomScopedLifestyle(string name = null) : base(name ?? "Custom")
            {
                this.realLifestyle = Lifestyle.Transient;
            }

            public CustomScopedLifestyle(Scope scope) : this()
            {
                this.Scope = scope;
            }

            public int GetCurrentScopeCoreCallCount { get; private set; }
            
            public int CurrentScopeProviderCallCount { get; private set; }

            public bool ScopeUsed => this.GetCurrentScopeCoreCallCount + this.CurrentScopeProviderCallCount > 0;

            public Scope Scope { get; }

            protected internal override Func<Scope> CreateCurrentScopeProvider(Container container)
            {
                return () =>
                {
                    this.CurrentScopeProviderCallCount++;
                    return this.Scope ?? new Scope(container);
                };
            }

            protected internal override Registration CreateRegistrationCore<TConcrete>(Container container)
            {
                return this.realLifestyle.CreateRegistration<TConcrete>(container);
            }

            protected internal override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
                Container container)
            {
                return this.realLifestyle.CreateRegistration(instanceCreator, container);
            }

            protected override Scope GetCurrentScopeCore(Container container)
            {
                this.GetCurrentScopeCoreCallCount++;
                return base.GetCurrentScopeCore(container);
            }
        }
    }
}