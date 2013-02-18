namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExplicitPropertyInjectionExtensionsTests
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
            container.Options.AutowireProperties<Attribute>();
        }

        [TestMethod]
        public void GetInstance_TypeWithoutGivenAttributeApplied_DoesNotInjectProperty()
        {
            // Arrange
            var container = new Container();

            container.Options.AutowireProperties<Inject1Attribute>();

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

            container.Options.AutowireProperties<Inject1Attribute>();

            container.RegisterSingle<ILogger>(expectedDependency);

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

            container.Options.AutowireProperties<Inject1Attribute>();

            container.RegisterSingle<ILogger>(expectedDependency);
            
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

            container.Options.AutowireProperties<Inject2Attribute>();

            container.RegisterSingle<ILogger>(expectedDependency);

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

            container.Options.AutowireProperties<Inject1Attribute>();

            try
            {
                // Act
                // ServiceWithAttributedProperty depends on ILogger
                var service = container.GetInstance<ServiceWithAttributedProperty>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                Assert.IsTrue(ex.Message.Contains(typeof(ServiceWithAttributedProperty).Name + " threw an exception."));
                Assert.IsTrue(ex.Message.Contains(typeof(ILogger).Name + " could be found."));
            }
        }

        [TestMethod]
        public void AutoInjectPropertiesWithAttribute_MixedWithRegisterWithContext_ShouldInjectContextualDependency()
        {
            // Arrange
            var container = new Container();

            // This attribute must be applied before RegisterWithContext gets applied, since the registered
            // events adds new 
            container.Options.AutowireProperties<Inject2Attribute>();

            container.RegisterWithContext<ILogger>(context => new ContextualLogger(context));
                        
            // Act
            var service = container.GetInstance<ServiceWithAttributedProperties>();

            // Assert
            Assert.IsInstanceOfType(service.Logger1, typeof(ContextualLogger));
            Assert.AreEqual(
                typeof(ServiceWithAttributedProperties).Name,
                ((ContextualLogger)service.Logger1).Context.ImplementationType.Name);
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

            public DependencyContext Context { get; private set; }
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