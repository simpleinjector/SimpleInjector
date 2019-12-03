namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Lifestyles;

    [TestClass]
    public class AutoVerification
    {
        [TestMethod]
        public void BuildExpression_WithAutoVerificationEnabledOnAcyclicGraph_ShouldNotCauseACyclicGraphException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.EnableAutoVerification = true;

            container.Register<ICommand, ConcreteCommand>(new AsyncScopedLifestyle());

            InstanceProducer producer = container.GetRegistration(typeof(ICommand));

            // Act
            // NOTE: I had to change the places in InstanceProducer where the container was locked, to prevent
            // this test from failing.
            producer.BuildExpression();
        }

        [TestMethod]
        public void GetInstance_AutoVerificationEnabled_ShouldVerifyContainerOnFirstCall()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.EnableAutoVerification = true;

            // Registering a service depending on an unregister type ILogger
            container.Register<ServiceDependingOn<ILogger>>();

            container.Register<ICommand, ConcreteCommand>();

            // Act
            Action action = () => container.GetInstance<ICommand>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "ServiceDependingOn<ILogger> contains the parameter with name 'dependency' and type " +
                "ILogger, but ILogger is not registered",
                action);
        }

        [TestMethod]
        public void GetInstance_AutoVerificationEnabled_ShouldDiagnoseContainerOnFirstCall()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.EnableAutoVerification = true;

            // Registering a Singleton service depending on a register Transient type ILogger
            container.Register<ServiceDependingOn<ILogger>>(Lifestyle.Singleton);
            container.Register<ILogger, NullLogger>(Lifestyle.Transient);

            container.Register<ICommand, ConcreteCommand>();

            // Act
            Action action = () => container.GetInstance<ICommand>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "ServiceDependingOn<ILogger> (Singleton) depends on ILogger implemented by NullLogger (Transient)",
                action);
        }

        [TestMethod]
        public void GetInstance_OnInvalidConfigurationAutoVerificationEnabled_ShouldThrowExceptionExplainingAutoVerificationIsEnabled()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.EnableAutoVerification = true;

            // Registering a service depending on an unregister type ILogger
            container.Register<ServiceDependingOn<ILogger>>();

            container.Register<ICommand, ConcreteCommand>();

            // Act
            Action action = () => container.GetInstance<ICommand>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                Verification was triggered because Container.Options.EnableAutoVerification was enabled. 
                To prevent the container from being verified on first resolve, set
                Container.Options.EnableAutoVerification to false."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstance_OnDiagnosticErrorWithAutoVerificationEnabled_ShouldThrowExceptionExplainingAutoVerificationIsEnabled()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.EnableAutoVerification = true;

            // Registering a Singleton service depending on a register Transient type ILogger
            container.Register<ServiceDependingOn<ILogger>>(Lifestyle.Singleton);
            container.Register<ILogger, NullLogger>(Lifestyle.Transient);

            container.Register<ICommand, ConcreteCommand>();

            // Act
            Action action = () => container.GetInstance<ICommand>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                Verification was triggered because Container.Options.EnableAutoVerification was enabled. 
                To prevent the container from being verified on first resolve, set
                Container.Options.EnableAutoVerification to false."
                .TrimInside(),
                action);
        }

        public class CaptivatingCompositeLogger<TCollection> : ILogger
            where TCollection : IEnumerable<ILogger>
        {
            private readonly List<ILogger> captiveLoggers;

            public CaptivatingCompositeLogger(TCollection loggers) =>
                this.captiveLoggers = loggers.ToList();
        }
    }
}