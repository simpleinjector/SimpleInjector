namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// These tests verify whether the container correctly reports on existing "lookalike" registrations. A
    /// lookalike is a different type with the same name. Simple Injector reports on these lookalikes, because
    /// lookalikes often cause major confusion on the user, because of the similarities in naming.
    /// </summary>
    [TestClass]
    public class LookalikeErrorTests
    {
        [TestMethod]
        public void GetInstance_OnMissingTypeWithNoLookalike_DoesNotReportLookalikes()
        {
            // Arrange
            var container = new Container();

            // Act
            Action action = () => container.GetInstance<IDuplicate>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "Note that there exists a registration for a different type",
                action);
        }

        [TestMethod]
        public void GetInstance_OnMissingConstructorDependencyWithNoLookalike_DoesNotReportLookalikes()
        {
            // Arrange
            var container = new Container();

            // Act
            Action action = () => container.GetInstance<ServiceDependingOn<IDuplicate>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "Note that there exists a registration for a different type",
                action);
        }

        [TestMethod]
        public void GetInstance_OnMissingTypeWithLookalikeAsExternalProducer_DoesNotReportLookalikes()
        {
            // Arrange
            var container = new Container();

            var externalProducer = Lifestyle.Transient.CreateProducer<IDuplicate, Duplicate>(container);

            // Act
            Action action = () => container.GetInstance<ServiceDependingOn<IDuplicate>>();

            // Assert
            // This exception is not expected to be thrown, because external producers are not registered
            // in the container and can not be resolved from the container. Reporting them would be confusing.
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "Note that there exists a registration for a different type",
                action);

            GC.KeepAlive(externalProducer);
        }

        [TestMethod]
        public void GetInstance_ConsumerDependingOnConditionalRegistrationsThatDoNotGetInjected_DoesNotReportLookalikes()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional(typeof(ILogger), typeof(NullLogger), Lifestyle.Singleton, c => false);
            container.RegisterConditional(typeof(ILogger), typeof(ConsoleLogger), Lifestyle.Singleton, c => false);

            // Act
            Action action = () => container.GetInstance<ServiceWithDependency<ILogger>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                "Note that there exists a registration for a different type",
                action);
        }

        [TestMethod]
        public void GetInstance_OnMissingTypeWithExistingLookalike_WarnsAboutThisLookalike()
        {
            // Arrange
            var container = new Container();

            container.Register<IDuplicate, Duplicate>();

            // Act
            Action action = () => container.GetInstance<SimpleInjector.Tests.Unit.Duplicates.IDuplicate>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                Note that there exists a registration for a different type
                SimpleInjector.Tests.Unit.IDuplicate while the requested type is
                SimpleInjector.Tests.Unit.Duplicates.IDuplicate."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_MissingConstructorDependencyWithExistingLookalike_WarnsAboutThisLookalike()
        {
            // Arrange
            var container = new Container();

            container.Register<IDuplicate, Duplicate>();
            container.Register<ServiceDependingOn<SimpleInjector.Tests.Unit.Duplicates.IDuplicate>>();

            // Act
            Action action = () => container.GetInstance<ServiceDependingOn<SimpleInjector.Tests.Unit.Duplicates.IDuplicate>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                Note that there exists a registration for a different type
                SimpleInjector.Tests.Unit.IDuplicate"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_OnMissingConstructorDependencyWithExistingOpenGenericLookalike_WarnsAboutThisLookalike()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(IDuplicate<>), typeof(Duplicate<>));
            container.Register<ServiceDependingOn<SimpleInjector.Tests.Unit.Duplicates.IDuplicate<object>>>();

            // Act
            Action action = () =>
                container.GetInstance<ServiceDependingOn<SimpleInjector.Tests.Unit.Duplicates.IDuplicate<object>>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                Note that there exists a registration for a different type
                SimpleInjector.Tests.Unit.IDuplicate<T>"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_OnMissingConstructorDependencyWithExistingConditionalNonGenericLookalike_WarnsAboutThisLookalike()
        {
            // Arrange
            var container = new Container();

            container.RegisterConditional(typeof(IDuplicate), typeof(Duplicate), c => true);

            container.Register<ServiceDependingOn<SimpleInjector.Tests.Unit.Duplicates.IDuplicate>>();

            // Act
            Action action = () =>
                container.GetInstance<ServiceDependingOn<SimpleInjector.Tests.Unit.Duplicates.IDuplicate>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                Note that there exists a registration for a different type
                SimpleInjector.Tests.Unit.IDuplicate"
                .TrimInside(),
                action);
        }

        // #807
        [TestMethod]
        public void GetInstance_OnMissingConstructorDependencyWithExistingConditionalFactoryNonGenericLookalike_WarnsAboutThisLookalike()
        {
            // Arrange
            var container = new Container();

            container.RegisterConditional(
                typeof(IDuplicate),
                c => typeof(Duplicate<>).MakeGenericType(c.Consumer.ImplementationType),
                Lifestyle.Singleton,
                c => true);

            container.Register<ServiceDependingOn<SimpleInjector.Tests.Unit.Duplicates.IDuplicate>>();

            // Act
            Action action = () =>
                container.GetInstance<ServiceDependingOn<SimpleInjector.Tests.Unit.Duplicates.IDuplicate>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                Note that there exists a registration for a different type
                SimpleInjector.Tests.Unit.IDuplicate"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_OnMissingConstructorDependencyWithExistingConditionalFactoryGenericLookalike_WarnsAboutThisLookalike()
        {
            // Arrange
            var container = new Container();

            container.RegisterConditional(
                typeof(IDuplicate<>),
                c => typeof(Duplicate<>).MakeGenericType(c.Consumer.ImplementationType),
                Lifestyle.Singleton,
                c => true);

            container.Register<ServiceDependingOn<SimpleInjector.Tests.Unit.Duplicates.IDuplicate<object>>>();

            // Act
            Action action = () =>
                container.GetInstance<ServiceDependingOn<SimpleInjector.Tests.Unit.Duplicates.IDuplicate<object>>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                Note that there exists a registration for a different type
                SimpleInjector.Tests.Unit.IDuplicate"
                .TrimInside(),
                action);
        }
    }
}