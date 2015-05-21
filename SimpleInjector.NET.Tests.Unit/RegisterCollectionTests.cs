namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterCollectionTests
    {
        public interface ILogStuf
        { 
        }

        [TestMethod]
        public void RegisterCollectionTServiceAssemblyArray_RegisteringNonGenericServiceAndAssemblyWithMultipleImplementations_RegistersThoseImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection<ILogStuf>(new[] { this.GetType().Assembly });

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllLoggers(loggers);
        }

        [TestMethod]
        public void RegisterCollectionTServiceAssemblyEnumerable_AccidentallyUsingTheSameAssemblyTwice_RegistersThoseImplementationsOnce()
        {
            // Arrange
            var container = ContainerFactory.New();

            var assemblies = Enumerable.Repeat(this.GetType().Assembly, 2);

            container.RegisterCollection<ILogStuf>(assemblies);

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllLoggers(loggers);
        }

        [TestMethod]
        public void RegisterCollectionServiceAssemblyArray_RegisteringNonGenericServiceAndAssemblyWithMultipleImplementations_RegistersThoseImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ILogStuf), new[] { this.GetType().Assembly });

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllLoggers(loggers);
        }

        [TestMethod]
        public void RegisterCollectionServiceAssemblyEnumerable_RegisteringNonGenericServiceAndAssemblyWithMultipleImplementations_RegistersThoseImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterCollection(typeof(ILogStuf), Enumerable.Repeat(this.GetType().Assembly, 1));

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllLoggers(loggers);
        }

        [TestMethod]
        public void RegisterCollection_UnexpectedCSharpOverloadResolution_ThrowsDescriptiveException()
        {
            // Arrange
            var container = new Container();

            // Act
            // Here the user might think he calls RegisterCollection(Type, params Type[]), but instead
            // RegisterCollection<Type>(new[] { typeof(ILogger), typeof(NullLogger) }) is called. 
            Action action = () => container.RegisterCollection(typeof(ILogger), typeof(NullLogger));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The most likely cause of this happening is because the C# overload resolution picked " +
                "a different method for you than you expected to call. The method C# selected for you is: " +
                "RegisterCollection<Type>",
                action);
        }

        [TestMethod]
        public void RegisterCollection_WithOpenGenericTypeThatIsRegisteredAsSingleton_RespectsTheRegisteredLifestyle()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(InternalEventHandler<>), typeof(InternalEventHandler<>), Lifestyle.Singleton);

            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(InternalEventHandler<>) });

            // Act
            var handler1 = container.GetAllInstances<IEventHandler<int>>().Single();
            var handler2 = container.GetAllInstances<IEventHandler<int>>().Single();

            // Assert
            Assert.AreSame(handler1, handler2, "Singleton was expected.");
        }

        private static void Assert_ContainsAllLoggers(IEnumerable loggers)
        {
            var instances = loggers.Cast<ILogStuf>().ToArray();

            string types = string.Join(", ", instances.Select(instance => instance.GetType().Name));

            Assert.AreEqual(3, instances.Length, "Actual: " + types);
            Assert.IsTrue(instances.OfType<LogStuff1>().Any(), "Actual: " + types);
            Assert.IsTrue(instances.OfType<LogStuff2>().Any(), "Actual: " + types);
            Assert.IsTrue(instances.OfType<LogStuff3>().Any(), "Actual: " + types);
        }

        public class LogStuff1 : ILogStuf
        {
        }

        public class LogStuff2 : ILogStuf
        {
        }

        internal class LogStuff3 : ILogStuf
        {
        }
    }
}