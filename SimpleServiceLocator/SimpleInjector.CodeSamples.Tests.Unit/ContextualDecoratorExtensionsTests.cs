namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Extensions;

    [TestClass]
    public class ContextualDecoratorExtensionsTests
    {
        [TestMethod]
        public void GetInstance_ResolvingConditionallyDecoratedInstanceWithPredicateTrue_AppliesTheDecorator()
        {
            // Arrange
            var container = new Container();

            container.Options.EnableContextualDecoratorSupport();

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            container.RegisterContextualDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>),
                parameter => true);

            // Act
            var instance = container.GetInstance<Consumer<ICommandHandler<RealCommand>>>();

            // Assert
            Assert.IsInstanceOfType(instance.Dependency, typeof(CommandHandlerDecorator<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_ResolvingConditionallyDecoratedInstanceWithPredicateFalse_DoesNotApplyTheDecorator()
        {
            // Arrange
            var container = new Container();

            container.Options.EnableContextualDecoratorSupport();

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            container.RegisterContextualDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>),
                parameter => false);

            // Act
            var instance = container.GetInstance<Consumer<ICommandHandler<RealCommand>>>();

            // Assert
            Assert.IsInstanceOfType(instance.Dependency, typeof(NullCommandHandler<RealCommand>));
        }
        
        [TestMethod]
        public void GetInstance_ResolvingConditionallyDecoratedInstanceWithConditionalPredicate_AppliesDecoratorsBasedOnPredicate()
        {
            // Arrange
            var container = new Container();

            container.Options.EnableContextualDecoratorSupport();

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            container.RegisterContextualDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>),
                parameter => parameter.Name.StartsWith("cached"));

            // Act
            // Consumer has a constructor argument named 'dependency'
            var consumer = container.GetInstance<Consumer<ICommandHandler<RealCommand>>>();

            // CachedConsumer has a constructor argument named 'cachedDependency'
            var cachedConsumer = container.GetInstance<CachedConsumer<ICommandHandler<RealCommand>>>();

            // Assert
            Assert.IsInstanceOfType(consumer.Dependency, typeof(NullCommandHandler<RealCommand>));
            Assert.IsInstanceOfType(cachedConsumer.Dependency, typeof(CommandHandlerDecorator<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_ResolvingInstanceWithTwoConditionallyDecoratorsRegistered_AppliesBothDecorators()
        {
            // Arrange
            var container = new Container();

            container.Options.EnableContextualDecoratorSupport();

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            container.RegisterContextualDecorator(
                typeof(ICommandHandler<>), 
                typeof(CommandHandlerDecorator<>),
                parameter => true);

            container.RegisterContextualDecorator(
                typeof(ICommandHandler<>), 
                typeof(AnotherCommandHandlerDecorator<>),
                parameter => true);            

            // Act
            var consumer = container.GetInstance<Consumer<ICommandHandler<RealCommand>>>();

            // Assert
            Assert.IsInstanceOfType(consumer.Dependency, typeof(AnotherCommandHandlerDecorator<RealCommand>));

            var decorator = consumer.Dependency as AnotherCommandHandlerDecorator<RealCommand>;

            Assert.IsInstanceOfType(decorator.Decoratee, typeof(CommandHandlerDecorator<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_ResolvingConditionallyDecoratedInstanceWithTwoDecoratorsAndConditionalPredicate_AppliesDecoratorsBasedOnPredicate()
        {
            // Arrange
            var container = new Container();

            container.Options.EnableContextualDecoratorSupport();

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            container.RegisterContextualDecorator(
                typeof(ICommandHandler<>),
                typeof(CommandHandlerDecorator<>),
                parameter => parameter.Name.StartsWith("cached"));

            container.RegisterContextualDecorator(
                typeof(ICommandHandler<>),
                typeof(AnotherCommandHandlerDecorator<>),
                parameter => parameter.Name.StartsWith("cached"));

            // Act
            var consumer = container.GetInstance<Consumer<ICommandHandler<RealCommand>>>();
            var cachedConsumer = container.GetInstance<CachedConsumer<ICommandHandler<RealCommand>>>();

            // Assert
            Assert.IsInstanceOfType(consumer.Dependency, typeof(NullCommandHandler<RealCommand>));

            Assert.IsInstanceOfType(cachedConsumer.Dependency, typeof(AnotherCommandHandlerDecorator<RealCommand>));

            var decorator = cachedConsumer.Dependency as AnotherCommandHandlerDecorator<RealCommand>;

            Assert.IsInstanceOfType(decorator.Decoratee, typeof(CommandHandlerDecorator<RealCommand>));
        }

        [TestMethod]
        public void GetInstance_ResolvingConditionallyDecoratedInstanceWrappedWithOtherNonTransientDecorator_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Options.EnableContextualDecoratorSupport();

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            container.RegisterContextualDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>),
                parameter => false);

            // Wrap the conditional decorator in a non-transient decorator.
            container.RegisterSingleDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>));

            try
            {
                // Act
                var instance = container.GetInstance<Consumer<ICommandHandler<RealCommand>>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (Exception ex)
            {
                AssertThat.StringContains(
                    "Make sure that all registered decorators that wrap this decorator are transient",
                    ex.Message);
            }
        }
        
        [TestMethod]
        public void GetInstance_ResolvingInstanceWithTwoConditionalDecoratorsWhereInnerDecoratorIsWrappedWithNonTransientDecorator_ThrowsExpectedException()
        {
            // Arrange
            var container = new Container();

            container.Options.EnableContextualDecoratorSupport();

            container.Register<ICommandHandler<RealCommand>, NullCommandHandler<RealCommand>>();

            container.RegisterContextualDecorator(
                typeof(ICommandHandler<>),
                typeof(CommandHandlerDecorator<>),
                parameter => parameter.Name.StartsWith("cached"));

            // Wrap the conditional decorator in a non-transient decorator.
            container.RegisterSingleDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerProxy<>));

            // And wrap with a second contextual decorator.
            container.RegisterContextualDecorator(
                typeof(ICommandHandler<>),
                typeof(AnotherCommandHandlerDecorator<>),
                parameter => parameter.Name.StartsWith("cached"));

            try
            {
                // Act
                var consumer = container.GetInstance<Consumer<ICommandHandler<RealCommand>>>();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (Exception ex)
            {
                AssertThat.StringContains(
                    "Couldn't apply the contextual decorator " + typeof(CommandHandlerDecorator<>).FullName,
                    ex.Message);

                AssertThat.StringContains(
                    "Make sure that all registered decorators that wrap this decorator are transient and " +
                    "don't depend on Func<",
                    ex.Message);
            }
        }

        [TestMethod]
        public void RegisterContextualDecorator_CalledBeforeEnableContextualDecoratorSupport_ThrowsException()
        {
            // Arrange
            var container = new Container();

            try
            {
                // Act
                container.RegisterContextualDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>),
                    parameter => true);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("EnableContextualDecoratorSupport"));
            }
        }

        [TestMethod]
        public void EnableContextualDecoratorSupport_RegisteredOnce_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            container.Options.EnableContextualDecoratorSupport();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EnableContextualDecoratorSupport_RegisteredTwice_Fails()
        {
            // Arrange
            var container = new Container();

            container.Options.EnableContextualDecoratorSupport();

            // Act
            container.Options.EnableContextualDecoratorSupport();
        }

        public sealed class Consumer<TDependency>
        {
            public readonly TDependency Dependency;

            public Consumer(TDependency dependency)
            {
                this.Dependency = dependency;
            }
        }

        public sealed class CachedConsumer<TDependency>
        {
            public readonly TDependency Dependency;

            public CachedConsumer(TDependency cachedDependency)
            {
                this.Dependency = cachedDependency;
            }
        }
    }
}