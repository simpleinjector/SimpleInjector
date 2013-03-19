namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    [TestClass]
    public class PropertyInjectionTests
    {
        [TestMethod]
        public void InjectingAllProperties_OnTypeWithPublicWritableProperty_InjectsPropertyDependency()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.RegisterSingle<ITimeProvider, RealTimeProvider>();

            // Act
            var service = container.GetInstance<ServiceWithProperty<ITimeProvider>>();

            // Assert
            Assert.IsNotNull(service.Dependency);
        }

        [TestMethod]
        public void InjectingAllProperties_OnTypeWithPublicWritablePropertyButRegistrationMissing_ThrowsException()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            // Act
            Action action = () => container.GetInstance<ServiceWithProperty<ITimeProvider>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "No registration for type ITimeProvider could be found", 
                action);
        }

        public class ServiceWithProperty<TDependency>
        {
            public TDependency Dependency { get; set; }
        }

        [TestMethod]
        public void InjectingAllProperties_OnTypeWithReadOnlyProperty_ThrowsExpectedException()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.RegisterSingle<ITimeProvider, RealTimeProvider>();

            // Act
            Action action = () => container.GetInstance<ServiceWithReadOnlyPropertyDependency<ITimeProvider>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(@"
                Property of type ITimeProvider with name 'Dependency' can't be injected, because it has no 
                set method.".TrimInside(),
                action);
        }

        public class ServiceWithReadOnlyPropertyDependency<TDependency>
        {
            public TDependency Dependency
            {
                get { return default(TDependency); }
            }
        }

        [TestMethod]
        public void InjectingAllProperties_OnTypeWithPrivateSetterProperty_Succeeds()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.RegisterSingle<ITimeProvider, RealTimeProvider>();

            // Act
            // Quite bizarre, but this even succeeds in the Silverlight sandbox. I don't know why.
            var service = container.GetInstance<ServiceWithPrivateSetPropertyDependency<ITimeProvider>>();

            // Assert
            Assert.IsNotNull(service.Dependency);
        }

        public class ServiceWithPrivateSetPropertyDependency<TDependency>
        {
            public TDependency Dependency { get; private set; }
        }

        [TestMethod]
        public void InjectAllProperties_TypeWithStaticProperty_ThrowsExpectedException()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.RegisterSingle<ITimeProvider, RealTimeProvider>();

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
                "Property of type ITimeProvider with name 'Dependency' can't be injected, because it is static.",
                action);
        }

        public class ServiceWithStaticPropertyDependency<TDependency>
        {
            public static TDependency Dependency { get; set; }
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
            Assert.IsFalse(object.ReferenceEquals(service.Dependency01, service.Dependency02));
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
            Assert.IsTrue(object.ReferenceEquals(service.Dependency01, service.Dependency02));
        }

        public class ServiceWithTwoPropertiesOfSameType<TDependency>
        {
            public TDependency Dependency01 { get; set; }

            public TDependency Dependency02 { get; set; }
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

#if !SILVERLIGHT
        [TestMethod]
        public void InjectingAllProperties_OnPrivateTypeWithPrivateSetterProperty_Succeeds()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.RegisterSingle<ITimeProvider, RealTimeProvider>();

            // Act
            var service = container.GetInstance<PrivateServiceWithPrivateSetPropertyDependency<ITimeProvider>>();

            // Assert
            Assert.IsNotNull(service.Dependency);
        }
#else // SILVERLIGHT
        [TestMethod]
        public void InjectingAllProperties_OnPrivateTypeWithPrivateSetterPropertyInSilverlight_FailsWithDescriptiveMessage()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.RegisterSingle<ITimeProvider, RealTimeProvider>();

            try
            {
                // Act
                container.GetInstance<PrivateServiceWithPrivateSetPropertyDependency<ITimeProvider>>();
            }
            catch (Exception ex)
            {
                AssertThat.ExceptionMessageContains(@"
                    The security restrictions of your application's sandbox do not permit the injection of 
                    one of its properties.".TrimInside(), ex);
            }
        }
#endif // SILVERLIGHT

        private class PrivateServiceWithPrivateSetPropertyDependency<TDependency>
        {
            internal TDependency Dependency { get; private set; }
        }

#if DEBUG
        [TestMethod]
        public void InjectAllProperties_OnTypeWithOnePropertyDependency_AddsThatDependencyAsKnownRelationship()
        {
            // Arrange
            var container = CreateContainerThatInjectsAllProperties();

            container.RegisterSingle<ITimeProvider, RealTimeProvider>();

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
#endif

        private static Container CreateContainerThatInjectsAllProperties()
        {
            var container = ContainerFactory.New();

            Predicate<PropertyInfo> allExceptPropertiesDeclaredOnRealTimeProvider =
                prop => prop.DeclaringType != typeof(RealTimeProvider);

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
                select property;

            Assert.IsFalse(uninjectedProperties.Any(),
                "Property: " + uninjectedProperties.FirstOrDefault() + " was not injected.");
        }

        private class PredicatePropertySelectionBehavior : IPropertySelectionBehavior
        {
            private readonly Predicate<PropertyInfo> propertySelector;

            public PredicatePropertySelectionBehavior(Predicate<PropertyInfo> propertySelector)
            {
                this.propertySelector = propertySelector;
            }

            public bool SelectProperty(Type serviceType, PropertyInfo property)
            {
                return this.propertySelector(property);
            }
        }
    }
}