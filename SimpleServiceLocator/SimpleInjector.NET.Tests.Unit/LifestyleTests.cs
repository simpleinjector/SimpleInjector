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

        private sealed class FakeLifestyle : Lifestyle
        {
            public FakeLifestyle(string name) : base(name)
            {
            }

            protected override int Length
            {
                get { throw new NotImplementedException(); }
            }

            public override Registration CreateRegistration<TService, TImplementation>(Container container)
            {
                throw new NotImplementedException();
            }

            public override Registration CreateRegistration<TService>(System.Func<TService> instanceCreator, 
                Container container)
            {
                throw new NotImplementedException();
            }
        }
    }
}