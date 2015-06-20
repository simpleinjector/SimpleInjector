namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Extensions.LifetimeScoping;

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
            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();

            // Act
            container.Register<RealTimeProvider>(Lifestyle.Scoped);
        }

        [TestMethod]
        public void GetInstance_ScopedRegistrationWithoutActiveScope_Throws()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();

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
            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();

            container.Register<RealTimeProvider>(Lifestyle.Scoped);

            using (container.BeginLifetimeScope())
            {
                // Act
                container.GetInstance<RealTimeProvider>();
            }
        }
 
        [TestMethod]
        public void InstanceProducerLifestyle_ForAScopedRegistration_HasTheExpectedDefaultScopedLifestyle()
        {
            // Arrange
            var expectedLifestyle = new LifetimeScopeLifestyle();

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
        public void MethodUnderTest_Scenario_Behavior()
        {
            // Arrange
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();

            container.Options.DefaultScopedLifestyle = Lifestyle.CreateHybrid(
                () => true,
                new CustomScopedLifestyle(new Scope()),
                container.Options.DefaultScopedLifestyle);

            container.Register<RealTimeProvider>(Lifestyle.Scoped);

            // Act
            container.GetInstance<RealTimeProvider>();
        }

        [TestMethod]
        public void Verify_RegistrationWithHybridLifestyleContainingScoped_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();
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

            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();

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
                "the instance is requested outside the context of a Lifetime Scope",
                action);
        }

        [TestMethod]
        public void Verify_NestedDepenencyWithScopedLifestyleWithCustomBuiltExpression_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();

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
        
        private sealed class CustomScopedLifestyle : ScopedLifestyle
        {
            private readonly Scope scope;

            public CustomScopedLifestyle(Scope scope = null) : base("Custom Scope")
            {
                this.scope = scope;
            }

            protected internal override Func<Scope> CreateCurrentScopeProvider(Container container)
            {
                return () => this.scope;
            }

            protected override int Length
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}