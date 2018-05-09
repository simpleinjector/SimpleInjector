namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ContainerCollectionsCreateTests
    {
        public interface ILogStuf
        {
        }

        private static readonly Assembly CurrentAssembly = typeof(RegisterCollectionTests).GetTypeInfo().Assembly;

        [TestMethod]
        public void Create_WhenReturnedCollectionIterated_ProducesTheExpectedInstances()
        {
            // Arrange
            var expectedTypes = new[] { typeof(NullLogger), typeof(ConsoleLogger) };

            var container = new Container();

            // Act
            var stream = container.Collections.Create<ILogger>(expectedTypes);

            // Assert
            AssertThat.SequenceEquals(expectedTypes, actualTypes: stream.Select(GetType));
        }

        [TestMethod]
        public void Create_CreatingMultipleCollectionsOfTheSameServiceType_ProducesTheExpectedInstancesForBothCollections()
        {
            // Arrange
            var expectedTypes1 = new[] { typeof(NullLogger), typeof(ConsoleLogger) };
            var expectedTypes2 = new[] { typeof(Logger<int>), typeof(Logger<bool>) };

            var container = new Container();

            // Act
            var stream1 = container.Collections.Create<ILogger>(expectedTypes1);
            var stream2 = container.Collections.Create<ILogger>(expectedTypes2);

            // Assert
            AssertThat.SequenceEquals(expectedTypes1, actualTypes: stream1.Select(GetType));
            AssertThat.SequenceEquals(expectedTypes2, actualTypes: stream2.Select(GetType));
        }

        [TestMethod]
        public void Create_CollectionWithAbstraction_CallsBackIntoContainerUponIterationToGetTheDefaultRegistration()
        {
            // Arrange
            var container = new Container();

            container.Register<ILogger, ConsoleLogger>();

            // Act
            var stream = container.Collections.Create<ILogger>(typeof(ILogger));

            // Assert
            AssertThat.SequenceEquals(
                expectedTypes: new[] { typeof(ConsoleLogger) },
                actualTypes: stream.Select(GetType));
        }

        [TestMethod]
        public void Create_IteratingACreatedCollection_LocksTheContainer()
        {
            // Arrange
            var container = new Container();

            var stream = container.Collections.Create<ILogger>(typeof(ConsoleLogger));

            stream.First();

            // Act
            Action action = () => container.Register<ILogger, NullLogger>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "The container can't be changed",
                action);
        }

        [TestMethod]
        public void Create_WhenIterated_PreservesLifestyles()
        {
            // Arrange
            var container = new Container();

            container.Register<ConsoleLogger>(Lifestyle.Transient);
            container.Register<NullLogger>(Lifestyle.Singleton);

            // Act
            var stream = container.Collections.Create<ILogger>(typeof(ConsoleLogger), typeof(NullLogger));

            // Assert
            Assert.AreNotSame(stream.First(), stream.First(), "ConsoleLogger was expected to be transient.");
            Assert.AreSame(stream.Second(), stream.Second(), "NulLLogger was expected to be singleton.");
        }

        [TestMethod]
        public void Create_WithSetOfRegistrations_ProducesTheExpectedInstancesUponIteration()
        {
            // Arrange
            var container = new Container();

            // Act
            var stream = container.Collections.Create<ILogger>(
                Lifestyle.Singleton.CreateRegistration<ConsoleLogger>(container),
                Lifestyle.Transient.CreateRegistration<NullLogger>(container));

            // Assert
            AssertThat.IsInstanceOfType(typeof(ConsoleLogger), stream.First());
            AssertThat.IsInstanceOfType(typeof(NullLogger), stream.Second());
            Assert.AreSame(stream.First(), stream.First(), "ConsoleLogger was expected to be singleton.");
            Assert.AreNotSame(stream.Second(), stream.Second(), "NullLogger was expected to be transient.");
        }
                
        [TestMethod]
        public void Create_MultipleStreamsUsingTheSameSingletonComponent_PreservesLifestyleAcrossStreams()
        {
            // Arrange
            var expectedTypes1 = new[] { typeof(NullLogger), typeof(ConsoleLogger) };
            var expectedTypes2 = new[] { typeof(Logger<int>), typeof(NullLogger) };

            var container = new Container();

            container.Register<NullLogger>(Lifestyle.Singleton);

            // Act
            var stream1 = container.Collections.Create<ILogger>(expectedTypes1);
            var stream2 = container.Collections.Create<ILogger>(expectedTypes2);

            var nullLoggerFromStream1 = stream1.OfType<NullLogger>().Single();
            var nullLoggerFromStream2 = stream2.OfType<NullLogger>().Single();

            // Assert
            Assert.AreSame(nullLoggerFromStream1, nullLoggerFromStream2, "NullLogger's lifestyle is torn.");
        }

        [TestMethod]
        public void Verify_WhenCollectionIsCreatedForTypeThatFailsDuringCreation_VerifyTestsTheCollection()
        {
            // Arrange
            var container = new Container();

            var stream = container.Collections.Create<ILogger>(typeof(FailingConstructorLogger));

            // Notice the explicit call to GC.Collect(). Simple Injector holds on to 'stuff' using WeakReferences
            // to ensure that to memory is leaked, but as long as stream is referenced, should it as well be
            // verified
            GC.Collect();

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                expectedMessage: nameof(FailingConstructorLogger),
                action: action);

            GC.KeepAlive(stream);
        }
        
        [TestMethod]
        public void Verify_WhenCollectionIsCreatedForRegistrationThatFailsDuringCreation_VerifyTestsTheCollection()
        {
            // Arrange
            var container = new Container();

            var stream = container.Collections.Create<ILogger>(
                Lifestyle.Transient.CreateRegistration<FailingConstructorLogger>(container));
            
            // Notice the explicit call to GC.Collect(). Simple Injector holds on to 'stuff' using WeakReferences
            // to ensure that to memory is leaked, but as long as stream is referenced, should it as well be
            // verified
            GC.Collect();

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                expectedMessage: nameof(FailingConstructorLogger),
                action: action);

            GC.KeepAlive(stream);
        }
        
        [TestMethod]
        public void Verify_WhenCollectionIsCreatedForTypeThatFailsDuringCreation_VerifySucceedsWhenCollectionWasAlreadyCollected()
        {
            // Arrange
            var container = new Container();

            var stream = container.Collections.Create<ILogger>(typeof(FailingConstructorLogger));

            // Explicitly clear the reference to stream, so ensure GC.Collect cleans it up (only needed when
            // running in debug).
            stream = null;

            // Notice the explicit call to GC.Collect(). Simple Injector holds on to 'stuff' using WeakReferences
            // to ensure that to memory is leaked, but as long as stream is referenced, should it as well be
            // verified
            GC.Collect();

            // Act
            // If Verify fails, it means we are still holding on to the collection somewhere. This can cause
            // a memory leak.
            container.Verify();
        }
        
        [TestMethod]
        public void Create_SuppliedWithNullTypeArray_ThrowsArgumentNullException()
        {
            // Arrange
            var container = new Container();

            // Act
            Action action = () => container.Collections.Create<ILogger>((Type[])null);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("serviceTypes", action);
        }

        [TestMethod]
        public void Create_SuppliedWithNullEnumerable_ThrowsArgumentNullException()
        {
            // Arrange
            var container = new Container();

            // Act
            Action action = () => container.Collections.Create<ILogger>((IEnumerable<Type>)null);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("serviceTypes", action);
        }

        [TestMethod]
        public void Create_SuppliedWithNullElement_ThrowsException()
        {
            // Arrange
            var expectedTypes = new[] { typeof(NullLogger), null };

            var container = new Container();

            // Act
            Action action = () => container.Collections.Create<ILogger>(expectedTypes);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The collection contains null elements.",
                action);
        }

        [TestMethod]
        public void Create_SuppliedWithIncompatibleType_ThrowsException()
        {
            // Arrange
            var listWithIncompatibleType = new[] { typeof(NullLogger), typeof(ConcreteCommand) };

            var container = new Container();

            // Act
            Action action = () => container.Collections.Create<ILogger>(listWithIncompatibleType);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                $"{nameof(ConcreteCommand)} does not implement {nameof(ILogger)}",
                action);
        }

        [TestMethod]
        public void Create_SuppliedWithOpenGenericType_ThrowsException()
        {
            // Arrange
            Type invalidType = typeof(Logger<>);

            var container = new Container();

            // Act
            Action action = () => container.Collections.Create<ILogger>(new[] { invalidType });

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The supplied type Logger<T> is an open generic type.",
                action);
        }
        
        [TestMethod]
        public void RegisterCollectionTServiceAssemblyArray_RegisteringNonGenericServiceAndAssemblyWithMultipleImplementations_RegistersThoseImplementations()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            var loggers = container.Collections.Create<ILogStuf>(new[] { CurrentAssembly });

            // Assert
            Assert_ContainsAllLoggers(loggers);
        }

        [TestMethod]
        public void RegisterCollectionTServiceAssemblyEnumerable_AccidentallyUsingTheSameAssemblyTwice_RegistersThoseImplementationsOnce()
        {
            // Arrange
            var container = ContainerFactory.New();

            var assemblies = Enumerable.Repeat(CurrentAssembly, 2);

            // Act
            var loggers = container.Collections.Create<ILogStuf>(assemblies);

            // Assert
            Assert_ContainsAllLoggers(loggers);
        }

        private static Type GetType<T>(T instance) => instance.GetType();
        
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

        public class LogStuff3 : ILogStuf
        {
        }
    }
}