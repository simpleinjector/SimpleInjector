namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

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
        public void AllowOverridingRegistrations_SetToTrue_ContainerAllowsOverridingClosedGenericRegistrations()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IGeneric<int>, IntGenericType>();

            container.Options.AllowOverridingRegistrations = true;

            // Replaces the previous registration
            container.Register<IGeneric<int>, GenericType<int>>();

            // Act
            var instance = container.GetInstance<IGeneric<int>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(GenericType<int>), instance);
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToTrue_ContainerAllowsOverridingClosedGenericConditionalRegistrationByNonConditional()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterConditional<IGeneric<int>, IntGenericType>(c => true);

            container.Options.AllowOverridingRegistrations = true;

            // Replaces the previous (conditional) registration
            container.Register<IGeneric<int>, GenericType<int>>();

            // Act
            var instance = container.GetInstance<IGeneric<int>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(GenericType<int>), instance);
        }

        [TestMethod]
        public void Register_OpenGenericRegistrationWithTypeConstraintWithWithNoOverlappingRegistrationsAndAllowOverridingRegistrationsTrue_Succeeds()
        {
            // Arrange
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;

            // Act
            container.Register(typeof(IEventHandler<>), typeof(ClassConstraintEventHandler<>));
        }

        [TestMethod]
        public void Register_OpenGenericRegistrationWithTypeConstraintWithExistingConditionalAndAllowOverridingRegistrationsTrue_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(IEventHandler<>), typeof(ClassConstraintEventHandler<>));
            container.Options.AllowOverridingRegistrations = true;

            // Act
            Action action = () => container.Register(typeof(IEventHandler<>), typeof(StructConstraintEventHandler<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "Your registration is considered conditional, because of its generic type constraints. " +
                "This makes Simple Injector apply it conditionally, based on its type constraints.",
                action);
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
        public void AllowOverridingRegistrations_SetToTrueWhileOverridingRegistrationWithSameImplementation_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(ILogger), typeof(NullLogger));
            container.Options.AllowOverridingRegistrations = true;

            // Act
            container.Register(typeof(ILogger), typeof(NullLogger));
        }

        [TestMethod]
        public void RegisterConditional_AllowOverridingRegistrationsSetTrueAndOverlappingConditionalExists_ThrowsException()
        {
            // Arrange
            var container = new Container();

            container.RegisterConditional(typeof(ILogger), typeof(NullLogger), c => true);
            container.Options.AllowOverridingRegistrations = true;

            // Act
            Action action = () => container.RegisterConditional(typeof(ILogger), typeof(ConsoleLogger), c => true);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<NotSupportedException>(
                "making of conditional registrations is not supported when AllowOverridingRegistrations is set",
                action);
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToTrueWhileOverridingRegistrationWithDifferentGenericImplementation_ResolvesNewType()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(ICommandHandler<>), typeof(NullCommandHandler<>));
            container.Options.AllowOverridingRegistrations = true;

            container.Register(typeof(ICommandHandler<>), typeof(DefaultCommandHandler<>));

            // Act
            var instance = container.GetInstance<ICommandHandler<int>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(DefaultCommandHandler<int>), instance);
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToTrueWhileOverridingRegistrationWithSameGenericImplementation_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(ICommandHandler<>), typeof(NullCommandHandler<>));
            container.Options.AllowOverridingRegistrations = true;

            container.Register(typeof(ICommandHandler<>), typeof(NullCommandHandler<>));

            // Act
            container.GetInstance<ICommandHandler<int>>();
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToFalseWhileOverridingRegistrationWithSameGenericImplementation_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(ICommandHandler<>), typeof(NullCommandHandler<>));
            container.Options.AllowOverridingRegistrations = false;

            // Act
            Action action = () => container.Register(typeof(ICommandHandler<>), typeof(NullCommandHandler<>));

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void AllowOverridingRegistrations_SetToTrueWhileOverridingGenericTypeWithoutConstraints_SuccessfullyReplacesTheOld()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(ICommandHandler<>), typeof(NullCommandHandler<>));
            container.Options.AllowOverridingRegistrations = true;
            container.Register(typeof(ICommandHandler<>), typeof(DefaultCommandHandler<>));

            // Act
            var instance = container.GetInstance<ICommandHandler<int>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(DefaultCommandHandler<int>), instance);
        }

        [TestMethod]
        public void Verify_SingletonRegistrationOverriddenWithExactSameImplementation_DoesNotCauseTornLifestyleError()
        {
            // Arrange
            var container = new Container();

            container.Register(typeof(ILogger), typeof(NullLogger), Lifestyle.Singleton);
            container.Options.AllowOverridingRegistrations = true;

            container.Register(typeof(ILogger), typeof(NullLogger), Lifestyle.Singleton);

            // Act
            container.Verify(VerificationOption.VerifyAndDiagnose);
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

            container.RegisterSingleton<object>("The first registration.");

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
            Action action = () => options.DependencyInjectionBehavior = null;

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void ConstructorInjectionBehavior_ChangedBeforeAnyRegistrations_ChangesThePropertyToTheSetInstance()
        {
            // Arrange
            var expectedBehavior = new AlternativeDependencyInjectionBehavior();

            var container = new Container();

            // Act
            container.Options.DependencyInjectionBehavior = expectedBehavior;

            // Assert
            Assert.IsTrue(object.ReferenceEquals(expectedBehavior, container.Options.DependencyInjectionBehavior),
                "The set_ConstructorInjectionBehavior did not work.");
        }

        [TestMethod]
        public void ConstructorInjectionBehavior_ChangedAfterFirstRegistration_Fails()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingleton<object>("The first registration.");

            // Act
            Action action = () => container.Options.DependencyInjectionBehavior = 
                new AlternativeDependencyInjectionBehavior();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "DependencyInjectionBehavior property cannot be changed after the first registration",
                action);
        }

        [TestMethod]
        public void ConstructorInjectionBehavior_ChangedAfterFirstCollectionRegistration_Fails()
        {
            // Arrange
            var expectedBehavior = new AlternativeDependencyInjectionBehavior();

            var container = new Container();

            container.RegisterCollection<ILogger>(new[] { typeof(NullLogger) });

            // Act
            Action action = () => container.Options.DependencyInjectionBehavior = expectedBehavior;

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "DependencyInjectionBehavior property cannot be changed after the first registration",
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

            container.RegisterSingleton<object>("The first registration.");

            // Act
            Action action = 
                () => container.Options.PropertySelectionBehavior = new AlternativePropertySelectionBehavior();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "PropertySelectionBehavior property cannot be changed after the first registration",
                action);
        }

        [TestMethod]
        public void DefaultLifestyle_ByDefault_ReturnsTheTransientLifestyle()
        {
            // Arrange
            var options = GetContainerOptions();

            // Act
            Lifestyle lifestyle = options.DefaultLifestyle;

            // Assert
            Assert.AreSame(lifestyle, Lifestyle.Transient);
        }

        [TestMethod]
        public void DefaultLifestyle_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = GetContainerOptions();

            // Act
            Action action = () => options.DefaultLifestyle = null;

            // Assert
            AssertThat.Throws<ArgumentNullException>(action);
        }

        [TestMethod]
        public void DefaultLifestyle_ChangedBeforeAnyRegistrations_ChangesThePropertyToTheSetInstance()
        {
            // Arrange
            var options = GetContainerOptions();

            // Act
            options.DefaultLifestyle = Lifestyle.Singleton;

            // Assert
            Assert.AreSame(Lifestyle.Singleton, options.DefaultLifestyle,
                "The set_DefaultLifestyle did not work.");
        }

        [TestMethod]
        public void DefaultLifestyle_ChangedMultipleTimesBeforeAnyRegistrations_ChangesThePropertyToTheSetInstance()
        {
            // Arrange
            var expectedLifestyle = new ThreadScopedLifestyle();

            var options = GetContainerOptions();

            // Act
            options.DefaultLifestyle = Lifestyle.Singleton;
            options.DefaultLifestyle = Lifestyle.Transient;

            // Assert
            Assert.AreSame(Lifestyle.Transient, options.DefaultLifestyle,
                "The set_DefaultLifestyle did not work.");
        }

        [TestMethod]
        public void DefaultLifestyle_ChangedAfterFirstRegistration_Fails()
        {
            // Arrange
            var container = new Container();

            container.RegisterSingleton<object>("The first registration.");

            // Act
            Action action = () => container.Options.DefaultLifestyle = Lifestyle.Singleton;

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "DefaultLifestyle property cannot be changed after the first registration",
                action);
        }
        
        [TestMethod]
        public void DefaultLifestyle_WithCustomLifestyle_MakesARegistrationUsingThatLifestyle()
        {
            // Arrange
            var container = new Container();
            container.Options.DefaultLifestyle = Lifestyle.Singleton;

            container.Register<ConcreteCommand>();

            // Act
            var command1 = container.GetInstance<ConcreteCommand>();
            var command2 = container.GetInstance<ConcreteCommand>();

            // Assert
            Assert.AreSame(command1, command2, "Same instance was expected");
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
            var expectedLifestyle = new ThreadScopedLifestyle();

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
            var expectedLifestyle = new ThreadScopedLifestyle();

            var options = GetContainerOptions();

            // Act
            options.DefaultScopedLifestyle = new ThreadScopedLifestyle();
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

            container.RegisterSingleton<object>("The first registration.");

            // Act
            Action action = () => container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

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

            options.DependencyInjectionBehavior = new AlternativeDependencyInjectionBehavior();

            // Act
            var description = options.DebuggerDisplayDescription;

            // Assert
            Assert.AreEqual("Custom Dependency Injection", description);
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
            options.DependencyInjectionBehavior = new AlternativeDependencyInjectionBehavior();
            options.PropertySelectionBehavior = new AlternativePropertySelectionBehavior();
            options.LifestyleSelectionBehavior = new AlternativeLifestyleSelectionBehavior();

            // Act
            var description = options.DebuggerDisplayDescription;

            // Assert
            Assert.AreEqual(@"
                Allows Overriding Registrations,
                Custom Constructor Resolution,
                Custom Dependency Injection,
                Custom Property Selection,
                Custom Lifestyle Selection
                ".TrimInside(), 
                description);
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
        public void PropertySelectionBehaviorSelectProperty_WithDefaultConfiguration_ReturnsFalse()
        {
            // Arrange
            PropertyInfo property = GetProperty<ClassWithLoggerProperty>(c => c.Logger);

            var options = GetContainerOptions();

            // Act
            var result = options.PropertySelectionBehavior.SelectProperty(property.DeclaringType, property);

            // Assert
            Assert.IsFalse(result);
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

        [TestMethod]
        public void LifestyleSelectionBehavior_DefaultImplementation_RedirectsToDefaultLifestyleProperty1()
        {
            // Arrange
            var options = GetContainerOptions();

            // Act
            var lifestyle = options.LifestyleSelectionBehavior.SelectLifestyle(typeof(object));

            // Assert
            Assert.AreSame(options.DefaultLifestyle, lifestyle);
        }

        [TestMethod]
        public void LifestyleSelectionBehavior_DefaultImplementation_RedirectsToDefaultLifestyleProperty2()
        {
            // Arrange
            var options = GetContainerOptions();
            options.DefaultLifestyle = Lifestyle.Singleton;

            // Act
            var lifestyle = options.LifestyleSelectionBehavior.SelectLifestyle(typeof(object));

            // Assert
            Assert.AreSame(Lifestyle.Singleton, lifestyle);
        }

        private static PropertyInfo GetProperty<T>(Expression<Func<T, object>> propertySelector)
        {
            var body = (MemberExpression)propertySelector.Body;

            return (PropertyInfo)body.Member;
        }

        private static ContainerOptions GetContainerOptions() => new Container().Options;

        public sealed class ClassWithContainerAsDependency
        {
            public ClassWithContainerAsDependency(Container container)
            {
                this.Container = container;
            }

            public Container Container { get; }
        }

        public class ClassWithLoggerProperty
        {
            public ILogger Logger { get; set; }
        }

        private sealed class AlternativeConstructorResolutionBehavior : IConstructorResolutionBehavior
        {
            public ConstructorInfo GetConstructor(Type impl) => impl.GetConstructors()[0];
        }

        private sealed class AlternativeDependencyInjectionBehavior : IDependencyInjectionBehavior
        {
            public InstanceProducer GetInstanceProducer(InjectionConsumerInfo consumer, bool throwOnFailure)
            {
                throw new NotImplementedException();
            }

            public void Verify(InjectionConsumerInfo consumer)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class AlternativePropertySelectionBehavior : IPropertySelectionBehavior
        {
            public bool SelectProperty(Type implementationType, PropertyInfo property)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class AlternativeLifestyleSelectionBehavior : ILifestyleSelectionBehavior
        {
            public Lifestyle SelectLifestyle(Type implementationType)
            {
                throw new NotImplementedException();
            }
        }
    }
}