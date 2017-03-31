namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    [TestClass]
    public class InstanceProducerTests
    {
        public interface IOne
        {
        }

        public interface ITwo
        {
        }

        public interface INode
        {
        }

        public interface INodeFactory
        {
        }

        [TestMethod]
        public void GetInstance_Always_LocksTheContainer()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration = container.GetRegistration(typeof(Container));

            // Act
            registration.GetInstance();

            // Assert
            Assert.IsTrue(container.IsLocked());
        }

        [TestMethod]
        public void BuildExpression_Always_LocksTheContainer()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration = container.GetRegistration(typeof(Container));

            // Act
            registration.BuildExpression();

            // Assert
            Assert.IsTrue(container.IsLocked());
        }

        [TestMethod]
        public void GetRelationships_AfterVerification_ReturnsTheExpectedRelationships()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>();

            var registration = container.GetRegistration(typeof(RealUserService));

            container.Verify();

            // Act
            var relationships = registration.GetRelationships();

            // Assert
            Assert.AreEqual(1, relationships.Length);
            Assert.AreEqual(typeof(RealUserService), relationships[0].ImplementationType);
            Assert.AreEqual(Lifestyle.Transient, relationships[0].Lifestyle);
            Assert.AreEqual(typeof(IUserRepository), relationships[0].Dependency.ServiceType);
        }

        // This test proves a bug in v2.2.3.
        [TestMethod]
        public void GetRelationships_ForInstanceProducerThatSharesThRegistrationWithAnOtherProducer_HasItsOwnSetOfRegistrations()
        {
            // Arrange
            var container = ContainerFactory.New();

            var registration = Lifestyle.Transient.CreateRegistration<OneAndTwo>(container);

            container.AddRegistration(typeof(IOne), registration);
            container.AddRegistration(typeof(ITwo), registration);

            // Decorator to wrap around IOne (and not ITwo).
            container.RegisterDecorator(typeof(IOne), typeof(OneDecorator));

            InstanceProducer twoProducer = container.GetRegistration(typeof(ITwo));

            container.Verify();

            // Act
            KnownRelationship[] relationships = twoProducer.GetRelationships();

            // Assert
            Assert.IsFalse(relationships.Any(),
                "The InstanceProducer for ITwo was expected to have no relationships. Current: " +
                relationships.Select(r => r.ImplementationType).ToFriendlyNamesText());
        }

        [TestMethod]
        public void VisualizeObjectGraph_NestedObjectGraphWithMultipleDecorators_BuildsTheExpectedGraph()
        {
            // Arrange
            string expectedObjectGraph =
@"PluginDecorator(
    PluginDecoratorWithDependencyOfType<FakeTimeProvider>(
        FakeTimeProvider(),
        PluginDecorator<Int32>(
            PluginWithDependencyOfType<RealTimeProvider>(
                RealTimeProvider()))))";

            var container = ContainerFactory.New();

            container.Register<IPlugin, PluginWithDependencyOfType<RealTimeProvider>>();
            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator<int>));
            container.RegisterDecorator(typeof(IPlugin),
                typeof(PluginDecoratorWithDependencyOfType<FakeTimeProvider>));
            container.RegisterDecorator(typeof(IPlugin), typeof(PluginDecorator));

            container.Verify();

            var pluginProducer = container.GetRegistration(typeof(IPlugin));

            // Act
            string actualObjectGraph = pluginProducer.VisualizeObjectGraph();

            // Assert
            Assert.AreEqual(expectedObjectGraph, actualObjectGraph);
        }

        [TestMethod]
        public void VisualizeObjectGraph_WhenCalledBeforeInstanceIsCreated_ThrowsAnInvalidOperationException()
        {
            // Arrange
            var container = new Container();

            container.Register<RealTimeProvider>();

            InstanceProducer producer = container.GetRegistration(typeof(RealTimeProvider));

            // Act
            Action action = () => producer.VisualizeObjectGraph();

            // Assert
            AssertThat.Throws<InvalidOperationException>(action,
                "When the instance hasn't been created or the Expression hasn't been built, there's not yet " +
                "enough information to visualize the object graph. Instead of returning an incorrect result " +
                "we expect the library to throw an exception here.");
        }

        [TestMethod]
        public void VisualizeObjectGraph_DelayedCyclicReference_VisualizesTheExpectedObjectGraph()
        {
            // Arrange
            string expectedGraph = @"InstanceProducerTests.NodeFactory(
    IEnumerable<InstanceProducerTests.INode>(
        InstanceProducerTests.NodeFactory(
            IEnumerable<InstanceProducerTests.INode>(/* cyclic dependency graph detected */))))";

            var container = new Container();

            // class NodeOne(INodeFactory factory)
            container.RegisterCollection<INode>(new[] { typeof(NodeOne) });

            // class NodeFactory(IEnumerable<INode>)
            container.Register<INodeFactory, NodeFactory>();

            container.Verify();

            var registration = container.GetRegistration(typeof(INodeFactory));

            // Act
            string actualGraph = registration.VisualizeObjectGraph();

            // Assert
            Assert.AreEqual(expectedGraph, actualGraph);
        }

        [TestMethod]
        public void VisualizeObjectGraph_DependenciesAppearingMultipleTimesInObjectGraph_BuildsTheExpectedGraph()
        {
            // Arrange
            string expectedObjectGraph =
@"PluginWithDependencies<PluginWithDependencyOfType<PluginWithDependencyOfType<RealTimeProvider>>, ServiceDependingOn<PluginWithDependencyOfType<RealTimeProvider>>>(
    PluginWithDependencyOfType<PluginWithDependencyOfType<RealTimeProvider>>(
        PluginWithDependencyOfType<RealTimeProvider>(
            RealTimeProvider())),
    ServiceDependingOn<PluginWithDependencyOfType<RealTimeProvider>>(
        PluginWithDependencyOfType<RealTimeProvider>(
            RealTimeProvider())))";

            var container = ContainerFactory.New();

            var pluginProducer = container.GetRegistration(
                typeof(PluginWithDependencies<PluginWithDependencyOfType<PluginWithDependencyOfType<RealTimeProvider>>,
                    ServiceDependingOn<PluginWithDependencyOfType<RealTimeProvider>>>));

            container.Verify();

            // Act
            string actualObjectGraph = pluginProducer.VisualizeObjectGraph();

            // Assert
            Assert.AreEqual(expectedObjectGraph, actualObjectGraph);
        }

        public class OneAndTwo : IOne, ITwo
        {
        }

        public class OneDecorator : IOne
        {
            public OneDecorator(IOne one)
            {
            }
        }

        public class NodeOne : INode
        {
            public NodeOne(INodeFactory nodeFactory)
            {
            }
        }

        public class NodeFactory : INodeFactory
        {
            public NodeFactory(IEnumerable<INode> nodes)
            {
            }
        }
    }
}