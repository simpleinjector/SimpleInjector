namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Linq;
    using System.Runtime.Remoting.Proxies;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class ContextDependentExtensionsTests
    {
        public interface IContextualLogger
        {
            DependencyContext Context { get; }
        }

        public interface IRepository
        {
            IContextualLogger Logger { get; }
        }

        public interface IService
        {
        }

        [TestMethod]
        public void RegisterWithContext_CalledForAlreadyRegisteredService_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Register<IContextualLogger, ContextualLogger>();

            // Act
            Action action = () => 
                container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void GetInstance_ResolvingAConcreteTypeThatDependsOnAContextDependentType_InjectsExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            // Act
            var handler = container.GetInstance<RepositoryThatDependsOnLogger>();

            // Assert
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), handler.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_ResolvingAConcreteTypeWithInitializerThatDependsOnAContextDependentType_InjectsExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            container.RegisterInitializer<RepositoryThatDependsOnLogger>(_ => { });

            // Act
            var handler = container.GetInstance<RepositoryThatDependsOnLogger>();

            // Assert
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), handler.Logger.Context.ImplementationType);
        }
        
        [TestMethod]
        public void GetInstance_ResolvingAnInterfaceWhosImplementationDependsOnAContextDependentType_InjectsExpectedType()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            container.Register<IRepository, RepositoryThatDependsOnLogger>();

            // Act
            var handler = container.GetInstance<IRepository>() as RepositoryThatDependsOnLogger;

            // Assert
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), handler.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_CalledDirectlyOnTheContextDependentType_InjectsADependencyContextWithoutServiceType()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            // Act
            var logger = container.GetInstance<IContextualLogger>() as ContextualLogger;

            // Assert
            Assert.AreEqual(null, logger.Context.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_TypeWithContextRegisteredMultipleLevelsDeep_GetsInjectedWithExpectedContext()
        {
            // Arrange
            var container = new Container();

            container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            container.Register<IRepository, RepositoryThatDependsOnLogger>();

            // Act
            var service = container.GetInstance<ServiceThatDependsOnRepositoryAndCommand>();

            // Assert
            var logger = service.InjectedRepository.Logger;

            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), logger.Context.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_TypeWithContextMultipleLevelsWithAllSingletons_GetsInjectedWithExpectedContext()
        {
            // Arrange
            var container = new Container();

            container.Register<IService, ServiceThatDependsOnRepository>(Lifestyle.Singleton);
            container.Register<IRepository, RepositoryThatDependsOnLogger>(Lifestyle.Singleton);
            container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            container.GetRegistration(typeof(IContextualLogger)).Registration.SuppressDiagnosticWarning(
                DiagnosticType.LifestyleMismatch, "Depending on ContextualLogger is fine.");

            // Act
            var service = container.GetInstance<IService>() as ServiceThatDependsOnRepository;

            // Assert
            var logger = service.InjectedRepository.Logger;

            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), logger.Context.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInterceptedTypeThatDependsOnAContextDependentType_InjectsExpectedType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IRepository, RepositoryThatDependsOnLogger>();

            // Since both RegisterWithContext en InterceptWith work by replacing the underlighing Expression,
            // RegisterWithContext should be able to work correctly, even if the Expression has been altered.
            container.InterceptWith<FakeInterceptor>(type => type == typeof(IRepository));

            container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            // Act
            var repository = container.GetInstance<IRepository>();

            // Assert
            Assert_IsIntercepted(repository);
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), repository.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInterceptedSingletonTypeThatDependsOnAContextDependentType_InjectsExpectedType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IRepository, RepositoryThatDependsOnLogger>(Lifestyle.Transient);

            container.InterceptWith<FakeInterceptor>(type => type == typeof(IRepository));

            container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            // Act
            var repository = container.GetInstance<IRepository>();

            // Assert
            Assert_IsIntercepted(repository);
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), repository.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void GetInstance_ResolvingATypeThatDependsOnInterceptedTypeWithAContextDependentDependency_InjectsExpectedType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IRepository, RepositoryThatDependsOnLogger>(Lifestyle.Transient);

            // Since InterceptWith alters the Expression of ILogger, this would make it harder for 
            // the Expression visitor of RegisterWithContext to find and alter this expression. So this is
            // an interesting test.
            container.InterceptWith<FakeInterceptor>(type => type == typeof(IContextualLogger));

            container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            // Act
            var repository = container.GetInstance<IRepository>();

            // Assert
            Assert_IsIntercepted(repository.Logger);
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), repository.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void HybridLifestyle_WithContext_AppliesContextCorrectly1()
        {
            // Arrange
            var hybrid = Lifestyle.CreateHybrid(() => true, Lifestyle.Transient, Lifestyle.Singleton);

            var container = ContainerFactory.New();
            container.Options.SuppressLifestyleMismatchVerification = true;

            container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            container.Register<IRepository, RepositoryThatDependsOnLogger>(hybrid);

            // Act
            var repository = container.GetInstance<IRepository>();

            // Assert
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), repository.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void HybridLifestyle_WithContext_AppliesContextCorrectly2()
        {
            // Arrange
            var hybrid = Lifestyle.CreateHybrid(() => false, Lifestyle.Transient, Lifestyle.Singleton);

            var container = ContainerFactory.New();
            container.Options.SuppressLifestyleMismatchVerification = true;

            container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            container.Register<IRepository, RepositoryThatDependsOnLogger>(hybrid);

            // Act
            var repository = container.GetInstance<IRepository>();

            // Assert
            Assert.AreEqual(typeof(RepositoryThatDependsOnLogger), repository.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void DecoratedInstance_DecoratorDependingOnContextDependencyResolvedAsRootType_AppliesContextCorrectly()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            container.Register<IRepository, RepositoryThatDependsOnLogger>();
            container.Register<IService, ServiceThatDependsOnRepositoryAndCommand>();

            container.RegisterDecorator(typeof(IService), typeof(ServiceDecoratorWithLoggerDependency));

            // Act
            var decorator = (ServiceDecoratorWithLoggerDependency)container.GetInstance<IService>();

            // Assert
            Assert.AreEqual(typeof(ServiceDecoratorWithLoggerDependency), decorator.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void DecoratedInstance_DecoratorDependingOnContextDependencyResolvedAsSubType_AppliesContextCorrectly()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            container.Register<IRepository, RepositoryThatDependsOnLogger>();
            container.Register<IService, ServiceThatDependsOnRepositoryAndCommand>();

            container.RegisterDecorator(typeof(IService), typeof(ServiceDecoratorWithLoggerDependency));

            // Act
            var controller = container.GetInstance<ControllerDependingOnService>();

            var decorator = (ServiceDecoratorWithLoggerDependency)controller.Service;

            // Assert
            Assert.AreEqual(typeof(ServiceDecoratorWithLoggerDependency), decorator.Logger.Context.ImplementationType);
        }

        [TestMethod]
        public void ContextualInstance_Always_ContainsExpectedParameter()
        {
            // Arrange
            var expectedParameter = (
                from p in typeof(RepositoryThatDependsOnLogger).GetConstructors().Single().GetParameters()
                where p.ParameterType == typeof(IContextualLogger)
                select p)
                .Single();

            var container = ContainerFactory.New();

            container.RegisterWithContext<IContextualLogger>(context => new ContextualLogger(context));

            container.Register<IRepository, RepositoryThatDependsOnLogger>();

            // Act
            var logger = container.GetInstance<IRepository>().Logger;

            // Assert
            Assert.AreSame(expectedParameter, logger.Context.Parameter);
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

            public IService Service { get; }
        }

        public sealed class ServiceDecoratorWithLoggerDependency : IService
        {
            public ServiceDecoratorWithLoggerDependency(IService decorated, IContextualLogger logger)
            {
                this.Decorated = decorated;
                this.Logger = logger;
            }

            public IService Decorated { get; }

            public IContextualLogger Logger { get; }
        }

        public sealed class ServiceThatDependsOnRepository : IService
        {
            public ServiceThatDependsOnRepository(IRepository repository, IContextualLogger logger)
            {
                this.InjectedRepository = (RepositoryThatDependsOnLogger)repository;
                this.InjectedLogger = (ContextualLogger)logger;
            }

            public RepositoryThatDependsOnLogger InjectedRepository { get; }

            public ContextualLogger InjectedLogger { get; }
        }

        public sealed class ServiceThatDependsOnRepositoryAndCommand : IService
        {
            public ServiceThatDependsOnRepositoryAndCommand(IRepository repository, IContextualLogger logger,
                ConcreteCommand justAnExtraArgumentToMakeUsFindBugsFaster)
            {
                this.InjectedRepository = (RepositoryThatDependsOnLogger)repository;
                this.InjectedLogger = (ContextualLogger)logger;
            }

            public RepositoryThatDependsOnLogger InjectedRepository { get; }

            public ContextualLogger InjectedLogger { get; }
        }

        public sealed class RepositoryThatDependsOnLogger : IRepository
        {
            public RepositoryThatDependsOnLogger(IContextualLogger logger)
            {
                this.Logger = logger;
            }

            public IContextualLogger Logger { get; }
        }

        public sealed class ContextualLogger : IContextualLogger
        {
            public ContextualLogger(DependencyContext context)
            {
                Assert.IsNotNull(context, "context should not be null.");

                this.Context = context;
            }

            public DependencyContext Context { get; }
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