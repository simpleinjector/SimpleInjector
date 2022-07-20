namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FlowingScopeTests
    {
        [TestMethod]
        public void GetInstance_ResolvingScopedDependencyDirectlyFromScope_ResolvesTheInstanceAsScoped()
        {
            // Arrange
            var container = ContainerFactory.New();

            // We need a 'dummy' scoped lifestyle to be able to use Lifestyle.Scoped
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Register<ILogger, NullLogger>(Lifestyle.Scoped);

            var scope1 = new Scope(container);
            var scope2 = new Scope(container);

            // Act
            var s1 = scope1.GetInstance<ServiceDependingOn<ILogger>>();
            var s2 = scope1.GetInstance<ServiceDependingOn<ILogger>>();
            var s3 = scope2.GetInstance<ServiceDependingOn<ILogger>>();

            // Assert
            Assert.AreSame(s1.Dependency, s2.Dependency, "Logger was expected to be scoped but was transient.");
            Assert.AreNotSame(s3.Dependency, s2.Dependency, "Logger was expected to be scoped but was singleton.");
        }

        [TestMethod]
        public void GetInstance_LambdaThatCallsBackIntoContainerExecutedFromScopeResolve_ResolvesTheInstanceAsScoped()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            // Calling back into the container to get a scoped instance, from within an instanceCreator lambda,
            // should work, in case the the root object is resolved from a scope.
            container.Register<ILogger>(() => container.GetInstance<NullLogger>());
            container.Register<NullLogger>(Lifestyle.Scoped);
            container.Register<ServiceDependingOn<ILogger>>();

            var scope = new Scope(container);

            // Act
            var s1 = scope.GetInstance<ServiceDependingOn<ILogger>>();
            var s2 = scope.GetInstance<ServiceDependingOn<ILogger>>();

            // Assert
            Assert.AreSame(s1.Dependency, s2.Dependency, "Logger was expected to be scoped.");
        }

        [TestMethod]
        public void ScopedDecoratoreeFactory_SuppliedWithValidScopes_ReturnsInstancesAccordingToTheirLifestyle()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Register<IPlugin, PluginImpl>(Lifestyle.Scoped);
            container.RegisterDecorator<IPlugin, ScopedPluginProxy>(Lifestyle.Singleton);

            var proxy = (ScopedPluginProxy)container.GetInstance<IPlugin>();
            Func<Scope, IPlugin> factory = proxy.Factory;

            var validScope = new Scope(container);

            // Act
            var plugin1 = factory(validScope);
            var plugin2 = factory(validScope);
            var plugin3 = factory(new Scope(container));

            // Assert
            AssertThat.IsInstanceOfType(typeof(PluginImpl), plugin1);
            Assert.AreSame(plugin1, plugin2, "Instance is not scoped but Transient.");
            Assert.AreNotSame(plugin2, plugin3, "Instance is not scoped but Singleton.");
        }

        [TestMethod]
        public void ScopedDecoratoreeFactory_SuppliedWithContainerlessScope_ThrowsDescriptiveException()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Register<IPlugin, PluginImpl>(Lifestyle.Scoped);
            container.RegisterDecorator<IPlugin, ScopedPluginProxy>(Lifestyle.Singleton);

            var proxy = (ScopedPluginProxy)container.GetInstance<IPlugin>();
            Func<Scope, IPlugin> factory = proxy.Factory;

            var containerlessScope = Activator.CreateInstance<Scope>();

            // Act
            Action action = () => factory(containerlessScope);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "For scoped decoratee factories to function, they have to be supplied with a Scope " +
                "instance that references the Container for which the object graph has been built. " +
                "But the Scope instance, provided to this Func<Scope, IPlugin> delegate does not " +
                "belong to any container. Please ensure the supplied Scope instance is created " +
                "using the constructor overload that accepts a Container instance.",
                action);
        }

        [TestMethod]
        public void ScopedDecoratoreeFactory_SuppliedWithScopeOfDifferentContainer_ThrowsDescriptiveException()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Register<IPlugin, PluginImpl>(Lifestyle.Scoped);
            container.RegisterDecorator<IPlugin, ScopedPluginProxy>(Lifestyle.Singleton);

            var proxy = (ScopedPluginProxy)container.GetInstance<IPlugin>();
            Func<Scope, IPlugin> factory = proxy.Factory;

            var scopeFromAnotherContainer = new Scope(new Container());

            // Act
            Action action = () => factory(scopeFromAnotherContainer);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "For scoped decoratee factories to function, they have to be supplied with a Scope " +
                "instance that references the Container for which the object graph has been built. But the " +
                "Scope instance, provided to this Func<Scope, IPlugin> delegate, references a different " +
                "Container instance.",
                action);
        }

        [TestMethod]
        public void InjectedScopeDecorateeFactory_WhenSuppliedWithAScopeInstance_CreatesScopedInstancesBasedOnThatScope()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Register<ICommandHandler<int>, NullCommandHandler<int>>(Lifestyle.Scoped);
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ScopedCommandHandlerProxy<>),
                Lifestyle.Singleton);

            var proxy = (ScopedCommandHandlerProxy<int>)container.GetInstance<ICommandHandler<int>>();
            var factory = proxy.DecorateeFactory;

            // Act
            var scope1 = new Scope(container);
            var handler1 = proxy.DecorateeFactory(scope1);
            var handler2 = proxy.DecorateeFactory(scope1);
            var handler3 = proxy.DecorateeFactory(new Scope(container));

            // Assert
            Assert.IsInstanceOfType(handler1, typeof(NullCommandHandler<int>));
            Assert.AreSame(handler1, handler2, "Handler is expected to be Scoped but was transient.");
            Assert.AreNotSame(handler2, handler3, "Handler is expected to be Scoped but was singleton.");
        }

        [TestMethod]
        public void ResolvingCollection_WithScopedElement_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            var scope = new Scope(container);

            var collection = scope.GetInstance<IEnumerable<ILogger>>();

            // Act
            collection.ToArray();
        }

        [TestMethod]
        public void ResolvingCollection_WithScopedElement_ResolvesTheElementAsScoped()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            var scope1 = new Scope(container);
            var scope2 = new Scope(container);

            // Act
            var logger1a = scope1.GetInstance<IEnumerable<ILogger>>().First();
            var logger1b = scope1.GetInstance<IEnumerable<ILogger>>().First();
            var logger2 = scope2.GetInstance<IEnumerable<ILogger>>().First();

            // Assert
            Assert.AreSame(logger1a, logger1b);
            Assert.AreNotSame(logger1a, logger2);
        }

        [TestMethod]
        public void ResolvingCollection_WithScopedElement_ResolvesTheElementAsScoped2()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Singleton);

            container.RegisterSingleton<ServiceDependingOn<IEnumerable<ILogger>>>();

            // Act
            container.Verify();
        }

        [TestMethod]
        public void ResolvingCollection_WithScopedElement_ResolvesTheElementAsScoped3()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            container.RegisterSingleton<ServiceDependingOn<IEnumerable<ILogger>>>();

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(@"
                ServiceDependingOn<IEnumerable<ILogger>> (Singleton) depends on IEnumerable<ILogger> (Scoped).
                IEnumerable<ILogger> was registered as Scoped by Simple Injector, because you set 
                'Options.DefaultScopedLifestyle' to 'ScopedLifestyle.Flowing' while one or more elements of 
                IEnumerable<ILogger> were registered as Scoped. This caused Simple Injector to capture the 
                active Scope inside the IEnumerable<ILogger> and forced its lifestyle to be lowered to 
                Scoped."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void GetInstanceScope_CalledMultipleTimesFromTheSameScope_ReturnsTheSameScopedInstances()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Register<ILogger, NullLogger>(Lifestyle.Scoped);

            container.Register<ServiceDependingOn<ILogger>>();

            InstanceProducer producer = container.GetRegistration<ServiceDependingOn<ILogger>>();

            var scope = new Scope(container);

            // Act
            var service1 = producer.GetInstance(scope) as ServiceDependingOn<ILogger>;
            var service2 = producer.GetInstance(scope) as ServiceDependingOn<ILogger>;

            // Assert
            Assert.AreSame(service1.Dependency, service2.Dependency);
        }

        [TestMethod]
        public void GetInstanceScope_CalledWithDifferentScopes_ReturnsDifferentScopedInstances()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Register<ILogger, NullLogger>(Lifestyle.Scoped);

            container.Register<ServiceDependingOn<ILogger>>();

            InstanceProducer producer = container.GetRegistration<ServiceDependingOn<ILogger>>();

            // Act
            var service1 = producer.GetInstance(new Scope(container)) as ServiceDependingOn<ILogger>;
            var service2 = producer.GetInstance(new Scope(container)) as ServiceDependingOn<ILogger>;

            // Assert
            Assert.AreNotSame(service1.Dependency, service2.Dependency);
        }

        [TestMethod]
        public void ScopeGetInstance_ResolvingArrayOnASecondScopeAfterTheFirstIsDisposed_ResultsInANewScopedInstance()
        {
            this.ScopeGetInstance_ResolvingCollectionOnASecondScopeAfterTheFirstIsDisposed_ResultsInANewScopedInstance(
                scope => scope.GetInstance<ILogger[]>().Single());
        }

        [TestMethod]
        public void ScopeGetInstance_ResolvingEnumerableOnASecondScopeAfterTheFirstIsDisposed_ResultsInANewScopedInstance()
        {
            this.ScopeGetInstance_ResolvingCollectionOnASecondScopeAfterTheFirstIsDisposed_ResultsInANewScopedInstance(
                scope => scope.GetInstance<IEnumerable<ILogger>>().Single());
        }

        [TestMethod]
        public void ScopeGetInstance_ResolvingReadOnlyCollectionOnASecondScopeAfterTheFirstIsDisposed_ResultsInANewScopedInstance()
        {
            this.ScopeGetInstance_ResolvingCollectionOnASecondScopeAfterTheFirstIsDisposed_ResultsInANewScopedInstance(
                scope => scope.GetInstance<ReadOnlyCollection<ILogger>>().Single());
        }

        private void ScopeGetInstance_ResolvingCollectionOnASecondScopeAfterTheFirstIsDisposed_ResultsInANewScopedInstance(
            Func<Scope, ILogger> loggerResolver)
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;
            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            container.Verify();

            ILogger logger1, logger2;

            // Act
            using (var scope = new Scope(container))
            {
                logger1 = loggerResolver(scope);
            }

            using (var scope = new Scope(container))
            {
                logger2 = scope.GetInstance<ReadOnlyCollection<ILogger>>().Single();
            }

            // Assert
            Assert.AreNotSame(logger1, logger2);
        }

        [TestMethod]
        public void ScopeGetInstance_ResolvingTypeDependingOnArrayOnASecondScopeAfterTheFirstIsDisposed_ResultsInANewScopedInstance()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;
            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);
            container.Register<ServiceDependingOn<ILogger[]>>();

            ILogger logger1;
            ILogger logger2;

            using (var scope = new Scope(container))
            {
                logger1 = scope.GetInstance<ServiceDependingOn<ILogger[]>>().Dependency.Single();
            }

            using (var scope = new Scope(container))
            {
                logger2 = scope.GetInstance<ServiceDependingOn<ILogger[]>>().Dependency.Single();
            }

            // Assert
            Assert.AreNotSame(logger1, logger2);
        }

        [TestMethod]
        public void ScopeGetInstance_ScopeRegistrationsBothAvailableThroughCollectionsAsThroughOneToOneMappings_ResolveAsTheSameInstanceWithinASingleScope()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;
            container.Register<ILogger, NullLogger>(Lifestyle.Scoped);
            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            var scope1 = new Scope(container);
            var scope2 = new Scope(container);

            // Act
            var singleInstance1 = scope1.GetInstance<ILogger>();
            var collectionInstance1 = scope1.GetInstance<ILogger[]>().Single();

            var singleInstance2 = scope2.GetInstance<ILogger>();
            var collectionInstance2 = scope2.GetInstance<ILogger[]>().Single();

            // Assert
            Assert.AreSame(singleInstance1, collectionInstance1);
            Assert.AreNotSame(singleInstance1, singleInstance2);
            Assert.AreSame(singleInstance2, collectionInstance2);
        }

        public class ScopedPluginProxy : IPlugin
        {
            public readonly Func<Scope, IPlugin> Factory;
            public ScopedPluginProxy(Func<Scope, IPlugin> factory) => this.Factory = factory;
        }

        public sealed class ScopedCommandHandlerProxy<T> : ICommandHandler<T>
        {
            public readonly Func<Scope, ICommandHandler<T>> DecorateeFactory;

            public ScopedCommandHandlerProxy(Func<Scope, ICommandHandler<T>> decorateeFactory)
            {
                this.DecorateeFactory = decorateeFactory;
            }
        }
    }
}