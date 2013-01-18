namespace SimpleInjector.Tests.Unit.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Analysis;
    using SimpleInjector.Extensions;

    [TestClass]
    public class ContainerDebugViewTests
    {
        [TestMethod]
        public void Ctor_WithValidArgument_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            new ContainerDebugView(container);
        }

        [TestMethod]
        public void Options_Always_ReturnsSameInstanceAsThatOfContainer()
        {
            // Arrange
            var container = new Container();

            // Act
            var debugView = new ContainerDebugView(container);

            // Assert
            Assert.IsTrue(object.ReferenceEquals(container.Options, debugView.Options));
        }

        [TestMethod]
        public void Ctor_Always_LeavesContainerUnlocked()
        {
            var container = new Container();

            new ContainerDebugView(container);

            // Act
            // Registration should succeed
            container.Register<IPlugin, PluginImpl>();

            // Assert
            Assert.IsFalse(container.IsLocked);
        }

        [TestMethod]
        public void Registrations_Always_LeavesContainerUnlocked()
        {
            var container = new Container();

            var debugView = new ContainerDebugView(container);

            // Act
            debugView.Registrations.ToArray();

            // Assert
            Assert.IsFalse(container.IsLocked);
        }

        [TestMethod]
        public void Ctor_ContainerWithoutConfigurationErrors_DoesNotContainAConfigationErrorsSection()
        {
            // Arrange
            var container = new Container();

            // Act
            var debugView = new ContainerDebugView(container);

            // Assert
            Assert.IsFalse(debugView.Items.Any(item => item.Name == "Configuration Errors"));
        }

        [TestMethod]
        public void Ctor_ContainerWithoutConfigurationErrors_ContainsAPotentialLifestyleMismatchesSection()
        {
            // Arrange
            var container = new Container();

            // Act
            var debugView = new ContainerDebugView(container);

            // Assert
            Assert.IsTrue(debugView.Items.Any(item => item.Name == "Potential Lifestyle Mismatches"));
        }
        
        [TestMethod]
        public void Ctor_ContainerWithConfigurationErrors_ReturnsASingleItemNamedConfigurationErrors()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin>(() => null);

            // Act
            var debugView = new ContainerDebugView(container);

            var configurationErrorsView = debugView.Items.Single();

            // Assert
            Assert.AreEqual("Configuration Errors", configurationErrorsView.Name);
        }

        [TestMethod]
        public void Ctor_ContainerWithConfigurationErrorsInCollections_ReturnsASingleItemNamedConfigurationErrors()
        {
            // Arrange
            var container = new Container();

            IEnumerable<IPlugin> plugins = new IPlugin[] { null };

            container.RegisterAll<IPlugin>(plugins);

            // Act
            var debugView = new ContainerDebugView(container);

            var configurationErrorsView = debugView.Items.Single();

            // Assert
            Assert.AreEqual("Configuration Errors", configurationErrorsView.Name);
        }

        [TestMethod]
        public void Ctor_ContainerWithOneConfigurationError_ReturnsASingleItemDescribingThatConfigurationError()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin>(() => null);

            // Act
            var debugView = new ContainerDebugView(container);

            var configurationErrorsView = debugView.Items.Single();
            var error = ((IEnumerable<DebuggerViewItem>)configurationErrorsView.Value).Single();

            // Assert
            Assert.AreEqual(typeof(IPlugin).Name, error.Name);
            AssertThat.ExceptionMessageContains("The registered delegate for type IPlugin returned null.",
                (Exception)error.Value);
        }

        [TestMethod]
        public void Ctor_ContainerWithMultipleConfigurationErrors_ReturnsTheExpectedErrorsView()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin>(() => null);
            container.Register<object>(() => null);

            // Act
            var debugView = new ContainerDebugView(container);

            var configurationErrorsView = debugView.Items.Single();
            var errors = ((IEnumerable<DebuggerViewItem>)configurationErrorsView.Value).ToArray();

            // Assert
            Assert.AreEqual("Configuration Errors", configurationErrorsView.Name);
            Assert.AreEqual("2 errors.", configurationErrorsView.Description);
            Assert.AreEqual(2, errors.Length);
        }

        [TestMethod]
        public void Ctor_Always_ClearsTheDelegateCacheToAllowOverridingRegistrations()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>();

            // RealUserService depends on IUserRepository
            container.Register<RealUserService>();
            
            // Act
            new ContainerDebugView(container);

            container.Options.AllowOverridingRegistrations = true;

            // Here we override the IUserRepository. If caches are not cleared, the RealUserService will be
            // injected with a InMemoryUserRepository.
            container.Register<IUserRepository, SqlUserRepository>();

            var service = container.GetInstance<RealUserService>();

            // Assert
            Assert.IsInstanceOfType(service.Repository, typeof(SqlUserRepository));
        }

        [TestMethod]
        public void Ctor_Always_ClearsTheDelegateCacheToAddingDecorators()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>();

            // RealUserService depends on IUserRepository
            container.Register<RealUserService>();

            // Act
            new ContainerDebugView(container);

            // Here we override the IUserRepository. If caches are not cleared, the RealUserService will be
            // injected with a InMemoryUserRepository.
            container.RegisterDecorator(typeof(IUserRepository), typeof(UserRepositoryDecorator));

            var service = container.GetInstance<RealUserService>();

            // Assert
            Assert.IsInstanceOfType(service.Repository, typeof(UserRepositoryDecorator));
        }

        [TestMethod]
        public void Ctor_CalledOnNotLockedContainer_ResetsAnyChangedLifestyles()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>(Lifestyle.Transient);

            // A singleton decorator will change the lifestyle of the registration to singleton
            container.RegisterSingleDecorator(typeof(IUserRepository), typeof(UserRepositoryDecorator));

            container.Verify();

            // Act
            // Calling ContainerDebugView on a not locked container.
            new ContainerDebugView(container);

            var registration = container.GetRegistration(typeof(IUserRepository));

            // Assert
            Assert.AreEqual(Lifestyle.Transient, registration.Lifestyle,
                "Lifestyle was expected to be reset to transient (with all other cache things).");
        }

        [TestMethod]
        public void Ctor_CalledOnLockedContainer_DoesNotResetLifestyle()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>(Lifestyle.Transient);

            container.RegisterSingleDecorator(typeof(IUserRepository), typeof(UserRepositoryDecorator));

            container.Verify();

            // Act
            var registration = container.GetRegistration(typeof(IUserRepository));

            // Locks the container
            registration.GetInstance();
            
            new ContainerDebugView(container);

            // Assert
            Assert.AreEqual(Lifestyle.Singleton, registration.Lifestyle,
                "Since the container is locked, the cache and lifestyle override should not be reset. " +
                "This would cause the container to regenerate all expressions and func<T> delegates again, " +
                "while this is not needed, since the container can't be changed.");
        }

        public class UserRepositoryDecorator : IUserRepository
        {
            public UserRepositoryDecorator(IUserRepository repository)
            {
            }

            public void Delete(int userId)
            {
            }
        }
    }
}
