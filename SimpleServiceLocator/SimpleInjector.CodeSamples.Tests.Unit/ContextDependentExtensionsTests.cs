namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System.Runtime.Remoting.Proxies;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Extensions;
    using SimpleInjector.Lifestyles;

    [TestClass]
    public class ContextDependentExtensionsTests
    {
        public interface ILogger
        {
            DependencyContext Context { get; }
        }

        public interface IRepository
        {
            ILogger Logger { get; }
        }

        public interface IService
        {
        }

        [TestMethod]
        public void GetInstance_ResolvingAConcreteTypeThatDependsOnAContextDependentType_InjectsExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<ILogger>(context => new ContectualLogger(context));

            // Act
            var handler = container.GetInstance<RepositoryThatDependsOnLogger>();

            // Assert
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), handler.Logger.Context.ServiceType);
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), handler.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_ResolvingAConcreteTypeWithInitializerThatDependsOnAContextDependentType_InjectsExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<ILogger>(context => new ContectualLogger(context));

            container.RegisterInitializer<RepositoryThatDependsOnLogger>(_ => { });

            // Act
            var handler = container.GetInstance<RepositoryThatDependsOnLogger>();

            // Assert
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), handler.Logger.Context.ServiceType);
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), handler.Logger.Context.ImplementationType);
        }
        
        [TestMethod]
        public void GetInstance_ResolvingAnInterfaceWhosImplementationDependsOnAContextDependentType_InjectsExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<ILogger>(context => new ContectualLogger(context));

            container.Register<IRepository, RepositoryThatDependsOnLogger>();

            // Act
            var handler = container.GetInstance<IRepository>() as RepositoryThatDependsOnLogger;

            // Assert
            Assert.AreEqual(typeof(IRepository), handler.Logger.Context.ServiceType);
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), handler.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_CalledDirectlyOnTheContextDependentType_InjectsADependencyContextWithoutServiceType()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<ILogger>(context => new ContectualLogger(context));

            // Act
            var logger = container.GetInstance<ILogger>() as ContectualLogger;

            // Assert
            Assert.AreEqual(null, logger.Context.ServiceType);
            Assert.AreEqual(null, logger.Context.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_TypeWithContextRegisteredMultipleLevelsDeep_GetsInjectedWithExpectedContext()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<ILogger>(context => new ContectualLogger(context));

            container.Register<IRepository, RepositoryThatDependsOnLogger>();

            // Act
            var service = container.GetInstance<ServiceThatDependsOnRepository>();

            // Assert
            var logger = service.InjectedRepository.Logger;

            Assert.AreEqual(typeof(IRepository), logger.Context.ServiceType);
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), logger.Context.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInterceptedTypeThatDependsOnAContextDependentType_InjectsExpectedType()
        {
            // Arrange
            var container = new Container();

            container.Register<IRepository, RepositoryThatDependsOnLogger>();

            // Since both RegisterWithContext en InterceptWith work by replacing the underlighing Expression,
            // RegisterWithContext should be able to work correctly, even if the Expression has been altered.
            container.InterceptWith<FakeInterceptor>(type => type == typeof(IRepository));

            container.RegisterWithContext<ILogger>(context => new ContectualLogger(context));

            // Act
            var repository = container.GetInstance<IRepository>();

            // Assert
            Assert_IsIntercepted(repository);
            Assert.AreEqual(typeof(IRepository), repository.Logger.Context.ServiceType);
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), repository.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInterceptedSingletonTypeThatDependsOnAContextDependentType_InjectsExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<IRepository, RepositoryThatDependsOnLogger>();

            container.InterceptWith<FakeInterceptor>(type => type == typeof(IRepository));

            container.RegisterWithContext<ILogger>(context => new ContectualLogger(context));

            // Act
            var repository = container.GetInstance<IRepository>();

            // Assert
            Assert_IsIntercepted(repository);
            Assert.AreEqual(typeof(IRepository), repository.Logger.Context.ServiceType);
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), repository.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_ResolvingATypeThatDependsOnInterceptedTypeWithAContextDependentDependency_InjectsExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<IRepository, RepositoryThatDependsOnLogger>();

            // Since InterceptWith alters the Expression of ILogger, this would make it harder for 
            // the Expression visitor of RegisterWithContext to find and alter this expression. So this is
            // an interesting test.
            container.InterceptWith<FakeInterceptor>(type => type == typeof(ILogger));

            container.RegisterWithContext<ILogger>(context => new ContectualLogger(context));

            // Act
            var repository = container.GetInstance<IRepository>();

            // Assert
            Assert_IsIntercepted(repository.Logger);
            Assert.AreEqual(typeof(IRepository), repository.Logger.Context.ServiceType);
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), repository.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void HybridLifestyle_WithContext_AppliesContextCorrectly1()
        {
            // Arrange
            var hybrid = new HybridLifestyle(() => true, Lifestyle.Transient, Lifestyle.Singleton);

            var container = new Container();

            container.RegisterWithContext<ILogger>(context => new ContectualLogger(context));

            container.Register<IRepository, RepositoryThatDependsOnLogger>(hybrid);

            // Act
            var repository = container.GetInstance<IRepository>();

            // Assert
            Assert.AreEqual(typeof(IRepository), repository.Logger.Context.ServiceType);
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), repository.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void HybridLifestyle_WithContext_AppliesContextCorrectly2()
        {
            // Arrange
            var hybrid = new HybridLifestyle(() => false, Lifestyle.Transient, Lifestyle.Singleton);

            var container = new Container();

            container.RegisterWithContext<ILogger>(context => new ContectualLogger(context));

            container.Register<IRepository, RepositoryThatDependsOnLogger>(hybrid);

            // Act
            var repository = container.GetInstance<IRepository>();

            // Assert
            Assert.AreEqual(typeof(IRepository), repository.Logger.Context.ServiceType);
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), repository.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void DecoratedInstance_DecoratorDependingOnContextDependencyResolvedAsRootType_AppliesContextCorrectly()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<ILogger>(context => new ContectualLogger(context));

            container.Register<IRepository, RepositoryThatDependsOnLogger>();
            container.Register<IService, ServiceThatDependsOnRepository>();

            container.RegisterDecorator(typeof(IService), typeof(ServiceDecoratorWithLoggerDependency));

            // Act
            var decorator = (ServiceDecoratorWithLoggerDependency)container.GetInstance<IService>();

            // Assert
            Assert.AreEqual(typeof(IService), decorator.Logger.Context.ServiceType);
            Assert.AreEqual(typeof(ServiceDecoratorWithLoggerDependency), decorator.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void DecoratedInstance_DecoratorDependingOnContextDependencyResolvedAsSubType_AppliesContextCorrectly()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<ILogger>(context => new ContectualLogger(context));

            container.Register<IRepository, RepositoryThatDependsOnLogger>();
            container.Register<IService, ServiceThatDependsOnRepository>();

            container.RegisterDecorator(typeof(IService), typeof(ServiceDecoratorWithLoggerDependency));

            // Act
            var controller = container.GetInstance<ControllerDependingOnService>();

            var decorator = (ServiceDecoratorWithLoggerDependency)controller.Service;

            // Assert
            Assert.AreEqual(typeof(IService), decorator.Logger.Context.ServiceType);
            Assert.AreEqual(typeof(ServiceDecoratorWithLoggerDependency), decorator.Logger.Context.ImplementationType);
        }

        private static void Assert_IsIntercepted(object proxy)
        {
            RealProxy realProxy = System.Runtime.Remoting.RemotingServices.GetRealProxy(proxy);

            Assert.IsNotNull(realProxy, "The given " + proxy.GetType().Name + " is not a proxy.");
        }

        public sealed class ControllerDependingOnService
        {
            public ControllerDependingOnService(IService service)
            {
                this.Service = service;
            }

            public IService Service { get; private set; }
        }

        public sealed class ServiceDecoratorWithLoggerDependency : IService
        {
            public ServiceDecoratorWithLoggerDependency(IService decorated, ILogger logger)
            {
                this.Decorated = decorated;
                this.Logger = logger;
            }

            public IService Decorated { get; private set; }

            public ILogger Logger { get; private set; }
        }

        public sealed class ServiceThatDependsOnRepository : IService
        {
            public ServiceThatDependsOnRepository(IRepository repository, ILogger logger,
                ConcreteCommand justAnExtraArgumentToMakeUsFindBugsFaster)
            {
                this.InjectedRepository = (RepositoryThatDependsOnLogger)repository;
                this.InjectedLogger = (ContectualLogger)logger;
            }

            public RepositoryThatDependsOnLogger InjectedRepository { get; private set; }

            public ContectualLogger InjectedLogger { get; private set; }
        }

        public sealed class RepositoryThatDependsOnLogger : IRepository
        {
            public RepositoryThatDependsOnLogger(ILogger logger)
            {
                this.Logger = logger;
            }

            public ILogger Logger { get; private set; }
        }

        public sealed class ContectualLogger : ILogger
        {
            public ContectualLogger(DependencyContext context)
            {
                Assert.IsNotNull(context, "context should not be null.");

                this.Context = context;
            }

            public DependencyContext Context { get; private set; }
        }

        public sealed class FakeInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
            }
        }
    }
}