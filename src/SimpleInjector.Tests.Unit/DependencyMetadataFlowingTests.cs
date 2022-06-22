namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector;

    // Tests for #861 Tests DependencyMetadata in ScopedLifestyle.Flowing mode.
    [TestClass]
    public class DependencyMetadataFlowingTests
    {
        [TestMethod]
        public void Resolving_ScopedInstanceFromSameMetadata_ShouldYieldSameScopedInstance()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Register<ILogger, NullLogger>(Lifestyle.Scoped);

            var scope = new Scope(container);

            // Act
            var metadata1 = scope.GetInstance<DependencyMetadata<ILogger>>();
            var logger1 = metadata1.GetInstance();
            var logger2 = metadata1.GetInstance();

            // Assert
            Assert.AreSame(logger1, logger2);
        }

        [TestMethod]
        public void Resolving_ScopedInstanceFromBothScopeAndMetadata_ShouldYieldTheSameInstance()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Register<ILogger, NullLogger>(Lifestyle.Scoped);

            var scope = new Scope(container);

            // Act
            var metadata = scope.GetInstance<DependencyMetadata<ILogger>>();
            var logger1 = metadata.GetInstance();
            var logger2 = scope.GetInstance<ILogger>();

            // Assert
            Assert.AreSame(logger1, logger2);
        }

        [TestMethod]
        public void Resolving_ScopedInstanceFromMetadatasFromDifferentScopes_ShouldYieldDifferentInstances()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Register<ILogger, NullLogger>(Lifestyle.Scoped);

            var scope1 = new Scope(container);
            var scope2 = new Scope(container);

            // Act
            var metadata1 = scope1.GetInstance<DependencyMetadata<ILogger>>();
            var metadata2 = scope2.GetInstance<DependencyMetadata<ILogger>>();
            var logger1 = metadata1.GetInstance();
            var logger2 = metadata2.GetInstance();

            // Assert
            Assert.AreNotSame(logger1, logger2);
        }

        [TestMethod]
        public void Resolving_ScopedCollectionFromSameMetadata_ShouldYieldSameScopedInstances()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            var scope = new Scope(container);

            // Act
            var metadatas = scope.GetInstance<IEnumerable<DependencyMetadata<ILogger>>>();
            var logger1 = metadatas.Single().GetInstance();
            var logger2 = metadatas.Single().GetInstance();

            // Assert
            Assert.AreSame(logger1, logger2);
        }

        [TestMethod]
        public void Resolving_ScopedCollectionFromBothScopeAndMetadata_ShouldYieldTheSameScopedInstances()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            var scope = new Scope(container);

            // Act
            var metadatas = scope.GetInstance<IEnumerable<DependencyMetadata<ILogger>>>();
            var logger1 = metadatas.Single().GetInstance();
            var logger2 = scope.GetInstance<IEnumerable<ILogger>>().Single();

            // Assert
            Assert.AreSame(logger1, logger2);
        }

        [TestMethod]
        public void Resolving_ScopedCollectionFromMetadatasFromDifferentScopes_ShouldYieldDifferentInstances()
        {
            // Arrangew
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            var scope1 = new Scope(container);
            var scope2 = new Scope(container);

            // Act
            var metadatas1 = scope1.GetInstance<IEnumerable<DependencyMetadata<ILogger>>>();
            var metadatas2 = scope2.GetInstance<IEnumerable<DependencyMetadata<ILogger>>>();
            var logger1 = metadatas1.Single().GetInstance();
            var logger2 = metadatas2.Single().GetInstance();

            // Assert
            Assert.AreNotSame(logger1, logger2);
        }

        [TestMethod]
        public void Resolving_ConsumerWithMetadataCollectionDependency_Succeeds()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            container.Register<ServiceDependingOn<IEnumerable<DependencyMetadata<ILogger>>>>();

            var scope = new Scope(container);

            // Act
            scope.GetInstance<ServiceDependingOn<IEnumerable<DependencyMetadata<ILogger>>>>();
        }

        [TestMethod]
        public void Verify_SingletonConsumerDependendingOnSingletonDependencyMetadata_Succeeds()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Register<ILogger, NullLogger>(Lifestyle.Singleton);
            container.Register<ServiceDependingOn<DependencyMetadata<ILogger>>>(Lifestyle.Singleton);

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_SingletonConsumerDependendingOnTransientDependencyMetadata_Succeeds()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Register<ILogger, NullLogger>(Lifestyle.Transient);
            container.Register<ServiceDependingOn<DependencyMetadata<ILogger>>>(Lifestyle.Singleton);

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_SingletonConsumerDependendingOnSingletonDependencyMetadataCollection_Succeeds()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;
            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Singleton);

            container.Register<ServiceDependingOn<IEnumerable<DependencyMetadata<ILogger>>>>(Lifestyle.Singleton);

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_SingletonConsumerDependendingOnSingletonDependencyMetadataReadOnlyCollection_Succeeds()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;
            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Singleton);

            container.Register<ServiceDependingOn<ReadOnlyCollection<DependencyMetadata<ILogger>>>>(Lifestyle.Singleton);

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_SingletonConsumerDependendingOnScopedDependencyMetadata_ThrowsDiagnosticException()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Register<ILogger, NullLogger>(Lifestyle.Scoped);
            container.Register<ServiceDependingOn<DependencyMetadata<ILogger>>>(Lifestyle.Singleton);

            // Act
            Action action = () => container.Verify();

            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(@"
                [Lifestyle Mismatch] ServiceDependingOn<DependencyMetadata<ILogger>> (Singleton) depends
                on DependencyMetadata<ILogger> (Scoped). DependencyMetadata<ILogger> was registered as
                Scoped by Simple Injector, because you set 'Options.DefaultScopedLifestyle' to
                'ScopedLifestyle.Flowing' while its described dependency ILogger or one of its
                dependencies was registered as Scoped. This caused Simple Injector to capture the active
                Scope inside the DependencyMetadata<ILogger> and forced its lifestyle to be lowered to
                Scoped."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void Verify_SingletonConsumerDependendingOnScopedDependencyMetadataCollection_ThrowsDiagnosticException()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Scoped);
            container.Collection.Append<ILogger, LoggerWithDependency<ITimeProvider>>(Lifestyle.Transient);
            container.Register<ServiceDependingOn<IEnumerable<DependencyMetadata<ILogger>>>>(Lifestyle.Singleton);

            // Act
            Action action = () => container.Verify();

            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(
                @"IEnumerable<DependencyMetadata<ILogger>> was registered as Scoped by Simple Injector",
                action);
        }

        [TestMethod]
        public void Verify_SingletonConsumerDependendingOnScopedDependencyMetadataReadOnlyCollection_ThrowsDiagnosticException()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;
            container.Collection.Append<ILogger, NullLogger>(Lifestyle.Scoped);

            container.Register<ServiceDependingOn<ReadOnlyCollection<DependencyMetadata<ILogger>>>>(Lifestyle.Singleton);

            // Act
            Action action = () => container.Verify();

            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(
                "ReadOnlyCollection<DependencyMetadata<ILogger>> was registered as Scoped by Simple Injector",
                action);
        }

        public sealed class LoggerWithDependency<TDependency> : ILogger
        {
            public LoggerWithDependency(TDependency dependency) => this.Dependency = dependency;

            public TDependency Dependency { get; }
        }
    }
}