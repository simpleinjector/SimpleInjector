namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class MostResolvableParametersConstructorResolutionBehaviorTests
    {
        [TestMethod]
        public void Register_TypeWithMultipleConstructors_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            // Act
            container.Register<IDisposable, MultipleConstructorsType>();
        }

        [TestMethod]
        public void RegisterConcrete_TypeWithMultipleConstructors_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            // Act
            container.Register<MultipleConstructorsType>();
        }

        [TestMethod]
        public void GetInstance_ResolvingAConcreteUnregisteredTypeWithMultipleConstructors_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            container.Register<ILogger, NullLogger>();
            container.Register<ICommand, ConcreteCommand>();

            // Act
            container.GetInstance<MultipleConstructorsType>();
        }

        [TestMethod]
        public void GetInstance_ResolvingARegisteredTypeWithMultipleConstructors_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            container.Register<ILogger, NullLogger>();
            container.Register<ICommand, ConcreteCommand>();

            container.Register<MultipleConstructorsType>();

            // Act
            container.GetInstance<MultipleConstructorsType>();
        }

        [TestMethod]
        public void GetInstance_ResolvingARegisteredTypeWithMultipleConstructors_InjectsDependenciesUsingMostParameterConstructor()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            container.Register<ILogger, NullLogger>();
            container.Register<ICommand, ConcreteCommand>();

            // Act
            var instance = container.GetInstance<MultipleConstructorsType>();

            // Assert
            Assert.IsNotNull(instance.Logger, "Logger should have been injected.");
            Assert.IsNotNull(instance.Command, "Command should have been injected.");
        }

        [TestMethod]
        public void GetInstance_ResolvingARegisteredTypeWithMultipleConstructors_InjectsDependenciesUsingConstructorThatSatisfiesRegisteredDependencies1()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            container.Register<ILogger, NullLogger>();

            // Act
            var instance = container.GetInstance<MultipleConstructorsType>();

            // Assert
            Assert.IsNotNull(instance.Logger, "Logger should have been injected.");
            Assert.IsNull(instance.Command, "Command should not have been injected.");
        }

        [TestMethod]
        public void GetInstance_ResolvingARegisteredTypeWithMultipleConstructors_InjectsDependenciesUsingConstructorThatSatisfiesRegisteredDependencies2()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            container.Register<ICommand, ConcreteCommand>();

            // Act
            var instance = container.GetInstance<MultipleConstructorsType>();

            // Assert
            Assert.IsNull(instance.Logger, "Logger should not have been injected.");
            Assert.IsNotNull(instance.Command, "Command should have been injected.");
        }

        [TestMethod]
        public void RegisterOpenGeneric_GenericTypeWithMultipleConstructors_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            // Act
            container.Register(typeof(IValidator<>), typeof(MultipleCtorNullValidator<>));
        }

        [TestMethod]
        public void GetInstance_GenericTypeWithMultipleConstructorsRegisteredWithRegisterOpenGeneric_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            container.Register(typeof(IValidator<>), typeof(MultipleCtorNullValidator<>));

            container.Register<ICommand, ConcreteCommand>();

            // Act
            container.GetInstance<IValidator<int>>();
        }

        [TestMethod]
        public void GetInstance_ContainerWithOverriddenConstructorInjectionBehavior_ResolvesInstanceSuccessfully()
        {
            // Arrange
            string expectedConnectionString = "fooBar";

            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            // The ConnectionStringsConvention overrides the ConstructorInjectionBehavior and allows resolving
            // constructor arguments of type string that are postfixed with 'ConnectionString'.
            container.Options.RegisterParameterConvention(new ConnectionStringsConvention(name => expectedConnectionString));

            // Act
            // TypeWithConnectionStringConstructorArgument contains 1 ctor with a 'string cs1ConnectionString'
            // argument. 'cs1' is available as key in app.config/connectionStrings.
            // The call to GetInstance will fail when the MostResolvableParametersConstructorResolutionBehavior
            // does not correctly callback to the ConstructorInjectionBehavior to check whether the type
            // can be resolved.
            var instance = container.GetInstance<TypeWithConnectionStringConstructorArgument>();

            // Assert
            Assert.AreEqual(expectedConnectionString, instance.ConnectionString);
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithTypeContainingMultipleConstructors_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            // Act
            container.Register(typeof(IValidator<>), new[] { typeof(MultipleCtorIntValidator) });
        }

        [TestMethod]
        public void GetInstance_TypeWithMultipleConstructorsRegisteredWithRegisterManyForOpenGeneric_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            container.Register(typeof(IValidator<>), new[] { typeof(MultipleCtorIntValidator) });

            container.Register<ILogger, NullLogger>();
            container.Register<ICommand, ConcreteCommand>();

            // Act
            container.GetInstance<IValidator<int>>();
        }

        [TestMethod]
        public void GetInstance_DecoratorWithMultipleConstructors_InjectsTheRealValidatorIntoTheDecoratorsConstructorWithTheMostParameters()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            container.Register<IValidator<int>, NullValidator<int>>();

            container.RegisterDecorator(typeof(IValidator<>), typeof(MultipleCtorsValidatorDecorator<>));

            container.Register<ILogger, NullLogger>();

            // Act
            var validator = container.GetInstance<IValidator<int>>();

            // Assert
            AssertThat.IsInstanceOfType(typeof(MultipleCtorsValidatorDecorator<int>), validator);
            AssertThat.IsInstanceOfType(typeof(NullValidator<int>),
                ((MultipleCtorsValidatorDecorator<int>)validator).WrappedValidator);
        }

        [TestMethod]
        public void Register_ConcreteTypeWithNoPublicConstructors_ThrowsExpectedException()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            try
            {
                // Act
                container.Register<TypeWithNoPublicConstructors>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                AssertThat.StringContains("it should contain at least one public constructor.", ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithNoPublicConstructors_ThrowsExpectedMessage()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            try
            {
                // Act
                container.GetInstance<TypeWithNoPublicConstructors>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains("it should contain at least one public constructor.", ex.Message);
            }
        }

        [TestMethod]
        public void GetInstance_UnregisteredConcreteTypeWithNoSelectableConstructors_ThrowsExpectedMessage()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            try
            {
                // Act
                // The type contains multiple public ctors, but we didn't register any needed dependencies.
                container.GetInstance<MultipleConstructorsWithSameNumberOfParametersType>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ActivationException ex)
            {
                AssertThat.StringContains("it should contain a public constructor that only contains " +
                    "parameters that can be resolved.", ex.Message);
            }
        }

        [TestMethod]
        public void GetAllInstances_CollectionWithDecoratorWithSingleConstructor_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostResolvableParametersConstructorResolutionBehavior();

            container.Register<ICommand, ConcreteCommand>();

            container.RegisterCollection(typeof(IValidator<>), new[]
            {
                typeof(MultipleCtorNullValidator<>),
            });

            // We have a design flaw in the library where it is really hard to deal with decorators that
            // are applied to collections. The issue was easily fixed for decorators with a single ctor,
            // but unfortunately not for decorators with multiple ctors. This test is missing, because it
            // will fail :(.
            container.RegisterDecorator(typeof(IValidator<>), typeof(SingleCtorValidatorDecorator<>));

            // Act
            container.GetAllInstances<IValidator<object>>().ToArray().First();
        }

        private static Container CreateContainerWithMostResolvableParametersConstructorResolutionBehavior()
        {
            var container = new Container();

            container.Options.ConstructorResolutionBehavior =
                new MostResolvableParametersConstructorResolutionBehavior(container);

            return container;
        }

        public sealed class MultipleConstructorsType : IDisposable
        {
            public readonly ILogger Logger;
            public readonly ICommand Command;

            public MultipleConstructorsType(ILogger logger)
            {
                this.Logger = logger;
            }

            public MultipleConstructorsType(ICommand command)
            {
                this.Command = command;
            }

            public MultipleConstructorsType(ILogger logger, ICommand command)
            {
                this.Logger = logger;
                this.Command = command;
            }

            public void Dispose()
            {
            }
        }

        public sealed class MultipleConstructorsWithSameNumberOfParametersType
        {
            public MultipleConstructorsWithSameNumberOfParametersType(ICommand logger)
            {
            }

            public MultipleConstructorsWithSameNumberOfParametersType(ILogger logger)
            {
            }

            public MultipleConstructorsWithSameNumberOfParametersType(ILogger logger, ICommand command)
            {
            }

            public MultipleConstructorsWithSameNumberOfParametersType(ICommand command, ILogger logger)
            {
            }
        }

        public class TypeWithConnectionStringConstructorArgument
        {
            public readonly string ConnectionString;

            // "cs1" is a connection string in the app.config of this test project.
            public TypeWithConnectionStringConstructorArgument(string cs1ConnectionString)
            {
                this.ConnectionString = cs1ConnectionString;
            }
        }

        public class MultipleCtorNullValidator<T> : IValidator<T>
        {
            public MultipleCtorNullValidator(ICommand command)
            {
            }

            public MultipleCtorNullValidator(ILogger logger, ICommand command)
            {
            }

            public void Validate(T instance)
            {
            }
        }

        public class MultipleCtorIntValidator : IValidator<int>
        {
            public MultipleCtorIntValidator(ICommand command)
            {
            }

            public MultipleCtorIntValidator(ILogger logger, ICommand command)
            {
            }

            public void Validate(int instance)
            {
            }
        }

        public class SingleCtorValidatorDecorator<T> : IValidator<T>
        {
            public readonly IValidator<T> WrappedValidator;

            public SingleCtorValidatorDecorator(IValidator<T> validator)
            {
                this.WrappedValidator = validator;
            }

            public void Validate(T instance)
            {
            }
        }

        public class MultipleCtorsValidatorDecorator<T> : IValidator<T>
        {
            public readonly IValidator<T> WrappedValidator;

            public MultipleCtorsValidatorDecorator(ICommand command)
            {
            }

            public MultipleCtorsValidatorDecorator(IValidator<T> validator, ILogger logger)
            {
                this.WrappedValidator = validator;
            }

            public void Validate(T instance)
            {
            }
        }

        public class TypeWithNoPublicConstructors
        {
            internal TypeWithNoPublicConstructors(ICommand command)
            {
            }
        }
    }
}