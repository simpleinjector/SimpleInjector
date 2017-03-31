namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Tests.Unit;

    [TestClass]
    public class MostParametersConstructorResolutionBehaviorTests
    {
        [TestMethod]
        public void Register_TypeWithMultipleConstructors_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostParametersConstructorResolutionBehavior();

            // Act
            container.Register<IDisposable, MultipleConstructorsType>();
        }

        [TestMethod]
        public void RegisterConcrete_TypeWithMultipleConstructors_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostParametersConstructorResolutionBehavior();

            // Act
            container.Register<MultipleConstructorsType>();
        }

        [TestMethod]
        public void GetInstance_ResolvingAConcreteUnregisteredTypeWithMultipleConstructors_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostParametersConstructorResolutionBehavior();

            container.Register<ILogger, NullLogger>();
            container.Register<ICommand, ConcreteCommand>();

            // Act
            container.GetInstance<MultipleConstructorsType>();
        }

        [TestMethod]
        public void GetInstance_ResolvingARegisteredTypeWithMultipleConstructors_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostParametersConstructorResolutionBehavior();

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
            Container container = CreateContainerWithMostParametersConstructorResolutionBehavior();

            container.Register<ILogger, NullLogger>();
            container.Register<ICommand, ConcreteCommand>();

            // Act
            var instance = container.GetInstance<MultipleConstructorsType>();

            // Assert
            Assert.IsNotNull(instance.Logger, "Logger should have been injected.");
            Assert.IsNotNull(instance.Command, "Command should have been injected.");
        }

        [TestMethod]
        public void Register_ConcreteTypeWithMultipleConstructorsWithSameNumberOfParameters_FailWithClearMessage()
        {
            // Arrange
            Container container = CreateContainerWithMostParametersConstructorResolutionBehavior();

            try
            {
                // Act
                container.Register<MultipleConstructorsWithSameNumberOfParametersType>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (Exception ex)
            {
                AssertThat.StringContains(typeof(MultipleConstructorsWithSameNumberOfParametersType).Name,
                    ex.Message);
                AssertThat.StringContains("contains multiple public constructors that contain 2 parameters",
                    ex.Message);
            }
        }

        [TestMethod]
        public void RegisterOpenGeneric_GenericTypeWithMultipleConstructors_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostParametersConstructorResolutionBehavior();

            // Act
            container.Register(typeof(IValidator<>), typeof(MultipleCtorNullValidator<>));
        }

        [TestMethod]
        public void GetInstance_GenericTypeWithMultipleConstructorsRegisteredWithRegisterOpenGeneric_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostParametersConstructorResolutionBehavior();

            container.Register(typeof(IValidator<>), typeof(MultipleCtorNullValidator<>));

            container.Register<ILogger, NullLogger>();
            container.Register<ICommand, ConcreteCommand>();

            // Act
            container.GetInstance<IValidator<int>>();
        }

        [TestMethod]
        public void RegisterManyForOpenGeneric_WithTypeContainingMultipleConstructors_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostParametersConstructorResolutionBehavior();

            // Act
            container.Register(typeof(IValidator<>), new[] { typeof(MultipleCtorIntValidator) });
        }

        [TestMethod]
        public void GetInstance_TypeWithMultipleConstructorsRegisteredWithRegisterManyForOpenGeneric_Succeeds()
        {
            // Arrange
            Container container = CreateContainerWithMostParametersConstructorResolutionBehavior();

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
            Container container = CreateContainerWithMostParametersConstructorResolutionBehavior();

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
            Container container = CreateContainerWithMostParametersConstructorResolutionBehavior();

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
            Container container = CreateContainerWithMostParametersConstructorResolutionBehavior();

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

        private static Container CreateContainerWithMostParametersConstructorResolutionBehavior()
        {
            var container = new Container();

            container.Options.ConstructorResolutionBehavior = new MostParametersConstructorResolutionBehavior();

            return container;
        }

        public sealed class MultipleConstructorsType : IDisposable
        {
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

            public ILogger Logger { get; }

            public ICommand Command { get; }

            public void Dispose()
            {
            }
        }

        public sealed class MultipleConstructorsWithSameNumberOfParametersType
        {
            public MultipleConstructorsWithSameNumberOfParametersType()
            {
            }

            public MultipleConstructorsWithSameNumberOfParametersType(ILogger logger, ICommand command)
            {
            }

            public MultipleConstructorsWithSameNumberOfParametersType(ICommand command, ILogger logger)
            {
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

        public class MultipleCtorsValidatorDecorator<T> : IValidator<T>
        {
            public MultipleCtorsValidatorDecorator(ICommand command)
            {
            }

            public MultipleCtorsValidatorDecorator(IValidator<T> validator, ILogger logger)
            {
                this.WrappedValidator = validator;
            }

            public IValidator<T> WrappedValidator { get; }

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