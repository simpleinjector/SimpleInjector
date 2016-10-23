namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class PerGraphLifestyleTests
    {
        [TestMethod]
        public void GetInstance_WithPerResolveInstanceInGraph_ReusesSameInstanceThroughoutGraph()
        {
            // Arrange
            var container = new Container();

            PerGraphLifestyle.EnableFor(container);

            container.Register<C>(new PerGraphLifestyle());

            // Act
            var a = container.GetInstance<A>();

            // Assert
            Assert.AreSame(a.C, a.B.C);
        }

        [TestMethod]
        public void GetInstance_WithPerResolveInstanceInGraph_ReturnsNewInstanceWithDifferentGetInstance()
        {
            // Arrange
            var container = new Container();

            PerGraphLifestyle.EnableFor(container);

            container.Register<C>(new PerGraphLifestyle());

            // Act
            var a1 = container.GetInstance<A>();
            var a2 = container.GetInstance<A>();

            // Assert
            Assert.AreNotSame(a1.C, a2.C);
        }

        [TestMethod]
        public void GetInstance_WithPerResolveInstanceInGraphAndGraphTornByGetInstance_ReturnsNewInstanceWithDifferentGetInstance()
        {
            // Arrange
            var container = new Container();

            PerGraphLifestyle.EnableFor(container);

            container.Register<C>(new PerGraphLifestyle());
            container.Register<B>(() => new B(container.GetInstance<C>()));

            // Act
            var a = container.GetInstance<A>();

            // Assert
            Assert.AreNotSame(a.C, a.B.C);
        }

        [TestMethod]
        public void GetInstance_PerResolveLifestyleNotEnabledWithPerResolveInstanceInGraph_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Register<C>(new PerGraphLifestyle());

            // Act
            Action action = () => container.GetInstance<A>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "PerGraphLifestyle.EnableFor",
                action,
                "Should warn about calling PerGraphLifestyle.EnableFor first");
        }

        public class A
        {
            public readonly B B;
            public readonly C C;

            public A(B b, C c)
            {
                this.B = b;
                this.C = c;
            }
        }

        public class B
        {
            public readonly C C;

            public B(C c)
            {
                this.C = c;
            }
        }

        public class C
        {
        }
    }
}