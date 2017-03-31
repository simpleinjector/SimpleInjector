namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class MethodInjectionExtensionsTests
    {
        [TestMethod]
        public void GetInstance_OnTypeThatContainsInjectionMethod_InjectsAllDependenciesAsExpected()
        {
            // Arrange
            var container = new Container();

            container.Options.EnableMethodInjectionWith<InjectAttribute>();

            container.Register<ILogger, NullLogger>(Lifestyle.Singleton);
            container.Register<ICommand, ConcreteCommand>();

            // Act
            var service = container.GetInstance<ClassWithInjectionMethod>();

            // Assert
            Assert.IsNotNull(service.Logger);
            Assert.IsNotNull(service.Command);
        }

        [TestMethod]
        public void GetInstance_OnTypeThatContainsInjectionMethodWithIncompleteConfiguration_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Options.EnableMethodInjectionWith<InjectAttribute>();

            container.Register<ILogger, NullLogger>(Lifestyle.Singleton);

            try
            {
                // Act
                var service = container.GetInstance<ClassWithInjectionMethod>();

                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains("No registration for type ICommand could be found.", ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_TypeWithInjectionMethodWithContextualDependency_InjectsTheContextualDependencyAsExpected()
        {
            // Arrange
            var container = new Container();

            container.Options.EnableMethodInjectionWith<InjectAttribute>();

            container.Register<ICommand, ConcreteCommand>();

            container.RegisterWithContext<ILogger>(context => new ContextualLogger(context));

            // Act
            var service = container.GetInstance<ClassWithInjectionMethod>();

            // Assert
            var contextualLogger = (ContextualLogger)service.Logger;

            var parentType = contextualLogger.Context.ImplementationType;

            Assert.AreEqual(typeof(ClassWithInjectionMethod), parentType,
                "These injected dependencies are expected to pass through the complete pipeline, which " +
                "means interception should take place.");
        }

        [TestMethod]
        public void GetInstance_InjectionAttributeOnStaticMethod_ThrowsExceptionWithExpectedMessage()
        {
            // Arrange
            var container = new Container();

            container.Options.EnableMethodInjectionWith<InjectAttribute>();

            container.Register<ILogger, NullLogger>(Lifestyle.Singleton);

            try
            {
                // Act
                var service = container.GetInstance<ClassWithStaticInjectionMethod>();

                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(
                    $"Method {typeof(ClassWithStaticInjectionMethod).ToFriendlyName()}.Initialize is static", 
                    ex.Message);
            }
        }

        public class ClassWithInjectionMethod
        {
            public ILogger Logger { get; private set; }

            public ICommand Command { get; private set; }

            [Inject]
            public void Initialize(ILogger logger, ICommand command)
            {
                this.Logger = logger;
                this.Command = command;
            }

            // No attribute: this method should not be called
            public void Initialize(ILogger logger)
            {
                Assert.Fail("This method should not be called.");
            }
        }

#pragma warning disable RCS1102 // Mark class as static.
        public class ClassWithStaticInjectionMethod
        {
            [Inject]
            public static void Initialize(ILogger logger)
            {
            }
        }
#pragma warning restore RCS1102 // Mark class as static.

        public sealed class ContextualLogger : ILogger
        {
            public ContextualLogger(DependencyContext context)
            {
                Assert.IsNotNull(context, "context should not be null.");

                this.Context = context;
            }

            public DependencyContext Context { get; }

            public void Log(string message)
            {
            }
        }
    }
}