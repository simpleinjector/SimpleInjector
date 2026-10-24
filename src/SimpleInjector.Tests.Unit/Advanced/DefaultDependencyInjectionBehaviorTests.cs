namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector;
    using SimpleInjector.Advanced;
    using SimpleInjector.Diagnostics.Tests.Unit;

    [TestClass]
    public class DefaultDependencyInjectionBehaviorTests
    {
        [TestMethod]
        public void BuildExpression_WithNullParameterArgument_ThrowsExpectedException()
        {
            // Arrange
            var behavior = GetContainerOptions().DependencyInjectionBehavior;

            // Act
            Action action = () => behavior.GetInstanceProducer(null, false);

            // Assert
            AssertThat.ThrowsWithParamName<ArgumentNullException>("dependency", action);
        }

        [TestMethod]
        public void DependencyInjectionBehavior_CustomBehaviorThatReturnsNull_ThrowsExpressiveException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.DependencyInjectionBehavior = new FakeDependencyInjectionBehavior
            {
                ProducerToReturn = null
            };

            container.Register<IUserRepository, SqlUserRepository>();

            // Act
            // RealUserService depends on IUserRepository
            Action action = () => container.GetInstance<RealUserService>();

            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "FakeDependencyInjectionBehavior that was registered through " +
                "the Container.Options.DependencyInjectionBehavior property, returned a null reference",
                action);
        }

        [TestMethod]
        public void Verify_TValueTypeParameter_ThrowsExpectedException()
        {
            // Arrange
            string expectedString = string.Format(@"
                The constructor of type {0}.{1} contains parameter 'intArgument' of type int, which can not
                be used for constructor injection because it is a value type.",
                this.GetType().Name,
                typeof(TypeWithSinglePublicConstructorWithValueTypeParameter).Name)
                .TrimInside();

            var behavior = new Container().Options.DependencyInjectionBehavior;

            var constructor =
                typeof(TypeWithSinglePublicConstructorWithValueTypeParameter).GetConstructors().Single();

            var consumer = new InjectionConsumerInfo(constructor.GetParameters().Single());

            try
            {
                // Act
                behavior.Verify(consumer);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(expectedString, ex.Message);
            }
        }

        // #1032
        [TestMethod]
        public void Register_DependencyRejectedByBehaviorWithoutErrorMessage_ThrowsArgumentException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.DependencyInjectionBehavior =
                new RejectFooWithoutMessageBehavior(container.Options.DependencyInjectionBehavior);

            container.Register<IFoo, FooBar>();

            // Act
            Action action = () => container.Register<ServiceDependingOn<IFoo>>();

            // Assert
            AssertThat.Throws<ArgumentException>(action,
                "The behavior rejected the IFoo dependency, so registration should fail, just like it does " +
                "when the behavior supplies an error message.");
        }

        [TestMethod]
        public void Verify_StringTypeParameter_ThrowsExpectedException()
        {
            // Arrange
            string expectedString = string.Format(@"
                The constructor of type {0}.{1} contains parameter 'stringArgument' of type string, which can
                not be used for constructor injection.",
                this.GetType().Name,
                typeof(TypeWithSinglePublicConstructorWithStringTypeParameter).Name)
                .TrimInside();

            var behavior = new Container().Options.DependencyInjectionBehavior;

            var constructor =
                typeof(TypeWithSinglePublicConstructorWithStringTypeParameter).GetConstructors().Single();

            var consumer = new InjectionConsumerInfo(constructor.GetParameters().Single());

            try
            {
                // Act
                behavior.Verify(consumer);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains(expectedString, ex.Message);
            }
        }

        private static ContainerOptions GetContainerOptions() => new Container().Options;

        private class TypeWithSinglePublicConstructorWithValueTypeParameter(int intArgument);

        private class TypeWithSinglePublicConstructorWithStringTypeParameter(string stringArgument);

        private sealed class FakeDependencyInjectionBehavior : IDependencyInjectionBehavior
        {
            public InstanceProducer ProducerToReturn { get; set; }

            public InstanceProducer GetInstanceProducer(InjectionConsumerInfo d, bool t) =>
                this.ProducerToReturn;

            public bool VerifyDependency(InjectionConsumerInfo dependency, out string errorMessage)
            {
                errorMessage = null;
                return true;
            }
        }

        private sealed class RejectFooWithoutMessageBehavior(IDependencyInjectionBehavior original)
            : IDependencyInjectionBehavior
        {
            public bool VerifyDependency(InjectionConsumerInfo dependency, out string errorMessage)
            {
                errorMessage = null;
                return dependency.Target.TargetType != typeof(IFoo);
            }

            public InstanceProducer GetInstanceProducer(InjectionConsumerInfo dependency, bool throwOnFailure) =>
                original.GetInstanceProducer(dependency, throwOnFailure);
        }
    }
}