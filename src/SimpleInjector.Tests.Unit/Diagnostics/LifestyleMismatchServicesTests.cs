namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class LifestyleMismatchServicesTests
    {
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_TransientToSingleton_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Transient, child: Lifestyle.Singleton);
            
            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Every service can safely depend on a singleton.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_TransientToTransient_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Transient, child: Lifestyle.Transient);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_SingletonToSingleton_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Singleton, child: Lifestyle.Singleton);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Every service can safely depend on a singleton.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_SingletonToTransient_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Singleton, child: Lifestyle.Transient);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_SingletonToLifetimeScope_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Singleton, child: Lifestyles.LifetimeScope);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_SingletonToLifetimeWcfOperation_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Singleton, child: Lifestyles.WcfOperation);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_SingletonToLifetimeWebRequest_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Singleton, child: Lifestyles.WebRequest);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result);
        }
        
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_LifetimeScopeToSingleton_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.LifetimeScope, child: Lifestyle.Singleton);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Every service can safely depend on a singleton.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_WcfOperationeToSingleton_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.WcfOperation, child: Lifestyle.Singleton);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Every service can safely depend on a singleton.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_WebRequestToSingleton_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.WebRequest, child: Lifestyle.Singleton);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Every service can safely depend on a singleton.");
        }
        
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_LifetimeScopeToTransient_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.LifetimeScope, child: Lifestyle.Transient);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "Services can not depend on a dependency with a shorter lifestyle.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_WcfOperationeToTransient_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.WcfOperation, child: Lifestyle.Transient);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "Services can not depend on a dependency with a shorter lifestyle.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_WebRequestToTransient_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.WebRequest, child: Lifestyle.Transient);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "Services can not depend on a dependency with a shorter lifestyle.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_TransientToUnknown_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Transient, child: Lifestyle.Unknown);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "A transient service can safely depend on any other dependency.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_UnknownToTransient_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Unknown, child: Lifestyle.Transient);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "An unknown lifestyle will always be bigger than transient.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_SingletonToUnknown_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Singleton, child: Lifestyle.Unknown);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "The unknown lifestyle will likely be shorter than singleton.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_UnknownToSingleton_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Unknown, child: Lifestyle.Singleton);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Every dependency can always safely depend on a singleton.");
        }
        
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_LifetimeScopeToUnknown_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.LifetimeScope, child: Lifestyle.Unknown);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "A lifestyle longer than transient can not safely depend on an unknown " +
                "lifestyle, since this unknown lifestyle could act like a transient.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_UnknownToLifetimeScope_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Unknown, child: Lifestyles.LifetimeScope);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "An unknown lifestyle can not safely depend on a lifestyle that is " +
                "shorter than singleton, since this unknown lifestyle could act like a singleton.");
        }
        
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_HybridToTheSameHybridInstance_DoesNotReportAMismatch()
        {
            // Arrange
            var hybrid = Lifestyle.CreateHybrid(() => true, Lifestyle.Transient, Lifestyle.Singleton);

            var dependency = CreateRelationship(parent: hybrid, child: hybrid);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Since the both the parent and child have exactly the same lifestyle, " +
                "there is expected to be no mismatch.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_ShortHybridToLongHybrid_DoesNotReportAMismatch()
        {
            // Arrange
            var parentHybrid = Lifestyle.CreateHybrid(() => true, Lifestyle.Transient, Lifestyle.Transient);
            var childHybrid = Lifestyle.CreateHybrid(() => true, Lifestyle.Singleton, Lifestyle.Singleton);

            var dependency = CreateRelationship(parent: parentHybrid, child: childHybrid);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Both lifestyles of the parent are shorter than those of the child.");
        }
        
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_ShortHybridToLongHybrid_ReportsMismatch()
        {
            // Arrange
            var parentHybrid = Lifestyle.CreateHybrid(() => true, Lifestyle.Singleton, Lifestyle.Singleton);
            var childHybrid = Lifestyle.CreateHybrid(() => true, Lifestyle.Singleton, Lifestyle.Transient);

            var dependency = CreateRelationship(parent: parentHybrid, child: childHybrid);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "Both lifestyles of the parent are longer than those of the child.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_ShortScopedHybridToLongScopedHybrid_ReportsMismatch()
        {
            // Arrange
            ScopedLifestyle singleton = new CustomScopedLifestyle(Lifestyle.Singleton);
            ScopedLifestyle transient = new CustomScopedLifestyle(Lifestyle.Transient);

            var parentHybrid = Lifestyle.CreateHybrid(() => true, singleton, singleton);
            var childHybrid = Lifestyle.CreateHybrid(() => true, singleton, transient);

            var dependency = CreateRelationship(parent: parentHybrid, child: childHybrid);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "Both lifestyles of the parent are longer than those of the child.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_NestedHybridSingletonToTransient1_ReportsMismatch()
        {
            // Arrange
            Func<bool> selector = () => true;

            var hybridWithDeeplyNestedSingleton = Lifestyle.CreateHybrid(selector,
                Lifestyle.Transient, 
                Lifestyle.CreateHybrid(selector,
                    Lifestyle.Transient, 
                    Lifestyle.CreateHybrid(selector,
                        Lifestyle.Transient, 
                        Lifestyle.CreateHybrid(selector,
                            Lifestyle.Transient, 
                            Lifestyle.CreateHybrid(selector,
                                Lifestyle.Transient, 
                                Lifestyle.Singleton)))));

            var dependency = 
                CreateRelationship(parent: hybridWithDeeplyNestedSingleton, child: Lifestyle.Transient);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "Since the hybrid lifestyle contains a singleton lifestyle, this " +
                "hybrid lifestyle should be considered to be a singleton when evaluated on the parent.");
        }
        
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_NestedHybridSingletonToTransient2_ReportsMismatch()
        {
            // Arrange
            Func<bool> selector = () => true; 
            
            var hybridWithDeeplyNestedTransient = Lifestyle.CreateHybrid(selector,
                Lifestyle.Singleton,
                Lifestyle.CreateHybrid(selector,
                    Lifestyle.Singleton,
                    Lifestyle.CreateHybrid(selector,
                        Lifestyle.Singleton,
                        Lifestyle.CreateHybrid(selector,
                            Lifestyle.Singleton,
                            Lifestyle.CreateHybrid(selector,
                                Lifestyle.Singleton,
                                Lifestyle.Transient)))));

            var dependency =
                CreateRelationship(parent: Lifestyle.Singleton, child: hybridWithDeeplyNestedTransient);

            // Act
            bool result = HasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "Since the hybrid lifestyle contains a transient lifestyle, this " +
                "hybrid lifestyle should be considered to be a transient when evaluated on the child.");
        }

        [TestMethod]
        public void GetInstance_DecoratorRegisteredTwiceAsSingletonWithTransientDecoratee_FailsDuringVerification()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<ICommandHandler<RealCommand>, StubCommandHandler>(Lifestyle.Transient);

            // Register the same decorator twice. 
            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionHandlerDecorator<>),
                Lifestyle.Singleton);

            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(TransactionHandlerDecorator<>),
                Lifestyle.Singleton);

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(
                "(Singleton) depends on ICommandHandler<RealCommand> implemented by StubCommandHandler",
                action);
        }

        private static KnownRelationship CreateRelationship(Lifestyle parent, Lifestyle child) => 
            new KnownRelationship(
                implementationType: typeof(RealTimeProvider),
                lifestyle: parent,
                dependency: 
                    new InstanceProducer(typeof(IDisposable), new DummyRegistration<IDisposable>(child)));

        private static bool HasPossibleLifestyleMismatch(KnownRelationship dependency) => 
            LifestyleMismatchChecker.HasLifestyleMismatch(new Container(), dependency);

        private class DummyRegistration<TImplementation> : Registration
        {
            public DummyRegistration(Lifestyle lifestyle) : base(lifestyle, new Container())
            {
            }

            public override Type ImplementationType => typeof(TImplementation);

            public override Expression BuildExpression()
            {
                throw new NotImplementedException();
            }
        }
        
        private class KnownDependencyConstructorParameters
        {
            public Type ParentServiceType { get; set; }

            public Type ParentImplementationType { get; set; }

            public Lifestyle ParentLifestyle { get; set; }

            public InstanceProducer ChildRegistration { get; set; }
        }
        
        private static class Lifestyles
        {
            internal static Lifestyle LifetimeScope = new FakeLifestyle("Lifetime Scope", 100);
            internal static Lifestyle WcfOperation = new FakeLifestyle("WCF Operation", 250);
            internal static Lifestyle WebRequest = new FakeLifestyle("Web Request", 300);
        }

        private class CustomScopedLifestyle : ScopedLifestyle
        {
            private readonly Lifestyle realLifestyle;

            public CustomScopedLifestyle(Lifestyle realLifestyle) : base("Custom " + realLifestyle.Name)
            {
                this.realLifestyle = realLifestyle;
            }

            protected internal override Func<Scope> CreateCurrentScopeProvider(Container container)
            {
                throw new NotImplementedException();
            }

            public override int Length => this.realLifestyle.ComponentLength(null);

            protected internal override Registration CreateRegistrationCore<TConcrete>(Container container)
            {
                return this.realLifestyle.CreateRegistration<TConcrete>(container);
            }

            protected internal override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator, 
                Container container)
            {
                return this.realLifestyle.CreateRegistration(instanceCreator, container);
            }
        }
    }
    
    internal class FakeLifestyle : Lifestyle
    {
        public FakeLifestyle(string name, int length) : base("Fake " + name)
        {
            this.Length = length;
        }

        public override int Length { get; }

        protected internal override Registration CreateRegistrationCore<TConcrete>(Container c) => 
            Transient.CreateRegistration<TConcrete>(c);

        protected internal override Registration CreateRegistrationCore<TService>(Func<TService> creator, Container c) => 
            Transient.CreateRegistration(creator, c);
    }
}