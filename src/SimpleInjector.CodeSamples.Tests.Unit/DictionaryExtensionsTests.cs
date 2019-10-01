namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Lifestyles;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class DictionaryExtensionsTests
    {
        private enum StrategyType { A, B, C }

        private interface IStrategy
        {
            StrategyType Type { get; }
        }

        [TestMethod]
        public void RegisterDictionary_ResolvedDictionary_DictionaryIsASingleton()
        {
            // Arrange
            var container = new Container();
            container.Options.ResolveUnregisteredConcreteTypes = false;

            container.Collection.Register<IStrategy>(typeof(A), typeof(B), typeof(C));

            container.Collection.RegisterDictionary<StrategyType, IStrategy>(s => s.Type);

            // Act
            var dictionary1 = container.GetInstance<IReadOnlyDictionary<StrategyType, IStrategy>>();
            var dictionary2 = container.GetInstance<IReadOnlyDictionary<StrategyType, IStrategy>>();

            // Assert
            Assert.AreSame(dictionary1, dictionary2, "The dictionary is expected to be a singleton.");
        }

        [TestMethod]
        public void RegisterDictionary_ResolvedDictionary_ResolvesUnderlyingServices()
        {
            // Arrange
            var container = new Container();
            container.Options.ResolveUnregisteredConcreteTypes = false;

            container.Collection.Register<IStrategy>(typeof(A), typeof(B), typeof(C));

            container.Collection.RegisterDictionary<StrategyType, IStrategy>(s => s.Type);

            var dictionary = container.GetInstance<IReadOnlyDictionary<StrategyType, IStrategy>>();

            // Act
            var a = dictionary[StrategyType.A];
            var b = dictionary[StrategyType.B];
            var c = dictionary[StrategyType.C];

            // Assert
            Assert.IsInstanceOfType(a, typeof(A));
            Assert.IsInstanceOfType(b, typeof(B));
            Assert.IsInstanceOfType(c, typeof(C));
        }

        [TestMethod]
        public void RegisterDictionary_ResolvedDictionary_ResolvesUsingExpectedLifestyles()
        {
            // Arrange
            var container = new Container();
            container.Options.ResolveUnregisteredConcreteTypes = false;

            container.Register<A>(Lifestyle.Singleton);
            container.Collection.Register<IStrategy>(typeof(A), typeof(B), typeof(C));

            container.Collection.RegisterDictionary<StrategyType, IStrategy>(s => s.Type);

            var dictionary = container.GetInstance<IReadOnlyDictionary<StrategyType, IStrategy>>();

            // Act
            var a1 = dictionary[StrategyType.A];
            var a2 = dictionary[StrategyType.A];
            var b1 = dictionary[StrategyType.B];
            var b2 = dictionary[StrategyType.B];

            // Assert
            Assert.AreSame(a1, a2, "Should be singleton");
            Assert.AreNotEqual(b1, b2, "Should be transient");
        }

        [TestMethod]
        public void RegisterDictionary_VerifyOnCorrectConfiguration_Succeeds()
        {
            // Arrange
            var container = new Container();
            container.Options.ResolveUnregisteredConcreteTypes = false;

            container.Collection.Register<IStrategy>(typeof(A), typeof(B), typeof(C));

            container.Collection.RegisterDictionary<StrategyType, IStrategy>(s => s.Type);

            container.Register<ServiceWithDependency<IReadOnlyDictionary<StrategyType, IStrategy>>>();

            // Act
            container.Verify();
        }

        [TestMethod]
        public void RegisterDictionary_IncompleteConfiguration_ResolvingDictionaryStillSucceeds()
        {
            // Arrange
            var container = new Container();
            container.Options.EnableAutoVerification = false;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Collection.Register<IStrategy>(typeof(A), typeof(B));

            // To simulate an incomplete configuration, we append a third, failing registration.
            container.Collection.Append<IStrategy>(() => throw new Exception("C"), Lifestyle.Transient);

            // We also make the second item scoped.
            container.Register<B>(Lifestyle.Scoped);

            container.Collection.RegisterDictionary<StrategyType, IStrategy>(s => s.Type);

            // Act
            // This call should still succeed, even with the invalid registration and with the scoped
            // registration (while resolving outside the context of an active scope), because at this point
            // the items are not resolved.
            var dictionary = container.GetInstance<IReadOnlyDictionary<StrategyType, IStrategy>>();
        }

        private class A : IStrategy
        {
            public StrategyType Type => StrategyType.A;
        }

        private class B : IStrategy
        {
            public StrategyType Type => StrategyType.B;
        }

        private class C : IStrategy
        {
            public StrategyType Type => StrategyType.C;
        }
    }
}