namespace SimpleInjector.Tests.Unit.Lifestyles
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SimpleInjector.Diagnostics.Tests.Unit;

    [TestClass]
    public class CustomLifestyleTests
    {
        private static readonly Lifestyle CustomLifestyle =
            Lifestyle.CreateCustom("Custom", creator => () => creator());

        [TestMethod]
        public void GetInstance_ConsumerOfCustomLifestyleThatReturnsNull_ThrowsActivationException()
        {
            // Arrange
            var lifestyle = Lifestyle.CreateCustom("Cache", instanceCreator => () => null);

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>(lifestyle);
            container.Register<RealUserService>();

            // Act
            Action action = () => container.GetInstance<RealUserService>();

            // Assert
            AssertThat.Throws<ActivationException>(action,
                "Null should never be injected into a consumer.");
        }

        [TestMethod]
        public void Verify_ConsumerOfCustomLifestyleThatReturnsNull_ThrowsException()
        {
            // Arrange
            var lifestyle = Lifestyle.CreateCustom("Cache", instanceCreator => () => null);

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>(lifestyle);
            container.Register<RealUserService>();

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.Throws<InvalidOperationException>(action,
                "Verify should detect that null is injected into FooConsumer.");
        }

        // When the custom lifestyle's applier factory itself returns null, the user currently gets "Expression
        // of type 'System.Object' cannot be invoked", which doesn't mention the lifestyle at all.
        [TestMethod]
        public void GetInstance_CustomLifestyleWhoseApplierFactoryReturnsNull_ThrowsExceptionThatMentionsTheLifestyle()
        {
            // Arrange
            var lifestyle = Lifestyle.CreateCustom("Cache", instanceCreator => null);

            var container = ContainerFactory.New();

            container.Register<IUserRepository, SqlUserRepository>(lifestyle);

            // Act
            Action action = () => container.GetInstance<IUserRepository>();

            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "The custom lifestyle 'Cache' returned null from its supplied CreateLifestyleApplier",
                action);
        }

        [TestMethod]
        public void GetInstance_OnRegistrationWithCustomLifestyle_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository, InMemoryUserRepository>(CustomLifestyle);

            // Act
            container.GetInstance<IUserRepository>();
        }

        [TestMethod]
        public void GetInstance_OnRegistrationWithDelegatedCustomLifestyle_Succeeds()
        {
            // Arrange
            var container = new Container();

            container.Register<IUserRepository>(() => new InMemoryUserRepository(), CustomLifestyle);

            // Act
            container.GetInstance<IUserRepository>();
        }

        // This is a regression test for bug 21012.
        [TestMethod]
        public void GetInstance_CustomLifestyleRegistrationWrappedWithProxyDecorator_DecoratorContextGetsSuppliedWithCorrectImplementationType()
        {
            // Arrange
            Type expectedImplementationType = typeof(ConcreteCommandHandler);
            Type actualImplementationType = null;

            var container = new Container();

            container.Register<ICommandHandler<ConcreteCommand>, ConcreteCommandHandler>(CustomLifestyle);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>),
                Lifestyle.Singleton, c =>
            {
                actualImplementationType = c.ImplementationType;
                return true;
            });

            // Act
            container.GetInstance<ICommandHandler<ConcreteCommand>>();

            // Assert
            AssertThat.AreEqual(expectedImplementationType, actualImplementationType);
        }

        [TestMethod]
        public void GetInstance_CustomLifestyleRegistrationWrappedWithTwoDecorators_CorrectImplementationTypeGetsSuppliedToTheSecondDecorator()
        {
            // Arrange
            Type expectedImplementationType = typeof(ConcreteCommandHandler);
            Type actualImplementationType = null;

            var container = new Container();

            container.Register<ICommandHandler<ConcreteCommand>, ConcreteCommandHandler>(CustomLifestyle);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>), Lifestyle.Singleton);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AsyncCommandHandlerProxy<>), Lifestyle.Singleton, c =>
            {
                actualImplementationType = c.ImplementationType;
                return true;
            });

            // Act
            container.GetInstance<ICommandHandler<ConcreteCommand>>();

            // Assert
            AssertThat.AreEqual(expectedImplementationType, actualImplementationType);
        }

        [TestMethod]
        public void Register_TwoRegistrationsForSameCustomLifestyleAndSameImplementation_DeduplicatesRegistration()
        {
            // Arrange
            var lifestyle = Lifestyle.CreateCustom("Custom1", creator => creator);

            var container = new Container();

            container.Register<IFoo, FooBar>(lifestyle);
            container.Register<IBar, FooBar>(lifestyle);

            // Act
            var producer1 = container.GetRegistration(typeof(IFoo));
            var producer2 = container.GetRegistration(typeof(IBar));

            // Assert
            Assert.AreSame(producer1.Registration, producer2.Registration,
                "Registrations for the same custom lifestyle are expected to be de-duplicated/cached");
        }

        [TestMethod]
        public void Register_TwoRegistrationsForDifferentCustomLifestyleButAndSameImplementation_DoesNotDeduplicateRegistrations()
        {
            // Arrange
            var lifestyle1 = Lifestyle.CreateCustom("Custom1", creator => creator);
            var lifestyle2 = Lifestyle.CreateCustom("Custom2", creator => creator);

            var container = new Container();

            container.Register<IFoo, FooBar>(lifestyle1);
            container.Register<IBar, FooBar>(lifestyle2);

            // Act
            var producer1 = container.GetRegistration(typeof(IFoo));
            var producer2 = container.GetRegistration(typeof(IBar));

            // Assert
            Assert.AreNotSame(producer1.Registration, producer2.Registration,
                "Each registration should get its own registration, because we are effectively dealing with " +
                "completely unrelated lifestyles. There are both 'custom' lifestyles, but might do caching " +
                "completely differently.");
        }

        public class ConcreteCommandHandler : ICommandHandler<ConcreteCommand>
        {
            public void Handle(ConcreteCommand command)
            {
            }
        }
    }
}