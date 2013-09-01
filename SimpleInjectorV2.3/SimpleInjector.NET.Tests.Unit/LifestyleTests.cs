namespace SimpleInjector.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Lifestyles;

    [TestClass]
    public class LifestyleTests
    {
        [TestMethod]
        public void Transient_Always_ReturnsAnInstance()
        {
            Assert.IsNotNull(Lifestyle.Transient);
        }

        [TestMethod]
        public void Transient_Always_ReturnsTheSameInstance()
        {
            // Act
            var transient1 = Lifestyle.Transient;
            var transient2 = Lifestyle.Transient;

            Assert.IsTrue(object.ReferenceEquals(transient1, transient2),
                "Lifestyle implementations must be tread-safe, so from a performance perspective, the " +
                "Lifestyle.Transient property must return a singleton.");
        }

        [TestMethod]
        public void Singleton_Always_ReturnsAnInstance()
        {
            Assert.IsNotNull(Lifestyle.Singleton);
        }

        [TestMethod]
        public void Singleton_Always_ReturnsTheSameInstance()
        {
            // Act
            var singleton1 = Lifestyle.Singleton;
            var singleton2 = Lifestyle.Singleton;

            Assert.IsTrue(object.ReferenceEquals(singleton1, singleton2),
                "Lifestyle implementations must be tread-safe, so from a performance perspective, the " +
                "Lifestyle.Singleton property must return a singleton.");
        }

        [TestMethod]
        public void Ctor_NullArgument_ThrowsExpectedException()
        {
            Action action = () => new FakeLifestyle(null);

            AssertThat.ThrowsWithParamName<ArgumentNullException>("name", action);
        }

        [TestMethod]
        public void Ctor_EmptyStringArgument_ThrowExpectedException()
        {
            Action action = () => new FakeLifestyle(string.Empty);

            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>("Value can not be empty.", action);
        }

        [TestMethod]
        public void Ctor_EmptyStringArgument_ThrowExpectedArgumentName()
        {
            Action action = () => new FakeLifestyle(string.Empty);

            AssertThat.ThrowsWithParamName<ArgumentException>("name", action);
        }

        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior2()
        {
            // Arrange
            var lifestyle = new FakeLifestyle();

            Assert.IsFalse(lifestyle.Initialized, "Test setup failed.");

            var container = new Container();

            // Act
            container.Register<ITimeProvider, RealTimeProvider>(lifestyle);

            // Assert
            Assert.IsTrue(lifestyle.Initialized);
        }

        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior3()
        {
            // Arrange
            var lifestyle = new FakeLifestyle();

            var container = new Container();

            // Act
            container.Register<ITimeProvider>(() => new RealTimeProvider(), lifestyle);

            // Assert
            Assert.IsTrue(lifestyle.Initialized);
        }

        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior4()
        {
            // Arrange
            var lifestyle = new FakeLifestyle();

            var container = new Container();

            // Act
            lifestyle.CreateRegistration<ITimeProvider, RealTimeProvider>(container);

            // Assert
            Assert.IsTrue(lifestyle.Initialized);
        }
        
        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior5()
        {
            // Arrange
            var lifestyle = new FakeLifestyle();

            var container = new Container();

            // Act
            lifestyle.CreateRegistration<ITimeProvider>(() => new RealTimeProvider(), container);

            // Assert
            Assert.IsTrue(lifestyle.Initialized);
        }

        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior6()
        {
            // Arrange
            var lifestyle = new FakeLifestyle();

            var container = new Container();

            // Act
            lifestyle.CreateRegistration(typeof(ITimeProvider), typeof(RealTimeProvider), container);

            // Assert
            Assert.IsTrue(lifestyle.Initialized);
        }

        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior7()
        {
            // Arrange
            var lifestyle = new FakeLifestyle();

            var container = new Container();

            // Act
            lifestyle.CreateRegistration(typeof(ITimeProvider), () => new RealTimeProvider(), container);

            // Assert
            Assert.IsTrue(lifestyle.Initialized);
        }
        
        private sealed class FakeLifestyle : Lifestyle
        {
            public FakeLifestyle() : base("Fake")
            {
            }

            public FakeLifestyle(string name) : base(name)
            {
            }

            public bool Initialized { get; private set; }

            public override void Initialize(Container container)
            {
                base.Initialize(container);
                this.Initialized = true;
            }

            protected override int Length
            {
                get { throw new NotImplementedException(); }
            }

            protected override Registration CreateRegistrationCore<TService, TImplementation>(
                Container container)
            {
                return Lifestyle.Transient.CreateRegistration<TService, TImplementation>(container);
            }

            protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator, 
                Container container)
            {
                return Lifestyle.Transient.CreateRegistration<TService>(instanceCreator, container);
            }
        }
    }
}