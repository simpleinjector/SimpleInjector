namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Lifestyles;

    [TestClass]
    public class GetRootRegistrationsTests
    {
        [TestMethod]
        public void GetRootRegistrations_DependencyTreeWithOneRoot_ReturnsOnlyThatRoot()
        {
            // Arrange
            Type[] expectedRoots = [typeof(ServiceWithDependency<AnotherServiceWithDependency<ILogger>>)];

            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register<ServiceWithDependency<AnotherServiceWithDependency<ILogger>>>();
            container.Register<AnotherServiceWithDependency<ILogger>>(Lifestyle.Scoped);
            container.Register<ILogger, NullLogger>(Lifestyle.Singleton);

            container.Verify();

            // Act
            InstanceProducer[] roots = container.GetRootRegistrations();

            // Assert
            Type[] actualRoots = roots.Select(r => r.ServiceType).ToArray();

            CollectionAssert.AreEquivalent(expectedRoots, actualRoots);
        }

        [TestMethod]
        public void GetRootRegistrations_DependencyTreeWithDecorator_ReturnsCorrectRoot()
        {
            // Arrange
            Type[] expectedRoots = [typeof(ICommandHandler<MyCommand>)];

            var container = new Container();

            container.Register<ICommandHandler<MyCommand>, MyCommandHandler>(); // Depends on ILogger

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(MyCommandHandlerDecorator<>));

            container.Register<ILogger, NoOpLogger>(Lifestyle.Singleton);

            container.Verify();

            // Act
            InstanceProducer[] roots = container.GetRootRegistrations();

            // Assert
            Type[] actualRoots = roots.Select(r => r.ServiceType).ToArray();

            CollectionAssert.AreEquivalent(expectedRoots, actualRoots, message:
                "Actual: " + string.Join(", ", actualRoots.Select(r => r.ToFriendlyName())));
        }

        private sealed class MyCommand;
        private sealed class MyCommandHandler(ILogger logger) : ICommandHandler<MyCommand> { }
        private sealed class MyCommandHandlerDecorator<T>(ICommandHandler<T> decoratee) : ICommandHandler<T> { }
        private sealed class NoOpLogger : ILogger;
    }
}
