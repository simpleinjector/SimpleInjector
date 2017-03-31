namespace SimpleInjector.Tests.Unit.Advanced
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

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
            AssertThat.ThrowsWithParamName<ArgumentNullException>("consumer", action);
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
                The constructor of type {0}.{1} contains parameter 'intArgument' of type Int32 which can not 
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

        [TestMethod]
        public void Verify_StringTypeParameter_ThrowsExpectedException()
        {
            // Arrange
            string expectedString = string.Format(@"
                The constructor of type {0}.{1} contains parameter 'stringArgument' of type String which can 
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

        private class TypeWithSinglePublicConstructorWithValueTypeParameter
        {
            public TypeWithSinglePublicConstructorWithValueTypeParameter(int intArgument)
            {
            }
        }

        private class TypeWithSinglePublicConstructorWithStringTypeParameter
        {
            public TypeWithSinglePublicConstructorWithStringTypeParameter(string stringArgument)
            {
            }
        }

        private sealed class FakeDependencyInjectionBehavior : IDependencyInjectionBehavior
        {
            public InstanceProducer ProducerToReturn { get; set; }

            public InstanceProducer GetInstanceProducer(InjectionConsumerInfo c, bool f) => this.ProducerToReturn;

            public void Verify(InjectionConsumerInfo consumer)
            {
            }
        }
    }
}