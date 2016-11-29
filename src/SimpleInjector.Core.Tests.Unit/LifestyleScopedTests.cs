namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Lifestyles;

    [TestClass]
    public class LifestyleScopedTests
    {
        [TestMethod]
        public void ContainerRegister_ContainerWithoutDefaultScopedLifestyle_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            // Act
            Action action = () => container.Register<RealTimeProvider>(Lifestyle.Scoped);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "To be able to use the Lifestyle.Scoped property, please ensure that the container is " +
                "configured with a default scoped lifestyle by setting the Container.Options." +
                "DefaultScopedLifestyle property with the required scoped lifestyle for your type of " +
                "application.",
                action);
        }

        [TestMethod]
        public void ContainerRegister_ContainerWithDefaultScopedLifestyle_Succeeds()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            // Act
            container.Register<RealTimeProvider>(Lifestyle.Scoped);
        }

        [TestMethod]
        public void GetInstance_ScopedRegistrationWithoutActiveScope_Throws()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            container.Register<RealTimeProvider>(Lifestyle.Scoped);

            // Act
            Action action = () => container.GetInstance<RealTimeProvider>();

            // Assert
            AssertThat.Throws<ActivationException>(action);
        }

        [TestMethod]
        public void GetInstance_ScopedRegistrationWithActiveScope_Succeeds()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            container.Register<RealTimeProvider>(Lifestyle.Scoped);

            using (ThreadScopedLifestyle.BeginScope(container))
            {
                // Act
                container.GetInstance<RealTimeProvider>();
            }
        }
 
        [TestMethod]
        public void InstanceProducerLifestyle_ForAScopedRegistration_HasTheExpectedDefaultScopedLifestyle()
        {
            // Arrange
            var expectedLifestyle = new ThreadScopedLifestyle();

            var container = new Container();
            container.Options.DefaultScopedLifestyle = expectedLifestyle;

            container.Register<RealTimeProvider>(Lifestyle.Scoped);

            InstanceProducer producer = container.GetRegistration(typeof(RealTimeProvider));

            // Act
            Lifestyle actualLifestyle = producer.Lifestyle;

            // Assert
            Assert.AreSame(expectedLifestyle, actualLifestyle);
        }

        [TestMethod]
        public void GetInstance_DefaultLifestyleWrappingTheOldLifestyle_RegistersTheInstanceUsingTheExpectedLifestyle()
        {
            // Arrange
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            var expectedLifestyle = Lifestyle.CreateHybrid(
                () => true,
                new CustomScopedLifestyle(new Scope(container)),
                container.Options.DefaultScopedLifestyle);

            container.Options.DefaultScopedLifestyle = expectedLifestyle;

            container.Register<RealTimeProvider>(Lifestyle.Scoped);

            // Act
            var registration = container.GetRegistration(typeof(RealTimeProvider));

            // Assert
            Assert.AreSame(expectedLifestyle, registration.Lifestyle);
        }

        [TestMethod]
        public void Verify_RegistrationWithHybridLifestyleContainingScoped_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();
            var hybridLifestyle = Lifestyle.CreateHybrid(() => true, Lifestyle.Transient, Lifestyle.Scoped);

            // class RealUserService(IUserRepository)
            container.Register<UserServiceBase, RealUserService>(hybridLifestyle);
            container.Register<IUserRepository, SqlUserRepository>(Lifestyle.Singleton);

            // Act
            container.Verify();
        }

        [TestMethod]
        public void CustomBuiltDelegate_NestedDepenencyWithScopedLifestyle_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            // class RealUserService(IUserRepository)
            container.Register<UserServiceBase, RealUserService>(Lifestyle.Transient);
            container.Register<IUserRepository, SqlUserRepository>(Lifestyle.Scoped);

            var expression = container.GetRegistration(typeof(UserServiceBase)).BuildExpression();

            // By building this expression we circumvent the container's scope optimizations and we force
            // a different path through the code causing Scope.GetScopelessInstance to be called.
            var userServiceFactory = Expression.Lambda<Func<UserServiceBase>>(expression).Compile();

            // Act
            Action action = () => userServiceFactory();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "the instance is requested outside the context of an active (Thread Scoped) scope",
                action);
        }

        [TestMethod]
        public void Verify_NestedDepenencyWithScopedLifestyleWithCustomBuiltExpression_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            // class RealUserService(IUserRepository)
            container.Register<UserServiceBase, RealUserService>(Lifestyle.Transient);
            container.Register<IUserRepository, SqlUserRepository>(Lifestyle.Scoped);

            container.Register<object>(() =>
            {
                var expression = container.GetRegistration(typeof(UserServiceBase)).BuildExpression();

                // By building this expression we circumvent the container's scope optimizations and we force
                // a different path through the code causing Scope.GetScopelessInstance to be called.
                var userServiceFactory = Expression.Lambda<Func<UserServiceBase>>(expression).Compile();

                return userServiceFactory();
            }, Lifestyle.Singleton);

            // Act
            container.Verify();
        }

        [TestMethod]
        public void DefaultScopedLifestyle_SetWithLifestyleScoped_ThrowsException()
        {
            // Arrange
            var container = new Container();

            // Act
            Action action = () => container.Options.DefaultScopedLifestyle = Lifestyle.Scoped;

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "DefaultScopedLifestyle can't be set with the value of Lifestyle.Scoped.",
                action,
                "Setting the DefaultScopedLifestyle with the value of Lifestyle.Scoped is not allowed, " +
                "because that would cause a cyclic reference and will trigger a stack overflow exception " +
                "later on when Lifestyle.Scoped is supplied to one of the Register methods.");
        }

        [TestMethod]
        public void ScopedProxyLifestyleDependencyLength_Always_ReturnsLengthOfDefaultScopedLifestyle()
        {
            // Arrange
            int expectedLength = 502; // Anything different than the default length for scoped (500).

            var container = new Container();

            container.Options.DefaultScopedLifestyle = new CustomScopedLifestyle(length: expectedLength);

            // Act
            int actualLength = Lifestyle.Scoped.DependencyLength(container);

            // Assert
            Assert.AreEqual(expectedLength, actualLength);
        }

        [TestMethod]
        public void ScopedProxyLifestyleCreateCurrentScopeProvider_Always_ReturnsScopeOfDefaultScopedLifestyle()
        {
            // Arrange
            var container = new Container();

            Scope expectedScope = new Scope(container);

            container.Options.DefaultScopedLifestyle = new CustomScopedLifestyle(scope: expectedScope);

            // Act
            Scope actualScope = Lifestyle.Scoped.CreateCurrentScopeProvider(container)();

            // Assert
            Assert.AreSame(expectedScope, actualScope);
        }

        [TestMethod]
        public void ScopedProxyLifestyleCreateRegistration_Always_WrapsTheScopeOfDefaultScopedLifestyle()
        {
            // Arrange
            var expectedLifestyle = new ThreadScopedLifestyle();

            var container = new Container();

            container.Options.DefaultScopedLifestyle = expectedLifestyle;

            // Act
            var registration = Lifestyle.Scoped.CreateRegistration(typeof(NullLogger), container);

            var actualLifestyle = registration.Lifestyle;

            // Assert
            Assert.AreSame(expectedLifestyle, actualLifestyle);
        }

        private sealed class CustomScopedLifestyle : ScopedLifestyle
        {
            private readonly Scope scope;

            public CustomScopedLifestyle(Scope scope = null, int? length = null) : base("Custom Scope")
            {
                this.scope = scope;
                this.Length = length ?? base.Length;
            }

            public override int Length { get; }

            protected internal override Func<Scope> CreateCurrentScopeProvider(Container container) => 
                () => this.scope;
        }
    }
}