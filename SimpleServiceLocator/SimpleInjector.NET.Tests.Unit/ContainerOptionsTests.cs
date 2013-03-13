namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    [TestClass]
    public class ContainerOptionsTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContainerConstructor_SuppliedWithNullContainerOptionsArgument_ThrowsException()
        {
            // Arrange
            ContainerOptions invalidOptions = null;

            // Act
            new Container(invalidOptions);
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void AllowOverridingRegistrations_SetToFalse_ContainerDoesNotAllowOverringRegistrations()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

            try
            {
                container.Register<IUserRepository, SqlUserRepository>();
            }
            catch
            {
                Assert.Fail("Test setup fail. This call is expected to succeed.");
            }

            // Act
            container.Register<IUserRepository, InMemoryUserRepository>();
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
            Assert.IsInstanceOfType(container.GetInstance<IUserRepository>(), typeof(InMemoryUserRepository),
                "The registration was not overridden properly.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AllowOverridingRegistrations_SetToFalse_ContainerDoesNotAllowOverringCollections()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

            try
            {
                container.RegisterAll<IUserRepository>(new SqlUserRepository());
            }
            catch
            {
                Assert.Fail("Test setup fail. This call was not expected to fail.");
            }

            // Act
            container.RegisterAll<IUserRepository>(new InMemoryUserRepository());
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
            Assert.IsInstanceOfType(instance, typeof(InMemoryUserRepository));
        }

        // NOTE: There was a bug in the framework. The container did not selfregister when the overloaded
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorResolutionBehavior_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = new ContainerOptions();

            // Act
            options.ConstructorResolutionBehavior = null;
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorInjectionBehavior_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = new ContainerOptions();

            // Act
            options.ConstructorInjectionBehavior = null;
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void PropertyInjectionBehavior_SetWithNullValue_ThrowsException()
        {
            // Arrange
            var options = new ContainerOptions();

            // Act
            options.PropertySelectionBehavior = null;
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
        public void DebuggerDisplayDescription_WithAllCustomValues_ReturnsExpectedMessage()
        {
            // Arrange
            var options = new ContainerOptions();

            options.AllowOverridingRegistrations = true;
            options.ConstructorResolutionBehavior = new AlternativeConstructorResolutionBehavior();
            options.ConstructorVerificationBehavior = new AlternativeConstructorVerificationBehavior();
            options.ConstructorInjectionBehavior = new AlternativeConstructorInjectionBehavior();
            options.PropertySelectionBehavior = new AlternativePropertySelectionBehavior();

            // Act
            var description = options.DebuggerDisplayDescription;

            // Assert
            Assert.AreEqual(@"
                Allows Overriding Registrations,
                Custom Constructor Resolution,
                Custom Constructor Verification,
                Custom Constructor Injection,
                Custom Property Selection
                ".TrimInside(), description);
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
    }
}