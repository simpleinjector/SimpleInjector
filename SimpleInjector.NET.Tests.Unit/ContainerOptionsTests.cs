namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;
    using SimpleInjector.Tests.Unit.Extensions;

    [TestClass]
    public class ContainerOptionsTests
    {
        [TestMethod]
        public void ContainerConstructor_SuppliedWithNullContainerOptionsArgument_ThrowsException()
        {
            // Arrange
            ContainerOptions invalidOptions = null;

            // Act
            Action action = () => new Container(invalidOptions);

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void AllowOverridingRegistrations_WhenNotSet_IsFalse()
        {
            // Arrange
            var options = new ContainerOptions();

            // Assert
            Assert.IsFalse(options.AllowOverridingRegistrations,
                "The default value must be false, because this is the behavior users will expect.");
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToFalse_ContainerDoesNotAllowOverringRegistrations()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

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
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

            container.RegisterAll(typeof(ILogger), new[] { typeof(NullLogger) });

            // Act
            Action action = () => container.RegisterAll(typeof(ILogger), new[] { typeof(NullLogger) });

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToFalse_ContainerDoesNotAllowOverridingRegistrationOfCollections()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

            container.RegisterAll(typeof(IEventHandler<ClassEvent>), new[] { typeof(NonGenericEventHandler) });

            // Act
            Action action = () => container.RegisterAll(typeof(IEventHandler<ClassEvent>), new[] 
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
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

            container.RegisterAll(typeof(IEventHandler<ClassEvent>), new[] { typeof(NonGenericEventHandler) });

            // Act
            container.RegisterAll(typeof(IEventHandler<AuditableEvent>), new[] { typeof(AuditableEventEventHandler) });

            // Assert
            AssertThat.IsInstanceOfType(typeof(NonGenericEventHandler), container.GetAllInstances<IEventHandler<ClassEvent>>().Single());

            AssertThat.IsInstanceOfType(typeof(AuditableEventEventHandler), container.GetAllInstances<IEventHandler<AuditableEvent>>().Single());
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToTrue_ContainerAllowsOverridingRegistrationOfCollections()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = true
            });

            container.RegisterAll(typeof(IEventHandler<ClassEvent>), new[] { typeof(NonGenericEventHandler) });

            // Act
            container.RegisterAll(typeof(IEventHandler<ClassEvent>), new[] 
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
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

            container.RegisterAll(typeof(IEventHandler<>), new[] { typeof(NonGenericEventHandler) });

            // Act
            Action action = () => container.RegisterAll(typeof(IEventHandler<>), new[] { typeof(AuditableEventEventHandler) });

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToTrue_ContainerAllowsDuplicateRegistrationsForAnOpenGenericType()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = true
            });

            container.RegisterAll(typeof(IEventHandler<>), new[] { typeof(AuditableEventEventHandlerWithUnknown<int>) });

            // Act
            container.RegisterAll(typeof(IEventHandler<>), new[] { typeof(AuditableEventEventHandler) });

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
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

            container.RegisterAll(typeof(IEventHandler<>), new[] { typeof(NonGenericEventHandler) });

            // Act
            container.RegisterAll(typeof(IValidate<>), new[] { typeof(NullValidator<int>) });
        }
        
        [TestMethod]
        public void AllowOverridingRegistrations_SetToTrue_ContainerReplacesAppendedRegistrationsAsWell()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = true
            });

            container.RegisterAll(typeof(IEventHandler<>), new[] { typeof(AuditableEventEventHandlerWithUnknown<int>) });

            container.AppendToCollection(typeof(IEventHandler<AuditableEvent>), typeof(NewConstraintEventHandler<AuditableEvent>));

            // Act
            container.RegisterAll(typeof(IEventHandler<>), new[] { typeof(AuditableEventEventHandler) });

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
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

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
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = true
            });

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
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

            container.RegisterAll<IUserRepository>(new SqlUserRepository());

            // Act
            Action action = () => container.RegisterAll<IUserRepository>(new InMemoryUserRepository());

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToTrue_ContainerDoesAllowOverringCollections()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = true
            });

            container.RegisterAll<IUserRepository>(new SqlUserRepository());

            // Act
            container.RegisterAll<IUserRepository>(new InMemoryUserRepository());

            // Assert
            var instance = container.GetAllInstances<IUserRepository>().Single();
            AssertThat.IsInstanceOfType(typeof(InMemoryUserRepository), instance);
        }

        // NOTE: There was a bug in the framework. The container did not self-register when the overloaded
        // constructor with the ContainerOptions was used. This test proves this bug.
        [TestMethod]
        public void ContainerWithOptions_ResolvingATypeThatDependsOnTheContainer_ContainerInjectsItself()
        {
            // Arrange
            var container = new Container(new ContainerOptions());

            // Act
            var instance = container.GetInstance<ClassWithContainerAsDependency>();

            // Assert
            Assert.AreEqual(container, instance.Container);
        }

        [TestMethod]
        public void ContainerWithOptions_SuppliedWithAnInstanceThatAlreadyBelongsToAnotherContainer_ThrowsExpectedException()
        {
            // Arrange
            var options = new ContainerOptions();

            var container1 = new Container(options);

            try
            {
                // Act
                new Container(options);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.ExceptionMessageContains(
                    "supplied ContainerOptions instance belongs to another Container instance.", ex);
            }
        }

        [TestMethod]
        public void ConstructorResolutionBehavior_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = new ContainerOptions();

            // Act
            Action action = () => options.ConstructorResolutionBehavior = null;

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void ConstructorResolutionBehavior_ChangedBeforeAnyRegistrations_ChangesThePropertyToTheSetInstance()
        {
            // Arrange
            var expectedBehavior = new AlternativeConstructorResolutionBehavior();

            var options = new ContainerOptions();

            var container = new Container(options);

            // Act
            options.ConstructorResolutionBehavior = expectedBehavior;

            // Assert
            Assert.IsTrue(object.ReferenceEquals(expectedBehavior, options.ConstructorResolutionBehavior),
                "The set_ConstructorResolutionBehavior did not work.");
        }

        [TestMethod]
        public void ConstructorResolutionBehavior_ChangedAfterFirstRegistration_Fails()
        {
            // Arrange
            var expectedBehavior = new AlternativeConstructorResolutionBehavior();

            var options = new ContainerOptions();

            var container = new Container(options);

            container.RegisterSingle<object>("The first registration.");

            try
            {
                // Act
                options.ConstructorResolutionBehavior = expectedBehavior;

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains(
                    "ConstructorResolutionBehavior property cannot be changed after the first registration",
                    ex);
            }
        }

        [TestMethod]
        public void ConstructorResolutionBehavior_ChangedAfterFirstCallToGetInstance_Fails()
        {
            // Arrange
            var expectedBehavior = new AlternativeConstructorResolutionBehavior();

            var options = new ContainerOptions();

            var container = new Container(options);

            // Request a concrete instance that can be created by the container, even without any registrations.
            container.GetInstance<ClassWithContainerAsDependency>();

            try
            {
                // Act
                options.ConstructorResolutionBehavior = expectedBehavior;

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.StringContains(
                    "ConstructorResolutionBehavior property cannot be changed after the first registration",
                    ex.Message);
            }
        }

        [TestMethod]
        public void ConstructorInjectionBehavior_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = new ContainerOptions();

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

            var options = new ContainerOptions();

            var container = new Container(options);

            // Act
            options.ConstructorInjectionBehavior = expectedBehavior;

            // Assert
            Assert.IsTrue(object.ReferenceEquals(expectedBehavior, options.ConstructorInjectionBehavior),
                "The set_ConstructorInjectionBehavior did not work.");
        }

        [TestMethod]
        public void ConstructorInjectionBehavior_ChangedAfterFirstRegistration_Fails()
        {
            // Arrange
            var expectedBehavior = new AlternativeConstructorInjectionBehavior();

            var options = new ContainerOptions();

            var container = new Container(options);

            container.RegisterSingle<object>("The first registration.");

            try
            {
                // Act
                options.ConstructorInjectionBehavior = expectedBehavior;

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains(
                    "ConstructorInjectionBehavior property cannot be changed after the first registration",
                    ex);
            }
        }

        [TestMethod]
        public void PropertyInjectionBehavior_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = new ContainerOptions();

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

            var options = new ContainerOptions();

            var container = new Container(options);

            // Act
            options.PropertySelectionBehavior = expectedBehavior;

            // Assert
            Assert.IsTrue(object.ReferenceEquals(expectedBehavior, options.PropertySelectionBehavior),
                "The set_PropertySelectionBehavior did not work.");
        }

        [TestMethod]
        public void PropertyInjectionBehavior_ChangedAfterFirstRegistration_Fails()
        {
            // Arrange
            var expectedBehavior = new AlternativePropertySelectionBehavior();

            var options = new ContainerOptions();

            var container = new Container(options);

            container.RegisterSingle<object>("The first registration.");

            try
            {
                // Act
                options.PropertySelectionBehavior = expectedBehavior;

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains(
                    "PropertySelectionBehavior property cannot be changed after the first registration",
                    ex);
            }
        }

        [TestMethod]
        public void BuildParameterExpression_CalledOnConstructorInjectionBehaviorWhenOptionsIsNotPartOfAContainer_ThrowsExpectedException()
        {
            // Arrange
            var options = new ContainerOptions();

            var parameter = 
                typeof(ClassWithContainerAsDependency).GetConstructors().First().GetParameters().First();
            
            // Act
            Action action = () => options.ConstructorInjectionBehavior.BuildParameterExpression(parameter);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                The ContainerOptions instance for this ConstructorInjectionBehavior is not part of a Container
                instance. Please make sure the ContainerOptions instance is supplied as argument to the 
                constructor of a Container.".TrimInside(), 
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
            var options = new ContainerOptions();

            // Act
            var description = options.DebuggerDisplayDescription;

            // Assert
            Assert.AreEqual("Default Configuration", description);
        }

        [TestMethod]
        public void DebuggerDisplayDescription_WithAllowOverridingRegistrations_ReturnsExpectedMessage()
        {
            // Arrange
            var options = new ContainerOptions();

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
            var options = new ContainerOptions();

            options.ConstructorResolutionBehavior = new AlternativeConstructorResolutionBehavior();

            // Act
            var description = options.DebuggerDisplayDescription;

            // Assert
            Assert.AreEqual("Custom Constructor Resolution", description);
        }

        [TestMethod]
        public void DebuggerDisplayDescription_WithOverriddenConstructorVerificationBehavior_ReturnsExpectedMessage()
        {
            // Arrange
            var options = new ContainerOptions();

            options.ConstructorVerificationBehavior = new AlternativeConstructorVerificationBehavior();

            // Act
            var description = options.DebuggerDisplayDescription;

            // Assert
            Assert.AreEqual("Custom Constructor Verification", description);
        }

        [TestMethod]
        public void DebuggerDisplayDescription_WithOverriddenConstructorInjectionBehavior_ReturnsExpectedMessage()
        {
            // Arrange
            var options = new ContainerOptions();

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
            var options = new ContainerOptions();

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
            var options = new ContainerOptions();

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
            var options = new ContainerOptions();

            options.AllowOverridingRegistrations = true;
            options.ConstructorResolutionBehavior = new AlternativeConstructorResolutionBehavior();
            options.ConstructorVerificationBehavior = new AlternativeConstructorVerificationBehavior();
            options.ConstructorInjectionBehavior = new AlternativeConstructorInjectionBehavior();
            options.PropertySelectionBehavior = new AlternativePropertySelectionBehavior();
            options.LifestyleSelectionBehavior = new AlternativeLifestyleSelectionBehavior();

            // Act
            var description = options.DebuggerDisplayDescription;

            // Assert
            Assert.AreEqual(@"
                Allows Overriding Registrations,
                Custom Constructor Resolution,
                Custom Constructor Verification,
                Custom Constructor Injection,
                Custom Property Selection,
                Custom Lifestyle Selection
                ".TrimInside(), description);
        }

        [TestMethod]
        public void ConstructorVerificationBehavior_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = new ContainerOptions();

            // Act
            Action action = () => options.ConstructorVerificationBehavior = null;

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }
        
        [TestMethod]
        public void PropertySelectionBehavior_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = new ContainerOptions();

            // Act
            Action action = () => options.PropertySelectionBehavior = null;

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void LifestyleSelectionBehavior_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = new ContainerOptions();

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
        }

        private sealed class AlternativeConstructorVerificationBehavior : IConstructorVerificationBehavior
        {
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