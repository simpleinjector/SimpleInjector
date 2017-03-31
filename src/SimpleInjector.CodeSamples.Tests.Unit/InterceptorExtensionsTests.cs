namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    public interface IWithOutAndRef
    {
        int Operate(ref string refValue, out string outValue);
    }

    [TestClass]
    public class InterceptorExtensionsTests
    {
        private static readonly Func<Type, bool> IsACommandPredicate = type => type.Name.EndsWith("Command");
        private static readonly Func<Type, bool> IsInterface = type => type.IsInterface;

        [TestMethod]
        public void InterceptWithGenericArgAndPredicate_InterceptingInterfacesEndingWithCommand_InterceptsTheInstance()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingleton<ILogger>(logger);
            container.Register<ICommand, CommandThatLogsOnExecute>();

            container.InterceptWith<InterceptorThatLogsBeforeAndAfter>(IsACommandPredicate);

            container.RegisterInitializer<InterceptorThatLogsBeforeAndAfter>(i => i.BeforeText = "Start ");
            container.RegisterInitializer<CommandThatLogsOnExecute>(c => c.ExecuteLogMessage = "Executing");
            container.RegisterInitializer<InterceptorThatLogsBeforeAndAfter>(i => i.AfterText = " Done");

            // Act
            var command = container.GetInstance<ICommand>();

            command.Execute();

            // Assert
            Assert.AreEqual("Start Executing Done", logger.Message);
        }

        [TestMethod]
        public void Intercept_Collection_InterceptsTheInstances()
        {
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingleton<ILogger>(logger);
            container.RegisterCollection<ICommand>(new[] { typeof(ConcreteCommand), typeof(ConcreteCommand) });

            container.InterceptWith<InterceptorThatLogsBeforeAndAfter>(IsACommandPredicate);

            container.RegisterInitializer<InterceptorThatLogsBeforeAndAfter>(i => i.BeforeText = "Log ");

            // Act
            var commands = container.GetAllInstances<ICommand>().ToList();

            commands.ForEach(Execute);

            // Assert
            Assert.AreEqual("Log Log ", logger.Message);
        }

        [TestMethod]
        public void Intercept_CollectionWithRegistrationInstances_InterceptsTheInstances()
        {
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingleton<ILogger>(logger);
            container.RegisterCollection<ICommand>(new[]
            {
                Lifestyle.Transient.CreateRegistration(typeof(ConcreteCommand), container),
                Lifestyle.Transient.CreateRegistration(typeof(ConcreteCommand), container),
            });

            container.InterceptWith<InterceptorThatLogsBeforeAndAfter>(IsACommandPredicate);

            container.RegisterInitializer<InterceptorThatLogsBeforeAndAfter>(i => i.BeforeText = "Log ");

            // Act
            var commands = container.GetAllInstances<ICommand>().ToList();

            commands.ForEach(Execute);

            // Assert
            Assert.AreEqual("Log Log ", logger.Message);
        }

        [TestMethod]
        public void Intercept_DecoratedGenericRegistrations_WorksLikeACharm()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingleton<ILogger>(logger);
            container.Register<ICommand, CommandThatLogsOnExecute>();

            container.Register(typeof(IValidator<>), typeof(LoggingValidator<>));

            container.RegisterDecorator(typeof(IValidator<>), typeof(LoggingValidatorDecorator<>));

            container.InterceptWith<InterceptorThatLogsBeforeAndAfter>(
                t => t.IsInterface && t.Name.StartsWith("IValidator"));

            container.RegisterInitializer<InterceptorThatLogsBeforeAndAfter>(i => i.BeforeText = "Inercepting ");
            container.RegisterInitializer<InterceptorThatLogsBeforeAndAfter>(i => i.AfterText = "Intercepted ");

            // Act
            container.GetInstance<IValidator<RealCommand>>().Validate(new RealCommand());

            // Assert
            Assert.AreEqual("Inercepting Decorating Validating Decorated Intercepted ", logger.Message);
        }

        [TestMethod]
        public void InterceptWithGenericArgAndPredicate_TwoInterceptors_InterceptsTheInstanceWithBothInterceptors()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingleton<ILogger>(logger);
            container.Register<ICommand, CommandThatLogsOnExecute>();

            container.InterceptWith<InterceptorThatLogsBeforeAndAfter>(IsACommandPredicate);
            container.InterceptWith<InterceptorThatLogsBeforeAndAfter2>(IsACommandPredicate);

            container.RegisterInitializer<InterceptorThatLogsBeforeAndAfter>(i => i.BeforeText = "Start1 ");
            container.RegisterInitializer<InterceptorThatLogsBeforeAndAfter2>(i => i.BeforeText = "Start2 ");
            container.RegisterInitializer<CommandThatLogsOnExecute>(c => c.ExecuteLogMessage = "Executing");
            container.RegisterInitializer<InterceptorThatLogsBeforeAndAfter>(i => i.AfterText = " Done1");
            container.RegisterInitializer<InterceptorThatLogsBeforeAndAfter2>(i => i.AfterText = " Done2");

            // Act
            var command = container.GetInstance<ICommand>();

            command.Execute();

            // Assert
            Assert.AreEqual("Start2 Start1 Executing Done1 Done2", logger.Message);
        }

        [TestMethod]
        public void InterceptWithGenericArgAndPredicate_InterceptingATransientWithSingletonInterceptor_ResultsInATransientProxy()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, FakeCommand>();

            container.Register<FakeInterceptor>(Lifestyle.Singleton);

            container.InterceptWith<FakeInterceptor>(IsACommandPredicate);

            // Act
            var command1 = container.GetInstance<ICommand>();
            var command2 = container.GetInstance<ICommand>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(command1, command2), "The proxy is expected to " +
                "be transient, since the interceptee is transient.");
        }

        [TestMethod]
        public void InterceptWithGenericArgAndPredicate_InterceptingASingletonWithATransientInterceptor_ResultsInATransientProxy()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, FakeCommand>(Lifestyle.Singleton);

            container.InterceptWith<FakeInterceptor>(IsACommandPredicate);

            // Act
            var command1 = container.GetInstance<ICommand>();
            var command2 = container.GetInstance<ICommand>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(command1, command2), "The proxy is expected to " +
                "be transient, because even if the interceptee is a singleton, the interceptor itself " +
                "is transient and it is possible it isn't thread-safe.");
        }

        [TestMethod]
        public void InterceptWithGenericArgAndPredicate_InterceptingASingletonWithASingletonInterceptor_ResultsInASingletonProxy()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommand, FakeCommand>(Lifestyle.Singleton);

            container.Register<FakeInterceptor>(Lifestyle.Singleton);

            container.InterceptWith<FakeInterceptor>(IsACommandPredicate);

            // Act
            var command1 = container.GetInstance<ICommand>();
            var command2 = container.GetInstance<ICommand>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(command1, command2), "The proxy is expected to " +
                "be singleton, because both the interceptee and the interceptor are singletons, which " +
                "makes it useless to create a new (expensive) proxy for them on each request.");
        }

        [TestMethod]
        public void InterceptWithGenericArgAndPredicate_RequestingAConcreteType_WillNotBeIntercepted()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = ContainerFactory.New();

            container.RegisterSingleton<ILogger>(logger);

            container.InterceptWith<InterceptorThatLogsBeforeAndAfter>(type => type == typeof(ICommand));

            container.RegisterInitializer<InterceptorThatLogsBeforeAndAfter>(i => i.BeforeText = "Start ");
            container.RegisterInitializer<CommandThatLogsOnExecute>(c => c.ExecuteLogMessage = "Executing");
            container.RegisterInitializer<InterceptorThatLogsBeforeAndAfter>(i => i.AfterText = " Done");

            // Act
            var command = container.GetInstance<CommandThatLogsOnExecute>();

            command.Execute();

            // Assert
            Assert.AreEqual("Executing", logger.Message);
            Assert.IsTrue(command.GetType() == typeof(CommandThatLogsOnExecute));
        }

        [TestMethod]
        public void InterceptWithInstanceAndPredicate_InterceptingInterfacesEndingWithCommand_InterceptsTheInstance()
        {
            // Arrange
            var logger = new FakeLogger();

            var singletonInterceptor = new InterceptorThatLogsBeforeAndAfter(logger)
            {
                BeforeText = "Start ",
                AfterText = " Done"
            };

            var container = ContainerFactory.New();

            container.RegisterSingleton<ILogger>(logger);
            container.Register<ICommand, CommandThatLogsOnExecute>();

            container.InterceptWith(singletonInterceptor, IsACommandPredicate);

            container.RegisterInitializer<CommandThatLogsOnExecute>(c => c.ExecuteLogMessage = "Executing");

            // Act
            var command = container.GetInstance<ICommand>();

            command.Execute();

            // Assert
            Assert.AreEqual("Start Executing Done", logger.Message);
        }

        [TestMethod]
        public void InterceptWithInstanceAndPredicate_TwoInterceptors_InterceptsTheInstanceWithBothInterceptors()
        {
            // Arrange
            var logger = new FakeLogger();

            var interceptor1 = new InterceptorThatLogsBeforeAndAfter(logger)
            {
                BeforeText = "Start1 ",
                AfterText = " Done1"
            };

            var interceptor2 = new InterceptorThatLogsBeforeAndAfter(logger)
            {
                BeforeText = "Start2 ",
                AfterText = " Done2"
            };

            var container = ContainerFactory.New();

            container.RegisterSingleton<ILogger>(logger);
            container.Register<ICommand, CommandThatLogsOnExecute>();

            container.InterceptWith(interceptor1, IsACommandPredicate);
            container.InterceptWith(interceptor2, IsACommandPredicate);

            container.RegisterInitializer<CommandThatLogsOnExecute>(c => c.ExecuteLogMessage = "Executing");

            // Act
            var command = container.GetInstance<ICommand>();

            command.Execute();

            // Assert
            Assert.AreEqual("Start2 Start1 Executing Done1 Done2", logger.Message);
        }

        [TestMethod]
        public void InterceptWithInstanceAndPredicate_InterceptingATransient_ResultsInATransientProxy()
        {
            // Arrange
            var container = new Container();

            var interceptor = new FakeInterceptor();

            container.Register<ICommand, FakeCommand>();

            container.InterceptWith(interceptor, IsACommandPredicate);

            // Act
            var command1 = container.GetInstance<ICommand>();
            var command2 = container.GetInstance<ICommand>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(command1, command2), "The proxy is expected to " +
                "be transient, since the interceptee is transient.");
        }

        [TestMethod]
        public void InterceptWithInstanceAndPredicate_InterceptingASingleton_ResultsInASingletonProxy()
        {
            // Arrange
            var container = ContainerFactory.New();

            var interceptor = new FakeInterceptor();

            container.Register<ICommand, FakeCommand>(Lifestyle.Singleton);

            container.InterceptWith(interceptor, IsACommandPredicate);

            // Act
            var command1 = container.GetInstance<ICommand>();
            var command2 = container.GetInstance<ICommand>();

            // Assert
            Assert.IsTrue(object.ReferenceEquals(command1, command2), "The proxy is expected to " +
                "be singleton, because both the interceptee and the interceptor are singletons, which " +
                "makes it useless to create a new (expensive) proxy for them on each request.");
        }

        [TestMethod]
        public void InterceptWithFuncAndPredicate_InterceptingInterfacesEndingWithCommand_InterceptsTheInstance()
        {
            // Arrange
            var logger = new FakeLogger();

            Func<IInterceptor> interceptorCreator = () => new InterceptorThatLogsBeforeAndAfter(logger)
            {
                BeforeText = "Start ",
                AfterText = " Done"
            };

            var container = new Container();

            container.RegisterSingleton<ILogger>(logger);
            container.Register<ICommand, CommandThatLogsOnExecute>();

            container.InterceptWith(interceptorCreator, IsACommandPredicate);

            container.RegisterInitializer<CommandThatLogsOnExecute>(c => c.ExecuteLogMessage = "Executing");

            // Act
            var command = container.GetInstance<ICommand>();

            command.Execute();

            // Assert
            Assert.AreEqual("Start Executing Done", logger.Message);
        }

        [TestMethod]
        public void InterceptWithFuncAndPredicate_TwoInterceptors_InterceptsTheInstanceWithBothInterceptors()
        {
            // Arrange
            var logger = new FakeLogger();

            Func<IInterceptor> interceptorCreator1 = () => new InterceptorThatLogsBeforeAndAfter(logger)
            {
                BeforeText = "Start1 ",
                AfterText = " Done1"
            };

            Func<IInterceptor> interceptorCreator2 = () => new InterceptorThatLogsBeforeAndAfter(logger)
            {
                BeforeText = "Start2 ",
                AfterText = " Done2"
            };

            var container = new Container();

            container.RegisterSingleton<ILogger>(logger);
            container.Register<ICommand, CommandThatLogsOnExecute>();

            container.InterceptWith(interceptorCreator1, IsACommandPredicate);
            container.InterceptWith(interceptorCreator2, IsACommandPredicate);

            container.RegisterInitializer<CommandThatLogsOnExecute>(c => c.ExecuteLogMessage = "Executing");

            // Act
            var command = container.GetInstance<ICommand>();

            command.Execute();

            // Assert
            Assert.AreEqual("Start2 Start1 Executing Done1 Done2", logger.Message);
        }

        [TestMethod]
        public void InterceptWithFuncAndPredicate_InterceptingATransient_ResultsInATransientProxy()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, FakeCommand>();

            container.InterceptWith(() => new FakeInterceptor(), IsACommandPredicate);

            // Act
            var command1 = container.GetInstance<ICommand>();
            var command2 = container.GetInstance<ICommand>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(command1, command2), "The proxy is expected to " +
                "be transient, since the interceptor is created using a Func<T>, and there is no way to " +
                "determine whether the delegate always returns the same or a new instance.");
        }

        [TestMethod]
        public void InterceptWithFuncAndPredicate_InterceptingASingleton_ResultsInATransientProxy()
        {
            // Arrange
            var container = new Container();

            var interceptor = new FakeInterceptor();

            container.Register<ICommand, FakeCommand>(Lifestyle.Singleton);

            container.InterceptWith(() => interceptor, IsACommandPredicate);

            // Act
            var command1 = container.GetInstance<ICommand>();
            var command2 = container.GetInstance<ICommand>();

            // Assert
            Assert.IsFalse(object.ReferenceEquals(command1, command2), "The proxy is expected to " +
                "be transient, since the interceptor is created using a Func<T>, and there is no way to " +
                "determine whether the delegate always returns the same or a new instance.");
        }

        [TestMethod]
        public void InterceptWithFuncAndPredicate_InterceptingWithExpressionBuiltEventArgs_RunsSuccessfully()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, FakeCommand>(Lifestyle.Singleton);
            container.Register<ILogger, FakeLogger>(Lifestyle.Singleton);

            container.InterceptWith(e => new BuiltInfoInterceptor(e), type => type.IsInterface);

            // Act
            var command = container.GetInstance<ICommand>();
            var logger = container.GetInstance<ILogger>();

            command.Execute();
            logger.Log("foo");
        }

        [TestMethod]
        public void InterceptWith_WithInterceptorWithNoPublicConstructor_ThrowsExpressiveException()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, FakeCommand>();

            // Act
            Action action = () => container.InterceptWith<InterceptorWithInternalConstructor>(IsACommandPredicate);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "For the container to be able to create " +
                "InterceptorExtensionsTests.InterceptorWithInternalConstructor it should have only " +
                "one public constructor",
                action);
        }

        [TestMethod]
        public void GetInstance_OnInterceptedTypeWithInterceptorWithUnresolvableDependency_ThrowsExpressiveException()
        {
            // Arrange
            var container = new Container();

            container.Register<ICommand, FakeCommand>();

            // The interceptor depends on ILogger, but it is not registered.
            container.InterceptWith<InterceptorWithDependencyOnLogger>(IsACommandPredicate);

            try
            {
                // Act
                container.GetInstance<ICommand>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("The constructor of type " +
                    "InterceptorExtensionsTests.InterceptorWithDependencyOnLogger contains the parameter " +
                    "with name 'logger' and type ILogger that is not registered."),
                    "Actual: " + ex.Message);
            }
        }

        [TestMethod]
        public void CallingInterceptedMethodWithReturnValue_InterceptedWithPassThroughInterceptor_ReturnsTheExpectedValue()
        {
            // Arrange
            int expectedReturnValue = 3;

            var container = new Container();

            var interceptee = new WithOutAndRef { ReturnValue = expectedReturnValue };

            container.RegisterSingleton<IWithOutAndRef>(interceptee);

            container.InterceptWith<FakeInterceptor>(IsInterface);

            var intercepted = container.GetInstance<IWithOutAndRef>();

            // Act
            string unused1 = null;
            string unused2;
            int actualReturnValue = intercepted.Operate(ref unused1, out unused2);

            // Assert
            Assert.AreEqual(expectedReturnValue, actualReturnValue);
        }

        [TestMethod]
        public void CallingInterceptedMethodWithRefArgument_InterceptedWithPassThroughInterceptor_PassesTheRefArgumentToTheInterceptee()
        {
            // Arrange
            string expectedRefValue = "ABC";

            var container = new Container();

            var interceptee = new WithOutAndRef();

            container.RegisterSingleton<IWithOutAndRef>(interceptee);

            container.InterceptWith<FakeInterceptor>(IsInterface);

            var intercepted = container.GetInstance<IWithOutAndRef>();

            // Act
            string refValue = expectedRefValue;
            string unused;
            intercepted.Operate(ref refValue, out unused);

            // Assert
            Assert.AreEqual(expectedRefValue, interceptee.SuppliedRefValue);
        }

        [TestMethod]
        public void CallingInterceptedMethodWithRefArgument_InterceptedWithPassThroughInterceptor_ChangesTheRefValue()
        {
            // Arrange
            string expectedRefValue = "ABC";

            var container = new Container();

            var interceptee = new WithOutAndRef { OutputRefValue = expectedRefValue };

            container.RegisterSingleton<IWithOutAndRef>(interceptee);

            container.InterceptWith<FakeInterceptor>(IsInterface);

            var intercepted = container.GetInstance<IWithOutAndRef>();

            // Act
            string unused;
            string actualRefValue = null;
            intercepted.Operate(ref actualRefValue, out unused);

            // Assert
            Assert.AreEqual(expectedRefValue, actualRefValue);
        }

        [TestMethod]
        public void CallingInterceptedMethodWithOutArgument_InterceptedWithPassThroughInterceptor_ReturnsTheExpectedOutValue()
        {
            // Arrange
            string expectedOutValue = "DEF";

            var container = new Container();

            var interceptee = new WithOutAndRef { OutValue = expectedOutValue };

            container.RegisterSingleton<IWithOutAndRef>(interceptee);

            container.InterceptWith<FakeInterceptor>(IsInterface);

            var intercepted = container.GetInstance<IWithOutAndRef>();

            // Act
            string actualOutValue;
            string unused = null;
            intercepted.Operate(ref unused, out actualOutValue);

            // Assert
            Assert.AreEqual(expectedOutValue, actualOutValue);
        }

        [TestMethod]
        public void CallingAnInterceptedMethod_InterceptorThatChangesTheInputParameters_GetsForwardedToTheInterceptee()
        {
            // Arrange  
            string expectedValue = "XYZ";

            var container = new Container();

            var interceptee = new WithOutAndRef();

            var interceptor = new DelegateInterceptor();

            interceptor.Intercepting += invocation =>
            {
                invocation.Arguments[0] = expectedValue;
            };

            container.RegisterSingleton<IWithOutAndRef>(interceptee);
            container.InterceptWith(interceptor, IsInterface);

            var intercepted = container.GetInstance<IWithOutAndRef>();

            // Act
            string refValue = "Something different";
            string unused;
            intercepted.Operate(ref refValue, out unused);

            // Assert
            Assert.AreEqual(expectedValue, interceptee.SuppliedRefValue);
        }

        [TestMethod]
        public void CallingAnInterceptedMethod_InterceptorThatChangesAnOutputParameter_OutputParameterFlowsBackToTheCaller()
        {
            // Arrange  
            string expectedOutValue = "KLM";

            var container = new Container();

            var interceptee = new WithOutAndRef();

            var interceptor = new DelegateInterceptor();

            interceptor.Intercepted += invocation =>
            {
                invocation.Arguments[1] = expectedOutValue;
            };

            container.RegisterSingleton<IWithOutAndRef>(interceptee);
            container.InterceptWith(interceptor, IsInterface);

            var intercepted = container.GetInstance<IWithOutAndRef>();

            // Act
            string unused = null;
            string actualOutValue;
            intercepted.Operate(ref unused, out actualOutValue);

            // Assert
            Assert.AreEqual(expectedOutValue, actualOutValue);
        }

        private static void Execute(ICommand command) => command.Execute();

        // Example interceptor
        private class InterceptorThatLogsBeforeAndAfter : IInterceptor
        {
            private readonly ILogger logger;

            public InterceptorThatLogsBeforeAndAfter(ILogger logger)
            {
                this.logger = logger;
            }

            public string BeforeText { get; set; }

            public string AfterText { get; set; }

            public void Intercept(IInvocation invocation)
            {
                this.Log(this.BeforeText);

                // Calls the decorated instance.
                invocation.Proceed();

                this.Log(this.AfterText);
            }

            private void Log(string message)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    this.logger.Log(message);
                }
            }
        }

        private class InterceptorThatLogsBeforeAndAfter2 : InterceptorThatLogsBeforeAndAfter
        {
            public InterceptorThatLogsBeforeAndAfter2(ILogger logger) : base(logger)
            {
            }
        }

        private class FakeInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
            }
        }

        private class DelegateInterceptor : IInterceptor
        {
            public event Action<IInvocation> Intercepting = _ => { };

            public event Action<IInvocation> Intercepted = _ => { };

            public void Intercept(IInvocation invocation)
            {
                this.Intercepting(invocation);
                invocation.Proceed();
                this.Intercepted(invocation);
            }
        }

        private class BuiltInfoInterceptor : IInterceptor
        {
            public BuiltInfoInterceptor(ExpressionBuiltEventArgs buildInfo)
            {
                this.BuildInfo = buildInfo;
            }

            public ExpressionBuiltEventArgs BuildInfo { get; set; }

            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
            }
        }

        private class InterceptorWithDependencyOnLogger : IInterceptor
        {
            public InterceptorWithDependencyOnLogger(ILogger logger)
            {
            }

            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
            }
        }

        private class InterceptorWithInternalConstructor : IInterceptor
        {
            internal InterceptorWithInternalConstructor()
            {
            }

            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
            }
        }

        private sealed class FakeLogger : ILogger
        {
            public string Message { get; private set; }

            public void Log(string message)
            {
                this.Message += message;
            }
        }

        private sealed class FakeCommand : ICommand
        {
            public void Execute()
            {
            }
        }

        private sealed class CommandThatLogsOnExecute : ICommand
        {
            private readonly ILogger logger;

            public CommandThatLogsOnExecute(ILogger logger)
            {
                this.logger = logger;
            }

            public string ExecuteLogMessage { get; set; }

            public void Execute()
            {
                if (!string.IsNullOrEmpty(this.ExecuteLogMessage))
                {
                    this.logger.Log(this.ExecuteLogMessage);
                }
            }
        }

        private class WithOutAndRef : IWithOutAndRef
        {
            public string SuppliedRefValue { get; private set; }

            public string OutputRefValue { get; set; }

            public string OutValue { get; set; }

            public int ReturnValue { get; set; }

            public int Operate(ref string refValue, out string outValue)
            {
                this.SuppliedRefValue = refValue;
                refValue = this.OutputRefValue;
                outValue = this.OutValue;
                return this.ReturnValue;
            }
        }
    }
}