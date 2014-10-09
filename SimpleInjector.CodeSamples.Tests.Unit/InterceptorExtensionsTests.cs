namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InterceptorExtensionsTests
    {
        private static readonly Func<Type, bool> IsACommandPredicate = type => type.Name.EndsWith("Command");

        [TestMethod]
        public void InterceptWithGenericArgAndPredicate_InterceptingInterfacesEndingWithCommand_InterceptsTheInstance()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingle<ILogger>(logger);
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
        public void InterceptWithGenericArgAndPredicate_TwoInterceptors_InterceptsTheInstanceWithBothInterceptors()
        {
            // Arrange
            var logger = new FakeLogger();

            var container = new Container();

            container.RegisterSingle<ILogger>(logger);
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

            container.RegisterSingle<FakeInterceptor>();

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

            container.RegisterSingle<ICommand, FakeCommand>();

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

            container.RegisterSingle<ICommand, FakeCommand>();

            container.RegisterSingle<FakeInterceptor>();

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

            container.RegisterSingle<ILogger>(logger);

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

            container.RegisterSingle<ILogger>(logger);
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

            container.RegisterSingle<ILogger>(logger);
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

            container.RegisterSingle<ICommand, FakeCommand>();

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

            container.RegisterSingle<ILogger>(logger);
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

            container.RegisterSingle<ILogger>(logger);
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

            container.RegisterSingle<ICommand, FakeCommand>();

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

            container.RegisterSingle<ICommand, FakeCommand>();
            container.RegisterSingle<ILogger, FakeLogger>();

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

            try
            {
                // Act
                container.InterceptWith<InterceptorWithInternalConstructor>(IsACommandPredicate);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("For the container to be able to create " + 
                    "InterceptorExtensionsTests+InterceptorWithInternalConstructor, it should contain " +
                    "exactly one public constructor"),
                    "Actual: " + ex.Message);
            }
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
                    "InterceptorExtensionsTests+InterceptorWithDependencyOnLogger contains the parameter " +
                    "of type ILogger with name 'logger' that is not registered."),
                    "Actual: " + ex.Message);
            }
        }

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
    }
}
