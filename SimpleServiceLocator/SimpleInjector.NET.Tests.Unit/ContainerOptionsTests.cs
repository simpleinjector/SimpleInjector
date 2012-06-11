namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq;
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
        public void AllowOverridingRegistrations_False_ContainerDoesNotAllowOverringRegistrations()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

            container.Register<IUserRepository, SqlUserRepository>();

            // Act
            container.Register<IUserRepository, InMemoryUserRepository>();
        }

        [TestMethod]
        public void AllowOverridingRegistrations_False_ContainerThrowsExpectedExceptionMessage()
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
                Assert.IsTrue(ex.Message.Contains("ContainerOptions"), "Actual: " + ex);
                Assert.IsTrue(ex.Message.Contains("AllowOverridingRegistrations"), "Actual: " + ex);
            }
        }

        [TestMethod]
        public void AllowOverridingRegistrations_True_ContainerDoesNotAllowOverringRegistrations()
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
            Assert.IsInstanceOfType(container.GetInstance<IUserRepository>(), typeof(InMemoryUserRepository));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AllowOverridingRegistrations_False_ContainerDoesNotAllowOverringCollections()
        {
            // Arrange
            var container = new Container(new ContainerOptions
            {
                AllowOverridingRegistrations = false
            });

            container.RegisterAll<IUserRepository>(new SqlUserRepository());

            // Act
            container.RegisterAll<IUserRepository>(new InMemoryUserRepository());
        }

        [TestMethod]
        public void AllowOverridingRegistrations_True_ContainerDoesNotAllowOverringCollections()
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
                AssertThat.StringContains(
                    "supplied ContainerOptions instance belongs to another Container instance.", ex.Message);
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
            Assert.IsTrue(object.ReferenceEquals(expectedBehavior, options.ConstructorResolutionBehavior));
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
                AssertThat.StringContains(
                    "ConstructorResolutionBehavior cannot be changed after the first registration",
                    ex.Message);
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
                    "ConstructorResolutionBehavior cannot be changed after the first registration",
                    ex.Message);
            }
        }

        [TestMethod]
        public void ConstructorResolutionBehavior_SetWithAValueThatBelongsToADifferentContainer1_Fails()
        {
            // Arrange
            var options1 = new ContainerOptions();
            var container1 = new Container(options1);

            var options2 = new ContainerOptions();

            // Register this options2 to a new container.
            var container2 = new Container(options2);

            try
            {
                // Act
                options2.ConstructorResolutionBehavior = options1.ConstructorResolutionBehavior;

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(
                    "The supplied ConstructorResolutionBehavior instance belongs to another Container",
                    ex.Message);

                AssertThat.ExceptionContainsParamName(ex, "value");
            }
        }

        [TestMethod]
        public void ConstructorResolutionBehavior_SetWithAValueThatBelongsToADifferentContainer2_Fails()
        {
            // Arrange
            var options1 = new ContainerOptions();
            var container1 = new Container(options1);

            // In this test, we don't register the options2 to a container, but we'd expect the assignment of
            // ConstructorResolutionBehavior still to fail.
            var options2 = new ContainerOptions();

            try
            {
                // Act
                options2.ConstructorResolutionBehavior = options1.ConstructorResolutionBehavior;

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains(
                    "The supplied ConstructorResolutionBehavior instance belongs to another Container",
                    ex.Message);

                AssertThat.ExceptionContainsParamName(ex, "value");
            }
        }

        [TestMethod]
        public void ConstructorResolutionBehavior_SetWithValueFromSameContainer_Succeeds()
        {
            // Arrange
            var options = new ContainerOptions();
            var container = new Container(options);
            var behavior = options.ConstructorResolutionBehavior;

            options.ConstructorResolutionBehavior = new AlternativeConstructorResolutionBehavior();

            // Act
            options.ConstructorResolutionBehavior = behavior;
        }

        public sealed class ClassWithContainerAsDependency
        {
            public ClassWithContainerAsDependency(Container container)
            {
                this.Container = container;
            }

            public Container Container { get; private set; }
        }

        private sealed class AlternativeConstructorResolutionBehavior : ConstructorResolutionBehavior
        {
            public override System.Reflection.ConstructorInfo GetConstructor(Type type)
            {
                return type.GetConstructors()[0];
            }
        }
    }
}