namespace SimpleInjector.Tests.Unit
{
    using System;
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
