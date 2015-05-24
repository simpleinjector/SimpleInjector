namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics.Analyzers;
    using SimpleInjector.Diagnostics.Debugger;
    using SimpleInjector.Extensions;
    using SimpleInjector.Extensions.LifetimeScoping;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class TornLifestyleContainerAnalyzerTests
    {
        [TestMethod]
        public void Analyze_TwoSingletonRegistrationsForTheSameImplementation_ReturnsExpectedWarning()
        {
            // Arrange
            string expectedMessage1 =
                "The registration for IFoo maps to the same implementation and lifestyle as the " +
                "registration for IBar does. They both map to FooBar (Singleton).";

            string expectedMessage2 =
                "The registration for IBar maps to the same implementation and lifestyle as the " +
                "registration for IFoo does. They both map to FooBar (Singleton).";

            var container = new Container();

            container.Register<IFoo, FooBar>(Lifestyle.Singleton);
            container.Register<IBar, FooBar>(Lifestyle.Singleton);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(2, results.Length, Actual(results));
            Assert_ContainsDescription(results, expectedMessage1);
            Assert_ContainsDescription(results, expectedMessage2);
        }

        [TestMethod]
        public void Analyze_TwoSingletonRegistrationsForTheSameImplementation_ReturnsSeverityWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IFoo, FooBar>(Lifestyle.Singleton);
            container.Register<IBar, FooBar>(Lifestyle.Singleton);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var result = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().First();

            // Assert
            Assert.AreEqual(DiagnosticSeverity.Warning, result.Severity);
        }

        [TestMethod]
        public void Analyze_FourSingletonRegistrationsForTheSameImplementation_ReturnsExpectedWarning()
        {
            // Arrange
            string expectedMessage1 =
                "The registration for IFoo maps to the same implementation and lifestyle as the " +
                "registrations for IFooExt, IBar and IBarExt do. They all map to FooBar (Singleton).";

            string expectedMessage2 =
                "The registration for IFooExt maps to the same implementation and lifestyle as the " +
                "registrations for IFoo, IBar and IBarExt do.";

            string expectedMessage3 =
                "The registration for IBar maps to the same implementation and lifestyle as the " +
                "registrations for IFoo, IFooExt and IBarExt do.";

            string expectedMessage4 =
                "The registration for IBarExt maps to the same implementation and lifestyle as the " +
                "registrations for IFoo, IFooExt and IBar do.";

            var container = new Container();

            container.Register<IFoo, FooBar>(Lifestyle.Singleton);
            container.Register<IFooExt, FooBar>(Lifestyle.Singleton);
            container.Register<IBar, FooBar>(Lifestyle.Singleton);
            container.Register<IBarExt, FooBar>(Lifestyle.Singleton);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(4, results.Length, Actual(results));

            Assert_ContainsDescription(results, expectedMessage1);
            Assert_ContainsDescription(results, expectedMessage2);
            Assert_ContainsDescription(results, expectedMessage3);
            Assert_ContainsDescription(results, expectedMessage4);
        }

        [TestMethod]
        public void Analyze_TwoRegistrationsWithSameLifestyleForTheSameImplementation_ReturnsWarning()
        {
            // Arrange
            string expectedMessage =
                "The registration for IFoo maps to the same implementation and lifestyle as the " +
                "registration for IBar does. They both map to FooBar (Lifetime Scope).";

            var container = new Container();

            container.Register<IFoo, FooBar>(new LifetimeScopeLifestyle());
            container.Register<IBar, FooBar>(new LifetimeScopeLifestyle());

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(2, results.Length, Actual(results));

            Assert_ContainsDescription(results, expectedMessage);
        }

        [TestMethod]
        public void Analyze_OneViolationWithSuppressDiagnosticWarningOnOneRegistration_OneWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IFoo, FooBar>(new LifetimeScopeLifestyle());
            container.Register<IBar, FooBar>(new LifetimeScopeLifestyle());

            container.Verify(VerificationOption.VerifyOnly);

            var registration = container.GetRegistration(typeof(IFoo)).Registration;

            registration.SuppressDiagnosticWarning(DiagnosticType.TornLifestyle);

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(1, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_OneViolationWithSuppressDiagnosticWarningOnTheOtherRegistration_OneWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IFoo, FooBar>(new LifetimeScopeLifestyle());
            container.Register<IBar, FooBar>(new LifetimeScopeLifestyle());

            container.Verify(VerificationOption.VerifyOnly);

            var registration = container.GetRegistration(typeof(IBar)).Registration;

            registration.SuppressDiagnosticWarning(DiagnosticType.TornLifestyle);

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(1, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_OneViolationWithSuppressDiagnosticWarningOnBothRegistrations_NoWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IFoo, FooBar>(new LifetimeScopeLifestyle());
            container.Register<IBar, FooBar>(new LifetimeScopeLifestyle());

            container.Verify(VerificationOption.VerifyOnly);

            var registration1 = container.GetRegistration(typeof(IFoo)).Registration;
            var registration2 = container.GetRegistration(typeof(IBar)).Registration;

            registration1.SuppressDiagnosticWarning(DiagnosticType.TornLifestyle);
            registration2.SuppressDiagnosticWarning(DiagnosticType.TornLifestyle);

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>();

            // Assert
            Assert.IsFalse(results.Any(), Actual(results));
        }

        [TestMethod]
        public void Analyze_TwoDistinctRegistrationsForSameLifestyle_ReturnsWarning()
        {
            // Arrange
            var container = new Container();

            var fooReg = Lifestyle.Singleton.CreateRegistration<IFoo, FooBar>(container);
            var barReg = Lifestyle.Singleton.CreateRegistration<IBar, FooBar>(container);

            container.AddRegistration(typeof(IFoo), fooReg);
            container.AddRegistration(typeof(IBar), barReg);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(2, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_TwoSingletonRegistrationsForTheSameImplementationWithDecoratorApplied_ReturnsWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IFoo, FooBar>(Lifestyle.Singleton);
            container.Register<IBar, FooBar>(Lifestyle.Singleton);

            container.RegisterDecorator(typeof(IFoo), typeof(FooDecorator), Lifestyle.Singleton);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(2, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_TwoSingletonRegistrationsForTheSameImplementationWithDecoratorsApplied_ReturnsWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IFoo, FooBar>(Lifestyle.Singleton);
            container.Register<IBar, FooBar>(Lifestyle.Singleton);

            container.RegisterDecorator(typeof(IFoo), typeof(FooDecorator));
            container.RegisterDecorator(typeof(IBar), typeof(BarDecorator));

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(2, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_TwoSingletonRegistrationsMappedOnFourServiceTypes_ReturnsExpectedWarnings()
        {
            // Arrange
            string expectedMessage1 =
                "The registration for IFoo maps to the same implementation and lifestyle as the " +
                "registrations for IBar and IBarExt do.";

            string expectedMessage2 =
            "The registration for IBar maps to the same implementation and lifestyle as the " +
            "registrations for IFoo and IFooExt do.";

            var container = new Container();

            var fooReg = Lifestyle.Singleton.CreateRegistration<FooBar>(container);
            var barReg = Lifestyle.Singleton.CreateRegistration<FooBar>(container);

            container.AddRegistration(typeof(IFoo), fooReg);
            container.AddRegistration(typeof(IFooExt), fooReg);
            container.AddRegistration(typeof(IBar), barReg);
            container.AddRegistration(typeof(IBarExt), barReg);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(4, results.Length, Actual(results));

            Assert_ContainsDescription(results, expectedMessage1);
            Assert_ContainsDescription(results, expectedMessage2);
        }

        [TestMethod]
        public void Analyze_TwoTransientRegistrationsForTheSameImplementation_DoesNotReturnsAWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IFoo, FooBar>(Lifestyle.Transient);
            container.Register<IBar, FooBar>(Lifestyle.Transient);

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_TwoRegistrationsForTheSameImplementationButWithDifferentLifestyles_DoesNotReturnsAWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IFoo, FooBar>(Lifestyle.Singleton);
            container.Register<IBar, FooBar>(new LifetimeScopeLifestyle());

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_TwoRegistrationsForTheSameImplementationMadeWithTheSameRegistrationInstance_DoesNotReturnsAWarning()
        {
            // Arrange
            var container = new Container();

            var reg = Lifestyle.Singleton.CreateRegistration<FooBar>(container);

            container.AddRegistration(typeof(IFoo), reg);
            container.AddRegistration(typeof(IBar), reg);

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        // This was a bug reported here: https://github.com/simpleinjector/SimpleInjector/issues/24
        [TestMethod]
        public void Analyze_TwoRegistrationsForTheSameImplementationMadeWithTheSameRegistrationInstanceWrappedBySingletonDecorators_DoesNotReturnsAWarning()
        {
            // Arrange
            var container = new Container();

            var reg = Lifestyle.Singleton.CreateRegistration<FooBar>(container);

            container.AddRegistration(typeof(IFoo), reg);
            container.AddRegistration(typeof(IBar), reg);

            container.RegisterDecorator(typeof(IBar), typeof(BarDecorator), Lifestyle.Singleton);

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_ConfigurationWithViolation_ReturnsTheExpectedDebuggerViewItems()
        {
            // Arrange
            var container = new Container();

            container.Register<IFoo, FooBar>(Lifestyle.Singleton);
            container.Register<IBar, FooBar>(Lifestyle.Singleton);

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = DebuggerGeneralWarningsContainerAnalyzer.Analyze(container).Value as DebuggerViewItem[];

            // Assert
            Assert.AreEqual(typeof(IBar).Name, results[0].Name);
            Assert.AreEqual(typeof(IFoo).Name, results[1].Name);
        }

        [TestMethod]
        public void Analyze_TwoDelegateRegistrationsForTheSameAbstractServiceType_DoesNotReturnAWarning()
        {
            // Arrange
            var container = new Container();

            var reg1 = Lifestyle.Singleton.CreateRegistration<IFoo>(() => new FooBar(), container);
            var reg2 = Lifestyle.Singleton.CreateRegistration<IFoo>(() => new ChocolateBar(), container);

            container.RegisterCollection(typeof(IFoo), new[] { reg1, reg2 });

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(0, results.Length,
                "Since the two registrations both use a delegate, there's no way of knowing what the actual " +
                "implementation type is, and we should therefore not see a warning in this case. " +
                Actual(results));
        }

        [TestMethod]
        public void Analyze_TwoDelegateRegistrationsForTheSameConcreteServiceType_DoesNotReturnAWarning()
        {
            // Arrange
            var container = new Container();

            var reg1 = Lifestyle.Singleton.CreateRegistration<FooBar>(() => new FooBar(), container);
            var reg2 = Lifestyle.Singleton.CreateRegistration<FooBar>(() => new FooBarSub(), container);

            container.RegisterCollection(typeof(IFoo), new[] { reg1, reg2 });

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(0, results.Length,
                "Since the two registrations both use a delegate, there's no way of knowing what the actual " +
                "implementation type is, and we should therefore not see a warning in this case. " +
                Actual(results));
        }

        [TestMethod]
        public void Analyze_TwoSingletonRegistrationsForTheSameConcreteType_ReturnsTheExpectedWarning()
        {
            // Arrange
            var container = new Container();

            var reg1 = Lifestyle.Singleton.CreateRegistration<FooBar>(container);
            var reg2 = Lifestyle.Singleton.CreateRegistration<FooBar>(container);

            container.RegisterCollection(typeof(IFoo), new[] { reg1, reg2 });

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<TornLifestyleDiagnosticResult>().ToArray();

            // Assert
            Assert.AreEqual(2, results.Length, Actual(results));
        }

        private static void Assert_ContainsDescription(TornLifestyleDiagnosticResult[] results,
            string expectedDescription)
        {
            var matchingResult =
                from result in results
                where result.Description.Contains(expectedDescription)
                select result;

            Assert.IsTrue(matchingResult.Any(), Actual(results));
        }

        private static string Actual(IEnumerable<DiagnosticResult> results)
        {
            return "Actual: " + string.Join(" - ", results.Select(r => r.Description));
        }
    }
}