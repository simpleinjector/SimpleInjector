namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class PropertyInjectionExtensionsTests
    {
        public interface ILogger
        {
        }

        public interface IService
        {
        }

        [TestMethod]
        public void AutoInjectPropertiesWithAttribute_Always_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Options.AutoWirePropertiesWithAttribute<Attribute>();
        }

        [TestMethod]
        public void GetInstance_TypeWithoutGivenAttributeApplied_DoesNotInjectProperty()
        {
            // Arrange
            var container = new Container();

            container.Options.AutoWirePropertiesWithAttribute<Inject1Attribute>();

            // Act
            var service = container.GetInstance<ServiceWithoutAttributedProperties>();

            // Assert
            Assert.IsNull(service.Logger);
        }

        [TestMethod]
        public void GetInstance_TypeWithGivenAttributeApplied_InjectsProperty()
        {
            // Arrange
            var expectedDependency = new Logger();

            var container = new Container();

            container.Options.AutoWirePropertiesWithAttribute<Inject1Attribute>();

            container.RegisterSingleton<ILogger>(expectedDependency);

            // Act
            var service = container.GetInstance<ServiceWithAttributedProperty>();

            // Assert
            Assert.IsNotNull(service.Logger);
        }

        [TestMethod]
        public void GetInstance_TypeWithGivenAttributesApplied_InjectsProperties1()
        {
            // Arrange
            var expectedDependency = new Logger();

            var container = new Container();

            container.Options.AutoWirePropertiesWithAttribute<Inject1Attribute>();

            container.RegisterSingleton<ILogger>(expectedDependency);
            
            // Act
            var service = container.GetInstance<ServiceWithAttributedProperties>();

            // Assert
            Assert.IsNotNull(service.Logger1);
            Assert.IsNotNull(service.Logger2);
            Assert.IsNotNull(service.Logger3);
            Assert.IsNull(service.Logger4);
        }

        [TestMethod]
        public void GetInstance_TypeWithGivenAttributesApplied_InjectsProperties2()
        {
            // Arrange
            var expectedDependency = new Logger();

            var container = new Container();

            container.Options.AutoWirePropertiesWithAttribute<Inject2Attribute>();

            container.RegisterSingleton<ILogger>(expectedDependency);

            // Act
            var service = container.GetInstance<ServiceWithAttributedProperties>();

            // Assert
            Assert.IsNotNull(service.Logger1);
            Assert.IsNotNull(service.Logger2);
            Assert.IsNotNull(service.Logger3);
            Assert.IsNotNull(service.Logger4);
            Assert.IsNotNull(service.Logger5);
            Assert.IsNotNull(service.Logger6);
            Assert.IsNotNull(service.Logger7);
        }

        [TestMethod]
        public void GetInstance_TypeWithGivenAttributeAppliedButDependencyMissing_ThrowsException()
        {
            // Arrange
            var container = new Container();

            container.Options.AutoWirePropertiesWithAttribute<Inject1Attribute>();

            // Act
            // ServiceWithAttributedProperty depends on ILogger
            Action action = () => container.GetInstance<ServiceWithAttributedProperty>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                typeof(ServiceWithAttributedProperty).Name + 
                " could be found and an implicit registration could not be made.",
                action);

            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "ILogger that is not registered",
                action);
        }

        [TestMethod]
        public void AutoInjectPropertiesWithAttribute_MixedWithRegisterWithContext_ShouldInjectContextualDependency()
        {
            // Arrange
            var container = new Container();

            // This attribute must be applied before RegisterWithContext gets applied, since the registered
            // events adds new 
            container.Options.AutoWirePropertiesWithAttribute<Inject2Attribute>();

            container.RegisterWithContext<ILogger>(context => new ContextualLogger(context));
                        
            // Act
            var service = container.GetInstance<ServiceWithAttributedProperties>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(ContextualLogger), service.Logger1);
            Assert.AreEqual(
                typeof(ServiceWithAttributedProperties).Name,
                ((ContextualLogger)service.Logger1).Context.ImplementationType.Name);
        }

        [TestMethod]
        public void AutowireProperty_OnSingleProperty_InjectsThatPropertyAndNothingElse()
        {
            // Arrange
            var container = new Container();

            container.Options.EnablePropertyAutoWiring();

            container.Register<ILogger, Logger>();

            container.Register<ServiceWithAttributedProperties>();

            // Act
            container.AutoWireProperty<ServiceWithAttributedProperties>(s => s.Logger1);

            // Assert
            var service = container.GetInstance<ServiceWithAttributedProperties>();

            Assert.IsNotNull(service.Logger1);

            Assert.IsNull(service.Logger2);
            Assert.IsNull(service.Logger3);
            Assert.IsNull(service.Logger4);
            Assert.IsNull(service.Logger5);
            Assert.IsNull(service.Logger6);
            Assert.IsNull(service.Logger7);
        }
        
        [TestMethod]
        public void AutowireProperty_WithoutACallToEnablePropertyAutowiring_ThrowException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.AutoWireProperty<ServiceWithAttributedProperties>(s => s.Logger1);

                // Assert
                Assert.Fail("Expected exception.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(
                    ex.Message.Contains("Please call container.Options.EnablePropertyAutoWiring() first."));
            }
        }

        public sealed class Inject1Attribute : Attribute
        {
            public Inject1Attribute()
            {
            }
        }

        public sealed class Inject2Attribute : Attribute
        {
            public Inject2Attribute()
            {
            }
        }

        public class Logger : ILogger
        {
        }

        public class ContextualLogger : ILogger
        {
            public ContextualLogger(DependencyContext context)
            {
                this.Context = context;
            }

            public DependencyContext Context { get; }
        }

        public class ServiceWithoutAttributedProperties : IService
        {
            public ILogger Logger { get; set; }
        }

        public class ServiceWithAttributedProperty : IService
        {
            [Inject1]
            public ILogger Logger { get; set; }
        }

        public class ServiceWithAttributedProperties : IService
        {
            [Inject1]
            [Inject2]
            public ILogger Logger1 { get; set; }

            [Inject1]
            [Inject2]
            public ILogger Logger2 { get; set; }

            [Inject1]
            [Inject2]
            public ILogger Logger3 { get; set; }

            [Inject2]
            public ILogger Logger4 { get; set; }

            [Inject2]
            public ILogger Logger5 { get; set; }

            [Inject2]
            public ILogger Logger6 { get; set; }

            [Inject2]
            public ILogger Logger7 { get; set; }
        }
    }
}