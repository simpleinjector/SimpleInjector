namespace SimpleInjector.Tests.Unit.Analysis
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Analysis;
    using SimpleInjector.Lifestyles;
        
#if DEBUG
    [TestClass]
    public class LifestyleMismatchServicesTests
    {
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_TransientToSingleton_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Transient, child: Lifestyle.Singleton);
            
            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Every service can safely depend on a singleton.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_TransientToTransient_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Transient, child: Lifestyle.Transient);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_SingletonToSingleton_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Singleton, child: Lifestyle.Singleton);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Every service can safely depend on a singleton.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_SingletonToTransient_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Singleton, child: Lifestyle.Transient);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_SingletonToLifetimeScope_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Singleton, child: Lifestyles.LifetimeScope);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_SingletonToLifetimeWcfOperation_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Singleton, child: Lifestyles.WcfOperation);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_SingletonToLifetimeWebRequest_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Singleton, child: Lifestyles.WebRequest);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result);
        }
        
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_LifetimeScopeToSingleton_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.LifetimeScope, child: Lifestyle.Singleton);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Every service can safely depend on a singleton.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_WcfOperationeToSingleton_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.WcfOperation, child: Lifestyle.Singleton);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Every service can safely depend on a singleton.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_WebRequestToSingleton_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.WebRequest, child: Lifestyle.Singleton);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Every service can safely depend on a singleton.");
        }
        
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_LifetimeScopeToTransient_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.LifetimeScope, child: Lifestyle.Transient);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "Services can not depend on a dependency with a shorter lifestyle.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_WcfOperationeToTransient_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.WcfOperation, child: Lifestyle.Transient);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "Services can not depend on a dependency with a shorter lifestyle.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_WebRequestToTransient_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.WebRequest, child: Lifestyle.Transient);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "Services can not depend on a dependency with a shorter lifestyle.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_TransientToUnknown_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Transient, child: Lifestyles.Unknown);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "A transient service can safely depend on any other dependency.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_UnknownToTransient_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.Unknown, child: Lifestyle.Transient);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "An unknown lifestyle will always be bigger than transient.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_SingletonToUnknown_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyle.Singleton, child: Lifestyles.Unknown);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "The unknown lifestyle will likely be shorter than singleton.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_UnknownToSingleton_DoesNotReportAMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.Unknown, child: Lifestyle.Singleton);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Every dependency can always safely depend on a singleton.");
        }
        
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_LifetimeScopeToUnknown_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.LifetimeScope, child: Lifestyles.Unknown);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "A lifestyle longer than transient can not safely depend on an unknown " +
                "lifestyle, since this unknown lifestyle could act like a transient.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_UnknownToLifetimeScope_ReportsMismatch()
        {
            // Arrange
            var dependency = CreateRelationship(parent: Lifestyles.Unknown, child: Lifestyles.LifetimeScope);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "An unknown lifestyle can not safely depend on a lifestyle that is " +
                "shorter than singleton, since this unknown lifestyle could act like a singleton.");
        }
        
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_HybridToTheSameHybridInstance_DoesNotReportAMismatch()
        {
            // Arrange
            var hybrid = new HybridLifestyle(() => true, Lifestyle.Transient, Lifestyle.Singleton);

            var dependency = CreateRelationship(parent: hybrid, child: hybrid);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Since the both the parent and child have exactly the same lifestyle, " +
                "there is expected to be no mismatch.");
        }

        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_ShortHybridToLongHybrid_DoesNotReportAMismatch()
        {
            // Arrange
            var parentHybrid = new HybridLifestyle(() => true, Lifestyle.Transient, Lifestyle.Transient);
            var childHybrid = new HybridLifestyle(() => true, Lifestyle.Singleton, Lifestyle.Singleton);

            var dependency = CreateRelationship(parent: parentHybrid, child: childHybrid);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsFalse(result, "Both lifestyles of the parent are shorter than those of the child.");
        }
        
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_ShortHybridToLongHybrid_ReportsMismatch()
        {
            // Arrange
            var parentHybrid = new HybridLifestyle(() => true, Lifestyle.Singleton, Lifestyle.Singleton);
            var childHybrid = new HybridLifestyle(() => true, Lifestyle.Singleton, Lifestyle.Transient);

            var dependency = CreateRelationship(parent: parentHybrid, child: childHybrid);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "Both lifestyles of the parent are longer than those of the child.");
        }
        
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_NestedHybridSingletonToTransient1_ReportsMismatch()
        {
            // Arrange
            var hybridWithDeeplyNestedSingleton = new HybridLifestyle(() => true,
                Lifestyle.Transient, 
                new HybridLifestyle(() => true,
                    Lifestyle.Transient, 
                    new HybridLifestyle(() => true,
                        Lifestyle.Transient, 
                        new HybridLifestyle(() => true,
                            Lifestyle.Transient, 
                            new HybridLifestyle(() => true,
                                Lifestyle.Transient, 
                                Lifestyle.Singleton)))));

            var dependency = 
                CreateRelationship(parent: hybridWithDeeplyNestedSingleton, child: Lifestyle.Transient);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "Since the hybrid lifestyle contains a singleton lifestyle, this " +
                "hybrid lifestyle should be considered to be a singleton when evaluated on the parent.");
        }
        
        [TestMethod]
        public void DependencyHasPossibleLifestyleMismatch_NestedHybridSingletonToTransient2_ReportsMismatch()
        {
            // Arrange
            var hybridWithDeeplyNestedTransient = new HybridLifestyle(() => true,
                Lifestyle.Singleton,
                new HybridLifestyle(() => true,
                    Lifestyle.Singleton,
                    new HybridLifestyle(() => true,
                        Lifestyle.Singleton,
                        new HybridLifestyle(() => true,
                            Lifestyle.Singleton,
                            new HybridLifestyle(() => true,
                                Lifestyle.Singleton,
                                Lifestyle.Transient)))));

            var dependency =
                CreateRelationship(parent: Lifestyle.Singleton, child: hybridWithDeeplyNestedTransient);

            // Act
            bool result = LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency);

            // Assert
            Assert.IsTrue(result, "Since the hybrid lifestyle contains a transient lifestyle, this " +
                "hybrid lifestyle should be considered to be a transient when evaluated on the child.");
        }

        private static KnownRelationship CreateRelationship(Lifestyle parent, Lifestyle child)
        {
            var container = new Container();

            return new KnownRelationship(
                implementationType: typeof(RealTimeProvider),
                lifestyle: parent,
                dependency: new InstanceProducer(typeof(IDisposable), new DummyRegistration(child)));
        }

        private class DummyRegistration : Registration
        {
            public DummyRegistration(Lifestyle lifestyle) : base(lifestyle, new Container())
            {
            }

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
            internal static Lifestyle Unknown = UnknownLifestyle.Instance;
        }
    }
    
    internal class FakeLifestyle : Lifestyle
    {
        private readonly int length;

        public FakeLifestyle(string name, int length) : base("Fake " + name)
        {
            this.length = length;
        }

        protected override int Length
        {
            get { return this.length; }
        }

        public override Registration CreateRegistration<TService, TImplementation>(Container container)
        {
            return Lifestyle.Transient.CreateRegistration<TService, TImplementation>(container);
        }

        public override Registration CreateRegistration<TService>(Func<TService> instanceCreator, Container container)
        {
            return Lifestyle.Transient.CreateRegistration<TService>(instanceCreator, container);
        }
    }
#endif
}