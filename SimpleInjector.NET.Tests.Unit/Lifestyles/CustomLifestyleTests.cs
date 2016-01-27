namespace SimpleInjector.Tests.Unit.Lifestyles
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CustomLifestyleTests
    {
        private static readonly Lifestyle CustomLifestyle = 
            Lifestyle.CreateCustom("Custom", creator => () => creator());

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
        
        public class ConcreteCommandHandler : ICommandHandler<ConcreteCommand>
        {
            public void Handle(ConcreteCommand command)
            {
            }
        }
    }
}