namespace SimpleInjector.Tests.Unit
{
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

            container.RegisterAll<ILogStuf>(new[] { this.GetType().Assembly });

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllLoggers(loggers);
        }

        [TestMethod]
        public void RegisterCollectionTServiceAssemblyEnumerable_RegisteringNonGenericServiceAndAssemblyWithMultipleImplementations_RegistersThoseImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<ILogStuf>(Enumerable.Repeat(this.GetType().Assembly, 1));

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

            container.RegisterAll<ILogStuf>(assemblies);

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllLoggers(loggers);
        }
        
        [TestMethod]
        public void RegisterCollectionTServiceAccessibilityOptionAssemblyArray_RegisteringNonGenericServiceAndAssemblyWithMultiplePublicImplementations_RegistersThosePublicImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<ILogStuf>(AccessibilityOption.PublicTypesOnly, 
                new[] { this.GetType().Assembly });

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllPublicLoggers(loggers);
        }

        [TestMethod]
        public void RegisterCollectionTServiceAssemblyEnumerable_RegisteringNonGenericServiceAndAssemblyWithMultiplePublicImplementations_RegistersThosePublicImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll<ILogStuf>(AccessibilityOption.PublicTypesOnly, 
                Enumerable.Repeat(this.GetType().Assembly, 1));

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllPublicLoggers(loggers);
        }

        [TestMethod]
        public void RegisterCollectionServiceAssemblyArray_RegisteringNonGenericServiceAndAssemblyWithMultipleImplementations_RegistersThoseImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll(typeof(ILogStuf), new[] { this.GetType().Assembly });

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

            container.RegisterAll(typeof(ILogStuf), Enumerable.Repeat(this.GetType().Assembly, 1));

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllLoggers(loggers);
        }

        [TestMethod]
        public void RegisterCollectionServiceAccessibilityOptionAssemblyArray_RegisteringNonGenericServiceAndAssemblyWithMultiplePublicImplementations_RegistersThosePublicImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll(typeof(ILogStuf), AccessibilityOption.PublicTypesOnly,
                new[] { this.GetType().Assembly });

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllPublicLoggers(loggers);
        }

        [TestMethod]
        public void RegisterCollectionServiceAssemblyEnumerable_RegisteringNonGenericServiceAndAssemblyWithMultiplePublicImplementations_RegistersThosePublicImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterAll(typeof(ILogStuf), AccessibilityOption.PublicTypesOnly,
                Enumerable.Repeat(this.GetType().Assembly, 1));

            // Act
            var loggers = container.GetAllInstances<ILogStuf>();

            // Assert
            Assert_ContainsAllPublicLoggers(loggers);
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

        private static void Assert_ContainsAllPublicLoggers(IEnumerable loggers)
        {
            var instances = loggers.Cast<ILogStuf>().ToArray();

            string types = string.Join(", ", instances.Select(instance => instance.GetType().Name));

            Assert.AreEqual(2, instances.Length, "Actual: " + types);
            Assert.IsTrue(instances.OfType<LogStuff1>().Any(), "Actual: " + types);
            Assert.IsTrue(instances.OfType<LogStuff2>().Any(), "Actual: " + types);
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