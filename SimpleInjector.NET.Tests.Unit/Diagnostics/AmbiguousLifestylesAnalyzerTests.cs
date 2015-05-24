namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Extensions.LifetimeScoping;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class AmbiguousLifestylesAnalyzerTests
    {
        [TestMethod]
        public void Analyze_TwoRegistrationsWithDifferentLifestylesForTheSameImplementation_ReturnsExpectedWarnings()
        {
            // Arrange
            string expectedMessage1 = @"
                The registration for IFoo (Singleton) maps to the same implementation (FooBar) as the 
                registration for IBar (Transient) does, but the registration maps to a different lifestyle. 
                This will cause each registration to resolve to a different instance."
                .TrimInside();

            string expectedMessage2 = @"
                The registration for IBar (Transient) maps to the same implementation (FooBar) as the 
                registration for IFoo (Singleton) does, but the registration maps to a different lifestyle. 
                This will cause each registration to resolve to a different instance."
                .TrimInside();

            var container = new Container();

            container.Register<IFoo, FooBar>(Lifestyle.Singleton);
            container.Register<IBar, FooBar>(Lifestyle.Transient);

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container);

            // Assert
            Assert.AreEqual(2, results.Length, Actual(results));
            Assert_ContainsDescription(results, expectedMessage1);
            Assert_ContainsDescription(results, expectedMessage2);
            Assert_AllOfType<AmbiguousLifestylesDiagnosticResult>(results);
        }

        [TestMethod]
        public void Analyze_TwoRegistrationsWithDifferentLifestylesForTheSameImplementation_ReturnsSeverityWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IFoo, FooBar>(Lifestyle.Singleton);
            container.Register<IBar, FooBar>(Lifestyle.Transient);

            container.Verify();

            // Act
            var result = Analyzer.Analyze(container).OfType<AmbiguousLifestylesDiagnosticResult>().First();

            // Assert
            Assert.AreEqual(DiagnosticSeverity.Warning, result.Severity);
        }

        [TestMethod]
        public void Analyze_TwoRegistrationsWithDifferentLifestylesForTheSameImplementation_ReturnsExpectedDiagnosticResult()
        {
            // Arrange
            var container = new Container();

            container.Register<IFoo, FooBar>(Lifestyle.Singleton);
            container.Register<IBar, FooBar>(Lifestyle.Transient);

            container.Verify();

            var fooRegistration = container.GetRegistration(typeof(IFoo));
            var barRegistration = container.GetRegistration(typeof(IBar));

            // Act
            var result = (
                from r in Analyzer.Analyze(container).OfType<AmbiguousLifestylesDiagnosticResult>()
                where r.DiagnosedRegistration == fooRegistration
                select r)
                .First();

            // Assert
            Assert.AreSame(barRegistration, result.ConflictingRegistrations.Single());
            Assert.AreEqual(DiagnosticType.AmbiguousLifestyles, result.DiagnosticType);
            Assert.AreEqual(typeof(IFoo), result.ServiceType);
            Assert.AreEqual(typeof(FooBar), result.ImplementationType);
            Assert.IsTrue(result.Lifestyles.Contains(Lifestyle.Transient));
            Assert.IsTrue(result.Lifestyles.Contains(Lifestyle.Singleton));
            Assert.AreEqual(2, result.Lifestyles.Count);
        }

        [TestMethod]
        public void Analyze_FourProducersForTwoRegistrationsWithDifferentLifestylesForTheSameImplementation_ReturnsExpectedWarnings()
        {
            // Arrange
            string expectedMessage1 = @"
                The registration for IFoo (Lifetime Scope) maps to the same implementation (FooBar) as the 
                registrations for IFooExt (Singleton) and IBarExt (Singleton) do"
                .TrimInside();

            string expectedMessage2 = @"
                The registration for IFooExt (Singleton) maps to the same implementation (FooBar) as the 
                registrations for IFoo (Lifetime Scope) and IBar (Lifetime Scope) do"
                .TrimInside();

            var container = new Container();

            var transientFooBar = new LifetimeScopeLifestyle().CreateRegistration<FooBar>(container);
            var singletonFooBar = Lifestyle.Singleton.CreateRegistration<FooBar>(container);

            container.AddRegistration(typeof(IFoo), transientFooBar);
            container.AddRegistration(typeof(IBar), transientFooBar);

            container.AddRegistration(typeof(IFooExt), singletonFooBar);
            container.AddRegistration(typeof(IBarExt), singletonFooBar);

            container.Verify();

            // Act
            var results =
                Analyzer.Analyze(container).OfType<AmbiguousLifestylesDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(4, results.Length, Actual(results));
            Assert_ContainsDescription(results, expectedMessage1);
            Assert_ContainsDescription(results, expectedMessage2);
            Assert_AllOfType<AmbiguousLifestylesDiagnosticResult>(results);
        }

        [TestMethod]
        public void Analyze_ThreeRegistrationsWithDifferentLifestylesForTheSameImplementation_ReturnsExpectedWarnings()
        {
            // Arrange
            string expectedMessage = @"
                The registration for IFoo (Transient) maps to the same implementation (FooBar) as the 
                registrations for IBar (Lifetime Scope) and IFooExt (Singleton) do"
                .TrimInside();

            var container = new Container();

            container.Register<IFoo, FooBar>(Lifestyle.Transient);
            container.Register<IBar, FooBar>(new LifetimeScopeLifestyle());
            container.Register<IFooExt, FooBar>(Lifestyle.Singleton);

            container.Verify();

            // Act
            var results =
                Analyzer.Analyze(container).OfType<AmbiguousLifestylesDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(3, results.Length, Actual(results));
            Assert_ContainsDescription(results, expectedMessage);
            Assert_AllOfType<AmbiguousLifestylesDiagnosticResult>(results);
        }

        [TestMethod]
        public void Analyze_TwoRegistrationsWithMultipleInstancesOfTheSameLifestyle_GivesNoWarning()
        {
            // Arrange
            var container = new Container();

            var scopedFooBar1 = new LifetimeScopeLifestyle().CreateRegistration<FooBar>(container);
            var scopedFooBar2 = new LifetimeScopeLifestyle().CreateRegistration<FooBar>(container);

            container.AddRegistration(typeof(IFoo), scopedFooBar1);
            container.AddRegistration(typeof(IBar), scopedFooBar1);
            container.AddRegistration(typeof(IFooExt), scopedFooBar2);
            container.AddRegistration(typeof(IBarExt), scopedFooBar2);

            container.Verify();

            // Act
            var results =
                Analyzer.Analyze(container).OfType<AmbiguousLifestylesDiagnosticResult>().ToArray();

            // Assert
            Assert.IsFalse(results.Any(),
                "Diagnostics should be performed on the lifestyle's type, not on the instance because the " +
                "user is allowed to create multiple instances of the same lifestyle object. Actual: " +
                Actual(results));
        }

        [TestMethod]
        public void Analyze_TwoRegistrationsWithSameLifestyleForTheSameImplementation_ReturnsNoWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IFoo, FooBar>(new LifetimeScopeLifestyle());
            container.Register<IBar, FooBar>(new LifetimeScopeLifestyle());

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<AmbiguousLifestylesDiagnosticResult>();

            // Assert
            Assert.IsFalse(results.Any(), Actual(results));
        }

        [TestMethod]
        public void Analyze_OneViolationWithSuppressDiagnosticWarningOnOneRegistration_OneWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IFoo, FooBar>(Lifestyle.Transient);
            container.Register<IBar, FooBar>(Lifestyle.Singleton);

            container.Verify();

            var registration = container.GetRegistration(typeof(IFoo)).Registration;

            registration.SuppressDiagnosticWarning(DiagnosticType.AmbiguousLifestyles);

            // Act
            var results = Analyzer.Analyze(container).OfType<AmbiguousLifestylesDiagnosticResult>();

            // Assert
            Assert.AreEqual(1, results.Count(), Actual(results));
        }

        [TestMethod]
        public void Analyze_OneViolationWithSuppressDiagnosticWarningOnBothRegistrations_NoWarnings()
        {
            // Arrange
            var container = new Container();

            container.Register<IFoo, FooBar>(Lifestyle.Transient);
            container.Register<IBar, FooBar>(Lifestyle.Singleton);

            container.Verify();

            var registration1 = container.GetRegistration(typeof(IFoo)).Registration;
            var registration2 = container.GetRegistration(typeof(IBar)).Registration;

            registration1.SuppressDiagnosticWarning(DiagnosticType.AmbiguousLifestyles);
            registration2.SuppressDiagnosticWarning(DiagnosticType.AmbiguousLifestyles);

            // Act
            var results = Analyzer.Analyze(container).OfType<AmbiguousLifestylesDiagnosticResult>();

            // Assert
            Assert.IsFalse(results.Any(), Actual(results));
        }

        [TestMethod]
        public void Analyze_TwoRegistrationsOfSameImplementationThatAreNotPartOfTheContainer_StillGivesExpectedWarnings()
        {
            // Arrange
            var container = new Container();

            var a = Lifestyle.Transient.CreateProducer<IFoo, FooBar>(container);
            var b = Lifestyle.Singleton.CreateProducer<IFoo, FooBar>(container);

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container);

            // Assert
            Assert.AreEqual(2, results.Length, Actual(results));
            Assert_AllOfType<AmbiguousLifestylesDiagnosticResult>(results);

            GC.KeepAlive(a);
            GC.KeepAlive(b);
        }

        [TestMethod]
        public void Analyze_TwoDelegateRegistrationsForTheSameServiceTypeWithDifferentLifestyles_NoWarning()
        {
            // Arrange
            var container = new Container();

            var a = Lifestyle.Transient.CreateProducer<IFoo>(() => new FooBar(), container);
            var b = Lifestyle.Singleton.CreateProducer<IFoo>(() => new ChocolateBar(), container);

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container);

            // Assert
            Assert.AreEqual(0, results.Length, 
                "Registrations for delegates must be suppressed, because Simple Injector has no notion of " + 
                "the actual implementation. " +
                Actual(results));

            GC.KeepAlive(a);
            GC.KeepAlive(b);
        }
        
        private static void Assert_ContainsDescription(IEnumerable<DiagnosticResult> results,
            string expectedDescription)
        {
            var matchingResult =
                from result in results
                where result.Description.Contains(expectedDescription)
                select result;

            Assert.IsTrue(matchingResult.Any(), "Expected: " + expectedDescription + " " + Actual(results));
        }

        private static void Assert_AllOfType<T>(IEnumerable<DiagnosticResult> items) where T : DiagnosticResult
        {
            var notOfT =
                from item in items
                where !(item is T)
                select item;

            Assert.IsFalse(notOfT.Any(), "Not all items where of type " + typeof(T).Name + ". " + Actual(items));
        }

        private static string Actual(IEnumerable<DiagnosticResult> results)
        {
            return "Actual: " + string.Join(" - ", results.Select(r => r.Description));
        }
    }
}