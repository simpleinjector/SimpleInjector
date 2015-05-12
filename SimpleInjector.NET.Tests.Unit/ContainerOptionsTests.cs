namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;
    using SimpleInjector.Extensions.LifetimeScoping;
    using SimpleInjector.Tests.Unit.Extensions;

    [TestClass]
    public class ContainerOptionsTests
    {
        [TestMethod]
        public void AllowOverridingRegistrations_WhenNotSet_IsFalse()
        {
            // Arrange
            ContainerOptions options = GetContainerOptions();

            // Assert
            Assert.IsFalse(options.AllowOverridingRegistrations,
                "The default value must be false, because this is the behavior users will expect.");
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToFalse_ContainerDoesNotAllowOverringRegistrations()
        {
            // Arrange
            var container = new Container();
            container.Options.AllowOverridingRegistrations = false;
            
            container.Register<IUserRepository, SqlUserRepository>();

            // Act
            Action action = () => container.Register<IUserRepository, InMemoryUserRepository>();

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToFalse_ContainerDoesNotAllowOverridingRegistrationOfNonGenericCollections()
        {
            // Arrange
            var container = new Container();
            container.Options.AllowOverridingRegistrations = false;

            container.RegisterCollection(typeof(ILogger), new[] { typeof(NullLogger) });

            // Act
            Action action = () => container.RegisterCollection(typeof(ILogger), new[] { typeof(NullLogger) });

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToFalse_ContainerDoesNotAllowOverridingRegistrationOfCollections()
        {
            // Arrange
            var container = new Container();
            container.Options.AllowOverridingRegistrations = false;

            container.RegisterCollection(typeof(IEventHandler<ClassEvent>), new[] { typeof(NonGenericEventHandler) });

            // Act
            Action action = () => container.RegisterCollection(typeof(IEventHandler<ClassEvent>), new[] 
            {
                typeof(ClassConstraintEventHandler<ClassEvent>) 
            });

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToFalse_ContainerAllowsRegistrationOfUnrelatedCollections()
        {
            // Arrange
            var container = new Container();
            container.Options.AllowOverridingRegistrations = false;

            container.RegisterCollection(typeof(IEventHandler<ClassEvent>), new[] { typeof(NonGenericEventHandler) });

            // Act
            container.RegisterCollection(typeof(IEventHandler<AuditableEvent>), new[] { typeof(AuditableEventEventHandler) });

            // Assert
            AssertThat.IsInstanceOfType(typeof(NonGenericEventHandler), container.GetAllInstances<IEventHandler<ClassEvent>>().Single());

            AssertThat.IsInstanceOfType(typeof(AuditableEventEventHandler), container.GetAllInstances<IEventHandler<AuditableEvent>>().Single());
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToTrue_ContainerAllowsOverridingRegistrationOfCollections()
        {
            // Arrange
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;

            container.RegisterCollection(typeof(IEventHandler<ClassEvent>), new[] { typeof(NonGenericEventHandler) });

            // Act
            container.RegisterCollection(typeof(IEventHandler<ClassEvent>), new[] 
            {
                typeof(ClassConstraintEventHandler<ClassEvent>) 
            });

            var handlers = container.GetAllInstances<IEventHandler<ClassEvent>>().Select(h => h.GetType()).ToArray();

            // Assert
            Assert.IsTrue(handlers.Length == 1 && handlers[0] == typeof(ClassConstraintEventHandler<ClassEvent>),
                "Actual: " + handlers.ToFriendlyNamesText());
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToFalse_ContainerDoesNotAllowDuplicateRegistrationsForAnOpenGenericType()
        {
            // Arrange
            var container = new Container();
            container.Options.AllowOverridingRegistrations = false;

            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(NonGenericEventHandler) });

            // Act
            Action action = () => container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(AuditableEventEventHandler) });

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToTrue_ContainerAllowsDuplicateRegistrationsForAnOpenGenericType()
        {
            // Arrange
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;

            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(AuditableEventEventHandlerWithUnknown<int>) });

            // Act
            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(AuditableEventEventHandler) });

            // Assert
            var handlers = container.GetAllInstances<IEventHandler<AuditableEvent>>().Select(h => h.GetType()).ToArray();

            // Assert
            Assert.IsTrue(handlers.Length == 1 && handlers[0] == typeof(AuditableEventEventHandler),
                "Actual: " + handlers.ToFriendlyNamesText());
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToFalse_ContainerAllowsRegistratingUnrelatedOpenGenericCollections()
        {
            // Arrange
            var container = new Container();
            container.Options.AllowOverridingRegistrations = false;

            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(NonGenericEventHandler) });

            // Act
            container.RegisterCollection(typeof(IValidate<>), new[] { typeof(NullValidator<int>) });
        }
        
        [TestMethod]
        public void AllowOverridingRegistrations_SetToTrue_ContainerReplacesAppendedRegistrationsAsWell()
        {
            // Arrange
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;

            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(AuditableEventEventHandlerWithUnknown<int>) });

            container.AppendToCollection(typeof(IEventHandler<AuditableEvent>), typeof(NewConstraintEventHandler<AuditableEvent>));

            // Act
            container.RegisterCollection(typeof(IEventHandler<>), new[] { typeof(AuditableEventEventHandler) });

            // Assert
            var handlers = container.GetAllInstances<IEventHandler<AuditableEvent>>().Select(h => h.GetType()).ToArray();

            // Assert
            Assert.IsTrue(handlers.Length == 1 && handlers[0] == typeof(AuditableEventEventHandler),
                "Actual: " + handlers.ToFriendlyNamesText());
        }
        
        [TestMethod]
        public void AllowOverridingRegistrations_SetToFalse_ContainerThrowsExpectedExceptionMessage()
        {
            // Arrange
            var container = new Container();
            container.Options.AllowOverridingRegistrations = false;

            container.Register<IUserRepository, SqlUserRepository>();

            try
            {
                // Act
                container.Register<IUserRepository, InMemoryUserRepository>();
            }
            catch (InvalidOperationException ex)
            {
                // Assert
                AssertThat.ExceptionMessageContains("Container.Options.AllowOverridingRegistrations", ex);
            }
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToTrue_ContainerDoesAllowOverringRegistrations()
        {
            // Arrange
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;

            container.Register<IUserRepository, SqlUserRepository>();

            // Act
            container.Register<IUserRepository, InMemoryUserRepository>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(InMemoryUserRepository), container.GetInstance<IUserRepository>(), "The registration was not overridden properly.");
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToFalse_ContainerDoesNotAllowOverringCollections()
        {
            // Arrange
            var container = new Container();
            container.Options.AllowOverridingRegistrations = false;

            container.RegisterCollection<IUserRepository>(new SqlUserRepository());

            // Act
            Action action = () => container.RegisterCollection<IUserRepository>(new InMemoryUserRepository());

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToTrue_ContainerDoesAllowOverringCollections()
        {
            // Arrange
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;

            container.RegisterCollection<IUserRepository>(new SqlUserRepository());

            // Act
            container.RegisterCollection<IUserRepository>(new InMemoryUserRepository());

            // Assert
            var instance = container.GetAllInstances<IUserRepository>().Single();
            AssertThat.IsInstanceOfType(typeof(InMemoryUserRepository), instance);
        }

        [TestMethod]
        public void ConstructorResolutionBehavior_ChangedBeforeAnyRegistrations_ChangesThePropertyToTheSetInstance()
        {
            // Arrange
            var expectedBehavior = new AlternativeConstructorResolutionBehavior();

            var container = new Container();

            // Act
            container.Options.ConstructorResolutionBehavior = expectedBehavior;

            // Assert
            Assert.IsTrue(object.ReferenceEquals(expectedBehavior, container.Options.ConstructorResolutionBehavior),
                "The set_ConstructorResolutionBehavior did not work.");
        }

        [TestMethod]
        public void ConstructorResolutionBehavior_ChangedAfterFirstRegistration_Fails()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<object>("The first registration.");

            // Act
            Action action = () => container.Options.ConstructorResolutionBehavior = 
                new AlternativeConstructorResolutionBehavior();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "ConstructorResolutionBehavior property cannot be changed after the first registration",
                action);
        }

        [TestMethod]
        public void ConstructorResolutionBehavior_ChangedAfterFirstCallToGetInstance_Fails()
        {
            // Arrange
            var expectedBehavior = new AlternativeConstructorResolutionBehavior();

            var container = new Container();

            // Request a concrete instance that can be created by the container, even without any registrations.
            container.GetInstance<ClassWithContainerAsDependency>();

            // Act
            Action action = () => container.Options.ConstructorResolutionBehavior = expectedBehavior;

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "ConstructorResolutionBehavior property cannot be changed after the first registration",
                action);
        }

        [TestMethod]
        public void ConstructorInjectionBehavior_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = GetContainerOptions();

            // Act
            Action action = () => options.ConstructorInjectionBehavior = null;

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void ConstructorInjectionBehavior_ChangedBeforeAnyRegistrations_ChangesThePropertyToTheSetInstance()
        {
            // Arrange
            var expectedBehavior = new AlternativeConstructorInjectionBehavior();

            var container = new Container();

            // Act
            container.Options.ConstructorInjectionBehavior = expectedBehavior;

            // Assert
            Assert.IsTrue(object.ReferenceEquals(expectedBehavior, container.Options.ConstructorInjectionBehavior),
                "The set_ConstructorInjectionBehavior did not work.");
        }

        [TestMethod]
        public void ConstructorInjectionBehavior_ChangedAfterFirstRegistration_Fails()
        {
            // Arrange
            var expectedBehavior = new AlternativeConstructorInjectionBehavior();

            var container = new Container();

            container.RegisterSingle<object>("The first registration.");

            // Act
            Action action = () => container.Options.ConstructorInjectionBehavior = expectedBehavior;

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "ConstructorInjectionBehavior property cannot be changed after the first registration",
                action);
        }

        [TestMethod]
        public void PropertyInjectionBehavior_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = GetContainerOptions();

            // Act
            Action action = () => options.PropertySelectionBehavior = null;

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void PropertyInjectionBehavior_ChangedBeforeAnyRegistrations_ChangesThePropertyToTheSetInstance()
        {
            // Arrange
            var expectedBehavior = new AlternativePropertySelectionBehavior();

            var options = GetContainerOptions();

            // Act
            options.PropertySelectionBehavior = expectedBehavior;

            // Assert
            Assert.AreSame(expectedBehavior, options.PropertySelectionBehavior,
                "The set_PropertySelectionBehavior did not work.");
        }

        [TestMethod]
        public void PropertyInjectionBehavior_ChangedAfterFirstRegistration_Fails()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<object>("The first registration.");

            // Act
            Action action = 
                () => container.Options.PropertySelectionBehavior = new AlternativePropertySelectionBehavior();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "PropertySelectionBehavior property cannot be changed after the first registration",
                action);
        }

        [TestMethod]
        public void DefaultScopedLifestyle_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = GetContainerOptions();

            // Act
            Action action = () => options.DefaultScopedLifestyle = null;

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void DefaultScopedLifestyle_ChangedBeforeAnyRegistrations_ChangesThePropertyToTheSetInstance()
        {
            // Arrange
            var expectedLifestyle = new LifetimeScopeLifestyle();

            var options = GetContainerOptions();

            // Act
            options.DefaultScopedLifestyle = expectedLifestyle;

            // Assert
            Assert.AreSame(expectedLifestyle, options.DefaultScopedLifestyle,
                "The set_DefaultScopedLifestyle did not work.");
        }

        [TestMethod]
        public void DefaultScopedLifestyle_ChangedMultipleTimesBeforeAnyRegistrations_ChangesThePropertyToTheSetInstance()
        {
            // Arrange
            var expectedLifestyle = new LifetimeScopeLifestyle();

            var options = GetContainerOptions();

            // Act
            options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();
            options.DefaultScopedLifestyle = expectedLifestyle;

            // Assert
            Assert.AreSame(expectedLifestyle, options.DefaultScopedLifestyle,
                "The set_DefaultScopedLifestyle did not work.");
        }

        [TestMethod]
        public void DefaultScopedLifestyle_ChangedAfterFirstRegistration_Fails()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingle<object>("The first registration.");

            // Act
            Action action = () => container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "DefaultScopedLifestyle property cannot be changed after the first registration",
                action);
        }

        [TestMethod]
        public void ContainerOptions_Is_DecoratedWithADebuggerDisplayAttribute()
        {
            // Arrange
            Type containerOptionsType = typeof(ContainerOptions);
            
            // Act
            var debuggerDisplayAttributes = 
                containerOptionsType.GetCustomAttributes(typeof(DebuggerDisplayAttribute), false);
            
            // Assert
            Assert.AreEqual(1, debuggerDisplayAttributes.Length);
        }

        [TestMethod]
        public void ContainerOptions_DebuggerDisplayAttribute_ReferencesExpectedProperty()
        {
            // Arrange
            var debuggerDisplayAttribute =
                typeof(ContainerOptions).GetCustomAttributes(typeof(DebuggerDisplayAttribute), false)
                .Single() as DebuggerDisplayAttribute;

            var debuggerDisplayDescriptionProperty = 
                GetProperty<ContainerOptions>(options => options.DebuggerDisplayDescription);

            // Act
            string value = debuggerDisplayAttribute.Value;

            // Assert
            Assert.IsTrue(value.Contains(debuggerDisplayDescriptionProperty.Name), "actual: " + value);
        }

        [TestMethod]
        public void DebuggerDisplayDescription_WithDefaultConfiguration_ReturnsExpectedMessage()
        {
            // Arrange
            var options = GetContainerOptions();

            // Act
            var description = options.DebuggerDisplayDescription;

            // Assert
            Assert.AreEqual("Default Configuration", description);
        }

        [TestMethod]
        public void DebuggerDisplayDescription_WithAllowOverridingRegistrations_ReturnsExpectedMessage()
        {
            // Arrange
            var options = GetContainerOptions();

            options.AllowOverridingRegistrations = true;

            // Act
            var description = options.DebuggerDisplayDescription;

            // Assert
            Assert.AreEqual("Allows Overriding Registrations", description);
        }

        [TestMethod]
        public void DebuggerDisplayDescription_WithOverriddenConstructorResolutionBehavior_ReturnsExpectedMessage()
        {
            // Arrange
            var options = GetContainerOptions();

            options.ConstructorResolutionBehavior = new AlternativeConstructorResolutionBehavior();

            // Act
            var description = options.DebuggerDisplayDescription;

            // Assert
            Assert.AreEqual("Custom Constructor Resolution", description);
        }

        [TestMethod]
        public void DebuggerDisplayDescription_WithOverriddenConstructorInjectionBehavior_ReturnsExpectedMessage()
        {
            // Arrange
            var options = GetContainerOptions();

            options.ConstructorInjectionBehavior = new AlternativeConstructorInjectionBehavior();

            // Act
            var description = options.DebuggerDisplayDescription;

            // Assert
            Assert.AreEqual("Custom Constructor Injection", description);
        }

        [TestMethod]
        public void DebuggerDisplayDescription_WithOverriddenPropertySelectionBehavior_ReturnsExpectedMessage()
        {
            // Arrange
            var options = GetContainerOptions();

            options.PropertySelectionBehavior = new AlternativePropertySelectionBehavior();

            // Act
            var description = options.DebuggerDisplayDescription;

            // Assert
            Assert.AreEqual("Custom Property Selection", description);
        }

        [TestMethod]
        public void DebuggerDisplayDescription_WithOverriddenLifestyleSelectionBehavior_ReturnsExpectedMessage()
        {
            // Arrange
            var options = GetContainerOptions();

            options.LifestyleSelectionBehavior = new AlternativeLifestyleSelectionBehavior();

            // Act
            var description = options.DebuggerDisplayDescription;

            // Assert
            Assert.AreEqual("Custom Lifestyle Selection", description);
        }

        [TestMethod]
        public void DebuggerDisplayDescription_WithAllCustomValues_ReturnsExpectedMessage()
        {
            // Arrange
            var options = GetContainerOptions();

            options.AllowOverridingRegistrations = true;
            options.ConstructorResolutionBehavior = new AlternativeConstructorResolutionBehavior();
            options.ConstructorInjectionBehavior = new AlternativeConstructorInjectionBehavior();
            options.PropertySelectionBehavior = new AlternativePropertySelectionBehavior();
            options.LifestyleSelectionBehavior = new AlternativeLifestyleSelectionBehavior();

            // Act
            var description = options.DebuggerDisplayDescription;

            // Assert
            Assert.AreEqual(@"
                Allows Overriding Registrations,
                Custom Constructor Resolution,
                Custom Constructor Injection,
                Custom Property Selection,
                Custom Lifestyle Selection
                ".TrimInside(), description);
        }

        [TestMethod]
        public void PropertySelectionBehavior_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = GetContainerOptions();

            // Act
            Action action = () => options.PropertySelectionBehavior = null;

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void LifestyleSelectionBehavior_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = GetContainerOptions();

            // Act
            Action action = () => options.LifestyleSelectionBehavior = null;

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        private static MemberInfo GetProperty<T>(Expression<Func<T, object>> propertySelector)
        {
            var body = (MemberExpression)propertySelector.Body;

            return body.Member;
        }

        private static ContainerOptions GetContainerOptions()
        {
            return new Container().Options;
        }

        public sealed class ClassWithContainerAsDependency
        {
            public ClassWithContainerAsDependency(Container container)
            {
                this.Container = container;
            }

            public Container Container { get; private set; }
        }

        private sealed class AlternativeConstructorResolutionBehavior : IConstructorResolutionBehavior
        {
            public ConstructorInfo GetConstructor(Type serviceType, Type implementationType)
            {
                return implementationType.GetConstructors()[0];
            }
        }

        private sealed class AlternativeConstructorInjectionBehavior : IConstructorInjectionBehavior
        {
            public Expression BuildParameterExpression(ParameterInfo parameter)
            {
                throw new NotImplementedException();
            }

            public void Verify(ParameterInfo parameter)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class AlternativePropertySelectionBehavior : IPropertySelectionBehavior
        {
            public bool SelectProperty(Type serviceType, PropertyInfo property)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class AlternativeLifestyleSelectionBehavior : ILifestyleSelectionBehavior
        {
            public Lifestyle SelectLifestyle(Type serviceType, Type implementationType)
            {
                throw new NotImplementedException();
            }
        }
    }
}