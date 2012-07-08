namespace SimpleInjector.Tests.Unit
{
    using System;
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
                AssertThat.ExceptionMessageContains("ContainerOptions", ex);
                AssertThat.ExceptionMessageContains("AllowOverridingRegistrations", ex);
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
    }
}