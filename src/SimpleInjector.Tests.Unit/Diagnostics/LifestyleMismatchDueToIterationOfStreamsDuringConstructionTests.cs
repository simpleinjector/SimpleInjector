﻿namespace SimpleInjector.Tests.Unit.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Lifestyles;
    using SimpleInjector.Tests.Unit;

    // Test cases for #553
    [TestClass]
    public class LifestyleMismatchDueToIterationOfStreamsDuringConstructionTests
    {
        private const string ResolvingServicesFromAnInjectedCollectionMessage =
            "Resolving services from an injected collection during object construction";

        [TestMethod]
        public void GetInstance_SingletonThatNotIteratesStreamInCtorInjectedWithStreamWithTransient_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Transient);

            container.RegisterSingleton<ILogger, NoncaptivatingCompositeLogger<IEnumerable<ILogger>>>();

            // Act
            container.GetInstance<ILogger>();
        }

        [TestMethod]
        public void GetInstance_SingletonThatIteratesStreamInCtorInjectedWithStreamWithSingleton_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Singleton);

            container.RegisterSingleton<ILogger, CaptivatingCompositeLogger<IEnumerable<ILogger>>>();

            // Act
            container.GetInstance<ILogger>();
        }

        [TestMethod]
        public void GetInstance_ScopedThatIteratesStreamInCtorInjectedWithStreamWithTransient_Succeeds()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Transient);

            container.Register<ILogger, CaptivatingCompositeLogger<IEnumerable<ILogger>>>(Lifestyle.Scoped);

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                // Because of performance considerations, the detection is only done inside
                // singleton components. That's why this call should succeed.
                container.GetInstance<ILogger>();
            }
        }

        [TestMethod]
        public void GetInstance_SingletonThatIteratesIEnumerableInCtorInjectedWithStreamWithTransient_Throws() =>
            GetInstance_SingletonThatIteratesStreamInCtorInjectedWithStreamWithTransient_Throws(typeof(IEnumerable<ILogger>));

        [TestMethod]
        public void GetInstance_SingletonThatIteratesIListInCtorInjectedWithStreamWithTransient_Throws() =>
            GetInstance_SingletonThatIteratesStreamInCtorInjectedWithStreamWithTransient_Throws(typeof(IList<ILogger>));

        [TestMethod]
        public void GetInstance_SingletonThatIteratesICollectionInCtorInjectedWithStreamWithTransient_Throws() =>
            GetInstance_SingletonThatIteratesStreamInCtorInjectedWithStreamWithTransient_Throws(typeof(ICollection<ILogger>));

        [TestMethod]
        public void GetInstance_SingletonThatIteratesIReadOnlyCollectionInCtorInjectedWithStreamWithTransient_Throws() =>
            GetInstance_SingletonThatIteratesStreamInCtorInjectedWithStreamWithTransient_Throws(typeof(IReadOnlyCollection<ILogger>));

        [TestMethod]
        public void GetInstance_SingletonThatIteratesIReadOnlyListInCtorInjectedWithStreamWithTransient_Throws() =>
            GetInstance_SingletonThatIteratesStreamInCtorInjectedWithStreamWithTransient_Throws(typeof(IReadOnlyList<ILogger>));

        [TestMethod]
        public void GetInstance_SingletonThatIteratesCollectionInCtorInjectedWithStreamWithTransient_Throws() =>
            GetInstance_SingletonThatIteratesStreamInCtorInjectedWithStreamWithTransient_Throws(typeof(Collection<ILogger>));

        [TestMethod]
        public void GetInstance_SingletonThatGetsInjectedWithArrayWithTransients_ThrowsMismatchDetected()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append<ILogger, ConsoleLogger>(Lifestyle.Transient);
            container.RegisterSingleton<ILogger, NoncaptivatingCompositeLogger<ILogger[]>>();

            // Act
            Action action = () => container.GetInstance<ILogger>();

            // Assert
            // No special communication about iterating during the collection: this can't be
            // detected as array is not a stream and can't be intercepted.
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                ResolvingServicesFromAnInjectedCollectionMessage,
                action);

            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "lifestyle mismatch has been detected",
                action);
        }

        // #755
        [TestMethod]
        public void Verify_SingletonThatGetsInjectedWithArrayWithTransients_ThrowsMismatchDetected()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append<ILogger, ConsoleLogger>(Lifestyle.Transient);
            container.RegisterSingleton<ILogger, NoncaptivatingCompositeLogger<ILogger[]>>();

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(
                "it depends on the mutable collection type ILogger[]. " +
                "This causes ConsoleLogger to be resolved during object construction",
                action);
        }

        [TestMethod]
        public void GetInstance_SingletonThatGetsInjectedWithListWithTransients_ThrowsMismatchDetected()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append<ILogger, ConsoleLogger>(Lifestyle.Transient);
            container.RegisterSingleton<ILogger, NoncaptivatingCompositeLogger<List<ILogger>>>();

            // Act
            Action action = () => container.GetInstance<ILogger>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageDoesNotContain<ActivationException>(
                ResolvingServicesFromAnInjectedCollectionMessage,
                action);

            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "lifestyle mismatch has been detected",
                action);
        }

        // #755
        [TestMethod]
        public void Verify_SingletonThatGetsInjectedInjectedWithListWithTransients_ThrowsMismatchDetected()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append<ILogger, ConsoleLogger>(Lifestyle.Transient);
            container.RegisterSingleton<ILogger, NoncaptivatingCompositeLogger<List<ILogger>>>();

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(
                "it depends on the mutable collection type List<ILogger>. " +
                "This causes ConsoleLogger to be resolved during object construction",
                action);
        }

        // #1003
        [TestMethod]
        public void Verify_SingletonThatDependsOnMutableCollectionType_ThrowsMismatchWithDescriptiveMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append<ILogger, ConsoleLogger>(Lifestyle.Singleton);
            container.RegisterSingleton<ServiceDependingOn<ILogger[]>>();

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(
                "ILogger[] is a mutable collection type. Simple Injector always creates the mutable " +
                "collection types array and List<T> as transient, because a consumer can change the " +
                "contents of such collection, which could break seemingly unrelated parts parts of your " +
                "application if the collection was shared between consumers. Instead, either consider " +
                "lowering the lifestyle of ServiceDependingOn<ILogger[]> or change " +
                "ServiceDependingOn<ILogger[]>'s dependency from ILogger[] to one of the collection types " +
                "that stream services (e.g. IEnumerable<ILogger>, ICollection<ILogger>, etc).",
                action);
        }

        // #755
        [TestMethod]
        public void Verify_SingletonThatIteratesCollectionInCtorWithCollectionWithTransients_ThrowsMismatchDetected()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Append<ILogger, ConsoleLogger>(Lifestyle.Transient);
            container.RegisterSingleton<ILogger, CaptivatingCompositeLogger<Collection<ILogger>>>();

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(
                "instead of storing the injected Collection<ILogger> in a private field",
                action);
        }

        [TestMethod]
        public void MethodUnGetInstance_SingletonThatIteratesEnumerableInCtorInjectedWithStreamWithMixedLifestyles_Throws()
        {
            // Arrange
            var container = new Container();
            container.Options.EnableAutoVerification = false;

            container.Collection.Append<ILogger, ConsoleLogger>(Lifestyle.Singleton);
            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Transient);

            container.RegisterSingleton<ILogger, CaptivatingCompositeLogger<IEnumerable<ILogger>>>();

            // Act
            Action action = () => container.GetInstance<ILogger>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "A lifestyle mismatch has been detected.",
                action);
        }

        [TestMethod]
        public void MethodUnGetInstance_SingletonThatIteratesEnumerableInCtorInjectedWithStreamWithScopedInstance_Throws()
        {
            // Arrange
            var container = new Container();
            container.Options.EnableAutoVerification = false;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            container.RegisterSingleton<ILogger, CaptivatingCompositeLogger<IEnumerable<ILogger>>>();

            // Act
            Action action = () =>
            {
                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    // Even though the composite is requested inside an active scope, enumerating the
                    // dependencies inside the constructor should still fail.
                    container.GetInstance<ILogger>();
                }
            };

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "A lifestyle mismatch has been detected.",
                action);
        }

        [TestMethod]
        public void MethodUnGetInstance_SingletonThatIteratesIListInCtorInjectedWithStreamWithMixedLifestyles_Throws()
        {
            // Arrange
            var container = new Container();

            // Collection with more components, where the captive component is not the first.
            container.Collection.Append<ILogger, ConsoleLogger>(Lifestyle.Singleton);
            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Transient);

            container.RegisterSingleton<ILogger, CaptivatingCompositeLogger<IList<ILogger>>>();

            // Act
            Action action = () => container.GetInstance<ILogger>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "(Singleton) depends on ILogger implemented by NullLogger (Transient).",
                action);
        }

        [TestMethod]
        public void Analyze_SingletonDependingOnSingletonThatIteratesStreamInCtor_GivesExpectedResults()
        {
            // Arrange
            Type expectedType = typeof(CaptivatingCompositeLogger<IList<ILogger>>);

            var container = new Container();

            // Prevents Verify from throwing an exception; we want to check the analyzer's results
            container.Options.SuppressLifestyleMismatchVerification = true;

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Transient);

            container.RegisterSingleton(typeof(ILogger), expectedType);

            // Another singleton. This singleton should not cause interference with the results.
            container.RegisterSingleton<ServiceDependingOn<ILogger>>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<LifestyleMismatchDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(1, results.Length, message: Actual(results));

            var result = results[0];

            AssertThat.StringContains(
                expectedMessage: $"{expectedType.ToFriendlyName()} (Singleton) depends on ILogger",
                actualMessage: result.Description);
        }

        // Tests #554
        [TestMethod]
        public void VerifyOnly_SingletonIteratingStreamInCtorWithCaptiveStream_ThrowsMismatchException()
        {
            // Arrange
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            container.RegisterSingleton<ILogger, CaptivatingCompositeLogger<IEnumerable<ILogger>>>();

            // Act
            Action action = () => container.Verify(VerificationOption.VerifyOnly);

            // Assert
            // This call should fail as lifestyle mismatches are even reported during VerifyOnly
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "(Singleton) depends on ILogger implemented by NullLogger (Async Scoped).",
                action);
        }

        [TestMethod]
        public void VerifyOnly_SingletonIteratingStreamInCtorWithMismatchWarningSuppressed_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.AppendCollection<ILogger, NullLogger>(Lifestyle.Scoped, DiagnosticType.LifestyleMismatch);

            container.RegisterSingleton<ILogger, CaptivatingCompositeLogger<IEnumerable<ILogger>>>();

            // Act
            container.Verify(VerificationOption.VerifyOnly);
        }

        [TestMethod]
        public void VisualizeObjectGraph_ForSingletonThatCaptivatesInjectedStream_VisualizesTheCaptivatedElements()
        {
            // Arrange
            string expectedGraph =
$@"{typeof(CaptivatingCompositeLogger<IEnumerable<ILogger>>).ToFriendlyName()}(
    IEnumerable<ILogger>(
        NullLogger(),
        ConsoleLogger()),
    NullLogger(),
    ConsoleLogger())";

            var container = new Container();

            container.AppendCollection<ILogger, NullLogger>(Lifestyle.Transient, DiagnosticType.LifestyleMismatch);
            container.AppendCollection<ILogger, ConsoleLogger>(Lifestyle.Transient, DiagnosticType.LifestyleMismatch);

            container.RegisterSingleton<ILogger, CaptivatingCompositeLogger<IEnumerable<ILogger>>>();

            container.Verify();

            // Act
            var actualGraph = container.GetRegistration(typeof(ILogger))
                .VisualizeObjectGraph(new VisualizationOptions { IncludeLifestyleInformation = false });

            // Assert
            Assert.AreEqual(expectedGraph, actualGraph);
        }

        [TestMethod]
        public void Analyzer_SingletonThatCaptivatesInjectedStream_DoesNotCauseSRPViolations()
        {
            // Arrange
            var container = new Container();
            var ignoreMismatches = DiagnosticType.LifestyleMismatch;

            container.AppendCollection<ILogger, NullLogger>(Lifestyle.Transient, ignoreMismatches);
            container.AppendCollection<ILogger, ConsoleLogger>(Lifestyle.Transient, ignoreMismatches);
            container.AppendCollection<ILogger, Logger<int>>(Lifestyle.Transient, ignoreMismatches);
            container.AppendCollection<ILogger, Logger<bool>>(Lifestyle.Transient, ignoreMismatches);
            container.AppendCollection<ILogger, Logger<float>>(Lifestyle.Transient, ignoreMismatches);
            container.AppendCollection<ILogger, Logger<byte>>(Lifestyle.Transient, ignoreMismatches);
            container.AppendCollection<ILogger, Logger<object>>(Lifestyle.Transient, ignoreMismatches);
            container.AppendCollection<ILogger, Logger<double>>(Lifestyle.Transient, ignoreMismatches);
            container.AppendCollection<ILogger, Logger<short>>(Lifestyle.Transient, ignoreMismatches);

            container.RegisterSingleton<ILogger, CaptivatingCompositeLogger<IEnumerable<ILogger>>>();

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<SingleResponsibilityViolationDiagnosticResult>().ToArray();

            // Assert
            Assert.IsFalse(
                results.Any(),
                Actual(results) + " actual graph: " +
                container.GetRegistration(typeof(ILogger)).VisualizeObjectGraph());
        }

        // #678
        [TestMethod]
        public void Verify_SingletonCompositeCheckingCollectionForNullValues_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            container.RegisterSingleton<ILogger>(() =>
            {
                // Check for null should succeed
                container.GetAllInstances<ILogger>().Contains(null);

                return new NullLogger();
            });

            // Act
            container.Verify();
        }

        // #678
        [TestMethod]
        public void Verify_SingletonCompositeCheckingCollectionForAValue_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            container.RegisterSingleton<ILogger>(() =>
            {
                // Check for a not-null value should succeed, as the Contains method does not return
                // an instance and can, therefore, never cause a lifestyle mismatch (the user can't
                // store such instance).
                container.GetAllInstances<ILogger>().Contains(new NullLogger());

                return new NullLogger();
            });

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_SingletonThatIteratesStreamInCtorInjectedWithStreamWithAppendedSingleton_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Collection.Append<ILogger, ConsoleLogger>(Lifestyle.Singleton);

            Type captivatingComposite = typeof(CaptivatingCompositeLogger<IEnumerable<ILogger>>);

            container.RegisterSingleton(typeof(ILogger), captivatingComposite);

            // Act
            container.Verify();
        }

        // #769
        [TestMethod]
        public void Verify_SingletonThatIteratesStreamInCtorInjectedWithStreamWithForwaredSingleton_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Here the collection's ConsoleLogger registration is forwarded to the single registration.
            // This should have the same effect as calling Append<S, I>(Lifestyle.Singleton)
            container.Collection.Register<ILogger>(typeof(ConsoleLogger));
            container.RegisterSingleton<ConsoleLogger>();

            Type captivatingComposite = typeof(CaptivatingCompositeLogger<IEnumerable<ILogger>>);

            container.RegisterSingleton(typeof(ILogger), captivatingComposite);

            // Act
            container.Verify();
        }

        private static void GetInstance_SingletonThatIteratesStreamInCtorInjectedWithStreamWithTransient_Throws(
            Type dependencyType)
        {
            // Arrange
            var container = new Container();

            container.Collection.Append<ILogger, ConsoleLogger>(Lifestyle.Transient);

            Type captivatingComposite =
                typeof(CaptivatingCompositeLogger<>).MakeGenericType(dependencyType);

            container.RegisterSingleton(typeof(ILogger), captivatingComposite);

            // Act
            Action action = () => container.GetInstance<ILogger>();

            // Assert
            string name = captivatingComposite.ToFriendlyName();
            string collectionName = dependencyType.ToFriendlyName();

            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                $@"ConsoleLogger is part of the {collectionName} that is injected into {name}.
                The problem in {name} is that instead of storing the injected {collectionName}
                in a private field and iterating over it at the point its instances are
                required, ConsoleLogger is being resolved (from the collection) during object
                construction. {ResolvingServicesFromAnInjectedCollectionMessage}
                (e.g. by calling loggers.ToList() in the constructor) is not advised."
                .TrimInside(),
                action);
        }

        private static string Actual(IEnumerable<DiagnosticResult> results) =>
            "actual: " + string.Join(" - ", results.Select(r => r.Description));

        public class NoncaptivatingCompositeLogger<TCollection> : ILogger
            where TCollection : IEnumerable<ILogger>
        {
            public NoncaptivatingCompositeLogger(TCollection loggers) { }
        }

        public class CaptivatingCompositeLogger<TCollection> : ILogger
            where TCollection : IEnumerable<ILogger>
        {
            private readonly List<ILogger> captiveLoggers;

            public CaptivatingCompositeLogger(TCollection loggers) =>
                this.captiveLoggers = loggers.ToList();
        }
    }
}