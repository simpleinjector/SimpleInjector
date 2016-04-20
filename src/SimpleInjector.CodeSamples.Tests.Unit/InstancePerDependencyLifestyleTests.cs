namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InstancePerDependencyLifestyleTests
    {
        [TestMethod]
        public void GetInstance_ResolvingSingletonDependingOnInstancePerDependencyInstance_Succeeds()
        {
            // Arrange
            var container = new Container();

            var lifestyle = new InstancePerDependencyLifestyle();

            container.RegisterSingleton<CommandWithILoggerDependency>();

            container.Register<ILogger, NullLogger>(new InstancePerDependencyLifestyle());

            // Act
            container.GetInstance<CommandWithILoggerDependency>();
        }

        [TestMethod]
        public void Verify_RegistrationWithSingletonDependingOnInstancePerDependencyInstance_Succeeds()
        {
            // Arrange
            var container = new Container();

            var lifestyle = new InstancePerDependencyLifestyle();

            container.RegisterSingleton<CommandWithILoggerDependency>();

            container.Register<ILogger, NullLogger>(new InstancePerDependencyLifestyle());

            // Act
            container.Verify();
        }

        [TestMethod]
        public void GetInstance_ResolvingTransientDependingOnInstancePerDependencyInstance_EveryInstanceGetsNewInstance()
        {
            // Arrange
            var container = new Container();

            var lifestyle = new InstancePerDependencyLifestyle();

            container.Register<CommandWithILoggerDependency>();

            container.Register<ILogger, NullLogger>(new InstancePerDependencyLifestyle());

            // Act
            var c1 = container.GetInstance<CommandWithILoggerDependency>();
            var c2 = container.GetInstance<CommandWithILoggerDependency>();

            // Assert
            Assert.AreNotSame(c1.Logger, c2.Logger);
        }
    }
}
