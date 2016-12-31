namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    [TestClass]
    public class RegisterResolveInterceptorTests
    {
        private static readonly Predicate<InitializationContext> Always = c =>
        {
            Assert.IsNotNull(c);
            return true;
        };

        [TestMethod]
        public void ContainerGetInstance_ResolvingAnInstance_CallsTheInterceptorWithADeletateThatReturnsTheExpectedInstance()
        {
            // Arrange
            var resolvedInstances = new InstanceInitializationDataCollection();

            var container = new Container();

            container.Options.RegisterResolveInterceptor(resolvedInstances.Intercept, Always);

            // Act
            container.GetInstance<Container>();

            object actualInstance = resolvedInstances.First().Instance;

            // Assert
            AssertThat.IsInstanceOfType(typeof(Container), actualInstance, resolvedInstances.ToString());
            Assert.AreEqual(1, resolvedInstances.Count, resolvedInstances.ToString());
        }

        [TestMethod]
        public void InstanceProducerGetInstance_ResolvingAnInstance_CallsTheInterceptorWithADeletateThatReturnsTheExpectedInstance()
        {
            // Arrange
            var resolvedInstances = new InstanceInitializationDataCollection();

            var container = new Container();

            container.Options.RegisterResolveInterceptor(resolvedInstances.Intercept, Always);

            // Act
            container.GetRegistration(typeof(Container)).GetInstance();

            object actualInstance = resolvedInstances.Single().Instance;

            // Assert
            AssertThat.IsInstanceOfType(typeof(Container), actualInstance, resolvedInstances.ToString());
        }

        [TestMethod]
        public void ContainerGetInstance_ResolvingAnInstanceWithDependencies_OnlyCallsTheDelegateForThisRootInstance()
        {
            // Arrange
            var resolvedInstances = new InstanceInitializationDataCollection();

            var container = new Container();

            container.Options.RegisterResolveInterceptor(resolvedInstances.Intercept, Always);

            // Act
            container.GetInstance<Dep1>();

            object actualInstance = resolvedInstances.First().Instance;

            // Assert
            AssertThat.IsInstanceOfType(typeof(Dep1), actualInstance, resolvedInstances.ToString());
            Assert.AreEqual(1, resolvedInstances.Count, resolvedInstances.ToString());
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInstanceTwice_CallsTheInterceptorDelegateTwice()
        {
            // Arrange
            var resolvedInstances = new InstanceInitializationDataCollection();

            var container = new Container();

            container.Options.RegisterResolveInterceptor(resolvedInstances.Intercept, Always);

            // Act
            container.GetInstance<Container>();
            container.GetInstance<Container>();

            // Assert
            Assert.AreEqual(2, resolvedInstances.Count, resolvedInstances.ToString());
        }

        [TestMethod]
        public void GetInstance_ResolvingAnInstance_CallsTheInterceptorWithTheExpectedInitializationContext()
        {
            // Arrange
            var resolvedInstances = new InstanceInitializationDataCollection();

            var container = new Container();

            container.Options.RegisterResolveInterceptor(resolvedInstances.Intercept, Always);

            // Act
            container.GetInstance<Container>();

            InitializationContext actualContext = resolvedInstances.First().Context;

            // Assert
            Assert.AreSame(Lifestyle.Singleton, actualContext.Registration.Lifestyle, resolvedInstances.ToString());
            Assert.AreEqual(typeof(Container), actualContext.Registration.ImplementationType, resolvedInstances.ToString());
        }

        [TestMethod]
        public void GetInstance_ThreeResolveInterceptorsRegistered_CallsAllThreeInterceptors()
        {
            // Arrange
            var resolvedInstances = new InstanceInitializationDataCollection();

            var container = new Container();

            container.Options.RegisterResolveInterceptor(resolvedInstances.Intercept, Always);
            container.Options.RegisterResolveInterceptor(resolvedInstances.Intercept, Always);
            container.Options.RegisterResolveInterceptor(resolvedInstances.Intercept, Always);

            // Act
            container.GetInstance<Container>();

            // Assert
            Assert.AreEqual(3, resolvedInstances.Count, resolvedInstances.ToString());
        }

        [TestMethod]
        public void GetInstance_ThreeResolveInterceptorsRegistered_FlowsInstanceCorrectlyThroughInterceptors()
        {
            // Arrange
            var resolvedInstances = new InstanceInitializationDataCollection();

            var container = new Container();

            container.Options.RegisterResolveInterceptor(resolvedInstances.Intercept, Always);
            container.Options.RegisterResolveInterceptor(resolvedInstances.Intercept, Always);
            container.Options.RegisterResolveInterceptor(resolvedInstances.Intercept, Always);

            // Act
            container.GetInstance<Container>();

            // Assert
            Assert.AreSame(container, resolvedInstances.Last().Instance);
        }

        [TestMethod]
        public void GetInstance_ThreeResolveInterceptorsRegistered_InterceptorsAreWrappedInOrderOfRegistration()
        {
            // Arrange
            string expectedOrder = "CBA";
            string actualOrder = string.Empty;

            var resolvedInstances = new InstanceInitializationDataCollection();

            var container = new Container();

            container.Options.RegisterResolveInterceptor(new Interceptor(() => actualOrder += "A").Intercept, Always);
            container.Options.RegisterResolveInterceptor(new Interceptor(() => actualOrder += "B").Intercept, Always);
            container.Options.RegisterResolveInterceptor(new Interceptor(() => actualOrder += "C").Intercept, Always);

            // Act
            container.GetInstance<Container>();

            // Assert
            Assert.AreEqual(expectedOrder, actualOrder);
        }

        [TestMethod]
        public void GetInstance_ThreeResolveInterceptorsRegisteredWithTwoPredicateFalse_AppliesTheCorrectInterceptor()
        {
            // Arrange
            string expected = "B";
            string actual = string.Empty;

            var resolvedInstances = new InstanceInitializationDataCollection();

            var container = new Container();

            container.Options.RegisterResolveInterceptor(new Interceptor(() => actual += "A").Intercept, c => false);
            container.Options.RegisterResolveInterceptor(new Interceptor(() => actual += "B").Intercept, Always);
            container.Options.RegisterResolveInterceptor(new Interceptor(() => actual += "C").Intercept, c => false);

            // Act
            container.GetInstance<Container>();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetInstance_InterceptorThatReturnsNull_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Options.RegisterResolveInterceptor((c, p) => null, Always);

            // Act
            Action action = () => container.GetInstance<Container>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "delegate that was registered using 'RegisterResolveInterceptor' returned null",
                action);
        }

        private sealed class InstanceInitializationDataCollection : List<InstanceInitializationPair>
        {
            private int recursiveCount = 100;

            public object Intercept(InitializationContext context, Func<object> instanceProducer)
            {
                if (this.recursiveCount < 0)
                {
                    Assert.Fail("Stack overflow prevented.");
                }

                this.recursiveCount--;

                object instance = instanceProducer();

                this.Add(new InstanceInitializationPair(context, instance));

                return instance;
            }

            public override string ToString()
            {
                var registrations =
                    from data in this
                    select new
                    {
                        ServiceType = data.Context.Producer.ServiceType.ToFriendlyName(),
                        Lifestyle = data.Context.Registration.Lifestyle.Name,
                    };

                return string.Join(Environment.NewLine, registrations.Select(r => r.ToString()));
            }
        }

        private sealed class InstanceInitializationPair
        {
            public InstanceInitializationPair(InitializationContext context, object instance)
            {
                this.Context = context;
                this.Instance = instance;
            }

            public InitializationContext Context { get; }
            public object Instance { get; }
        }

        private sealed class Interceptor
        {
            private readonly Action before;
            private int recursiveCount = 20;

            public Interceptor(Action before)
            {
                this.before = before;
            }

            internal object Intercept(InitializationContext context, Func<object> instanceProducer)
            {
                if (this.recursiveCount < 0)
                {
                    Assert.Fail("Stack overflow prevented.");
                }

                this.recursiveCount--;

                this.before();

                return instanceProducer();
            }
        }
    }
}