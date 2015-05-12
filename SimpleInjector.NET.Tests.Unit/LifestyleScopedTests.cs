namespace SimpleInjector.Tests.Unit
{
    using System;
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