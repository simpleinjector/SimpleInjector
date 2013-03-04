namespace SimpleInjector.Interception.Tests.Unit
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Remoting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public interface ILogger
    {
    }

    public interface IRepository
    {
        object Get(int key);
    }

    public interface ICommandHandler<TCommand>
    {
    }
    
    [TestClass]
    public class InterceptorExtesionsTests
    {
        [TestMethod]
        public void GetInstance_ForInterceptedInterface_Intercepts()
        {
            // Arrange
            var container = new Container();

            container.Register<IRepository, InMemoryRepository>();
            container.Intercept<IRepository>().With<EmptyInterceptor>();

            // Act
            var repository = container.GetInstance<IRepository>();

            // Assert
            Assert_IsIntercepted(repository);
        }

        [TestMethod]
        public void GetInstance_ForInterceptedInterface_Intercepts2()
        {
            // Arrange
            var container = new Container();

            container.Register<IRepository, InMemoryRepository>();
            container.Intercept(typeof(IRepository)).With<EmptyInterceptor>();

            // Act
            var repository = container.GetInstance<InMemoryRepository>();

            // Assert
            Assert_IsNotIntercepted(repository);
        }

        [TestMethod]
        public void GetInstance_OnConcreteTypeImplementingInterceptedInterface_DoesNotIntercept()
        {
            // Arrange
            var container = new Container();

            container.Register<IRepository, InMemoryRepository>();
            container.Intercept<IRepository>().With<EmptyInterceptor>();

            // Act
            var repository = container.GetInstance<InMemoryRepository>();

            // Assert
            Assert_IsNotIntercepted(repository);
        }

        [TestMethod]
        public void GetInstance_OnNotInterceptedInterface_DoesNotIntercept()
        {
            // Arrange
            var container = new Container();

            container.Register<ILogger, NullLogger>();
            container.Register<IRepository, InMemoryRepository>();
            container.Intercept<IRepository>().With<EmptyInterceptor>();

            // Act
            var logger = container.GetInstance<ILogger>();

            // Assert
            Assert_IsNotIntercepted(logger);
        }
        
        [TestMethod]
        public void GetInstance_OnNotInterceptedInterface_DoesNotIntercept2()
        {
            // Arrange
            var container = new Container();

            container.Register<ILogger, NullLogger>();
            container.Register<IRepository, InMemoryRepository>();
            container.Intercept(typeof(IRepository)).With<EmptyInterceptor>();

            // Act
            var logger = container.GetInstance<ILogger>();

            // Assert
            Assert_IsNotIntercepted(logger);
        }

        [TestMethod]
        public void GetInstance_InterceptedGenericTypeDefinition_InterceptsClosedGenericVersion()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommandHandler<int>, IntCommandHandler>();
            container.Intercept(typeof(ICommandHandler<>)).With<EmptyInterceptor>();

            // Act
            var handler = container.GetInstance<ICommandHandler<int>>();

            // Assert
            Assert_IsIntercepted(handler);
        }

        [TestMethod]
        public void GetInstance_InterceptionFilterUsingDelegate_InterceptsExpectedTypes()
        {
            // Arrange
            var container = new Container();

            container.Register<ILogger, NullLogger>();
            container.Register<IRepository, InMemoryRepository>();
            container.Intercept(type => type.IsInterface).With<EmptyInterceptor>();

            // Act
            var logger = container.GetInstance<ILogger>();
            var repository = container.GetInstance<IRepository>();
            var implementation = container.GetInstance<NullLogger>();

            // Assert
            Assert_IsIntercepted(logger);
            Assert_IsIntercepted(repository);
            Assert_IsNotIntercepted(implementation);
        }

        [TestMethod]
        public void GetInstance_WithInterceptedInterface_InterceptorGetsInitialized()
        {
            // Arrange
            bool interceptorGetsInitialized = false;

            var container = new Container();

            container.Register<IRepository, InMemoryRepository>();
            container.Intercept<IRepository>().With<EmptyInterceptor>();

            container.RegisterInitializer<EmptyInterceptor>(interceptor => interceptorGetsInitialized = true);
            
            // Act
            var repository = container.GetInstance<IRepository>();
            
            // Assert
            Assert.IsTrue(interceptorGetsInitialized);
        }

        [TestMethod]
        public void Proceed_CalledOnTheInvocation_InvokesTheInterceptee()
        {
            // Arrange
            var realRepository = new InMemoryRepository();

            var container = new Container();

            container.RegisterSingle<IRepository>(realRepository);
            container.Intercept<IRepository>().With(new ActionInterceptor(invocation =>
            {
                invocation.Proceed();   
            }));

            IRepository proxy = container.GetInstance<IRepository>();

            // Act
            proxy.Get(0);

            // Assert
            Assert.IsTrue(realRepository.Invoked);
        }

        [TestMethod]
        public void CallingProxy_Always_CallsTheInterceptor()
        {
            // Arrange
            var interceptor = new EmptyInterceptor();

            var container = new Container();

            container.Register<IRepository, InMemoryRepository>();
            container.Intercept<IRepository>().With(interceptor);

            var proxy = container.GetInstance<IRepository>();

            // Act
            proxy.Get(0);

            // Assert
            Assert.IsTrue(interceptor.Invoked);
        }

        [TestMethod]
        public void CallingAMethodOnTheInterceptedType_Always_SuppliesTheInterceptorWithAnInvocationWithTheCorrectMethod()
        {
            // Arrange
            var interceptor = new EmptyInterceptor();

            var container = new Container();

            container.Register<IRepository, InMemoryRepository>();
            container.Intercept<IRepository>().With(interceptor);

            var repository = container.GetInstance<IRepository>();

            // Act
            repository.Get(0);

            // Assert
            Assert.AreEqual(GetMethod<IRepository>(r => r.Get(0)), interceptor.SuppliedInvocation.Method);
        }
        
        [TestMethod]
        public void CallingAMethodOnTheInterceptedType_Always_SuppliesTheInterceptorWithAnInvocationWithTheCorrectReturnValue()
        {
            // Arrange
            object returnedInstance = new object();
            var realRepository = new InMemoryRepository { InstanceToReturn = returnedInstance };
            var interceptor = new EmptyInterceptor();

            var container = new Container();

            container.RegisterSingle<IRepository>(realRepository);
            container.Intercept<IRepository>().With(interceptor, Lifestyle.Singleton);

            var repository = container.GetInstance<IRepository>();

            // Act
            repository.Get(0);

            // Assert
            Assert.AreEqual(returnedInstance, interceptor.SuppliedInvocation.ReturnValue);
        }

        [TestMethod]
        public void GetInstance_InstanceInterceptedWithSingletonInterceptor_AlwaysReturnsSameInstance()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<IRepository, InMemoryRepository>();
            container.Intercept<IRepository>().With<EmptyInterceptor>(Lifestyle.Singleton);

            // Act
            var instance1 = container.GetInstance<IRepository>();
            var instance2 = container.GetInstance<IRepository>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(instance1, instance2));
        }

        private static MethodBase GetMethod<T>(Expression<Action<T>> methodSelector)
        {
            var methodCall = (MethodCallExpression)methodSelector.Body;

            return methodCall.Method;
        }

        private static void Assert_IsIntercepted(object instance)
        {
            Assert.IsTrue(RemotingServices.IsTransparentProxy(instance));
        }

        private static void Assert_IsNotIntercepted(object instance)
        {
            Assert.IsFalse(RemotingServices.IsTransparentProxy(instance));
        }
    }

    public class SomeRepository : IRepository
    {
        public SomeRepository(ILogger logger)
        {
        }

        public object Get(int key)
        {
            throw new NotImplementedException();
        }
    }

    public class LoggingInterceptor : IInterceptor
    {
        public LoggingInterceptor(ILogger logger)
        {
        }

        public void Intercept(IInvocation invocation)
        {
            throw new NotImplementedException();
        }
    }

    public class NullLogger : ILogger 
    { 
    }

    public class IntCommandHandler : ICommandHandler<int> 
    { 
    }

    public class InMemoryRepository : IRepository
    {
        public int SuppliedValue { get; private set; }

        public object InstanceToReturn { get; set; }

        public bool Invoked { get; private set; }

        public object Get(int key)
        {
            this.SuppliedValue = key;

            this.Invoked = true;

            return this.InstanceToReturn;
        }
    }

    public class ActionInterceptor : IInterceptor
    {
        private Action<IInvocation> intercept;

        public ActionInterceptor(Action<IInvocation> intercept)
        {
            this.intercept = intercept;
        }

        public void Intercept(IInvocation invocation)
        {
            this.intercept(invocation);
        }
    }

    public class EmptyInterceptor : IInterceptor
    {
        public IInvocation SuppliedInvocation { get; private set; }

        public bool Invoked 
        {
            get { return this.SuppliedInvocation != null; }
        }

        public virtual void Intercept(IInvocation invocation)
        {
            this.SuppliedInvocation = invocation;

            invocation.Proceed();
        }
    }

    public class BeforeInterceptor : EmptyInterceptor
    {
        private Action<IInvocation> before;

        public BeforeInterceptor(Action<IInvocation> before)
        {
            this.before = before;
        }

        public override void Intercept(IInvocation invocation)
        {
            this.before(invocation);

            base.Intercept(invocation);
        }
    }
}