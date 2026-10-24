namespace SimpleInjector.Tests.Unit
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests that verify DI Container instance does not leak memory.</summary>
    [TestClass]
    [TestCategory("Memory")]
    public class MemoryTests
    {
        public MemoryTests()
        {
            // Pre-JIT methods.
            GetTotalMemory();
            Assert_NoMemoryLeak(new MemoryResults());
        }

        [TestMethod]
        public void CreatingManyContainers_WithNoRegistrations_DoesNotIncreaseMemoryFootprint()
        {
            var values = new MemoryResults
            {
                NumberOfIterations = 100_000
            };

            // Warmup
            BuildContainers(BuildEmptyContainer, count: values.NumberOfIterations);
            GetTotalMemory();
            BuildContainers(BuildEmptyContainer, count: values.NumberOfIterations);

            values.InitialMemoryFootprint = GetTotalMemory();

            // Act
            BuildContainers(BuildEmptyContainer, count: values.NumberOfIterations);

            values.CurrentMemoryFootprint = GetTotalMemory();

            // Assert
            Assert_NoMemoryLeak(values);
        }

        private static void BuildEmptyContainer()
        {
            var container = new Container();
        }

        [TestMethod]
        public void CreatingManyContainers_WithDecoratorRegistration_DoesNotIncreaseMemoryFootprint()
        {
            var values = new MemoryResults
            {
                NumberOfIterations = 1_000
            };

            // Warmup
            BuildContainers(BuildSimpleVerifiedContainerWithDecorator, count: values.NumberOfIterations);
            GetTotalMemory();
            BuildContainers(BuildSimpleVerifiedContainerWithDecorator, count: values.NumberOfIterations);

            values.InitialMemoryFootprint = GetTotalMemory();

            // Act
            BuildContainers(BuildSimpleVerifiedContainerWithDecorator, count: values.NumberOfIterations);

            values.CurrentMemoryFootprint = GetTotalMemory();

            // Assert
            Assert_NoMemoryLeak(values);
        }

        [TestMethod]
        public void Dispose_ContainerAfterSettingFlowingScopeAsCurrentScope_ContainerCanBeGarbageCollected()
        {
            var values = new MemoryResults
            {
                NumberOfIterations = 1_000
            };

            // Warmup
            BuildContainers(BuildContainerThatSetsFlowingScope, count: values.NumberOfIterations);
            GetTotalMemory();
            BuildContainers(BuildContainerThatSetsFlowingScope, count: values.NumberOfIterations);

            values.InitialMemoryFootprint = GetTotalMemory();

            // Act
            BuildContainers(BuildContainerThatSetsFlowingScope, count: values.NumberOfIterations);

            values.CurrentMemoryFootprint = GetTotalMemory();

            // Assert
            Assert_NoMemoryLeak(values);
        }

        private static void BuildContainerThatSetsFlowingScope()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;
            container.Register<IUserRepository, DisposableUserRepository>(Lifestyle.Singleton);
            container.GetInstance<IUserRepository>();

            var scope = new Scope(container);

            ScopedLifestyle.Flowing.SetCurrentScope(scope);

            scope.Dispose();
            container.Dispose();
        }

        private static void BuildSimpleVerifiedContainerWithDecorator()
        {
            var container = new Container();

            container.Register<ILogger, NullLogger>();
            container.RegisterDecorator<ILogger, LoggerDecorator>();

            container.Verify();

            container.Dispose();
        }

        private static void BuildContainers(Action containerBuilder, int count = 10)
        {
            for (int i = 0; i < count; i++)
            {
                containerBuilder();
            }
        }

        private static void Assert_NoMemoryLeak(MemoryResults results)
        {
            // If number of bytes of memory increases is less than one byte per iteration, we consider it
            // to not be a leak.
            long acceptedErrorMargin = results.NumberOfIterations - 1;

            long increase =
                results.CurrentMemoryFootprint - results.InitialMemoryFootprint;

            long increasePerIteration = increase / results.NumberOfIterations;

            Assert.IsTrue((increase - acceptedErrorMargin) <= 0,
                message: $"Memory footprint increased by {ToMemorySize(increase)}. This is a memory leak " +
                    $"of {ToMemorySize(increasePerIteration)} per iteration. " +
                    "Note that this test might fail when run in parallel with other tests. It's a bit unstable.");
        }

        private static long GetTotalMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return GC.GetTotalMemory(forceFullCollection: true);
        }

        private static string ToMemorySize(long bytes) =>
            bytes >= 1024 * 1024 * 10
            ? (bytes / (1024 * 1024)) + " MB"
            : bytes >= 1024 * 10
                ? (bytes / 1024) + " KB"
                : bytes + " bytes";

        private class MemoryResults
        {
            public long InitialMemoryFootprint { get; set; }
            public long CurrentMemoryFootprint { get; set; }
            public int NumberOfIterations { get; set; } = 1;
        }

        private class DisposableUserRepository : IUserRepository;
    }
}