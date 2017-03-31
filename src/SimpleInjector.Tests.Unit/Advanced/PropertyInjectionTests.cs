namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    /// <summary>Tests for property injection.</summary>
    [TestClass]
    public partial class PropertyInjectionTests
    {
        public interface IService
        {
        }

        [TestMethod]
        public void InjectingAllProperties_OnTypeWithPublicWritableProperty_InjectsPropertyDependency()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            // Act
            var service = container.GetInstance<ServiceWithProperty<ITimeProvider>>();

            // Assert
            Assert.IsNotNull(service.Dependency);
        }

        [TestMethod]
        public void InjectingAllProperties_OnTypeWithPublicWritableProperty_InjectsPropertyOfBaseType()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            // Act
            var service = container.GetInstance<SubClassServiceWithProperty<ITimeProvider>>();

            // Assert
            Assert.IsNotNull(service.BaseClassDependency, "The dependency from the base should be injected.");
        }

        [TestMethod]
        public void InjectingAllProperties_OnTypeWithPublicWritablePropertyButRegistrationMissing_ThrowsException()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            // Act
            Action action = () => container.GetInstance<ServiceWithProperty<ITimeProvider>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                property with name 'Dependency' and type ITimeProvider that is not registered. 
                Please ensure ITimeProvider is registered"
                .TrimInside(), 
                action);
        }

        [TestMethod]
        public void InjectingAllProperties_OnTypeWithReadOnlyProperty_ThrowsExpectedException()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            // Act
            Action action = () => container.GetInstance<ServiceWithReadOnlyPropertyDependency<ITimeProvider>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                The property named 'Dependency' with type ITimeProvider and declared on type
                PropertyInjectionTests.ServiceWithReadOnlyPropertyDependency<ITimeProvider>
                can't be used for injection, because it has no set method.".TrimInside(),
                action);
        }

        [TestMethod]
        public void InjectingAllProperties_OnTypeWithPrivateSetterProperty_Succeeds()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            // Act
            // Quite bizarre, but this even succeeds in the Silverlight sandbox. I don't know why.
            var service = container.GetInstance<ServiceWithPrivateSetPropertyDependency<ITimeProvider>>();

            // Assert
            Assert.IsNotNull(service.Dependency);
        }

        [TestMethod]
        public void InjectAllProperties_TypeWithStaticProperty_ThrowsExpectedException()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            // Act
            Action action = () => container.GetInstance<ServiceWithStaticPropertyDependency<ITimeProvider>>();

            // Assert
            // An exception should be thrown and static properties should not be ignored. Ignoring them could
            // lead to a fragile configuration, because when a IPropertySelectionBehavior implementation
            // returns true for that given property, it would not expect it to be ignored. Take for instance
            // an custom IPropertySelectionBehavior that reacts on some [Inject] attribute to enable property
            // injection. When an application developer decorates a property with [Inject], ignoring that 
            // property when it is static would be a bad thing. On the other hand, it would be as bad as trying
            // to inject into the static property.
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "Property of type ITimeProvider with name 'Dependency' can't be used for injection, because it is static.",
                action);
        }

        [TestMethod]
        public void InjectAllProperties_ServiceWithMultiplePropertiesOfSameType_AlwaysInjectNewTransientType()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Transient);

            // Act
            var service = container.GetInstance<ServiceWithTwoPropertiesOfSameType<ITimeProvider>>();

            // Assert
            Assert.AreNotSame(service.Dependency01, service.Dependency02);
        }

        [TestMethod]
        public void InjectAllProperties_ServiceWithMultiplePropertiesOfSameType_AlwaysInjectSameSingleton()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            // Act
            var service = container.GetInstance<ServiceWithTwoPropertiesOfSameType<ITimeProvider>>();

            // Assert
            Assert.AreSame(service.Dependency01, service.Dependency02);
        }

        [TestMethod]
        public void InjectAllProperties_OnTypeWithLotsOfProperties_InjectsAllProperties()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            // Act
            // This type has more than 15 properties. This allows us to test the recursive behavior of the
            // code that builds the injection delegate. It uses Func<T> delegates for this, but there are only
            // 17 Func delegates (with up to 16 input arguments) so the building has to be stacked recursively.
            var service = container.GetInstance<ServiceWithLotsOfProperties<ITimeProvider>>();

            // Assert
            Assert_ContainsNoUninjectedProperties(service);
        }

        [TestMethod]
        public void InjectAllProperties_OnTypeWithOnePropertyDependency_AddsThatDependencyAsKnownRelationship()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            container.Register<ServiceWithProperty<ITimeProvider>>();

            container.Verify();

            var expectedDependency = container.GetRegistration(typeof(ITimeProvider));

            // Act
            var relationships =
                container.GetRegistration(typeof(ServiceWithProperty<ITimeProvider>))
                .GetRelationships();

            // Assert
            Assert.AreEqual(1, relationships.Length);
            Assert.AreEqual(typeof(ServiceWithProperty<ITimeProvider>), relationships[0].ImplementationType);
            Assert.AreEqual(Lifestyle.Transient, relationships[0].Lifestyle);
            Assert.AreEqual(expectedDependency, relationships[0].Dependency);
        }

        [TestMethod]
        public void InjectAllProperties_OnContainerUncontrolledSingleton_InjectsProperty()
        {
            // Arrange
            var singleton = new ServiceWithProperty<ITimeProvider>();

            Assert.IsNull(singleton.Dependency, "Test setup failed.");

            var container = CreateContainerThatInjectsAllProperties();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            container.RegisterSingleton<ServiceWithProperty<ITimeProvider>>(singleton);

            // Act
            container.GetInstance<ServiceWithProperty<ITimeProvider>>();

            // Assert
            Assert.IsNotNull(singleton.Dependency);
        }

        [TestMethod]
        public void InjectAllProperties_OnContainerUncontrolledSingletonsCollection_InjectsProperty()
        {
            // Arrange
            var singleton = new ServiceWithProperty<ITimeProvider>();

            var container = CreateContainerThatInjectsAllProperties();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            ServiceWithProperty<ITimeProvider>[] services = new[] { singleton };

            container.RegisterCollection<ServiceWithProperty<ITimeProvider>>(services);

            // Act
            container.GetAllInstances<ServiceWithProperty<ITimeProvider>>().ToArray();

            // Assert
            Assert.IsNotNull(singleton.Dependency);
        }
        
        [TestMethod]
        public void InjectAllProperties_OnContainerUncontrolledSingleton_DoesNotInjectPropertiesOfImplementation()
        {
            // Arrange
            var singleton = new ServiceWithProperty<ITimeProvider>();

            Assert.IsNull(singleton.Dependency, "Test setup failed.");

            var container = CreateContainerThatInjectsAllProperties();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            container.RegisterSingleton<IService>(singleton);

            // Act
            container.GetInstance<IService>();

            // Assert
            Assert.IsNull(singleton.Dependency);
        }

        [TestMethod]
        public void InjectAllProperties_OnContainerUncontrolledSingletonsCollection_DoesNotInjectPropertiesOfImplementation()
        {
            // Arrange
            var singleton = new ServiceWithProperty<ITimeProvider>();

            var container = CreateContainerThatInjectsAllProperties();

            container.Register<ITimeProvider, RealTimeProvider>(Lifestyle.Singleton);

            container.RegisterCollection<IService>(new[] { singleton });

            // Act
            container.GetAllInstances<IService>().ToArray();

            // Assert
            Assert.IsNull(singleton.Dependency);
        }

        [TestMethod]
        public void InjectAllProperties_Always_PropertyDependenciesAreAlwaysCreatedBeforeTheTypeInWhichTheyGetInjectedInto()
        {
            // Arrange
            var expectedOrderOfCreation = new List<Type>
            {
                typeof(PropertyDependency),
                typeof(ComponentWithPropertyDependency),
            };

            var actualOrderOfCreation = new List<Type>();

            var container = CreateContainerThatInjectsAllProperties();

            container.RegisterSingleton<Action<object>>(instance => actualOrderOfCreation.Add(instance.GetType()));

            // Act
            container.GetInstance<ComponentWithPropertyDependency>();

            // Assert
            Assert.IsTrue(
                expectedOrderOfCreation.SequenceEqual(actualOrderOfCreation),
                "Types were expected to be created in the following order: {0}, " +
                "but they actually were created in the order: {1}. " +
                "Creation of property dependencies -before- the actual type is important, because this " +
                "allows them to be correctly disposed when disposing is done in the opposite order of " + 
                "creation.",
                string.Join(", ", expectedOrderOfCreation.Select(type => type.ToFriendlyName())),
                string.Join(", ", actualOrderOfCreation.Select(type => type.ToFriendlyName())));
        }
        
        [TestMethod]
        public void GetInstance_InjectingPropertyWithConditionalRegistration_UsesTheExpectedPredicateContext()
        {
            // Arrange
            PredicateContext context = null;

            var container = CreateContainerThatInjectsAllProperties();

            container.RegisterConditional<ITimeProvider, RealTimeProvider>(c =>
            {
                context = c;

                Assert.AreSame(c.ServiceType, typeof(ITimeProvider));
                Assert.AreSame(c.ImplementationType, typeof(RealTimeProvider));
                Assert.IsNotNull(c.Consumer, "c.Consumer is null");
                Assert.AreSame(context.Consumer.ImplementationType, typeof(ServiceWithProperty<ITimeProvider>));
                Assert.AreEqual(context.Consumer.Target.Name, "Dependency");
                Assert.AreSame(context.Consumer.Target.TargetType, typeof(ITimeProvider));

                return true;
            });

            container.Register<ServiceWithProperty<ITimeProvider>>();

            // Act
            var service = container.GetInstance<ServiceWithProperty<ITimeProvider>>();

            // Assert
            Assert.IsNotNull(service.Dependency);
        }

        private static Container CreateContainerThatInjectsAllProperties()
        {
            var container = ContainerFactory.New();

            Predicate<PropertyInfo> allExceptPropertiesDeclaredOnRealTimeProvider =
                prop => 
                    prop.DeclaringType != typeof(RealTimeProvider) && 
                    prop.DeclaringType != typeof(Container) &&
                    prop.DeclaringType != typeof(Delegate);

            container.Options.PropertySelectionBehavior = 
                new PredicatePropertySelectionBehavior(allExceptPropertiesDeclaredOnRealTimeProvider);

            return container;
        }

        private static void Assert_ContainsNoUninjectedProperties(object instance)
        {
            var uninjectedProperties =
                from property in instance.GetType().GetProperties()
                let value = property.GetValue(instance, null)
                where value == null
                select property.Name;

            Assert.IsFalse(uninjectedProperties.Any(),
                "Properties " + string.Join(" + ", uninjectedProperties) + " were not injected.");
        }

        public class ServiceWithProperty<TDependency> : IService
        {
            public TDependency Dependency { get; set; }
        }

        public class BaseClassServiceWithProperty<TDependency> : IService
        {
            public TDependency BaseClassDependency { get; set; }
        }
        
        public class SubClassServiceWithProperty<TDependency> : BaseClassServiceWithProperty<TDependency>
        {
            public TDependency Dependency { get; set; }
        }

        public class ServiceWithReadOnlyPropertyDependency<TDependency>
        {
            public TDependency Dependency => default(TDependency);
        }

        public class ServiceWithPrivateSetPropertyDependency<TDependency>
        {
            // NOTE: 'private set' is required for tests to work.
            public TDependency Dependency { get; private set; }
        }

#pragma warning disable RCS1102 // Mark class as static.
        public class ServiceWithStaticPropertyDependency<TDependency>
        {
            public static TDependency Dependency { get; set; }
        }
#pragma warning restore RCS1102 // Mark class as static.

        public class ServiceWithTwoPropertiesOfSameType<TDependency>
        {
            public TDependency Dependency01 { get; set; }

            public TDependency Dependency02 { get; set; }
        }

        public class ServiceWithLotsOfProperties<TDependency>
        {
            public TDependency Dependency01 { get; set; }

            public TDependency Dependency02 { get; set; }

            public TDependency Dependency03 { get; set; }

            public TDependency Dependency04 { get; set; }

            public TDependency Dependency05 { get; set; }

            public TDependency Dependency06 { get; set; }

            public TDependency Dependency07 { get; set; }

            public TDependency Dependency08 { get; set; }

            public TDependency Dependency09 { get; set; }

            public TDependency Dependency10 { get; set; }

            public TDependency Dependency11 { get; set; }

            public TDependency Dependency12 { get; set; }

            public TDependency Dependency13 { get; set; }

            public TDependency Dependency14 { get; set; }

            public TDependency Dependency15 { get; set; }

            public TDependency Dependency16 { get; set; }

            public TDependency Dependency17 { get; set; }

            public TDependency Dependency18 { get; set; }

            public TDependency Dependency19 { get; set; }

            public TDependency Dependency20 { get; set; }
        }
        
        public class ComponentWithPropertyDependency
        {
            public ComponentWithPropertyDependency(Action<object> creationCallback)
            {
                creationCallback(this);
            }

            public PropertyDependency Dependency { get; set; }
        }

        public class PropertyDependency
        {
            public PropertyDependency(Action<object> creationCallback)
            {
                creationCallback(this);
            }
        }
        
        private class PrivateServiceWithPrivateSetPropertyDependency<TDependency>
        {
            // NOTE: 'private set' is required.
            internal TDependency Dependency { get; private set; }
        }

        private class PredicatePropertySelectionBehavior : IPropertySelectionBehavior
        {
            private readonly Predicate<PropertyInfo> selector;

            public PredicatePropertySelectionBehavior(Predicate<PropertyInfo> selector)
            {
                this.selector = selector;
            }

            public bool SelectProperty(Type type, PropertyInfo property) => this.selector(property);
        }
    }
}