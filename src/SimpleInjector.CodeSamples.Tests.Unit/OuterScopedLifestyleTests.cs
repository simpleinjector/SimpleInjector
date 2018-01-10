using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector.Lifestyles;

namespace SimpleInjector.CodeSamples.Tests.Unit
{
    [TestClass]
    public class OuterScopedLifestyleTests
    {
        public interface IService
        {
        }

        public interface IZ
        {
        }

        [TestMethod]
        public void GetInstance_TwoResolvesOnUnrelatedScopes_ResolvesDifferentInstances()
        {
            // Arrange
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var lifestyle = new OuterScopedLifestyle();

            container.Register<IService, Service>(lifestyle);

            IService s1;
            IService s2;

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                s1 = container.GetInstance<IService>();
            }

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                s2 = container.GetInstance<IService>();
            }

            // Assert
            Assert.AreNotSame(s1, s2);
        }



        [TestMethod]
        public void GetInstance_OnInnerScope_ResolvesSameInstanceAsItsOuterScope()
        {
            // Arrange
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var lifestyle = new OuterScopedLifestyle();

            container.Register<IService, Service>(lifestyle);
            container.Register<ILogger, NullLogger>(Lifestyle.Scoped);

            IService s1;
            IService s2;

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                s1 = container.GetInstance<IService>();

                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    s2 = container.GetInstance<IService>();
                }
            }

            // Assert
            Assert.AreSame(s1, s2);
        }

        [TestMethod]
        public void GetInstance_ResolvingOuterScopedLifestyleDependingOnScopedLifestyle_ThrowsLifestyleMismatchException()
        {
            // Arrange
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var lifestyle = new OuterScopedLifestyle();

            container.Register<IService, ServiceDependingOn<ILogger>>(lifestyle);
            container.Register<ILogger, NullLogger>(Lifestyle.Scoped);

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                // Act
                Action action = () => container.GetInstance<IService>();

                // Assert
                var ex = Assert.ThrowsException<ActivationException>(action);

                Assert.IsTrue(ex.Message.Contains("lifestyle mismatch"), message: "Actual: " + ex.Message);
            }
        }

        public class Service : IService
        {
        }

        public class ServiceDependingOn<TDependency> : IService
        {
            public ServiceDependingOn(TDependency dependency)
            {
            }
        }
    }
}